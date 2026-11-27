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
public class PelatihanController : Controller
{
    private readonly ILayananPelatihan _layananPelatihan;
    private readonly ILayananDepartemen _layananDepartemen;
    private readonly KonteksAplikasi _context;

    public PelatihanController(
        ILayananPelatihan layananPelatihan,
        ILayananDepartemen layananDepartemen,
        KonteksAplikasi context)
    {
        _layananPelatihan = layananPelatihan;
        _layananDepartemen = layananDepartemen;
        _context = context;
    }

    #region Dashboard Pelatihan
    // GET: /Pelatihan
    [Authorize(Roles = $"{NamaPeran.SuperAdmin},{NamaPeran.HRD},{NamaPeran.Manager}")]
    public async Task<IActionResult> Index()
    {
        var model = await _layananPelatihan.DapatkanDashboardAsync();
        return View(model);
    }
    #endregion

    #region Program Pelatihan
    // GET: /Pelatihan/Program
    [Authorize(Roles = $"{NamaPeran.SuperAdmin},{NamaPeran.HRD},{NamaPeran.Manager}")]
    public async Task<IActionResult> Program(MetodePelatihan? metode, StatusPelatihan? status, string? kataKunci)
    {
        ViewBag.MetodeTerpilih = metode;
        ViewBag.StatusTerpilih = status;
        ViewBag.KataKunci = kataKunci;

        var daftar = await _layananPelatihan.DapatkanSemuaProgramAsync(status);

        if (metode.HasValue)
        {
            daftar = daftar.Where(p => p.Metode == metode.Value).ToList();
        }

        if (!string.IsNullOrWhiteSpace(kataKunci))
        {
            var kWord = kataKunci.Trim().ToLower();
            daftar = daftar.Where(p => p.KodeProgram.ToLower().Contains(kWord) ||
                                       p.NamaProgram.ToLower().Contains(kWord) ||
                                       p.Penyelenggara.ToLower().Contains(kWord)).ToList();
        }

        return View(daftar);
    }

    // GET: /Pelatihan/TambahProgram
    [Authorize(Roles = $"{NamaPeran.SuperAdmin},{NamaPeran.HRD}")]
    public async Task<IActionResult> TambahProgram()
    {
        await SiapkanDropdownDepartemenAsync();
        var model = new FormProgramPelatihanViewModel
        {
            KodeProgram = $"TRN-{DateTime.Now:yyyyMMdd}-{new Random().Next(100, 999)}",
            TanggalMulai = DateTime.Today.AddDays(7),
            TanggalSelesai = DateTime.Today.AddDays(7)
        };
        return View(model);
    }

