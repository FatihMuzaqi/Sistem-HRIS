using SistemHRD.Models.Entitas;
using SistemHRD.Models.Enums;
using SistemHRD.Services.Interfaces;

namespace SistemHRD.Services.Implementasi;

/// <summary>
/// Implementasi mesin kalkulator gaji standar regulasi ketenagakerjaan dan perpajakan Indonesia (PP 58/2023 & BPJS).
/// </summary>
public class LayananKalkulatorGaji : ILayananKalkulatorGaji
{
    public HasilKalkulasiGajiDto HitungGaji(ParameterKalkulasiGajiDto parameter)
    {
        var hasil = new HasilKalkulasiGajiDto
        {
            KaryawanId = parameter.KaryawanId,
            StatusPTKP = parameter.StatusPTKP,
            MetodePajak = parameter.Kebijakan.MetodePph21,
            HariKerjaEfektif = parameter.HariKerjaEfektif,
            JumlahHadir = parameter.JumlahHadir,
            JumlahTerlambat = parameter.JumlahTerlambat,
            TotalMenitTerlambat = parameter.TotalMenitTerlambat,
            JumlahJamLembur = parameter.JumlahJamLembur
        };

        var detailSnapshot = new List<BarisDetailPenggajianDto>();
        int urutan = 1;

        // 1. Ekstraksi Komponen Tetap & Variabel Karyawan
        decimal gajiPokok = 0;
        decimal totalTunjanganTetap = 0;
        decimal totalTunjanganTidakTetap = 0;
        decimal basisUpahBpjs = 0;
        decimal brutoKenaPajakDasar = 0;

        foreach (var k in parameter.DaftarKomponenGaji.Where(x => x.Jenis == JenisKomponenGaji.Pendapatan && !x.DihitungOtomatisOlehSistem))
        {
            if (k.Kategori == KategoriKomponenGaji.GajiPokok)
            {
                gajiPokok += k.Nominal;
            }
            else if (k.Sifat == SifatKomponenGaji.Tetap)
            {
                totalTunjanganTetap += k.Nominal;
            }
            else
            {
                totalTunjanganTidakTetap += k.Nominal;
            }

            if (k.MasukKalkulasiBpjs)
            {
                basisUpahBpjs += k.Nominal;
            }

            if (k.KenaPajak)
            {
                brutoKenaPajakDasar += k.Nominal;
            }

            detailSnapshot.Add(new BarisDetailPenggajianDto
            {
                KomponenGajiId = k.KomponenGajiId,
                KodeKomponen = k.KodeKomponen,
                NamaKomponen = k.NamaKomponen,
                Jenis = k.Jenis,
                Sifat = k.Sifat,
                Kategori = k.Kategori,
                Nominal = k.Nominal,
                Keterangan = k.Keterangan ?? "Komponen pendapatan rutin karyawan",
                Urutan = urutan++
            });
        }

        hasil.GajiPokok = gajiPokok;
        hasil.TotalTunjanganTetap = totalTunjanganTetap;
        hasil.TotalTunjanganTidakTetap = totalTunjanganTidakTetap;

        // 2. Kalkulasi Upah Lembur (Berdasarkan Jam Lembur Terverifikasi)
        decimal uangLembur = 0;
        if (parameter.Kebijakan.HitungLemburOtomatis && parameter.JumlahJamLembur > 0)
        {
            uangLembur = HitungUpahLembur(gajiPokok, totalTunjanganTetap, parameter.JumlahJamLembur, parameter.Kebijakan.TarifLemburPerJamFlat);
            hasil.TotalUangLembur = uangLembur;
            brutoKenaPajakDasar += uangLembur;

            var lemburKomponen = parameter.DaftarKomponenGaji.FirstOrDefault(x => x.Kategori == KategoriKomponenGaji.UangLembur);
            detailSnapshot.Add(new BarisDetailPenggajianDto
            {
                KomponenGajiId = lemburKomponen?.KomponenGajiId,
                KodeKomponen = lemburKomponen?.KodeKomponen ?? "LMBR",
                NamaKomponen = lemburKomponen?.NamaKomponen ?? "Upah Lembur",
                Jenis = JenisKomponenGaji.Pendapatan,
                Sifat = SifatKomponenGaji.TidakTetap,
                Kategori = KategoriKomponenGaji.UangLembur,
                Nominal = uangLembur,
                Keterangan = $"Upah lembur sebanyak {parameter.JumlahJamLembur} jam kerja",
                Urutan = urutan++
            });
        }

        // Total Pendapatan Kotor
        hasil.TotalPendapatanKotor = gajiPokok + totalTunjanganTetap + totalTunjanganTidakTetap + uangLembur;

        // 3. Kalkulasi BPJS Kesehatan & BPJS Ketenagakerjaan
        var (bpjsKesPekerja, bpjsKesPerusahaan) = HitungBpjsKesehatan(basisUpahBpjs, parameter.Kebijakan);
        var (jhtPekerja, jhtPerusahaan, jpPekerja, jpPerusahaan, jkkPerusahaan, jkmPerusahaan) = HitungBpjsKetenagakerjaan(basisUpahBpjs, parameter.Kebijakan);

        hasil.TotalPotonganBpjs = bpjsKesPekerja + jhtPekerja + jpPekerja;
        hasil.TotalBpjsDitanggungPerusahaan = bpjsKesPerusahaan + jhtPerusahaan + jpPerusahaan + jkkPerusahaan + jkmPerusahaan;

        // Snapshot Baris Potongan BPJS Pekerja
        if (parameter.Kebijakan.TerapkanBpjsKesehatan && bpjsKesPekerja > 0)
        {
            var kom = parameter.DaftarKomponenGaji.FirstOrDefault(x => x.Kategori == KategoriKomponenGaji.BpjsKesehatan);
            detailSnapshot.Add(new BarisDetailPenggajianDto
            {
                KomponenGajiId = kom?.KomponenGajiId,
                KodeKomponen = kom?.KodeKomponen ?? "BPJS-KES",
                NamaKomponen = kom?.NamaKomponen ?? "BPJS Kesehatan (Pekerja 1%)",
                Jenis = JenisKomponenGaji.Potongan,
                Sifat = SifatKomponenGaji.Tetap,
                Kategori = KategoriKomponenGaji.BpjsKesehatan,
                Nominal = bpjsKesPekerja,
                NominalDitanggungPerusahaan = bpjsKesPerusahaan,
                Keterangan = $"Iuran BPJS Kesehatan (Pekerja 1%: Rp {bpjsKesPekerja:N0}, Perusahaan 4%: Rp {bpjsKesPerusahaan:N0})",
                Urutan = urutan++
            });
        }

        if (parameter.Kebijakan.TerapkanBpjsKetenagakerjaan)
        {
            if (jhtPekerja > 0)
            {
                var komJht = parameter.DaftarKomponenGaji.FirstOrDefault(x => x.Kategori == KategoriKomponenGaji.BpjsKetenagakerjaanJHT);
                detailSnapshot.Add(new BarisDetailPenggajianDto
                {
                    KomponenGajiId = komJht?.KomponenGajiId,
                    KodeKomponen = komJht?.KodeKomponen ?? "BPJS-TK-JHT",
                    NamaKomponen = komJht?.NamaKomponen ?? "BPJS Ketenagakerjaan JHT (2%)",
                    Jenis = JenisKomponenGaji.Potongan,
                    Sifat = SifatKomponenGaji.Tetap,
                    Kategori = KategoriKomponenGaji.BpjsKetenagakerjaanJHT,
                    Nominal = jhtPekerja,
                    NominalDitanggungPerusahaan = jhtPerusahaan,
                    Keterangan = $"Jaminan Hari Tua (Pekerja 2%: Rp {jhtPekerja:N0}, Perusahaan 3.7%: Rp {jhtPerusahaan:N0})",
                    Urutan = urutan++
                });
            }

            if (jpPekerja > 0)
            {
                var komJp = parameter.DaftarKomponenGaji.FirstOrDefault(x => x.Kategori == KategoriKomponenGaji.BpjsKetenagakerjaanJP);
                detailSnapshot.Add(new BarisDetailPenggajianDto
                {
                    KomponenGajiId = komJp?.KomponenGajiId,
                    KodeKomponen = komJp?.KodeKomponen ?? "BPJS-TK-JP",
                    NamaKomponen = komJp?.NamaKomponen ?? "BPJS Ketenagakerjaan JP (1%)",
                    Jenis = JenisKomponenGaji.Potongan,
                    Sifat = SifatKomponenGaji.Tetap,
                    Kategori = KategoriKomponenGaji.BpjsKetenagakerjaanJP,
                    Nominal = jpPekerja,
                    NominalDitanggungPerusahaan = jpPerusahaan,
                    Keterangan = $"Jaminan Pensiun (Pekerja 1%: Rp {jpPekerja:N0}, Perusahaan 2%: Rp {jpPerusahaan:N0})",
                    Urutan = urutan++
                });
            }
        }

        // 4. Kalkulasi Pajak PPh 21 (PP 58/2023 TER)
        // Menurut UU Pajak, objek PPh 21 bruto mencakup: Pendapatan Bruto Karyawan + JKK Perusahaan + JKM Perusahaan + BPJS Kes Perusahaan
        decimal brutoKenaPajakFinal = brutoKenaPajakDasar;
        if (parameter.Kebijakan.TerapkanBpjsKetenagakerjaan)
        {
            brutoKenaPajakFinal += (jkkPerusahaan + jkmPerusahaan);
        }
        if (parameter.Kebijakan.TerapkanBpjsKesehatan)
        {
            brutoKenaPajakFinal += bpjsKesPerusahaan;
        }

        decimal pph21 = 0;
        if (parameter.Kebijakan.TerapkanPph21)
        {
            pph21 = HitungPph21Bulanan(brutoKenaPajakFinal, parameter.StatusPTKP, parameter.Kebijakan.MetodePph21);
            hasil.TotalPotonganPph21 = pph21;

            if (pph21 > 0)
            {
                var komPph = parameter.DaftarKomponenGaji.FirstOrDefault(x => x.Kategori == KategoriKomponenGaji.PajakPPh21);
                detailSnapshot.Add(new BarisDetailPenggajianDto
                {
                    KomponenGajiId = komPph?.KomponenGajiId,
                    KodeKomponen = komPph?.KodeKomponen ?? "PPH21",
                    NamaKomponen = komPph?.NamaKomponen ?? "PPh 21 (PP 58/2023)",
                    Jenis = JenisKomponenGaji.Potongan,
                    Sifat = SifatKomponenGaji.TidakTetap,
                    Kategori = KategoriKomponenGaji.PajakPPh21,
                    Nominal = pph21,
                    Keterangan = $"PPh 21 bulanan berdasarkan TER kategori {DapatkanKategoriTer(parameter.StatusPTKP)} (Bruto kena pajak: Rp {brutoKenaPajakFinal:N0})",
                    Urutan = urutan++
                });
            }
        }

        // 5. Kalkulasi Potongan Keterlambatan Absensi
        decimal dendaTelat = 0;
        if (parameter.Kebijakan.TerapkanPotonganKeterlambatan && parameter.TotalMenitTerlambat > 0)
        {
            dendaTelat = HitungDendaKeterlambatan(
                parameter.TotalMenitTerlambat,
                parameter.Kebijakan.ToleransiKeterlambatanGratisMenit,
                parameter.Kebijakan.DendaKeterlambatanPerMenit);

            hasil.TotalPotonganKeterlambatan = dendaTelat;

            if (dendaTelat > 0)
            {
                var komTelat = parameter.DaftarKomponenGaji.FirstOrDefault(x => x.Kategori == KategoriKomponenGaji.PotonganKeterlambatan);
                detailSnapshot.Add(new BarisDetailPenggajianDto
                {
                    KomponenGajiId = komTelat?.KomponenGajiId,
                    KodeKomponen = komTelat?.KodeKomponen ?? "POT-TELAT",
                    NamaKomponen = komTelat?.NamaKomponen ?? "Potongan Keterlambatan",
                    Jenis = JenisKomponenGaji.Potongan,
                    Sifat = SifatKomponenGaji.TidakTetap,
                    Kategori = KategoriKomponenGaji.PotonganKeterlambatan,
                    Nominal = dendaTelat,
                    Keterangan = $"Keterlambatan total {parameter.TotalMenitTerlambat} menit (Toleransi: {parameter.Kebijakan.ToleransiKeterlambatanGratisMenit} menit, Tarif: Rp {parameter.Kebijakan.DendaKeterlambatanPerMenit:N0}/menit)",
                    Urutan = urutan++
                });
            }
        }

        // 6. Potongan Tambahan Lainnya (Kasbon / Pinjaman / Koperasi)
        decimal potonganLainnya = 0;
        foreach (var k in parameter.DaftarKomponenGaji.Where(x => x.Jenis == JenisKomponenGaji.Potongan && !x.DihitungOtomatisOlehSistem))
        {
            potonganLainnya += k.Nominal;
            detailSnapshot.Add(new BarisDetailPenggajianDto
            {
                KomponenGajiId = k.KomponenGajiId,
                KodeKomponen = k.KodeKomponen,
                NamaKomponen = k.NamaKomponen,
                Jenis = k.Jenis,
                Sifat = k.Sifat,
                Kategori = k.Kategori,
                Nominal = k.Nominal,
                Keterangan = k.Keterangan ?? "Potongan rutin karyawan",
                Urutan = urutan++
            });
        }
        hasil.TotalPotonganLainnya = potonganLainnya;

        // 7. Akumulasi Total Potongan dan Gaji Bersih
        hasil.TotalPotongan = hasil.TotalPotonganBpjs + hasil.TotalPotonganPph21 + hasil.TotalPotonganKeterlambatan + hasil.TotalPotonganLainnya;
        hasil.GajiBersih = Math.Max(0, hasil.TotalPendapatanKotor - hasil.TotalPotongan);
        hasil.DaftarDetail = detailSnapshot;

        return hasil;
    }

