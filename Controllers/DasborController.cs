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
/// Controller utama untuk menyajikan halaman dashboard modular berbasis peran
/// (SuperAdmin, HRD, Manager, Finance, dan Karyawan) dengan isolasi data Multi-Tenant.
/// </summary>
[Authorize]
public class DasborController : Controller
{
    private readonly KonteksAplikasi _context;
    private readonly UserManager<Pengguna> _userManager;
    private readonly ILayananTenant _layananTenant;

    public DasborController(
        KonteksAplikasi context,
        UserManager<Pengguna> userManager,
        ILayananTenant layananTenant)
    {
        _context = context;
        _userManager = userManager;
        _layananTenant = layananTenant;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return RedirectToAction("Masuk", "Autentikasi");
        }

        var roles = await _userManager.GetRolesAsync(user);
        var peranUtama = roles.FirstOrDefault() ?? NamaPeran.Karyawan;

        var model = new DasborViewModel
        {
            PeranPengguna = peranUtama,
            NamaPengguna = user.NamaLengkap,
            IsSuperAdmin = roles.Contains(NamaPeran.SuperAdmin)
        };

        if (model.IsSuperAdmin)
        {
            // Metrik Platform SuperAdmin (seluruh tenant)
            model.TotalPerusahaanAktif = await _context.Perusahaan
                .IgnoreQueryFilters()
                .CountAsync(p => p.StatusAktif && !p.IsDeleted);

            model.TotalPenggunaSistem = await _context.Users
                .IgnoreQueryFilters()
                .CountAsync(u => u.StatusAktif && !u.IsDeleted);

            var hariIni = DateTimeOffset.UtcNow.Date;
            model.TotalAktivitasHariIni = await _context.AuditLog
                .IgnoreQueryFilters()
                .CountAsync(a => a.Waktu >= hariIni);

            model.LogAktivitasTerakhir = await _context.AuditLog
                .IgnoreQueryFilters()
                .OrderByDescending(a => a.Waktu)
                .Take(8)
                .ToListAsync();

            model.DaftarPerusahaanPlatform = await _context.Perusahaan
                .IgnoreQueryFilters()
                .Where(p => !p.IsDeleted)
                .OrderBy(p => p.KodePerusahaan)
                .ToListAsync();
        }
        else
        {
            // Metrik Tingkat Perusahaan (Terisolasi via Global Query Filter)
            var perusahaanAktif = await _layananTenant.DapatkanPerusahaanAktifAsync();
            model.NamaPerusahaan = perusahaanAktif?.NamaPerusahaan ?? "Perusahaan";
            model.KodePerusahaan = perusahaanAktif?.KodePerusahaan ?? "-";

            // Jumlah akun pengguna terdaftar pada tenant ini
            model.TotalKaryawanPerusahaan = await _context.Users
                .CountAsync(u => u.StatusAktif && !u.IsDeleted);

            // Log aktivitas terkait perusahaan ini
            model.LogAktivitasTerakhir = await _context.AuditLog
                .OrderByDescending(a => a.Waktu)
                .Take(6)
                .ToListAsync();

            model.DaftarPenggunaTenant = await _context.Users
                .Where(u => !u.IsDeleted)
                .OrderBy(u => u.NamaLengkap)
                .Take(5)
                .ToListAsync();
        }

        return View(model);
    }

    /// <summary>
    /// Endpoint khusus SuperAdmin untuk berpindah konteks view perusahaan atau melihat data spesifik tenant.
    /// </summary>
    [HttpPost]
    [Authorize(Roles = NamaPeran.SuperAdmin)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> GantiPerusahaanAktif(Guid? perusahaanId)
    {
        if (perusahaanId.HasValue)
        {
            var p = await _context.Perusahaan.IgnoreQueryFilters().FirstOrDefaultAsync(x => x.Id == perusahaanId.Value);
            if (p != null)
            {
                _layananTenant.SetTenant(p.Id, p.KodePerusahaan);
                TempData["PesanSukses"] = $"Konteks aktif dialihkan ke perusahaan: {p.NamaPerusahaan} ({p.KodePerusahaan})";
            }
        }
        else
        {
            TempData["PesanSukses"] = "Konteks dialihkan ke seluruh platform (Platform Global View).";
        }

        return RedirectToAction(nameof(Index));
    }
}
