using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemHRD.Models.Entitas;

/// <summary>
/// Entitas Sertifikat Kelulusan Pelatihan Karyawan.
/// </summary>
public class SertifikatPelatihan : EntitasTenantDasar
{
    [Required]
    public Guid PesertaPelatihanId { get; set; }

    [ForeignKey(nameof(PesertaPelatihanId))]
    public virtual PesertaPelatihan PesertaPelatihan { get; set; } = null!;

    [Required]
    [MaxLength(100)]
    public string NomorSertifikat { get; set; } = string.Empty;

    [Required]
    [MaxLength(150)]
    public string JudulSertifikat { get; set; } = string.Empty;

    [Required]
    public DateTime TanggalTerbit { get; set; } = DateTime.Today;

    public DateTime? TanggalKedaluwarsa { get; set; }

    [Required]
    [MaxLength(150)]
    public string PenerbitSertifikat { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string KodeVerifikasi { get; set; } = string.Empty;

    [MaxLength(255)]
    public string? FileSertifikat { get; set; }

    [MaxLength(500)]
    public string? Catatan { get; set; }
}