    public decimal HitungUpahLembur(decimal gajiPokok, decimal tunjanganTetap, decimal jamLembur, decimal tarifFlat = 0)
    {
        if (jamLembur <= 0) return 0;

        if (tarifFlat > 0)
        {
            return Math.Round(jamLembur * tarifFlat, 2, MidpointRounding.AwayFromZero);
        }

        // Formula Standar Kemnaker (PP 35/2021): 1/173 x (Gaji Pokok + Tunjangan Tetap)
        var upahSebulan = gajiPokok + tunjanganTetap;
        if (upahSebulan <= 0) return 0;

        var upahPerJam = upahSebulan / 173.0m;
        return Math.Round(jamLembur * upahPerJam, 2, MidpointRounding.AwayFromZero);
    }

    public decimal HitungDendaKeterlambatan(int totalMenitTerlambat, int toleransiGratisMenit, decimal dendaPerMenit)
    {
        if (totalMenitTerlambat <= 0 || dendaPerMenit <= 0) return 0;

        var menitKenaDenda = Math.Max(0, totalMenitTerlambat - toleransiGratisMenit);
        return Math.Round(menitKenaDenda * dendaPerMenit, 2, MidpointRounding.AwayFromZero);
    }

    public (decimal Pekerja, decimal Perusahaan) HitungBpjsKesehatan(decimal basisUpah, KebijakanPenggajianPerusahaan kebijakan)
    {
        if (!kebijakan.TerapkanBpjsKesehatan || basisUpah <= 0)
        {
            return (0, 0);
        }

        var basis = Math.Min(basisUpah, kebijakan.BatasMaksimalUpahBpjsKesehatan);
        var pekerja = Math.Round(basis * (kebijakan.PersenBpjsKesehatanPekerja / 100.0m), 2, MidpointRounding.AwayFromZero);
        var perusahaan = Math.Round(basis * (kebijakan.PersenBpjsKesehatanPerusahaan / 100.0m), 2, MidpointRounding.AwayFromZero);

        return (pekerja, perusahaan);
    }

