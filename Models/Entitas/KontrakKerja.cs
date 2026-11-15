using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SistemHRD.Models.Enums;

namespace SistemHRD.Models.Entitas;

/// <summary>
/// Entitas Kontrak Kerja dan riwayat perjanjian kerja karyawan.
/// </summary>
public class KontrakKerja : EntitasTenantDasar
{
    [Required]
    public Guid KaryawanId { get; set; }

    [ForeignKey(nameof(KaryawanId))]
    public virtual Karyawan? Karyawan { get; set; }

    [Required]
    [MaxLength(50)]
    public string NomorKontrak { get; set; } = string.Empty;

    public JenisKontrak JenisKontrak { get; set; } = JenisKontrak.PKWT;

    public DateTime TanggalMulai { get; set; }

    public DateTime? TanggalBerakhir { get; set; }

    public StatusKontrak StatusKontrak { get; set; } = StatusKontrak.Aktif;

    [MaxLength(255)]
    public string? Keterangan { get; set; }

    [MaxLength(255)]
    public string? DokumenUrl { get; set; }
}
