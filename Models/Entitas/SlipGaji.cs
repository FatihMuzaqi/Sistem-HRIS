using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemHRD.Models.Entitas;

/// <summary>
/// Dokumen resmi slip gaji yang diterbitkan dan dapat diakses oleh karyawan melalui ESS.
/// Dilengkapi kode verifikasi unik (hash/token) untuk validasi otentisitas dokumen cetak.
/// </summary>
public class SlipGaji : EntitasTenantDasar
{
    [Required]
    public Guid PenggajianId { get; set; }

    [ForeignKey(nameof(PenggajianId))]
    public virtual Penggajian? Penggajian { get; set; }

    [Required]
    [MaxLength(50)]
    public string NomorSlipGaji { get; set; } = string.Empty;

    [Required]
    [MaxLength(64)]
    public string KodeVerifikasi { get; set; } = string.Empty;

    public DateTimeOffset WaktuDiterbitkan { get; set; } = DateTimeOffset.UtcNow;

    public Guid? DiterbitkanOlehId { get; set; }

    [ForeignKey(nameof(DiterbitkanOlehId))]
    public virtual Pengguna? DiterbitkanOleh { get; set; }

    public bool SudahDilihatKaryawan { get; set; } = false;

    public DateTimeOffset? WaktuDilihatKaryawan { get; set; }

    [MaxLength(500)]
    public string? CatatanKaki { get; set; }
}
