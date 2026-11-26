namespace SistemHRD.Models.Enums;

public enum KategoriAset
{
    ElektronikLaptop = 1,
    PeralatanKantor = 2,
    KendaraanOperasional = 3,
    Furniture = 4,
    LisensiPerangkatLunak = 5,
    Lainnya = 6
}

public enum KondisiAset
{
    SangatBaik = 1,
    Baik = 2,
    RusakRingan = 3,
    RusakBerat = 4,
    Hilang = 5
}

public enum StatusAset
{
    Tersedia = 1,
    Digunakan = 2,
    DalamPerbaikan = 3,
    Dihapuskan = 4
}

public enum TipeAktivitasAset
{
    Penyerahan = 1,
    Pengembalian = 2,
    Perbaikan = 3,
    PemeriksaanKondisi = 4
}
