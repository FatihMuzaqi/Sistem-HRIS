using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using SistemHRD.Data;
using SistemHRD.Models.Entitas;
using SistemHRD.Models.Enums;
using SistemHRD.Services.Interfaces;
using SistemHRD.ViewModels;

namespace SistemHRD.Services.Implementasi;

public class LayananCuti : ILayananCuti
{
    private readonly KonteksAplikasi _context;
    private readonly IWebHostEnvironment _webHostEnvironment;
    private readonly ILayananAudit _layananAudit;
    private readonly ILayananTenant _layananTenant;

    public LayananCuti(
        KonteksAplikasi context,
        IWebHostEnvironment webHostEnvironment,
        ILayananAudit layananAudit,
        ILayananTenant layananTenant)
    {
        _context = context;
        _webHostEnvironment = webHostEnvironment;
        _layananAudit = layananAudit;
        _layananTenant = layananTenant;
    }

    public async Task<List<JenisCuti>> DapatkanDaftarJenisCutiAsync()
    {
        return await _context.JenisCuti
            .OrderBy(j => j.NamaJenisCuti)
            .ToListAsync();
    }

    public async Task<JenisCuti?> DapatkanJenisCutiByIdAsync(Guid id)
    {
        return await _context.JenisCuti.FindAsync(id);
    }

    public async Task<(bool Sukses, string Pesan)> SimpanJenisCutiAsync(JenisCutiViewModel model)
    {
        var duplikat = await _context.JenisCuti
            .AnyAsync(j => j.KodeJenisCuti.ToUpper() == model.KodeJenisCuti.Trim().ToUpper() && (!model.Id.HasValue || j.Id != model.Id.Value));

        if (duplikat)
        {
            return (false, $"Kode jenis cuti '{model.KodeJenisCuti}' sudah terdaftar.");
        }

        if (model.Id.HasValue && model.Id.Value != Guid.Empty)
        {
            var jc = await _context.JenisCuti.FindAsync(model.Id.Value);
            if (jc == null) return (false, "Jenis cuti tidak ditemukan.");

            jc.NamaJenisCuti = model.NamaJenisCuti.Trim();
            jc.KodeJenisCuti = model.KodeJenisCuti.Trim().ToUpper();
            jc.JatahHariPerTahun = model.JatahHariPerTahun;
            jc.PotongCutiTahunan = model.PotongCutiTahunan;
            jc.WajibLampiran = model.WajibLampiran;
            jc.Keterangan = model.Keterangan;
            jc.StatusAktif = model.StatusAktif;

            _context.JenisCuti.Update(jc);
            await _context.SaveChangesAsync();
            await _layananAudit.CatatAsync("UBAH_JENIS_CUTI", "JenisCuti", $"Memperbarui jenis cuti {jc.NamaJenisCuti} ({jc.KodeJenisCuti})");
            return (true, "Jenis cuti berhasil diperbarui.");
        }
        else
        {
            var jcBaru = new JenisCuti
            {
                Id = Guid.NewGuid(),
                NamaJenisCuti = model.NamaJenisCuti.Trim(),
                KodeJenisCuti = model.KodeJenisCuti.Trim().ToUpper(),
                JatahHariPerTahun = model.JatahHariPerTahun,
                PotongCutiTahunan = model.PotongCutiTahunan,
                WajibLampiran = model.WajibLampiran,
                Keterangan = model.Keterangan,
                StatusAktif = model.StatusAktif
            };

            await _context.JenisCuti.AddAsync(jcBaru);
            await _context.SaveChangesAsync();
            await _layananAudit.CatatAsync("TAMBAH_JENIS_CUTI", "JenisCuti", $"Menambahkan jenis cuti baru {jcBaru.NamaJenisCuti} ({jcBaru.KodeJenisCuti})");
            return (true, "Jenis cuti baru berhasil ditambahkan.");
        }
    }

    public async Task<(bool Sukses, string Pesan)> HapusJenisCutiAsync(Guid id)
    {
        var jc = await _context.JenisCuti.FindAsync(id);
        if (jc == null) return (false, "Jenis cuti tidak ditemukan.");

        var dipakai = await _context.PengajuanCuti.AnyAsync(p => p.JenisCutiId == id);
        if (dipakai)
        {
            return (false, "Jenis cuti tidak dapat dihapus karena sudah memiliki data pengajuan cuti karyawan.");
        }

        _context.JenisCuti.Remove(jc);
        await _context.SaveChangesAsync();
        await _layananAudit.CatatAsync("HAPUS_JENIS_CUTI", "JenisCuti", $"Menghapus jenis cuti {jc.NamaJenisCuti} ({jc.KodeJenisCuti})");
        return (true, "Jenis cuti berhasil dihapus.");
    }

