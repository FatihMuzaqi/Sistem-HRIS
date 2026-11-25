using Microsoft.EntityFrameworkCore;
using SistemHRD.Data;
using SistemHRD.Models.Entitas;
using SistemHRD.Models.Enums;
using SistemHRD.Services.Interfaces;
using SistemHRD.ViewModels;

namespace SistemHRD.Services.Implementasi;

public class LayananPenilaianKinerja : ILayananPenilaianKinerja
{
    private readonly KonteksAplikasi _context;
    private readonly ILayananAudit _layananAudit;
    private readonly ILayananTenant _layananTenant;

    public LayananPenilaianKinerja(
        KonteksAplikasi context,
        ILayananAudit layananAudit,
        ILayananTenant layananTenant)
    {
        _context = context;
        _layananAudit = layananAudit;
        _layananTenant = layananTenant;
    }

    #region Dashboard Kinerja
    public async Task<DashboardKinerjaViewModel> DapatkanDashboardAsync()
    {
        var periodeAktifList = await _context.PeriodePenilaian
            .Where(p => p.Status == StatusPeriodePenilaian.Aktif)
            .OrderByDescending(p => p.Tahun)
            .Select(p => new PeriodePenilaianItemViewModel
            {
                Id = p.Id,
                NamaPeriode = p.NamaPeriode,
                Tahun = p.Tahun,
                SemesterAtauKuartal = p.SemesterAtauKuartal,
                TanggalMulai = p.TanggalMulai,
                TanggalSelesai = p.TanggalSelesai,
                Status = p.Status,
                Catatan = p.Catatan,
                TotalKaryawanDinilai = p.DaftarPenilaianKinerja.Count,
                RataRataSkor = p.DaftarPenilaianKinerja.Any() ? p.DaftarPenilaianKinerja.Average(pk => pk.TotalSkorAkhir) : 0
            })
            .ToListAsync();

        var semuaPenilaian = await _context.PenilaianKinerja
            .Include(p => p.Karyawan!)
                .ThenInclude(k => k.Departemen)
            .Include(p => p.Karyawan!)
                .ThenInclude(k => k.Jabatan)
            .Include(p => p.PeriodePenilaian)
            .ToListAsync();

        var totalDinilai = semuaPenilaian.Count;
        var rataSkor = totalDinilai > 0 ? semuaPenilaian.Average(p => p.TotalSkorAkhir) : 0;
        var totalIndikator = await _context.IndikatorKPI.CountAsync(i => i.StatusAktif);

        // Distribusi Predikat Kinerja (A, B, C, D, E) untuk Chart.js
        var distribusi = new Dictionary<string, int>
        {
            ["A (Istimewa)"] = semuaPenilaian.Count(p => p.PredikatKinerja == KategoriKinerja.Istimewa),
            ["B (Baik)"] = semuaPenilaian.Count(p => p.PredikatKinerja == KategoriKinerja.Baik),
            ["C (Cukup)"] = semuaPenilaian.Count(p => p.PredikatKinerja == KategoriKinerja.Cukup),
            ["D (Kurang)"] = semuaPenilaian.Count(p => p.PredikatKinerja == KategoriKinerja.Kurang),
            ["E (Sangat Kurang)"] = semuaPenilaian.Count(p => p.PredikatKinerja == KategoriKinerja.SangatKurang)
        };

        // Rata-rata Skor per Departemen untuk Chart.js (Bar)
        var skorPerDept = semuaPenilaian
            .GroupBy(p => p.Karyawan?.Departemen?.NamaDepartemen ?? "Umum")
            .ToDictionary(
                g => g.Key,
                g => Math.Round(g.Average(p => p.TotalSkorAkhir), 2));

        // Penilaian Terbaru
        var penilaianTerbaru = semuaPenilaian
            .OrderByDescending(p => p.TanggalPenilaian)
            .Take(5)
            .Select(p => new PenilaianKinerjaItemViewModel
            {
                Id = p.Id,
                PeriodePenilaianId = p.PeriodePenilaianId,
                NamaPeriode = p.PeriodePenilaian?.NamaPeriode ?? "-",
                KaryawanId = p.KaryawanId,
                NIK = p.Karyawan?.NomorIndukKaryawan ?? "-",
                NamaKaryawan = p.Karyawan?.NamaLengkap ?? "-",
                NamaDepartemen = p.Karyawan?.Departemen?.NamaDepartemen ?? "-",
                NamaJabatan = p.Karyawan?.Jabatan?.NamaJabatan ?? "-",
                NamaPenilai = p.NamaPenilai,
                TanggalPenilaian = p.TanggalPenilaian,
                TotalSkorAkhir = p.TotalSkorAkhir,
                PredikatKinerja = p.PredikatKinerja,
                Status = p.Status
            })
            .ToList();

        return new DashboardKinerjaViewModel
        {
            TotalPeriodeAktif = periodeAktifList.Count,
            TotalKaryawanTelahDinilai = totalDinilai,
            RataRataSkorPerusahaan = Math.Round(rataSkor, 2),
            TotalIndikatorKPIAktif = totalIndikator,
            DistribusiPredikatKinerja = distribusi,
            RataRataSkorPerDepartemen = skorPerDept,
            PeriodeAktif = periodeAktifList,
            PenilaianTerbaru = penilaianTerbaru
        };
    }
    #endregion

