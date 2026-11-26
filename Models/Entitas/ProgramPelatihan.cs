using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SistemHRD.Models.Enums;

namespace SistemHRD.Models.Entitas;

/// <summary>
/// Entitas Program Pelatihan Karyawan (Training & Development) per Perusahaan.
/// </summary>
public class ProgramPelatihan : EntitasTenantDasar
{
    [Required]
    [MaxLength(50)]
    public string KodeProgram { get; set; } = string.Empty;

    [Required]
    [MaxLength(150)]
    public string NamaProgram { get; set; } = string.Empty;

    [Required]
    public string Deskripsi { get; set; } = string.Empty;

    [Required]
    [MaxLength(150)]
    public string Penyelenggara { get; set; } = string.Empty;

    [MaxLength(150)]
    public string? Instruktur { get; set; }

    [Required]
    public MetodePelatihan Metode { get; set; } = MetodePelatihan.TatapMuka;

    [Required]
    [MaxLength(255)]
    public string LokasiAtauTautan { get; set; } = string.Empty;

    [Required]
    public DateTime TanggalMulai { get; set; }

    [Required]
    public DateTime TanggalSelesai { get; set; }

    [Column(TypeName = "decimal(5,2)")]
    public decimal TotalJam { get; set; } = 8.00m;

    [Column(TypeName = "decimal(18,2)")]
    public decimal BiayaPerPeserta { get; set; } = 0;

    public int MaksimalPeserta { get; set; } = 30;

    [Required]
    public StatusPelatihan Status { get; set; } = StatusPelatihan.PendaftaranDibuka;

    public Guid? DepartemenId { get; set; }

    [ForeignKey(nameof(DepartemenId))]
    public virtual Departemen? Departemen { get; set; }

    public virtual ICollection<PesertaPelatihan> DaftarPeserta { get; set; } = new List<PesertaPelatihan>();
}
