using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemHRD.Models.Entitas;

/// <summary>
/// Master konfigurasi shift kerja operasional perusahaan.
/// </summary>
public class ShiftKerja : EntitasTenantDasar
{
    [Required]
    [MaxLength(50)]
    public string NamaShift { get; set; } = string.Empty;

    [Required]
    [MaxLength(20)]
    public string KodeShift { get; set; } = string.Empty;

    public TimeSpan JamMasuk { get; set; } = new TimeSpan(8, 0, 0);

    public TimeSpan JamPulang { get; set; } = new TimeSpan(17, 0, 0);

    public int ToleransiKeterlambatanMenit { get; set; } = 15;

    [MaxLength(255)]
    public string? Keterangan { get; set; }

    public bool StatusAktif { get; set; } = true;

    // Relasi
    public virtual ICollection<JadwalKerja> DaftarJadwalKerja { get; set; } = new List<JadwalKerja>();
    public virtual ICollection<Kehadiran> DaftarKehadiran { get; set; } = new List<Kehadiran>();
}
