using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemHRD.Models.Entitas;

/// <summary>
/// Pemetaan konfigurasi nominal komponen gaji untuk masing-masing karyawan.
/// </summary>
public class GajiKaryawan : EntitasTenantDasar
{
    [Required]
    public Guid KaryawanId { get; set; }

    [ForeignKey(nameof(KaryawanId))]
    public virtual Karyawan? Karyawan { get; set; }

    [Required]
    public Guid KomponenGajiId { get; set; }

    [ForeignKey(nameof(KomponenGajiId))]
    public virtual KomponenGaji? KomponenGaji { get; set; }

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal Nominal { get; set; } = 0;

    public bool StatusAktif { get; set; } = true;

    [MaxLength(255)]
    public string? Keterangan { get; set; }
}
