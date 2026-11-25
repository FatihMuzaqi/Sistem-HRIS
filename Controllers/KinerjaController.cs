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
public class KinerjaController : Controller
{
    private readonly ILayananPenilaianKinerja _layananKinerja;
    private readonly ILayananDepartemen _layananDepartemen;
    private readonly ILayananJabatan _layananJabatan;
    private readonly ILayananKaryawan _layananKaryawan;
    private readonly KonteksAplikasi _context;

    public KinerjaController(
        ILayananPenilaianKinerja layananKinerja,
        ILayananDepartemen layananDepartemen,
        ILayananJabatan layananJabatan,
        ILayananKaryawan layananKaryawan,
        KonteksAplikasi context)
    {
        _layananKinerja = layananKinerja;
        _layananDepartemen = layananDepartemen;
        _layananJabatan = layananJabatan;
        _layananKaryawan = layananKaryawan;
        _context = context;
    }

    #region Dashboard Kinerja
    // GET: /Kinerja
    [Authorize(Roles = $"{NamaPeran.SuperAdmin},{NamaPeran.HRD},{NamaPeran.Manager}")]
    public async Task<IActionResult> Index()
    {
        // Jika karyawan murni, alihkan ke Riwayat Penilaian Pribadi
        if (User.IsInRole(NamaPeran.Karyawan) &&
            !User.IsInRole(NamaPeran.SuperAdmin) &&
            !User.IsInRole(NamaPeran.HRD) &&
            !User.IsInRole(NamaPeran.Manager))
        {
            return RedirectToAction(nameof(RiwayatSaya));
        }

        var model = await _layananKinerja.DapatkanDashboardAsync();
        return View(model);
    }
    #endregion

    #region Periode Penilaian
    // GET: /Kinerja/Periode
    [Authorize(Roles = $"{NamaPeran.SuperAdmin},{NamaPeran.HRD},{NamaPeran.Manager}")]
    public async Task<IActionResult> Periode()
    {
        var daftar = await _layananKinerja.DapatkanSemuaPeriodeAsync();
        return View(daftar);
    }

    // GET: /Kinerja/TambahPeriode
    [Authorize(Roles = $"{NamaPeran.SuperAdmin},{NamaPeran.HRD}")]
    public IActionResult TambahPeriode()
    {
        var model = new FormPeriodePenilaianViewModel
        {
            NamaPeriode = $"Penilaian Kinerja Semester { (DateTime.Today.Month <= 6 ? 1 : 2) } - {DateTime.Today.Year}",
            Tahun = DateTime.Today.Year,
            SemesterAtauKuartal = DateTime.Today.Month <= 6 ? "Semester 1" : "Semester 2",
            TanggalMulai = DateTime.Today,
            TanggalSelesai = DateTime.Today.AddMonths(1),
            Status = StatusPeriodePenilaian.Aktif
        };
        return View(model);
    }

