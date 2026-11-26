using System.Text;
using Microsoft.EntityFrameworkCore;
using SistemHRD.Data;
using SistemHRD.Models.Enums;
using SistemHRD.Services.Interfaces;
using SistemHRD.ViewModels;

namespace SistemHRD.Services.Implementasi;

public class LayananLaporanPusat : ILayananLaporanPusat
{
    private readonly KonteksAplikasi _db;
    private readonly ILayananTenant _layananTenant;

    public LayananLaporanPusat(KonteksAplikasi db, ILayananTenant layananTenant)
    {
        _db = db;
        _layananTenant = layananTenant;
    }

    public async Task<LaporanHubViewModel> DapatkanRingkasanEksekutifAsync()
    {
        var sekarang = DateTime.Today;
        var awalBulan = new DateTime(sekarang.Year, sekarang.Month, 1);
        var akhirBulan = awalBulan.AddMonths(1).AddDays(-1);

        var totalKaryawanAktif = await _db.Karyawan
            .AsNoTracking()
            .CountAsync(k => !k.IsDeleted && (k.StatusKaryawan == StatusKaryawan.Tetap || k.StatusKaryawan == StatusKaryawan.Kontrak || k.StatusKaryawan == StatusKaryawan.Percobaan));

        // Kehadiran bulan ini
        var queryKehadiranBulanIni = _db.Kehadiran
            .AsNoTracking()
            .Where(k => !k.IsDeleted && k.Tanggal >= awalBulan && k.Tanggal <= akhirBulan);

        var totalCatatanKehadiran = await queryKehadiranBulanIni.CountAsync();
        var totalHadir = await queryKehadiranBulanIni.CountAsync(k => k.Status == StatusKehadiran.Hadir);
        decimal persentaseKehadiran = totalCatatanKehadiran > 0
            ? Math.Round(((decimal)totalHadir / totalCatatanKehadiran) * 100, 1)
            : 0;

        // Total beban payroll bulan ini (periode berjalan)
        var totalBebanPayroll = await _db.Penggajian
            .AsNoTracking()
            .Where(p => !p.IsDeleted && p.PeriodePenggajian != null && p.PeriodePenggajian.Tahun == sekarang.Year && p.PeriodePenggajian.Bulan == sekarang.Month)
            .SumAsync(p => (decimal?)p.GajiBersih) ?? 0;

        // Total lowongan aktif
        var totalLowonganAktif = await _db.LowonganKerja
            .AsNoTracking()
            .CountAsync(l => !l.IsDeleted && l.Status == StatusLowongan.Dibuka);

        // Rata-rata skor kinerja
        var rataRataSkor = await _db.PenilaianKinerja
            .AsNoTracking()
            .Where(p => !p.IsDeleted && p.Status == StatusPenilaianKinerja.Disetujui)
            .AverageAsync(p => (decimal?)p.TotalSkorAkhir) ?? 0;

        // Pelatihan berlangsung / pendaftaran dibuka
        var totalPelatihan = await _db.ProgramPelatihan
            .AsNoTracking()
            .CountAsync(p => !p.IsDeleted && (p.Status == StatusPelatihan.Berlangsung || p.Status == StatusPelatihan.PendaftaranDibuka));

        // Aset inventaris
        var totalAset = await _db.AsetPerusahaan
            .AsNoTracking()
            .CountAsync(a => !a.IsDeleted && a.Status != StatusAset.Dihapuskan);

        var totalNilaiAset = await _db.AsetPerusahaan
            .AsNoTracking()
            .Where(a => !a.IsDeleted && a.Status != StatusAset.Dihapuskan)
            .SumAsync(a => (decimal?)a.NilaiPerolehan) ?? 0;

        return new LaporanHubViewModel
        {
            TotalKaryawanAktif = totalKaryawanAktif,
            PersentaseKehadiranBulanIni = persentaseKehadiran,
            TotalBebanPayrollBulanIni = totalBebanPayroll,
            TotalLowonganAktif = totalLowonganAktif,
            RataRataSkorKinerja = Math.Round(rataRataSkor, 2),
            TotalPelatihanBerjalan = totalPelatihan,
            TotalAsetTercatat = totalAset,
            TotalNilaiAset = totalNilaiAset
        };
    }

