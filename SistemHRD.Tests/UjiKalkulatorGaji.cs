using SistemHRD.Models.Entitas;
using SistemHRD.Models.Enums;
using SistemHRD.Services.Implementasi;
using SistemHRD.Services.Interfaces;
using Xunit;

namespace SistemHRD.Tests;

/// <summary>
/// Unit test komprehensif untuk pengujian perhitungan gaji dengan berbagai skenario regulasi Indonesia.
/// </summary>
public class UjiKalkulatorGaji
{
    private readonly ILayananKalkulatorGaji _kalkulator;

    public UjiKalkulatorGaji()
    {
        _kalkulator = new LayananKalkulatorGaji();
    }

    [Fact]
    public void UjiKalkulasiGajiPokokDanTunjanganTetap_MenghasilkanPendapatanKotorAkurat()
    {
        // Skenario: Karyawan dengan Gaji Pokok 8jt + Tunjangan Jabatan 1.5jt + Transport 500rb
        var param = new ParameterKalkulasiGajiDto
        {
            KaryawanId = Guid.NewGuid(),
            StatusPTKP = StatusPTKP.TK0,
            JumlahJamLembur = 0,
            TotalMenitTerlambat = 0,
            Kebijakan = new KebijakanPenggajianPerusahaan
            {
                TerapkanBpjsKesehatan = false,
                TerapkanBpjsKetenagakerjaan = false,
                TerapkanPph21 = false
            },
            DaftarKomponenGaji = new List<ItemGajiKaryawanDto>
            {
                new() { KodeKomponen = "GP", NamaKomponen = "Gaji Pokok", Jenis = JenisKomponenGaji.Pendapatan, Sifat = SifatKomponenGaji.Tetap, Kategori = KategoriKomponenGaji.GajiPokok, Nominal = 8000000m },
                new() { KodeKomponen = "TJ-JAB", NamaKomponen = "Tunjangan Jabatan", Jenis = JenisKomponenGaji.Pendapatan, Sifat = SifatKomponenGaji.Tetap, Kategori = KategoriKomponenGaji.TunjanganTetap, Nominal = 1500000m },
                new() { KodeKomponen = "TJ-TRN", NamaKomponen = "Tunjangan Transportasi", Jenis = JenisKomponenGaji.Pendapatan, Sifat = SifatKomponenGaji.Tetap, Kategori = KategoriKomponenGaji.TunjanganTetap, Nominal = 500000m }
            }
        };

        var hasil = _kalkulator.HitungGaji(param);

        Assert.Equal(8000000m, hasil.GajiPokok);
        Assert.Equal(2000000m, hasil.TotalTunjanganTetap);
        Assert.Equal(10000000m, hasil.TotalPendapatanKotor);
        Assert.Equal(10000000m, hasil.GajiBersih);
    }

    [Fact]
    public void UjiKalkulasiLemburStandarKemnaker_FormulaSesuaiPeraturanPemerintah()
    {
        // Gaji Pokok 8.000.000 + Tunjangan Tetap 2.000.000 = 10.000.000
        // Upah Sejam = 10.000.000 / 173 = 57.803,468...
        // 10 Jam Lembur = 578.034,68
        var lembur = _kalkulator.HitungUpahLembur(8000000m, 2000000m, 10m, tarifFlat: 0);

        Assert.True(lembur > 578000m && lembur < 578100m);
    }

    [Fact]
    public void UjiKalkulasiLemburTarifFlat_MengikutiKonfigurasiKebijakanPerusahaan()
    {
        // Tarif flat 50.000 / jam x 5 jam = 250.000
        var lembur = _kalkulator.HitungUpahLembur(8000000m, 0m, 5m, tarifFlat: 50000m);

        Assert.Equal(250000m, lembur);
    }