    #region Periode Penilaian
    public async Task<List<PeriodePenilaianItemViewModel>> DapatkanSemuaPeriodeAsync()
    {
        return await _context.PeriodePenilaian
            .Include(p => p.DaftarPenilaianKinerja)
            .OrderByDescending(p => p.Tahun)
            .ThenByDescending(p => p.TanggalMulai)
            .Select(p => new PeriodePenilaianItemViewModel
            {
                Id = p.Id,
                NamaPeriode = p.NamaPeriode,
                Tahun = p.Tahun,
                SemesterAtauKuartal = p.SemesterAtauKuartal,
                TanggalMulai = p.TanggalMulai,
                TanggalSelesai = p.TanggalSelesai,
                Status = p.Status,
                Catatan = p.Catatan,
                TotalKaryawanDinilai = p.DaftarPenilaianKinerja.Count,
                RataRataSkor = p.DaftarPenilaianKinerja.Any() ? Math.Round(p.DaftarPenilaianKinerja.Average(pk => pk.TotalSkorAkhir), 2) : 0
            })
            .ToListAsync();
    }

    public async Task<PeriodePenilaianItemViewModel?> DapatkanPeriodeByIdAsync(Guid id)
    {
        var p = await _context.PeriodePenilaian
            .Include(x => x.DaftarPenilaianKinerja)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (p == null) return null;

        return new PeriodePenilaianItemViewModel
        {
            Id = p.Id,
            NamaPeriode = p.NamaPeriode,
            Tahun = p.Tahun,
            SemesterAtauKuartal = p.SemesterAtauKuartal,
            TanggalMulai = p.TanggalMulai,
            TanggalSelesai = p.TanggalSelesai,
            Status = p.Status,
            Catatan = p.Catatan,
            TotalKaryawanDinilai = p.DaftarPenilaianKinerja.Count,
            RataRataSkor = p.DaftarPenilaianKinerja.Any() ? Math.Round(p.DaftarPenilaianKinerja.Average(pk => pk.TotalSkorAkhir), 2) : 0
        };
    }

