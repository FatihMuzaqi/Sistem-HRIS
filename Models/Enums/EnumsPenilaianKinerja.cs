namespace SistemHRD.Models.Enums;

public enum StatusPeriodePenilaian
{
    Draf = 1,
    Aktif = 2,
    PenilaianSelesai = 3,
    Ditutup = 4
}

public enum JenisIndikatorKPI
{
    Kuantitatif = 1,
    Kualitatif = 2,
    Perilaku = 3,
    Proyek = 4
}

public enum StatusPenilaianKinerja
{
    Draf = 1,
    MenungguPersetujuan = 2,
    Disetujui = 3
}

public enum KategoriKinerja
{
    SangatKurang = 1,  // E: < 60
    Kurang = 2,        // D: 60 - 69.99
    Cukup = 3,         // C: 70 - 79.99
    Baik = 4,          // B: 80 - 89.99
    Istimewa = 5       // A: >= 90
}
