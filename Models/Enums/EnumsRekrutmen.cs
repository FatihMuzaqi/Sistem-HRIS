namespace SistemHRD.Models.Enums;

public enum StatusLowongan
{
    Draf = 1,
    Dibuka = 2,
    Ditutup = 3,
    Dibatalkan = 4
}

public enum TipePekerjaan
{
    PenuhWaktu = 1,
    ParuhWaktu = 2,
    Kontrak = 3,
    Magang = 4,
    Remote = 5
}

public enum StatusPelamar
{
    Baru = 1,
    SeleksiAdministrasi = 2,
    LolosAdministrasi = 3,
    Wawancara = 4,
    PenawaranKerja = 5,
    Diterima = 6,
    Ditolak = 7
}

public enum JenisTahapanSeleksi
{
    Administrasi = 1,
    TesTertulis = 2,
    WawancaraHR = 3,
    WawancaraUser = 4,
    TesTeknis = 5,
    MedicalCheckUp = 6,
    Offering = 7
}

public enum StatusWawancara
{
    Dijadwalkan = 1,
    Selesai = 2,
    Dibatalkan = 3,
    Reschedule = 4
}

public enum MetodeWawancara
{
    TatapMuka = 1,
    Daring = 2
}

public enum HasilKeputusanSeleksi
{
    Lolos = 1,
    Dipertimbangkan = 2,
    TidakLolos = 3
}
