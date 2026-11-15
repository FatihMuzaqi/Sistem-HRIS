using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemHRD.Models.Entitas;

/// <summary>
/// Entitas riwayat pengalaman kerja karyawan sebelum bergabung.
/// </summary>
public class RiwayatPekerjaan : EntitasTenantDasar
{
    [Required]
    public Guid KaryawanId { get; set; }

    [ForeignKey(nameof(KaryawanId))]
    public virtual Karyawan? Karyawan { get; set; }

    [Required]
    [MaxLength(150)]
    public string NamaPerusahaanSebelumnya { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string JabatanTerakhir { get; set; } = string.Empty;

    public int TahunMulai { get; set; }

    public int TahunSelesai { get; set; }

    [MaxLength(500)]
    public string? DeskripsiTugas { get; set; }
}
