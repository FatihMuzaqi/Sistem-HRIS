using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SistemHRD.Models.Enums;

namespace SistemHRD.Models.Entitas;

/// <summary>
/// Catatan riwayat persetujuan (approval) pengajuan cuti pada setiap tahap (Manager / HRD).
/// </summary>
public class PersetujuanCuti : EntitasTenantDasar
{
    [Required]
    public Guid PengajuanCutiId { get; set; }

    [ForeignKey(nameof(PengajuanCutiId))]
    public virtual PengajuanCuti? PengajuanCuti { get; set; }

    public TahapPersetujuanCuti Tahap { get; set; } = TahapPersetujuanCuti.Manager;

    [Required]
    public Guid PenyetujuId { get; set; }

    [ForeignKey(nameof(PenyetujuId))]
    public virtual Pengguna? Penyetuju { get; set; }

    public KeputusanPersetujuan Keputusan { get; set; } = KeputusanPersetujuan.Disetujui;

    [MaxLength(500)]
    public string? Catatan { get; set; }

    public DateTime WaktuPersetujuan { get; set; } = DateTime.UtcNow;
}
