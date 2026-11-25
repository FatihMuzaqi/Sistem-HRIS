using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SistemHRD.Models.Enums;

namespace SistemHRD.Models.Entitas;

/// <summary>
/// Entitas Penjadwalan Wawancara Kandidat.
/// </summary>
public class JadwalWawancara : EntitasTenantDasar
{
    [Required]
    public Guid PelamarId { get; set; }

    [ForeignKey(nameof(PelamarId))]
    public virtual Pelamar? Pelamar { get; set; }

    public Guid? TahapanSeleksiId { get; set; }

    [ForeignKey(nameof(TahapanSeleksiId))]
    public virtual TahapanSeleksi? TahapanSeleksi { get; set; }

    public Guid? PewawancaraId { get; set; }

    [ForeignKey(nameof(PewawancaraId))]
    public virtual Pengguna? Pewawancara { get; set; }

    [Required]
    [MaxLength(150)]
    public string NamaPewawancara { get; set; } = string.Empty;

    public DateTime TanggalWawancara { get; set; }

    public TimeSpan JamMulai { get; set; }

    public TimeSpan JamSelesai { get; set; }

    [Required]
    public MetodeWawancara Metode { get; set; } = MetodeWawancara.Daring;

    [MaxLength(255)]
    public string LokasiAtauTautan { get; set; } = string.Empty;

    [Required]
    public StatusWawancara Status { get; set; } = StatusWawancara.Dijadwalkan;

    [MaxLength(500)]
    public string? CatatanUndangan { get; set; }
}