    public async Task<(bool Sukses, string Pesan, Guid? Id)> SimpanPeriodeAsync(FormPeriodePenilaianViewModel model)
    {
        var tenantId = _layananTenant.PerusahaanIdSaatIni ?? Guid.Empty;

        var duplikat = await _context.PeriodePenilaian
            .AnyAsync(p => p.NamaPeriode == model.NamaPeriode && p.Tahun == model.Tahun && (!model.Id.HasValue || p.Id != model.Id.Value));

        if (duplikat)
        {
            return (false, $"Periode penilaian '{model.NamaPeriode}' untuk tahun {model.Tahun} sudah ada.", null);
        }

        PeriodePenilaian entity;
        bool isNew = !model.Id.HasValue;

        if (isNew)
        {
            entity = new PeriodePenilaian
            {
                Id = Guid.NewGuid(),
                PerusahaanId = tenantId
            };
            await _context.PeriodePenilaian.AddAsync(entity);
        }
        else
        {
            var existing = await _context.PeriodePenilaian.FindAsync(model.Id!.Value);
            if (existing == null) return (false, "Data periode penilaian tidak ditemukan.", null);
            entity = existing;
        }

        entity.NamaPeriode = model.NamaPeriode.Trim();
        entity.Tahun = model.Tahun;
        entity.SemesterAtauKuartal = model.SemesterAtauKuartal?.Trim();
        entity.TanggalMulai = model.TanggalMulai;
        entity.TanggalSelesai = model.TanggalSelesai;
        entity.Status = model.Status;
        entity.Catatan = model.Catatan?.Trim();

        await _context.SaveChangesAsync();

        await _layananAudit.CatatAsync(
            isNew ? "BUAT_PERIODE_PENILAIAN" : "UBAH_PERIODE_PENILAIAN",
            "PeriodePenilaian",
            $"{(isNew ? "Membuat" : "Memperbarui")} periode penilaian kinerja: {entity.NamaPeriode} ({entity.Tahun})");

        return (true, $"Periode penilaian '{entity.NamaPeriode}' berhasil disimpan.", entity.Id);
    }

    public async Task<(bool Sukses, string Pesan)> HapusPeriodeAsync(Guid id)
    {
        var periode = await _context.PeriodePenilaian
            .Include(p => p.DaftarPenilaianKinerja)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (periode == null) return (false, "Data periode penilaian tidak ditemukan.");

        if (periode.DaftarPenilaianKinerja.Any())
        {
            return (false, "Periode penilaian tidak dapat dihapus karena sudah memiliki data penilaian karyawan yang tercatat.");
        }

        _context.PeriodePenilaian.Remove(periode);
        await _context.SaveChangesAsync();

        await _layananAudit.CatatAsync("HAPUS_PERIODE_PENILAIAN", "PeriodePenilaian", $"Menghapus periode penilaian {periode.NamaPeriode}");
        return (true, "Periode penilaian berhasil dihapus.");
    }
    #endregion

    #region Indikator KPI
    public async Task<List<IndikatorKPIItemViewModel>> DapatkanSemuaIndikatorAsync(Guid? departemenId = null)
    {
        var query = _context.IndikatorKPI
            .Include(i => i.Departemen)
            .Include(i => i.Jabatan)
            .AsQueryable();

        if (departemenId.HasValue)
        {
            query = query.Where(i => i.DepartemenId == departemenId.Value || i.DepartemenId == null);
        }

        return await query
            .OrderBy(i => i.DepartemenId.HasValue ? 1 : 0)
            .ThenBy(i => i.KodeIndikator)
            .Select(i => new IndikatorKPIItemViewModel
            {
                Id = i.Id,
                DepartemenId = i.DepartemenId,
                NamaDepartemen = i.Departemen != null ? i.Departemen.NamaDepartemen : "Semua Departemen (Umum)",
                JabatanId = i.JabatanId,
                NamaJabatan = i.Jabatan != null ? i.Jabatan.NamaJabatan : "Semua Jabatan",
                KodeIndikator = i.KodeIndikator,
                NamaIndikator = i.NamaIndikator,
                Deskripsi = i.Deskripsi,
                Jenis = i.Jenis,
                TargetKuantitatif = i.TargetKuantitatif,
                Satuan = i.Satuan,
                BobotBawaan = i.BobotBawaan,
                StatusAktif = i.StatusAktif
            })
            .ToListAsync();
    }