    // 1. Karyawan & Departemen
    public async Task<LaporanKaryawanViewModel> DapatkanLaporanKaryawanAsync(Guid? departemenId, StatusKaryawan? status, string? kataKunci, int halaman = 1, int ukuranHalaman = 15)
    {
        var query = _db.Karyawan
            .AsNoTracking()
            .Include(k => k.Departemen)
            .Include(k => k.Jabatan)
            .Where(k => !k.IsDeleted)
            .AsQueryable();

        if (departemenId.HasValue && departemenId != Guid.Empty)
        {
            query = query.Where(k => k.DepartemenId == departemenId);
        }

        if (status.HasValue)
        {
            query = query.Where(k => k.StatusKaryawan == status.Value);
        }

        if (!string.IsNullOrWhiteSpace(kataKunci))
        {
            var kWord = kataKunci.Trim().ToLower();
            query = query.Where(k => k.NomorIndukKaryawan.ToLower().Contains(kWord) ||
                                     k.NamaLengkap.ToLower().Contains(kWord) ||
                                     k.Email.ToLower().Contains(kWord));
        }

        var totalData = await query.CountAsync();
        var totalHalaman = (int)Math.Ceiling(totalData / (double)ukuranHalaman);
        if (totalHalaman < 1) totalHalaman = 1;
        if (halaman < 1) halaman = 1;
        if (halaman > totalHalaman) halaman = totalHalaman;

        var items = await query
            .OrderBy(k => k.Departemen != null ? k.Departemen.NamaDepartemen : "")
            .ThenBy(k => k.NamaLengkap)
            .Skip((halaman - 1) * ukuranHalaman)
            .Take(ukuranHalaman)
            .Select(k => new BarisLaporanKaryawanViewModel
            {
                NIK = k.NomorIndukKaryawan,
                NamaLengkap = k.NamaLengkap,
                Email = k.Email,
                NomorTelepon = k.NomorTelepon ?? "-",
                NamaDepartemen = k.Departemen != null ? k.Departemen.NamaDepartemen : "-",
                NamaJabatan = k.Jabatan != null ? k.Jabatan.NamaJabatan : "-",
                JenisKelamin = k.JenisKelamin == JenisKelamin.LakiLaki ? "Laki-laki" : "Perempuan",
                TanggalMasuk = k.TanggalMasuk,
                Status = k.StatusKaryawan
            })
            .ToListAsync();

        // Statistik
        var deptStats = await _db.Karyawan
            .AsNoTracking()
            .Where(k => !k.IsDeleted)
            .Include(k => k.Departemen)
            .GroupBy(k => k.Departemen != null ? k.Departemen.NamaDepartemen : "Tanpa Departemen")
            .Select(g => new { Nama = g.Key, Jumlah = g.Count() })
            .ToDictionaryAsync(x => x.Nama, x => x.Jumlah);

        var statusStats = await _db.Karyawan
            .AsNoTracking()
            .Where(k => !k.IsDeleted)
            .GroupBy(k => k.StatusKaryawan)
            .Select(g => new { Status = g.Key.ToString(), Jumlah = g.Count() })
            .ToDictionaryAsync(x => x.Status, x => x.Jumlah);

        return new LaporanKaryawanViewModel
        {
            DepartemenIdTerpilih = departemenId,
            StatusTerpilih = status,
            KataKunci = kataKunci,
            DataKaryawan = items,
            HalamanSaatIni = halaman,
            TotalHalaman = totalHalaman,
            TotalData = totalData,
            UkuranHalaman = ukuranHalaman,
            StatistikDepartemen = deptStats,
            StatistikStatus = statusStats
        };
    }

    public async Task<LaporanDistribusiDepartemenViewModel> DapatkanLaporanDepartemenAsync()
    {
        var departemenList = await _db.Departemen
            .AsNoTracking()
            .Where(d => !d.IsDeleted)
            .Include(d => d.DaftarKaryawan)
            .OrderBy(d => d.NamaDepartemen)
            .ToListAsync();

        var rekap = departemenList.Select(d => new BarisLaporanDepartemenViewModel
        {
            DepartemenId = d.Id,
            KodeDepartemen = d.KodeDepartemen,
            NamaDepartemen = d.NamaDepartemen,
            TotalKaryawan = d.DaftarKaryawan.Count(k => !k.IsDeleted),
            TotalKaryawanTetap = d.DaftarKaryawan.Count(k => !k.IsDeleted && k.StatusKaryawan == StatusKaryawan.Tetap),
            TotalKaryawanKontrak = d.DaftarKaryawan.Count(k => !k.IsDeleted && (k.StatusKaryawan == StatusKaryawan.Kontrak || k.StatusKaryawan == StatusKaryawan.Percobaan)),
            TotalLakiLaki = d.DaftarKaryawan.Count(k => !k.IsDeleted && k.JenisKelamin == JenisKelamin.LakiLaki),
            TotalPerempuan = d.DaftarKaryawan.Count(k => !k.IsDeleted && k.JenisKelamin == JenisKelamin.Perempuan)
        }).ToList();

        var totalKaryawan = rekap.Sum(r => r.TotalKaryawan);

        return new LaporanDistribusiDepartemenViewModel
        {
            RekapDepartemen = rekap,
            TotalSeluruhKaryawan = totalKaryawan
        };
    }

