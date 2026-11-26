using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SistemHRD.Models.Enums;

namespace SistemHRD.Models.Entitas;

/// <summary>
/// Entitas Master Inventaris Aset Perusahaan.
/// </summary>
public class AsetPerusahaan : EntitasTenantDasar
{
    [Required]
    [MaxLength(50)]
    public string NomorAset { get; set; } = string.Empty;

    [Required]
    [MaxLength(150)]
    public string NamaAset { get; set; } = string.Empty;

    [Required]
    public KategoriAset Kategori { get; set; } = KategoriAset.ElektronikLaptop;

    [MaxLength(100)]
    public string? MerkModel { get; set; }

    [MaxLength(100)]
    public string? NomorSeri { get; set; }

    [Required]
    public DateTime TanggalPembelian { get; set; } = DateTime.Today;

    [Column(TypeName = "decimal(18,2)")]
    public decimal NilaiPerolehan { get; set; } = 0;

    [Required]
    public KondisiAset Kondisi { get; set; } = KondisiAset.SangatBaik;

    [Required]
    public StatusAset Status { get; set; } = StatusAset.Tersedia;

    [Required]
    [MaxLength(100)]
    public string LokasiPenyimpanan { get; set; } = "Gudang IT / Operasional";

    public string? DeskripsiSpesifikasi { get; set; }

    public Guid? PemegangSaatIniId { get; set; }

    [ForeignKey(nameof(PemegangSaatIniId))]
    public virtual Karyawan? PemegangSaatIni { get; set; }

    public virtual ICollection<PenyerahanAset> DaftarPenyerahan { get; set; } = new List<PenyerahanAset>();
    public virtual ICollection<RiwayatAset> DaftarRiwayat { get; set; } = new List<RiwayatAset>();
}
