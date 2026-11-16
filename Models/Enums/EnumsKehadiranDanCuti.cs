namespace SistemHRD.Models.Enums;

/// <summary>
/// Status kehadiran harian karyawan.
/// </summary>
public enum StatusKehadiran
{
    Hadir = 1,
    Terlambat = 2,
    Izin = 3,
    Sakit = 4,
    Cuti = 5,
    Alpa = 6,
    DinasLuar = 7
}

/// <summary>
/// Status persetujuan lembur.
/// </summary>
public enum StatusPengajuanLembur
{
    MenungguPersetujuan = 1,
    Disetujui = 2,
    Ditolak = 3
}

/// <summary>
/// Status siklus persetujuan pengajuan cuti (2 Tahap: Manager -> HRD).
/// </summary>
public enum StatusPengajuanCuti
{
    MenungguPersetujuanManager = 1,
    DisetujuiManager = 2,
    Disetujui = 3,
    Ditolak = 4,
    Dibatalkan = 5
}

/// <summary>
/// Tahap dalam alur persetujuan cuti.
/// </summary>
public enum TahapPersetujuanCuti
{
    Manager = 1,
    HRD = 2
}

/// <summary>
/// Keputusan persetujuan oleh peninjau.
/// </summary>
public enum KeputusanPersetujuan
{
    Disetujui = 1,
    Ditolak = 2
}
