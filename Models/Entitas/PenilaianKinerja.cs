using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SistemHRD.Models.Enums;

namespace SistemHRD.Models.Entitas;

/// <summary>
/// Entitas Evaluasi dan Penilaian Kinerja Karyawan per Periode.
/// </summary>
public class PenilaianKinerja : EntitasTenantDasar
{
    [Required]
    public Guid PeriodePenilaianId { get; set; }

    [ForeignKey(nameof(PeriodePenilaianId))]
    public virtual PeriodePenilaian? PeriodePenilaian { get; set; }

    [Required]
    public Guid KaryawanId { get; set; }

    [ForeignKey(nameof(KaryawanId))]
    public virtual Karyawan? Karyawan { get; set; }

    [Required]
    public Guid PenilaiId { get; set; }

    [ForeignKey(nameof(PenilaiId))]
    public virtual Pengguna? Penilai { get; set; }

    [Required]
    [MaxLength(150)]
    public string NamaPenilai { get; set; } = string.Empty;

    public DateTime TanggalPenilaian { get; set; } = DateTime.Today;

    [Column(TypeName = "decimal(5,2)")]
    public decimal TotalSkorAkhir { get; set; }

    [Required]
    public KategoriKinerja PredikatKinerja { get; set; } = KategoriKinerja.Cukup;

    [Required]
    public string KomentarEvaluasi { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? RekomendasiPengembangan { get; set; }

    [Required]
    public StatusPenilaianKinerja Status { get; set; } = StatusPenilaianKinerja.Draf;

    // Koleksi Relasi
    public virtual ICollection<DetailPenilaian> DaftarDetailPenilaian { get; set; } = new List<DetailPenilaian>();
}
