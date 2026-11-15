using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SistemHRD.Common.Constants;
using SistemHRD.Data;
using SistemHRD.Services.Interfaces;
using SistemHRD.ViewModels;

namespace SistemHRD.Controllers;

[Authorize(Roles = $"{NamaPeran.SuperAdmin},{NamaPeran.HRD}")]
public class JabatanController : Controller
{
    private readonly ILayananJabatan _layananJabatan;
    private readonly KonteksAplikasi _context;

    public JabatanController(ILayananJabatan layananJabatan, KonteksAplikasi context)
    {
        _layananJabatan = layananJabatan;
        _context = context;
    }

    // GET: /Jabatan
    public async Task<IActionResult> Index()
    {
        var daftarJabatan = await _layananJabatan.DapatkanSemuaAsync();
        return View(daftarJabatan);
    }

    // GET: /Jabatan/Tambah
    public async Task<IActionResult> Tambah()
    {
        var model = new JabatanFormViewModel
        {
            OpsiDepartemen = await DapatkanOpsiDepartemenAsync()
        };
        return View(model);
    }

    // POST: /Jabatan/Tambah
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Tambah(JabatanFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            model.OpsiDepartemen = await DapatkanOpsiDepartemenAsync();
            return View(model);
        }

        var (sukses, pesanError) = await _layananJabatan.SimpanAsync(model);
        if (!sukses)
        {
            ModelState.AddModelError(string.Empty, pesanError ?? "Gagal menambahkan jabatan.");
            model.OpsiDepartemen = await DapatkanOpsiDepartemenAsync();
            return View(model);
        }

        TempData["PesanSukses"] = $"Jabatan '{model.NamaJabatan}' berhasil ditambahkan.";
        return RedirectToAction(nameof(Index));
    }

    // GET: /Jabatan/Ubah/{id}
    public async Task<IActionResult> Ubah(Guid id)
    {
        var jabatan = await _layananJabatan.DapatkanBerdasarkanIdAsync(id);
        if (jabatan == null)
        {
            TempData["PesanError"] = "Jabatan tidak ditemukan.";
            return RedirectToAction(nameof(Index));
        }

        var model = new JabatanFormViewModel
        {
            Id = jabatan.Id,
            NamaJabatan = jabatan.NamaJabatan,
            DepartemenId = jabatan.DepartemenId,
            Deskripsi = jabatan.Deskripsi,
            LevelJabatan = jabatan.LevelJabatan,
            StatusAktif = jabatan.StatusAktif,
            OpsiDepartemen = await DapatkanOpsiDepartemenAsync()
        };

        return View(model);
    }

    // POST: /Jabatan/Ubah/{id}
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Ubah(Guid id, JabatanFormViewModel model)
    {
        if (id != model.Id)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            model.OpsiDepartemen = await DapatkanOpsiDepartemenAsync();
            return View(model);
        }

        var (sukses, pesanError) = await _layananJabatan.SimpanAsync(model);
        if (!sukses)
        {
            ModelState.AddModelError(string.Empty, pesanError ?? "Gagal memperbarui jabatan.");
            model.OpsiDepartemen = await DapatkanOpsiDepartemenAsync();
            return View(model);
        }

        TempData["PesanSukses"] = $"Jabatan '{model.NamaJabatan}' berhasil diperbarui.";
        return RedirectToAction(nameof(Index));
    }

    // POST: /Jabatan/Hapus/{id}
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Hapus(Guid id)
    {
        var (sukses, pesanError) = await _layananJabatan.HapusAsync(id);
        if (!sukses)
        {
            TempData["PesanError"] = pesanError ?? "Gagal menghapus jabatan.";
        }
        else
        {
            TempData["PesanSukses"] = "Jabatan berhasil dihapus.";
        }

        return RedirectToAction(nameof(Index));
    }

    // GET: /Jabatan/DapatkanBerdasarkanDepartemen?departemenId=...
    [HttpGet]
    public async Task<IActionResult> DapatkanBerdasarkanDepartemen(Guid departemenId)
    {
        var daftarJabatan = await _layananJabatan.DapatkanBerdasarkanDepartemenAsync(departemenId);
        var hasil = daftarJabatan.Select(j => new
        {
            id = j.Id,
            namaJabatan = j.NamaJabatan
        });
        return Json(hasil);
    }

    private async Task<List<SelectListItem>> DapatkanOpsiDepartemenAsync()
    {
        return await _context.Departemen
            .Where(d => d.StatusAktif)
            .OrderBy(d => d.NamaDepartemen)
            .Select(d => new SelectListItem
            {
                Value = d.Id.ToString(),
                Text = d.NamaDepartemen
            })
            .ToListAsync();
    }
}
