using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemHRD.Models.Entitas;

/// <summary>
/// Relasi many-to-many antara Pengguna dan Perusahaan, memungkinkan satu akun memiliki afiliasi ke beberapa entitas bisnis.
/// </summary>
public class PenggunaPerusahaan : ITenantEntity
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid PenggunaId { get; set; }

    [ForeignKey(nameof(PenggunaId))]
    public virtual Pengguna? Pengguna { get; set; }

    [Required]
    public Guid PerusahaanId { get; set; }

    [ForeignKey(nameof(PerusahaanId))]
    public virtual Perusahaan? Perusahaan { get; set; }

    [MaxLength(50)]
    public string? PeranDiPerusahaan { get; set; } // HRD, Manager, Finance, Karyawan

    public bool IsDefault { get; set; } = true;

    public bool StatusAktif { get; set; } = true;

    public DateTimeOffset JoinedAt { get; set; } = DateTimeOffset.UtcNow;
}