    public async Task<List<SaldoCutiViewModel>> DapatkanDaftarSaldoCutiAsync(int tahun, Guid? departemenId, Guid? karyawanId)
    {
        var query = _context.SaldoCuti
            .Include(s => s.Karyawan!)
                .ThenInclude(k => k.Departemen)
            .Include(s => s.JenisCuti)
            .Where(s => s.PeriodeTahun == tahun);

        if (departemenId.HasValue)
        {
            query = query.Where(s => s.Karyawan != null && s.Karyawan.DepartemenId == departemenId.Value);
        }

        if (karyawanId.HasValue)
        {
            query = query.Where(s => s.KaryawanId == karyawanId.Value);
        }

        return await query
            .OrderBy(s => s.Karyawan != null ? s.Karyawan.NamaLengkap : string.Empty)
            .ThenBy(s => s.JenisCuti != null ? s.JenisCuti.NamaJenisCuti : string.Empty)
            .Select(s => new SaldoCutiViewModel
            {
                Id = s.Id,
                KaryawanId = s.KaryawanId,
                NomorIndukKaryawan = s.Karyawan != null ? s.Karyawan.NomorIndukKaryawan : string.Empty,
                NamaKaryawan = s.Karyawan != null ? s.Karyawan.NamaLengkap : string.Empty,
                NamaDepartemen = s.Karyawan != null && s.Karyawan.Departemen != null ? s.Karyawan.Departemen.NamaDepartemen : "-",
                JenisCutiId = s.JenisCutiId,
                NamaJenisCuti = s.JenisCuti != null ? s.JenisCuti.NamaJenisCuti : string.Empty,
                KodeJenisCuti = s.JenisCuti != null ? s.JenisCuti.KodeJenisCuti : string.Empty,
                PeriodeTahun = s.PeriodeTahun,
                JumlahHakCuti = s.JumlahHakCuti,
                CutiTerpakai = s.CutiTerpakai,
                SisaCuti = s.SisaCuti,
                Catatan = s.Catatan
            })
            .ToListAsync();
    }

    public async Task<SaldoCuti?> DapatkanSaldoCutiKaryawanAsync(Guid karyawanId, Guid jenisCutiId, int tahun)
    {
        return await _context.SaldoCuti
            .FirstOrDefaultAsync(s => s.KaryawanId == karyawanId && s.JenisCutiId == jenisCutiId && s.PeriodeTahun == tahun);
    }

    public async Task<(bool Sukses, string Pesan)> AturSaldoCutiAsync(AturSaldoCutiViewModel model)
    {
        var saldo = await _context.SaldoCuti
            .FirstOrDefaultAsync(s => s.KaryawanId == model.KaryawanId && s.JenisCutiId == model.JenisCutiId && s.PeriodeTahun == model.PeriodeTahun);

        if (saldo != null)
        {
            saldo.JumlahHakCuti = model.JumlahHakCuti;
            saldo.SisaCuti = Math.Max(0, model.JumlahHakCuti - saldo.CutiTerpakai);
            saldo.Catatan = model.Catatan;
            _context.SaldoCuti.Update(saldo);
            await _context.SaveChangesAsync();
            await _layananAudit.CatatAsync("UBAH_SALDO_CUTI", "SaldoCuti", $"Memperbarui saldo cuti karyawan {model.KaryawanId} tahun {model.PeriodeTahun} menjadi {model.JumlahHakCuti} hari.");
            return (true, "Saldo cuti berhasil diperbarui.");
        }
        else
        {
            var baru = new SaldoCuti
            {
                Id = Guid.NewGuid(),
                KaryawanId = model.KaryawanId,
                JenisCutiId = model.JenisCutiId,
                PeriodeTahun = model.PeriodeTahun,
                JumlahHakCuti = model.JumlahHakCuti,
                CutiTerpakai = 0,
                SisaCuti = model.JumlahHakCuti,
                Catatan = model.Catatan
            };
            await _context.SaldoCuti.AddAsync(baru);
            await _context.SaveChangesAsync();
            await _layananAudit.CatatAsync("TAMBAH_SALDO_CUTI", "SaldoCuti", $"Inisialisasi saldo cuti karyawan {model.KaryawanId} tahun {model.PeriodeTahun} sebesar {model.JumlahHakCuti} hari.");
            return (true, "Saldo cuti berhasil diatur.");
        }
    }

