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
public class CutiController : Controller
{
    private readonly ILayananCuti _layananCuti;
    private readonly ILayananDepartemen _layananDepartemen;
    private readonly KonteksAplikasi _context;

    public CutiController(
        ILayananCuti layananCuti,
        ILayananDepartemen layananDepartemen,
        KonteksAplikasi context)
    {
        _layananCuti = layananCuti;
        _layananDepartemen = layananDepartemen;
        _context = context;
    }

    // GET: /Cuti
    public async Task<IActionResult> Index(FilterPengajuanCutiViewModel? filter)
    {
        filter ??= new FilterPengajuanCutiViewModel();
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

        var model = await _layananCuti.DapatkanDaftarPengajuanCutiAsync(filter, karyawanId);
        await SiapkanViewBagDepartemenDanJenisCutiAsync(filter.DepartemenId, filter.JenisCutiId);

        return View(model);
    }

    // GET: /Cuti/Pengajuan (Form Pengajuan Cuti Baru)
    public async Task<IActionResult> Pengajuan()
    {
        var karyawanId = await DapatkanKaryawanIdPenggunaAktifAsync();
        if (!karyawanId.HasValue)
        {
            TempData["PesanError"] = "Akun Anda belum ditautkan ke profil data karyawan. Hubungi HRD untuk menautkan akun pengguna Anda.";
            return RedirectToAction(nameof(Index));
        }

        await SiapkanViewBagJenisCutiAsync();

        var model = new FormPengajuanCutiViewModel
        {
            KaryawanId = karyawanId.Value,
            TanggalMulai = DateTime.Today.AddDays(1),
            TanggalSelesai = DateTime.Today.AddDays(1),
            JumlahHari = 1
        };

        return View(model);
    }

    // POST: /Cuti/Pengajuan
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Pengajuan(FormPengajuanCutiViewModel model)
    {
        var karyawanId = await DapatkanKaryawanIdPenggunaAktifAsync();
        if (!karyawanId.HasValue)
        {
            TempData["PesanError"] = "Profil data karyawan tidak ditemukan.";
            return RedirectToAction(nameof(Index));
        }

        if (!ModelState.IsValid)
        {
            var errors = string.Join("; ", ModelState.Values.SelectMany(v => v.Errors).Select(e => !string.IsNullOrEmpty(e.ErrorMessage) ? e.ErrorMessage : e.Exception?.Message));
            TempData["PesanError"] = $"Validasi form gagal: {errors}";
            await SiapkanViewBagJenisCutiAsync(model.JenisCutiId);
            return View(model);
        }

        var (sukses, pesan, pengajuanId) = await _layananCuti.AjukanCutiAsync(karyawanId.Value, model, model.BerkasDokumen);

        if (!sukses)
        {
            TempData["PesanError"] = pesan;
            await SiapkanViewBagJenisCutiAsync(model.JenisCutiId);
            return View(model);
        }

        TempData["PesanSukses"] = pesan;
        return RedirectToAction(nameof(Detail), new { id = pengajuanId });
    }

    // GET: /Cuti/Detail/{id}
    public async Task<IActionResult> Detail(Guid id)
    {
        var model = await _layananCuti.DapatkanDetailPengajuanCutiAsync(id);
        if (model == null)
        {
            TempData["PesanError"] = "Data pengajuan cuti tidak ditemukan.";
            return RedirectToAction(nameof(Index));
        }

        return View(model);
    }

