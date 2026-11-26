using Microsoft.EntityFrameworkCore;
using SistemHRD.Data;
using SistemHRD.Models.Entitas;
using SistemHRD.Models.Enums;
using SistemHRD.Services.Interfaces;
using SistemHRD.ViewModels;

namespace SistemHRD.Services.Implementasi;

public class LayananManajemenAset : ILayananManajemenAset
{
    private readonly KonteksAplikasi _context;
    private readonly ILayananTenant _layananTenant;

    public LayananManajemenAset(KonteksAplikasi context, ILayananTenant layananTenant)
    {
        _context = context;
        _layananTenant = layananTenant;
    }

    public async Task<DashboardAsetViewModel> DapatkanDashboardAsync()
    {
        var model = new DashboardAsetViewModel();

        model.TotalAset = await _context.AsetPerusahaan.CountAsync();
        model.TotalAsetTersedia = await _context.AsetPerusahaan
            .CountAsync(a => a.Status == StatusAset.Tersedia);
        model.TotalAsetDigunakan = await _context.AsetPerusahaan
            .CountAsync(a => a.Status == StatusAset.Digunakan);
        model.TotalAsetDalamPerbaikan = await _context.AsetPerusahaan
            .CountAsync(a => a.Status == StatusAset.DalamPerbaikan);

        model.TotalNilaiInventaris = await _context.AsetPerusahaan
            .SumAsync(a => (decimal?)a.NilaiPerolehan) ?? 0;

        // Statistik Kategori
        var katList = await _context.AsetPerusahaan
            .GroupBy(a => a.Kategori)
            .Select(g => new { Kategori = g.Key.ToString(), Count = g.Count() })
            .ToListAsync();
        foreach (var k in katList) model.StatistikKategori[k.Kategori] = k.Count;

        // Statistik Kondisi
        var konList = await _context.AsetPerusahaan
            .GroupBy(a => a.Kondisi)
            .Select(g => new { Kondisi = g.Key.ToString(), Count = g.Count() })
            .ToListAsync();
        foreach (var k in konList) model.StatistikKondisi[k.Kondisi] = k.Count;

        // Aset Terbaru
        model.AsetTerbaru = await _context.AsetPerusahaan
            .Include(a => a.PemegangSaatIni)
            .OrderByDescending(a => a.CreatedAt)
            .Take(5)
            .Select(a => new AsetItemViewModel
            {
                Id = a.Id,
                NomorAset = a.NomorAset,
                NamaAset = a.NamaAset,
                Kategori = a.Kategori,
                MerkModel = a.MerkModel,
                NomorSeri = a.NomorSeri,
                TanggalPembelian = a.TanggalPembelian,
                NilaiPerolehan = a.NilaiPerolehan,
                Kondisi = a.Kondisi,
                Status = a.Status,
                LokasiPenyimpanan = a.LokasiPenyimpanan,
                NamaPemegang = a.PemegangSaatIni != null ? a.PemegangSaatIni.NamaLengkap : null,
                NIKPemegang = a.PemegangSaatIni != null ? a.PemegangSaatIni.NomorIndukKaryawan : null,
                PemegangSaatIniId = a.PemegangSaatIniId
            })
            .ToListAsync();

        // Penyerahan Terbaru
        model.PenyerahanTerbaru = await _context.PenyerahanAset
            .Include(p => p.AsetPerusahaan)
            .Include(p => p.Karyawan)
            .OrderByDescending(p => p.CreatedAt)
            .Take(5)
            .Select(p => new PenyerahanAsetItemViewModel
            {
                Id = p.Id,
                AsetPerusahaanId = p.AsetPerusahaanId,
                NomorAset = p.AsetPerusahaan.NomorAset,
                NamaAset = p.AsetPerusahaan.NamaAset,
                KaryawanId = p.KaryawanId,
                NIK = p.Karyawan.NomorIndukKaryawan,
                NamaKaryawan = p.Karyawan.NamaLengkap,
                NomorDokumen = p.NomorDokumen,
                TanggalPenyerahan = p.TanggalPenyerahan,
                KondisiSaatPenyerahan = p.KondisiSaatPenyerahan,
                StatusPeminjaman = p.StatusPeminjaman
            })
            .ToListAsync();

        return model;
    }

