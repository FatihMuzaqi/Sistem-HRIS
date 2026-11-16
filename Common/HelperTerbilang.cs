namespace SistemHRD.Common;

/// <summary>
/// Helper untuk mengubah angka nominal uang menjadi teks terbilang Bahasa Indonesia.
/// </summary>
public static class HelperTerbilang
{
    private static readonly string[] Satuan = { "", "Satu", "Dua", "Tiga", "Empat", "Lima", "Enam", "Tujuh", "Delapan", "Sembilan", "Sepuluh", "Sebelas" };

    public static string Konversi(decimal nilai)
    {
        var angka = (long)Math.Floor(nilai);
        if (angka == 0) return "Nol Rupiah";
        if (angka < 0) return "Minus " + KonversiPositif(Math.Abs(angka)) + " Rupiah";
        return KonversiPositif(angka) + " Rupiah";
    }

    private static string KonversiPositif(long angka)
    {
        if (angka < 12)
        {
            return Satuan[angka];
        }
        if (angka < 20)
        {
            return Satuan[angka - 10] + " Belas";
        }
        if (angka < 100)
        {
            return Satuan[angka / 10] + " Puluh " + KonversiPositif(angka % 10);
        }
        if (angka < 200)
        {
            return "Seratus " + KonversiPositif(angka - 100);
        }
        if (angka < 1000)
        {
            return Satuan[angka / 100] + " Ratus " + KonversiPositif(angka % 100);
        }
        if (angka < 2000)
        {
            return "Seribu " + KonversiPositif(angka - 1000);
        }
        if (angka < 1000000)
        {
            return KonversiPositif(angka / 1000) + " Ribu " + KonversiPositif(angka % 1000);
        }
        if (angka < 1000000000)
        {
            return KonversiPositif(angka / 1000000) + " Juta " + KonversiPositif(angka % 1000000);
        }
        if (angka < 1000000000000)
        {
            return KonversiPositif(angka / 1000000000) + " Miliar " + KonversiPositif(angka % 1000000000);
        }

        return KonversiPositif(angka / 1000000000000) + " Triliun " + KonversiPositif(angka % 1000000000000);
    }
}
