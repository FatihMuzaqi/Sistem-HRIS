using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SistemHRD.Models.Enums;

namespace SistemHRD.Models.Entitas;

/// <summary>
/// Entitas Alokasi & Berita Acara Serah Terima (BAST) Penyerahan Aset ke Karyawan.
/// </summary>
public class PenyerahanAset : EntitasTenantDasar
{
    [Required]
    public Guid AsetPerusahaanId { get; set; }

    [ForeignKey(nameof(AsetPerusahaanId))]
    public virtual AsetPerusahaan AsetPerusahaan { get; set; } = null!;

    [Required]
    public Guid KaryawanId { get; set; }

    [ForeignKey(nameof(KaryawanId))]
    public virtual Karyawan Karyawan { get; set; } = null!;

    [Required]
    [MaxLength(50)]
    public string NomorDokumen { get; set; } = string.Empty;

    [Required]
    public DateTime TanggalPenyerahan { get; set; } = DateTime.Today;

    public DateTime? TanggalTargetPengembalian { get; set; }

    public DateTime? TanggalRealisasiPengembalian { get; set; }

    [Required]
    public KondisiAset KondisiSaatPenyerahan { get; set; } = KondisiAset.SangatBaik;

    public KondisiAset? KondisiSaatPengembalian { get; set; }

    [Required]
    [MaxLength(50)]
    public string StatusPeminjaman { get; set; } = "Digunakan";

    [MaxLength(255)]
    public string? KelengkapanAset { get; set; }

    [MaxLength(500)]
    public string? CatatanPenyerahan { get; set; }

    [MaxLength(500)]
    public string? CatatanPengembalian { get; set; }

    public Guid DiserahkanOlehId { get; set; }

    [ForeignKey(nameof(DiserahkanOlehId))]
    public virtual Pengguna? DiserahkanOleh { get; set; }

    public Guid? DiterimaKembaliOlehId { get; set; }

    [ForeignKey(nameof(DiterimaKembaliOlehId))]
    public virtual Pengguna? DiterimaKembaliOleh { get; set; }
}
