namespace SistemHRD.Models.Enums;

public enum JenisKelamin
{
    LakiLaki = 1,
    Perempuan = 2
}

public enum StatusKaryawan
{
    Tetap = 1,
    Kontrak = 2,
    Percobaan = 3,
    Magang = 4,
    Nonaktif = 5
}

public enum JenisKontrak
{
    PKWT = 1,
    PKWTT = 2,
    Magang = 3,
    Freelance = 4
}

public enum StatusKontrak
{
    Aktif = 1,
    Selesai = 2,
    Diperpanjang = 3,
    Dibatalkan = 4
}

public enum JenisDokumen
{
    KTP = 1,
    KK = 2,
    Ijazah = 3,
    CV = 4,
    Kontrak = 5,
    Sertifikat = 6,
    NPWP = 7,
    Lainnya = 8
}
