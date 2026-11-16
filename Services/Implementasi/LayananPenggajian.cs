using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using SistemHRD.Common;
using SistemHRD.Data;
using SistemHRD.Models.Entitas;
using SistemHRD.Models.Enums;
using SistemHRD.Services.Interfaces;
using SistemHRD.ViewModels;

namespace SistemHRD.Services.Implementasi;

public class LayananPenggajian : ILayananPenggajian
{
    private readonly KonteksAplikasi _context;
    private readonly ILayananTenant _layananTenant;
    private readonly ILayananAudit _layananAudit;
    private readonly ILayananKalkulatorGaji _kalkulator;

    public LayananPenggajian(
        KonteksAplikasi context,
        ILayananTenant layananTenant,
        ILayananAudit layananAudit,
        ILayananKalkulatorGaji kalkulator)
    {
        _context = context;
        _layananTenant = layananTenant;
        _layananAudit = layananAudit;
        _kalkulator = kalkulator;
    }

    #region Master Komponen Gaji

    public async Task<List<KomponenGajiViewModel>> DapatkanDaftarKomponenAsync()
    {
        return await _context.KomponenGaji
            .OrderBy(k => k.Jenis)
            .ThenBy(k => k.KodeKomponen)
            .Select(k => new KomponenGajiViewModel
            {
                Id = k.Id,
                KodeKomponen = k.KodeKomponen,
                NamaKomponen = k.NamaKomponen,
                Jenis = k.Jenis,
                Sifat = k.Sifat,
                Kategori = k.Kategori,
                NilaiBawaan = k.NilaiBawaan,
                KenaPajak = k.KenaPajak,
                MasukKalkulasiBpjs = k.MasukKalkulasiBpjs,
                DihitungOtomatisOlehSistem = k.DihitungOtomatisOlehSistem,
                StatusAktif = k.StatusAktif,
                Keterangan = k.Keterangan
            })
            .ToListAsync();
    }

    public async Task<KomponenGaji?> DapatkanKomponenByIdAsync(Guid id)
    {
        return await _context.KomponenGaji.FindAsync(id);
    }

    public async Task<(bool Sukses, string Pesan)> SimpanKomponenAsync(KomponenGajiViewModel model)
    {
        var tenantId = _layananTenant.PerusahaanIdSaatIni;

        var kodeDuplikat = await _context.KomponenGaji
            .AnyAsync(k => k.KodeKomponen.ToUpper() == model.KodeKomponen.Trim().ToUpper() && (!model.Id.HasValue || k.Id != model.Id.Value));

        if (kodeDuplikat)
        {
            return (false, $"Kode komponen '{model.KodeKomponen}' sudah digunakan pada perusahaan ini.");
        }

        if (model.Id.HasValue && model.Id.Value != Guid.Empty)
        {
            var komponen = await _context.KomponenGaji.FindAsync(model.Id.Value);
            if (komponen == null) return (false, "Komponen gaji tidak ditemukan.");

            komponen.KodeKomponen = model.KodeKomponen.Trim().ToUpper();
            komponen.NamaKomponen = model.NamaKomponen.Trim();
            komponen.Jenis = model.Jenis;
            komponen.Sifat = model.Sifat;
            komponen.Kategori = model.Kategori;
            komponen.NilaiBawaan = model.NilaiBawaan;
            komponen.KenaPajak = model.KenaPajak;
            komponen.MasukKalkulasiBpjs = model.MasukKalkulasiBpjs;
            komponen.DihitungOtomatisOlehSistem = model.DihitungOtomatisOlehSistem;
            komponen.StatusAktif = model.StatusAktif;
            komponen.Keterangan = model.Keterangan;

            _context.KomponenGaji.Update(komponen);
            await _context.SaveChangesAsync();

            await _layananAudit.CatatAsync("UBAH_KOMPONEN_GAJI", "KomponenGaji", $"Memperbarui komponen gaji {komponen.KodeKomponen} - {komponen.NamaKomponen}");
            return (true, "Komponen gaji berhasil diperbarui.");
        }
        else
        {
            var baru = new KomponenGaji
            {
                Id = Guid.NewGuid(),
                PerusahaanId = tenantId ?? Guid.Empty,
                KodeKomponen = model.KodeKomponen.Trim().ToUpper(),
                NamaKomponen = model.NamaKomponen.Trim(),
                Jenis = model.Jenis,
                Sifat = model.Sifat,
                Kategori = model.Kategori,
                NilaiBawaan = model.NilaiBawaan,
                KenaPajak = model.KenaPajak,
                MasukKalkulasiBpjs = model.MasukKalkulasiBpjs,
                DihitungOtomatisOlehSistem = model.DihitungOtomatisOlehSistem,
                StatusAktif = model.StatusAktif,
                Keterangan = model.Keterangan
            };

            await _context.KomponenGaji.AddAsync(baru);
            await _context.SaveChangesAsync();

            await _layananAudit.CatatAsync("TAMBAH_KOMPONEN_GAJI", "KomponenGaji", $"Menambahkan komponen gaji baru {baru.KodeKomponen} - {baru.NamaKomponen}");
            return (true, "Komponen gaji baru berhasil ditambahkan.");
        }
    }

    public async Task<(bool Sukses, string Pesan)> HapusKomponenAsync(Guid id)
    {
        var komponen = await _context.KomponenGaji.FindAsync(id);
        if (komponen == null) return (false, "Komponen gaji tidak ditemukan.");

        var dipakaiDiPayroll = await _context.DetailPenggajian.AnyAsync(d => d.KomponenGajiId == id);
        if (dipakaiDiPayroll)
        {
            // Nonaktifkan saja jika sudah pernah digunakan dalam riwayat payroll
            komponen.StatusAktif = false;
            _context.KomponenGaji.Update(komponen);
            await _context.SaveChangesAsync();
            return (true, "Komponen dinonaktifkan karena telah tercatat dalam riwayat penggajian terdahulu.");
        }

        _context.KomponenGaji.Remove(komponen);
        await _context.SaveChangesAsync();

        await _layananAudit.CatatAsync("HAPUS_KOMPONEN_GAJI", "KomponenGaji", $"Menghapus komponen gaji {komponen.KodeKomponen}");
        return (true, "Komponen gaji berhasil dihapus.");
    }

    #endregion

    #region Pengaturan Gaji Karyawan