    public async Task<DaftarAsetPagedViewModel> DapatkanDaftarAsetAsync(
        KategoriAset? kategori = null,
        StatusAset? status = null,
        KondisiAset? kondisi = null,
        string? kataKunci = null,
        int halaman = 1,
        int ukuranHalaman = 10)
    {
        var query = _context.AsetPerusahaan
            .Include(a => a.PemegangSaatIni)
            .AsQueryable();

        if (kategori.HasValue) query = query.Where(a => a.Kategori == kategori.Value);
        if (status.HasValue) query = query.Where(a => a.Status == status.Value);
        if (kondisi.HasValue) query = query.Where(a => a.Kondisi == kondisi.Value);

        if (!string.IsNullOrWhiteSpace(kataKunci))
        {
            var k = kataKunci.Trim().ToLower();
            query = query.Where(a => a.NomorAset.ToLower().Contains(k) ||
                                     a.NamaAset.ToLower().Contains(k) ||
                                     (a.MerkModel != null && a.MerkModel.ToLower().Contains(k)) ||
                                     (a.NomorSeri != null && a.NomorSeri.ToLower().Contains(k)));
        }

        var totalData = await query.CountAsync();
        var totalHalaman = (int)Math.Ceiling(totalData / (double)ukuranHalaman);
        if (totalHalaman < 1) totalHalaman = 1;
        if (halaman < 1) halaman = 1;
        if (halaman > totalHalaman) halaman = totalHalaman;

        var data = await query
            .OrderBy(a => a.NomorAset)
            .Skip((halaman - 1) * ukuranHalaman)
            .Take(ukuranHalaman)
            .Select(a => new AsetItemViewModel
            {
                Id = a.Id,
                NomorAset = a.NomorAset,
                NamaAset = a.NamaAset,
                Kategori = a.Kategori,
                MerkModel = a.MerkModel,
                NomorSeri = a.NomorSeri,
                TanggalPembelian = a.TanggalPembelian,
                NilaiPerolehan = a.NilaiPerolehan,
                Kondisi = a.Kondisi,
                Status = a.Status,
                LokasiPenyimpanan = a.LokasiPenyimpanan,
                NamaPemegang = a.PemegangSaatIni != null ? a.PemegangSaatIni.NamaLengkap : null,
                NIKPemegang = a.PemegangSaatIni != null ? a.PemegangSaatIni.NomorIndukKaryawan : null,
                PemegangSaatIniId = a.PemegangSaatIniId
            })
            .ToListAsync();

        return new DaftarAsetPagedViewModel
        {
            DaftarAset = data,
            HalamanSaatIni = halaman,
            TotalHalaman = totalHalaman,
            UkuranHalaman = ukuranHalaman,
            TotalData = totalData,
            KategoriTerpilih = kategori,
            StatusTerpilih = status,
            KondisiTerpilih = kondisi,
            KataKunci = kataKunci
        };
    }

