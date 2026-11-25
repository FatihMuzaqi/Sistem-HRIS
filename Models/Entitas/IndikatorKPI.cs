using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SistemHRD.Models.Enums;

namespace SistemHRD.Models.Entitas;

/// <summary>
/// Entitas Indikator KPI (Key Performance Indicator) sebagai dasar objektif penilaian kinerja.
/// </summary>
public class IndikatorKPI : EntitasTenantDasar
{
    public Guid? DepartemenId { get; set; }

    [ForeignKey(nameof(DepartemenId))]
    public virtual Departemen? Departemen { get; set; }

    public Guid? JabatanId { get; set; }

    [ForeignKey(nameof(JabatanId))]
    public virtual Jabatan? Jabatan { get; set; }

    [Required]
    [MaxLength(50)]
    public string KodeIndikator { get; set; } = string.Empty;

    [Required]
    [MaxLength(150)]
    public string NamaIndikator { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Deskripsi { get; set; }

    [Required]
    public JenisIndikatorKPI Jenis { get; set; } = JenisIndikatorKPI.Kuantitatif;

    [Column(TypeName = "decimal(10,2)")]
    public decimal? TargetKuantitatif { get; set; }

    [MaxLength(50)]
    public string? Satuan { get; set; } = "%";

    [Column(TypeName = "decimal(5,2)")]
    public decimal BobotBawaan { get; set; } = 20.00m;

    public bool StatusAktif { get; set; } = true;
}