    public async Task<(bool Sukses, string Pesan, Guid? Id)> SimpanIndikatorAsync(FormIndikatorKPIViewModel model)
    {
        var tenantId = _layananTenant.PerusahaanIdSaatIni ?? Guid.Empty;

        var duplikat = await _context.IndikatorKPI
            .AnyAsync(i => i.KodeIndikator == model.KodeIndikator && (!model.Id.HasValue || i.Id != model.Id.Value));

        if (duplikat)
        {
            return (false, $"Kode indikator KPI '{model.KodeIndikator}' sudah digunakan.", null);
        }

        IndikatorKPI entity;
        bool isNew = !model.Id.HasValue;

        if (isNew)
        {
            entity = new IndikatorKPI
            {
                Id = Guid.NewGuid(),
                PerusahaanId = tenantId
            };
            await _context.IndikatorKPI.AddAsync(entity);
        }
        else
        {
            var existing = await _context.IndikatorKPI.FindAsync(model.Id!.Value);
            if (existing == null) return (false, "Data indikator KPI tidak ditemukan.", null);
            entity = existing;
        }

        entity.DepartemenId = model.DepartemenId;
        entity.JabatanId = model.JabatanId;
        entity.KodeIndikator = model.KodeIndikator.Trim().ToUpper();
        entity.NamaIndikator = model.NamaIndikator.Trim();
        entity.Deskripsi = model.Deskripsi?.Trim();
        entity.Jenis = model.Jenis;
        entity.TargetKuantitatif = model.TargetKuantitatif;
        entity.Satuan = model.Satuan?.Trim();
        entity.BobotBawaan = model.BobotBawaan;
        entity.StatusAktif = model.StatusAktif;

        await _context.SaveChangesAsync();

        await _layananAudit.CatatAsync(
            isNew ? "BUAT_INDIKATOR_KPI" : "UBAH_INDIKATOR_KPI",
            "IndikatorKPI",
            $"{(isNew ? "Membuat" : "Memperbarui")} indikator KPI: {entity.KodeIndikator} - {entity.NamaIndikator}");

        return (true, $"Indikator KPI '{entity.NamaIndikator}' berhasil disimpan.", entity.Id);
    }

    public async Task<(bool Sukses, string Pesan)> HapusIndikatorAsync(Guid id)
    {
        var indikator = await _context.IndikatorKPI.FindAsync(id);
        if (indikator == null) return (false, "Data indikator KPI tidak ditemukan.");

        var dipakaiDiPenilaian = await _context.DetailPenilaian.AnyAsync(d => d.IndikatorKPIId == id);
        if (dipakaiDiPenilaian)
        {
            return (false, "Indikator KPI tidak dapat dihapus karena telah digunakan dalam riwayat evaluasi penilaian kinerja.");
        }

        _context.IndikatorKPI.Remove(indikator);
        await _context.SaveChangesAsync();

        await _layananAudit.CatatAsync("HAPUS_INDIKATOR_KPI", "IndikatorKPI", $"Menghapus indikator KPI {indikator.KodeIndikator}");
        return (true, "Indikator KPI berhasil dihapus.");
    }
    #endregion

