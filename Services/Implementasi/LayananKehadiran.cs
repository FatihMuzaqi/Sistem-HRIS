using Microsoft.EntityFrameworkCore;
using SistemHRD.Data;
using SistemHRD.Models.Entitas;
using SistemHRD.Models.Enums;
using SistemHRD.Services.Interfaces;
using SistemHRD.ViewModels;

namespace SistemHRD.Services.Implementasi;

public class LayananKehadiran : ILayananKehadiran
{
    private readonly KonteksAplikasi _context;
    private readonly ILayananAudit _layananAudit;
    private readonly ILayananTenant _layananTenant;

    public LayananKehadiran(
        KonteksAplikasi context,
        ILayananAudit layananAudit,
        ILayananTenant layananTenant)
    {
        _context = context;
        _layananAudit = layananAudit;
        _layananTenant = layananTenant;
    }

    public async Task<KehadiranHariIniViewModel> DapatkanKehadiranHariIniAsync(Guid karyawanId, DateTime tanggal)
    {
        var karyawan = await _context.Karyawan
            .Include(k => k.Departemen)
            .Include(k => k.Jabatan)
            .FirstOrDefaultAsync(k => k.Id == karyawanId);

        var model = new KehadiranHariIniViewModel
        {
            KaryawanId = karyawanId,
            Tanggal = tanggal.Date,
            NomorIndukKaryawan = karyawan?.NomorIndukKaryawan ?? string.Empty,
            NamaKaryawan = karyawan?.NamaLengkap ?? string.Empty,
            NamaDepartemen = karyawan?.Departemen?.NamaDepartemen ?? "-",
            NamaJabatan = karyawan?.Jabatan?.NamaJabatan ?? "-"
        };

        // Cari Jadwal Shift hari ini
        var jadwal = await _context.JadwalKerja
            .Include(j => j.ShiftKerja)
            .FirstOrDefaultAsync(j => j.KaryawanId == karyawanId && j.Tanggal.Date == tanggal.Date);

        var shift = jadwal?.ShiftKerja ?? await _context.ShiftKerja.FirstOrDefaultAsync(s => s.StatusAktif);

        if (shift != null)
        {
            model.ShiftKerjaId = shift.Id;
            model.NamaShift = shift.NamaShift;
            model.JamMasukShift = shift.JamMasuk;
            model.JamPulangShift = shift.JamPulang;
            model.ToleransiMenit = shift.ToleransiKeterlambatanMenit;
        }

        // Cari riwayat kehadiran hari ini
        var kehadiran = await _context.Kehadiran
            .FirstOrDefaultAsync(k => k.KaryawanId == karyawanId && k.Tanggal.Date == tanggal.Date);

        if (kehadiran != null)
        {
            model.SudahCheckIn = kehadiran.WaktuMasuk != null;
            model.WaktuMasuk = kehadiran.WaktuMasuk;
            model.Status = kehadiran.Status;
            model.MenitKeterlambatan = kehadiran.MenitKeterlambatan;
            model.LokasiMasuk = kehadiran.LokasiMasuk;

            model.SudahCheckOut = kehadiran.WaktuPulang != null;
            model.WaktuPulang = kehadiran.WaktuPulang;
            model.MenitPulangCepat = kehadiran.MenitPulangCepat;
            model.TotalJamKerja = kehadiran.TotalJamKerja;
            model.LokasiPulang = kehadiran.LokasiPulang;
        }

        return model;
    }