    public async Task<(bool Sukses, string Pesan)> InisialisasiSaldoCutiTahunanMassalAsync(int tahun)
    {
        var cutiTahunan = await _context.JenisCuti
            .FirstOrDefaultAsync(j => j.KodeJenisCuti == "CT-THN");

        if (cutiTahunan == null)
        {
            return (false, "Jenis Cuti Tahunan (Kode: CT-THN) belum dikonfigurasi.");
        }

        var karyawanAktif = await _context.Karyawan
            .Where(k => k.StatusKaryawan == StatusKaryawan.Tetap || k.StatusKaryawan == StatusKaryawan.Kontrak)
            .ToListAsync();

        int dibuat = 0;
        foreach (var kry in karyawanAktif)
        {
            var ada = await _context.SaldoCuti
                .AnyAsync(s => s.KaryawanId == kry.Id && s.JenisCutiId == cutiTahunan.Id && s.PeriodeTahun == tahun);

            if (!ada)
            {
                await _context.SaldoCuti.AddAsync(new SaldoCuti
                {
                    Id = Guid.NewGuid(),
                    KaryawanId = kry.Id,
                    JenisCutiId = cutiTahunan.Id,
                    PeriodeTahun = tahun,
                    JumlahHakCuti = cutiTahunan.JatahHariPerTahun,
                    CutiTerpakai = 0,
                    SisaCuti = cutiTahunan.JatahHariPerTahun,
                    Catatan = $"Inisialisasi otomatis saldo cuti tahunan periode {tahun}"
                });
                dibuat++;
            }
        }

        await _context.SaveChangesAsync();
        await _layananAudit.CatatAsync("INISIALISASI_SALDO_MASSAL", "SaldoCuti", $"Menginisialisasi saldo cuti tahun {tahun} untuk {dibuat} karyawan.");
        return (true, $"Berhasil menginisialisasi saldo cuti tahun {tahun} untuk {dibuat} karyawan.");
    }

