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
public class RekrutmenController : Controller
{
    private readonly ILayananRekrutmen _layananRekrutmen;
    private readonly ILayananDepartemen _layananDepartemen;
    private readonly ILayananJabatan _layananJabatan;
    private readonly KonteksAplikasi _context;

    public RekrutmenController(
        ILayananRekrutmen layananRekrutmen,
        ILayananDepartemen layananDepartemen,
        ILayananJabatan layananJabatan,
        KonteksAplikasi context)
    {
        _layananRekrutmen = layananRekrutmen;
        _layananDepartemen = layananDepartemen;
        _layananJabatan = layananJabatan;
        _context = context;
    }

    #region Dashboard Rekrutmen
    // GET: /Rekrutmen
    [Authorize(Roles = $"{NamaPeran.SuperAdmin},{NamaPeran.HRD},{NamaPeran.Manager}")]
    public async Task<IActionResult> Index()
    {
        var model = await _layananRekrutmen.DapatkanDashboardAsync();
        return View(model);
    }
    #endregion

    #region Lowongan Kerja
    // GET: /Rekrutmen/Lowongan
    [Authorize(Roles = $"{NamaPeran.SuperAdmin},{NamaPeran.HRD},{NamaPeran.Manager}")]
    public async Task<IActionResult> Lowongan(StatusLowongan? status)
    {
        ViewBag.StatusTerpilih = status;
        var daftar = await _layananRekrutmen.DapatkanSemuaLowonganAsync(status);
        return View(daftar);
    }

    // GET: /Rekrutmen/TambahLowongan
    [Authorize(Roles = $"{NamaPeran.SuperAdmin},{NamaPeran.HRD}")]
    public async Task<IActionResult> TambahLowongan()
    {
        await SiapkanDropdownDepartemenDanJabatanAsync();
        var model = new FormLowonganKerjaViewModel
        {
            KodeLowongan = $"LOW-{DateTime.Today:yyyyMM}-{new Random().Next(100, 999)}",
            TanggalDibuka = DateTime.Today,
            TanggalDitutup = DateTime.Today.AddMonths(1),
            Status = StatusLowongan.Dibuka
        };
        return View(model);
    }

