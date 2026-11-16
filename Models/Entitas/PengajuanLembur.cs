using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SistemHRD.Models.Enums;

namespace SistemHRD.Models.Entitas;

/// <summary>
/// Pengajuan kerja lembur karyawan di luar jam kerja normal.
/// </summary>
public class PengajuanLembur : EntitasTenantDasar
{
    [Required]
    public Guid KaryawanId { get; set; }

    [ForeignKey(nameof(KaryawanId))]
    public virtual Karyawan? Karyawan { get; set; }

    [Required]
    [MaxLength(50)]
    public string NomorPengajuan { get; set; } = string.Empty;

    public DateTime TanggalLembur { get; set; }

    public TimeSpan JamMulai { get; set; }

    public TimeSpan JamSelesai { get; set; }

    public decimal TotalJam { get; set; }

    [Required]
    [MaxLength(500)]
    public string AlasanLembur { get; set; } = string.Empty;

    public StatusPengajuanLembur Status { get; set; } = StatusPengajuanLembur.MenungguPersetujuan;

    public Guid? DisetujuiOlehId { get; set; }

    [ForeignKey(nameof(DisetujuiOlehId))]
    public virtual Pengguna? DisetujuiOleh { get; set; }

    public DateTime? WaktuPersetujuan { get; set; }

    [MaxLength(500)]
    public string? CatatanPersetujuan { get; set; }
}
