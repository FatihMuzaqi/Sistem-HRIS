using System.ComponentModel.DataAnnotations;

namespace SistemHRD.ViewModels;

/// <summary>
/// Model data untuk tampilan dan pembaruan profil pengguna.
/// </summary>
public class ProfilViewModel
{
    public Guid Id { get; set; }

    [Display(Name = "Alamat Email")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Nama lengkap wajib diisi.")]
    [MaxLength(150, ErrorMessage = "Nama lengkap maksimal 150 karakter.")]
    [Display(Name = "Nama Lengkap")]
    public string NamaLengkap { get; set; } = string.Empty;

    [Phone(ErrorMessage = "Nomor telepon tidak valid.")]
    [MaxLength(30)]
    [Display(Name = "Nomor Telepon")]
    public string? NomorTelepon { get; set; }

    [Display(Name = "Perusahaan")]
    public string? NamaPerusahaan { get; set; }

    [Display(Name = "Kode Perusahaan")]
    public string? KodePerusahaan { get; set; }

    [Display(Name = "Hak Akses / Peran")]
    public string PeranUtama { get; set; } = string.Empty;

    [Display(Name = "Bergabung Sejak")]
    public DateTimeOffset CreatedAt { get; set; }
}

/// <summary>
/// Model data untuk form pengubahan kata sandi akun pengguna.
/// </summary>
public class UbahKataSandiViewModel
{
    [Required(ErrorMessage = "Kata sandi saat ini wajib diisi.")]
    [DataType(DataType.Password)]
    [Display(Name = "Kata Sandi Saat Ini")]
    public string KataSandiLama { get; set; } = string.Empty;

    [Required(ErrorMessage = "Kata sandi baru wajib diisi.")]
    [StringLength(100, ErrorMessage = "{0} minimal harus {2} karakter.", MinimumLength = 6)]
    [DataType(DataType.Password)]
    [Display(Name = "Kata Sandi Baru")]
    public string KataSandiBaru { get; set; } = string.Empty;

    [DataType(DataType.Password)]
    [Display(Name = "Konfirmasi Kata Sandi Baru")]
    [Compare("KataSandiBaru", ErrorMessage = "Konfirmasi kata sandi tidak cocok dengan kata sandi baru.")]
    public string KonfirmasiKataSandi { get; set; } = string.Empty;
}
