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
public class KehadiranController : Controller
{
    private readonly ILayananKehadiran _layananKehadiran;
    private readonly ILayananDepartemen _layananDepartemen;
    private readonly KonteksAplikasi _context;

    public KehadiranController(
        ILayananKehadiran layananKehadiran,
        ILayananDepartemen layananDepartemen,
        KonteksAplikasi context)
    {
        _layananKehadiran = layananKehadiran;
        _layananDepartemen = layananDepartemen;
        _context = context;
    }

    // GET: /Kehadiran
    public async Task<IActionResult> Index(FilterKehadiranViewModel? filter)
    {
        filter ??= new FilterKehadiranViewModel();
        var isKaryawan = User.IsInRole(NamaPeran.Karyawan);
        if (isKaryawan)
        {
            var karyawanId = await DapatkanKaryawanIdPenggunaAktifAsync();
            if (karyawanId.HasValue)
            {
                filter.KaryawanId = karyawanId.Value;
            }
        }

        var model = await _layananKehadiran.DapatkanDaftarKehadiranAsync(filter);
        await SiapkanViewBagDepartemenAsync(filter.DepartemenId);

        return View(model);
    }

    // GET: /Kehadiran/Presensi (Layar Check-In / Check-Out Interaktif ESS)
    public async Task<IActionResult> Presensi()
    {
        var karyawanId = await DapatkanKaryawanIdPenggunaAktifAsync();
        if (!karyawanId.HasValue)
        {
            TempData["PesanError"] = "Akun Anda belum ditautkan ke profil data karyawan. Hubungi HRD untuk menautkan akun pengguna Anda.";
            return RedirectToAction(nameof(Index));
        }

        var model = await _layananKehadiran.DapatkanKehadiranHariIniAsync(karyawanId.Value, DateTime.Today);
        return View(model);
    }

    // POST: /Kehadiran/CheckIn
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CheckIn(CheckInRequestViewModel model)
    {
        var karyawanId = await DapatkanKaryawanIdPenggunaAktifAsync();
        if (!karyawanId.HasValue)
        {
            TempData["PesanError"] = "Profil karyawan tidak ditemukan.";
            return RedirectToAction(nameof(Presensi));
        }

        var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
        var (sukses, pesan, _) = await _layananKehadiran.CheckInAsync(karyawanId.Value, model, ip);

        if (sukses)
        {
            TempData["PesanSukses"] = pesan;
        }
        else
        {
            TempData["PesanError"] = pesan;
        }

        return RedirectToAction(nameof(Presensi));
    }

    // POST: /Kehadiran/CheckOut
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CheckOut(CheckOutRequestViewModel model)
    {
        var karyawanId = await DapatkanKaryawanIdPenggunaAktifAsync();
        if (!karyawanId.HasValue)
        {
            TempData["PesanError"] = "Profil karyawan tidak ditemukan.";
            return RedirectToAction(nameof(Presensi));
        }

        var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
        var (sukses, pesan, _) = await _layananKehadiran.CheckOutAsync(karyawanId.Value, model, ip);

        if (sukses)
        {
            TempData["PesanSukses"] = pesan;
        }
        else
        {
            TempData["PesanError"] = pesan;
        }

        return RedirectToAction(nameof(Presensi));
    }

    // GET: /Kehadiran/Rekap (Rekapitulasi Bulanan)
    [Authorize(Roles = $"{NamaPeran.SuperAdmin},{NamaPeran.HRD},{NamaPeran.Manager},{NamaPeran.Finance}")]
    public async Task<IActionResult> Rekap(int? bulan, int? tahun, Guid? departemenId, Guid? karyawanId)
    {
        var bln = bulan ?? DateTime.Today.Month;
        var thn = tahun ?? DateTime.Today.Year;

        var model = await _layananKehadiran.DapatkanRekapitulasiAsync(bln, thn, departemenId, karyawanId);
        await SiapkanViewBagDepartemenAsync(departemenId);

        return View(model);
    }

    // GET: /Kehadiran/Shift (Master Shift Kerja)
    [Authorize(Roles = $"{NamaPeran.SuperAdmin},{NamaPeran.HRD}")]
    public async Task<IActionResult> Shift()
    {
        var shifts = await _layananKehadiran.DapatkanDaftarShiftAsync();
        return View(shifts);
    }