    // 2. Kehadiran, Keterlambatan, & Lembur
    public async Task<LaporanKehadiranViewModel> DapatkanLaporanKehadiranAsync(DateTime tanggalMulai, DateTime tanggalSelesai, Guid? departemenId, int halaman = 1, int ukuranHalaman = 15)
    {
        var tMulai = tanggalMulai.Date;
        var tSelesai = tanggalSelesai.Date;

        var query = _db.Kehadiran
            .AsNoTracking()
            .Include(k => k.Karyawan)
                .ThenInclude(k => k!.Departemen)
            .Where(k => !k.IsDeleted && k.Tanggal >= tMulai && k.Tanggal <= tSelesai);

        if (departemenId.HasValue && departemenId != Guid.Empty)
        {
            query = query.Where(k => k.Karyawan != null && k.Karyawan.DepartemenId == departemenId);
        }

        var totalData = await query.CountAsync();
        var totalHalaman = (int)Math.Ceiling(totalData / (double)ukuranHalaman);
        if (totalHalaman < 1) totalHalaman = 1;
        if (halaman < 1) halaman = 1;
        if (halaman > totalHalaman) halaman = totalHalaman;

        var items = await query
            .OrderByDescending(k => k.Tanggal)
            .ThenBy(k => k.Karyawan != null ? k.Karyawan.NamaLengkap : "")
            .Skip((halaman - 1) * ukuranHalaman)
            .Take(ukuranHalaman)
            .Select(k => new BarisLaporanKehadiranViewModel
            {
                Tanggal = k.Tanggal,
                NIK = k.Karyawan != null ? k.Karyawan.NomorIndukKaryawan : "-",
                NamaKaryawan = k.Karyawan != null ? k.Karyawan.NamaLengkap : "-",
                NamaDepartemen = k.Karyawan != null && k.Karyawan.Departemen != null ? k.Karyawan.Departemen.NamaDepartemen : "-",
                JamMasuk = k.WaktuMasuk.HasValue ? k.WaktuMasuk.Value.TimeOfDay : null,
                JamKeluar = k.WaktuKeluar.HasValue ? k.WaktuKeluar.Value.TimeOfDay : null,
                StatusKehadiran = k.Status.ToString(),
                MenitKeterlambatan = k.MenitKeterlambatan,
                TotalJamKerja = k.TotalJamKerja
            })
            .ToListAsync();

        var totalHadir = await query.CountAsync(k => k.Status == StatusKehadiran.Hadir);
        var totalIzin = await query.CountAsync(k => k.Status == StatusKehadiran.Izin);
        var totalSakit = await query.CountAsync(k => k.Status == StatusKehadiran.Sakit);
        var totalAlpa = await query.CountAsync(k => k.Status == StatusKehadiran.Alpa);
        var totalTerlambat = await query.CountAsync(k => k.MenitKeterlambatan > 0);

        return new LaporanKehadiranViewModel
        {
            TanggalMulai = tMulai,
            TanggalSelesai = tSelesai,
            DepartemenIdTerpilih = departemenId,
            DataKehadiran = items,
            HalamanSaatIni = halaman,
            TotalHalaman = totalHalaman,
            TotalData = totalData,
            TotalHadir = totalHadir,
            TotalIzin = totalIzin,
            TotalSakit = totalSakit,
            TotalAlpa = totalAlpa,
            TotalTerlambat = totalTerlambat
        };
    }

    public async Task<LaporanKeterlambatanViewModel> DapatkanLaporanKeterlambatanAsync(DateTime tanggalMulai, DateTime tanggalSelesai, Guid? departemenId, int halaman = 1, int ukuranHalaman = 15)
    {
        var tMulai = tanggalMulai.Date;
        var tSelesai = tanggalSelesai.Date;

        var query = _db.Kehadiran
            .AsNoTracking()
            .Include(k => k.Karyawan)
                .ThenInclude(k => k!.Departemen)
            .Include(k => k.JadwalKerja)
            .Where(k => !k.IsDeleted && k.Tanggal >= tMulai && k.Tanggal <= tSelesai && k.MenitKeterlambatan > 0);

        if (departemenId.HasValue && departemenId != Guid.Empty)
        {
            query = query.Where(k => k.Karyawan != null && k.Karyawan.DepartemenId == departemenId);
        }

        var totalData = await query.CountAsync();
        var totalHalaman = (int)Math.Ceiling(totalData / (double)ukuranHalaman);
        if (totalHalaman < 1) totalHalaman = 1;
        if (halaman < 1) halaman = 1;
        if (halaman > totalHalaman) halaman = totalHalaman;

        var items = await query
            .OrderByDescending(k => k.Tanggal)
            .ThenByDescending(k => k.MenitKeterlambatan)
            .Skip((halaman - 1) * ukuranHalaman)
            .Take(ukuranHalaman)
            .Select(k => new BarisLaporanKeterlambatanViewModel
            {
                Tanggal = k.Tanggal,
                NIK = k.Karyawan != null ? k.Karyawan.NomorIndukKaryawan : "-",
                NamaKaryawan = k.Karyawan != null ? k.Karyawan.NamaLengkap : "-",
                NamaDepartemen = k.Karyawan != null && k.Karyawan.Departemen != null ? k.Karyawan.Departemen.NamaDepartemen : "-",
                JamJadwalMasuk = k.JadwalKerja != null ? k.JadwalKerja.JamMasukJadwal : null,
                JamRealisasiMasuk = k.WaktuMasuk.HasValue ? k.WaktuMasuk.Value.TimeOfDay : null,
                MenitTerlambat = k.MenitKeterlambatan,
                EstimasiDenda = k.MenitKeterlambatan * 1000
            })
            .ToListAsync();

        var akumulasiMenit = await query.SumAsync(k => (int?)k.MenitKeterlambatan) ?? 0;

        return new LaporanKeterlambatanViewModel
        {
            TanggalMulai = tMulai,
            TanggalSelesai = tSelesai,
            DepartemenIdTerpilih = departemenId,
            DataKeterlambatan = items,
            HalamanSaatIni = halaman,
            TotalHalaman = totalHalaman,
            TotalData = totalData,
            TotalKasusKeterlambatan = totalData,
            TotalAkumulasiMenitTerlambat = akumulasiMenit
        };
    }