    public async Task<(bool Sukses, string Pesan, Kehadiran? Data)> CheckInAsync(Guid karyawanId, CheckInRequestViewModel model, string? ipAddress)
    {
        var hariIni = DateTime.Today;
        var waktuSekarang = DateTime.Now;

        var ada = await _context.Kehadiran
            .AnyAsync(k => k.KaryawanId == karyawanId && k.Tanggal.Date == hariIni);

        if (ada)
        {
            return (false, "Anda sudah melakukan Check-In untuk hari ini.", null);
        }

        // Dapatkan shift kerja
        var jadwal = await _context.JadwalKerja
            .Include(j => j.ShiftKerja)
            .FirstOrDefaultAsync(j => j.KaryawanId == karyawanId && j.Tanggal.Date == hariIni);

        var shift = jadwal?.ShiftKerja ?? await _context.ShiftKerja.FirstOrDefaultAsync(s => s.StatusAktif);

        var jamMasukAcuan = shift?.JamMasuk ?? new TimeSpan(8, 0, 0);
        var toleransi = shift?.ToleransiKeterlambatanMenit ?? 15;
        var batasWaktu = jamMasukAcuan.Add(TimeSpan.FromMinutes(toleransi));

        var waktuMasukTime = waktuSekarang.TimeOfDay;
        var status = StatusKehadiran.Hadir;
        var menitTerlambat = 0;

        if (waktuMasukTime > batasWaktu)
        {
            status = StatusKehadiran.Terlambat;
            menitTerlambat = (int)Math.Max(0, (waktuMasukTime - jamMasukAcuan).TotalMinutes);
        }

        var kehadiran = new Kehadiran
        {
            Id = Guid.NewGuid(),
            KaryawanId = karyawanId,
            ShiftKerjaId = shift?.Id,
            Tanggal = hariIni,
            WaktuMasuk = waktuSekarang,
            Status = status,
            MenitKeterlambatan = menitTerlambat,
            CatatanMasuk = model.CatatanMasuk,
            LokasiMasuk = !string.IsNullOrWhiteSpace(model.LokasiMasuk) ? model.LokasiMasuk : (ipAddress ?? "Kantor"),
            FotoMasuk = model.FotoMasuk
        };

        await _context.Kehadiran.AddAsync(kehadiran);
        await _context.SaveChangesAsync();

        await _layananAudit.CatatAsync(
            "CHECK_IN",
            "Kehadiran",
            $"Karyawan {karyawanId} melakukan Check-In pada {waktuSekarang:dd/MM/yyyy HH:mm:ss} (Status: {status}, Keterlambatan: {menitTerlambat} menit).");

        return (true, $"Check-In berhasil dicatat ({status}).", kehadiran);
    }

    public async Task<(bool Sukses, string Pesan, Kehadiran? Data)> CheckOutAsync(Guid karyawanId, CheckOutRequestViewModel model, string? ipAddress)
    {
        var hariIni = DateTime.Today;
        var waktuSekarang = DateTime.Now;

        var kehadiran = await _context.Kehadiran
            .Include(k => k.ShiftKerja)
            .FirstOrDefaultAsync(k => k.KaryawanId == karyawanId && k.Tanggal.Date == hariIni);

        if (kehadiran == null)
        {
            return (false, "Anda belum melakukan Check-In hari ini.", null);
        }

        if (kehadiran.WaktuPulang != null)
        {
            return (false, "Anda sudah melakukan Check-Out sebelumnya untuk hari ini.", null);
        }

        kehadiran.WaktuPulang = waktuSekarang;
        kehadiran.CatatanPulang = model.CatatanPulang;
        kehadiran.LokasiPulang = !string.IsNullOrWhiteSpace(model.LokasiPulang) ? model.LokasiPulang : (ipAddress ?? "Kantor");

        // Hitung menit pulang cepat
        var jamPulangAcuan = kehadiran.ShiftKerja?.JamPulang ?? new TimeSpan(17, 0, 0);
        if (waktuSekarang.TimeOfDay < jamPulangAcuan)
        {
            kehadiran.MenitPulangCepat = (int)Math.Max(0, (jamPulangAcuan - waktuSekarang.TimeOfDay).TotalMinutes);
        }

        // Hitung total jam kerja
        if (kehadiran.WaktuMasuk.HasValue)
        {
            var selisih = waktuSekarang - kehadiran.WaktuMasuk.Value;
            kehadiran.TotalJamKerja = Math.Round((decimal)selisih.TotalHours, 2);
        }

        _context.Kehadiran.Update(kehadiran);
        await _context.SaveChangesAsync();

        await _layananAudit.CatatAsync(
            "CHECK_OUT",
            "Kehadiran",
            $"Karyawan {karyawanId} melakukan Check-Out pada {waktuSekarang:dd/MM/yyyy HH:mm:ss} (Total Jam Kerja: {kehadiran.TotalJamKerja} jam).");

        return (true, "Check-Out berhasil dicatat.", kehadiran);
    }

