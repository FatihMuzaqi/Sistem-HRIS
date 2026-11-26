using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SistemHRD.Models.Enums;

namespace SistemHRD.Models.Entitas;

/// <summary>
/// Entitas Peserta Pelatihan Karyawan.
/// </summary>
public class PesertaPelatihan : EntitasTenantDasar
{
    [Required]
    public Guid ProgramPelatihanId { get; set; }

    [ForeignKey(nameof(ProgramPelatihanId))]
    public virtual ProgramPelatihan ProgramPelatihan { get; set; } = null!;

    [Required]
    public Guid KaryawanId { get; set; }

    [ForeignKey(nameof(KaryawanId))]
    public virtual Karyawan Karyawan { get; set; } = null!;

    [Required]
    public StatusKeikutsertaan StatusKeikutsertaan { get; set; } = StatusKeikutsertaan.Terdaftar;

    [Column(TypeName = "decimal(5,2)")]
    public decimal? NilaiAkhir { get; set; }

    [MaxLength(500)]
    public string? Catatan { get; set; }

    [Required]
    public DateTime TanggalDidaftarkan { get; set; } = DateTime.Today;

    public DateTime? TanggalSelesai { get; set; }

    public virtual SertifikatPelatihan? Sertifikat { get; set; }
}
