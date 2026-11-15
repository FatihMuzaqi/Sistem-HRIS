using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemHRD.Models.Entitas;

/// <summary>
/// Entitas pencatatan log audit untuk melacak seluruh aktivitas penting dan mutasi data sistem.
/// </summary>
public class AuditLog
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// ID Perusahaan yang terafiliasi. Bernilai null jika aksi dilakukan pada level platform (SuperAdmin).
    /// </summary>
    public Guid? PerusahaanId { get; set; }

    [ForeignKey(nameof(PerusahaanId))]
    public virtual Perusahaan? Perusahaan { get; set; }

    public Guid? PenggunaId { get; set; }

    [ForeignKey(nameof(PenggunaId))]
    public virtual Pengguna? Pengguna { get; set; }

    [MaxLength(150)]
    public string? NamaPengguna { get; set; }

    [Required]
    [MaxLength(50)]
    public string Aksi { get; set; } = string.Empty; // LOGIN, LOGOUT, CREATE, UPDATE, DELETE, AKSES_DITOLAK

    [MaxLength(100)]
    public string? NamaEntitas { get; set; }

    [MaxLength(100)]
    public string? KunciEntitas { get; set; }

    [Column(TypeName = "longtext")]
    public string? DataLama { get; set; }

    [Column(TypeName = "longtext")]
    public string? DataBaru { get; set; }

    [MaxLength(50)]
    public string? AlamatIP { get; set; }

    [MaxLength(255)]
    public string? UserAgent { get; set; }

    public DateTimeOffset Waktu { get; set; } = DateTimeOffset.UtcNow;
}
