using System.Security.Claims;
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

[Authorize]
public class AsetController : Controller
{
    private readonly ILayananManajemenAset _layananAset;
    private readonly ILayananDepartemen _layananDepartemen;
    private readonly KonteksAplikasi _context;

    public AsetController(
        ILayananManajemenAset layananAset,
        ILayananDepartemen layananDepartemen,
        KonteksAplikasi context)
    {
        _layananAset = layananAset;
        _layananDepartemen = layananDepartemen;
        _context = context;
    }

    #region Dashboard Aset
    // GET: /Aset
    [Authorize(Roles = $"{NamaPeran.SuperAdmin},{NamaPeran.HRD},{NamaPeran.Manager}")]
    public async Task<IActionResult> Index()
    {
        var model = await _layananAset.DapatkanDashboardAsync();
        return View(model);
    }
    #endregion

    #region Inventaris Aset
    // GET: /Aset/Daftar
    [Authorize(Roles = $"{NamaPeran.SuperAdmin},{NamaPeran.HRD},{NamaPeran.Manager}")]
    public async Task<IActionResult> Daftar(KategoriAset? kategori, StatusAset? status, KondisiAset? kondisi, string? kataKunci, int halaman = 1)
    {
        ViewBag.KategoriTerpilih = kategori;
        ViewBag.StatusTerpilih = status;
        ViewBag.KondisiTerpilih = kondisi;
        ViewBag.KataKunci = kataKunci;

        var pagedModel = await _layananAset.DapatkanDaftarAsetAsync(kategori, status, kondisi, kataKunci, halaman, 10);
        return View(pagedModel);
    }

    // GET: /Aset/TambahAset
    [Authorize(Roles = $"{NamaPeran.SuperAdmin},{NamaPeran.HRD}")]
    public IActionResult TambahAset()
    {
        var model = new FormAsetViewModel
        {
            NomorAset = $"AST-{DateTime.Now:yyyyMMdd}-{new Random().Next(100, 999)}",
            TanggalPembelian = DateTime.Today,
            LokasiPenyimpanan = "Gudang IT / Operasional"
        };
        return View(model);
    }