    public async Task<DaftarKehadiranViewModel> DapatkanDaftarKehadiranAsync(FilterKehadiranViewModel filter)
    {
        var query = _context.Kehadiran
            .Include(k => k.Karyawan!)
                .ThenInclude(ky => ky.Departemen)
            .Include(k => k.Karyawan!)
                .ThenInclude(ky => ky.Jabatan)
            .Include(k => k.ShiftKerja)
            .AsQueryable();

        if (filter.TanggalMulai.HasValue)
        {
            query = query.Where(k => k.Tanggal.Date >= filter.TanggalMulai.Value.Date);
        }

        if (filter.TanggalSelesai.HasValue)
        {
            query = query.Where(k => k.Tanggal.Date <= filter.TanggalSelesai.Value.Date);
        }

        if (filter.DepartemenId.HasValue)
        {
            query = query.Where(k => k.Karyawan!.DepartemenId == filter.DepartemenId.Value);
        }

        if (filter.KaryawanId.HasValue)
        {
            query = query.Where(k => k.KaryawanId == filter.KaryawanId.Value);
        }

        if (filter.Status.HasValue)
        {
            query = query.Where(k => k.Status == filter.Status.Value);
        }

        if (!string.IsNullOrWhiteSpace(filter.Cari))
        {
            var cari = filter.Cari.Trim().ToLower();
            query = query.Where(k =>
                k.Karyawan!.NamaLengkap.ToLower().Contains(cari) ||
                k.Karyawan!.NomorIndukKaryawan.ToLower().Contains(cari));
        }

        var totalItem = await query.CountAsync();

        // Statistik agregat
        var semuaStatus = await query.Select(k => k.Status).ToListAsync();
        var totalHadir = semuaStatus.Count(s => s == StatusKehadiran.Hadir);
        var totalTerlambat = semuaStatus.Count(s => s == StatusKehadiran.Terlambat);
        var totalIzin = semuaStatus.Count(s => s == StatusKehadiran.Izin);
        var totalSakit = semuaStatus.Count(s => s == StatusKehadiran.Sakit);
        var totalCuti = semuaStatus.Count(s => s == StatusKehadiran.Cuti);
        var totalAlpa = semuaStatus.Count(s => s == StatusKehadiran.Alpa);

        var data = await query
            .OrderByDescending(k => k.Tanggal)
            .ThenByDescending(k => k.WaktuMasuk)
            .Skip((filter.Halaman - 1) * filter.UkuranHalaman)
            .Take(filter.UkuranHalaman)
            .Select(k => new ItemKehadiranViewModel
            {
                Id = k.Id,
                KaryawanId = k.KaryawanId,
                NomorIndukKaryawan = k.Karyawan != null ? k.Karyawan.NomorIndukKaryawan : string.Empty,
                NamaKaryawan = k.Karyawan != null ? k.Karyawan.NamaLengkap : string.Empty,
                NamaDepartemen = k.Karyawan != null && k.Karyawan.Departemen != null ? k.Karyawan.Departemen.NamaDepartemen : "-",
                NamaJabatan = k.Karyawan != null && k.Karyawan.Jabatan != null ? k.Karyawan.Jabatan.NamaJabatan : "-",
                Tanggal = k.Tanggal,
                NamaShift = k.ShiftKerja != null ? k.ShiftKerja.NamaShift : "Default",
                WaktuMasuk = k.WaktuMasuk,
                WaktuPulang = k.WaktuPulang,
                Status = k.Status,
                MenitKeterlambatan = k.MenitKeterlambatan,
                MenitPulangCepat = k.MenitPulangCepat,
                TotalJamKerja = k.TotalJamKerja,
                CatatanMasuk = k.CatatanMasuk,
                CatatanPulang = k.CatatanPulang,
                LokasiMasuk = k.LokasiMasuk
            })
            .ToListAsync();

        return new DaftarKehadiranViewModel
        {
            Filter = filter,
            DaftarKehadiran = data,
            TotalItem = totalItem,
            TotalHadir = totalHadir,
            TotalTerlambat = totalTerlambat,
            TotalIzin = totalIzin,
            TotalSakit = totalSakit,
            TotalCuti = totalCuti,
            TotalAlpa = totalAlpa
        };
    }