    public (decimal JhtPekerja, decimal JhtPerusahaan, decimal JpPekerja, decimal JpPerusahaan, decimal JkkPerusahaan, decimal JkmPerusahaan)
        HitungBpjsKetenagakerjaan(decimal basisUpah, KebijakanPenggajianPerusahaan kebijakan)
    {
        if (!kebijakan.TerapkanBpjsKetenagakerjaan || basisUpah <= 0)
        {
            return (0, 0, 0, 0, 0, 0);
        }

        // JHT (Jaminan Hari Tua)
        var jhtPekerja = Math.Round(basisUpah * (kebijakan.PersenBpjsJhtPekerja / 100.0m), 2, MidpointRounding.AwayFromZero);
        var jhtPerusahaan = Math.Round(basisUpah * (kebijakan.PersenBpjsJhtPerusahaan / 100.0m), 2, MidpointRounding.AwayFromZero);

        // JP (Jaminan Pensiun) - Ada batas maksimal upah
        var basisJp = Math.Min(basisUpah, kebijakan.BatasMaksimalUpahBpjsJp);
        var jpPekerja = Math.Round(basisJp * (kebijakan.PersenBpjsJpPekerja / 100.0m), 2, MidpointRounding.AwayFromZero);
        var jpPerusahaan = Math.Round(basisJp * (kebijakan.PersenBpjsJpPerusahaan / 100.0m), 2, MidpointRounding.AwayFromZero);

        // JKK & JKM (Ditanggung Perusahaan Penuh)
        var jkkPerusahaan = Math.Round(basisUpah * (kebijakan.PersenBpjsJkkPerusahaan / 100.0m), 2, MidpointRounding.AwayFromZero);
        var jkmPerusahaan = Math.Round(basisUpah * (kebijakan.PersenBpjsJkmPerusahaan / 100.0m), 2, MidpointRounding.AwayFromZero);

        return (jhtPekerja, jhtPerusahaan, jpPekerja, jpPerusahaan, jkkPerusahaan, jkmPerusahaan);
    }

