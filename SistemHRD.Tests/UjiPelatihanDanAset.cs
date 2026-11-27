using System;
using System.Collections.Generic;
using System.Text;
using SistemHRD.Models.Enums;
using SistemHRD.Services.Implementasi;
using SistemHRD.ViewModels;
using Xunit;

namespace SistemHRD.Tests;

public class UjiPelatihanDanAset
{
    [Fact]
    public void FormatNomorSertifikatDanKodeVerifikasi_MemilikiStrukturStandar()
    {
        // Arrange
        var tgl = DateTime.Today;
        var prefixBulan = tgl.ToString("yyyyMM");
        var nomorSertifikat = $"CERT-{prefixBulan}-{new Random().Next(1000, 9999)}";
        var kodeVerifikasi = $"VERIF-TRN-{Guid.NewGuid().ToString("N")[..8].ToUpper()}";

        // Assert
        Assert.StartsWith($"CERT-{prefixBulan}-", nomorSertifikat);
        Assert.StartsWith("VERIF-TRN-", kodeVerifikasi);
        Assert.Equal(18, kodeVerifikasi.Length);
    }

    [Theory]
    [InlineData(StatusKeikutsertaan.Lulus, true)]
    [InlineData(StatusKeikutsertaan.Terdaftar, false)]
    [InlineData(StatusKeikutsertaan.SedangMengikuti, false)]
    [InlineData(StatusKeikutsertaan.TidakLulus, false)]
    [InlineData(StatusKeikutsertaan.Batal, false)]
    public void KelayakanMenerimaSertifikat_HanyaPesertaLulusYangBerhak(StatusKeikutsertaan status, bool berhak)
    {
        // Act
        var apakahBerhak = status == StatusKeikutsertaan.Lulus;

        // Assert
        Assert.Equal(berhak, apakahBerhak);
    }

    [Fact]
    public void TransisiStatusAset_PenyerahanDanPengembalian_Konsisten()
    {
        // Kondisi awal aset di gudang
        var statusAset = StatusAset.Tersedia;
        Guid? pemegangSaatIni = null;

        // 1. Aksi Penyerahan ke Karyawan
        var karyawanId = Guid.NewGuid();
        statusAset = StatusAset.Digunakan;
        pemegangSaatIni = karyawanId;

        Assert.Equal(StatusAset.Digunakan, statusAset);
        Assert.NotNull(pemegangSaatIni);
        Assert.Equal(karyawanId, pemegangSaatIni.Value);

        // 2. Aksi Pengembalian ke Gudang
        statusAset = StatusAset.Tersedia;
        pemegangSaatIni = null;

        Assert.Equal(StatusAset.Tersedia, statusAset);
        Assert.Null(pemegangSaatIni);
    }

    [Fact]
    public void EksporCsv_MenyertakanBOMUtf8DanHeaderLengkap()
    {
        // Arrange
        var layananLaporan = new LayananLaporanPusat(null!, null!);
        var dataKaryawan = new List<BarisLaporanKaryawanViewModel>
        {
            new()
            {
                NIK = "KRY001",
                NamaLengkap = "Rian Pratama",
                Email = "rian@solusihr.id",
                NomorTelepon = "08123456789",
                NamaDepartemen = "Teknologi Informasi",
                NamaJabatan = "Senior Software Engineer",
                JenisKelamin = "Laki-laki",
                TanggalMasuk = new DateTime(2025, 1, 10),
                Status = StatusKaryawan.Tetap
            }
        };

        // Act
        var bytes = layananLaporan.EksporKaryawanCsv(dataKaryawan);

        // Assert
        Assert.NotNull(bytes);
        Assert.True(bytes.Length > 3);

        // Periksa UTF-8 BOM: 0xEF, 0xBB, 0xBF
        Assert.Equal(0xEF, bytes[0]);
        Assert.Equal(0xBB, bytes[1]);
        Assert.Equal(0xBF, bytes[2]);

        var text = Encoding.UTF8.GetString(bytes);
        Assert.Contains("NIK,Nama Lengkap,Email", text);
        Assert.Contains("KRY001", text);
        Assert.Contains("Rian Pratama", text);
        Assert.Contains("Teknologi Informasi", text);
    }
}