    public async Task<AturGajiKaryawanViewModel?> DapatkanAturGajiKaryawanAsync(Guid karyawanId)
    {
        var karyawan = await _context.Karyawan
            .Include(k => k.Departemen)
            .Include(k => k.Jabatan)
            .FirstOrDefaultAsync(k => k.Id == karyawanId);

        if (karyawan == null) return null;

        var semuaKomponen = await _context.KomponenGaji
            .Where(k => k.StatusAktif && !k.DihitungOtomatisOlehSistem)
            .OrderBy(k => k.Jenis)
            .ThenBy(k => k.KodeKomponen)
            .ToListAsync();

        var gajiKaryawanList = await _context.GajiKaryawan
            .Where(g => g.KaryawanId == karyawanId)
            .ToListAsync();

        var itemKomponen = new List<ItemKomponenGajiKaryawanViewModel>();
        decimal gp = 0;
        decimal tjTetap = 0;

        foreach (var k in semuaKomponen)
        {
            var existing = gajiKaryawanList.FirstOrDefault(g => g.KomponenGajiId == k.Id);
            var nominal = existing?.Nominal ?? k.NilaiBawaan;
            var aktif = existing?.StatusAktif ?? (existing != null);

            if (aktif && k.Jenis == JenisKomponenGaji.Pendapatan)
            {
                if (k.Kategori == KategoriKomponenGaji.GajiPokok) gp += nominal;
                else if (k.Sifat == SifatKomponenGaji.Tetap) tjTetap += nominal;
            }

            itemKomponen.Add(new ItemKomponenGajiKaryawanViewModel
            {
                KomponenGajiId = k.Id,
                KodeKomponen = k.KodeKomponen,
                NamaKomponen = k.NamaKomponen,
                Jenis = k.Jenis,
                Sifat = k.Sifat,
                Kategori = k.Kategori,
                Nominal = nominal,
                StatusAktif = aktif,
                DihitungOtomatisOlehSistem = k.DihitungOtomatisOlehSistem,
                Keterangan = existing?.Keterangan
            });
        }

        return new AturGajiKaryawanViewModel
        {
            KaryawanId = karyawan.Id,
            NomorIndukKaryawan = karyawan.NomorIndukKaryawan,
            NamaKaryawan = karyawan.NamaLengkap,
            NamaDepartemen = karyawan.Departemen?.NamaDepartemen ?? "-",
            NamaJabatan = karyawan.Jabatan?.NamaJabatan ?? "-",
            StatusPTKP = karyawan.StatusPTKP,
            NamaBank = karyawan.NamaBank,
            NomorRekeningBank = karyawan.NomorRekeningBank,
            NomorNPWP = karyawan.NomorNPWP,
            NomorBpjsKesehatan = karyawan.NomorBpjsKesehatan,
            NomorBpjsKetenagakerjaan = karyawan.NomorBpjsKetenagakerjaan,
            DaftarKomponen = itemKomponen,
            TotalGajiPokok = gp,
            TotalTunjanganTetap = tjTetap,
            TotalEstimasiTakeHomePay = gp + tjTetap
        };
    }

    public async Task<(bool Sukses, string Pesan)> SimpanGajiKaryawanAsync(FormSimpanGajiKaryawanViewModel model)
    {
        var karyawan = await _context.Karyawan.FindAsync(model.KaryawanId);
        if (karyawan == null) return (false, "Data karyawan tidak ditemukan.");

        var tenantId = _layananTenant.PerusahaanIdSaatIni;

        karyawan.StatusPTKP = model.StatusPTKP;
        karyawan.NamaBank = model.NamaBank;
        karyawan.NomorRekeningBank = model.NomorRekeningBank;
        karyawan.NomorNPWP = model.NomorNPWP;
        karyawan.NomorBpjsKesehatan = model.NomorBpjsKesehatan;
        karyawan.NomorBpjsKetenagakerjaan = model.NomorBpjsKetenagakerjaan;
        _context.Karyawan.Update(karyawan);

        var existingGaji = await _context.GajiKaryawan
            .Where(g => g.KaryawanId == model.KaryawanId)
            .ToListAsync();

        foreach (var item in model.Komponen)
        {
            var match = existingGaji.FirstOrDefault(g => g.KomponenGajiId == item.KomponenGajiId);
            if (match != null)
            {
                match.Nominal = item.Nominal;
                match.StatusAktif = item.StatusAktif;
                match.Keterangan = item.Keterangan;
                _context.GajiKaryawan.Update(match);
            }
            else if (item.StatusAktif && item.Nominal >= 0)
            {
                await _context.GajiKaryawan.AddAsync(new GajiKaryawan
                {
                    Id = Guid.NewGuid(),
                    PerusahaanId = tenantId ?? Guid.Empty,
                    KaryawanId = model.KaryawanId,
                    KomponenGajiId = item.KomponenGajiId,
                    Nominal = item.Nominal,
                    StatusAktif = item.StatusAktif,
                    Keterangan = item.Keterangan
                });
            }
        }

        await _context.SaveChangesAsync();
        await _layananAudit.CatatAsync("ATUR_GAJI_KARYAWAN", "GajiKaryawan", $"Memperbarui pengaturan gaji karyawan {karyawan.NomorIndukKaryawan} - {karyawan.NamaLengkap}");
        return (true, "Struktur komponen gaji karyawan berhasil disimpan.");
    }

    #endregion

    #region Kebijakan Penggajian

    public async Task<KebijakanPenggajianViewModel> DapatkanKebijakanPerusahaanAsync()
    {
        var kebijakan = await _context.KebijakanPenggajianPerusahaan.FirstOrDefaultAsync();
        if (kebijakan == null)
        {
            var tenantId = _layananTenant.PerusahaanIdSaatIni;
            kebijakan = new KebijakanPenggajianPerusahaan
            {
                Id = Guid.NewGuid(),
                PerusahaanId = tenantId ?? Guid.Empty,
                HitungLemburOtomatis = true,
                TerapkanPotonganKeterlambatan = true,
                TerapkanBpjsKesehatan = true,
                TerapkanBpjsKetenagakerjaan = true,
                TerapkanPph21 = true,
                MetodePph21 = MetodePajakPPh21.TER_PP58
            };
            await _context.KebijakanPenggajianPerusahaan.AddAsync(kebijakan);
            await _context.SaveChangesAsync();
        }

        return new KebijakanPenggajianViewModel
        {
            Id = kebijakan.Id,
            HitungLemburOtomatis = kebijakan.HitungLemburOtomatis,
            TarifLemburPerJamFlat = kebijakan.TarifLemburPerJamFlat,
            TerapkanPotonganKeterlambatan = kebijakan.TerapkanPotonganKeterlambatan,
            ToleransiKeterlambatanGratisMenit = kebijakan.ToleransiKeterlambatanGratisMenit,
            DendaKeterlambatanPerMenit = kebijakan.DendaKeterlambatanPerMenit,
            TerapkanBpjsKesehatan = kebijakan.TerapkanBpjsKesehatan,
            PersenBpjsKesehatanPerusahaan = kebijakan.PersenBpjsKesehatanPerusahaan,
            PersenBpjsKesehatanPekerja = kebijakan.PersenBpjsKesehatanPekerja,
            BatasMaksimalUpahBpjsKesehatan = kebijakan.BatasMaksimalUpahBpjsKesehatan,
            TerapkanBpjsKetenagakerjaan = kebijakan.TerapkanBpjsKetenagakerjaan,
            PersenBpjsJhtPerusahaan = kebijakan.PersenBpjsJhtPerusahaan,
            PersenBpjsJhtPekerja = kebijakan.PersenBpjsJhtPekerja,
            PersenBpjsJpPerusahaan = kebijakan.PersenBpjsJpPerusahaan,
            PersenBpjsJpPekerja = kebijakan.PersenBpjsJpPekerja,
            BatasMaksimalUpahBpjsJp = kebijakan.BatasMaksimalUpahBpjsJp,
            PersenBpjsJkkPerusahaan = kebijakan.PersenBpjsJkkPerusahaan,
            PersenBpjsJkmPerusahaan = kebijakan.PersenBpjsJkmPerusahaan,
            TerapkanPph21 = kebijakan.TerapkanPph21,
            MetodePph21 = kebijakan.MetodePph21,
            TanggalCutoffAwal = kebijakan.TanggalCutoffAwal,
            TanggalCutoffAkhir = kebijakan.TanggalCutoffAkhir,
            TanggalPembayaranGaji = kebijakan.TanggalPembayaranGaji,
            CatatanSlipGajiDefault = kebijakan.CatatanSlipGajiDefault
        };
    }

