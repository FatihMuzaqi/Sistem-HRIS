using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemHRD.Models.Entitas;

/// <summary>
/// Entitas kontak darurat yang dapat dihubungi terkait karyawan.
/// </summary>
public class KontakDaruratKaryawan : EntitasTenantDasar
{
    [Required]
    public Guid KaryawanId { get; set; }

    [ForeignKey(nameof(KaryawanId))]
    public virtual Karyawan? Karyawan { get; set; }

    [Required]
    [MaxLength(150)]
    public string NamaLengkap { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string Hubungan { get; set; } = string.Empty; // Pasangan, Orang Tua, Saudara Kandung, Anak, dll.

    [Required]
    [MaxLength(30)]
    public string NomorTelepon { get; set; } = string.Empty;

    [MaxLength(255)]
    public string? Alamat { get; set; }
}