    public async Task<LaporanLemburViewModel> DapatkanLaporanLemburAsync(DateTime tanggalMulai, DateTime tanggalSelesai, Guid? departemenId, int halaman = 1, int ukuranHalaman = 15)
    {
        var tMulai = tanggalMulai.Date;
        var tSelesai = tanggalSelesai.Date;

        var query = _db.PengajuanLembur
            .AsNoTracking()
            .Include(l => l.Karyawan)
                .ThenInclude(k => k!.Departemen)
            .Where(l => !l.IsDeleted && l.TanggalLembur >= tMulai && l.TanggalLembur <= tSelesai);

        if (departemenId.HasValue && departemenId != Guid.Empty)
        {
            query = query.Where(l => l.Karyawan != null && l.Karyawan.DepartemenId == departemenId);
        }

        var totalData = await query.CountAsync();
        var totalHalaman = (int)Math.Ceiling(totalData / (double)ukuranHalaman);
        if (totalHalaman < 1) totalHalaman = 1;
        if (halaman < 1) halaman = 1;
        if (halaman > totalHalaman) halaman = totalHalaman;

        var items = await query
            .OrderByDescending(l => l.TanggalLembur)
            .Skip((halaman - 1) * ukuranHalaman)
            .Take(ukuranHalaman)
            .Select(l => new BarisLaporanLemburViewModel
            {
                Tanggal = l.TanggalLembur,
                NIK = l.Karyawan != null ? l.Karyawan.NomorIndukKaryawan : "-",
                NamaKaryawan = l.Karyawan != null ? l.Karyawan.NamaLengkap : "-",
                NamaDepartemen = l.Karyawan != null && l.Karyawan.Departemen != null ? l.Karyawan.Departemen.NamaDepartemen : "-",
                JamMulai = l.JamMulai,
                JamSelesai = l.JamSelesai,
                DurasiJam = l.DurasiJam,
                AlasanLembur = l.AlasanLembur,
                StatusPersetujuan = l.Status.ToString()
            })
            .ToListAsync();

        var queryDisetujui = query.Where(l => l.Status == StatusPengajuanLembur.Disetujui);
        var totalJam = await queryDisetujui.SumAsync(l => (decimal?)l.DurasiJam) ?? 0;
        var totalUang = await queryDisetujui.SumAsync(l => (decimal?)l.UangLemburDisetujui) ?? 0;

        return new LaporanLemburViewModel
        {
            TanggalMulai = tMulai,
            TanggalSelesai = tSelesai,
            DepartemenIdTerpilih = departemenId,
            DataLembur = items,
            HalamanSaatIni = halaman,
            TotalHalaman = totalHalaman,
            TotalData = totalData,
            TotalJamLemburDisetujui = totalJam,
            TotalEstimasiUangLembur = totalUang
        };
    }

    // 3. Cuti
    public async Task<LaporanCutiViewModel> DapatkanLaporanCutiAsync(int tahun, Guid? departemenId, Guid? jenisCutiId, int halaman = 1, int ukuranHalaman = 15)
    {
        var query = _db.PengajuanCuti
            .AsNoTracking()
            .Include(c => c.Karyawan)
                .ThenInclude(k => k!.Departemen)
            .Include(c => c.JenisCuti)
            .Where(c => !c.IsDeleted && c.TanggalMulai.Year == tahun);

        if (departemenId.HasValue && departemenId != Guid.Empty)
        {
            query = query.Where(c => c.Karyawan != null && c.Karyawan.DepartemenId == departemenId);
        }

        if (jenisCutiId.HasValue && jenisCutiId != Guid.Empty)
        {
            query = query.Where(c => c.JenisCutiId == jenisCutiId);
        }

        var totalData = await query.CountAsync();
        var totalHalaman = (int)Math.Ceiling(totalData / (double)ukuranHalaman);
        if (totalHalaman < 1) totalHalaman = 1;
        if (halaman < 1) halaman = 1;
        if (halaman > totalHalaman) halaman = totalHalaman;

        var items = await query
            .OrderByDescending(c => c.TanggalMulai)
            .Skip((halaman - 1) * ukuranHalaman)
            .Take(ukuranHalaman)
            .Select(c => new BarisLaporanCutiViewModel
            {
                NomorPengajuan = c.NomorPengajuan,
                NIK = c.Karyawan != null ? c.Karyawan.NomorIndukKaryawan : "-",
                NamaKaryawan = c.Karyawan != null ? c.Karyawan.NamaLengkap : "-",
                NamaDepartemen = c.Karyawan != null && c.Karyawan.Departemen != null ? c.Karyawan.Departemen.NamaDepartemen : "-",
                JenisCuti = c.JenisCuti != null ? c.JenisCuti.NamaJenisCuti : "-",
                TanggalMulai = c.TanggalMulai,
                TanggalSelesai = c.TanggalSelesai,
                DurasiHari = c.JumlahHari,
                Status = c.Status.ToString(),
                Alasan = c.Alasan
            })
            .ToListAsync();

        var queryDisetujui = query.Where(c => c.Status == StatusPengajuanCuti.Disetujui);
        var totalHariDisetujui = await queryDisetujui.SumAsync(c => (int?)c.JumlahHari) ?? 0;

        var rekapJenis = await queryDisetujui
            .GroupBy(c => c.JenisCuti != null ? c.JenisCuti.NamaJenisCuti : "Lainnya")
            .Select(g => new { Nama = g.Key, JumlahHari = g.Sum(x => x.JumlahHari) })
            .ToDictionaryAsync(x => x.Nama, x => x.JumlahHari);

        return new LaporanCutiViewModel
        {
            TahunTerpilih = tahun,
            DepartemenIdTerpilih = departemenId,
            JenisCutiIdTerpilih = jenisCutiId,
            DataCuti = items,
            HalamanSaatIni = halaman,
            TotalHalaman = totalHalaman,
            TotalData = totalData,
            TotalHariCutiDisetujui = totalHariDisetujui,
            RekapPerJenisCuti = rekapJenis
        };
    }