    #region Penilaian Kinerja
    public async Task<List<PenilaianKinerjaItemViewModel>> DapatkanSemuaPenilaianAsync(Guid? periodeId = null, Guid? karyawanId = null)
    {
        var query = _context.PenilaianKinerja
            .Include(p => p.PeriodePenilaian)
            .Include(p => p.Karyawan!)
                .ThenInclude(k => k.Departemen)
            .Include(p => p.Karyawan!)
                .ThenInclude(k => k.Jabatan)
            .AsQueryable();

        if (periodeId.HasValue) query = query.Where(p => p.PeriodePenilaianId == periodeId.Value);
        if (karyawanId.HasValue) query = query.Where(p => p.KaryawanId == karyawanId.Value);

        return await query
            .OrderByDescending(p => p.TanggalPenilaian)
            .Select(p => new PenilaianKinerjaItemViewModel
            {
                Id = p.Id,
                PeriodePenilaianId = p.PeriodePenilaianId,
                NamaPeriode = p.PeriodePenilaian != null ? p.PeriodePenilaian.NamaPeriode : "-",
                KaryawanId = p.KaryawanId,
                NIK = p.Karyawan != null ? p.Karyawan.NomorIndukKaryawan : "-",
                NamaKaryawan = p.Karyawan != null ? p.Karyawan.NamaLengkap : "-",
                NamaDepartemen = p.Karyawan != null && p.Karyawan.Departemen != null ? p.Karyawan.Departemen.NamaDepartemen : "-",
                NamaJabatan = p.Karyawan != null && p.Karyawan.Jabatan != null ? p.Karyawan.Jabatan.NamaJabatan : "-",
                NamaPenilai = p.NamaPenilai,
                TanggalPenilaian = p.TanggalPenilaian,
                TotalSkorAkhir = p.TotalSkorAkhir,
                PredikatKinerja = p.PredikatKinerja,
                Status = p.Status
            })
            .ToListAsync();
    }

    public async Task<FormInputPenilaianKinerjaViewModel?> SiapkanFormPenilaianAsync(Guid periodeId, Guid karyawanId)
    {
        var periode = await _context.PeriodePenilaian.FindAsync(periodeId);
        if (periode == null) return null;

        var karyawan = await _context.Karyawan
            .Include(k => k.Departemen)
            .Include(k => k.Jabatan)
            .FirstOrDefaultAsync(k => k.Id == karyawanId);

        if (karyawan == null) return null;

        // Ambil indikator KPI aktif yang relevan (umum atau khusus departemen karyawan)
        var indikatorList = await _context.IndikatorKPI
            .Where(i => i.StatusAktif && (i.DepartemenId == null || i.DepartemenId == karyawan.DepartemenId))
            .OrderBy(i => i.KodeIndikator)
            .ToListAsync();

        // Cek apakah sudah pernah dinilai sebelumnya di periode ini
        var existingPenilaian = await _context.PenilaianKinerja
            .Include(p => p.DaftarDetailPenilaian)
            .FirstOrDefaultAsync(p => p.PeriodePenilaianId == periodeId && p.KaryawanId == karyawanId);

        var form = new FormInputPenilaianKinerjaViewModel
        {
            Id = existingPenilaian?.Id,
            PeriodePenilaianId = periodeId,
            NamaPeriode = periode.NamaPeriode,
            KaryawanId = karyawanId,
            NamaKaryawan = karyawan.NamaLengkap,
            NIK = karyawan.NomorIndukKaryawan,
            NamaDepartemen = karyawan.Departemen?.NamaDepartemen ?? "-",
            NamaJabatan = karyawan.Jabatan?.NamaJabatan ?? "-",
            TanggalPenilaian = existingPenilaian?.TanggalPenilaian ?? DateTime.Today,
            KomentarEvaluasi = existingPenilaian?.KomentarEvaluasi ?? string.Empty,
            RekomendasiPengembangan = existingPenilaian?.RekomendasiPengembangan,
            Status = existingPenilaian?.Status ?? StatusPenilaianKinerja.Disetujui
        };

        foreach (var ind in indikatorList)
        {
            var detailMatch = existingPenilaian?.DaftarDetailPenilaian.FirstOrDefault(d => d.IndikatorKPIId == ind.Id);

            form.DaftarDetail.Add(new ItemInputDetailPenilaianViewModel
            {
                IndikatorKPIId = ind.Id,
                NamaIndikator = $"{ind.KodeIndikator} - {ind.NamaIndikator}",
                Target = ind.TargetKuantitatif.HasValue ? $"{ind.TargetKuantitatif} {ind.Satuan}" : "-",
                Satuan = ind.Satuan,
                BobotPersen = detailMatch?.BobotPersen ?? ind.BobotBawaan,
                Realisasi = detailMatch?.Realisasi,
                SkorIndikator = detailMatch?.SkorIndikator ?? 80,
                CatatanIndikator = detailMatch?.CatatanIndikator
            });
        }

        return form;
    }