    public async Task<(bool Sukses, string Pesan)> SimpanKebijakanPerusahaanAsync(KebijakanPenggajianViewModel model)
    {
        var kebijakan = await _context.KebijakanPenggajianPerusahaan.FirstOrDefaultAsync();
        var tenantId = _layananTenant.PerusahaanIdSaatIni;

        if (kebijakan == null)
        {
            kebijakan = new KebijakanPenggajianPerusahaan
            {
                Id = Guid.NewGuid(),
                PerusahaanId = tenantId ?? Guid.Empty
            };
            await _context.KebijakanPenggajianPerusahaan.AddAsync(kebijakan);
        }

        kebijakan.HitungLemburOtomatis = model.HitungLemburOtomatis;
        kebijakan.TarifLemburPerJamFlat = model.TarifLemburPerJamFlat;
        kebijakan.TerapkanPotonganKeterlambatan = model.TerapkanPotonganKeterlambatan;
        kebijakan.ToleransiKeterlambatanGratisMenit = model.ToleransiKeterlambatanGratisMenit;
        kebijakan.DendaKeterlambatanPerMenit = model.DendaKeterlambatanPerMenit;
        kebijakan.TerapkanBpjsKesehatan = model.TerapkanBpjsKesehatan;
        kebijakan.PersenBpjsKesehatanPerusahaan = model.PersenBpjsKesehatanPerusahaan;
        kebijakan.PersenBpjsKesehatanPekerja = model.PersenBpjsKesehatanPekerja;
        kebijakan.BatasMaksimalUpahBpjsKesehatan = model.BatasMaksimalUpahBpjsKesehatan;
        kebijakan.TerapkanBpjsKetenagakerjaan = model.TerapkanBpjsKetenagakerjaan;
        kebijakan.PersenBpjsJhtPerusahaan = model.PersenBpjsJhtPerusahaan;
        kebijakan.PersenBpjsJhtPekerja = model.PersenBpjsJhtPekerja;
        kebijakan.PersenBpjsJpPerusahaan = model.PersenBpjsJpPerusahaan;
        kebijakan.PersenBpjsJpPekerja = model.PersenBpjsJpPekerja;
        kebijakan.BatasMaksimalUpahBpjsJp = model.BatasMaksimalUpahBpjsJp;
        kebijakan.PersenBpjsJkkPerusahaan = model.PersenBpjsJkkPerusahaan;
        kebijakan.PersenBpjsJkmPerusahaan = model.PersenBpjsJkmPerusahaan;
        kebijakan.TerapkanPph21 = model.TerapkanPph21;
        kebijakan.MetodePph21 = model.MetodePph21;
        kebijakan.TanggalCutoffAwal = model.TanggalCutoffAwal;
        kebijakan.TanggalCutoffAkhir = model.TanggalCutoffAkhir;
        kebijakan.TanggalPembayaranGaji = model.TanggalPembayaranGaji;
        kebijakan.CatatanSlipGajiDefault = model.CatatanSlipGajiDefault;

        _context.KebijakanPenggajianPerusahaan.Update(kebijakan);
        await _context.SaveChangesAsync();

        await _layananAudit.CatatAsync("UBAH_KEBIJAKAN_PENGGAJIAN", "KebijakanPenggajianPerusahaan", "Memperbarui konfigurasi tarif BPJS, Lembur, dan PPh 21 perusahaan");
        return (true, "Kebijakan penggajian perusahaan berhasil diperbarui.");
    }

    #endregion

    #region Periode Penggajian

    public async Task<List<PeriodePenggajianItemViewModel>> DapatkanDaftarPeriodeAsync()
    {
        return await _context.PeriodePenggajian
            .OrderByDescending(p => p.Tahun)
            .ThenByDescending(p => p.Bulan)
            .Select(p => new PeriodePenggajianItemViewModel
            {
                Id = p.Id,
                NamaPeriode = p.NamaPeriode,
                Bulan = p.Bulan,
                Tahun = p.Tahun,
                TanggalMulai = p.TanggalMulai,
                TanggalSelesai = p.TanggalSelesai,
                TanggalPembayaran = p.TanggalPembayaran,
                Status = p.Status,
                TotalKaryawan = p.TotalKaryawan,
                TotalPengeluaranGaji = p.TotalPengeluaranGaji,
                Catatan = p.Catatan,
                DisetujuiOleh = p.DisetujuiOleh != null ? p.DisetujuiOleh.NamaLengkap : null,
                WaktuDisetujui = p.WaktuDisetujui
            })
            .ToListAsync();
    }

    public async Task<PeriodePenggajian?> DapatkanPeriodeByIdAsync(Guid id)
    {
        return await _context.PeriodePenggajian.FindAsync(id);
    }

    public async Task<DetailPeriodePenggajianViewModel?> DapatkanDetailPeriodeAsync(Guid id)
    {
        var periode = await _context.PeriodePenggajian
            .Include(p => p.DisetujuiOleh)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (periode == null) return null;

        var daftarPayroll = await _context.Penggajian
            .Include(p => p.Karyawan!)
                .ThenInclude(k => k.Departemen)
            .Include(p => p.Karyawan!)
                .ThenInclude(k => k.Jabatan)
            .Where(p => p.PeriodePenggajianId == id)
            .OrderBy(p => p.Karyawan != null ? p.Karyawan.NamaLengkap : string.Empty)
            .Select(p => new ItemPayrollKaryawanViewModel
            {
                PenggajianId = p.Id,
                KaryawanId = p.KaryawanId,
                NomorSlipGaji = p.NomorSlipGaji,
                NomorIndukKaryawan = p.Karyawan != null ? p.Karyawan.NomorIndukKaryawan : string.Empty,
                NamaKaryawan = p.Karyawan != null ? p.Karyawan.NamaLengkap : string.Empty,
                NamaDepartemen = p.Karyawan != null && p.Karyawan.Departemen != null ? p.Karyawan.Departemen.NamaDepartemen : "-",
                NamaJabatan = p.Karyawan != null && p.Karyawan.Jabatan != null ? p.Karyawan.Jabatan.NamaJabatan : "-",
                GajiPokok = p.GajiPokok,
                TotalPendapatanKotor = p.TotalPendapatanKotor,
                TotalPotongan = p.TotalPotongan,
                GajiBersih = p.GajiBersih,
                Status = p.Status,
                ApakahTerkunci = p.ApakahTerkunci
            })
            .ToListAsync();

        var model = new DetailPeriodePenggajianViewModel
        {
            Id = periode.Id,
            NamaPeriode = periode.NamaPeriode,
            Bulan = periode.Bulan,
            Tahun = periode.Tahun,
            TanggalMulai = periode.TanggalMulai,
            TanggalSelesai = periode.TanggalSelesai,
            TanggalPembayaran = periode.TanggalPembayaran,
            Status = periode.Status,
            TotalKaryawan = daftarPayroll.Count,
            TotalPengeluaranGaji = daftarPayroll.Sum(p => p.GajiBersih),
            Catatan = periode.Catatan,
            DisetujuiOleh = periode.DisetujuiOleh?.NamaLengkap,
            WaktuDisetujui = periode.WaktuDisetujui,

            TotalGajiPokok = daftarPayroll.Sum(p => p.GajiPokok),
            TotalTunjangan = daftarPayroll.Sum(p => p.TotalPendapatanKotor - p.GajiPokok),
            TotalPotonganLainnya = daftarPayroll.Sum(p => p.TotalPotongan),
            DaftarPenggajian = daftarPayroll
        };

        return model;
    }