    // 4. Penggajian
    public async Task<LaporanPenggajianViewModel> DapatkanLaporanPenggajianAsync(int tahun, int? bulan, Guid? departemenId, int halaman = 1, int ukuranHalaman = 15)
    {
        var query = _db.Penggajian
            .AsNoTracking()
            .Include(p => p.PeriodePenggajian)
            .Include(p => p.Karyawan)
                .ThenInclude(k => k!.Departemen)
            .Include(p => p.Karyawan)
                .ThenInclude(k => k!.Jabatan)
            .Where(p => !p.IsDeleted && p.PeriodePenggajian != null && p.PeriodePenggajian.Tahun == tahun);

        if (bulan.HasValue && bulan.Value >= 1 && bulan.Value <= 12)
        {
            query = query.Where(p => p.PeriodePenggajian != null && p.PeriodePenggajian.Bulan == bulan.Value);
        }

        if (departemenId.HasValue && departemenId != Guid.Empty)
        {
            query = query.Where(p => p.Karyawan != null && p.Karyawan.DepartemenId == departemenId);
        }

        var totalData = await query.CountAsync();
        var totalHalaman = (int)Math.Ceiling(totalData / (double)ukuranHalaman);
        if (totalHalaman < 1) totalHalaman = 1;
        if (halaman < 1) halaman = 1;
        if (halaman > totalHalaman) halaman = totalHalaman;

        var items = await query
            .OrderBy(p => p.Karyawan != null ? p.Karyawan.NamaLengkap : "")
            .Skip((halaman - 1) * ukuranHalaman)
            .Take(ukuranHalaman)
            .Select(p => new BarisLaporanPenggajianViewModel
            {
                PeriodeNama = p.PeriodePenggajian != null ? p.PeriodePenggajian.NamaPeriode : "-",
                NIK = p.Karyawan != null ? p.Karyawan.NomorIndukKaryawan : "-",
                NamaKaryawan = p.Karyawan != null ? p.Karyawan.NamaLengkap : "-",
                NamaDepartemen = p.Karyawan != null && p.Karyawan.Departemen != null ? p.Karyawan.Departemen.NamaDepartemen : "-",
                NamaJabatan = p.Karyawan != null && p.Karyawan.Jabatan != null ? p.Karyawan.Jabatan.NamaJabatan : "-",
                GajiPokok = p.GajiPokok,
                TotalTunjangan = p.TotalTunjanganTetap + p.TotalTunjanganTidakTetap,
                UangLembur = p.TotalUangLembur,
                PotonganBpjsPph = p.TotalPotonganBpjs + p.TotalPotonganPph21,
                GajiBersih = p.GajiBersih,
                Status = p.Status.ToString()
            })
            .ToListAsync();

        var totalGajiPokok = await query.SumAsync(p => (decimal?)p.GajiPokok) ?? 0;
        var totalTunjangan = await query.SumAsync(p => (decimal?)(p.TotalTunjanganTetap + p.TotalTunjanganTidakTetap)) ?? 0;
        var totalLembur = await query.SumAsync(p => (decimal?)p.TotalUangLembur) ?? 0;
        var totalPotongan = await query.SumAsync(p => (decimal?)p.TotalPotongan) ?? 0;
        var totalGajiBersih = await query.SumAsync(p => (decimal?)p.GajiBersih) ?? 0;
        var totalBebanPerusahaan = await query.SumAsync(p => (decimal?)(p.TotalPendapatanKotor + p.TotalBpjsDitanggungPerusahaan)) ?? 0;

        return new LaporanPenggajianViewModel
        {
            TahunTerpilih = tahun,
            BulanTerpilih = bulan,
            DepartemenIdTerpilih = departemenId,
            DataPenggajian = items,
            HalamanSaatIni = halaman,
            TotalHalaman = totalHalaman,
            TotalData = totalData,
            TotalGajiPokok = totalGajiPokok,
            TotalTunjangan = totalTunjangan,
            TotalLembur = totalLembur,
            TotalPotongan = totalPotongan,
            TotalGajiBersih = totalGajiBersih,
            TotalBebanPerusahaan = totalBebanPerusahaan
        };
    }

