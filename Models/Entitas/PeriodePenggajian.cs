using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SistemHRD.Models.Enums;

namespace SistemHRD.Models.Entitas;

/// <summary>
/// Siklus periode penggajian bulanan per perusahaan.
/// </summary>
public class PeriodePenggajian : EntitasTenantDasar
{
    [Required]
    [MaxLength(100)]
    public string NamaPeriode { get; set; } = string.Empty;

    [Range(1, 12)]
    public int Bulan { get; set; }

    [Range(2020, 2100)]
    public int Tahun { get; set; }

    public DateTime TanggalMulai { get; set; }

    public DateTime TanggalSelesai { get; set; }

    public DateTime TanggalPembayaran { get; set; }

    public StatusPeriodePenggajian Status { get; set; } = StatusPeriodePenggajian.Draf;

    public int TotalKaryawan { get; set; } = 0;

    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalPengeluaranGaji { get; set; } = 0;

    [MaxLength(500)]
    public string? Catatan { get; set; }

    public Guid? DiajukanOlehId { get; set; }

    [ForeignKey(nameof(DiajukanOlehId))]
    public virtual Pengguna? DiajukanOleh { get; set; }

    public DateTimeOffset? WaktuDiajukan { get; set; }

    public Guid? DisetujuiOlehId { get; set; }

    [ForeignKey(nameof(DisetujuiOlehId))]
    public virtual Pengguna? DisetujuiOleh { get; set; }

    public DateTimeOffset? WaktuDisetujui { get; set; }

    // Relasi navigasi
    public virtual ICollection<Penggajian> DaftarPenggajian { get; set; } = new List<Penggajian>();
}