    public async Task<AsetPerusahaan?> DapatkanAsetByIdAsync(Guid id)
    {
        return await _context.AsetPerusahaan
            .Include(a => a.PemegangSaatIni)
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<DetailAsetViewModel?> DapatkanDetailAsetAsync(Guid id)
    {
        var a = await _context.AsetPerusahaan
            .Include(ast => ast.PemegangSaatIni)
                .ThenInclude(k => k!.Departemen)
            .Include(ast => ast.PemegangSaatIni)
                .ThenInclude(k => k!.Jabatan)
            .Include(ast => ast.DaftarPenyerahan)
                .ThenInclude(p => p.Karyawan)
            .Include(ast => ast.DaftarRiwayat)
                .ThenInclude(r => r.KaryawanTerkait)
            .FirstOrDefaultAsync(ast => ast.Id == id);

        if (a == null) return null;

        return new DetailAsetViewModel
        {
            Id = a.Id,
            NomorAset = a.NomorAset,
            NamaAset = a.NamaAset,
            Kategori = a.Kategori,
            MerkModel = a.MerkModel,
            NomorSeri = a.NomorSeri,
            TanggalPembelian = a.TanggalPembelian,
            NilaiPerolehan = a.NilaiPerolehan,
            Kondisi = a.Kondisi,
            Status = a.Status,
            LokasiPenyimpanan = a.LokasiPenyimpanan,
            DeskripsiSpesifikasi = a.DeskripsiSpesifikasi,
            PemegangSaatIniId = a.PemegangSaatIniId,
            NamaPemegang = a.PemegangSaatIni?.NamaLengkap,
            NIKPemegang = a.PemegangSaatIni?.NomorIndukKaryawan,
            DepartemenPemegang = a.PemegangSaatIni?.Departemen?.NamaDepartemen,
            JabatanPemegang = a.PemegangSaatIni?.Jabatan?.NamaJabatan,
            RiwayatPenyerahan = a.DaftarPenyerahan.Select(p => new PenyerahanAsetItemViewModel
            {
                Id = p.Id,
                AsetPerusahaanId = p.AsetPerusahaanId,
                NomorAset = a.NomorAset,
                NamaAset = a.NamaAset,
                KaryawanId = p.KaryawanId,
                NIK = p.Karyawan.NomorIndukKaryawan,
                NamaKaryawan = p.Karyawan.NamaLengkap,
                NomorDokumen = p.NomorDokumen,
                TanggalPenyerahan = p.TanggalPenyerahan,
                TanggalTargetPengembalian = p.TanggalTargetPengembalian,
                TanggalRealisasiPengembalian = p.TanggalRealisasiPengembalian,
                KondisiSaatPenyerahan = p.KondisiSaatPenyerahan,
                KondisiSaatPengembalian = p.KondisiSaatPengembalian,
                StatusPeminjaman = p.StatusPeminjaman,
                KelengkapanAset = p.KelengkapanAset,
                CatatanPenyerahan = p.CatatanPenyerahan,
                CatatanPengembalian = p.CatatanPengembalian
            }).OrderByDescending(p => p.TanggalPenyerahan).ToList(),
            RiwayatAktivitas = a.DaftarRiwayat.Select(r => new RiwayatAsetItemViewModel
            {
                Id = r.Id,
                TipeAktivitas = r.TipeAktivitas,
                TanggalAktivitas = r.TanggalAktivitas,
                NamaKaryawanTerkait = r.KaryawanTerkait?.NamaLengkap,
                NamaPetugas = r.NamaPetugas,
                KondisiSebelumnya = r.KondisiSebelumnya,
                KondisiTerbaru = r.KondisiTerbaru,
                Deskripsi = r.Deskripsi,
                BiayaPerbaikan = r.BiayaPerbaikan
            }).OrderByDescending(r => r.TanggalAktivitas).ToList()
        };
    }

    public async Task<(bool Sukses, string Pesan, Guid? Id)> SimpanAsetAsync(FormAsetViewModel model)
    {
        var perusahaanId = _layananTenant.PerusahaanIdSaatIni;
        if (!perusahaanId.HasValue) return (false, "Sesi perusahaan tidak valid.", null);

        var duplikat = await _context.AsetPerusahaan
            .AnyAsync(a => a.NomorAset == model.NomorAset && (!model.Id.HasValue || a.Id != model.Id.Value));

        if (duplikat)
        {
            return (false, $"Nomor aset '{model.NomorAset}' sudah terdaftar dalam perusahaan Anda.", null);
        }

        AsetPerusahaan aset;
        if (model.Id.HasValue && model.Id.Value != Guid.Empty)
        {
            var existing = await _context.AsetPerusahaan.FindAsync(model.Id.Value);
            if (existing == null) return (false, "Aset tidak ditemukan.", null);
            aset = existing;
        }
        else
        {
            aset = new AsetPerusahaan { PerusahaanId = perusahaanId.Value };
            _context.AsetPerusahaan.Add(aset);
        }

        aset.NomorAset = model.NomorAset;
        aset.NamaAset = model.NamaAset;
        aset.Kategori = model.Kategori;
        aset.MerkModel = model.MerkModel;
        aset.NomorSeri = model.NomorSeri;
        aset.TanggalPembelian = model.TanggalPembelian;
        aset.NilaiPerolehan = model.NilaiPerolehan;
        aset.Kondisi = model.Kondisi;
        aset.Status = model.Status;
        aset.LokasiPenyimpanan = model.LokasiPenyimpanan;
        aset.DeskripsiSpesifikasi = model.DeskripsiSpesifikasi;

        await _context.SaveChangesAsync();
        return (true, "Data aset berhasil disimpan.", aset.Id);
    }

    public async Task<(bool Sukses, string Pesan)> HapusAsetAsync(Guid id)
    {
        var aset = await _context.AsetPerusahaan.FindAsync(id);
        if (aset == null) return (false, "Aset tidak ditemukan.");

        if (aset.PemegangSaatIniId.HasValue || aset.Status == StatusAset.Digunakan)
        {
            return (false, "Aset tidak dapat dihapus karena saat ini sedang dipinjamkan / digunakan oleh karyawan.");
        }

        _context.AsetPerusahaan.Remove(aset);
        await _context.SaveChangesAsync();
        return (true, "Data aset berhasil dihapus.");
    }

    public async Task<(bool Sukses, string Pesan)> SerahkanAsetAsync(FormPenyerahanAsetViewModel model, Guid diserahkanOlehId, string namaPetugas)
    {
        var aset = await _context.AsetPerusahaan.FindAsync(model.AsetPerusahaanId);
        if (aset == null) return (false, "Aset tidak ditemukan.");

        if (aset.Status == StatusAset.Digunakan || aset.PemegangSaatIniId.HasValue)
        {
            return (false, "Aset ini sedang digunakan oleh karyawan lain. Harap lakukan proses pengembalian terlebih dahulu.");
        }

        if (aset.Status == StatusAset.DalamPerbaikan || aset.Status == StatusAset.Dihapuskan)
        {
            return (false, $"Aset tidak dapat diserahkan karena status saat ini adalah '{aset.Status}'.");
        }

        var karyawan = await _context.Karyawan.FindAsync(model.KaryawanId);
        if (karyawan == null) return (false, "Data karyawan penerima tidak ditemukan.");

        using var tx = await _context.Database.BeginTransactionAsync();
        try
        {
            var penyerahan = new PenyerahanAset
            {
                PerusahaanId = aset.PerusahaanId,
                AsetPerusahaanId = aset.Id,
                KaryawanId = karyawan.Id,
                NomorDokumen = model.NomorDokumen,
                TanggalPenyerahan = model.TanggalPenyerahan,
                TanggalTargetPengembalian = model.TanggalTargetPengembalian,
                KondisiSaatPenyerahan = model.KondisiSaatPenyerahan,
                StatusPeminjaman = "Digunakan",
                KelengkapanAset = model.KelengkapanAset,
                CatatanPenyerahan = model.CatatanPenyerahan,
                DiserahkanOlehId = diserahkanOlehId
            };
            _context.PenyerahanAset.Add(penyerahan);

            var kondisiSebelumnya = aset.Kondisi;
            aset.PemegangSaatIniId = karyawan.Id;
            aset.Status = StatusAset.Digunakan;
            aset.Kondisi = model.KondisiSaatPenyerahan;

            var riwayat = new RiwayatAset
            {
                PerusahaanId = aset.PerusahaanId,
                AsetPerusahaanId = aset.Id,
                TipeAktivitas = TipeAktivitasAset.Penyerahan,
                TanggalAktivitas = DateTime.Now,
                KaryawanTerkaitId = karyawan.Id,
                NamaPetugas = namaPetugas,
                KondisiSebelumnya = kondisiSebelumnya,
                KondisiTerbaru = model.KondisiSaatPenyerahan,
                Deskripsi = $"Penyerahan aset kepada {karyawan.NamaLengkap} ({karyawan.NomorIndukKaryawan}) berdasarkan dokumen {model.NomorDokumen}."
            };
            _context.RiwayatAset.Add(riwayat);

            await _context.SaveChangesAsync();
            await tx.CommitAsync();

            return (true, $"Aset '{aset.NamaAset}' berhasil diserahkan kepada {karyawan.NamaLengkap}.");
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync();
            return (false, $"Terjadi kesalahan saat memproses penyerahan aset: {ex.Message}");
        }
    }

    public async Task<(bool Sukses, string Pesan)> KembalikanAsetAsync(FormPengembalianAsetViewModel model, Guid diterimaKembaliOlehId, string namaPetugas)
    {
        var penyerahan = await _context.PenyerahanAset
            .Include(p => p.AsetPerusahaan)
            .Include(p => p.Karyawan)
            .FirstOrDefaultAsync(p => p.Id == model.PenyerahanAsetId);

        if (penyerahan == null) return (false, "Dokumen penyerahan aset tidak ditemukan.");

        var aset = penyerahan.AsetPerusahaan;

        using var tx = await _context.Database.BeginTransactionAsync();
        try
        {
            penyerahan.TanggalRealisasiPengembalian = model.TanggalRealisasiPengembalian;
            penyerahan.KondisiSaatPengembalian = model.KondisiSaatPengembalian;
            penyerahan.StatusPeminjaman = model.StatusPeminjaman;
            penyerahan.CatatanPengembalian = model.CatatanPengembalian;
            penyerahan.DiterimaKembaliOlehId = diterimaKembaliOlehId;

            var kondisiSebelumnya = aset.Kondisi;
            aset.PemegangSaatIniId = null;
            aset.Kondisi = model.KondisiSaatPengembalian;

            if (model.KondisiSaatPengembalian == KondisiAset.RusakBerat || model.KondisiSaatPengembalian == KondisiAset.RusakRingan)
            {
                aset.Status = StatusAset.DalamPerbaikan;
            }
            else if (model.KondisiSaatPengembalian == KondisiAset.Hilang)
            {
                aset.Status = StatusAset.Dihapuskan;
            }
            else
            {
                aset.Status = StatusAset.Tersedia;
            }

            var riwayat = new RiwayatAset
            {
                PerusahaanId = aset.PerusahaanId,
                AsetPerusahaanId = aset.Id,
                TipeAktivitas = TipeAktivitasAset.Pengembalian,
                TanggalAktivitas = DateTime.Now,
                KaryawanTerkaitId = penyerahan.KaryawanId,
                NamaPetugas = namaPetugas,
                KondisiSebelumnya = kondisiSebelumnya,
                KondisiTerbaru = model.KondisiSaatPengembalian,
                Deskripsi = $"Pengembalian aset dari {penyerahan.Karyawan.NamaLengkap} ({penyerahan.Karyawan.NomorIndukKaryawan}) dengan kondisi '{model.KondisiSaatPengembalian}'."
            };
            _context.RiwayatAset.Add(riwayat);

            await _context.SaveChangesAsync();
            await tx.CommitAsync();

            return (true, $"Aset '{aset.NamaAset}' berhasil diterima kembali dan tercatat di inventaris.");
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync();
            return (false, $"Terjadi kesalahan saat memproses pengembalian aset: {ex.Message}");
        }
    }

    public async Task<List<PenyerahanAsetItemViewModel>> DapatkanAsetKaryawanAktifAsync(Guid karyawanId)
    {
        return await _context.PenyerahanAset
            .Include(p => p.AsetPerusahaan)
            .Include(p => p.Karyawan)
            .Where(p => p.KaryawanId == karyawanId && p.StatusPeminjaman == "Digunakan")
            .OrderByDescending(p => p.TanggalPenyerahan)
            .Select(p => new PenyerahanAsetItemViewModel
            {
                Id = p.Id,
                AsetPerusahaanId = p.AsetPerusahaanId,
                NomorAset = p.AsetPerusahaan.NomorAset,
                NamaAset = p.AsetPerusahaan.NamaAset,
                KaryawanId = p.KaryawanId,
                NIK = p.Karyawan.NomorIndukKaryawan,
                NamaKaryawan = p.Karyawan.NamaLengkap,
                NomorDokumen = p.NomorDokumen,
                TanggalPenyerahan = p.TanggalPenyerahan,
                TanggalTargetPengembalian = p.TanggalTargetPengembalian,
                KondisiSaatPenyerahan = p.KondisiSaatPenyerahan,
                StatusPeminjaman = p.StatusPeminjaman,
                KelengkapanAset = p.KelengkapanAset,
                CatatanPenyerahan = p.CatatanPenyerahan
            })
            .ToListAsync();
    }

    public async Task<List<RiwayatAsetItemViewModel>> DapatkanRiwayatAsetAsync(Guid asetId)
    {
        return await _context.RiwayatAset
            .Include(r => r.KaryawanTerkait)
            .Where(r => r.AsetPerusahaanId == asetId)
            .OrderByDescending(r => r.TanggalAktivitas)
            .Select(r => new RiwayatAsetItemViewModel
            {
                Id = r.Id,
                TipeAktivitas = r.TipeAktivitas,
                TanggalAktivitas = r.TanggalAktivitas,
                NamaKaryawanTerkait = r.KaryawanTerkait != null ? r.KaryawanTerkait.NamaLengkap : null,
                NamaPetugas = r.NamaPetugas,
                KondisiSebelumnya = r.KondisiSebelumnya,
                KondisiTerbaru = r.KondisiTerbaru,
                Deskripsi = r.Deskripsi,
                BiayaPerbaikan = r.BiayaPerbaikan
            })
            .ToListAsync();
    }
}