    public async Task<(bool Sukses, string Pesan, Guid? PeriodeId)> SimpanPeriodeAsync(FormPeriodePenggajianViewModel model)
    {
        var tenantId = _layananTenant.PerusahaanIdSaatIni;

        var duplikat = await _context.PeriodePenggajian
            .AnyAsync(p => p.Bulan == model.Bulan && p.Tahun == model.Tahun && (!model.Id.HasValue || p.Id != model.Id.Value));

        if (duplikat)
        {
            return (false, $"Periode penggajian untuk bulan {model.Bulan} tahun {model.Tahun} sudah ada pada perusahaan ini.", null);
        }

        if (model.Id.HasValue && model.Id.Value != Guid.Empty)
        {
            var periode = await _context.PeriodePenggajian.FindAsync(model.Id.Value);
            if (periode == null) return (false, "Periode penggajian tidak ditemukan.", null);

            if (periode.Status == StatusPeriodePenggajian.Disetujui || periode.Status == StatusPeriodePenggajian.Selesai)
            {
                return (false, "Periode yang sudah disetujui tidak boleh diubah.", null);
            }

            periode.NamaPeriode = model.NamaPeriode.Trim();
            periode.Bulan = model.Bulan;
            periode.Tahun = model.Tahun;
            periode.TanggalMulai = model.TanggalMulai.Date;
            periode.TanggalSelesai = model.TanggalSelesai.Date;
            periode.TanggalPembayaran = model.TanggalPembayaran.Date;
            periode.Catatan = model.Catatan;

            _context.PeriodePenggajian.Update(periode);
            await _context.SaveChangesAsync();

            await _layananAudit.CatatAsync("UBAH_PERIODE_PENGGAJIAN", "PeriodePenggajian", $"Memperbarui periode {periode.NamaPeriode}");
            return (true, "Periode penggajian berhasil diperbarui.", periode.Id);
        }
        else
        {
            var baru = new PeriodePenggajian
            {
                Id = Guid.NewGuid(),
                PerusahaanId = tenantId ?? Guid.Empty,
                NamaPeriode = model.NamaPeriode.Trim(),
                Bulan = model.Bulan,
                Tahun = model.Tahun,
                TanggalMulai = model.TanggalMulai.Date,
                TanggalSelesai = model.TanggalSelesai.Date,
                TanggalPembayaran = model.TanggalPembayaran.Date,
                Status = StatusPeriodePenggajian.Draf,
                Catatan = model.Catatan
            };

            await _context.PeriodePenggajian.AddAsync(baru);
            await _context.SaveChangesAsync();

            await _layananAudit.CatatAsync("TAMBAH_PERIODE_PENGGAJIAN", "PeriodePenggajian", $"Membuat periode penggajian baru {baru.NamaPeriode}");
            return (true, "Periode penggajian baru berhasil dibuat.", baru.Id);
        }
    }

    public async Task<(bool Sukses, string Pesan)> HapusPeriodeAsync(Guid id)
    {
        var periode = await _context.PeriodePenggajian.FindAsync(id);
        if (periode == null) return (false, "Periode penggajian tidak ditemukan.");

        if (periode.Status == StatusPeriodePenggajian.Disetujui || periode.Status == StatusPeriodePenggajian.Selesai)
        {
            return (false, "Periode yang sudah disetujui terkunci dan tidak boleh dihapus.");
        }

        _context.PeriodePenggajian.Remove(periode);
        await _context.SaveChangesAsync();

        await _layananAudit.CatatAsync("HAPUS_PERIODE_PENGGAJIAN", "PeriodePenggajian", $"Menghapus periode {periode.NamaPeriode}");
        return (true, "Periode penggajian berhasil dihapus.");
    }

    #endregion

    #region Proses Kalkulasi & Approval Payroll