    public decimal HitungPph21Bulanan(decimal penghasilanBruto, StatusPTKP statusPtkp, MetodePajakPPh21 metode)
    {
        if (penghasilanBruto <= 0 || metode == MetodePajakPPh21.Nett)
        {
            return 0;
        }

        var kategoriTer = DapatkanKategoriTer(statusPtkp);
        var tarifPersen = DapatkanTarifTerPP58(penghasilanBruto, kategoriTer);

        var pajak = Math.Round(penghasilanBruto * (tarifPersen / 100.0m), 0, MidpointRounding.AwayFromZero);
        return pajak;
    }

    private static string DapatkanKategoriTer(StatusPTKP ptkp)
    {
        return ptkp switch
        {
            StatusPTKP.TK0 or StatusPTKP.TK1 or StatusPTKP.K0 => "A",
            StatusPTKP.TK2 or StatusPTKP.TK3 or StatusPTKP.K1 or StatusPTKP.K2 => "B",
            StatusPTKP.K3 => "C",
            _ => "A"
        };
    }

    /// <summary>
    /// Tabel Tarif Efektif Rata-Rata Bulanan PP 58 Tahun 2023 (Kategori A, B, C).
    /// </summary>
    private static decimal DapatkanTarifTerPP58(decimal bruto, string kategori)
    {
        return kategori switch
        {
            "A" => DapatkanTarifTerA(bruto),
            "B" => DapatkanTarifTerB(bruto),
            "C" => DapatkanTarifTerC(bruto),
            _ => DapatkanTarifTerA(bruto)
        };
    }