    // POST: /Cuti/SetujuiManager
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = $"{NamaPeran.SuperAdmin},{NamaPeran.HRD},{NamaPeran.Manager}")]
    public async Task<IActionResult> SetujuiManager(PersetujuanCutiRequestViewModel model)
    {
        var userId = DapatkanUserIdPenggunaAktif();
        if (!userId.HasValue)
        {
            TempData["PesanError"] = "Sesi autentikasi tidak valid.";
            return RedirectToAction(nameof(Detail), new { id = model.PengajuanCutiId });
        }

        var (sukses, pesan) = await _layananCuti.ProsesPersetujuanManagerAsync(model.PengajuanCutiId, true, model.Catatan, userId.Value);
        if (sukses)
        {
            TempData["PesanSukses"] = pesan;
        }
        else
        {
            TempData["PesanError"] = pesan;
        }

        return RedirectToAction(nameof(Detail), new { id = model.PengajuanCutiId });
    }

    // POST: /Cuti/TolakManager
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = $"{NamaPeran.SuperAdmin},{NamaPeran.HRD},{NamaPeran.Manager}")]
    public async Task<IActionResult> TolakManager(PersetujuanCutiRequestViewModel model)
    {
        var userId = DapatkanUserIdPenggunaAktif();
        if (!userId.HasValue)
        {
            TempData["PesanError"] = "Sesi autentikasi tidak valid.";
            return RedirectToAction(nameof(Detail), new { id = model.PengajuanCutiId });
        }

        var (sukses, pesan) = await _layananCuti.ProsesPersetujuanManagerAsync(model.PengajuanCutiId, false, model.Catatan, userId.Value);
        if (sukses)
        {
            TempData["PesanSukses"] = pesan;
        }
        else
        {
            TempData["PesanError"] = pesan;
        }

        return RedirectToAction(nameof(Detail), new { id = model.PengajuanCutiId });
    }

    // POST: /Cuti/VerifikasiHRD
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = $"{NamaPeran.SuperAdmin},{NamaPeran.HRD}")]
    public async Task<IActionResult> VerifikasiHRD(PersetujuanCutiRequestViewModel model)
    {
        var userId = DapatkanUserIdPenggunaAktif();
        if (!userId.HasValue)
        {
            TempData["PesanError"] = "Sesi autentikasi tidak valid.";
            return RedirectToAction(nameof(Detail), new { id = model.PengajuanCutiId });
        }

        var (sukses, pesan) = await _layananCuti.ProsesVerifikasiHRDAsync(model.PengajuanCutiId, true, model.Catatan, userId.Value);
        if (sukses)
        {
            TempData["PesanSukses"] = pesan;
        }
        else
        {
            TempData["PesanError"] = pesan;
        }

        return RedirectToAction(nameof(Detail), new { id = model.PengajuanCutiId });
    }

    // POST: /Cuti/TolakHRD
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = $"{NamaPeran.SuperAdmin},{NamaPeran.HRD}")]
    public async Task<IActionResult> TolakHRD(PersetujuanCutiRequestViewModel model)
    {
        var userId = DapatkanUserIdPenggunaAktif();
        if (!userId.HasValue)
        {
            TempData["PesanError"] = "Sesi autentikasi tidak valid.";
            return RedirectToAction(nameof(Detail), new { id = model.PengajuanCutiId });
        }

        var (sukses, pesan) = await _layananCuti.ProsesVerifikasiHRDAsync(model.PengajuanCutiId, false, model.Catatan, userId.Value);
        if (sukses)
        {
            TempData["PesanSukses"] = pesan;
        }
        else
        {
            TempData["PesanError"] = pesan;
        }

        return RedirectToAction(nameof(Detail), new { id = model.PengajuanCutiId });
    }

    // GET: /Cuti/UnduhDokumen/{id}
    public async Task<IActionResult> UnduhDokumen(Guid id)
    {
        var (konten, tipeKonten, namaFile) = await _layananCuti.UnduhDokumenCutiAsync(id);
        if (konten == null || string.IsNullOrEmpty(tipeKonten) || string.IsNullOrEmpty(namaFile))
        {
            return NotFound("Dokumen lampiran cuti tidak ditemukan atau Anda tidak berwenang mengaksesnya.");
        }

        return File(konten, tipeKonten, namaFile);
    }

    // GET: /Cuti/JenisCuti
    [Authorize(Roles = $"{NamaPeran.SuperAdmin},{NamaPeran.HRD}")]
    public async Task<IActionResult> JenisCuti()
    {
        var data = await _layananCuti.DapatkanDaftarJenisCutiAsync();
        return View(data);
    }

    // POST: /Cuti/SimpanJenisCuti
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = $"{NamaPeran.SuperAdmin},{NamaPeran.HRD}")]
    public async Task<IActionResult> SimpanJenisCuti(JenisCutiViewModel model)
    {
        if (!ModelState.IsValid)
        {
            TempData["PesanError"] = "Data jenis cuti tidak valid.";
            return RedirectToAction(nameof(JenisCuti));
        }

        var (sukses, pesan) = await _layananCuti.SimpanJenisCutiAsync(model);
        if (sukses)
        {
            TempData["PesanSukses"] = pesan;
        }
        else
        {
            TempData["PesanError"] = pesan;
        }

        return RedirectToAction(nameof(JenisCuti));
    }

    // POST: /Cuti/HapusJenisCuti/{id}
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = $"{NamaPeran.SuperAdmin},{NamaPeran.HRD}")]
    public async Task<IActionResult> HapusJenisCuti(Guid id)
    {
        var (sukses, pesan) = await _layananCuti.HapusJenisCutiAsync(id);
        if (sukses)
        {
            TempData["PesanSukses"] = pesan;
        }
        else
        {
            TempData["PesanError"] = pesan;
        }

        return RedirectToAction(nameof(JenisCuti));
    }

    // GET: /Cuti/SaldoCuti
    [Authorize(Roles = $"{NamaPeran.SuperAdmin},{NamaPeran.HRD}")]
    public async Task<IActionResult> SaldoCuti(int? tahun, Guid? departemenId, Guid? karyawanId)
    {
        var thn = tahun ?? DateTime.Today.Year;
        var data = await _layananCuti.DapatkanDaftarSaldoCutiAsync(thn, departemenId, karyawanId);

        await SiapkanViewBagDepartemenAsync(departemenId);
        await SiapkanViewBagJenisCutiAsync();
        await SiapkanViewBagKaryawanAsync(karyawanId);
        ViewBag.Tahun = thn;

        return View(data);
    }

    // POST: /Cuti/AturSaldoCuti
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = $"{NamaPeran.SuperAdmin},{NamaPeran.HRD}")]
    public async Task<IActionResult> AturSaldoCuti(AturSaldoCutiViewModel model)
    {
        if (!ModelState.IsValid)
        {
            TempData["PesanError"] = "Data pengaturan saldo cuti tidak valid.";
            return RedirectToAction(nameof(SaldoCuti), new { tahun = model.PeriodeTahun });
        }

        var (sukses, pesan) = await _layananCuti.AturSaldoCutiAsync(model);
        if (sukses)
        {
            TempData["PesanSukses"] = pesan;
        }
        else
        {
            TempData["PesanError"] = pesan;
        }

        return RedirectToAction(nameof(SaldoCuti), new { tahun = model.PeriodeTahun });
    }

    // POST: /Cuti/InisialisasiSaldoMassal
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = $"{NamaPeran.SuperAdmin},{NamaPeran.HRD}")]
    public async Task<IActionResult> InisialisasiSaldoMassal(int tahun)
    {
        var (sukses, pesan) = await _layananCuti.InisialisasiSaldoCutiTahunanMassalAsync(tahun);
        if (sukses)
        {
            TempData["PesanSukses"] = pesan;
        }
        else
        {
            TempData["PesanError"] = pesan;
        }

        return RedirectToAction(nameof(SaldoCuti), new { tahun });
    }

    private Guid? DapatkanUserIdPenggunaAktif()
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!string.IsNullOrEmpty(userIdStr) && Guid.TryParse(userIdStr, out var userId))
        {
            return userId;
        }
        return null;
    }

    private async Task<Guid?> DapatkanKaryawanIdPenggunaAktifAsync()
    {
        var userId = DapatkanUserIdPenggunaAktif();
        if (userId.HasValue)
        {
            var kry = await _context.Karyawan.FirstOrDefaultAsync(k => k.PenggunaId == userId.Value);
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

    private async Task SiapkanViewBagJenisCutiAsync(Guid? jenisCutiIdTerpilih = null)
    {
        var daftarJenisCuti = await _layananCuti.DapatkanDaftarJenisCutiAsync();
        ViewBag.DaftarJenisCuti = new SelectList(daftarJenisCuti, "Id", "NamaJenisCuti", jenisCutiIdTerpilih);
    }

    private async Task SiapkanViewBagDepartemenAsync(Guid? departemenIdTerpilih = null)
    {
        var daftarDept = await _layananDepartemen.DapatkanSemuaAsync();
        ViewBag.DaftarDepartemen = new SelectList(daftarDept, "Id", "NamaDepartemen", departemenIdTerpilih);
    }

    private async Task SiapkanViewBagDepartemenDanJenisCutiAsync(Guid? departemenIdTerpilih, Guid? jenisCutiIdTerpilih)
    {
        await SiapkanViewBagDepartemenAsync(departemenIdTerpilih);
        await SiapkanViewBagJenisCutiAsync(jenisCutiIdTerpilih);
    }

    private async Task SiapkanViewBagKaryawanAsync(Guid? karyawanIdTerpilih = null)
    {
        var daftarKaryawan = await _context.Karyawan
            .Where(k => k.StatusKaryawan == StatusKaryawan.Tetap || k.StatusKaryawan == StatusKaryawan.Kontrak)
            .OrderBy(k => k.NamaLengkap)
            .Select(k => new { k.Id, Nama = $"{k.NomorIndukKaryawan} - {k.NamaLengkap}" })
            .ToListAsync();
        ViewBag.DaftarKaryawan = new SelectList(daftarKaryawan, "Id", "Nama", karyawanIdTerpilih);
    }
}