    public async Task<RekapKehadiranViewModel> DapatkanRekapitulasiAsync(int bulan, int tahun, Guid? departemenId, Guid? karyawanId)
    {
        var tanggalAwal = new DateTime(tahun, bulan, 1);
        var tanggalAkhir = tanggalAwal.AddMonths(1).AddDays(-1);

        var queryKaryawan = _context.Karyawan
            .Include(k => k.Departemen)
            .Where(k => k.StatusKaryawan == StatusKaryawan.Tetap || k.StatusKaryawan == StatusKaryawan.Kontrak);

        if (departemenId.HasValue)
        {
            queryKaryawan = queryKaryawan.Where(k => k.DepartemenId == departemenId.Value);
        }

        if (karyawanId.HasValue)
        {
            queryKaryawan = queryKaryawan.Where(k => k.Id == karyawanId.Value);
        }

        var daftarKaryawan = await queryKaryawan
            .OrderBy(k => k.NamaLengkap)
            .ToListAsync();

        var queryKehadiran = await _context.Kehadiran
            .Where(k => k.Tanggal.Date >= tanggalAwal.Date && k.Tanggal.Date <= tanggalAkhir.Date)
            .ToListAsync();

        var queryLembur = await _context.PengajuanLembur
            .Where(l => l.TanggalLembur.Date >= tanggalAwal.Date && l.TanggalLembur.Date <= tanggalAkhir.Date && l.Status == StatusPengajuanLembur.Disetujui)
            .ToListAsync();

        var daftarRekap = new List<ItemRekapKehadiranViewModel>();

        foreach (var kry in daftarKaryawan)
        {
            var presensiKry = queryKehadiran.Where(k => k.KaryawanId == kry.Id).ToList();
            var lemburKry = queryLembur.Where(l => l.KaryawanId == kry.Id).ToList();

            daftarRekap.Add(new ItemRekapKehadiranViewModel
            {
                KaryawanId = kry.Id,
                NomorIndukKaryawan = kry.NomorIndukKaryawan,
                NamaKaryawan = kry.NamaLengkap,
                NamaDepartemen = kry.Departemen?.NamaDepartemen ?? "-",
                TotalHadir = presensiKry.Count(p => p.Status == StatusKehadiran.Hadir),
                TotalTerlambat = presensiKry.Count(p => p.Status == StatusKehadiran.Terlambat),
                TotalIzin = presensiKry.Count(p => p.Status == StatusKehadiran.Izin),
                TotalSakit = presensiKry.Count(p => p.Status == StatusKehadiran.Sakit),
                TotalCuti = presensiKry.Count(p => p.Status == StatusKehadiran.Cuti),
                TotalAlpa = presensiKry.Count(p => p.Status == StatusKehadiran.Alpa),
                TotalMenitKeterlambatan = presensiKry.Sum(p => p.MenitKeterlambatan),
                TotalJamKerja = presensiKry.Sum(p => p.TotalJamKerja),
                TotalJamLembur = lemburKry.Sum(l => l.TotalJam)
            });
        }

        return new RekapKehadiranViewModel
        {
            Bulan = bulan,
            Tahun = tahun,
            DepartemenId = departemenId,
            KaryawanId = karyawanId,
            DaftarRekap = daftarRekap
        };
    }

    public async Task<List<ShiftKerja>> DapatkanDaftarShiftAsync()
    {
        return await _context.ShiftKerja
            .OrderBy(s => s.NamaShift)
            .ToListAsync();
    }

    public async Task<ShiftKerja?> DapatkanShiftByIdAsync(Guid id)
    {
        return await _context.ShiftKerja.FindAsync(id);
    }

