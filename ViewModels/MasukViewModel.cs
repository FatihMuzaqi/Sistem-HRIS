using System.ComponentModel.DataAnnotations;

namespace SistemHRD.ViewModels;

/// <summary>
/// Model data untuk form login autentikasi pengguna.
/// </summary>
public class MasukViewModel
{
    [Required(ErrorMessage = "Email wajib diisi.")]
    [EmailAddress(ErrorMessage = "Format email tidak valid.")]
    [Display(Name = "Alamat Email")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Kata sandi wajib diisi.")]
    [DataType(DataType.Password)]
    [Display(Name = "Kata Sandi")]
    public string KataSandi { get; set; } = string.Empty;

    [Display(Name = "Ingat Sesi Saya")]
    public bool IngatSaya { get; set; } = false;

    public string? ReturnUrl { get; set; }
}