    public async Task<DetailPenilaianKinerjaViewModel?> DapatkanDetailPenilaianAsync(Guid id)
    {
        var p = await _context.PenilaianKinerja
            .Include(x => x.PeriodePenilaian)
            .Include(x => x.Karyawan!)
                .ThenInclude(k => k.Departemen)
            .Include(x => x.Karyawan!)
                .ThenInclude(k => k.Jabatan)
            .Include(x => x.DaftarDetailPenilaian!)
                .ThenInclude(d => d.IndikatorKPI)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (p == null) return null;

        return new DetailPenilaianKinerjaViewModel
        {
            Id = p.Id,
            PeriodePenilaianId = p.PeriodePenilaianId,
            NamaPeriode = p.PeriodePenilaian?.NamaPeriode ?? "-",
            TahunPeriode = p.PeriodePenilaian?.Tahun ?? DateTime.Today.Year,
            KaryawanId = p.KaryawanId,
            NIK = p.Karyawan?.NomorIndukKaryawan ?? "-",
            NamaKaryawan = p.Karyawan?.NamaLengkap ?? "-",
            NamaDepartemen = p.Karyawan?.Departemen?.NamaDepartemen ?? "-",
            NamaJabatan = p.Karyawan?.Jabatan?.NamaJabatan ?? "-",
            NamaPenilai = p.NamaPenilai,
            TanggalPenilaian = p.TanggalPenilaian,
            TotalSkorAkhir = p.TotalSkorAkhir,
            PredikatKinerja = p.PredikatKinerja,
            KomentarEvaluasi = p.KomentarEvaluasi,
            RekomendasiPengembangan = p.RekomendasiPengembangan,
            Status = p.Status,
            DaftarDetail = p.DaftarDetailPenilaian
                .OrderBy(d => d.NamaIndikatorSnapshot)
                .Select(d => new ItemDetailPenilaianTampilViewModel
                {
                    NamaIndikator = d.NamaIndikatorSnapshot,
                    Target = d.TargetSnapshot,
                    Realisasi = d.Realisasi,
                    BobotPersen = d.BobotPersen,
                    SkorIndikator = d.SkorIndikator,
                    SkorTertimbang = d.SkorTertimbang,
                    CatatanIndikator = d.CatatanIndikator
                })
                .ToList()
        };
    }