    public async Task<(bool Sukses, string Pesan, Guid? PengajuanId)> AjukanCutiAsync(Guid karyawanId, FormPengajuanCutiViewModel model, IFormFile? berkasDokumen)
    {
        if (model.TanggalSelesai.Date < model.TanggalMulai.Date)
        {
            return (false, "Tanggal selesai cuti tidak boleh mendahului tanggal mulai cuti.", null);
        }

        var jumlahHari = (model.TanggalSelesai.Date - model.TanggalMulai.Date).Days + 1;
        if (jumlahHari <= 0)
        {
            return (false, "Jumlah hari cuti tidak valid.", null);
        }

        var jenisCuti = await _context.JenisCuti.FindAsync(model.JenisCutiId);
        if (jenisCuti == null)
        {
            return (false, "Jenis cuti yang dipilih tidak valid.", null);
        }

        // 1. Validasi Bentrok Jadwal Pengajuan Cuti Lain
        var adaBentrok = await _context.PengajuanCuti
            .AnyAsync(c =>
                c.KaryawanId == karyawanId &&
                c.Status != StatusPengajuanCuti.Ditolak &&
                c.Status != StatusPengajuanCuti.Dibatalkan &&
                c.TanggalMulai.Date <= model.TanggalSelesai.Date &&
                c.TanggalSelesai.Date >= model.TanggalMulai.Date);

        if (adaBentrok)
        {
            return (false, "Terdapat jadwal pengajuan cuti lain yang bersinggungan pada rentang tanggal tersebut.", null);
        }

        // 2. Validasi Sisa Saldo Cuti
        if (jenisCuti.PotongCutiTahunan)
        {
            var saldo = await _context.SaldoCuti
                .FirstOrDefaultAsync(s =>
                    s.KaryawanId == karyawanId &&
                    s.JenisCutiId == jenisCuti.Id &&
                    s.PeriodeTahun == model.TanggalMulai.Year);

            if (saldo == null)
            {
                // Cek apakah ada saldo di cuti tahunan reguler
                var cutiTahunan = await _context.JenisCuti.FirstOrDefaultAsync(j => j.KodeJenisCuti == "CT-THN");
                if (cutiTahunan != null)
                {
                    saldo = await _context.SaldoCuti.FirstOrDefaultAsync(s =>
                        s.KaryawanId == karyawanId &&
                        s.JenisCutiId == cutiTahunan.Id &&
                        s.PeriodeTahun == model.TanggalMulai.Year);
                }
            }

            var sisa = saldo?.SisaCuti ?? 0;
            if (sisa < jumlahHari)
            {
                return (false, $"Sisa saldo cuti Anda tidak mencukupi (Tersisa: {sisa} hari, Diajukan: {jumlahHari} hari).", null);
            }
        }

        // 3. Validasi Dokumen Wajib
        if (jenisCuti.WajibLampiran && berkasDokumen == null)
        {
            return (false, $"Jenis cuti '{jenisCuti.NamaJenisCuti}' mewajibkan unggah dokumen pendukung (misal: surat keterangan dokter).", null);
        }

        string? namaDokumen = null;
        string? jalurDokumen = null;

        if (berkasDokumen != null && berkasDokumen.Length > 0)
        {
            if (berkasDokumen.Length > 5 * 1024 * 1024)
            {
                return (false, "Ukuran berkas lampiran tidak boleh lebih dari 5 MB.", null);
            }

            var ekstensi = Path.GetExtension(berkasDokumen.FileName).ToLowerInvariant();
            var ekstensiDiizinkan = new[] { ".pdf", ".jpg", ".jpeg", ".png", ".doc", ".docx" };
            if (!ekstensiDiizinkan.Contains(ekstensi))
            {
                return (false, "Format berkas tidak diizinkan. Gunakan PDF, JPG, PNG, atau DOC/DOCX.", null);
            }

            var folderTarget = Path.Combine(_webHostEnvironment.ContentRootPath, "App_Data", "DokumenCuti");
            Directory.CreateDirectory(folderTarget);

            var namaFileUnik = $"CUTI_{Guid.NewGuid():N}{ekstensi}";
            jalurDokumen = Path.Combine(folderTarget, namaFileUnik);

            using (var stream = new FileStream(jalurDokumen, FileMode.Create))
            {
                await berkasDokumen.CopyToAsync(stream);
            }

            namaDokumen = berkasDokumen.FileName;
        }

        var nomorPengajuan = $"CUTI/{model.TanggalMulai:yyyyMM}/{Guid.NewGuid().ToString()[..6].ToUpper()}";

        var pengajuan = new PengajuanCuti
        {
            Id = Guid.NewGuid(),
            KaryawanId = karyawanId,
            JenisCutiId = model.JenisCutiId,
            NomorPengajuan = nomorPengajuan,
            TanggalMulai = model.TanggalMulai.Date,
            TanggalSelesai = model.TanggalSelesai.Date,
            JumlahHari = jumlahHari,
            Alasan = model.Alasan.Trim(),
            AlamatSelamaCuti = model.AlamatSelamaCuti,
            NomorTeleponDarurat = model.NomorTeleponDarurat,
            NamaDokumenPendukung = namaDokumen,
            JalurDokumenPendukung = jalurDokumen,
            Status = StatusPengajuanCuti.MenungguPersetujuanManager
        };

        await _context.PengajuanCuti.AddAsync(pengajuan);
        await _context.SaveChangesAsync();

        await _layananAudit.CatatAsync("AJUKAN_CUTI", "PengajuanCuti", $"Karyawan {karyawanId} mengajukan cuti {nomorPengajuan} ({jumlahHari} hari, mulai {model.TanggalMulai:dd/MM/yyyy}).");

        return (true, $"Pengajuan cuti ({nomorPengajuan}) berhasil dikirim dan menunggu persetujuan Manager.", pengajuan.Id);
    }

