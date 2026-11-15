using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemHRD.Data;
using SistemHRD.Models.Entitas;
using SistemHRD.Services.Interfaces;
using SistemHRD.ViewModels;

namespace SistemHRD.Controllers;

/// <summary>
/// Controller untuk mengelola profil dan keamanan akun pengguna yang sedang login.
/// </summary>
[Authorize]
public class PenggunaController : Controller
{
    private readonly UserManager<Pengguna> _userManager;
    private readonly SignInManager<Pengguna> _signInManager;
    private readonly KonteksAplikasi _context;
    private readonly ILayananAudit _layananAudit;

    public PenggunaController(
        UserManager<Pengguna> userManager,
        SignInManager<Pengguna> signInManager,
        KonteksAplikasi context,
        ILayananAudit layananAudit)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _context = context;
        _layananAudit = layananAudit;
    }

    [HttpGet]
    public async Task<IActionResult> Profil()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction("Masuk", "Autentikasi");

        var roles = await _userManager.GetRolesAsync(user);
        var perusahaan = user.PerusahaanId.HasValue
            ? await _context.Perusahaan.IgnoreQueryFilters().FirstOrDefaultAsync(p => p.Id == user.PerusahaanId.Value)
            : null;

        var model = new ProfilViewModel
        {
            Id = user.Id,
            Email = user.Email ?? string.Empty,
            NamaLengkap = user.NamaLengkap,
            NomorTelepon = user.PhoneNumber,
            NamaPerusahaan = perusahaan?.NamaPerusahaan ?? "Platform Administrator (Global)",
            KodePerusahaan = perusahaan?.KodePerusahaan ?? "-",
            PeranUtama = roles.FirstOrDefault() ?? "Karyawan",
            CreatedAt = user.CreatedAt
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UbahProfil(ProfilViewModel model)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction("Masuk", "Autentikasi");

        if (string.IsNullOrWhiteSpace(model.NamaLengkap))
        {
            TempData["PesanError"] = "Nama lengkap tidak boleh kosong.";
            return RedirectToAction(nameof(Profil));
        }

        var namaLama = user.NamaLengkap;
        user.NamaLengkap = model.NamaLengkap.Trim();
        user.PhoneNumber = model.NomorTelepon?.Trim();
        user.UpdatedAt = DateTimeOffset.UtcNow;

        var hasil = await _userManager.UpdateAsync(user);
        if (hasil.Succeeded)
        {
            await _layananAudit.CatatAsync("UPDATE_PROFIL", "Pengguna", user.Id.ToString(), namaLama, user.NamaLengkap);
            TempData["PesanSukses"] = "Profil Anda berhasil diperbarui.";
        }
        else
        {
            TempData["PesanError"] = string.Join("; ", hasil.Errors.Select(e => e.Description));
        }

        return RedirectToAction(nameof(Profil));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UbahKataSandi(UbahKataSandiViewModel model)
    {
        if (!ModelState.IsValid)
        {
            TempData["PesanError"] = "Validasi kata sandi tidak valid. Pastikan konfirmasi cocok dan minimal 6 karakter.";
            return RedirectToAction(nameof(Profil));
        }

        var user = await _userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction("Masuk", "Autentikasi");

        var hasil = await _userManager.ChangePasswordAsync(user, model.KataSandiLama, model.KataSandiBaru);
        if (hasil.Succeeded)
        {
            await _layananAudit.CatatAsync("UBAH_PASSWORD", "Pengguna", user.Id.ToString(), null, "Kata sandi berhasil diubah.");
            await _signInManager.RefreshSignInAsync(user);
            TempData["PesanSukses"] = "Kata sandi Anda berhasil diubah.";
        }
        else
        {
            TempData["PesanError"] = string.Join("; ", hasil.Errors.Select(e => e.Description));
        }

        return RedirectToAction(nameof(Profil));
    }
}
