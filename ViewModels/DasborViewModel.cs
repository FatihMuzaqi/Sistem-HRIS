using SistemHRD.Models.Entitas;

namespace SistemHRD.ViewModels;

/// <summary>
/// Model data terpadu untuk tampilan dasbor analitik berbasis peran (SuperAdmin, HRD, Manager, Finance, Karyawan).
/// </summary>
public class DasborViewModel
{
    public string PeranPengguna { get; set; } = string.Empty;
    public string NamaPengguna { get; set; } = string.Empty;
    public string? NamaPerusahaan { get; set; }
    public string? KodePerusahaan { get; set; }
    public bool IsSuperAdmin { get; set; }

    // Statistik SuperAdmin (Platform Level)
    public int TotalPerusahaanAktif { get; set; }
    public int TotalPenggunaSistem { get; set; }
    public int TotalAktivitasHariIni { get; set; }

    // Statistik HRD / Manager / Finance (Tenant Level)
    public int TotalKaryawanPerusahaan { get; set; }
    public int TotalPenggunaAktifTenant { get; set; }
    public int PresensiHadirHariIni { get; set; }
    public int PengajuanCutiPending { get; set; }

    // Daftar Data Terkini
    public List<AuditLog> LogAktivitasTerakhir { get; set; } = new();
    public List<Perusahaan> DaftarPerusahaanPlatform { get; set; } = new();
    public List<Pengguna> DaftarPenggunaTenant { get; set; } = new();
}