    public async Task<(bool Sukses, string Pesan)> SimpanShiftAsync(ShiftKerjaViewModel model)
    {
        var tenantId = _layananTenant.PerusahaanIdSaatIni;

        var kodeDuplikat = await _context.ShiftKerja
            .AnyAsync(s => s.KodeShift.ToUpper() == model.KodeShift.Trim().ToUpper() && (!model.Id.HasValue || s.Id != model.Id.Value));

        if (kodeDuplikat)
        {
            return (false, $"Kode shift '{model.KodeShift}' sudah digunakan pada perusahaan ini.");
        }

        if (model.Id.HasValue && model.Id.Value != Guid.Empty)
        {
            var shift = await _context.ShiftKerja.FindAsync(model.Id.Value);
            if (shift == null) return (false, "Shift kerja tidak ditemukan.");

            shift.NamaShift = model.NamaShift.Trim();
            shift.KodeShift = model.KodeShift.Trim().ToUpper();
            shift.JamMasuk = model.JamMasuk;
            shift.JamPulang = model.JamPulang;
            shift.ToleransiKeterlambatanMenit = model.ToleransiKeterlambatanMenit;
            shift.Keterangan = model.Keterangan;
            shift.StatusAktif = model.StatusAktif;

            _context.ShiftKerja.Update(shift);
            await _context.SaveChangesAsync();
            await _layananAudit.CatatAsync("UBAH_SHIFT", "ShiftKerja", $"Memperbarui data shift {shift.NamaShift} ({shift.KodeShift})");
            return (true, "Shift kerja berhasil diperbarui.");
        }
        else
        {
            var shiftBaru = new ShiftKerja
            {
                Id = Guid.NewGuid(),
                NamaShift = model.NamaShift.Trim(),
                KodeShift = model.KodeShift.Trim().ToUpper(),
                JamMasuk = model.JamMasuk,
                JamPulang = model.JamPulang,
                ToleransiKeterlambatanMenit = model.ToleransiKeterlambatanMenit,
                Keterangan = model.Keterangan,
                StatusAktif = model.StatusAktif
            };

            await _context.ShiftKerja.AddAsync(shiftBaru);
            await _context.SaveChangesAsync();
            await _layananAudit.CatatAsync("TAMBAH_SHIFT", "ShiftKerja", $"Menambahkan shift baru {shiftBaru.NamaShift} ({shiftBaru.KodeShift})");
            return (true, "Shift kerja baru berhasil disimpan.");
        }
    }

    public async Task<(bool Sukses, string Pesan)> HapusShiftAsync(Guid id)
    {
        var shift = await _context.ShiftKerja.FindAsync(id);
        if (shift == null) return (false, "Shift kerja tidak ditemukan.");

        var dipakaiKehadiran = await _context.Kehadiran.AnyAsync(k => k.ShiftKerjaId == id);
        if (dipakaiKehadiran)
        {
            return (false, "Shift kerja tidak dapat dihapus karena sudah memiliki riwayat presensi karyawan.");
        }

        _context.ShiftKerja.Remove(shift);
        await _context.SaveChangesAsync();
        await _layananAudit.CatatAsync("HAPUS_SHIFT", "ShiftKerja", $"Menghapus shift kerja {shift.NamaShift} ({shift.KodeShift})");
        return (true, "Shift kerja berhasil dihapus.");
    }

