using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
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
public class KaryawanController : Controller
{
    private readonly ILayananKaryawan _layananKaryawan;
    private readonly KonteksAplikasi _context;
    private readonly IWebHostEnvironment _webHostEnvironment;

    public KaryawanController(
        ILayananKaryawan layananKaryawan,
        KonteksAplikasi context,
        IWebHostEnvironment webHostEnvironment)
    {
        _layananKaryawan = layananKaryawan;
        _context = context;
        _webHostEnvironment = webHostEnvironment;
    }

    // GET: /Karyawan
    public async Task<IActionResult> Index(
        string? cari,
        Guid? departemenId,
        Guid? jabatanId,
        StatusKaryawan? status,
        int halaman = 1,
        int ukuranHalaman = 10)
    {
        var model = await _layananKaryawan.DapatkanDaftarAsync(cari, departemenId, jabatanId, status, halaman, ukuranHalaman);
        return View(model);
    }

    // GET: /Karyawan/Tambah
    public async Task<IActionResult> Tambah()
    {
        var model = new KaryawanFormViewModel
        {
            OpsiDepartemen = await DapatkanOpsiDepartemenAsync(),
            OpsiJabatan = new List<SelectListItem>()
        };

        return View(model);
    }

    // POST: /Karyawan/Tambah
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Tambah(KaryawanFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            model.OpsiDepartemen = await DapatkanOpsiDepartemenAsync();
            model.OpsiJabatan = await DapatkanOpsiJabatanAsync(model.DepartemenId);
            return View(model);
        }

        var (sukses, pesanError) = await _layananKaryawan.SimpanKaryawanAsync(model, model.BerkasFoto);
        if (!sukses)
        {
            ModelState.AddModelError(string.Empty, pesanError ?? "Gagal menambahkan karyawan.");
            model.OpsiDepartemen = await DapatkanOpsiDepartemenAsync();
            model.OpsiJabatan = await DapatkanOpsiJabatanAsync(model.DepartemenId);
            return View(model);
        }