    [Fact]
    public void UjiDendaKeterlambatan_ToleransiMengurangiPotonganSecaraTepat()
    {
        // Keterlambatan 45 menit, toleransi 15 menit, denda 1.000 / menit
        // Menit kena denda = 45 - 15 = 30 menit -> Rp 30.000
        var denda = _kalkulator.HitungDendaKeterlambatan(45, 15, 1000m);
        Assert.Equal(30000m, denda);

        // Keterlambatan di bawah toleransi (10 menit, toleransi 15 menit) -> 0
        var dendaNol = _kalkulator.HitungDendaKeterlambatan(10, 15, 1000m);
        Assert.Equal(0m, dendaNol);
    }

    [Fact]
    public void UjiKalkulasiBpjsKesehatan_BatasAtasUpahDiterapkan()
    {
        var kebijakan = new KebijakanPenggajianPerusahaan
        {
            TerapkanBpjsKesehatan = true,
            PersenBpjsKesehatanPekerja = 1.00m,
            PersenBpjsKesehatanPerusahaan = 4.00m,
            BatasMaksimalUpahBpjsKesehatan = 12000000m
        };

        // Kasus 1: Upah 8.000.000 (di bawah cap)
        var (pek1, per1) = _kalkulator.HitungBpjsKesehatan(8000000m, kebijakan);
        Assert.Equal(80000m, pek1);   // 1% x 8jt
        Assert.Equal(320000m, per1);  // 4% x 8jt

        // Kasus 2: Upah 20.000.000 (di atas cap 12jt)
        var (pek2, per2) = _kalkulator.HitungBpjsKesehatan(20000000m, kebijakan);
        Assert.Equal(120000m, pek2);  // 1% x 12jt max cap
        Assert.Equal(480000m, per2);  // 4% x 12jt max cap
    }

    [Fact]
    public void UjiKalkulasiBpjsKetenagakerjaan_JhtDanJpAkuratDenganBatasPensiun()
    {
        var kebijakan = new KebijakanPenggajianPerusahaan
        {
            TerapkanBpjsKetenagakerjaan = true,
            PersenBpjsJhtPekerja = 2.00m,
            PersenBpjsJhtPerusahaan = 3.70m,
            PersenBpjsJpPekerja = 1.00m,
            PersenBpjsJpPerusahaan = 2.00m,
            BatasMaksimalUpahBpjsJp = 10042300m,
            PersenBpjsJkkPerusahaan = 0.24m,
            PersenBpjsJkmPerusahaan = 0.30m
        };

        // Upah 15.000.000
        var (jhtPek, jhtPer, jpPek, jpPer, jkkPer, jkmPer) = _kalkulator.HitungBpjsKetenagakerjaan(15000000m, kebijakan);

        // JHT tanpa batas cap: 15jt x 2% = 300rb
        Assert.Equal(300000m, jhtPek);
        Assert.Equal(555000m, jhtPer); // 15jt x 3.7%

        // JP dibatasi 10.042.300: 10.042.300 x 1% = 100.423
        Assert.Equal(100423m, jpPek);
        Assert.Equal(200846m, jpPer); // 10.042.300 x 2% = 200.846

        // JKK: 15jt x 0.24% = 36.000
        Assert.Equal(36000m, jkkPer);
        // JKM: 15jt x 0.30% = 45.000
        Assert.Equal(45000m, jkmPer);
    }

    [Fact]
    public void UjiKalkulasiPph21_TarifEfektifRataRataPP58KategoriASesuaiTier()
    {
        // PTKP TK/0 -> Kategori TER A
        // Bruto Rp 8.000.000 -> Rentang > 7.500.000 s/d 8.550.000 -> Tarif 1.50%
        // Pajak = 8.000.000 x 1.50% = 120.000
        var pph = _kalkulator.HitungPph21Bulanan(8000000m, StatusPTKP.TK0, MetodePajakPPh21.TER_PP58);
        Assert.Equal(120000m, pph);

        // Bruto Rp 5.000.000 (di bawah PTKP bulanan Rp 5.400.000) -> Tarif 0% -> Pajak Rp 0
        var pphNol = _kalkulator.HitungPph21Bulanan(5000000m, StatusPTKP.TK0, MetodePajakPPh21.TER_PP58);
        Assert.Equal(0m, pphNol);
    }

