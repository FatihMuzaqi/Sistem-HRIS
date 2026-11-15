using System.ComponentModel.DataAnnotations;

namespace SistemHRD.Models.Entitas;

/// <summary>
/// Kelas dasar untuk entitas non-tenant (seperti Perusahaan, Konfigurasi Global, dll).
/// </summary>
public abstract class EntitasDasar : IAuditEntity, ISoftDelete
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    [MaxLength(100)]
    public string? CreatedBy { get; set; }

    public DateTimeOffset? UpdatedAt { get; set; }

    [MaxLength(100)]
    public string? UpdatedBy { get; set; }

    public bool IsDeleted { get; set; } = false;

    public DateTimeOffset? DeletedAt { get; set; }

    [MaxLength(100)]
    public string? DeletedBy { get; set; }
}
