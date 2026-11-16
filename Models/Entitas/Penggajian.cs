using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SistemHRD.Models.Enums;

namespace SistemHRD.Models.Entitas;

/// <summary>
/// Catatan penggajian per karyawan untuk suatu periode penggajian tertentu.
/// </summary>
public class Penggajian : EntitasTenantDasar
{
    [Required]
    public Guid PeriodePenggajianId { get; set; }

    [ForeignKey(nameof(PeriodePenggajianId))]
    public virtual PeriodePenggajian? PeriodePenggajian { get; set; }

    [Required]
    public Guid KaryawanId { get; set; }

    [ForeignKey(nameof(KaryawanId))]
    public virtual Karyawan? Karyawan { get; set; }

    [Required]
    [MaxLength(50)]
    public string NomorSlipGaji { get; set; } = string.Empty;

    public StatusPTKP StatusPTKP { get; set; } = StatusPTKP.TK0;

    public MetodePajakPPh21 MetodePajak { get; set; } = MetodePajakPPh21.TER_PP58;

    // Data Rekapitulasi Kehadiran & Lembur (Snapshot saat penggajian dihitung)
    public int HariKerjaEfektif { get; set; } = 0;

    public int JumlahHadir { get; set; } = 0;

    public int JumlahTerlambat { get; set; } = 0;

    public int TotalMenitTerlambat { get; set; } = 0;

    [Column(TypeName = "decimal(4,2)")]
    public decimal JumlahJamLembur { get; set; } = 0;

    // Rincian Akumulasi Finansial
    [Column(TypeName = "decimal(18,2)")]
    public decimal GajiPokok { get; set; } = 0;

    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalTunjanganTetap { get; set; } = 0;

    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalTunjanganTidakTetap { get; set; } = 0;

    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalUangLembur { get; set; } = 0;

    /// <summary>
    /// Total seluruh pendapatan kotor (Gaji Pokok + Tunjangan Tetap + Tunjangan Variabel + Lembur).
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalPendapatanKotor { get; set; } = 0;

    // Rincian Akumulasi Potongan
    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalPotonganBpjs { get; set; } = 0;

    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalPotonganPph21 { get; set; } = 0;

    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalPotonganKeterlambatan { get; set; } = 0;

    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalPotonganLainnya { get; set; } = 0;

    /// <summary>
    /// Total seluruh potongan (BPJS + Pajak + Denda Telat + Kasbon/Lainnya).
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalPotongan { get; set; } = 0;

    /// <summary>
    /// Gaji Bersih (Take Home Pay) = TotalPendapatanKotor - TotalPotongan.
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal GajiBersih { get; set; } = 0;

    // Biaya Tambahan Perusahaan (Bukan pengurang gaji karyawan, dicatat untuk laporan finansial)
    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalBpjsDitanggungPerusahaan { get; set; } = 0;

    public StatusPenggajian Status { get; set; } = StatusPenggajian.Draf;

    /// <summary>
    /// Menandakan apakah penggajian telah terkunci (disetujui).
    /// Payroll yang terkunci dilarang diubah tanpa prosedur koreksi tercatat.
    /// </summary>
    public bool ApakahTerkunci { get; set; } = false;

    [MaxLength(500)]
    public string? Catatan { get; set; }

    public Guid? DisetujuiOlehId { get; set; }

    [ForeignKey(nameof(DisetujuiOlehId))]
    public virtual Pengguna? DisetujuiOleh { get; set; }

    public DateTimeOffset? WaktuDisetujui { get; set; }

    // Relasi navigasi
    public virtual ICollection<DetailPenggajian> DaftarDetailPenggajian { get; set; } = new List<DetailPenggajian>();
    public virtual SlipGaji? SlipGaji { get; set; }
}