    [Fact]
    public void UjiKalkulasiPph21_TarifEfektifRataRataPP58KategoriBSesuaiTier()
    {
        // PTKP K/1 -> Kategori TER B
        // Bruto Rp 10.000.000 -> Rentang > 9.200.000 s/d 10.750.000 -> Tarif 1.50%
        // Pajak = 10.000.000 x 1.50% = 150.000
        var pph = _kalkulator.HitungPph21Bulanan(10000000m, StatusPTKP.K1, MetodePajakPPh21.TER_PP58);
        Assert.Equal(150000m, pph);
    }

    [Fact]
    public void UjiKalkulasiGajiLengkap_EndToEndPayrollTakeHomePay()
    {
        // Skenario Lengkap:
        // Karyawan Rian Pratama (TK/0)
        // GP = 8.000.000, TJ-JAB = 1.500.000, TJ-TRN = 500.000
        // Total Bruto Kas = 10.000.000
        // Basis BPJS = 9.500.000 (GP + TJ-JAB)
        // BPJS Kes Pekerja (1%) = 95.000
        // BPJS JHT Pekerja (2%) = 190.000
        // BPJS JP Pekerja (1%) = 95.000
        // Total Potongan BPJS = 380.000
        // Denda telat 30 menit (toleransi 15 mnt, denda 1000/mnt) = 15.000
        var param = new ParameterKalkulasiGajiDto
        {
            KaryawanId = Guid.NewGuid(),
            StatusPTKP = StatusPTKP.TK0,
            JumlahJamLembur = 0,
            TotalMenitTerlambat = 30,
            Kebijakan = new KebijakanPenggajianPerusahaan
            {
                TerapkanBpjsKesehatan = true,
                TerapkanBpjsKetenagakerjaan = true,
                TerapkanPph21 = true,
                MetodePph21 = MetodePajakPPh21.TER_PP58,
                TerapkanPotonganKeterlambatan = true,
                ToleransiKeterlambatanGratisMenit = 15,
                DendaKeterlambatanPerMenit = 1000
            },
            DaftarKomponenGaji = new List<ItemGajiKaryawanDto>
            {
                new() { KodeKomponen = "GP", NamaKomponen = "Gaji Pokok", Jenis = JenisKomponenGaji.Pendapatan, Sifat = SifatKomponenGaji.Tetap, Kategori = KategoriKomponenGaji.GajiPokok, Nominal = 8000000m, MasukKalkulasiBpjs = true, KenaPajak = true },
                new() { KodeKomponen = "TJ-JAB", NamaKomponen = "Tunjangan Jabatan", Jenis = JenisKomponenGaji.Pendapatan, Sifat = SifatKomponenGaji.Tetap, Kategori = KategoriKomponenGaji.TunjanganTetap, Nominal = 1500000m, MasukKalkulasiBpjs = true, KenaPajak = true },
                new() { KodeKomponen = "TJ-TRN", NamaKomponen = "Tunjangan Transportasi", Jenis = JenisKomponenGaji.Pendapatan, Sifat = SifatKomponenGaji.Tetap, Kategori = KategoriKomponenGaji.TunjanganTetap, Nominal = 500000m, MasukKalkulasiBpjs = false, KenaPajak = true }
            }
        };

        var hasil = _kalkulator.HitungGaji(param);

        Assert.Equal(10000000m, hasil.TotalPendapatanKotor);
        Assert.Equal(380000m, hasil.TotalPotonganBpjs);
        Assert.Equal(15000m, hasil.TotalPotonganKeterlambatan);
        Assert.True(hasil.TotalPotonganPph21 > 0);
        Assert.True(hasil.GajiBersih > 0 && hasil.GajiBersih < hasil.TotalPendapatanKotor);
        Assert.Equal(hasil.TotalPendapatanKotor - hasil.TotalPotongan, hasil.GajiBersih);
        Assert.True(hasil.DaftarDetail.Count >= 6); // Rincian snapshot tersimpan
    }
}
