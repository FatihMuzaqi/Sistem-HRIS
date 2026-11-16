using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SistemHRD.Models.Enums;

namespace SistemHRD.Models.Entitas;

/// <summary>
/// Snapshot baris komponen penggajian individual saat slip gaji/payroll dihitung.
/// Menyimpan nama, kode, jenis, sifat, dan nilai nominal statis agar tidak terpengaruh perubahan master di masa depan.
/// </summary>
public class DetailPenggajian : EntitasTenantDasar
{
    [Required]
    public Guid PenggajianId { get; set; }

    [ForeignKey(nameof(PenggajianId))]
    public virtual Penggajian? Penggajian { get; set; }

    public Guid? KomponenGajiId { get; set; }

    [ForeignKey(nameof(KomponenGajiId))]
    public virtual KomponenGaji? KomponenGaji { get; set; }

    [Required]
    [MaxLength(50)]
    public string KodeKomponen { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string NamaKomponen { get; set; } = string.Empty;

    public JenisKomponenGaji Jenis { get; set; } = JenisKomponenGaji.Pendapatan;

    public SifatKomponenGaji Sifat { get; set; } = SifatKomponenGaji.Tetap;

    public KategoriKomponenGaji Kategori { get; set; } = KategoriKomponenGaji.TunjanganTetap;

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal Nominal { get; set; } = 0;

    /// <summary>
    /// Nominal iuran yang ditanggung oleh perusahaan untuk komponen ini (jika ada, e.g. BPJS pemberi kerja).
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal NominalDitanggungPerusahaan { get; set; } = 0;

    [MaxLength(255)]
    public string? Keterangan { get; set; }

    public int Urutan { get; set; } = 0;
}
