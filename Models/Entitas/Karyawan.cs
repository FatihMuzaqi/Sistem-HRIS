using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SistemHRD.Models.Enums;

namespace SistemHRD.Models.Entitas;

/// <summary>
/// Entitas data induk Karyawan yang terikat pada Perusahaan (Multi-Tenant).
/// </summary>
public class Karyawan : EntitasTenantDasar
{
    [Required]
    [MaxLength(30)]
    public string NomorIndukKaryawan { get; set; } = string.Empty;

    [Required]
    [MaxLength(150)]
    public string NamaLengkap { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? TempatLahir { get; set; }

    public DateTime TanggalLahir { get; set; }

    public JenisKelamin JenisKelamin { get; set; } = JenisKelamin.LakiLaki;

    [MaxLength(255)]
    public string? Alamat { get; set; }

    [MaxLength(30)]
    public string? NomorTelepon { get; set; }

    [MaxLength(100)]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    public DateTime TanggalMasuk { get; set; }

    [Required]
    public Guid DepartemenId { get; set; }

    [ForeignKey(nameof(DepartemenId))]
    public virtual Departemen? Departemen { get; set; }

    [Required]
    public Guid JabatanId { get; set; }

    [ForeignKey(nameof(JabatanId))]
    public virtual Jabatan? Jabatan { get; set; }

    public StatusKaryawan StatusKaryawan { get; set; } = StatusKaryawan.Tetap;

    [MaxLength(255)]
    public string? FotoProfil { get; set; }

    /// <summary>
    /// Status Penghasilan Tidak Kena Pajak (PTKP) untuk perhitungan PPh 21.
    /// </summary>
    public StatusPTKP StatusPTKP { get; set; } = StatusPTKP.TK0;

    [MaxLength(50)]
    public string? NamaBank { get; set; }

    [MaxLength(50)]
    public string? NomorRekeningBank { get; set; }

    [MaxLength(50)]
    public string? NomorNPWP { get; set; }

    [MaxLength(50)]
    public string? NomorBpjsKesehatan { get; set; }

    [MaxLength(50)]
    public string? NomorBpjsKetenagakerjaan { get; set; }

    /// <summary>
    /// ID Akun Pengguna (Identity) jika karyawan memiliki akses ke sistem ESS.
    /// </summary>
    public Guid? PenggunaId { get; set; }

    [ForeignKey(nameof(PenggunaId))]
    public virtual Pengguna? Pengguna { get; set; }

    // Koleksi Relasi
    public virtual ICollection<KontrakKerja> DaftarKontrakKerja { get; set; } = new List<KontrakKerja>();
    public virtual ICollection<DokumenKaryawan> DaftarDokumen { get; set; } = new List<DokumenKaryawan>();
    public virtual ICollection<KontakDaruratKaryawan> DaftarKontakDarurat { get; set; } = new List<KontakDaruratKaryawan>();
    public virtual ICollection<RiwayatPekerjaan> DaftarRiwayatPekerjaan { get; set; } = new List<RiwayatPekerjaan>();
    public virtual ICollection<Kehadiran> DaftarKehadiran { get; set; } = new List<Kehadiran>();
    public virtual ICollection<JadwalKerja> DaftarJadwalKerja { get; set; } = new List<JadwalKerja>();
    public virtual ICollection<PengajuanLembur> DaftarPengajuanLembur { get; set; } = new List<PengajuanLembur>();
    public virtual ICollection<SaldoCuti> DaftarSaldoCuti { get; set; } = new List<SaldoCuti>();
    public virtual ICollection<PengajuanCuti> DaftarPengajuanCuti { get; set; } = new List<PengajuanCuti>();
    public virtual ICollection<GajiKaryawan> DaftarGajiKaryawan { get; set; } = new List<GajiKaryawan>();
    public virtual ICollection<Penggajian> DaftarPenggajian { get; set; } = new List<Penggajian>();
}