    public async Task<List<JadwalKerjaViewModel>> DapatkanJadwalKerjaAsync(DateTime tanggalMulai, DateTime tanggalSelesai, Guid? departemenId, Guid? karyawanId)
    {
        var query = _context.JadwalKerja
            .Include(j => j.Karyawan!)
                .ThenInclude(k => k.Departemen)
            .Include(j => j.ShiftKerja)
            .Where(j => j.Tanggal.Date >= tanggalMulai.Date && j.Tanggal.Date <= tanggalSelesai.Date);

        if (departemenId.HasValue)
        {
            query = query.Where(j => j.Karyawan != null && j.Karyawan.DepartemenId == departemenId.Value);
        }

        if (karyawanId.HasValue)
        {
            query = query.Where(j => j.KaryawanId == karyawanId.Value);
        }

        return await query
            .OrderBy(j => j.Tanggal)
            .ThenBy(j => j.Karyawan != null ? j.Karyawan.NamaLengkap : string.Empty)
            .Select(j => new JadwalKerjaViewModel
            {
                Id = j.Id,
                KaryawanId = j.KaryawanId,
                NomorIndukKaryawan = j.Karyawan != null ? j.Karyawan.NomorIndukKaryawan : string.Empty,
                NamaKaryawan = j.Karyawan != null ? j.Karyawan.NamaLengkap : string.Empty,
                NamaDepartemen = j.Karyawan != null && j.Karyawan.Departemen != null ? j.Karyawan.Departemen.NamaDepartemen : "-",
                ShiftKerjaId = j.ShiftKerjaId,
                NamaShift = j.ShiftKerja != null ? j.ShiftKerja.NamaShift : "Default",
                JamMasuk = j.ShiftKerja != null ? j.ShiftKerja.JamMasuk : TimeSpan.Zero,
                JamPulang = j.ShiftKerja != null ? j.ShiftKerja.JamPulang : TimeSpan.Zero,
                Tanggal = j.Tanggal,
                StatusHariLibur = j.StatusHariLibur,
                Catatan = j.Catatan
            })
            .ToListAsync();
    }

    public async Task<(bool Sukses, string Pesan)> SimpanJadwalKerjaAsync(FormJadwalKerjaViewModel model)
    {
        var jadwalAda = await _context.JadwalKerja
            .FirstOrDefaultAsync(j => j.KaryawanId == model.KaryawanId && j.Tanggal.Date == model.Tanggal.Date);

        if (jadwalAda != null)
        {
            jadwalAda.ShiftKerjaId = model.ShiftKerjaId;
            jadwalAda.StatusHariLibur = model.StatusHariLibur;
            jadwalAda.Catatan = model.Catatan;
            _context.JadwalKerja.Update(jadwalAda);
            await _context.SaveChangesAsync();
            return (true, "Jadwal kerja berhasil diperbarui.");
        }
        else
        {
            var baru = new JadwalKerja
            {
                Id = Guid.NewGuid(),
                KaryawanId = model.KaryawanId,
                ShiftKerjaId = model.ShiftKerjaId,
                Tanggal = model.Tanggal.Date,
                StatusHariLibur = model.StatusHariLibur,
                Catatan = model.Catatan
            };
            await _context.JadwalKerja.AddAsync(baru);
            await _context.SaveChangesAsync();
            return (true, "Jadwal kerja berhasil disimpan.");
        }
    }

    public async Task<(bool Sukses, string Pesan)> AjukanLemburAsync(Guid karyawanId, FormPengajuanLemburViewModel model)
    {
        if (model.JamSelesai <= model.JamMulai)
        {
            return (false, "Jam selesai lembur harus lebih besar dari jam mulai lembur.");
        }

        var totalJam = (decimal)(model.JamSelesai - model.JamMulai).TotalHours;

        var nomorPengajuan = $"LMBR/{DateTime.Today:yyyyMM}/{Guid.NewGuid().ToString()[..6].ToUpper()}";

        var pengajuan = new PengajuanLembur
        {
            Id = Guid.NewGuid(),
            KaryawanId = karyawanId,
            NomorPengajuan = nomorPengajuan,
            TanggalLembur = model.TanggalLembur.Date,
            JamMulai = model.JamMulai,
            JamSelesai = model.JamSelesai,
            TotalJam = Math.Round(totalJam, 2),
            AlasanLembur = model.AlasanLembur.Trim(),
            Status = StatusPengajuanLembur.MenungguPersetujuan
        };

        await _context.PengajuanLembur.AddAsync(pengajuan);
        await _context.SaveChangesAsync();

        await _layananAudit.CatatAsync("AJUKAN_LEMBUR", "PengajuanLembur", $"Karyawan {karyawanId} mengajukan lembur {nomorPengajuan} ({totalJam} jam).");

        return (true, $"Pengajuan lembur ({nomorPengajuan}) berhasil diajukan.");
    }