    public async Task<DaftarPengajuanCutiViewModel> DapatkanDaftarPengajuanCutiAsync(FilterPengajuanCutiViewModel filter, Guid? karyawanId = null)
    {
        var query = _context.PengajuanCuti
            .Include(p => p.Karyawan!)
                .ThenInclude(k => k.Departemen)
            .Include(p => p.JenisCuti)
            .AsQueryable();

        if (karyawanId.HasValue)
        {
            query = query.Where(p => p.KaryawanId == karyawanId.Value);
        }

        if (filter.KaryawanId.HasValue)
        {
            query = query.Where(p => p.KaryawanId == filter.KaryawanId.Value);
        }

        if (filter.DepartemenId.HasValue)
        {
            query = query.Where(p => p.Karyawan != null && p.Karyawan.DepartemenId == filter.DepartemenId.Value);
        }

        if (filter.Status.HasValue)
        {
            query = query.Where(p => p.Status == filter.Status.Value);
        }

        if (filter.JenisCutiId.HasValue)
        {
            query = query.Where(p => p.JenisCutiId == filter.JenisCutiId.Value);
        }

        if (filter.Bulan.HasValue && filter.Tahun.HasValue)
        {
            var awal = new DateTime(filter.Tahun.Value, filter.Bulan.Value, 1);
            var akhir = awal.AddMonths(1).AddDays(-1);
            query = query.Where(p => p.TanggalMulai.Date >= awal.Date && p.TanggalMulai.Date <= akhir.Date);
        }

        if (!string.IsNullOrWhiteSpace(filter.Cari))
        {
            var cari = filter.Cari.Trim().ToLower();
            query = query.Where(p =>
                p.NomorPengajuan.ToLower().Contains(cari) ||
                (p.Karyawan != null && (p.Karyawan.NamaLengkap.ToLower().Contains(cari) || p.Karyawan.NomorIndukKaryawan.ToLower().Contains(cari))));
        }

        var totalItem = await query.CountAsync();

        var semuaStatus = await query.Select(p => p.Status).ToListAsync();
        var totalMenungguManager = semuaStatus.Count(s => s == StatusPengajuanCuti.MenungguPersetujuanManager);
        var totalMenungguHRD = semuaStatus.Count(s => s == StatusPengajuanCuti.DisetujuiManager);
        var totalDisetujui = semuaStatus.Count(s => s == StatusPengajuanCuti.Disetujui);
        var totalDitolak = semuaStatus.Count(s => s == StatusPengajuanCuti.Ditolak);

        var data = await query
            .OrderByDescending(p => p.CreatedAt)
            .Skip((filter.Halaman - 1) * filter.UkuranHalaman)
            .Take(filter.UkuranHalaman)
            .Select(p => new PengajuanCutiItemViewModel
            {
                Id = p.Id,
                NomorPengajuan = p.NomorPengajuan,
                KaryawanId = p.KaryawanId,
                NomorIndukKaryawan = p.Karyawan != null ? p.Karyawan.NomorIndukKaryawan : string.Empty,
                NamaKaryawan = p.Karyawan != null ? p.Karyawan.NamaLengkap : string.Empty,
                NamaDepartemen = p.Karyawan != null && p.Karyawan.Departemen != null ? p.Karyawan.Departemen.NamaDepartemen : "-",
                JenisCutiId = p.JenisCutiId,
                NamaJenisCuti = p.JenisCuti != null ? p.JenisCuti.NamaJenisCuti : string.Empty,
                TanggalMulai = p.TanggalMulai,
                TanggalSelesai = p.TanggalSelesai,
                JumlahHari = p.JumlahHari,
                Alasan = p.Alasan,
                Status = p.Status,
                AdaDokumen = !string.IsNullOrEmpty(p.NamaDokumenPendukung),
                TanggalDiajukan = p.CreatedAt
            })
            .ToListAsync();

        // Info saldo cuti pribadi jika spesifik karyawan
        int? saldoPribadi = null;
        int? terpakaiPribadi = null;
        int? sisaPribadi = null;

        if (karyawanId.HasValue)
        {
            var saldo = await _context.SaldoCuti
                .Include(s => s.JenisCuti)
                .FirstOrDefaultAsync(s => s.KaryawanId == karyawanId.Value && s.JenisCuti != null && s.JenisCuti.KodeJenisCuti == "CT-THN" && s.PeriodeTahun == DateTime.Today.Year);

            if (saldo != null)
            {
                saldoPribadi = saldo.JumlahHakCuti;
                terpakaiPribadi = saldo.CutiTerpakai;
                sisaPribadi = saldo.SisaCuti;
            }
        }

        return new DaftarPengajuanCutiViewModel
        {
            Filter = filter,
            DaftarPengajuan = data,
            TotalItem = totalItem,
            TotalMenungguManager = totalMenungguManager,
            TotalMenungguHRD = totalMenungguHRD,
            TotalDisetujui = totalDisetujui,
            TotalDitolak = totalDitolak,
            SaldoCutiPribadi = saldoPribadi,
            CutiTerpakaiPribadi = terpakaiPribadi,
            SisaCutiPribadi = sisaPribadi
        };
    }

