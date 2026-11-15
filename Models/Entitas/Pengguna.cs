using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace SistemHRD.Models.Entitas;

/// <summary>
/// Entitas Pengguna sistem yang terintegrasi dengan ASP.NET Core Identity dan Multi-Tenant.
/// </summary>
public class Pengguna : IdentityUser<Guid>, IAuditEntity, ISoftDelete
{
    [Required]
    [MaxLength(150)]
    public string NamaLengkap { get; set; } = string.Empty;

    /// <summary>
    /// ID Perusahaan aktif tenant. Bernilai null untuk SuperAdmin tingkat platform.
    /// </summary>
    public Guid? PerusahaanId { get; set; }

    [ForeignKey(nameof(PerusahaanId))]
    public virtual Perusahaan? Perusahaan { get; set; }

    [MaxLength(255)]
    public string? AvatarUrl { get; set; }

    public bool StatusAktif { get; set; } = true;

    // Relasi ke tabel perantara PenggunaPerusahaan
    public virtual ICollection<PenggunaPerusahaan> DaftarPenggunaPerusahaan { get; set; } = new List<PenggunaPerusahaan>();

    // Jejak Audit
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    [MaxLength(100)]
    public string? CreatedBy { get; set; }

    public DateTimeOffset? UpdatedAt { get; set; }

    [MaxLength(100)]
    public string? UpdatedBy { get; set; }

    // Soft Delete
    public bool IsDeleted { get; set; } = false;

    public DateTimeOffset? DeletedAt { get; set; }

    [MaxLength(100)]
    public string? DeletedBy { get; set; }
}