    // 5. Rekrutmen
    public async Task<LaporanRekrutmenViewModel> DapatkanLaporanRekrutmenAsync(DateTime? tanggalMulai, DateTime? tanggalSelesai, Guid? lowonganId, int halaman = 1, int ukuranHalaman = 15)
    {
        var query = _db.Pelamar
            .AsNoTracking()
            .Include(p => p.LowonganKerja)
            .Where(p => !p.IsDeleted)
            .AsQueryable();

        if (tanggalMulai.HasValue)
        {
            var tMulaiUtc = DateTime.SpecifyKind(tanggalMulai.Value.Date, DateTimeKind.Utc);
            query = query.Where(p => p.CreatedAt >= tMulaiUtc);
        }

        if (tanggalSelesai.HasValue)
        {
            var tSelesaiUtc = DateTime.SpecifyKind(tanggalSelesai.Value.Date.AddDays(1).AddTicks(-1), DateTimeKind.Utc);
            query = query.Where(p => p.CreatedAt <= tSelesaiUtc);
        }

        if (lowonganId.HasValue && lowonganId != Guid.Empty)
        {
            query = query.Where(p => p.LowonganKerjaId == lowonganId);
        }

        var totalData = await query.CountAsync();
        var totalHalaman = (int)Math.Ceiling(totalData / (double)ukuranHalaman);
        if (totalHalaman < 1) totalHalaman = 1;
        if (halaman < 1) halaman = 1;
        if (halaman > totalHalaman) halaman = totalHalaman;

        var items = await query
            .OrderByDescending(p => p.CreatedAt)
            .Skip((halaman - 1) * ukuranHalaman)
            .Take(ukuranHalaman)
            .Select(p => new BarisLaporanRekrutmenViewModel
            {
                NomorRegistrasi = p.NomorRegistrasi,
                NamaPelamar = p.NamaLengkap,
                PosisiLowongan = p.LowonganKerja != null ? p.LowonganKerja.JudulLowongan : "-",
                Pendidikan = p.PendidikanTerakhir ?? "-",
                PengalamanTahun = p.PengalamanKerjaTahun,
                TanggalMelamar = p.CreatedAt.LocalDateTime,
                Status = p.Status
            })
            .ToListAsync();

        var totalPelamar = totalData;
        var totalLolosWawancara = await query.CountAsync(p => p.Status == StatusPelamar.Wawancara || p.Status == StatusPelamar.PenawaranKerja || p.Status == StatusPelamar.Diterima);
        var totalDiterima = await query.CountAsync(p => p.Status == StatusPelamar.Diterima);

        var rekapStatus = await query
            .GroupBy(p => p.Status)
            .Select(g => new { Status = g.Key.ToString(), Jumlah = g.Count() })
            .ToDictionaryAsync(x => x.Status, x => x.Jumlah);

        return new LaporanRekrutmenViewModel
        {
            TanggalMulai = tanggalMulai,
            TanggalSelesai = tanggalSelesai,
            LowonganIdTerpilih = lowonganId,
            DataRekrutmen = items,
            HalamanSaatIni = halaman,
            TotalHalaman = totalHalaman,
            TotalData = totalData,
            TotalPelamarMasuk = totalPelamar,
            TotalLolosWawancara = totalLolosWawancara,
            TotalKandidatDiterima = totalDiterima,
            RekapStatusPelamar = rekapStatus
        };
    }

    // 6. Kinerja
    public async Task<LaporanKinerjaPusatViewModel> DapatkanLaporanKinerjaAsync(Guid? periodeId, Guid? departemenId, int halaman = 1, int ukuranHalaman = 15)
    {
        var query = _db.PenilaianKinerja
            .AsNoTracking()
            .Include(p => p.PeriodePenilaian)
            .Include(p => p.Karyawan)
                .ThenInclude(k => k!.Departemen)
            .Where(p => !p.IsDeleted)
            .AsQueryable();

        if (periodeId.HasValue && periodeId != Guid.Empty)
        {
            query = query.Where(p => p.PeriodePenilaianId == periodeId);
        }

        if (departemenId.HasValue && departemenId != Guid.Empty)
        {
            query = query.Where(p => p.Karyawan != null && p.Karyawan.DepartemenId == departemenId);
        }

        var totalData = await query.CountAsync();
        var totalHalaman = (int)Math.Ceiling(totalData / (double)ukuranHalaman);
        if (totalHalaman < 1) totalHalaman = 1;
        if (halaman < 1) halaman = 1;
        if (halaman > totalHalaman) halaman = totalHalaman;

        var items = await query
            .OrderByDescending(p => p.TotalSkorAkhir)
            .Skip((halaman - 1) * ukuranHalaman)
            .Take(ukuranHalaman)
            .Select(p => new BarisLaporanKinerjaViewModel
            {
                NamaPeriode = p.PeriodePenilaian != null ? p.PeriodePenilaian.NamaPeriode : "-",
                NIK = p.Karyawan != null ? p.Karyawan.NomorIndukKaryawan : "-",
                NamaKaryawan = p.Karyawan != null ? p.Karyawan.NamaLengkap : "-",
                NamaDepartemen = p.Karyawan != null && p.Karyawan.Departemen != null ? p.Karyawan.Departemen.NamaDepartemen : "-",
                NamaPenilai = p.NamaPenilai,
                TanggalPenilaian = p.TanggalPenilaian,
                TotalSkorAkhir = p.TotalSkorAkhir,
                PredikatKinerja = p.PredikatKinerja
            })
            .ToListAsync();

        var rataRata = await query.AverageAsync(p => (decimal?)p.TotalSkorAkhir) ?? 0;

        var predikatDist = await query
            .GroupBy(p => p.PredikatKinerja)
            .Select(g => new { Predikat = g.Key.ToString(), Jumlah = g.Count() })
            .ToDictionaryAsync(x => x.Predikat, x => x.Jumlah);

        var deptAvg = await query
            .Where(p => p.Karyawan != null && p.Karyawan.Departemen != null)
            .GroupBy(p => p.Karyawan!.Departemen!.NamaDepartemen)
            .Select(g => new { NamaDept = g.Key, AvgSkor = Math.Round(g.Average(x => x.TotalSkorAkhir), 2) })
            .ToDictionaryAsync(x => x.NamaDept, x => x.AvgSkor);

        return new LaporanKinerjaPusatViewModel
        {
            PeriodeIdTerpilih = periodeId,
            DepartemenIdTerpilih = departemenId,
            DataKinerja = items,
            HalamanSaatIni = halaman,
            TotalHalaman = totalHalaman,
            TotalData = totalData,
            RataRataSkorPerusahaan = Math.Round(rataRata, 2),
            DistribusiPredikat = predikatDist,
            RataRataSkorPerDepartemen = deptAvg
        };
    }