        TempData["PesanSukses"] = $"Karyawan '{model.NamaLengkap}' berhasil ditambahkan.";
        return RedirectToAction(nameof(Index));
    }

    // GET: /Karyawan/Ubah/{id}
    public async Task<IActionResult> Ubah(Guid id)
    {
        var karyawan = await _layananKaryawan.DapatkanBerdasarkanIdAsync(id);
        if (karyawan == null)
        {
            TempData["PesanError"] = "Karyawan tidak ditemukan.";
            return RedirectToAction(nameof(Index));
        }

        var model = new KaryawanFormViewModel
        {
            Id = karyawan.Id,
            NomorIndukKaryawan = karyawan.NomorIndukKaryawan,
            NamaLengkap = karyawan.NamaLengkap,
            TempatLahir = karyawan.TempatLahir,
            TanggalLahir = karyawan.TanggalLahir,
            JenisKelamin = karyawan.JenisKelamin,
            Alamat = karyawan.Alamat,
            NomorTelepon = karyawan.NomorTelepon,
            Email = karyawan.Email,
            TanggalMasuk = karyawan.TanggalMasuk,
            DepartemenId = karyawan.DepartemenId,
            JabatanId = karyawan.JabatanId,
            StatusKaryawan = karyawan.StatusKaryawan,
            FotoProfilSaatIni = karyawan.FotoProfil,
            OpsiDepartemen = await DapatkanOpsiDepartemenAsync(),
            OpsiJabatan = await DapatkanOpsiJabatanAsync(karyawan.DepartemenId)
        };

        return View(model);
    }

    // POST: /Karyawan/Ubah/{id}
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Ubah(Guid id, KaryawanFormViewModel model)
    {
        if (id != model.Id)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            model.OpsiDepartemen = await DapatkanOpsiDepartemenAsync();
            model.OpsiJabatan = await DapatkanOpsiJabatanAsync(model.DepartemenId);
            return View(model);
        }

        var (sukses, pesanError) = await _layananKaryawan.SimpanKaryawanAsync(model, model.BerkasFoto);
        if (!sukses)
        {
            ModelState.AddModelError(string.Empty, pesanError ?? "Gagal memperbarui data karyawan.");
            model.OpsiDepartemen = await DapatkanOpsiDepartemenAsync();
            model.OpsiJabatan = await DapatkanOpsiJabatanAsync(model.DepartemenId);
            return View(model);
        }

        TempData["PesanSukses"] = $"Data karyawan '{model.NamaLengkap}' berhasil diperbarui.";
        return RedirectToAction(nameof(Detail), new { id = model.Id });
    }

    // GET: /Karyawan/Detail/{id}
    public async Task<IActionResult> Detail(Guid id, string tab = "pribadi")
    {
        var model = await _layananKaryawan.DapatkanDetailAsync(id);
        if (model == null)
        {
            TempData["PesanError"] = "Karyawan tidak ditemukan.";
            return RedirectToAction(nameof(Index));
        }

        ViewBag.ActiveTab = tab;
        return View(model);
    }

    // POST: /Karyawan/UbahStatus/{id}
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UbahStatus(Guid id)
    {
        var (sukses, pesanError) = await _layananKaryawan.UbahStatusAktifAsync(id);
        if (!sukses)
        {
            TempData["PesanError"] = pesanError ?? "Gagal memperbarui status karyawan.";
        }
        else
        {
            TempData["PesanSukses"] = "Status karyawan berhasil diperbarui.";
        }

        return RedirectToAction(nameof(Detail), new { id });
    }

    // POST: /Karyawan/TambahKontrak
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> TambahKontrak([Bind(Prefix = "")] KontrakKerjaFormViewModel formKontrak)
    {
        if (!ModelState.IsValid)
        {
            TempData["PesanError"] = "Form kontrak tidak valid. Pastikan semua kolom wajib diisi.";
            return RedirectToAction(nameof(Detail), new { id = formKontrak.KaryawanId, tab = "kontrak" });
        }

        var (sukses, pesanError) = await _layananKaryawan.TambahKontrakAsync(formKontrak);
        if (!sukses)
        {
            TempData["PesanError"] = pesanError ?? "Gagal menambahkan kontrak.";
        }
        else
        {
            TempData["PesanSukses"] = "Kontrak kerja berhasil ditambahkan.";
        }

        return RedirectToAction(nameof(Detail), new { id = formKontrak.KaryawanId, tab = "kontrak" });
    }

    // POST: /Karyawan/TambahKontakDarurat
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> TambahKontakDarurat([Bind(Prefix = "")] KontakDaruratFormViewModel formKontakDarurat)
    {
        if (!ModelState.IsValid)
        {
            TempData["PesanError"] = "Form kontak darurat tidak valid.";
            return RedirectToAction(nameof(Detail), new { id = formKontakDarurat.KaryawanId, tab = "kontak-darurat" });
        }

        var (sukses, pesanError) = await _layananKaryawan.TambahKontakDaruratAsync(formKontakDarurat);
        if (!sukses)
        {
            TempData["PesanError"] = pesanError ?? "Gagal menambahkan kontak darurat.";
        }
        else
        {
            TempData["PesanSukses"] = "Kontak darurat berhasil ditambahkan.";
        }

        return RedirectToAction(nameof(Detail), new { id = formKontakDarurat.KaryawanId, tab = "kontak-darurat" });
    }

    // POST: /Karyawan/TambahRiwayatPekerjaan
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> TambahRiwayatPekerjaan([Bind(Prefix = "")] RiwayatPekerjaanFormViewModel formRiwayatPekerjaan)
    {
        if (!ModelState.IsValid)
        {
            TempData["PesanError"] = "Form riwayat pekerjaan tidak valid.";
            return RedirectToAction(nameof(Detail), new { id = formRiwayatPekerjaan.KaryawanId, tab = "riwayat" });
        }

        var (sukses, pesanError) = await _layananKaryawan.TambahRiwayatPekerjaanAsync(formRiwayatPekerjaan);
        if (!sukses)
        {
            TempData["PesanError"] = pesanError ?? "Gagal menambahkan riwayat pekerjaan.";
        }
        else
        {
            TempData["PesanSukses"] = "Riwayat pekerjaan berhasil ditambahkan.";
        }

        return RedirectToAction(nameof(Detail), new { id = formRiwayatPekerjaan.KaryawanId, tab = "riwayat" });
    }

    // POST: /Karyawan/UploadDokumen
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UploadDokumen([Bind(Prefix = "")] DokumenUploadViewModel formDokumen)
    {
        if (!ModelState.IsValid)
        {
            TempData["PesanError"] = "Form unggah dokumen tidak valid.";
            return RedirectToAction(nameof(Detail), new { id = formDokumen.KaryawanId, tab = "dokumen" });
        }

        var (sukses, pesanError) = await _layananKaryawan.UploadDokumenAsync(formDokumen);
        if (!sukses)
        {
            TempData["PesanError"] = pesanError ?? "Gagal mengunggah berkas dokumen.";
        }
        else
        {
            TempData["PesanSukses"] = "Dokumen berhasil diunggah.";
        }

        return RedirectToAction(nameof(Detail), new { id = formDokumen.KaryawanId, tab = "dokumen" });
    }

    // GET: /Karyawan/UnduhDokumen/{id}
    public async Task<IActionResult> UnduhDokumen(Guid id)
    {
        var dokumen = await _layananKaryawan.DapatkanDokumenAsync(id);
        if (dokumen == null)
        {
            return NotFound("Dokumen tidak ditemukan atau Anda tidak memiliki akses.");
        }

        var pathFisik = Path.Combine(_webHostEnvironment.WebRootPath, dokumen.PathFile);
        if (!System.IO.File.Exists(pathFisik))
        {
            return NotFound("Berkas fisik dokumen tidak ditemukan di server.");
        }

        var contentType = dokumen.TipeFile ?? "application/octet-stream";
        return PhysicalFile(pathFisik, contentType, dokumen.NamaFile);
    }

    // POST: /Karyawan/HapusDokumen/{id}
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> HapusDokumen(Guid id, Guid karyawanId)
    {
        var (sukses, pesanError) = await _layananKaryawan.HapusDokumenAsync(id);
        if (!sukses)
        {
            TempData["PesanError"] = pesanError ?? "Gagal menghapus dokumen.";
        }
        else
        {
            TempData["PesanSukses"] = "Dokumen berhasil dihapus.";
        }

        return RedirectToAction(nameof(Detail), new { id = karyawanId, tab = "dokumen" });
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

    private async Task<List<SelectListItem>> DapatkanOpsiJabatanAsync(Guid? departemenId)
    {
        var query = _context.Jabatan.Where(j => j.StatusAktif);
        if (departemenId.HasValue && departemenId.Value != Guid.Empty)
        {
            query = query.Where(j => j.DepartemenId == departemenId.Value);
        }

        return await query
            .OrderBy(j => j.NamaJabatan)
            .Select(j => new SelectListItem
            {
                Value = j.Id.ToString(),
                Text = j.NamaJabatan
            })
            .ToListAsync();
    }
}
