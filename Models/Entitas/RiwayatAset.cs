using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SistemHRD.Models.Enums;

namespace SistemHRD.Models.Entitas;

/// <summary>
/// Entitas Log Riwayat Aktivitas & Perjalanan Hidup Aset.
/// </summary>
public class RiwayatAset : EntitasTenantDasar
{
    [Required]
    public Guid AsetPerusahaanId { get; set; }

    [ForeignKey(nameof(AsetPerusahaanId))]
    public virtual AsetPerusahaan AsetPerusahaan { get; set; } = null!;

    [Required]
    public TipeAktivitasAset TipeAktivitas { get; set; } = TipeAktivitasAset.Penyerahan;

    [Required]
    public DateTime TanggalAktivitas { get; set; } = DateTime.Now;

    public Guid? KaryawanTerkaitId { get; set; }

    [ForeignKey(nameof(KaryawanTerkaitId))]
    public virtual Karyawan? KaryawanTerkait { get; set; }

    [Required]
    [MaxLength(150)]
    public string NamaPetugas { get; set; } = string.Empty;

    public KondisiAset KondisiSebelumnya { get; set; }

    public KondisiAset KondisiTerbaru { get; set; }

    [Required]
    public string Deskripsi { get; set; } = string.Empty;

    [Column(TypeName = "decimal(18,2)")]
    public decimal? BiayaPerbaikan { get; set; }
}