    // 7. Pelatihan
    public async Task<LaporanPelatihanPusatViewModel> DapatkanLaporanPelatihanAsync(DateTime? tanggalMulai, DateTime? tanggalSelesai, Guid? departemenId, int halaman = 1, int ukuranHalaman = 15)
    {
        var query = _db.ProgramPelatihan
            .AsNoTracking()
            .Include(p => p.DaftarPeserta)
            .Where(p => !p.IsDeleted)
            .AsQueryable();

        if (tanggalMulai.HasValue)
        {
            var tMulai = tanggalMulai.Value.Date;
            query = query.Where(p => p.TanggalMulai >= tMulai);
        }

        if (tanggalSelesai.HasValue)
        {
            var tSelesai = tanggalSelesai.Value.Date;
            query = query.Where(p => p.TanggalSelesai <= tSelesai);
        }

        var totalData = await query.CountAsync();
        var totalHalaman = (int)Math.Ceiling(totalData / (double)ukuranHalaman);
        if (totalHalaman < 1) totalHalaman = 1;
        if (halaman < 1) halaman = 1;
        if (halaman > totalHalaman) halaman = totalHalaman;

        var items = await query
            .OrderByDescending(p => p.TanggalMulai)
            .Skip((halaman - 1) * ukuranHalaman)
            .Take(ukuranHalaman)
            .Select(p => new BarisLaporanPelatihanViewModel
            {
                KodeProgram = p.KodeProgram,
                NamaProgram = p.NamaProgram,
                Penyelenggara = p.Penyelenggara,
                Metode = p.Metode,
                TanggalMulai = p.TanggalMulai,
                TanggalSelesai = p.TanggalSelesai,
                JumlahPeserta = p.DaftarPeserta.Count(ps => !ps.IsDeleted),
                JumlahLulus = p.DaftarPeserta.Count(ps => !ps.IsDeleted && ps.StatusKeikutsertaan == StatusKeikutsertaan.Lulus),
                Status = p.Status
            })
            .ToListAsync();

        var totalProgram = totalData;
        var totalPeserta = await _db.PesertaPelatihan.AsNoTracking().CountAsync(p => !p.IsDeleted);
        var totalPesertaLulus = await _db.PesertaPelatihan.AsNoTracking().CountAsync(p => !p.IsDeleted && p.StatusKeikutsertaan == StatusKeikutsertaan.Lulus);
        var totalBiaya = await query.SumAsync(p => (decimal?)p.BiayaPerPeserta * p.DaftarPeserta.Count(ps => !ps.IsDeleted)) ?? 0;

        return new LaporanPelatihanPusatViewModel
        {
            TanggalMulai = tanggalMulai,
            TanggalSelesai = tanggalSelesai,
            DepartemenIdTerpilih = departemenId,
            DataPelatihan = items,
            HalamanSaatIni = halaman,
            TotalHalaman = totalHalaman,
            TotalData = totalData,
            TotalProgram = totalProgram,
            TotalPeserta = totalPeserta,
            TotalPesertaLulus = totalPesertaLulus,
            TotalBiayaPelatihan = totalBiaya
        };
    }

    // 8. Aset
    public async Task<LaporanAsetPusatViewModel> DapatkanLaporanAsetAsync(KategoriAset? kategori, StatusAset? status, KondisiAset? kondisi, int halaman = 1, int ukuranHalaman = 15)
    {
        var query = _db.AsetPerusahaan
            .AsNoTracking()
            .Include(a => a.PemegangSaatIni)
                .ThenInclude(k => k!.Departemen)
            .Where(a => !a.IsDeleted)
            .AsQueryable();

        if (kategori.HasValue)
        {
            query = query.Where(a => a.Kategori == kategori.Value);
        }

        if (status.HasValue)
        {
            query = query.Where(a => a.Status == status.Value);
        }

        if (kondisi.HasValue)
        {
            query = query.Where(a => a.Kondisi == kondisi.Value);
        }

        var totalData = await query.CountAsync();
        var totalHalaman = (int)Math.Ceiling(totalData / (double)ukuranHalaman);
        if (totalHalaman < 1) totalHalaman = 1;
        if (halaman < 1) halaman = 1;
        if (halaman > totalHalaman) halaman = totalHalaman;

        var items = await query
            .OrderBy(a => a.NomorAset)
            .Skip((halaman - 1) * ukuranHalaman)
            .Take(ukuranHalaman)
            .Select(a => new BarisLaporanAsetViewModel
            {
                NomorAset = a.NomorAset,
                NamaAset = a.NamaAset,
                Kategori = a.Kategori,
                MerkModel = a.MerkModel,
                TanggalBeli = a.TanggalPembelian,
                NilaiPerolehan = a.NilaiPerolehan,
                Kondisi = a.Kondisi,
                Status = a.Status,
                NamaPemegang = a.PemegangSaatIni != null ? a.PemegangSaatIni.NamaLengkap : "-",
                DepartemenPemegang = a.PemegangSaatIni != null && a.PemegangSaatIni.Departemen != null ? a.PemegangSaatIni.Departemen.NamaDepartemen : "-"
            })
            .ToListAsync();

        var totalNilai = await query.SumAsync(a => (decimal?)a.NilaiPerolehan) ?? 0;

        var rekapKondisi = await query
            .GroupBy(a => a.Kondisi)
            .Select(g => new { Kondisi = g.Key.ToString(), Jumlah = g.Count() })
            .ToDictionaryAsync(x => x.Kondisi, x => x.Jumlah);

        var rekapKategori = await query
            .GroupBy(a => a.Kategori)
            .Select(g => new { Kategori = g.Key.ToString(), Jumlah = g.Count() })
            .ToDictionaryAsync(x => x.Kategori, x => x.Jumlah);

        return new LaporanAsetPusatViewModel
        {
            KategoriTerpilih = kategori,
            StatusTerpilih = status,
            KondisiTerpilih = kondisi,
            DataAset = items,
            HalamanSaatIni = halaman,
            TotalHalaman = totalHalaman,
            TotalData = totalData,
            TotalUnitAset = totalData,
            TotalNilaiInventaris = totalNilai,
            RekapKondisi = rekapKondisi,
            RekapKategori = rekapKategori
        };
    }