    public async Task<PengajuanCutiDetailViewModel?> DapatkanDetailPengajuanCutiAsync(Guid id)
    {
        var pengajuan = await _context.PengajuanCuti
            .Include(p => p.Karyawan!)
                .ThenInclude(k => k.Departemen)
            .Include(p => p.Karyawan!)
                .ThenInclude(k => k.Jabatan)
            .Include(p => p.JenisCuti)
            .Include(p => p.DaftarPersetujuan)
                .ThenInclude(ap => ap.Penyetuju)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (pengajuan == null) return null;

        // Ambil sisa saldo cuti
        var saldo = await _context.SaldoCuti
            .FirstOrDefaultAsync(s => s.KaryawanId == pengajuan.KaryawanId && s.JenisCutiId == pengajuan.JenisCutiId && s.PeriodeTahun == pengajuan.TanggalMulai.Year);

        return new PengajuanCutiDetailViewModel
        {
            Id = pengajuan.Id,
            NomorPengajuan = pengajuan.NomorPengajuan,
            KaryawanId = pengajuan.KaryawanId,
            NomorIndukKaryawan = pengajuan.Karyawan?.NomorIndukKaryawan ?? string.Empty,
            NamaKaryawan = pengajuan.Karyawan?.NamaLengkap ?? string.Empty,
            NamaDepartemen = pengajuan.Karyawan?.Departemen?.NamaDepartemen ?? "-",
            NamaJabatan = pengajuan.Karyawan?.Jabatan?.NamaJabatan ?? "-",
            Email = pengajuan.Karyawan?.Email ?? string.Empty,
            NomorTelepon = pengajuan.Karyawan?.NomorTelepon,
            JenisCutiId = pengajuan.JenisCutiId,
            NamaJenisCuti = pengajuan.JenisCuti?.NamaJenisCuti ?? string.Empty,
            PotongCutiTahunan = pengajuan.JenisCuti?.PotongCutiTahunan ?? false,
            TanggalMulai = pengajuan.TanggalMulai,
            TanggalSelesai = pengajuan.TanggalSelesai,
            JumlahHari = pengajuan.JumlahHari,
            Alasan = pengajuan.Alasan,
            AlamatSelamaCuti = pengajuan.AlamatSelamaCuti,
            NomorTeleponDarurat = pengajuan.NomorTeleponDarurat,
            NamaDokumenPendukung = pengajuan.NamaDokumenPendukung,
            Status = pengajuan.Status,
            TanggalDiajukan = pengajuan.CreatedAt,
            SisaSaldoCuti = saldo?.SisaCuti,
            DaftarPersetujuan = pengajuan.DaftarPersetujuan
                .OrderBy(ap => ap.WaktuPersetujuan)
                .Select(ap => new ItemRiwayatPersetujuanViewModel
                {
                    Tahap = ap.Tahap,
                    NamaPenyetuju = ap.Penyetuju != null ? ap.Penyetuju.NamaLengkap : "User",
                    Keputusan = ap.Keputusan,
                    Catatan = ap.Catatan,
                    WaktuPersetujuan = ap.WaktuPersetujuan
                })
                .ToList()
        };
    }

