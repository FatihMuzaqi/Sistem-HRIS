using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SistemHRD.Models.Enums;

namespace SistemHRD.Models.Entitas;

/// <summary>
/// Entitas penyimpanan berkas dan dokumen penting karyawan (KTP, Ijazah, Kontrak, dll).
/// </summary>
public class DokumenKaryawan : EntitasTenantDasar
{
    [Required]
    public Guid KaryawanId { get; set; }

    [ForeignKey(nameof(KaryawanId))]
    public virtual Karyawan? Karyawan { get; set; }

    [Required]
    [MaxLength(150)]
    public string NamaDokumen { get; set; } = string.Empty;

    public JenisDokumen JenisDokumen { get; set; } = JenisDokumen.Lainnya;

    [Required]
    [MaxLength(255)]
    public string NamaFile { get; set; } = string.Empty;

    public long UkuranFile { get; set; } // dalam bytes

    [MaxLength(100)]
    public string? TipeFile { get; set; }

    [Required]
    [MaxLength(500)]
    public string PathFile { get; set; } = string.Empty;

    [MaxLength(255)]
    public string? Keterangan { get; set; }
}