    private static decimal DapatkanTarifTerA(decimal bruto)
    {
        if (bruto <= 5400000m) return 0.00m;
        if (bruto <= 5650000m) return 0.25m;
        if (bruto <= 5950000m) return 0.50m;
        if (bruto <= 6300000m) return 0.75m;
        if (bruto <= 6750000m) return 1.00m;
        if (bruto <= 7500000m) return 1.25m;
        if (bruto <= 8550000m) return 1.50m;
        if (bruto <= 9650000m) return 1.75m;
        if (bruto <= 10050000m) return 2.00m;
        if (bruto <= 10350000m) return 2.25m;
        if (bruto <= 10700000m) return 2.50m;
        if (bruto <= 12500000m) return 3.00m;
        if (bruto <= 13750000m) return 4.00m;
        if (bruto <= 15100000m) return 5.00m;
        if (bruto <= 16950000m) return 6.00m;
        if (bruto <= 19750000m) return 7.00m;
        if (bruto <= 24150000m) return 8.00m;
        if (bruto <= 26450000m) return 9.00m;
        if (bruto <= 28000000m) return 10.00m;
        if (bruto <= 30050000m) return 11.00m;
        if (bruto <= 32400000m) return 12.00m;
        if (bruto <= 35400000m) return 13.00m;
        if (bruto <= 39100000m) return 14.00m;
        if (bruto <= 43850000m) return 15.00m;
        return 16.00m;
    }