    // POST: /Pelatihan/TambahProgram
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = $"{NamaPeran.SuperAdmin},{NamaPeran.HRD}")]
    public async Task<IActionResult> TambahProgram(FormProgramPelatihanViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await SiapkanDropdownDepartemenAsync();
            return View(model);
        }

        var (sukses, pesan, id) = await _layananPelatihan.SimpanProgramAsync(model);

        if (!sukses)
        {
            await SiapkanDropdownDepartemenAsync();
            ModelState.AddModelError(string.Empty, pesan);
            return View(model);
        }

        TempData["PesanSukses"] = pesan;
        return RedirectToAction(nameof(DetailProgram), new { id = id!.Value });
    }

    // GET: /Pelatihan/UbahProgram/{id}
    [Authorize(Roles = $"{NamaPeran.SuperAdmin},{NamaPeran.HRD}")]
    public async Task<IActionResult> UbahProgram(Guid id)
    {
        var detail = await _layananPelatihan.DapatkanDetailProgramAsync(id);
        if (detail == null)
        {
            TempData["PesanError"] = "Program pelatihan tidak ditemukan.";
            return RedirectToAction(nameof(Program));
        }

        var programDb = await _layananPelatihan.DapatkanProgramByIdAsync(id);
        await SiapkanDropdownDepartemenAsync(programDb?.DepartemenId);

        var model = new FormProgramPelatihanViewModel
        {
            Id = detail.Id,
            KodeProgram = detail.KodeProgram,
            NamaProgram = detail.NamaProgram,
            Deskripsi = detail.Deskripsi,
            Penyelenggara = detail.Penyelenggara,
            Instruktur = detail.Instruktur,
            Metode = detail.Metode,
            LokasiAtauTautan = detail.LokasiAtauTautan,
            TanggalMulai = detail.TanggalMulai,
            TanggalSelesai = detail.TanggalSelesai,
            TotalJam = detail.TotalJam,
            BiayaPerPeserta = detail.BiayaPerPeserta,
            MaksimalPeserta = detail.MaksimalPeserta,
            Status = detail.Status,
            DepartemenId = programDb?.DepartemenId
        };

        return View(model);
    }

    // POST: /Pelatihan/UbahProgram/{id}
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = $"{NamaPeran.SuperAdmin},{NamaPeran.HRD}")]
    public async Task<IActionResult> UbahProgram(Guid id, FormProgramPelatihanViewModel model)
    {
        model.Id = id;
        if (!ModelState.IsValid)
        {
            await SiapkanDropdownDepartemenAsync(model.DepartemenId);
            return View(model);
        }

        var (sukses, pesan, _) = await _layananPelatihan.SimpanProgramAsync(model);

        if (!sukses)
        {
            await SiapkanDropdownDepartemenAsync(model.DepartemenId);
            ModelState.AddModelError(string.Empty, pesan);
            return View(model);
        }

        TempData["PesanSukses"] = pesan;
        return RedirectToAction(nameof(DetailProgram), new { id });
    }

    // GET: /Pelatihan/DetailProgram/{id}
    [Authorize(Roles = $"{NamaPeran.SuperAdmin},{NamaPeran.HRD},{NamaPeran.Manager}")]
    public async Task<IActionResult> DetailProgram(Guid id)
    {
        var detail = await _layananPelatihan.DapatkanDetailProgramAsync(id);
        if (detail == null)
        {
            TempData["PesanError"] = "Program pelatihan tidak ditemukan.";
            return RedirectToAction(nameof(Program));
        }

        await SiapkanViewBagKaryawanDanDepartemenAsync();
        return View(detail);
    }

    // POST: /Pelatihan/HapusProgram/{id}
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = $"{NamaPeran.SuperAdmin},{NamaPeran.HRD}")]
    public async Task<IActionResult> HapusProgram(Guid id)
    {
        var (sukses, pesan) = await _layananPelatihan.HapusProgramAsync(id);

        if (sukses)
        {
            TempData["PesanSukses"] = pesan;
        }
        else
        {
            TempData["PesanError"] = pesan;
        }

        return RedirectToAction(nameof(Program));
    }
    #endregion

    #region Peserta Pelatihan
    // POST: /Pelatihan/DaftarPeserta
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = $"{NamaPeran.SuperAdmin},{NamaPeran.HRD}")]
    public async Task<IActionResult> DaftarPeserta(FormDaftarPesertaViewModel model)
    {
        if (!ModelState.IsValid)
        {
            TempData["PesanError"] = "Data pendaftaran tidak valid.";
            return RedirectToAction(nameof(DetailProgram), new { id = model.ProgramPelatihanId });
        }

        var (sukses, pesan) = await _layananPelatihan.DaftarkanPesertaAsync(model);

        if (sukses)
        {
            TempData["PesanSukses"] = pesan;
        }
        else
        {
            TempData["PesanError"] = pesan;
        }

        return RedirectToAction(nameof(DetailProgram), new { id = model.ProgramPelatihanId });
    }

    // POST: /Pelatihan/DaftarPesertaMassal
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = $"{NamaPeran.SuperAdmin},{NamaPeran.HRD}")]
    public async Task<IActionResult> DaftarPesertaMassal(FormDaftarPesertaMassalViewModel model)
    {
        var (sukses, pesan, jumlah) = await _layananPelatihan.DaftarkanPesertaMassalAsync(model);

        if (sukses)
        {
            TempData["PesanSukses"] = $"{pesan} ({jumlah} karyawan terdaftar).";
        }
        else
        {
            TempData["PesanError"] = pesan;
        }

        return RedirectToAction(nameof(DetailProgram), new { id = model.ProgramPelatihanId });
    }

    // POST: /Pelatihan/PerbaruiStatusPeserta
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = $"{NamaPeran.SuperAdmin},{NamaPeran.HRD}")]
    public async Task<IActionResult> PerbaruiStatusPeserta(FormUpdateStatusPesertaViewModel model, Guid programId)
    {
        var (sukses, pesan) = await _layananPelatihan.PerbaruiStatusPesertaAsync(model);

        if (sukses)
        {
            // Jika status lulus, terbitkan sertifikat resmi otomatis bila belum ada
            if (model.StatusKeikutsertaan == StatusKeikutsertaan.Lulus)
            {
                var penerbit = "Divisi People Development";
                await _layananPelatihan.TerbitkanSertifikatAsync(model.PesertaId, penerbit, null, model.Catatan);
            }
            TempData["PesanSukses"] = pesan;
        }
        else
        {
            TempData["PesanError"] = pesan;
        }

        return RedirectToAction(nameof(DetailProgram), new { id = programId });
    }

    // POST: /Pelatihan/HapusPeserta
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = $"{NamaPeran.SuperAdmin},{NamaPeran.HRD}")]
    public async Task<IActionResult> HapusPeserta(Guid pesertaId, Guid programId)
    {
        var (sukses, pesan) = await _layananPelatihan.HapusPesertaAsync(pesertaId);

        if (sukses)
        {
            TempData["PesanSukses"] = pesan;
        }
        else
        {
            TempData["PesanError"] = pesan;
        }

        return RedirectToAction(nameof(DetailProgram), new { id = programId });
    }
    #endregion

    #region Sertifikat & ESS Karyawan
    // GET: /Pelatihan/Sertifikat/{id}
    public async Task<IActionResult> Sertifikat(Guid id)
    {
        var sertifikat = await _layananPelatihan.DapatkanSertifikatByIdAsync(id);
        if (sertifikat == null)
        {
            TempData["PesanError"] = "Sertifikat tidak ditemukan.";
            return RedirectToAction(nameof(Index));
        }

        return View(sertifikat);
    }

    // GET: /Pelatihan/RiwayatSaya
    public async Task<IActionResult> RiwayatSaya()
    {
        var karyawanId = await DapatkanKaryawanIdPenggunaAktifAsync();
        if (!karyawanId.HasValue)
        {
            TempData["PesanError"] = "Profil karyawan Anda belum terhubung. Hubungi HRD.";
            return RedirectToAction("Index", "Dashboard");
        }

        var riwayat = await _layananPelatihan.DapatkanRiwayatPelatihanKaryawanAsync(karyawanId.Value);
        return View(riwayat);
    }
    #endregion

    #region Helpers
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

    private async Task SiapkanDropdownDepartemenAsync(Guid? departemenIdTerpilih = null)
    {
        var daftarDept = await _layananDepartemen.DapatkanSemuaAsync();
        ViewBag.DaftarDepartemen = new SelectList(daftarDept, "Id", "NamaDepartemen", departemenIdTerpilih);
    }

    private async Task SiapkanViewBagKaryawanDanDepartemenAsync()
    {
        await SiapkanDropdownDepartemenAsync();

        var daftarKaryawan = await _context.Karyawan
            .Where(k => !k.IsDeleted && (k.StatusKaryawan == StatusKaryawan.Tetap || k.StatusKaryawan == StatusKaryawan.Kontrak || k.StatusKaryawan == StatusKaryawan.Percobaan))
            .OrderBy(k => k.NamaLengkap)
            .Select(k => new { k.Id, Nama = $"{k.NomorIndukKaryawan} - {k.NamaLengkap} ({(k.Departemen != null ? k.Departemen.NamaDepartemen : "-")})" })
            .ToListAsync();
        ViewBag.DaftarKaryawan = new SelectList(daftarKaryawan, "Id", "Nama");
    }
    #endregion
}