    public async Task<(bool Sukses, string Pesan, int TotalKaryawanDihitung)> HitungPenggajianBatchAsync(Guid periodeId, Guid? departemenId, Guid userId)
    {
        var periode = await _context.PeriodePenggajian.FindAsync(periodeId);
        if (periode == null) return (false, "Periode penggajian tidak ditemukan.", 0);

        if (periode.Status == StatusPeriodePenggajian.Disetujui || periode.Status == StatusPeriodePenggajian.Selesai)
        {
            return (false, "Periode penggajian ini sudah disetujui dan terkunci. Tidak dapat dihitung ulang.", 0);
        }

        var tenantId = _layananTenant.PerusahaanIdSaatIni;

        // Ambil Kebijakan Penggajian Perusahaan
        var kebijakan = await _context.KebijakanPenggajianPerusahaan.FirstOrDefaultAsync() ?? new KebijakanPenggajianPerusahaan();

        // Ambil daftar karyawan aktif
        var queryKaryawan = _context.Karyawan
            .Where(k => k.StatusKaryawan == StatusKaryawan.Tetap || k.StatusKaryawan == StatusKaryawan.Kontrak);

        if (departemenId.HasValue)
        {
            queryKaryawan = queryKaryawan.Where(k => k.DepartemenId == departemenId.Value);
        }

        var daftarKaryawan = await queryKaryawan.ToListAsync();
        if (!daftarKaryawan.Any())
        {
            return (false, "Tidak ada data karyawan aktif untuk diproses penggajian.", 0);
        }

        // Jalankan Transaksi Database untuk menjamin konsistensi
        using var transaksi = await _context.Database.BeginTransactionAsync();
        try
        {
            int dihitung = 0;
            decimal akumulasiGajiBersih = 0;

            // Query data presensi pada rentang cutoff
            var presensiList = await _context.Kehadiran
                .Where(k => k.Tanggal.Date >= periode.TanggalMulai.Date && k.Tanggal.Date <= periode.TanggalSelesai.Date)
                .ToListAsync();

            // Query lembur yang disetujui pada rentang cutoff
            var lemburList = await _context.PengajuanLembur
                .Where(l => l.TanggalLembur.Date >= periode.TanggalMulai.Date &&
                            l.TanggalLembur.Date <= periode.TanggalSelesai.Date &&
                            l.Status == StatusPengajuanLembur.Disetujui)
                .ToListAsync();

            // Query komponen gaji karyawan
            var semuaGajiKaryawan = await _context.GajiKaryawan
                .Include(g => g.KomponenGaji)
                .Where(g => g.StatusAktif)
                .ToListAsync();

            // Query seluruh komponen master aktif
            var semuaKomponenMaster = await _context.KomponenGaji
                .Where(k => k.StatusAktif)
                .ToListAsync();

            // Ambil nomor urut slip awal
            var countExistingSlip = await _context.Penggajian
                .Where(p => p.PeriodePenggajianId == periodeId)
                .CountAsync();

            int urutanSlip = countExistingSlip + 1;

            foreach (var kry in daftarKaryawan)
            {
                var presensiKry = presensiList.Where(p => p.KaryawanId == kry.Id).ToList();
                var lemburKry = lemburList.Where(l => l.KaryawanId == kry.Id).ToList();
                var gajiKry = semuaGajiKaryawan.Where(g => g.KaryawanId == kry.Id).ToList();

                int jumlahHadir = presensiKry.Count(p => p.Status == StatusKehadiran.Hadir || p.Status == StatusKehadiran.Terlambat);
                int jumlahTerlambat = presensiKry.Count(p => p.Status == StatusKehadiran.Terlambat);
                int totalMenitTerlambat = presensiKry.Sum(p => p.MenitKeterlambatan);
                decimal totalJamLembur = lemburKry.Sum(l => l.TotalJam);

                // Buat list item DTO untuk kalkulator
                var komponenDtoList = new List<ItemGajiKaryawanDto>();

                // Masukkan komponen gaji karyawan
                foreach (var g in gajiKry)
                {
                    if (g.KomponenGaji == null) continue;
                    komponenDtoList.Add(new ItemGajiKaryawanDto
                    {
                        KomponenGajiId = g.KomponenGajiId,
                        KodeKomponen = g.KomponenGaji.KodeKomponen,
                        NamaKomponen = g.KomponenGaji.NamaKomponen,
                        Jenis = g.KomponenGaji.Jenis,
                        Sifat = g.KomponenGaji.Sifat,
                        Kategori = g.KomponenGaji.Kategori,
                        Nominal = g.Nominal,
                        KenaPajak = g.KomponenGaji.KenaPajak,
                        MasukKalkulasiBpjs = g.KomponenGaji.MasukKalkulasiBpjs,
                        DihitungOtomatisOlehSistem = g.KomponenGaji.DihitungOtomatisOlehSistem,
                        Keterangan = g.Keterangan
                    });
                }

                // Masukkan komponen otomatis yang belum ada di profil karyawan agar kalkulator dapat melampirkannya
                foreach (var km in semuaKomponenMaster.Where(x => x.DihitungOtomatisOlehSistem))
                {
                    if (!komponenDtoList.Any(x => x.Kategori == km.Kategori))
                    {
                        komponenDtoList.Add(new ItemGajiKaryawanDto
                        {
                            KomponenGajiId = km.Id,
                            KodeKomponen = km.KodeKomponen,
                            NamaKomponen = km.NamaKomponen,
                            Jenis = km.Jenis,
                            Sifat = km.Sifat,
                            Kategori = km.Kategori,
                            Nominal = 0,
                            KenaPajak = km.KenaPajak,
                            MasukKalkulasiBpjs = km.MasukKalkulasiBpjs,
                            DihitungOtomatisOlehSistem = true
                        });
                    }
                }

                var paramKalkulasi = new ParameterKalkulasiGajiDto
                {
                    KaryawanId = kry.Id,
                    NomorIndukKaryawan = kry.NomorIndukKaryawan,
                    NamaKaryawan = kry.NamaLengkap,
                    StatusPTKP = kry.StatusPTKP,
                    HariKerjaEfektif = 22,
                    JumlahHadir = jumlahHadir > 0 ? jumlahHadir : 22, // Default 22 hari jika belum ada presensi
                    JumlahTerlambat = jumlahTerlambat,
                    TotalMenitTerlambat = totalMenitTerlambat,
                    JumlahJamLembur = totalJamLembur,
                    DaftarKomponenGaji = komponenDtoList,
                    Kebijakan = kebijakan
                };

                var hasil = _kalkulator.HitungGaji(paramKalkulasi);

                // Cek apakah sudah ada record penggajian untuk periode dan karyawan ini (Pencegahan Duplikasi)
                var existingPayroll = await _context.Penggajian
                    .Include(p => p.DaftarDetailPenggajian)
                    .FirstOrDefaultAsync(p => p.PeriodePenggajianId == periodeId && p.KaryawanId == kry.Id);

                if (existingPayroll != null)
                {
                    if (existingPayroll.ApakahTerkunci)
                    {
                        continue; // Lewati jika sudah terkunci
                    }

                    // Hapus detail lama untuk digantikan dengan kalkulasi snapshot baru
                    _context.DetailPenggajian.RemoveRange(existingPayroll.DaftarDetailPenggajian);

                    existingPayroll.StatusPTKP = hasil.StatusPTKP;
                    existingPayroll.MetodePajak = hasil.MetodePajak;
                    existingPayroll.HariKerjaEfektif = hasil.HariKerjaEfektif;
                    existingPayroll.JumlahHadir = hasil.JumlahHadir;
                    existingPayroll.JumlahTerlambat = hasil.JumlahTerlambat;
                    existingPayroll.TotalMenitTerlambat = hasil.TotalMenitTerlambat;
                    existingPayroll.JumlahJamLembur = hasil.JumlahJamLembur;
                    existingPayroll.GajiPokok = hasil.GajiPokok;
                    existingPayroll.TotalTunjanganTetap = hasil.TotalTunjanganTetap;
                    existingPayroll.TotalTunjanganTidakTetap = hasil.TotalTunjanganTidakTetap;
                    existingPayroll.TotalUangLembur = hasil.TotalUangLembur;
                    existingPayroll.TotalPendapatanKotor = hasil.TotalPendapatanKotor;
                    existingPayroll.TotalPotonganBpjs = hasil.TotalPotonganBpjs;
                    existingPayroll.TotalPotonganPph21 = hasil.TotalPotonganPph21;
                    existingPayroll.TotalPotonganKeterlambatan = hasil.TotalPotonganKeterlambatan;
                    existingPayroll.TotalPotonganLainnya = hasil.TotalPotonganLainnya;
                    existingPayroll.TotalPotongan = hasil.TotalPotongan;
                    existingPayroll.GajiBersih = hasil.GajiBersih;
                    existingPayroll.TotalBpjsDitanggungPerusahaan = hasil.TotalBpjsDitanggungPerusahaan;
                    existingPayroll.Status = StatusPenggajian.Dihitung;

                    foreach (var d in hasil.DaftarDetail)
                    {
                        existingPayroll.DaftarDetailPenggajian.Add(new DetailPenggajian
                        {
                            Id = Guid.NewGuid(),
                            PerusahaanId = tenantId ?? Guid.Empty,
                            PenggajianId = existingPayroll.Id,
                            KomponenGajiId = d.KomponenGajiId,
                            KodeKomponen = d.KodeKomponen,
                            NamaKomponen = d.NamaKomponen,
                            Jenis = d.Jenis,
                            Sifat = d.Sifat,
                            Kategori = d.Kategori,
                            Nominal = d.Nominal,
                            NominalDitanggungPerusahaan = d.NominalDitanggungPerusahaan,
                            Keterangan = d.Keterangan,
                            Urutan = d.Urutan
                        });
                    }

                    _context.Penggajian.Update(existingPayroll);
                    akumulasiGajiBersih += existingPayroll.GajiBersih;
                }
                else
                {
                    var nomorSlip = $"SLIP/{periode.Tahun}{periode.Bulan:D2}/{urutanSlip++:D4}";

                    var baruPayroll = new Penggajian
                    {
                        Id = Guid.NewGuid(),
                        PerusahaanId = tenantId ?? Guid.Empty,
                        PeriodePenggajianId = periodeId,
                        KaryawanId = kry.Id,
                        NomorSlipGaji = nomorSlip,
                        StatusPTKP = hasil.StatusPTKP,
                        MetodePajak = hasil.MetodePajak,
                        HariKerjaEfektif = hasil.HariKerjaEfektif,
                        JumlahHadir = hasil.JumlahHadir,
                        JumlahTerlambat = hasil.JumlahTerlambat,
                        TotalMenitTerlambat = hasil.TotalMenitTerlambat,
                        JumlahJamLembur = hasil.JumlahJamLembur,
                        GajiPokok = hasil.GajiPokok,
                        TotalTunjanganTetap = hasil.TotalTunjanganTetap,
                        TotalTunjanganTidakTetap = hasil.TotalTunjanganTidakTetap,
                        TotalUangLembur = hasil.TotalUangLembur,
                        TotalPendapatanKotor = hasil.TotalPendapatanKotor,
                        TotalPotonganBpjs = hasil.TotalPotonganBpjs,
                        TotalPotonganPph21 = hasil.TotalPotonganPph21,
                        TotalPotonganKeterlambatan = hasil.TotalPotonganKeterlambatan,
                        TotalPotonganLainnya = hasil.TotalPotonganLainnya,
                        TotalPotongan = hasil.TotalPotongan,
                        GajiBersih = hasil.GajiBersih,
                        TotalBpjsDitanggungPerusahaan = hasil.TotalBpjsDitanggungPerusahaan,
                        Status = StatusPenggajian.Dihitung,
                        ApakahTerkunci = false
                    };

                    foreach (var d in hasil.DaftarDetail)
                    {
                        baruPayroll.DaftarDetailPenggajian.Add(new DetailPenggajian
                        {
                            Id = Guid.NewGuid(),
                            PerusahaanId = tenantId ?? Guid.Empty,
                            PenggajianId = baruPayroll.Id,
                            KomponenGajiId = d.KomponenGajiId,
                            KodeKomponen = d.KodeKomponen,
                            NamaKomponen = d.NamaKomponen,
                            Jenis = d.Jenis,
                            Sifat = d.Sifat,
                            Kategori = d.Kategori,
                            Nominal = d.Nominal,
                            NominalDitanggungPerusahaan = d.NominalDitanggungPerusahaan,
                            Keterangan = d.Keterangan,
                            Urutan = d.Urutan
                        });
                    }

                    await _context.Penggajian.AddAsync(baruPayroll);
                    akumulasiGajiBersih += baruPayroll.GajiBersih;
                }

                dihitung++;
            }

            // Perbarui summary periode
            periode.Status = StatusPeriodePenggajian.Diproses;
            periode.TotalKaryawan = dihitung;
            periode.TotalPengeluaranGaji = akumulasiGajiBersih;
            _context.PeriodePenggajian.Update(periode);

            await _context.SaveChangesAsync();
            await transaksi.CommitAsync();

            await _layananAudit.CatatAsync("HITUNG_GAJI_BATCH", "PeriodePenggajian", $"Kalkulasi batch payroll periode {periode.NamaPeriode} berhasil dihitung untuk {dihitung} karyawan.");
            return (true, $"Kalkulasi penggajian berhasil diproses untuk {dihitung} karyawan.", dihitung);
        }
        catch (Exception ex)
        {
            await transaksi.RollbackAsync();
            return (false, $"Gagal menghitung penggajian batch: {ex.Message}", 0);
        }
    }

