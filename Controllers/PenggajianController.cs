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
public class PenggajianController : Controller
{
    private readonly ILayananPenggajian _layananPenggajian;
    private readonly ILayananDepartemen _layananDepartemen;
    private readonly KonteksAplikasi _context;

    public PenggajianController(
        ILayananPenggajian layananPenggajian,
        ILayananDepartemen layananDepartemen,
        KonteksAplikasi context)
    {
        _layananPenggajian = layananPenggajian;
        _layananDepartemen = layananDepartemen;
        _context = context;
    }

    #region Dashboard & Periode Penggajian

    // GET: /Penggajian
    public async Task<IActionResult> Index()
    {
        // Jika karyawan murni (bukan HRD/Finance/SuperAdmin/Manager), arahkan langsung ke riwayat slip ESS miliknya
        if (User.IsInRole(NamaPeran.Karyawan) &&
            !User.IsInRole(NamaPeran.SuperAdmin) &&
            !User.IsInRole(NamaPeran.HRD) &&
            !User.IsInRole(NamaPeran.Finance) &&
            !User.IsInRole(NamaPeran.Manager))
        {
            return RedirectToAction(nameof(Riwayat));
        }

        var daftarPeriode = await _layananPenggajian.DapatkanDaftarPeriodeAsync();
        return View(daftarPeriode);
    }

    // GET: /Penggajian/TambahPeriode
    [Authorize(Roles = $"{NamaPeran.SuperAdmin},{NamaPeran.HRD},{NamaPeran.Finance}")]
    public async Task<IActionResult> TambahPeriode()
    {
        var kebijakan = await _layananPenggajian.DapatkanKebijakanPerusahaanAsync();
        var hariIni = DateTime.Today;

        var bulanDepan = hariIni.AddMonths(1);
        var tglMulai = new DateTime(hariIni.Year, hariIni.Month, Math.Min(kebijakan.TanggalCutoffAwal, DateTime.DaysInMonth(hariIni.Year, hariIni.Month)));
        var tglSelesai = new DateTime(bulanDepan.Year, bulanDepan.Month, Math.Min(kebijakan.TanggalCutoffAkhir, DateTime.DaysInMonth(bulanDepan.Year, bulanDepan.Month)));
        var tglBayar = new DateTime(bulanDepan.Year, bulanDepan.Month, Math.Min(kebijakan.TanggalPembayaranGaji, DateTime.DaysInMonth(bulanDepan.Year, bulanDepan.Month)));

        var model = new FormPeriodePenggajianViewModel
        {
            NamaPeriode = $"Gaji Periode {hariIni:MMMM yyyy}",
            Bulan = hariIni.Month,
            Tahun = hariIni.Year,
            TanggalMulai = tglMulai,
            TanggalSelesai = tglSelesai,
            TanggalPembayaran = tglBayar
        };

        return View(model);
    }

