using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SistemHRD.Models.Enums;

namespace SistemHRD.Models.Entitas;

/// <summary>
/// Permohonan pengajuan cuti karyawan dengan alur persetujuan multi-tahap (Manager -> HRD).
/// </summary>
public class PengajuanCuti : EntitasTenantDasar
{
    [Required]
    public Guid KaryawanId { get; set; }

    [ForeignKey(nameof(KaryawanId))]
    public virtual Karyawan? Karyawan { get; set; }

    [Required]
    public Guid JenisCutiId { get; set; }

    [ForeignKey(nameof(JenisCutiId))]
    public virtual JenisCuti? JenisCuti { get; set; }

    [Required]
    [MaxLength(50)]
    public string NomorPengajuan { get; set; } = string.Empty;

    public DateTime TanggalMulai { get; set; }

    public DateTime TanggalSelesai { get; set; }

    public int JumlahHari { get; set; }

    [Required]
    [MaxLength(500)]
    public string Alasan { get; set; } = string.Empty;

    [MaxLength(255)]
    public string? AlamatSelamaCuti { get; set; }

    [MaxLength(50)]
    public string? NomorTeleponDarurat { get; set; }

    [MaxLength(255)]
    public string? NamaDokumenPendukung { get; set; }

    [MaxLength(500)]
    public string? JalurDokumenPendukung { get; set; }

    public StatusPengajuanCuti Status { get; set; } = StatusPengajuanCuti.MenungguPersetujuanManager;

    // Relasi ke riwayat persetujuan
    public virtual ICollection<PersetujuanCuti> DaftarPersetujuan { get; set; } = new List<PersetujuanCuti>();
}