    public async Task<(bool Sukses, string Pesan)> SetujuiPenggajianPeriodeAsync(Guid periodeId, Guid userId, string? catatan)
    {
        var periode = await _context.PeriodePenggajian.FindAsync(periodeId);
        if (periode == null) return (false, "Periode penggajian tidak ditemukan.");

        if (periode.Status == StatusPeriodePenggajian.Disetujui || periode.Status == StatusPeriodePenggajian.Selesai)
        {
            return (false, "Periode penggajian ini sudah disetujui sebelumnya.");
        }

        var daftarPayroll = await _context.Penggajian
            .Where(p => p.PeriodePenggajianId == periodeId)
            .ToListAsync();

        if (!daftarPayroll.Any())
        {
            return (false, "Periode ini belum memiliki data kalkulasi penggajian. Silakan hitung gaji terlebih dahulu.");
        }

        var kebijakan = await _context.KebijakanPenggajianPerusahaan.FirstOrDefaultAsync() ?? new KebijakanPenggajianPerusahaan();
        var tenantId = _layananTenant.PerusahaanIdSaatIni;

        using var transaksi = await _context.Database.BeginTransactionAsync();
        try
        {
            var waktuSekarang = DateTimeOffset.UtcNow;

            foreach (var p in daftarPayroll)
            {
                p.Status = StatusPenggajian.Disetujui;
                p.ApakahTerkunci = true;
                p.DisetujuiOlehId = userId;
                p.WaktuDisetujui = waktuSekarang;
                _context.Penggajian.Update(p);

                // Terbitkan Slip Gaji resmi dengan kode verifikasi unik
                var existingSlip = await _context.SlipGaji.FirstOrDefaultAsync(s => s.PenggajianId == p.Id);
                if (existingSlip == null)
                {
                    var rawToken = $"{p.Id}_{p.NomorSlipGaji}_{waktuSekarang.Ticks}";
                    var hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(rawToken));
                    var kodeVerifikasi = Convert.ToHexString(hashBytes)[..16].ToUpper();

                    await _context.SlipGaji.AddAsync(new SlipGaji
                    {
                        Id = Guid.NewGuid(),
                        PerusahaanId = tenantId ?? Guid.Empty,
                        PenggajianId = p.Id,
                        NomorSlipGaji = p.NomorSlipGaji,
                        KodeVerifikasi = kodeVerifikasi,
                        WaktuDiterbitkan = waktuSekarang,
                        DiterbitkanOlehId = userId,
                        CatatanKaki = kebijakan.CatatanSlipGajiDefault
                    });
                }
            }

            periode.Status = StatusPeriodePenggajian.Disetujui;
            periode.DisetujuiOlehId = userId;
            periode.WaktuDisetujui = waktuSekarang;
            if (!string.IsNullOrEmpty(catatan))
            {
                periode.Catatan = (periode.Catatan != null ? periode.Catatan + " | " : "") + catatan;
            }

            _context.PeriodePenggajian.Update(periode);
            await _context.SaveChangesAsync();
            await transaksi.CommitAsync();

            await _layananAudit.CatatAsync("SETUJUI_PAYROLL", "PeriodePenggajian", $"Menyetujui dan mengunci seluruh payroll periode {periode.NamaPeriode} ({daftarPayroll.Count} slip gaji diterbitkan).");
            return (true, $"Periode penggajian {periode.NamaPeriode} berhasil disetujui. Seluruh payroll telah dikunci dan slip gaji resmi diterbitkan.");
        }
        catch (Exception ex)
        {
            await transaksi.RollbackAsync();
            return (false, $"Gagal menyetujui payroll: {ex.Message}");
        }
    }

    #endregion

    #region Detail & Slip Gaji

    public async Task<RincianPenggajianViewModel?> DapatkanRincianPenggajianAsync(Guid penggajianId)
    {
        var p = await _context.Penggajian
            .Include(x => x.PeriodePenggajian)
            .Include(x => x.Karyawan!)
                .ThenInclude(k => k.Departemen)
            .Include(x => x.Karyawan!)
                .ThenInclude(k => k.Jabatan)
            .Include(x => x.DaftarDetailPenggajian)
            .FirstOrDefaultAsync(x => x.Id == penggajianId);

        if (p == null) return null;

        var pendapatan = p.DaftarDetailPenggajian
            .Where(d => d.Jenis == JenisKomponenGaji.Pendapatan)
            .OrderBy(d => d.Urutan)
            .Select(d => new BarisDetailPenggajianViewModel
            {
                KodeKomponen = d.KodeKomponen,
                NamaKomponen = d.NamaKomponen,
                Nominal = d.Nominal,
                NominalDitanggungPerusahaan = d.NominalDitanggungPerusahaan,
                Keterangan = d.Keterangan
            })
            .ToList();

        var potongan = p.DaftarDetailPenggajian
            .Where(d => d.Jenis == JenisKomponenGaji.Potongan)
            .OrderBy(d => d.Urutan)
            .Select(d => new BarisDetailPenggajianViewModel
            {
                KodeKomponen = d.KodeKomponen,
                NamaKomponen = d.NamaKomponen,
                Nominal = d.Nominal,
                NominalDitanggungPerusahaan = d.NominalDitanggungPerusahaan,
                Keterangan = d.Keterangan
            })
            .ToList();

        return new RincianPenggajianViewModel
        {
            PenggajianId = p.Id,
            PeriodePenggajianId = p.PeriodePenggajianId,
            NamaPeriode = p.PeriodePenggajian?.NamaPeriode ?? "-",
            NomorSlipGaji = p.NomorSlipGaji,
            TanggalPembayaran = p.PeriodePenggajian?.TanggalPembayaran ?? DateTime.Today,
            KaryawanId = p.KaryawanId,
            NomorIndukKaryawan = p.Karyawan?.NomorIndukKaryawan ?? string.Empty,
            NamaKaryawan = p.Karyawan?.NamaLengkap ?? string.Empty,
            NamaDepartemen = p.Karyawan?.Departemen?.NamaDepartemen ?? "-",
            NamaJabatan = p.Karyawan?.Jabatan?.NamaJabatan ?? "-",
            NamaBank = p.Karyawan?.NamaBank,
            NomorRekeningBank = p.Karyawan?.NomorRekeningBank,
            NomorNPWP = p.Karyawan?.NomorNPWP,
            StatusPTKP = p.StatusPTKP,
            MetodePajak = p.MetodePajak,
            HariKerjaEfektif = p.HariKerjaEfektif,
            JumlahHadir = p.JumlahHadir,
            JumlahTerlambat = p.JumlahTerlambat,
            TotalMenitTerlambat = p.TotalMenitTerlambat,
            JumlahJamLembur = p.JumlahJamLembur,
            GajiPokok = p.GajiPokok,
            TotalTunjanganTetap = p.TotalTunjanganTetap,
            TotalTunjanganTidakTetap = p.TotalTunjanganTidakTetap,
            TotalUangLembur = p.TotalUangLembur,
            TotalPendapatanKotor = p.TotalPendapatanKotor,
            TotalPotonganBpjs = p.TotalPotonganBpjs,
            TotalPotonganPph21 = p.TotalPotonganPph21,
            TotalPotonganKeterlambatan = p.TotalPotonganKeterlambatan,
            TotalPotonganLainnya = p.TotalPotonganLainnya,
            TotalPotongan = p.TotalPotongan,
            GajiBersih = p.GajiBersih,
            TotalBpjsDitanggungPerusahaan = p.TotalBpjsDitanggungPerusahaan,
            Status = p.Status,
            ApakahTerkunci = p.ApakahTerkunci,
            Catatan = p.Catatan,
            DaftarPendapatan = pendapatan,
            DaftarPotongan = potongan
        };
    }

    public async Task<SlipGajiViewModel?> DapatkanSlipGajiAsync(Guid penggajianId, Guid? karyawanIdPengguna = null)
    {
        var p = await _context.Penggajian
            .Include(x => x.PeriodePenggajian)
            .Include(x => x.Karyawan!)
                .ThenInclude(k => k.Departemen)
            .Include(x => x.Karyawan!)
                .ThenInclude(k => k.Jabatan)
            .Include(x => x.DaftarDetailPenggajian)
            .Include(x => x.SlipGaji)
            .FirstOrDefaultAsync(x => x.Id == penggajianId);

        if (p == null) return null;

        // Otorisasi karyawan jika ESS: tidak boleh membuka slip milik rekan lain
        if (karyawanIdPengguna.HasValue && p.KaryawanId != karyawanIdPengguna.Value)
        {
            return null;
        }

        // Tandai sudah dilihat jika karyawan yang membuka
        if (karyawanIdPengguna.HasValue && p.SlipGaji != null && !p.SlipGaji.SudahDilihatKaryawan)
        {
            p.SlipGaji.SudahDilihatKaryawan = true;
            p.SlipGaji.WaktuDilihatKaryawan = DateTimeOffset.UtcNow;
            _context.SlipGaji.Update(p.SlipGaji);
            await _context.SaveChangesAsync();
        }

        var perusahaan = await _context.Perusahaan.FirstOrDefaultAsync(pr => pr.Id == p.PerusahaanId);

        var pendapatan = p.DaftarDetailPenggajian
            .Where(d => d.Jenis == JenisKomponenGaji.Pendapatan)
            .OrderBy(d => d.Urutan)
            .Select(d => new BarisDetailPenggajianViewModel
            {
                KodeKomponen = d.KodeKomponen,
                NamaKomponen = d.NamaKomponen,
                Nominal = d.Nominal,
                Keterangan = d.Keterangan
            })
            .ToList();

        var potongan = p.DaftarDetailPenggajian
            .Where(d => d.Jenis == JenisKomponenGaji.Potongan)
            .OrderBy(d => d.Urutan)
            .Select(d => new BarisDetailPenggajianViewModel
            {
                KodeKomponen = d.KodeKomponen,
                NamaKomponen = d.NamaKomponen,
                Nominal = d.Nominal,
                Keterangan = d.Keterangan
            })
            .ToList();

        return new SlipGajiViewModel
        {
            PenggajianId = p.Id,
            NomorSlipGaji = p.NomorSlipGaji,
            KodeVerifikasi = p.SlipGaji?.KodeVerifikasi ?? "VERIF-" + Guid.NewGuid().ToString()[..8].ToUpper(),
            WaktuDiterbitkan = p.SlipGaji?.WaktuDiterbitkan ?? DateTimeOffset.UtcNow,
            NamaPerusahaan = perusahaan?.NamaPerusahaan ?? "SistemHRD Enterprise",
            AlamatPerusahaan = perusahaan?.Alamat,
            TeleponPerusahaan = perusahaan?.NomorTelepon,
            NPWPPerusahaan = perusahaan?.NPWP,
            NamaPeriode = p.PeriodePenggajian?.NamaPeriode ?? "-",
            Bulan = p.PeriodePenggajian?.Bulan ?? DateTime.Today.Month,
            Tahun = p.PeriodePenggajian?.Tahun ?? DateTime.Today.Year,
            TanggalMulaiCutoff = p.PeriodePenggajian?.TanggalMulai ?? DateTime.Today,
            TanggalSelesaiCutoff = p.PeriodePenggajian?.TanggalSelesai ?? DateTime.Today,
            TanggalPembayaran = p.PeriodePenggajian?.TanggalPembayaran ?? DateTime.Today,
            NomorIndukKaryawan = p.Karyawan?.NomorIndukKaryawan ?? string.Empty,
            NamaKaryawan = p.Karyawan?.NamaLengkap ?? string.Empty,
            NamaDepartemen = p.Karyawan?.Departemen?.NamaDepartemen ?? "-",
            NamaJabatan = p.Karyawan?.Jabatan?.NamaJabatan ?? "-",
            NamaBank = p.Karyawan?.NamaBank,
            NomorRekeningBank = p.Karyawan?.NomorRekeningBank,
            NomorNPWP = p.Karyawan?.NomorNPWP,
            StatusPTKP = p.StatusPTKP,
            HariKerjaEfektif = p.HariKerjaEfektif,
            JumlahHadir = p.JumlahHadir,
            JumlahTerlambat = p.JumlahTerlambat,
            JumlahJamLembur = p.JumlahJamLembur,
            DaftarPendapatan = pendapatan,
            DaftarPotongan = potongan,
            TotalPendapatanKotor = p.TotalPendapatanKotor,
            TotalPotongan = p.TotalPotongan,
            GajiBersih = p.GajiBersih,
            TerbilangGajiBersih = HelperTerbilang.Konversi(p.GajiBersih),
            CatatanKaki = p.SlipGaji?.CatatanKaki ?? "Slip gaji ini diterbitkan otomatis oleh SistemHRD."
        };
    }

    public async Task<List<ItemPayrollKaryawanViewModel>> DapatkanRiwayatPenggajianKaryawanAsync(Guid karyawanId)
    {
        return await _context.Penggajian
            .Include(p => p.PeriodePenggajian)
            .Include(p => p.Karyawan!)
                .ThenInclude(k => k.Departemen)
            .Include(p => p.Karyawan!)
                .ThenInclude(k => k.Jabatan)
            .Where(p => p.KaryawanId == karyawanId && (p.Status == StatusPenggajian.Disetujui || p.Status == StatusPenggajian.Dibayarkan))
            .OrderByDescending(p => p.PeriodePenggajian != null ? p.PeriodePenggajian.Tahun : 0)
            .ThenByDescending(p => p.PeriodePenggajian != null ? p.PeriodePenggajian.Bulan : 0)
            .Select(p => new ItemPayrollKaryawanViewModel
            {
                PenggajianId = p.Id,
                KaryawanId = p.KaryawanId,
                NomorSlipGaji = p.NomorSlipGaji,
                NomorIndukKaryawan = p.Karyawan != null ? p.Karyawan.NomorIndukKaryawan : string.Empty,
                NamaKaryawan = p.Karyawan != null ? p.Karyawan.NamaLengkap : string.Empty,
                NamaDepartemen = p.Karyawan != null && p.Karyawan.Departemen != null ? p.Karyawan.Departemen.NamaDepartemen : "-",
                NamaJabatan = p.Karyawan != null && p.Karyawan.Jabatan != null ? p.Karyawan.Jabatan.NamaJabatan : "-",
                GajiPokok = p.GajiPokok,
                TotalPendapatanKotor = p.TotalPendapatanKotor,
                TotalPotongan = p.TotalPotongan,
                GajiBersih = p.GajiBersih,
                Status = p.Status,
                ApakahTerkunci = p.ApakahTerkunci
            })
            .ToListAsync();
    }

    #endregion

    #region Ekspor Data

    public async Task<(byte[]? FileBytes, string NamaFile)> EksporLaporanPayrollCsvAsync(Guid periodeId)
    {
        var periode = await _context.PeriodePenggajian.FindAsync(periodeId);
        if (periode == null) return (null, string.Empty);

        var daftarPayroll = await _context.Penggajian
            .Include(p => p.Karyawan!)
                .ThenInclude(k => k.Departemen)
            .Include(p => p.Karyawan!)
                .ThenInclude(k => k.Jabatan)
            .Where(p => p.PeriodePenggajianId == periodeId)
            .OrderBy(p => p.Karyawan != null ? p.Karyawan.NamaLengkap : string.Empty)
            .ToListAsync();

        var sb = new StringBuilder();
        // Header CSV
        sb.AppendLine("No,Nomor Slip Gaji,NIK,Nama Karyawan,Departemen,Jabatan,Bank,Nomor Rekening,Status PTKP,Gaji Pokok,Total Tunjangan,Uang Lembur,Pendapatan Kotor,Potongan BPJS,Potongan PPh 21,Potongan Terlambat,Potongan Lain,Total Potongan,Gaji Bersih");

        int no = 1;
        foreach (var p in daftarPayroll)
        {
            var namaDept = (p.Karyawan?.Departemen?.NamaDepartemen ?? "-").Replace(",", " ");
            var namaJab = (p.Karyawan?.Jabatan?.NamaJabatan ?? "-").Replace(",", " ");
            var namaKry = (p.Karyawan?.NamaLengkap ?? string.Empty).Replace(",", " ");
            var bank = (p.Karyawan?.NamaBank ?? "-").Replace(",", " ");
            var rek = (p.Karyawan?.NomorRekeningBank ?? "-").Replace(",", " ");
            var totalTj = p.TotalTunjanganTetap + p.TotalTunjanganTidakTetap;

            sb.AppendLine($"{no++},{p.NomorSlipGaji},{p.Karyawan?.NomorIndukKaryawan},{namaKry},{namaDept},{namaJab},{bank},{rek},{p.StatusPTKP},{p.GajiPokok:F2},{totalTj:F2},{p.TotalUangLembur:F2},{p.TotalPendapatanKotor:F2},{p.TotalPotonganBpjs:F2},{p.TotalPotonganPph21:F2},{p.TotalPotonganKeterlambatan:F2},{p.TotalPotonganLainnya:F2},{p.TotalPotongan:F2},{p.GajiBersih:F2}");
        }

        var namaFile = $"Rekap_Payroll_{periode.Tahun}_{periode.Bulan:D2}.csv";
        var bytes = Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(sb.ToString())).ToArray();

        return (bytes, namaFile);
    }

    #endregion
}
