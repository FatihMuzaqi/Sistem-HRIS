using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using SistemHRD.Models.Entitas;
using SistemHRD.Models.Enums;

namespace SistemHRD.ViewModels;

/// <summary>
/// Model data untuk daftar karyawan dengan fitur pencarian, filter, dan pagination.
/// </summary>
public class KaryawanListViewModel
{
    public List<Karyawan> DaftarKaryawan { get; set; } = new();

    // Parameter Filter & Pencarian
    public string? Cari { get; set; }
    public Guid? DepartemenId { get; set; }
    public Guid? JabatanId { get; set; }
    public StatusKaryawan? Status { get; set; }

    // Parameter Pagination
    public int HalamanSaatIni { get; set; } = 1;
    public int UkuranHalaman { get; set; } = 10;
    public int TotalData { get; set; }
    public int TotalHalaman => (int)Math.Ceiling((double)TotalData / UkuranHalaman);
    public bool AdaHalamanSebelumnya => HalamanSaatIni > 1;
    public bool AdaHalamanBerikutnya => HalamanSaatIni < TotalHalaman;

    // Dropdown Pilihan Filter
    public List<SelectListItem> OpsiDepartemen { get; set; } = new();
    public List<SelectListItem> OpsiJabatan { get; set; } = new();
}

/// <summary>
/// Model data untuk form Tambah dan Ubah Karyawan.
/// </summary>
public class KaryawanFormViewModel
{
    public Guid? Id { get; set; }

    [Required(ErrorMessage = "Nomor Induk Karyawan (NIK) wajib diisi.")]
    [MaxLength(30, ErrorMessage = "NIK maksimal 30 karakter.")]
    [Display(Name = "Nomor Induk Karyawan (NIK)")]
    public string NomorIndukKaryawan { get; set; } = string.Empty;

    [Required(ErrorMessage = "Nama lengkap wajib diisi.")]
    [MaxLength(150, ErrorMessage = "Nama lengkap maksimal 150 karakter.")]
    [Display(Name = "Nama Lengkap")]
    public string NamaLengkap { get; set; } = string.Empty;

    [MaxLength(100)]
    [Display(Name = "Tempat Lahir")]
    public string? TempatLahir { get; set; }

    [Required(ErrorMessage = "Tanggal lahir wajib diisi.")]
    [DataType(DataType.Date)]
    [Display(Name = "Tanggal Lahir")]
    public DateTime TanggalLahir { get; set; } = DateTime.Today.AddYears(-22);

    [Required(ErrorMessage = "Jenis kelamin wajib dipilih.")]
    [Display(Name = "Jenis Kelamin")]
    public JenisKelamin JenisKelamin { get; set; } = JenisKelamin.LakiLaki;

    [MaxLength(255)]
    [Display(Name = "Alamat Domisili")]
    public string? Alamat { get; set; }

    [MaxLength(30)]
    [Phone(ErrorMessage = "Nomor telepon tidak valid.")]
    [Display(Name = "Nomor Telepon")]
    public string? NomorTelepon { get; set; }

    [Required(ErrorMessage = "Alamat email wajib diisi.")]
    [EmailAddress(ErrorMessage = "Format email tidak valid.")]
    [Display(Name = "Alamat Email")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Tanggal masuk wajib diisi.")]
    [DataType(DataType.Date)]
    [Display(Name = "Tanggal Masuk")]
    public DateTime TanggalMasuk { get; set; } = DateTime.Today;

    [Required(ErrorMessage = "Departemen wajib dipilih.")]
    [Display(Name = "Departemen")]
    public Guid DepartemenId { get; set; }

    [Required(ErrorMessage = "Jabatan wajib dipilih.")]
    [Display(Name = "Jabatan")]
    public Guid JabatanId { get; set; }

    [Required(ErrorMessage = "Status karyawan wajib dipilih.")]
    [Display(Name = "Status Karyawan")]
    public StatusKaryawan StatusKaryawan { get; set; } = StatusKaryawan.Tetap;

    [Display(Name = "Foto Profil")]
    public IFormFile? BerkasFoto { get; set; }

    public string? FotoProfilSaatIni { get; set; }

    // Opsi Dropdown
    public List<SelectListItem> OpsiDepartemen { get; set; } = new();
    public List<SelectListItem> OpsiJabatan { get; set; } = new();
}

/// <summary>
/// Model data untuk halaman detail karyawan komprehensif (berbasis tab).
/// </summary>
public class KaryawanDetailViewModel
{
    public Karyawan Karyawan { get; set; } = null!;
    public List<KontrakKerja> DaftarKontrakKerja { get; set; } = new();
    public List<DokumenKaryawan> DaftarDokumen { get; set; } = new();
    public List<KontakDaruratKaryawan> DaftarKontakDarurat { get; set; } = new();
    public List<RiwayatPekerjaan> DaftarRiwayatPekerjaan { get; set; } = new();