    // POST: /Aset/TambahAset
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = $"{NamaPeran.SuperAdmin},{NamaPeran.HRD}")]
    public async Task<IActionResult> TambahAset(FormAsetViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var (sukses, pesan, id) = await _layananAset.SimpanAsetAsync(model);

        if (!sukses)
        {
            ModelState.AddModelError(string.Empty, pesan);
            return View(model);
        }

        TempData["PesanSukses"] = pesan;
        return RedirectToAction(nameof(DetailAset), new { id = id!.Value });
    }

    // GET: /Aset/UbahAset/{id}
    [Authorize(Roles = $"{NamaPeran.SuperAdmin},{NamaPeran.HRD}")]
    public async Task<IActionResult> UbahAset(Guid id)
    {
        var detail = await _layananAset.DapatkanDetailAsetAsync(id);
        if (detail == null)
        {
            TempData["PesanError"] = "Aset tidak ditemukan.";
            return RedirectToAction(nameof(Daftar));
        }

        var model = new FormAsetViewModel
        {
            Id = detail.Id,
            NomorAset = detail.NomorAset,
            NamaAset = detail.NamaAset,
            Kategori = detail.Kategori,
            MerkModel = detail.MerkModel,
            NomorSeri = detail.NomorSeri,
            TanggalPembelian = detail.TanggalPembelian,
            NilaiPerolehan = detail.NilaiPerolehan,
            Kondisi = detail.Kondisi,
            LokasiPenyimpanan = detail.LokasiPenyimpanan,
            DeskripsiSpesifikasi = detail.DeskripsiSpesifikasi
        };

        return View(model);
    }

    // POST: /Aset/UbahAset/{id}
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = $"{NamaPeran.SuperAdmin},{NamaPeran.HRD}")]
    public async Task<IActionResult> UbahAset(Guid id, FormAsetViewModel model)
    {
        model.Id = id;
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var (sukses, pesan, _) = await _layananAset.SimpanAsetAsync(model);

        if (!sukses)
        {
            ModelState.AddModelError(string.Empty, pesan);
            return View(model);
        }

        TempData["PesanSukses"] = pesan;
        return RedirectToAction(nameof(DetailAset), new { id });
    }

    // GET: /Aset/DetailAset/{id}
    [Authorize(Roles = $"{NamaPeran.SuperAdmin},{NamaPeran.HRD},{NamaPeran.Manager}")]
    public async Task<IActionResult> DetailAset(Guid id)
    {
        var detail = await _layananAset.DapatkanDetailAsetAsync(id);
        if (detail == null)
        {
            TempData["PesanError"] = "Aset tidak ditemukan.";
            return RedirectToAction(nameof(Daftar));
        }

        await SiapkanViewBagKaryawanAsync();
        return View(detail);
    }

    // POST: /Aset/HapusAset/{id}
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = $"{NamaPeran.SuperAdmin},{NamaPeran.HRD}")]
    public async Task<IActionResult> HapusAset(Guid id)
    {
        var (sukses, pesan) = await _layananAset.HapusAsetAsync(id);

        if (sukses)
        {
            TempData["PesanSukses"] = pesan;
        }
        else
        {
            TempData["PesanError"] = pesan;
        }

        return RedirectToAction(nameof(Daftar));
    }
    #endregion

    #region Penyerahan & Pengembalian Aset
    // POST: /Aset/SerahkanAset
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = $"{NamaPeran.SuperAdmin},{NamaPeran.HRD}")]
    public async Task<IActionResult> SerahkanAset(FormPenyerahanAsetViewModel model)
    {
        if (!ModelState.IsValid)
        {
            TempData["PesanError"] = "Data penyerahan aset tidak lengkap.";
            return RedirectToAction(nameof(DetailAset), new { id = model.AsetPerusahaanId });
        }

        var (userId, userName) = DapatkanPenggunaSaatIni();
        var (sukses, pesan) = await _layananAset.SerahkanAsetAsync(model, userId, userName);

        if (sukses)
        {
            TempData["PesanSukses"] = pesan;
        }
        else
        {
            TempData["PesanError"] = pesan;
        }

        return RedirectToAction(nameof(DetailAset), new { id = model.AsetPerusahaanId });
    }

    // POST: /Aset/KembalikanAset
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = $"{NamaPeran.SuperAdmin},{NamaPeran.HRD}")]
    public async Task<IActionResult> KembalikanAset(FormPengembalianAsetViewModel model)
    {
        if (!ModelState.IsValid)
        {
            TempData["PesanError"] = "Data pengembalian aset tidak lengkap.";
            return RedirectToAction(nameof(DetailAset), new { id = model.AsetPerusahaanId });
        }

        var (userId, userName) = DapatkanPenggunaSaatIni();
        var (sukses, pesan) = await _layananAset.KembalikanAsetAsync(model, userId, userName);

        if (sukses)
        {
            TempData["PesanSukses"] = pesan;
        }
        else
        {
            TempData["PesanError"] = pesan;
        }

        return RedirectToAction(nameof(DetailAset), new { id = model.AsetPerusahaanId });
    }
    #endregion

    #region ESS Karyawan
    // GET: /Aset/RiwayatSaya
    public async Task<IActionResult> RiwayatSaya()
    {
        var karyawanId = await DapatkanKaryawanIdPenggunaAktifAsync();
        if (!karyawanId.HasValue)
        {
            TempData["PesanError"] = "Profil karyawan Anda belum terhubung. Hubungi HRD.";
            return RedirectToAction("Index", "Dashboard");
        }

        var daftarAset = await _layananAset.DapatkanAsetKaryawanAktifAsync(karyawanId.Value);
        return View(daftarAset);
    }
    #endregion

    #region Helpers
    private (Guid userId, string userName) DapatkanPenggunaSaatIni()
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var userId = Guid.TryParse(userIdStr, out var uGuid) ? uGuid : Guid.Empty;
        var userName = User.Identity?.Name ?? "Pengguna";
        return (userId, userName);
    }

    private async Task<Guid?> DapatkanKaryawanIdPenggunaAktifAsync()
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (Guid.TryParse(userIdStr, out var userId))
        {
            var kry = await _context.Karyawan.FirstOrDefaultAsync(k => !k.IsDeleted && k.PenggunaId == userId);
            if (kry != null) return kry.Id;
        }

        var email = User.FindFirstValue(ClaimTypes.Email);
        if (!string.IsNullOrEmpty(email))
        {
            var kryByEmail = await _context.Karyawan.FirstOrDefaultAsync(k => !k.IsDeleted && k.Email.ToLower() == email.ToLower());
            if (kryByEmail != null) return kryByEmail.Id;
        }

        return null;
    }

    private async Task SiapkanViewBagKaryawanAsync()
    {
        var daftarKaryawan = await _context.Karyawan
            .Where(k => !k.IsDeleted && (k.StatusKaryawan == StatusKaryawan.Tetap || k.StatusKaryawan == StatusKaryawan.Kontrak || k.StatusKaryawan == StatusKaryawan.Percobaan))
            .OrderBy(k => k.NamaLengkap)
            .Select(k => new { k.Id, Nama = $"{k.NomorIndukKaryawan} - {k.NamaLengkap} ({(k.Departemen != null ? k.Departemen.NamaDepartemen : "-")})" })
            .ToListAsync();
        ViewBag.DaftarKaryawan = new SelectList(daftarKaryawan, "Id", "Nama");
    }
    #endregion
}
