using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SistemHRD.Models.Enums;

namespace SistemHRD.Models.Entitas;

/// <summary>
/// Entitas Tahapan Seleksi (Hiring Pipeline) per Lowongan Pekerjaan.
/// </summary>
public class TahapanSeleksi : EntitasTenantDasar
{
    [Required]
    public Guid LowonganKerjaId { get; set; }

    [ForeignKey(nameof(LowonganKerjaId))]
    public virtual LowonganKerja? LowonganKerja { get; set; }

    [Required]
    [MaxLength(100)]
    public string NamaTahapan { get; set; } = string.Empty;

    [Required]
    public JenisTahapanSeleksi JenisTahapan { get; set; } = JenisTahapanSeleksi.WawancaraHR;

    public int Urutan { get; set; } = 1;

    [MaxLength(255)]
    public string? Keterangan { get; set; }

    public bool StatusAktif { get; set; } = true;
}