    public async Task<(bool Sukses, string Pesan, Guid? Id)> SimpanPenilaianAsync(
        FormInputPenilaianKinerjaViewModel model,
        Guid penilaiId,
        string namaPenilai)
    {
        var tenantId = _layananTenant.PerusahaanIdSaatIni ?? Guid.Empty;

        // Validasi: "Jangan membuat skor kinerja yang tidak memiliki dasar indikator penilaian"
        if (model.DaftarDetail == null || !model.DaftarDetail.Any())
        {
            return (false, "Penilaian kinerja wajib memiliki minimal satu indikator KPI yang dinilai.", null);
        }

        var karyawan = await _context.Karyawan.FindAsync(model.KaryawanId);
        if (karyawan == null) return (false, "Data karyawan tidak ditemukan.", null);

        var periode = await _context.PeriodePenilaian.FindAsync(model.PeriodePenilaianId);
        if (periode == null) return (false, "Data periode penilaian tidak ditemukan.", null);

        // Ambil data indikator dari DB untuk snapshot nama dan validitas
        var indikatorIds = model.DaftarDetail.Select(d => d.IndikatorKPIId).Distinct().ToList();
        var indikatorDb = await _context.IndikatorKPI
            .Where(i => indikatorIds.Contains(i.Id))
            .ToDictionaryAsync(i => i.Id);

        if (indikatorDb.Count != indikatorIds.Count)
        {
            return (false, "Terdapat indikator KPI yang tidak valid atau tidak terdaftar pada perusahaan Anda.", null);
        }

        // Kalkulasi Skor Berbobot Objektif
        decimal totalSkor = 0;
        decimal totalBobot = model.DaftarDetail.Sum(d => d.BobotPersen);

        var detailEntities = new List<DetailPenilaian>();

        foreach (var item in model.DaftarDetail)
        {
            var ind = indikatorDb[item.IndikatorKPIId];

            // Skor tertimbang = (Bobot / TotalBobot) * SkorIndikator
            // Atau standard (Bobot% / 100) * Skor
            var skorTertimbang = totalBobot > 0
                ? Math.Round((item.BobotPersen / totalBobot) * item.SkorIndikator, 2)
                : 0;

            totalSkor += skorTertimbang;

            detailEntities.Add(new DetailPenilaian
            {
                Id = Guid.NewGuid(),
                PerusahaanId = tenantId,
                IndikatorKPIId = item.IndikatorKPIId,
                NamaIndikatorSnapshot = $"{ind.KodeIndikator} - {ind.NamaIndikator}",
                TargetSnapshot = ind.TargetKuantitatif.HasValue ? $"{ind.TargetKuantitatif} {ind.Satuan}" : "-",
                Realisasi = item.Realisasi?.Trim(),
                BobotPersen = item.BobotPersen,
                SkorIndikator = item.SkorIndikator,
                SkorTertimbang = skorTertimbang,
                CatatanIndikator = item.CatatanIndikator?.Trim()
            });
        }

        totalSkor = Math.Min(100, Math.Max(0, Math.Round(totalSkor, 2)));

        // Tentukan Predikat Kinerja
        KategoriKinerja predikat = totalSkor switch
        {
            >= 90.00m => KategoriKinerja.Istimewa,
            >= 80.00m => KategoriKinerja.Baik,
            >= 70.00m => KategoriKinerja.Cukup,
            >= 60.00m => KategoriKinerja.Kurang,
            _ => KategoriKinerja.SangatKurang
        };

        using var transaksi = await _context.Database.BeginTransactionAsync();
        try
        {
            var existing = await _context.PenilaianKinerja
                .Include(p => p.DaftarDetailPenilaian)
                .FirstOrDefaultAsync(p => p.PeriodePenilaianId == model.PeriodePenilaianId && p.KaryawanId == model.KaryawanId);

            PenilaianKinerja penilaian;

            if (existing != null)
            {
                penilaian = existing;
                penilaian.TanggalPenilaian = model.TanggalPenilaian;
                penilaian.PenilaiId = penilaiId;
                penilaian.NamaPenilai = namaPenilai;
                penilaian.TotalSkorAkhir = totalSkor;
                penilaian.PredikatKinerja = predikat;
                penilaian.KomentarEvaluasi = model.KomentarEvaluasi.Trim();
                penilaian.RekomendasiPengembangan = model.RekomendasiPengembangan?.Trim();
                penilaian.Status = model.Status;

                // Hapus detail lama dan ganti baru
                _context.DetailPenilaian.RemoveRange(existing.DaftarDetailPenilaian);
                foreach (var d in detailEntities)
                {
                    d.PenilaianKinerjaId = penilaian.Id;
                    await _context.DetailPenilaian.AddAsync(d);
                }
            }
            else
            {
                penilaian = new PenilaianKinerja
                {
                    Id = Guid.NewGuid(),
                    PerusahaanId = tenantId,
                    PeriodePenilaianId = model.PeriodePenilaianId,
                    KaryawanId = model.KaryawanId,
                    PenilaiId = penilaiId,
                    NamaPenilai = namaPenilai,
                    TanggalPenilaian = model.TanggalPenilaian,
                    TotalSkorAkhir = totalSkor,
                    PredikatKinerja = predikat,
                    KomentarEvaluasi = model.KomentarEvaluasi.Trim(),
                    RekomendasiPengembangan = model.RekomendasiPengembangan?.Trim(),
                    Status = model.Status
                };

                await _context.PenilaianKinerja.AddAsync(penilaian);

                foreach (var d in detailEntities)
                {
                    d.PenilaianKinerjaId = penilaian.Id;
                    penilaian.DaftarDetailPenilaian.Add(d);
                }
            }

            await _context.SaveChangesAsync();
            await transaksi.CommitAsync();

            await _layananAudit.CatatAsync(
                "SIMPAN_PENILAIAN_KINERJA",
                "PenilaianKinerja",
                $"Menyimpan evaluasi kinerja {karyawan.NamaLengkap} pada periode {periode.NamaPeriode}. Skor Akhir: {totalSkor:F2} ({predikat})");

            return (true, $"Penilaian kinerja '{karyawan.NamaLengkap}' berhasil disimpan dengan skor akhir {totalSkor:F2} ({predikat}).", penilaian.Id);
        }
        catch (Exception ex)
        {
            await transaksi.RollbackAsync();
            return (false, $"Gagal menyimpan penilaian kinerja: {ex.Message}", null);
        }
    }