    // POST: /Rekrutmen/TambahLowongan
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = $"{NamaPeran.SuperAdmin},{NamaPeran.HRD}")]
    public async Task<IActionResult> TambahLowongan(FormLowonganKerjaViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await SiapkanDropdownDepartemenDanJabatanAsync(model.DepartemenId, model.JabatanId);
            return View(model);
        }

        var (sukses, pesan, id) = await _layananRekrutmen.SimpanLowonganAsync(model);
        if (!sukses)
        {
            TempData["PesanError"] = pesan;
            await SiapkanDropdownDepartemenDanJabatanAsync(model.DepartemenId, model.JabatanId);
            return View(model);
        }

        TempData["PesanSukses"] = pesan;
        return RedirectToAction(nameof(DetailLowongan), new { id = id!.Value });
    }

    // GET: /Rekrutmen/UbahLowongan/{id}
    [Authorize(Roles = $"{NamaPeran.SuperAdmin},{NamaPeran.HRD}")]
    public async Task<IActionResult> UbahLowongan(Guid id)
    {
        var lowongan = await _layananRekrutmen.DapatkanDetailLowonganAsync(id);
        if (lowongan == null)
        {
            TempData["PesanError"] = "Lowongan kerja tidak ditemukan.";
            return RedirectToAction(nameof(Lowongan));
        }

        var model = new FormLowonganKerjaViewModel
        {
            Id = lowongan.Id,
            KodeLowongan = lowongan.KodeLowongan,
            JudulLowongan = lowongan.JudulLowongan,
            DeskripsiPekerjaan = lowongan.DeskripsiPekerjaan,
            Kualifikasi = lowongan.Kualifikasi,
            TipePekerjaan = lowongan.TipePekerjaan,
            LokasiKerja = lowongan.LokasiKerja,
            JumlahDibutuhkan = lowongan.JumlahDibutuhkan,
            GajiMin = lowongan.GajiMin,
            GajiMax = lowongan.GajiMax,
            TanggalDibuka = lowongan.TanggalDibuka,
            TanggalDitutup = lowongan.TanggalDitutup,
            Status = lowongan.Status
        };

        await SiapkanDropdownDepartemenDanJabatanAsync();
        return View(model);
    }

    // POST: /Rekrutmen/UbahLowongan/{id}
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = $"{NamaPeran.SuperAdmin},{NamaPeran.HRD}")]
    public async Task<IActionResult> UbahLowongan(FormLowonganKerjaViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await SiapkanDropdownDepartemenDanJabatanAsync(model.DepartemenId, model.JabatanId);
            return View(model);
        }

        var (sukses, pesan, id) = await _layananRekrutmen.SimpanLowonganAsync(model);
        if (!sukses)
        {
            TempData["PesanError"] = pesan;
            await SiapkanDropdownDepartemenDanJabatanAsync(model.DepartemenId, model.JabatanId);
            return View(model);
        }

        TempData["PesanSukses"] = pesan;
        return RedirectToAction(nameof(DetailLowongan), new { id = id!.Value });
    }

    // GET: /Rekrutmen/DetailLowongan/{id}
    [Authorize(Roles = $"{NamaPeran.SuperAdmin},{NamaPeran.HRD},{NamaPeran.Manager}")]
    public async Task<IActionResult> DetailLowongan(Guid id)
    {
        var model = await _layananRekrutmen.DapatkanDetailLowonganAsync(id);
        if (model == null)
        {
            TempData["PesanError"] = "Lowongan kerja tidak ditemukan.";
            return RedirectToAction(nameof(Lowongan));
        }

        return View(model);
    }

    // POST: /Rekrutmen/HapusLowongan/{id}
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = $"{NamaPeran.SuperAdmin},{NamaPeran.HRD}")]
    public async Task<IActionResult> HapusLowongan(Guid id)
    {
        var (sukses, pesan) = await _layananRekrutmen.HapusLowonganAsync(id);
        if (sukses) TempData["PesanSukses"] = pesan;
        else TempData["PesanError"] = pesan;

        return RedirectToAction(nameof(Lowongan));
    }
    #endregion

    #region Pelamar
    // GET: /Rekrutmen/Pelamar
    [Authorize(Roles = $"{NamaPeran.SuperAdmin},{NamaPeran.HRD},{NamaPeran.Manager}")]
    public async Task<IActionResult> Pelamar(Guid? lowonganId, StatusPelamar? status)
    {
        ViewBag.LowonganIdTerpilih = lowonganId;
        ViewBag.StatusTerpilih = status;

        var daftarLowongan = await _layananRekrutmen.DapatkanSemuaLowonganAsync();
        ViewBag.DaftarLowongan = new SelectList(daftarLowongan, "Id", "JudulLowongan", lowonganId);

        var daftarPelamar = await _layananRekrutmen.DapatkanSemuaPelamarAsync(lowonganId, status);
        return View(daftarPelamar);
    }

    // GET: /Rekrutmen/DaftarPelamar
    [Authorize(Roles = $"{NamaPeran.SuperAdmin},{NamaPeran.HRD}")]
    public async Task<IActionResult> DaftarPelamar(Guid? lowonganId)
    {
        var daftarLowongan = await _layananRekrutmen.DapatkanSemuaLowonganAsync(StatusLowongan.Dibuka);
        ViewBag.DaftarLowongan = new SelectList(daftarLowongan, "Id", "JudulLowongan", lowonganId);

        var model = new FormPelamarViewModel
        {
            LowonganKerjaId = lowonganId ?? (daftarLowongan.FirstOrDefault()?.Id ?? Guid.Empty)
        };
        return View(model);
    }

    // POST: /Rekrutmen/DaftarPelamar
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = $"{NamaPeran.SuperAdmin},{NamaPeran.HRD}")]
    public async Task<IActionResult> DaftarPelamar(FormPelamarViewModel model)
    {
        if (!ModelState.IsValid)
        {
            var daftarLowongan = await _layananRekrutmen.DapatkanSemuaLowonganAsync(StatusLowongan.Dibuka);
            ViewBag.DaftarLowongan = new SelectList(daftarLowongan, "Id", "JudulLowongan", model.LowonganKerjaId);
            return View(model);
        }

        var (sukses, pesan, id) = await _layananRekrutmen.DaftarPelamarAsync(model);
        if (!sukses)
        {
            TempData["PesanError"] = pesan;
            var daftarLowongan = await _layananRekrutmen.DapatkanSemuaLowonganAsync(StatusLowongan.Dibuka);
            ViewBag.DaftarLowongan = new SelectList(daftarLowongan, "Id", "JudulLowongan", model.LowonganKerjaId);
            return View(model);
        }

        TempData["PesanSukses"] = pesan;
        return RedirectToAction(nameof(DetailPelamar), new { id = id!.Value });
    }

    // GET: /Rekrutmen/DetailPelamar/{id}
    [Authorize(Roles = $"{NamaPeran.SuperAdmin},{NamaPeran.HRD},{NamaPeran.Manager}")]
    public async Task<IActionResult> DetailPelamar(Guid id)
    {
        var model = await _layananRekrutmen.DapatkanDetailPelamarAsync(id);
        if (model == null)
        {
            TempData["PesanError"] = "Data pelamar tidak ditemukan.";
            return RedirectToAction(nameof(Pelamar));
        }

        // Siapkan data untuk modal seleksi administrasi, jadwal wawancara & hasil
        await SiapkanDropdownDepartemenDanJabatanAsync();
        return View(model);
    }

    // POST: /Rekrutmen/SeleksiAdministrasi
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = $"{NamaPeran.SuperAdmin},{NamaPeran.HRD}")]
    public async Task<IActionResult> SeleksiAdministrasi(FormSeleksiAdministrasiViewModel model)
    {
        var (sukses, pesan) = await _layananRekrutmen.ProsesSeleksiAdministrasiAsync(model);
        if (sukses) TempData["PesanSukses"] = pesan;
        else TempData["PesanError"] = pesan;

        return RedirectToAction(nameof(DetailPelamar), new { id = model.PelamarId });
    }

    // GET: /Rekrutmen/UnduhCV/{id}
    [Authorize(Roles = $"{NamaPeran.SuperAdmin},{NamaPeran.HRD},{NamaPeran.Manager}")]
    public async Task<IActionResult> UnduhCV(Guid id)
    {
        var (bytes, contentType, fileName) = await _layananRekrutmen.UnduhCVAsync(id);
        if (bytes == null)
        {
            TempData["PesanError"] = "Berkas CV tidak ditemukan atau Anda tidak berwenang mengakses berkas ini.";
            return RedirectToAction(nameof(Pelamar));
        }

        return File(bytes, contentType, fileName);
    }
    #endregion

    #region Wawancara & Hasil Seleksi
    // POST: /Rekrutmen/SimpanJadwalWawancara
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = $"{NamaPeran.SuperAdmin},{NamaPeran.HRD},{NamaPeran.Manager}")]
    public async Task<IActionResult> SimpanJadwalWawancara(FormJadwalWawancaraViewModel model)
    {
        if (string.IsNullOrWhiteSpace(model.NamaPewawancara))
        {
            model.NamaPewawancara = User.Identity?.Name ?? "Pewawancara HRD";
        }

        var (sukses, pesan, _) = await _layananRekrutmen.SimpanJadwalWawancaraAsync(model);
        if (sukses) TempData["PesanSukses"] = pesan;
        else TempData["PesanError"] = pesan;

        return RedirectToAction(nameof(DetailPelamar), new { id = model.PelamarId });
    }

    // POST: /Rekrutmen/SimpanHasilWawancara
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = $"{NamaPeran.SuperAdmin},{NamaPeran.HRD},{NamaPeran.Manager}")]
    public async Task<IActionResult> SimpanHasilWawancara(FormHasilSeleksiViewModel model)
    {
        var userId = DapatkanUserIdPenggunaAktif();
        if (!userId.HasValue)
        {
            TempData["PesanError"] = "Sesi autentikasi tidak valid.";
            return RedirectToAction(nameof(DetailPelamar), new { id = model.PelamarId });
        }

        var namaPenilai = User.Identity?.Name ?? "Penilai HRD";
        var (sukses, pesan, _) = await _layananRekrutmen.SimpanHasilSeleksiAsync(model, userId.Value, namaPenilai);
        if (sukses) TempData["PesanSukses"] = pesan;
        else TempData["PesanError"] = pesan;

        return RedirectToAction(nameof(DetailPelamar), new { id = model.PelamarId });
    }

    // POST: /Rekrutmen/UbahStatusPelamar
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = $"{NamaPeran.SuperAdmin},{NamaPeran.HRD}")]
    public async Task<IActionResult> UbahStatusPelamar(Guid pelamarId, StatusPelamar statusBaru, string? catatan)
    {
        var (sukses, pesan) = await _layananRekrutmen.UbahStatusPelamarAsync(pelamarId, statusBaru, catatan);
        if (sukses) TempData["PesanSukses"] = pesan;
        else TempData["PesanError"] = pesan;

        return RedirectToAction(nameof(DetailPelamar), new { id = pelamarId });
    }

    // POST: /Rekrutmen/TerimaKandidat
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = $"{NamaPeran.SuperAdmin},{NamaPeran.HRD}")]
    public async Task<IActionResult> TerimaKandidat(FormTerimaPelamarViewModel model)
    {
        var (sukses, pesan, karyawanId) = await _layananRekrutmen.TerimaDanKonversiKaryawanAsync(model);
        if (!sukses)
        {
            TempData["PesanError"] = pesan;
            return RedirectToAction(nameof(DetailPelamar), new { id = model.PelamarId });
        }

        TempData["PesanSukses"] = pesan;
        return RedirectToAction("Detail", "Karyawan", new { id = karyawanId!.Value });
    }
    #endregion

    #region Helpers
    private Guid? DapatkanUserIdPenggunaAktif()
    {
        var claimId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(claimId, out var id) ? id : null;
    }

    private async Task SiapkanDropdownDepartemenDanJabatanAsync(Guid? departemenIdTerpilih = null, Guid? jabatanIdTerpilih = null)
    {
        var daftarDept = await _layananDepartemen.DapatkanSemuaAsync();
        ViewBag.DaftarDepartemen = new SelectList(daftarDept, "Id", "NamaDepartemen", departemenIdTerpilih);

        var daftarJab = await _layananJabatan.DapatkanSemuaAsync();
        ViewBag.DaftarJabatan = new SelectList(daftarJab, "Id", "NamaJabatan", jabatanIdTerpilih);
    }
    #endregion
}
