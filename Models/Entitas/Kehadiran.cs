using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SistemHRD.Models.Enums;

namespace SistemHRD.Models.Entitas;

/// <summary>
/// Data presensi harian karyawan (Check-in, Check-out, keterlambatan, dan lokasi).
/// </summary>
public class Kehadiran : EntitasTenantDasar
{
    [Required]
    public Guid KaryawanId { get; set; }

    [ForeignKey(nameof(KaryawanId))]
    public virtual Karyawan? Karyawan { get; set; }

    public Guid? ShiftKerjaId { get; set; }

    [ForeignKey(nameof(ShiftKerjaId))]
    public virtual ShiftKerja? ShiftKerja { get; set; }

    public DateTime Tanggal { get; set; }

    public DateTime? WaktuMasuk { get; set; }

    public DateTime? WaktuPulang { get; set; }

    public StatusKehadiran Status { get; set; } = StatusKehadiran.Hadir;

    public int MenitKeterlambatan { get; set; } = 0;

    public int MenitPulangCepat { get; set; } = 0;

    public decimal TotalJamKerja { get; set; } = 0;

    [MaxLength(255)]
    public string? CatatanMasuk { get; set; }

    [MaxLength(255)]
    public string? CatatanPulang { get; set; }

    [MaxLength(255)]
    public string? LokasiMasuk { get; set; }

    [MaxLength(255)]
    public string? LokasiPulang { get; set; }

    [MaxLength(255)]
    public string? FotoMasuk { get; set; }

    [MaxLength(255)]
    public string? FotoPulang { get; set; }
}