    public async Task<LaporanPerformaKaryawanViewModel> DapatkanLaporanPerformaAsync(Guid? karyawanId, Guid? departemenId, int? tahun)
    {
        var query = _context.PenilaianKinerja
            .Include(p => p.PeriodePenilaian)
            .Include(p => p.Karyawan!)
                .ThenInclude(k => k.Departemen)
            .Include(p => p.Karyawan!)
                .ThenInclude(k => k.Jabatan)
            .AsQueryable();

        if (karyawanId.HasValue) query = query.Where(p => p.KaryawanId == karyawanId.Value);
        if (departemenId.HasValue) query = query.Where(p => p.Karyawan != null && p.Karyawan.DepartemenId == departemenId.Value);
        if (tahun.HasValue) query = query.Where(p => p.PeriodePenilaian != null && p.PeriodePenilaian.Tahun == tahun.Value);

        var list = await query
            .OrderByDescending(p => p.PeriodePenilaian != null ? p.PeriodePenilaian.Tahun : 0)
            .ThenByDescending(p => p.TanggalPenilaian)
            .Select(p => new PenilaianKinerjaItemViewModel
            {
                Id = p.Id,
                PeriodePenilaianId = p.PeriodePenilaianId,
                NamaPeriode = p.PeriodePenilaian != null ? p.PeriodePenilaian.NamaPeriode : "-",
                KaryawanId = p.KaryawanId,
                NIK = p.Karyawan != null ? p.Karyawan.NomorIndukKaryawan : "-",
                NamaKaryawan = p.Karyawan != null ? p.Karyawan.NamaLengkap : "-",
                NamaDepartemen = p.Karyawan != null && p.Karyawan.Departemen != null ? p.Karyawan.Departemen.NamaDepartemen : "-",
                NamaJabatan = p.Karyawan != null && p.Karyawan.Jabatan != null ? p.Karyawan.Jabatan.NamaJabatan : "-",
                NamaPenilai = p.NamaPenilai,
                TanggalPenilaian = p.TanggalPenilaian,
                TotalSkorAkhir = p.TotalSkorAkhir,
                PredikatKinerja = p.PredikatKinerja,
                Status = p.Status
            })
            .ToListAsync();

        return new LaporanPerformaKaryawanViewModel
        {
            KaryawanIdTerpilih = karyawanId,
            DepartemenIdTerpilih = departemenId,
            TahunTerpilih = tahun,
            DaftarPenilaian = list,
            RataRataSkorTotal = list.Any() ? Math.Round(list.Average(p => p.TotalSkorAkhir), 2) : 0,
            TotalPenilaian = list.Count
        };
    }
    #endregion
}
