using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SistemHRD.Models.Enums;

namespace SistemHRD.Models.Entitas;

/// <summary>
/// Entitas Lowongan Pekerjaan (Job Vacancy) per Perusahaan.
/// </summary>
public class LowonganKerja : EntitasTenantDasar
{
    [Required]
    [MaxLength(50)]
    public string KodeLowongan { get; set; } = string.Empty;

    [Required]
    [MaxLength(150)]
    public string JudulLowongan { get; set; } = string.Empty;

    public Guid? DepartemenId { get; set; }

    [ForeignKey(nameof(DepartemenId))]
    public virtual Departemen? Departemen { get; set; }

    public Guid? JabatanId { get; set; }

    [ForeignKey(nameof(JabatanId))]
    public virtual Jabatan? Jabatan { get; set; }

    [Required]
    public string DeskripsiPekerjaan { get; set; } = string.Empty;

    [Required]
    public string Kualifikasi { get; set; } = string.Empty;

    [Required]
    public TipePekerjaan TipePekerjaan { get; set; } = TipePekerjaan.PenuhWaktu;

    [Required]
    [MaxLength(100)]
    public string LokasiKerja { get; set; } = "Head Office";

    public int JumlahDibutuhkan { get; set; } = 1;

    [Column(TypeName = "decimal(18,2)")]
    public decimal? GajiMin { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal? GajiMax { get; set; }

    public DateTime TanggalDibuka { get; set; } = DateTime.Today;

    public DateTime? TanggalDitutup { get; set; }

    [Required]
    public StatusLowongan Status { get; set; } = StatusLowongan.Draf;

    // Koleksi Relasi
    public virtual ICollection<Pelamar> DaftarPelamar { get; set; } = new List<Pelamar>();
    public virtual ICollection<TahapanSeleksi> DaftarTahapanSeleksi { get; set; } = new List<TahapanSeleksi>();
}
