using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemHRD.Models.Entitas;

/// <summary>
/// Master konfigurasi tipe cuti dan izin resmi perusahaan.
/// </summary>
public class JenisCuti : EntitasTenantDasar
{
    [Required]
    [MaxLength(100)]
    public string NamaJenisCuti { get; set; } = string.Empty;

    [Required]
    [MaxLength(20)]
    public string KodeJenisCuti { get; set; } = string.Empty;

    public int JatahHariPerTahun { get; set; } = 12;

    public bool PotongCutiTahunan { get; set; } = true;

    public bool WajibLampiran { get; set; } = false;

    public bool StatusAktif { get; set; } = true;

    [MaxLength(255)]
    public string? Keterangan { get; set; }

    // Relasi
    public virtual ICollection<SaldoCuti> DaftarSaldoCuti { get; set; } = new List<SaldoCuti>();
    public virtual ICollection<PengajuanCuti> DaftarPengajuanCuti { get; set; } = new List<PengajuanCuti>();
}