    private static decimal DapatkanTarifTerB(decimal bruto)
    {
        if (bruto <= 6200000m) return 0.00m;
        if (bruto <= 6500000m) return 0.25m;
        if (bruto <= 6850000m) return 0.50m;
        if (bruto <= 7300000m) return 0.75m;
        if (bruto <= 9200000m) return 1.00m;
        if (bruto <= 10750000m) return 1.50m;
        if (bruto <= 12300000m) return 2.00m;
        if (bruto <= 13550000m) return 2.50m;
        if (bruto <= 14900000m) return 3.00m;
        if (bruto <= 16400000m) return 4.00m;
        if (bruto <= 18450000m) return 5.00m;
        if (bruto <= 21850000m) return 6.00m;
        if (bruto <= 26000000m) return 7.00m;
        if (bruto <= 27700000m) return 8.00m;
        if (bruto <= 29350000m) return 9.00m;
        if (bruto <= 31450000m) return 10.00m;
        if (bruto <= 33950000m) return 11.00m;
        if (bruto <= 37100000m) return 12.00m;
        if (bruto <= 41100000m) return 13.00m;
        if (bruto <= 45800000m) return 14.00m;
        return 15.00m;
    }

    private static decimal DapatkanTarifTerC(decimal bruto)
    {
        if (bruto <= 6600000m) return 0.00m;
        if (bruto <= 6950000m) return 0.25m;
        if (bruto <= 7350000m) return 0.50m;
        if (bruto <= 7800000m) return 0.75m;
        if (bruto <= 8850000m) return 1.00m;
        if (bruto <= 9800000m) return 1.25m;
        if (bruto <= 10950000m) return 1.50m;
        if (bruto <= 11200000m) return 1.75m;
        if (bruto <= 12050000m) return 2.00m;
        if (bruto <= 12950000m) return 3.00m;
        if (bruto <= 14150000m) return 4.00m;
        if (bruto <= 15550000m) return 5.00m;
        if (bruto <= 17050000m) return 6.00m;
        if (bruto <= 19500000m) return 7.00m;
        if (bruto <= 22700000m) return 8.00m;
        if (bruto <= 26600000m) return 9.00m;
        if (bruto <= 28100000m) return 10.00m;
        if (bruto <= 30100000m) return 11.00m;
        if (bruto <= 32600000m) return 12.00m;
        if (bruto <= 35700000m) return 13.00m;
        if (bruto <= 39400000m) return 14.00m;
        if (bruto <= 44100000m) return 15.00m;
        return 16.00m;
    }
}