    // POST: /Kehadiran/SimpanShift
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = $"{NamaPeran.SuperAdmin},{NamaPeran.HRD}")]
    public async Task<IActionResult> SimpanShift(ShiftKerjaViewModel model)
    {
        if (!ModelState.IsValid)
        {
            TempData["PesanError"] = "Form shift tidak valid. Periksa kembali input Anda.";
            return RedirectToAction(nameof(Shift));
        }

        var (sukses, pesan) = await _layananKehadiran.SimpanShiftAsync(model);
        if (sukses)
        {
            TempData["PesanSukses"] = pesan;
        }
        else
        {
            TempData["PesanError"] = pesan;
        }

        return RedirectToAction(nameof(Shift));
    }

    // POST: /Kehadiran/HapusShift/{id}
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = $"{NamaPeran.SuperAdmin},{NamaPeran.HRD}")]
    public async Task<IActionResult> HapusShift(Guid id)
    {
        var (sukses, pesan) = await _layananKehadiran.HapusShiftAsync(id);
        if (sukses)
        {
            TempData["PesanSukses"] = pesan;
        }
        else
        {
            TempData["PesanError"] = pesan;
        }

        return RedirectToAction(nameof(Shift));
    }

    // GET: /Kehadiran/Lembur
    public async Task<IActionResult> Lembur(FilterLemburViewModel? filter)
    {
        filter ??= new FilterLemburViewModel();
        var isKaryawan = User.IsInRole(NamaPeran.Karyawan);
        Guid? karyawanId = null;

        if (isKaryawan)
        {
            karyawanId = await DapatkanKaryawanIdPenggunaAktifAsync();
            if (karyawanId.HasValue)
            {
                filter.KaryawanId = karyawanId.Value;
            }
        }

        var model = await _layananKehadiran.DapatkanDaftarLemburAsync(filter, karyawanId);
        return View(model);
    }

    // POST: /Kehadiran/AjukanLembur
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AjukanLembur(FormPengajuanLemburViewModel model)
    {
        var karyawanId = await DapatkanKaryawanIdPenggunaAktifAsync();
        if (!karyawanId.HasValue)
        {
            TempData["PesanError"] = "Profil karyawan tidak ditemukan.";
            return RedirectToAction(nameof(Lembur));
        }

        if (!ModelState.IsValid)
        {
            TempData["PesanError"] = "Data pengajuan lembur tidak valid.";
            return RedirectToAction(nameof(Lembur));
        }

        var (sukses, pesan) = await _layananKehadiran.AjukanLemburAsync(karyawanId.Value, model);
        if (sukses)
        {
            TempData["PesanSukses"] = pesan;
        }
        else
        {
            TempData["PesanError"] = pesan;
        }

        return RedirectToAction(nameof(Lembur));
    }

    // POST: /Kehadiran/ProsesPersetujuanLembur
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = $"{NamaPeran.SuperAdmin},{NamaPeran.HRD},{NamaPeran.Manager}")]
    public async Task<IActionResult> ProsesPersetujuanLembur(PersetujuanLemburViewModel model)
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
        {
            TempData["PesanError"] = "Sesi autentikasi tidak valid.";
            return RedirectToAction(nameof(Lembur));
        }

        var (sukses, pesan) = await _layananKehadiran.ProsesPersetujuanLemburAsync(model.PengajuanLemburId, model.Disetujui, model.Catatan, userId);
        if (sukses)
        {
            TempData["PesanSukses"] = pesan;
        }
        else
        {
            TempData["PesanError"] = pesan;
        }

        return RedirectToAction(nameof(Lembur));
    }

    private async Task<Guid?> DapatkanKaryawanIdPenggunaAktifAsync()
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!string.IsNullOrEmpty(userIdStr) && Guid.TryParse(userIdStr, out var userId))
        {
            var kry = await _context.Karyawan.FirstOrDefaultAsync(k => k.PenggunaId == userId);
            if (kry != null) return kry.Id;
        }

        var email = User.FindFirstValue(ClaimTypes.Email);
        if (!string.IsNullOrEmpty(email))
        {
            var kryByEmail = await _context.Karyawan.FirstOrDefaultAsync(k => k.Email.ToLower() == email.ToLower());
            if (kryByEmail != null) return kryByEmail.Id;
        }

        return null;
    }

    private async Task SiapkanViewBagDepartemenAsync(Guid? departemenIdTerpilih = null)
    {
        var daftarDept = await _layananDepartemen.DapatkanSemuaAsync();
        ViewBag.DaftarDepartemen = new SelectList(daftarDept, "Id", "NamaDepartemen", departemenIdTerpilih);
    }
}