    // Form inputs modal untuk tab-tab di halaman detail
    public KontrakKerjaFormViewModel FormKontrak { get; set; } = new();
    public DokumenUploadViewModel FormDokumen { get; set; } = new();
    public KontakDaruratFormViewModel FormKontakDarurat { get; set; } = new();
    public RiwayatPekerjaanFormViewModel FormRiwayatPekerjaan { get; set; } = new();
}

public class KontrakKerjaFormViewModel
{
    public Guid KaryawanId { get; set; }

    [Required(ErrorMessage = "Nomor kontrak wajib diisi.")]
    [MaxLength(50)]
    public string NomorKontrak { get; set; } = string.Empty;

    [Required]
    public JenisKontrak JenisKontrak { get; set; } = JenisKontrak.PKWT;

    [Required]
    [DataType(DataType.Date)]
    public DateTime TanggalMulai { get; set; } = DateTime.Today;

    [DataType(DataType.Date)]
    public DateTime? TanggalBerakhir { get; set; }

    [Required]
    public StatusKontrak StatusKontrak { get; set; } = StatusKontrak.Aktif;

    [MaxLength(255)]
    public string? Keterangan { get; set; }
}

public class DokumenUploadViewModel
{
    public Guid KaryawanId { get; set; }

    [Required(ErrorMessage = "Nama dokumen wajib diisi.")]
    [MaxLength(150)]
    public string NamaDokumen { get; set; } = string.Empty;

    [Required]
    public JenisDokumen JenisDokumen { get; set; } = JenisDokumen.Lainnya;

    [Required(ErrorMessage = "Berkas dokumen wajib diunggah.")]
    public IFormFile BerkasDokumen { get; set; } = null!;

    [MaxLength(255)]
    public string? Keterangan { get; set; }
}

public class KontakDaruratFormViewModel
{
    public Guid KaryawanId { get; set; }

    [Required(ErrorMessage = "Nama lengkap kontak wajib diisi.")]
    [MaxLength(150)]
    public string NamaLengkap { get; set; } = string.Empty;

    [Required(ErrorMessage = "Hubungan keluarga wajib diisi.")]
    [MaxLength(50)]
    public string Hubungan { get; set; } = string.Empty;

    [Required(ErrorMessage = "Nomor telepon wajib diisi.")]
    [MaxLength(30)]
    public string NomorTelepon { get; set; } = string.Empty;

    [MaxLength(255)]
    public string? Alamat { get; set; }
}

public class RiwayatPekerjaanFormViewModel
{
    public Guid KaryawanId { get; set; }

    [Required(ErrorMessage = "Nama perusahaan sebelumnya wajib diisi.")]
    [MaxLength(150)]
    public string NamaPerusahaanSebelumnya { get; set; } = string.Empty;

    [Required(ErrorMessage = "Jabatan terakhir wajib diisi.")]
    [MaxLength(100)]
    public string JabatanTerakhir { get; set; } = string.Empty;

    [Required]
    public int TahunMulai { get; set; } = DateTime.Today.Year - 3;

    [Required]
    public int TahunSelesai { get; set; } = DateTime.Today.Year - 1;

    [MaxLength(500)]
    public string? DeskripsiTugas { get; set; }
}

public class DepartemenFormViewModel
{
    public Guid? Id { get; set; }

    [Required(ErrorMessage = "Kode departemen wajib diisi.")]
    [MaxLength(20, ErrorMessage = "Kode departemen maksimal 20 karakter.")]
    [Display(Name = "Kode Departemen")]
    public string KodeDepartemen { get; set; } = string.Empty;

    [Required(ErrorMessage = "Nama departemen wajib diisi.")]
    [MaxLength(100, ErrorMessage = "Nama departemen maksimal 100 karakter.")]
    [Display(Name = "Nama Departemen")]
    public string NamaDepartemen { get; set; } = string.Empty;

    [MaxLength(255)]
    [Display(Name = "Deskripsi")]
    public string? Deskripsi { get; set; }

    [Display(Name = "Kepala Departemen")]
    public Guid? KepalaDepartemenId { get; set; }

    [Display(Name = "Status Aktif")]
    public bool StatusAktif { get; set; } = true;

    public List<SelectListItem> OpsiKaryawan { get; set; } = new();
}

public class JabatanFormViewModel
{
    public Guid? Id { get; set; }

    [Required(ErrorMessage = "Nama jabatan wajib diisi.")]
    [MaxLength(100, ErrorMessage = "Nama jabatan maksimal 100 karakter.")]
    [Display(Name = "Nama Jabatan")]
    public string NamaJabatan { get; set; } = string.Empty;

    [Required(ErrorMessage = "Departemen wajib dipilih.")]
    [Display(Name = "Departemen")]
    public Guid DepartemenId { get; set; }

    [MaxLength(255)]
    [Display(Name = "Deskripsi")]
    public string? Deskripsi { get; set; }

    [Required]
    [Range(1, 5, ErrorMessage = "Level jabatan harus antara 1 sampai 5.")]
    [Display(Name = "Level Jabatan (1 = Staff, 5 = Direktur)")]
    public int LevelJabatan { get; set; } = 1;

    [Display(Name = "Status Aktif")]
    public bool StatusAktif { get; set; } = true;

    public List<SelectListItem> OpsiDepartemen { get; set; } = new();
}
