using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemHRD.Models.Entitas;

/// <summary>
/// Entitas Jabatan / Posisi dalam suatu Departemen.
/// </summary>
public class Jabatan : EntitasTenantDasar
{
    [Required]
    [MaxLength(100)]
    public string NamaJabatan { get; set; } = string.Empty;

    [Required]
    public Guid DepartemenId { get; set; }

    [ForeignKey(nameof(DepartemenId))]
    public virtual Departemen? Departemen { get; set; }

    [MaxLength(255)]
    public string? Deskripsi { get; set; }

    /// <summary>
    /// Level jabatan: 1 = Staff/Pelaksana, 2 = Senior, 3 = Supervisor, 4 = Manager, 5 = Direktur/VP
    /// </summary>
    public int LevelJabatan { get; set; } = 1;

    public bool StatusAktif { get; set; } = true;

    // Koleksi Relasi
    public virtual ICollection<Karyawan> DaftarKaryawan { get; set; } = new List<Karyawan>();
}
