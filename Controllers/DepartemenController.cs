using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SistemHRD.Common.Constants;
using SistemHRD.Data;
using SistemHRD.Models.Enums;
using SistemHRD.Services.Interfaces;
using SistemHRD.ViewModels;

namespace SistemHRD.Controllers;

[Authorize(Roles = $"{NamaPeran.SuperAdmin},{NamaPeran.HRD}")]
public class DepartemenController : Controller
{
    private readonly ILayananDepartemen _layananDepartemen;
    private readonly KonteksAplikasi _context;

    public DepartemenController(ILayananDepartemen layananDepartemen, KonteksAplikasi context)
    {
        _layananDepartemen = layananDepartemen;
        _context = context;
    }

    // GET: /Departemen
    public async Task<IActionResult> Index()
    {
        var daftarDepartemen = await _layananDepartemen.DapatkanSemuaAsync();
        return View(daftarDepartemen);
    }

    // GET: /Departemen/Tambah
    public async Task<IActionResult> Tambah()
    {
        var model = new DepartemenFormViewModel
        {
            OpsiKaryawan = await DapatkanOpsiKaryawanAsync()
        };
        return View(model);
    }

    // POST: /Departemen/Tambah
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Tambah(DepartemenFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            model.OpsiKaryawan = await DapatkanOpsiKaryawanAsync();
            return View(model);
        }

        var (sukses, pesanError) = await _layananDepartemen.SimpanAsync(model);
        if (!sukses)
        {
            ModelState.AddModelError(string.Empty, pesanError ?? "Gagal menambahkan departemen.");
            model.OpsiKaryawan = await DapatkanOpsiKaryawanAsync();
            return View(model);
        }

        TempData["PesanSukses"] = $"Departemen '{model.NamaDepartemen}' berhasil ditambahkan.";
        return RedirectToAction(nameof(Index));
    }

    // GET: /Departemen/Ubah/{id}
    public async Task<IActionResult> Ubah(Guid id)
    {
        var departemen = await _layananDepartemen.DapatkanBerdasarkanIdAsync(id);
        if (departemen == null)
        {
            TempData["PesanError"] = "Departemen tidak ditemukan.";
            return RedirectToAction(nameof(Index));
        }

        var model = new DepartemenFormViewModel
        {
            Id = departemen.Id,
            KodeDepartemen = departemen.KodeDepartemen,
            NamaDepartemen = departemen.NamaDepartemen,
            Deskripsi = departemen.Deskripsi,
            KepalaDepartemenId = departemen.KepalaDepartemenId,
            StatusAktif = departemen.StatusAktif,
            OpsiKaryawan = await DapatkanOpsiKaryawanAsync()
        };

        return View(model);
    }

    // POST: /Departemen/Ubah/{id}
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Ubah(Guid id, DepartemenFormViewModel model)
    {
        if (id != model.Id)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            model.OpsiKaryawan = await DapatkanOpsiKaryawanAsync();
            return View(model);
        }

        var (sukses, pesanError) = await _layananDepartemen.SimpanAsync(model);
        if (!sukses)
        {
            ModelState.AddModelError(string.Empty, pesanError ?? "Gagal memperbarui departemen.");
            model.OpsiKaryawan = await DapatkanOpsiKaryawanAsync();
            return View(model);
        }

        TempData["PesanSukses"] = $"Departemen '{model.NamaDepartemen}' berhasil diperbarui.";
        return RedirectToAction(nameof(Index));
    }

    // POST: /Departemen/Hapus/{id}
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Hapus(Guid id)
    {
        var (sukses, pesanError) = await _layananDepartemen.HapusAsync(id);
        if (!sukses)
        {
            TempData["PesanError"] = pesanError ?? "Gagal menghapus departemen.";
        }
        else
        {
            TempData["PesanSukses"] = "Departemen berhasil dihapus.";
        }

        return RedirectToAction(nameof(Index));
    }

    private async Task<List<SelectListItem>> DapatkanOpsiKaryawanAsync()
    {
        return await _context.Karyawan
            .Where(k => k.StatusKaryawan != StatusKaryawan.Nonaktif)
            .OrderBy(k => k.NamaLengkap)
            .Select(k => new SelectListItem
            {
                Value = k.Id.ToString(),
                Text = $"{k.NamaLengkap} ({k.NomorIndukKaryawan})"
            })
            .ToListAsync();
    }
}
