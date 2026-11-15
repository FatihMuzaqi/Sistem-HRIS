using System.ComponentModel.DataAnnotations;

namespace SistemHRD.Models.Entitas;

/// <summary>
/// Entitas Perusahaan sebagai unit penyewa (Tenant) dalam arsitektur Multi-Tenant.
/// </summary>
public class Perusahaan : EntitasDasar
{
    [Required]
    [MaxLength(20)]
    public string KodePerusahaan { get; set; } = string.Empty;

    [Required]
    [MaxLength(150)]
    public string NamaPerusahaan { get; set; } = string.Empty;

    [MaxLength(100)]
    [EmailAddress]
    public string? Email { get; set; }

    [MaxLength(30)]
    public string? NomorTelepon { get; set; }

    [MaxLength(255)]
    public string? Alamat { get; set; }

    [MaxLength(50)]
    public string? NPWP { get; set; }

    [MaxLength(255)]
    public string? LogoUrl { get; set; }

    [MaxLength(100)]
    public string? Website { get; set; }

    public bool StatusAktif { get; set; } = true;

    // Koleksi Relasi
    public virtual ICollection<Pengguna> DaftarPengguna { get; set; } = new List<Pengguna>();
    public virtual ICollection<PenggunaPerusahaan> DaftarPenggunaPerusahaan { get; set; } = new List<PenggunaPerusahaan>();
    public virtual ICollection<AuditLog> DaftarAuditLog { get; set; } = new List<AuditLog>();
}
