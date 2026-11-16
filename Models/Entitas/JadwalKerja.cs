using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemHRD.Models.Entitas;

/// <summary>
/// Penugasan shift kerja karyawan pada tanggal tertentu.
/// </summary>
public class JadwalKerja : EntitasTenantDasar
{
    [Required]
    public Guid KaryawanId { get; set; }

    [ForeignKey(nameof(KaryawanId))]
    public virtual Karyawan? Karyawan { get; set; }

    [Required]
    public Guid ShiftKerjaId { get; set; }

    [ForeignKey(nameof(ShiftKerjaId))]
    public virtual ShiftKerja? ShiftKerja { get; set; }

    public DateTime Tanggal { get; set; }

    public bool StatusHariLibur { get; set; } = false;

    [MaxLength(255)]
    public string? Catatan { get; set; }
}