    // POST: /Kinerja/TambahPeriode
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = $"{NamaPeran.SuperAdmin},{NamaPeran.HRD}")]
    public async Task<IActionResult> TambahPeriode(FormPeriodePenilaianViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var (sukses, pesan, _) = await _layananKinerja.SimpanPeriodeAsync(model);
        if (!sukses)
        {
            TempData["PesanError"] = pesan;
            return View(model);
        }

        TempData["PesanSukses"] = pesan;
        return RedirectToAction(nameof(Periode));
    }

    // GET: /Kinerja/UbahPeriode/{id}
    [Authorize(Roles = $"{NamaPeran.SuperAdmin},{NamaPeran.HRD}")]
    public async Task<IActionResult> UbahPeriode(Guid id)
    {
        var p = await _layananKinerja.DapatkanPeriodeByIdAsync(id);
        if (p == null)
        {
            TempData["PesanError"] = "Periode penilaian tidak ditemukan.";
            return RedirectToAction(nameof(Periode));
        }

        var model = new FormPeriodePenilaianViewModel
        {
            Id = p.Id,
            NamaPeriode = p.NamaPeriode,
            Tahun = p.Tahun,
            SemesterAtauKuartal = p.SemesterAtauKuartal,
            TanggalMulai = p.TanggalMulai,
            TanggalSelesai = p.TanggalSelesai,
            Status = p.Status,
            Catatan = p.Catatan
        };
        return View(model);
    }

    // POST: /Kinerja/UbahPeriode/{id}
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = $"{NamaPeran.SuperAdmin},{NamaPeran.HRD}")]
    public async Task<IActionResult> UbahPeriode(FormPeriodePenilaianViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var (sukses, pesan, _) = await _layananKinerja.SimpanPeriodeAsync(model);
        if (!sukses)
        {
            TempData["PesanError"] = pesan;
            return View(model);
        }

        TempData["PesanSukses"] = pesan;
        return RedirectToAction(nameof(Periode));
    }

    // POST: /Kinerja/HapusPeriode/{id}
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = $"{NamaPeran.SuperAdmin},{NamaPeran.HRD}")]
    public async Task<IActionResult> HapusPeriode(Guid id)
    {
        var (sukses, pesan) = await _layananKinerja.HapusPeriodeAsync(id);
        if (sukses) TempData["PesanSukses"] = pesan;
        else TempData["PesanError"] = pesan;

        return RedirectToAction(nameof(Periode));
    }
    #endregion

    #region Indikator KPI
    // GET: /Kinerja/Indikator
    [Authorize(Roles = $"{NamaPeran.SuperAdmin},{NamaPeran.HRD},{NamaPeran.Manager}")]
    public async Task<IActionResult> Indikator(Guid? departemenId)
    {
        ViewBag.DepartemenIdTerpilih = departemenId;
        var daftarDept = await _layananDepartemen.DapatkanSemuaAsync();
        ViewBag.DaftarDepartemen = new SelectList(daftarDept, "Id", "NamaDepartemen", departemenId);

        var daftar = await _layananKinerja.DapatkanSemuaIndikatorAsync(departemenId);
        return View(daftar);
    }

    // GET: /Kinerja/TambahIndikator
    [Authorize(Roles = $"{NamaPeran.SuperAdmin},{NamaPeran.HRD}")]
    public async Task<IActionResult> TambahIndikator()
    {
        await SiapkanDropdownDepartemenDanJabatanAsync();
        var model = new FormIndikatorKPIViewModel
        {
            KodeIndikator = $"KPI-{DateTime.Today:yy}{new Random().Next(10, 99)}",
            TargetKuantitatif = 100,
            Satuan = "%",
            BobotBawaan = 20.00m,
            StatusAktif = true
        };
        return View(model);
    }

    // POST: /Kinerja/TambahIndikator
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = $"{NamaPeran.SuperAdmin},{NamaPeran.HRD}")]
    public async Task<IActionResult> TambahIndikator(FormIndikatorKPIViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await SiapkanDropdownDepartemenDanJabatanAsync(model.DepartemenId, model.JabatanId);
            return View(model);
        }

        var (sukses, pesan, _) = await _layananKinerja.SimpanIndikatorAsync(model);
        if (!sukses)
        {
            TempData["PesanError"] = pesan;
            await SiapkanDropdownDepartemenDanJabatanAsync(model.DepartemenId, model.JabatanId);
            return View(model);
        }

        TempData["PesanSukses"] = pesan;
        return RedirectToAction(nameof(Indikator));
    }

    // GET: /Kinerja/UbahIndikator/{id}
    [Authorize(Roles = $"{NamaPeran.SuperAdmin},{NamaPeran.HRD}")]
    public async Task<IActionResult> UbahIndikator(Guid id)
    {
        var indikator = await _context.IndikatorKPI.FindAsync(id);
        if (indikator == null)
        {
            TempData["PesanError"] = "Indikator KPI tidak ditemukan.";
            return RedirectToAction(nameof(Indikator));
        }

        var model = new FormIndikatorKPIViewModel
        {
            Id = indikator.Id,
            DepartemenId = indikator.DepartemenId,
            JabatanId = indikator.JabatanId,
            KodeIndikator = indikator.KodeIndikator,
            NamaIndikator = indikator.NamaIndikator,
            Deskripsi = indikator.Deskripsi,
            Jenis = indikator.Jenis,
            TargetKuantitatif = indikator.TargetKuantitatif,
            Satuan = indikator.Satuan,
            BobotBawaan = indikator.BobotBawaan,
            StatusAktif = indikator.StatusAktif
        };

        await SiapkanDropdownDepartemenDanJabatanAsync(model.DepartemenId, model.JabatanId);
        return View(model);
    }

    // POST: /Kinerja/UbahIndikator/{id}
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = $"{NamaPeran.SuperAdmin},{NamaPeran.HRD}")]
    public async Task<IActionResult> UbahIndikator(FormIndikatorKPIViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await SiapkanDropdownDepartemenDanJabatanAsync(model.DepartemenId, model.JabatanId);
            return View(model);
        }

        var (sukses, pesan, _) = await _layananKinerja.SimpanIndikatorAsync(model);
        if (!sukses)
        {
            TempData["PesanError"] = pesan;
            await SiapkanDropdownDepartemenDanJabatanAsync(model.DepartemenId, model.JabatanId);
            return View(model);
        }

        TempData["PesanSukses"] = pesan;
        return RedirectToAction(nameof(Indikator));
    }

    // POST: /Kinerja/HapusIndikator/{id}
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = $"{NamaPeran.SuperAdmin},{NamaPeran.HRD}")]
    public async Task<IActionResult> HapusIndikator(Guid id)
    {
        var (sukses, pesan) = await _layananKinerja.HapusIndikatorAsync(id);
        if (sukses) TempData["PesanSukses"] = pesan;
        else TempData["PesanError"] = pesan;

        return RedirectToAction(nameof(Indikator));
    }
    #endregion

    #region Penilaian Kinerja
    // GET: /Kinerja/Penilaian
    [Authorize(Roles = $"{NamaPeran.SuperAdmin},{NamaPeran.HRD},{NamaPeran.Manager}")]
    public async Task<IActionResult> Penilaian(Guid? periodeId, Guid? karyawanId)
    {
        ViewBag.PeriodeIdTerpilih = periodeId;
        ViewBag.KaryawanIdTerpilih = karyawanId;

        var daftarPeriode = await _layananKinerja.DapatkanSemuaPeriodeAsync();
        ViewBag.DaftarPeriode = new SelectList(daftarPeriode, "Id", "NamaPeriode", periodeId);

        var daftarKaryawan = await _context.Karyawan
            .Where(k => k.StatusKaryawan == StatusKaryawan.Tetap || k.StatusKaryawan == StatusKaryawan.Kontrak)
            .OrderBy(k => k.NamaLengkap)
            .Select(k => new { k.Id, Nama = $"{k.NomorIndukKaryawan} - {k.NamaLengkap}" })
            .ToListAsync();
        ViewBag.DaftarKaryawan = new SelectList(daftarKaryawan, "Id", "Nama", karyawanId);

        var daftar = await _layananKinerja.DapatkanSemuaPenilaianAsync(periodeId, karyawanId);
        return View(daftar);
    }

    // GET: /Kinerja/InputPenilaian?periodeId={id}&karyawanId={id}
    [Authorize(Roles = $"{NamaPeran.SuperAdmin},{NamaPeran.HRD},{NamaPeran.Manager}")]
    public async Task<IActionResult> InputPenilaian(Guid? periodeId, Guid? karyawanId)
    {
        if (!periodeId.HasValue || !karyawanId.HasValue)
        {
            var daftarPeriode = await _layananKinerja.DapatkanSemuaPeriodeAsync();
            var periodeAktif = daftarPeriode.FirstOrDefault(p => p.Status == StatusPeriodePenilaian.Aktif) ?? daftarPeriode.FirstOrDefault();

            var daftarKaryawan = await _context.Karyawan
                .Where(k => k.StatusKaryawan == StatusKaryawan.Tetap || k.StatusKaryawan == StatusKaryawan.Kontrak)
                .OrderBy(k => k.NamaLengkap)
                .Select(k => new { k.Id, Nama = $"{k.NomorIndukKaryawan} - {k.NamaLengkap}" })
                .ToListAsync();

            ViewBag.DaftarPeriode = new SelectList(daftarPeriode, "Id", "NamaPeriode", periodeAktif?.Id);
            ViewBag.DaftarKaryawan = new SelectList(daftarKaryawan, "Id", "Nama");
            return View("PilihKaryawanPenilaian");
        }

        var model = await _layananKinerja.SiapkanFormPenilaianAsync(periodeId.Value, karyawanId.Value);
        if (model == null)
        {
            TempData["PesanError"] = "Data karyawan atau periode penilaian tidak valid.";
            return RedirectToAction(nameof(Penilaian));
        }

        if (!model.DaftarDetail.Any())
        {
            TempData["PesanError"] = "Belum ada indikator KPI aktif yang dikonfigurasikan. Buat indikator KPI terlebih dahulu.";
            return RedirectToAction(nameof(Indikator));
        }

        return View(model);
    }

    // POST: /Kinerja/InputPenilaian
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = $"{NamaPeran.SuperAdmin},{NamaPeran.HRD},{NamaPeran.Manager}")]
    public async Task<IActionResult> InputPenilaian(FormInputPenilaianKinerjaViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var userId = DapatkanUserIdPenggunaAktif();
        if (!userId.HasValue)
        {
            TempData["PesanError"] = "Sesi autentikasi tidak valid.";
            return RedirectToAction(nameof(Penilaian));
        }

        var namaPenilai = User.Identity?.Name ?? "Penilai Manager";
        var (sukses, pesan, id) = await _layananKinerja.SimpanPenilaianAsync(model, userId.Value, namaPenilai);

        if (!sukses)
        {
            TempData["PesanError"] = pesan;
            return View(model);
        }

        TempData["PesanSukses"] = pesan;
        return RedirectToAction(nameof(DetailPenilaian), new { id = id!.Value });
    }

    // GET: /Kinerja/DetailPenilaian/{id}
    [Authorize]
    public async Task<IActionResult> DetailPenilaian(Guid id)
    {
        var model = await _layananKinerja.DapatkanDetailPenilaianAsync(id);
        if (model == null)
        {
            TempData["PesanError"] = "Lembar penilaian kinerja tidak ditemukan.";
            return RedirectToAction(nameof(Index));
        }

        // Jika karyawan ESS, pastikan hanya bisa membuka penilaian miliknya sendiri
        if (User.IsInRole(NamaPeran.Karyawan) &&
            !User.IsInRole(NamaPeran.SuperAdmin) &&
            !User.IsInRole(NamaPeran.HRD) &&
            !User.IsInRole(NamaPeran.Manager))
        {
            var userId = DapatkanUserIdPenggunaAktif();
            var kry = await _context.Karyawan.FirstOrDefaultAsync(k => k.PenggunaId == userId);
            if (kry == null || kry.Id != model.KaryawanId)
            {
                TempData["PesanError"] = "Anda tidak memiliki wewenang untuk melihat penilaian kinerja karyawan lain.";
                return RedirectToAction(nameof(RiwayatSaya));
            }
        }

        return View(model);
    }

    // GET: /Kinerja/Laporan
    [Authorize(Roles = $"{NamaPeran.SuperAdmin},{NamaPeran.HRD},{NamaPeran.Manager}")]
    public async Task<IActionResult> Laporan(Guid? karyawanId, Guid? departemenId, int? tahun)
    {
        ViewBag.KaryawanIdTerpilih = karyawanId;
        ViewBag.DepartemenIdTerpilih = departemenId;
        ViewBag.TahunTerpilih = tahun ?? DateTime.Today.Year;

        var daftarDept = await _layananDepartemen.DapatkanSemuaAsync();
        ViewBag.DaftarDepartemen = new SelectList(daftarDept, "Id", "NamaDepartemen", departemenId);

        var daftarKaryawan = await _context.Karyawan
            .Where(k => k.StatusKaryawan == StatusKaryawan.Tetap || k.StatusKaryawan == StatusKaryawan.Kontrak)
            .OrderBy(k => k.NamaLengkap)
            .Select(k => new { k.Id, Nama = $"{k.NomorIndukKaryawan} - {k.NamaLengkap}" })
            .ToListAsync();
        ViewBag.DaftarKaryawan = new SelectList(daftarKaryawan, "Id", "Nama", karyawanId);

        var model = await _layananKinerja.DapatkanLaporanPerformaAsync(karyawanId, departemenId, tahun);
        return View(model);
    }

    // GET: /Kinerja/RiwayatSaya (ESS Karyawan)
    [Authorize]
    public async Task<IActionResult> RiwayatSaya()
    {
        var userId = DapatkanUserIdPenggunaAktif();
        if (!userId.HasValue) return RedirectToAction("Index", "Dasbor");

        var kry = await _context.Karyawan.FirstOrDefaultAsync(k => k.PenggunaId == userId.Value);
        if (kry == null)
        {
            TempData["PesanError"] = "Profil karyawan tidak ditemukan.";
            return RedirectToAction("Index", "Dasbor");
        }

        var daftar = await _layananKinerja.DapatkanSemuaPenilaianAsync(null, kry.Id);
        ViewBag.NamaKaryawan = kry.NamaLengkap;
        return View(daftar);
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
