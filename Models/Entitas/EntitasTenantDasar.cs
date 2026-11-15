using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemHRD.Models.Entitas;

/// <summary>
/// Kelas dasar untuk entitas bisnis yang terikat pada suatu Perusahaan (Multi-Tenant).
/// </summary>
public abstract class EntitasTenantDasar : EntitasDasar, ITenantEntity
{
    [Required]
    public Guid PerusahaanId { get; set; }

    [ForeignKey(nameof(PerusahaanId))]
    public virtual Perusahaan? Perusahaan { get; set; }
}