    // POST: /Penggajian/TambahPeriode
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = $"{NamaPeran.SuperAdmin},{NamaPeran.HRD},{NamaPeran.Finance}")]
    public async Task<IActionResult> TambahPeriode(FormPeriodePenggajianViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var (sukses, pesan, periodeId) = await _layananPenggajian.SimpanPeriodeAsync(model);
        if (!sukses)
        {
            TempData["PesanError"] = pesan;
            return View(model);
        }

        TempData["PesanSukses"] = pesan;
        return RedirectToAction(nameof(PeriodeDetail), new { id = periodeId });
    }

    // GET: /Penggajian/PeriodeDetail/{id}
    [Authorize(Roles = $"{NamaPeran.SuperAdmin},{NamaPeran.HRD},{NamaPeran.Finance},{NamaPeran.Manager}")]
    public async Task<IActionResult> PeriodeDetail(Guid id)
    {
        var model = await _layananPenggajian.DapatkanDetailPeriodeAsync(id);
        if (model == null)
        {
            TempData["PesanError"] = "Periode penggajian tidak ditemukan.";
            return RedirectToAction(nameof(Index));
        }

        await SiapkanViewBagDepartemenAsync();
        return View(model);
    }

    // POST: /Penggajian/HapusPeriode/{id}
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = $"{NamaPeran.SuperAdmin},{NamaPeran.HRD},{NamaPeran.Finance}")]
    public async Task<IActionResult> HapusPeriode(Guid id)
    {
        var (sukses, pesan) = await _layananPenggajian.HapusPeriodeAsync(id);
        if (sukses)
        {
            TempData["PesanSukses"] = pesan;
        }
        else
        {
            TempData["PesanError"] = pesan;
        }

        return RedirectToAction(nameof(Index));
    }

    // POST: /Penggajian/HitungBatch
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = $"{NamaPeran.SuperAdmin},{NamaPeran.HRD},{NamaPeran.Finance}")]
    public async Task<IActionResult> HitungBatch(HitungGajiBatchRequestViewModel model)
    {
        var userId = DapatkanUserIdPenggunaAktif();
        if (!userId.HasValue)
        {
            TempData["PesanError"] = "Sesi pengguna tidak valid.";
            return RedirectToAction(nameof(PeriodeDetail), new { id = model.PeriodePenggajianId });
        }

        var (sukses, pesan, total) = await _layananPenggajian.HitungPenggajianBatchAsync(model.PeriodePenggajianId, model.DepartemenId, userId.Value);
        if (sukses)
        {
            TempData["PesanSukses"] = pesan;
        }
        else
        {
            TempData["PesanError"] = pesan;
        }

        return RedirectToAction(nameof(PeriodeDetail), new { id = model.PeriodePenggajianId });
    }

    // POST: /Penggajian/SetujuiPeriode
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = $"{NamaPeran.SuperAdmin},{NamaPeran.HRD},{NamaPeran.Finance}")]
    public async Task<IActionResult> SetujuiPeriode(SetujuiPeriodeRequestViewModel model)
    {
        var userId = DapatkanUserIdPenggunaAktif();
        if (!userId.HasValue)
        {
            TempData["PesanError"] = "Sesi pengguna tidak valid.";
            return RedirectToAction(nameof(PeriodeDetail), new { id = model.PeriodePenggajianId });
        }

        var (sukses, pesan) = await _layananPenggajian.SetujuiPenggajianPeriodeAsync(model.PeriodePenggajianId, userId.Value, model.Catatan);
        if (sukses)
        {
            TempData["PesanSukses"] = pesan;
        }
        else
        {
            TempData["PesanError"] = pesan;
        }

        return RedirectToAction(nameof(PeriodeDetail), new { id = model.PeriodePenggajianId });
    }

    #endregion

    #region Rincian Gaji & Cetak Slip Gaji

    // GET: /Penggajian/Detail/{id}
    [Authorize(Roles = $"{NamaPeran.SuperAdmin},{NamaPeran.HRD},{NamaPeran.Finance},{NamaPeran.Manager}")]
    public async Task<IActionResult> Detail(Guid id)
    {
        var model = await _layananPenggajian.DapatkanRincianPenggajianAsync(id);
        if (model == null)
        {
            TempData["PesanError"] = "Rincian penggajian karyawan tidak ditemukan.";
            return RedirectToAction(nameof(Index));
        }

        return View(model);
    }

    // GET: /Penggajian/Slip/{id}
    public async Task<IActionResult> Slip(Guid id)
    {
        Guid? karyawanIdLogin = null;
        if (User.IsInRole(NamaPeran.Karyawan) &&
            !User.IsInRole(NamaPeran.SuperAdmin) &&
            !User.IsInRole(NamaPeran.HRD) &&
            !User.IsInRole(NamaPeran.Finance) &&
            !User.IsInRole(NamaPeran.Manager))
        {
            karyawanIdLogin = await DapatkanKaryawanIdPenggunaAktifAsync();
            if (!karyawanIdLogin.HasValue)
            {
                TempData["PesanError"] = "Profil karyawan tidak ditemukan.";
                return RedirectToAction(nameof(Index));
            }
        }

        var model = await _layananPenggajian.DapatkanSlipGajiAsync(id, karyawanIdLogin);
        if (model == null)
        {
            TempData["PesanError"] = "Slip gaji tidak ditemukan atau Anda tidak berwenang mengakses slip gaji ini.";
            return RedirectToAction(nameof(Index));
        }

        return View(model);
    }

    // GET: /Penggajian/Riwayat (ESS Karyawan)
    public async Task<IActionResult> Riwayat()
    {
        var karyawanId = await DapatkanKaryawanIdPenggunaAktifAsync();
        if (!karyawanId.HasValue)
        {
            TempData["PesanError"] = "Akun Anda belum ditautkan ke data karyawan.";
            return RedirectToAction("Index", "Dasbor");
        }

        var data = await _layananPenggajian.DapatkanRiwayatPenggajianKaryawanAsync(karyawanId.Value);
        return View(data);
    }

    // GET: /Penggajian/Ekspor/{id}
    [Authorize(Roles = $"{NamaPeran.SuperAdmin},{NamaPeran.HRD},{NamaPeran.Finance}")]
    public async Task<IActionResult> Ekspor(Guid id, Guid? periodeId)
    {
        var targetId = id != Guid.Empty ? id : (periodeId ?? Guid.Empty);
        var (fileBytes, namaFile) = await _layananPenggajian.EksporLaporanPayrollCsvAsync(targetId);
        if (fileBytes == null)
        {
            TempData["PesanError"] = "Data laporan penggajian tidak ditemukan.";
            return RedirectToAction(nameof(Index));
        }

        return File(fileBytes, "text/csv; charset=utf-8", namaFile);
    }

    #endregion

    #region Master Komponen Gaji

    // GET: /Penggajian/Komponen
    [Authorize(Roles = $"{NamaPeran.SuperAdmin},{NamaPeran.HRD},{NamaPeran.Finance}")]
    public async Task<IActionResult> Komponen()
    {
        var daftar = await _layananPenggajian.DapatkanDaftarKomponenAsync();
        return View(daftar);
    }

    // POST: /Penggajian/SimpanKomponen
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = $"{NamaPeran.SuperAdmin},{NamaPeran.HRD},{NamaPeran.Finance}")]
    public async Task<IActionResult> SimpanKomponen(KomponenGajiViewModel model)
    {
        if (!ModelState.IsValid)
        {
            TempData["PesanError"] = "Data input komponen gaji tidak valid.";
            return RedirectToAction(nameof(Komponen));
        }

        var (sukses, pesan) = await _layananPenggajian.SimpanKomponenAsync(model);
        if (sukses)
        {
            TempData["PesanSukses"] = pesan;
        }
        else
        {
            TempData["PesanError"] = pesan;
        }

        return RedirectToAction(nameof(Komponen));
    }

    // POST: /Penggajian/HapusKomponen/{id}
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = $"{NamaPeran.SuperAdmin},{NamaPeran.HRD},{NamaPeran.Finance}")]
    public async Task<IActionResult> HapusKomponen(Guid id)
    {
        var (sukses, pesan) = await _layananPenggajian.HapusKomponenAsync(id);
        if (sukses)
        {
            TempData["PesanSukses"] = pesan;
        }
        else
        {
            TempData["PesanError"] = pesan;
        }

        return RedirectToAction(nameof(Komponen));
    }

    #endregion

    #region Pengaturan Gaji Karyawan

    // GET: /Penggajian/AturGajiKaryawan/{id}
    [Authorize(Roles = $"{NamaPeran.SuperAdmin},{NamaPeran.HRD},{NamaPeran.Finance}")]
    public async Task<IActionResult> AturGajiKaryawan(Guid id, Guid? karyawanId)
    {
        var targetKaryawanId = id != Guid.Empty ? id : (karyawanId ?? Guid.Empty);
        var model = await _layananPenggajian.DapatkanAturGajiKaryawanAsync(targetKaryawanId);
        if (model == null)
        {
            TempData["PesanError"] = "Data karyawan tidak ditemukan.";
            return RedirectToAction(nameof(Index));
        }

        return View(model);
    }

    // POST: /Penggajian/SimpanGajiKaryawan
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = $"{NamaPeran.SuperAdmin},{NamaPeran.HRD},{NamaPeran.Finance}")]
    public async Task<IActionResult> SimpanGajiKaryawan(FormSimpanGajiKaryawanViewModel model)
    {
        var (sukses, pesan) = await _layananPenggajian.SimpanGajiKaryawanAsync(model);
        if (sukses)
        {
            TempData["PesanSukses"] = pesan;
        }
        else
        {
            TempData["PesanError"] = pesan;
        }

        return RedirectToAction(nameof(AturGajiKaryawan), new { id = model.KaryawanId });
    }

    #endregion

    #region Kebijakan Penggajian

    // GET: /Penggajian/Kebijakan
    [Authorize(Roles = $"{NamaPeran.SuperAdmin},{NamaPeran.HRD},{NamaPeran.Finance}")]
    public async Task<IActionResult> Kebijakan()
    {
        var model = await _layananPenggajian.DapatkanKebijakanPerusahaanAsync();
        return View(model);
    }

    // POST: /Penggajian/SimpanKebijakan
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = $"{NamaPeran.SuperAdmin},{NamaPeran.HRD},{NamaPeran.Finance}")]
    public async Task<IActionResult> SimpanKebijakan(KebijakanPenggajianViewModel model)
    {
        var (sukses, pesan) = await _layananPenggajian.SimpanKebijakanPerusahaanAsync(model);
        if (sukses)
        {
            TempData["PesanSukses"] = pesan;
        }
        else
        {
            TempData["PesanError"] = pesan;
        }

        return RedirectToAction(nameof(Kebijakan));
    }

    #endregion

    #region Helpers Otorisasi & ViewBag

    private Guid? DapatkanUserIdPenggunaAktif()
    {
        var claimId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(claimId, out var id) ? id : null;
    }

    private async Task<Guid?> DapatkanKaryawanIdPenggunaAktifAsync()
    {
        var userId = DapatkanUserIdPenggunaAktif();
        if (!userId.HasValue) return null;

        var karyawan = await _context.Karyawan
            .FirstOrDefaultAsync(k => k.PenggunaId == userId.Value);

        return karyawan?.Id;
    }

    private async Task SiapkanViewBagDepartemenAsync(Guid? departemenIdTerpilih = null)
    {
        var daftarDept = await _layananDepartemen.DapatkanSemuaAsync();
        ViewBag.DaftarDepartemen = new SelectList(daftarDept, "Id", "NamaDepartemen", departemenIdTerpilih);
    }

    #endregion
}
