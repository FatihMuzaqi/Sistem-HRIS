using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SistemHRD.Models.Enums;

namespace SistemHRD.Models.Entitas;

/// <summary>
/// Entitas Calon Pelamar / Kandidat Rekrutmen.
/// </summary>
public class Pelamar : EntitasTenantDasar
{
    [Required]
    public Guid LowonganKerjaId { get; set; }

    [ForeignKey(nameof(LowonganKerjaId))]
    public virtual LowonganKerja? LowonganKerja { get; set; }

    [Required]
    [MaxLength(50)]
    public string NomorRegistrasi { get; set; } = string.Empty;

    [Required]
    [MaxLength(150)]
    public string NamaLengkap { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MaxLength(30)]
    public string NomorTelepon { get; set; } = string.Empty;

    [MaxLength(50)]
    public string? PendidikanTerakhir { get; set; }

    [MaxLength(150)]
    public string? NamaInstitusi { get; set; }

    [MaxLength(100)]
    public string? Jurusan { get; set; }

    [Column(TypeName = "decimal(4,1)")]
    public decimal PengalamanKerjaTahun { get; set; } = 0;

    [Column(TypeName = "decimal(18,2)")]
    public decimal? GajiDiharapkan { get; set; }

    [Required]
    [MaxLength(255)]
    public string NamaFileCV { get; set; } = string.Empty;

    [Required]
    [MaxLength(500)]
    public string JalurFileCV { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? CatatanAdministrasi { get; set; }

    [Required]
    public StatusPelamar Status { get; set; } = StatusPelamar.Baru;

    public Guid? KaryawanId { get; set; }

    [ForeignKey(nameof(KaryawanId))]
    public virtual Karyawan? Karyawan { get; set; }

    // Koleksi Relasi
    public virtual ICollection<JadwalWawancara> DaftarJadwalWawancara { get; set; } = new List<JadwalWawancara>();
    public virtual ICollection<HasilSeleksi> DaftarHasilSeleksi { get; set; } = new List<HasilSeleksi>();
}
