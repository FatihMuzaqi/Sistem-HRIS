using System.ComponentModel.DataAnnotations;
using SistemHRD.Models.Enums;

namespace SistemHRD.Models.Entitas;

/// <summary>
/// Entitas Siklus Periode Penilaian Kinerja Karyawan (Performance Appraisal Period).
/// </summary>
public class PeriodePenilaian : EntitasTenantDasar
{
    [Required]
    [MaxLength(100)]
    public string NamaPeriode { get; set; } = string.Empty;

    public int Tahun { get; set; } = DateTime.Today.Year;

    [MaxLength(50)]
    public string? SemesterAtauKuartal { get; set; }

    public DateTime TanggalMulai { get; set; }

    public DateTime TanggalSelesai { get; set; }

    [Required]
    public StatusPeriodePenilaian Status { get; set; } = StatusPeriodePenilaian.Draf;

    [MaxLength(500)]
    public string? Catatan { get; set; }

    // Koleksi Relasi
    public virtual ICollection<PenilaianKinerja> DaftarPenilaianKinerja { get; set; } = new List<PenilaianKinerja>();
}