    // Helper Ekspor CSV
    public byte[] EksporKaryawanCsv(List<BarisLaporanKaryawanViewModel> data)
    {
        var sb = new StringBuilder();
        sb.AppendLine("NIK,Nama Lengkap,Email,Nomor Telepon,Departemen,Jabatan,Jenis Kelamin,Tanggal Masuk,Status");

        foreach (var item in data)
        {
            sb.AppendLine($"\"{EscapeCsv(item.NIK)}\",\"{EscapeCsv(item.NamaLengkap)}\",\"{EscapeCsv(item.Email)}\",\"{EscapeCsv(item.NomorTelepon)}\",\"{EscapeCsv(item.NamaDepartemen)}\",\"{EscapeCsv(item.NamaJabatan)}\",\"{EscapeCsv(item.JenisKelamin)}\",\"{item.TanggalMasuk:yyyy-MM-dd}\",\"{item.Status}\"");
        }

        return TambahkanBomUtf8(sb.ToString());
    }

    public byte[] EksporKehadiranCsv(List<BarisLaporanKehadiranViewModel> data)
    {
        var sb = new StringBuilder();
        sb.AppendLine("Tanggal,NIK,Nama Karyawan,Departemen,Jam Masuk,Jam Keluar,Status,Keterlambatan (Menit),Total Jam Kerja");

        foreach (var item in data)
        {
            var jamMasuk = item.JamMasuk.HasValue ? item.JamMasuk.Value.ToString(@"hh\:mm") : "-";
            var jamKeluar = item.JamKeluar.HasValue ? item.JamKeluar.Value.ToString(@"hh\:mm") : "-";
            sb.AppendLine($"\"{item.Tanggal:yyyy-MM-dd}\",\"{EscapeCsv(item.NIK)}\",\"{EscapeCsv(item.NamaKaryawan)}\",\"{EscapeCsv(item.NamaDepartemen)}\",\"{jamMasuk}\",\"{jamKeluar}\",\"{item.StatusKehadiran}\",{item.MenitKeterlambatan},{item.TotalJamKerja.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture)}");
        }

        return TambahkanBomUtf8(sb.ToString());
    }

    public byte[] EksporPenggajianCsv(List<BarisLaporanPenggajianViewModel> data)
    {
        var sb = new StringBuilder();
        sb.AppendLine("Periode,NIK,Nama Karyawan,Departemen,Jabatan,Gaji Pokok,Total Tunjangan,Uang Lembur,Potongan BPJS & PPh,Gaji Bersih,Status");

        foreach (var item in data)
        {
            sb.AppendLine($"\"{EscapeCsv(item.PeriodeNama)}\",\"{EscapeCsv(item.NIK)}\",\"{EscapeCsv(item.NamaKaryawan)}\",\"{EscapeCsv(item.NamaDepartemen)}\",\"{EscapeCsv(item.NamaJabatan)}\",{item.GajiPokok.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture)},{item.TotalTunjangan.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture)},{item.UangLembur.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture)},{item.PotonganBpjsPph.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture)},{item.GajiBersih.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture)},\"{item.Status}\"");
        }

        return TambahkanBomUtf8(sb.ToString());
    }

    public byte[] EksporAsetCsv(List<BarisLaporanAsetViewModel> data)
    {
        var sb = new StringBuilder();
        sb.AppendLine("Nomor Aset,Nama Aset,Kategori,Merk / Model,Tanggal Beli,Nilai Perolehan,Kondisi,Status,Pemegang Saat Ini,Departemen");

        foreach (var item in data)
        {
            sb.AppendLine($"\"{EscapeCsv(item.NomorAset)}\",\"{EscapeCsv(item.NamaAset)}\",\"{item.Kategori}\",\"{EscapeCsv(item.MerkModel ?? "-")}\",\"{item.TanggalBeli:yyyy-MM-dd}\",{item.NilaiPerolehan.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture)},\"{item.Kondisi}\",\"{item.Status}\",\"{EscapeCsv(item.NamaPemegang ?? "-")}\",\"{EscapeCsv(item.DepartemenPemegang ?? "-")}\"");
        }

        return TambahkanBomUtf8(sb.ToString());
    }

    private static string EscapeCsv(string? val)
    {
        if (string.IsNullOrEmpty(val)) return string.Empty;
        return val.Replace("\"", "\"\"");
    }

    private static byte[] TambahkanBomUtf8(string text)
    {
        var bom = Encoding.UTF8.GetPreamble();
        var bytes = Encoding.UTF8.GetBytes(text);
        var hasil = new byte[bom.Length + bytes.Length];
        Buffer.BlockCopy(bom, 0, hasil, 0, bom.Length);
        Buffer.BlockCopy(bytes, 0, hasil, bom.Length, bytes.Length);
        return hasil;
    }
}