    public async Task<DaftarLemburViewModel> DapatkanDaftarLemburAsync(FilterLemburViewModel filter, Guid? karyawanId = null)
    {
        var query = _context.PengajuanLembur
            .Include(l => l.Karyawan!)
                .ThenInclude(k => k.Departemen)
            .Include(l => l.DisetujuiOleh)
            .AsQueryable();

        if (karyawanId.HasValue)
        {
            query = query.Where(l => l.KaryawanId == karyawanId.Value);
        }

        if (filter.KaryawanId.HasValue)
        {
            query = query.Where(l => l.KaryawanId == filter.KaryawanId.Value);
        }

        if (filter.Status.HasValue)
        {
            query = query.Where(l => l.Status == filter.Status.Value);
        }

        if (filter.Bulan.HasValue && filter.Tahun.HasValue)
        {
            var awal = new DateTime(filter.Tahun.Value, filter.Bulan.Value, 1);
            var akhir = awal.AddMonths(1).AddDays(-1);
            query = query.Where(l => l.TanggalLembur.Date >= awal.Date && l.TanggalLembur.Date <= akhir.Date);
        }

        var totalItem = await query.CountAsync();

        var data = await query
            .OrderByDescending(l => l.TanggalLembur)
            .Skip((filter.Halaman - 1) * filter.UkuranHalaman)
            .Take(filter.UkuranHalaman)
            .Select(l => new ItemLemburViewModel
            {
                Id = l.Id,
                KaryawanId = l.KaryawanId,
                NomorIndukKaryawan = l.Karyawan != null ? l.Karyawan.NomorIndukKaryawan : string.Empty,
                NamaKaryawan = l.Karyawan != null ? l.Karyawan.NamaLengkap : string.Empty,
                NamaDepartemen = l.Karyawan != null && l.Karyawan.Departemen != null ? l.Karyawan.Departemen.NamaDepartemen : "-",
                NomorPengajuan = l.NomorPengajuan,
                TanggalLembur = l.TanggalLembur,
                JamMulai = l.JamMulai,
                JamSelesai = l.JamSelesai,
                TotalJam = l.TotalJam,
                AlasanLembur = l.AlasanLembur,
                Status = l.Status,
                DisetujuiOleh = l.DisetujuiOleh != null ? l.DisetujuiOleh.NamaLengkap : null,
                WaktuPersetujuan = l.WaktuPersetujuan,
                CatatanPersetujuan = l.CatatanPersetujuan
            })
            .ToListAsync();

        return new DaftarLemburViewModel
        {
            Filter = filter,
            DaftarLembur = data,
            TotalItem = totalItem
        };
    }

    public async Task<(bool Sukses, string Pesan)> ProsesPersetujuanLemburAsync(Guid pengajuanLemburId, bool disetujui, string? catatan, Guid userId)
    {
        var lembur = await _context.PengajuanLembur.FindAsync(pengajuanLemburId);
        if (lembur == null) return (false, "Data pengajuan lembur tidak ditemukan.");

        if (lembur.Status != StatusPengajuanLembur.MenungguPersetujuan)
        {
            return (false, "Pengajuan lembur ini sudah diproses sebelumnya.");
        }

        lembur.Status = disetujui ? StatusPengajuanLembur.Disetujui : StatusPengajuanLembur.Ditolak;
        lembur.DisetujuiOlehId = userId;
        lembur.WaktuPersetujuan = DateTime.Now;
        lembur.CatatanPersetujuan = catatan;

        _context.PengajuanLembur.Update(lembur);
        await _context.SaveChangesAsync();

        var statusTeks = disetujui ? "disetujui" : "ditolak";
        await _layananAudit.CatatAsync("PERSETUJUAN_LEMBUR", "PengajuanLembur", $"Pengajuan lembur {lembur.NomorPengajuan} {statusTeks} oleh user {userId}.");

        return (true, $"Pengajuan lembur berhasil {statusTeks}.");
    }
}
