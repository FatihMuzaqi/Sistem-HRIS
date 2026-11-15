using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemHRD.Common.Constants;
using SistemHRD.Data;
using SistemHRD.Models.Entitas;
using SistemHRD.Services.Interfaces;
using SistemHRD.ViewModels;

namespace SistemHRD.Controllers;

/// <summary>
/// Controller untuk penanganan alur autentikasi pengguna: Login, Logout, dan Halaman Akses Ditolak.
/// </summary>
public class AutentikasiController : Controller
{
    private readonly SignInManager<Pengguna> _signInManager;
    private readonly UserManager<Pengguna> _userManager;
    private readonly KonteksAplikasi _context;
    private readonly ILayananAudit _layananAudit;

    public AutentikasiController(
        SignInManager<Pengguna> signInManager,
        UserManager<Pengguna> userManager,
        KonteksAplikasi context,
        ILayananAudit layananAudit)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _context = context;
        _layananAudit = layananAudit;
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Masuk(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Index", "Dasbor");
        }

        ViewData["ReturnUrl"] = returnUrl;
        return View(new MasukViewModel { ReturnUrl = returnUrl });
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Masuk(MasukViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        // Cari pengguna berdasarkan email (abaikan query filter multi-tenant untuk proses login)
        var user = await _context.Users
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Email == model.Email);

        if (user == null || user.IsDeleted)
        {
            await _layananAudit.CatatAsync("LOGIN_GAGAL", "Pengguna", model.Email, null, "Email tidak terdaftar.");
            ModelState.AddModelError(string.Empty, "Email atau kata sandi yang Anda masukkan salah.");
            return View(model);
        }

        if (!user.StatusAktif)
        {
            await _layananAudit.CatatAsync("LOGIN_DITOLAK", "Pengguna", user.Id.ToString(), null, "Akun pengguna dinonaktifkan.");
            ModelState.AddModelError(string.Empty, "Akun Anda saat ini dinonaktifkan. Silakan hubungi Administrator.");
            return View(model);
        }

        Perusahaan? perusahaan = null;
        // Jika pengguna terikat perusahaan, validasi status perusahaan
        if (user.PerusahaanId.HasValue)
        {
            perusahaan = await _context.Perusahaan
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(p => p.Id == user.PerusahaanId.Value);

            if (perusahaan == null || perusahaan.IsDeleted || !perusahaan.StatusAktif)
            {
                await _layananAudit.CatatAsync("LOGIN_DITOLAK", "Perusahaan", user.PerusahaanId.ToString(), null, "Perusahaan nonaktif.");
                ModelState.AddModelError(string.Empty, "Perusahaan tempat Anda terdaftar saat ini berstatus non-aktif.");
                return View(model);
            }
        }

        // Validasi kredensial password
        var passwordValid = await _userManager.CheckPasswordAsync(user, model.KataSandi);
        if (!passwordValid)
        {
            await _userManager.AccessFailedAsync(user);
            await _layananAudit.CatatAsync("LOGIN_GAGAL", "Pengguna", user.Id.ToString(), null, "Kata sandi salah.");
            ModelState.AddModelError(string.Empty, "Email atau kata sandi yang Anda masukkan salah.");
            return View(model);
        }

        // Reset hitungan percobaan gagal
        await _userManager.ResetAccessFailedCountAsync(user);

        // Siapkan klaim kustom untuk konteks Multi-Tenant & Identitas
        var customClaims = new List<Claim>
        {
            new Claim("NamaLengkap", user.NamaLengkap),
            new Claim(ClaimTypes.Name, user.Email ?? user.UserName ?? string.Empty),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
        };

        if (perusahaan != null)
        {
            customClaims.Add(new Claim("PerusahaanId", perusahaan.Id.ToString()));
            customClaims.Add(new Claim("KodePerusahaan", perusahaan.KodePerusahaan));
            customClaims.Add(new Claim("NamaPerusahaan", perusahaan.NamaPerusahaan));
        }

        // Buat sesi cookie login pengguna
        await _signInManager.SignInWithClaimsAsync(user, model.IngatSaya, customClaims);

        // Catat jejak audit login berhasil
        await _layananAudit.CatatAsync("LOGIN_BERHASIL", "Pengguna", user.Id.ToString(), null, $"Login berhasil dari {user.Email}");

        // Redirect sesuai ReturnUrl jika aman atau ke Dasbor
        if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
        {
            return Redirect(model.ReturnUrl);
        }

        return RedirectToAction("Index", "Dasbor");
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Keluar()
    {
        await _layananAudit.CatatAsync("LOGOUT", "Pengguna", User.Identity?.Name, null, "Pengguna berhasil logout");
        await _signInManager.SignOutAsync();
        TempData["PesanSukses"] = "Anda telah berhasil keluar dari sistem.";
        return RedirectToAction(nameof(Masuk));
    }

    [HttpGet]
    public IActionResult AksesDitolak(string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }
}