    public async Task<(bool Sukses, string Pesan)> ProsesPersetujuanManagerAsync(Guid pengajuanCutiId, bool disetujui, string? catatan, Guid userId)
    {
        var pengajuan = await _context.PengajuanCuti.FindAsync(pengajuanCutiId);
        if (pengajuan == null) return (false, "Pengajuan cuti tidak ditemukan.");

        if (pengajuan.Status != StatusPengajuanCuti.MenungguPersetujuanManager)
        {
            return (false, "Pengajuan cuti ini tidak dalam status menunggu persetujuan Manager.");
        }

        pengajuan.Status = disetujui ? StatusPengajuanCuti.DisetujuiManager : StatusPengajuanCuti.Ditolak;

        var persetujuan = new PersetujuanCuti
        {
            Id = Guid.NewGuid(),
            PengajuanCutiId = pengajuan.Id,
            Tahap = TahapPersetujuanCuti.Manager,
            PenyetujuId = userId,
            Keputusan = disetujui ? KeputusanPersetujuan.Disetujui : KeputusanPersetujuan.Ditolak,
            Catatan = catatan,
            WaktuPersetujuan = DateTime.UtcNow
        };

        _context.PengajuanCuti.Update(pengajuan);
        await _context.PersetujuanCuti.AddAsync(persetujuan);
        await _context.SaveChangesAsync();

        var aksiTeks = disetujui ? "disetujui oleh Manager dan diteruskan ke verifikasi HRD" : "ditolak oleh Manager";
        await _layananAudit.CatatAsync("PERSETUJUAN_CUTI_MANAGER", "PengajuanCuti", $"Pengajuan cuti {pengajuan.NomorPengajuan} {aksiTeks}.");

        return (true, $"Pengajuan cuti berhasil {aksiTeks}.");
    }

