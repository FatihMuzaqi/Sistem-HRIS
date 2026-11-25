using System;
using System.Collections.Generic;
using SistemHRD.Models.Enums;
using SistemHRD.ViewModels;
using Xunit;

namespace SistemHRD.Tests;

public class UjiPenilaianKinerja
{
    private (decimal TotalSkor, KategoriKinerja Predikat) HitungSkorObjektif(List<ItemInputDetailPenilaianViewModel> daftarDetail)
    {
        if (daftarDetail == null || daftarDetail.Count == 0)
        {
            throw new InvalidOperationException("Skor kinerja tidak dapat dihitung tanpa adanya rincian indikator KPI.");
        }

        decimal totalBobot = 0;
        foreach (var d in daftarDetail)
        {
            totalBobot += d.BobotPersen;
        }

        if (totalBobot <= 0) totalBobot = 100;

        decimal totalSkor = 0;
        foreach (var d in daftarDetail)
        {
            var bobotNormal = d.BobotPersen / totalBobot;
            var tertimbang = Math.Round(bobotNormal * d.SkorIndikator, 2);
            totalSkor += tertimbang;
        }

        totalSkor = Math.Round(totalSkor, 2);

        KategoriKinerja predikat;
        if (totalSkor >= 90.00m) predikat = KategoriKinerja.Istimewa;
        else if (totalSkor >= 80.00m) predikat = KategoriKinerja.Baik;
        else if (totalSkor >= 70.00m) predikat = KategoriKinerja.Cukup;
        else if (totalSkor >= 60.00m) predikat = KategoriKinerja.Kurang;
        else predikat = KategoriKinerja.SangatKurang;

        return (totalSkor, predikat);
    }

    [Fact]
    public void HitungSkor_SeluruhIndikatorMaksimal_MenghasilkanIstimewa()
    {
        // Arrange
        var daftarDetail = new List<ItemInputDetailPenilaianViewModel>
        {
            new() { BobotPersen = 25, SkorIndikator = 100 },
            new() { BobotPersen = 25, SkorIndikator = 100 },
            new() { BobotPersen = 25, SkorIndikator = 100 },
            new() { BobotPersen = 25, SkorIndikator = 100 }
        };

        // Act
        var (totalSkor, predikat) = HitungSkorObjektif(daftarDetail);

        // Assert
        Assert.Equal(100.00m, totalSkor);
        Assert.Equal(KategoriKinerja.Istimewa, predikat);
    }

    [Fact]
    public void HitungSkor_BobotBeragam_MenghasilkanSkorTertimbangAkurat()
    {
        // Arrange
        // Indikator 1: 40% * 90 = 36.00
        // Indikator 2: 30% * 80 = 24.00
        // Indikator 3: 30% * 70 = 21.00
        // Total = 36 + 24 + 21 = 81.00 -> Predikat Baik (B)
        var daftarDetail = new List<ItemInputDetailPenilaianViewModel>
        {
            new() { BobotPersen = 40, SkorIndikator = 90 },
            new() { BobotPersen = 30, SkorIndikator = 80 },
            new() { BobotPersen = 30, SkorIndikator = 70 }
        };

        // Act
        var (totalSkor, predikat) = HitungSkorObjektif(daftarDetail);

        // Assert
        Assert.Equal(81.00m, totalSkor);
        Assert.Equal(KategoriKinerja.Baik, predikat);
    }

    [Fact]
    public void HitungSkor_NormalisasiBobotJikaTidakPas100_TetapProporsional()
    {
        // Arrange: Total bobot 50 (bukan 100)
        // Indikator 1: 25 / 50 * 80 = 40.00
        // Indikator 2: 25 / 50 * 90 = 45.00
        // Total = 85.00 -> Baik
        var daftarDetail = new List<ItemInputDetailPenilaianViewModel>
        {
            new() { BobotPersen = 25, SkorIndikator = 80 },
            new() { BobotPersen = 25, SkorIndikator = 90 }
        };

        // Act
        var (totalSkor, predikat) = HitungSkorObjektif(daftarDetail);

        // Assert
        Assert.Equal(85.00m, totalSkor);
        Assert.Equal(KategoriKinerja.Baik, predikat);
    }

    [Fact]
    public void HitungSkor_TanpaIndikator_MenolakDanMelemparException()
    {
        // Assert: "Jangan membuat skor kinerja yang tidak memiliki dasar indikator penilaian"
        var daftarKosong = new List<ItemInputDetailPenilaianViewModel>();

        Assert.Throws<InvalidOperationException>(() => HitungSkorObjektif(daftarKosong));
    }

    [Theory]
    [InlineData(95.0, KategoriKinerja.Istimewa)]
    [InlineData(90.0, KategoriKinerja.Istimewa)]
    [InlineData(89.9, KategoriKinerja.Baik)]
    [InlineData(80.0, KategoriKinerja.Baik)]
    [InlineData(79.9, KategoriKinerja.Cukup)]
    [InlineData(70.0, KategoriKinerja.Cukup)]
    [InlineData(69.9, KategoriKinerja.Kurang)]
    [InlineData(60.0, KategoriKinerja.Kurang)]
    [InlineData(59.9, KategoriKinerja.SangatKurang)]
    [InlineData(40.0, KategoriKinerja.SangatKurang)]
    public void HitungSkor_BatasPredikatKinerja_TepatSesuaiStandar(decimal skorInput, KategoriKinerja predikatDiharapkan)
    {
        // Arrange
        var daftarDetail = new List<ItemInputDetailPenilaianViewModel>
        {
            new() { BobotPersen = 100, SkorIndikator = skorInput }
        };

        // Act
        var (_, predikat) = HitungSkorObjektif(daftarDetail);

        // Assert
        Assert.Equal(predikatDiharapkan, predikat);
    }
}
