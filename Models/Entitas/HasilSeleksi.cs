using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SistemHRD.Models.Enums;

namespace SistemHRD.Models.Entitas;

/// <summary>
/// Entitas Catatan Evaluasi dan Hasil Seleksi / Wawancara Kandidat.
/// </summary>
public class HasilSeleksi : EntitasTenantDasar
{
    [Required]
    public Guid PelamarId { get; set; }

    [ForeignKey(nameof(PelamarId))]
    public virtual Pelamar? Pelamar { get; set; }

    public Guid? TahapanSeleksiId { get; set; }

    [ForeignKey(nameof(TahapanSeleksiId))]
    public virtual TahapanSeleksi? TahapanSeleksi { get; set; }

    public Guid? JadwalWawancaraId { get; set; }

    [ForeignKey(nameof(JadwalWawancaraId))]
    public virtual JadwalWawancara? JadwalWawancara { get; set; }

    [Required]
    public Guid PenilaiId { get; set; }

    [ForeignKey(nameof(PenilaiId))]
    public virtual Pengguna? Penilai { get; set; }

    [Required]
    [MaxLength(150)]
    public string NamaPenilai { get; set; } = string.Empty;

    public DateTime TanggalPenilaian { get; set; } = DateTime.Today;

    [Column(TypeName = "decimal(5,2)")]
    public decimal Skor { get; set; }

    [Required]
    public string CatatanEvaluasi { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Kelebihan { get; set; }

    [MaxLength(500)]
    public string? Kekurangan { get; set; }

    [Required]
    public HasilKeputusanSeleksi Keputusan { get; set; } = HasilKeputusanSeleksi.Lolos;
}