    public async Task<(bool Sukses, string Pesan)> ProsesVerifikasiHRDAsync(Guid pengajuanCutiId, bool disetujui, string? catatan, Guid userId)
    {
        var pengajuan = await _context.PengajuanCuti
            .Include(p => p.JenisCuti)
            .FirstOrDefaultAsync(p => p.Id == pengajuanCutiId);

        if (pengajuan == null) return (false, "Pengajuan cuti tidak ditemukan.");

        // Validasi Idempotensi: Saldo tidak boleh berkurang dua kali jika approval dipanggil ulang!
        if (pengajuan.Status == StatusPengajuanCuti.Disetujui)
        {
            return (false, "Pengajuan cuti ini sudah disetujui dan diverifikasi sebelumnya.");
        }

        if (pengajuan.Status == StatusPengajuanCuti.Ditolak)
        {
            return (false, "Pengajuan cuti ini sudah ditolak sebelumnya.");
        }

        if (!disetujui)
        {
            pengajuan.Status = StatusPengajuanCuti.Ditolak;

            var persetujuanTolak = new PersetujuanCuti
            {
                Id = Guid.NewGuid(),
                PengajuanCutiId = pengajuan.Id,
                Tahap = TahapPersetujuanCuti.HRD,
                PenyetujuId = userId,
                Keputusan = KeputusanPersetujuan.Ditolak,
                Catatan = catatan,
                WaktuPersetujuan = DateTime.UtcNow
            };

            _context.PengajuanCuti.Update(pengajuan);
            await _context.PersetujuanCuti.AddAsync(persetujuanTolak);
            await _context.SaveChangesAsync();

            await _layananAudit.CatatAsync("VERIFIKASI_CUTI_HRD", "PengajuanCuti", $"Pengajuan cuti {pengajuan.NomorPengajuan} ditolak oleh HRD.");
            return (true, "Pengajuan cuti berhasil ditolak oleh HRD.");
        }

        // Jalankan Transaksi Database untuk perubahan saldo cuti secara atomik
        using var transaksi = await _context.Database.BeginTransactionAsync();
        try
        {
            // 1. Pemotongan Saldo Cuti jika jenis cuti memotong jatah
            if (pengajuan.JenisCuti != null && pengajuan.JenisCuti.PotongCutiTahunan)
            {
                var saldo = await _context.SaldoCuti
                    .FirstOrDefaultAsync(s =>
                        s.KaryawanId == pengajuan.KaryawanId &&
                        s.JenisCutiId == pengajuan.JenisCutiId &&
                        s.PeriodeTahun == pengajuan.TanggalMulai.Year);

                if (saldo == null)
                {
                    // Coba saldo Cuti Tahunan umum
                    var ctUmum = await _context.JenisCuti.FirstOrDefaultAsync(j => j.KodeJenisCuti == "CT-THN");
                    if (ctUmum != null)
                    {
                        saldo = await _context.SaldoCuti.FirstOrDefaultAsync(s =>
                            s.KaryawanId == pengajuan.KaryawanId &&
                            s.JenisCutiId == ctUmum.Id &&
                            s.PeriodeTahun == pengajuan.TanggalMulai.Year);
                    }
                }

                if (saldo != null)
                {
                    if (saldo.SisaCuti < pengajuan.JumlahHari)
                    {
                        await transaksi.RollbackAsync();
                        return (false, $"Sisa saldo cuti tidak mencukupi (Sisa: {saldo.SisaCuti} hari, Dibutuhkan: {pengajuan.JumlahHari} hari).");
                    }

                    saldo.CutiTerpakai += pengajuan.JumlahHari;
                    saldo.SisaCuti = Math.Max(0, saldo.JumlahHakCuti - saldo.CutiTerpakai);
                    _context.SaldoCuti.Update(saldo);
                }
            }

            // 2. Hubungkan langsung ke Kehadiran: Buat/perbarui kehadiran berstatus Cuti
            for (var tgl = pengajuan.TanggalMulai.Date; tgl <= pengajuan.TanggalSelesai.Date; tgl = tgl.AddDays(1))
            {
                if (tgl.DayOfWeek == DayOfWeek.Sunday) continue;

                var presensiAda = await _context.Kehadiran
                    .FirstOrDefaultAsync(k => k.KaryawanId == pengajuan.KaryawanId && k.Tanggal.Date == tgl);

                if (presensiAda == null)
                {
                    var presensiCuti = new Kehadiran
                    {
                        Id = Guid.NewGuid(),
                        KaryawanId = pengajuan.KaryawanId,
                        Tanggal = tgl,
                        Status = StatusKehadiran.Cuti,
                        CatatanMasuk = $"Cuti: {pengajuan.JenisCuti?.NamaJenisCuti} ({pengajuan.NomorPengajuan})"
                    };
                    await _context.Kehadiran.AddAsync(presensiCuti);
                }
                else
                {
                    presensiAda.Status = StatusKehadiran.Cuti;
                    presensiAda.CatatanMasuk = $"Cuti: {pengajuan.JenisCuti?.NamaJenisCuti} ({pengajuan.NomorPengajuan})";
                    _context.Kehadiran.Update(presensiAda);
                }
            }

            // 3. Perbarui Status Pengajuan Cuti
            pengajuan.Status = StatusPengajuanCuti.Disetujui;
            _context.PengajuanCuti.Update(pengajuan);

            // 4. Catat Log Persetujuan HRD
            var persetujuanHrd = new PersetujuanCuti
            {
                Id = Guid.NewGuid(),
                PengajuanCutiId = pengajuan.Id,
                Tahap = TahapPersetujuanCuti.HRD,
                PenyetujuId = userId,
                Keputusan = KeputusanPersetujuan.Disetujui,
                Catatan = catatan,
                WaktuPersetujuan = DateTime.UtcNow
            };
            await _context.PersetujuanCuti.AddAsync(persetujuanHrd);

            await _context.SaveChangesAsync();
            await transaksi.CommitAsync();

            await _layananAudit.CatatAsync("VERIFIKASI_CUTI_HRD", "PengajuanCuti",
                $"Pengajuan cuti {pengajuan.NomorPengajuan} diverifikasi dan disetujui HRD (Potong saldo: {pengajuan.JumlahHari} hari).");

            return (true, "Pengajuan cuti berhasil diverifikasi dan disetujui oleh HRD. Saldo cuti telah diperbarui.");
        }
        catch (Exception ex)
        {
            await transaksi.RollbackAsync();
            return (false, $"Terjadi kesalahan saat memproses approval cuti: {ex.Message}");
        }
    }

    public async Task<(byte[]? Konten, string? TipeKonten, string? NamaFile)> UnduhDokumenCutiAsync(Guid pengajuanCutiId)
    {
        var pengajuan = await _context.PengajuanCuti.FindAsync(pengajuanCutiId);
        if (pengajuan == null || string.IsNullOrEmpty(pengajuan.JalurDokumenPendukung))
        {
            return (null, null, null);
        }

        if (!File.Exists(pengajuan.JalurDokumenPendukung))
        {
            return (null, null, null);
        }

        var bytes = await File.ReadAllBytesAsync(pengajuan.JalurDokumenPendukung);
        var ekstensi = Path.GetExtension(pengajuan.JalurDokumenPendukung).ToLowerInvariant();

        var tipeKonten = ekstensi switch
        {
            ".pdf" => "application/pdf",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".doc" => "application/msword",
            ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            _ => "application/octet-stream"
        };

        return (bytes, tipeKonten, pengajuan.NamaDokumenPendukung ?? Path.GetFileName(pengajuan.JalurDokumenPendukung));
    }
}
