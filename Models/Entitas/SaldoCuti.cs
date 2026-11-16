using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemHRD.Models.Entitas;

/// <summary>
/// Saldo hak cuti tahunan per karyawan dan jenis cuti untuk suatu periode tahun.
/// </summary>
public class SaldoCuti : EntitasTenantDasar
{
    [Required]
    public Guid KaryawanId { get; set; }

    [ForeignKey(nameof(KaryawanId))]
    public virtual Karyawan? Karyawan { get; set; }

    [Required]
    public Guid JenisCutiId { get; set; }

    [ForeignKey(nameof(JenisCutiId))]
    public virtual JenisCuti? JenisCuti { get; set; }

    public int PeriodeTahun { get; set; }

    public int JumlahHakCuti { get; set; } = 12;

    public int CutiTerpakai { get; set; } = 0;

    public int SisaCuti { get; set; } = 12;

    [MaxLength(255)]
    public string? Catatan { get; set; }
}
