using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemHRD.Models.Entitas;

/// <summary>
/// Entitas Departemen / Divisi dalam struktur organisasi perusahaan.
/// </summary>
public class Departemen : EntitasTenantDasar
{
    [Required]
    [MaxLength(20)]
    public string KodeDepartemen { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string NamaDepartemen { get; set; } = string.Empty;

    [MaxLength(255)]
    public string? Deskripsi { get; set; }

    public Guid? KepalaDepartemenId { get; set; }

    [ForeignKey(nameof(KepalaDepartemenId))]
    public virtual Karyawan? KepalaDepartemen { get; set; }

    public bool StatusAktif { get; set; } = true;

    // Koleksi Relasi
    public virtual ICollection<Jabatan> DaftarJabatan { get; set; } = new List<Jabatan>();
    public virtual ICollection<Karyawan> DaftarKaryawan { get; set; } = new List<Karyawan>();
}
