using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemHRD.Models.Entitas;

/// <summary>
/// Rincian Penilaian Kinerja per Butir Indikator KPI yang Dievaluasi.
/// </summary>
public class DetailPenilaian : EntitasTenantDasar
{
    [Required]
    public Guid PenilaianKinerjaId { get; set; }

    [ForeignKey(nameof(PenilaianKinerjaId))]
    public virtual PenilaianKinerja? PenilaianKinerja { get; set; }

    [Required]
    public Guid IndikatorKPIId { get; set; }

    [ForeignKey(nameof(IndikatorKPIId))]
    public virtual IndikatorKPI? IndikatorKPI { get; set; }

    [Required]
    [MaxLength(150)]
    public string NamaIndikatorSnapshot { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? TargetSnapshot { get; set; }

    [MaxLength(100)]
    public string? Realisasi { get; set; }

    [Column(TypeName = "decimal(5,2)")]
    public decimal BobotPersen { get; set; }

    [Column(TypeName = "decimal(5,2)")]
    public decimal SkorIndikator { get; set; }

    [Column(TypeName = "decimal(5,2)")]
    public decimal SkorTertimbang { get; set; }

    [MaxLength(500)]
    public string? CatatanIndikator { get; set; }
}
