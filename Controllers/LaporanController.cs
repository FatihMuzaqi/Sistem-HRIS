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

[Authorize(Roles = $"{NamaPeran.SuperAdmin},{NamaPeran.HRD},{NamaPeran.Manager},{NamaPeran.Finance}")]
public class LaporanController : Controller
{
    private readonly ILayananLaporanPusat _layananLaporan;
    private readonly ILayananDepartemen _layananDepartemen;
    private readonly KonteksAplikasi _context;

    public LaporanController(
        ILayananLaporanPusat layananLaporan,
        ILayananDepartemen layananDepartemen,
        KonteksAplikasi context)
    {
        _layananLaporan = layananLaporan;
        _layananDepartemen = layananDepartemen;
        _context = context;
    }

    #region Hub Eksekutif Laporan
    // GET: /Laporan
    public async Task<IActionResult> Index()
    {
        var model = await _layananLaporan.DapatkanRingkasanEksekutifAsync();
        return View(model);
    }
    #endregion

    #region 1. Laporan Karyawan & Departemen
    // GET: /Laporan/Karyawan
    public async Task<IActionResult> Karyawan(Guid? departemenId, StatusKaryawan? status, string? kataKunci, int halaman = 1)
    {
        await SiapkanDropdownDepartemenAsync(departemenId);
        ViewBag.StatusTerpilih = status;
        ViewBag.KataKunci = kataKunci;

        var model = await _layananLaporan.DapatkanLaporanKaryawanAsync(departemenId, status, kataKunci, halaman, 15);
        return View(model);
    }

    // GET: /Laporan/EksporKaryawan
    public async Task<IActionResult> EksporKaryawan(Guid? departemenId, StatusKaryawan? status, string? kataKunci)
    {
        // Ambil data tanpa limit halaman untuk ekspor lengkap
        var model = await _layananLaporan.DapatkanLaporanKaryawanAsync(departemenId, status, kataKunci, 1, 10000);
        var bytes = _layananLaporan.EksporKaryawanCsv(model.DataKaryawan);
        var fileName = $"Laporan_Karyawan_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
        return File(bytes, "text/csv; charset=utf-8", fileName);
    }

    // GET: /Laporan/Departemen
    public async Task<IActionResult> Departemen()
    {
        var model = await _layananLaporan.DapatkanLaporanDepartemenAsync();
        return View(model);
    }
    #endregion

    #region 2. Kehadiran, Keterlambatan & Lembur
    // GET: /Laporan/Kehadiran
    public async Task<IActionResult> Kehadiran(DateTime? tanggalMulai, DateTime? tanggalSelesai, Guid? departemenId, int halaman = 1)
    {
        var tMulai = tanggalMulai ?? DateTime.Today.AddDays(-30);
        var tSelesai = tanggalSelesai ?? DateTime.Today;

        await SiapkanDropdownDepartemenAsync(departemenId);
        ViewBag.TanggalMulai = tMulai.ToString("yyyy-MM-dd");
        ViewBag.TanggalSelesai = tSelesai.ToString("yyyy-MM-dd");

        var model = await _layananLaporan.DapatkanLaporanKehadiranAsync(tMulai, tSelesai, departemenId, halaman, 15);
        return View(model);
    }

    // GET: /Laporan/EksporKehadiran
    public async Task<IActionResult> EksporKehadiran(DateTime? tanggalMulai, DateTime? tanggalSelesai, Guid? departemenId)
    {
        var tMulai = tanggalMulai ?? DateTime.Today.AddDays(-30);
        var tSelesai = tanggalSelesai ?? DateTime.Today;

        var model = await _layananLaporan.DapatkanLaporanKehadiranAsync(tMulai, tSelesai, departemenId, 1, 10000);
        var bytes = _layananLaporan.EksporKehadiranCsv(model.DataKehadiran);
        var fileName = $"Laporan_Kehadiran_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
        return File(bytes, "text/csv; charset=utf-8", fileName);
    }

    // GET: /Laporan/Keterlambatan
    public async Task<IActionResult> Keterlambatan(DateTime? tanggalMulai, DateTime? tanggalSelesai, Guid? departemenId, int halaman = 1)
    {
        var tMulai = tanggalMulai ?? DateTime.Today.AddDays(-30);
        var tSelesai = tanggalSelesai ?? DateTime.Today;

        await SiapkanDropdownDepartemenAsync(departemenId);
        ViewBag.TanggalMulai = tMulai.ToString("yyyy-MM-dd");
        ViewBag.TanggalSelesai = tSelesai.ToString("yyyy-MM-dd");

        var model = await _layananLaporan.DapatkanLaporanKeterlambatanAsync(tMulai, tSelesai, departemenId, halaman, 15);
        return View(model);
    }

    // GET: /Laporan/Lembur
    public async Task<IActionResult> Lembur(DateTime? tanggalMulai, DateTime? tanggalSelesai, Guid? departemenId, int halaman = 1)
    {
        var tMulai = tanggalMulai ?? DateTime.Today.AddDays(-30);
        var tSelesai = tanggalSelesai ?? DateTime.Today;

        await SiapkanDropdownDepartemenAsync(departemenId);
        ViewBag.TanggalMulai = tMulai.ToString("yyyy-MM-dd");
        ViewBag.TanggalSelesai = tSelesai.ToString("yyyy-MM-dd");

        var model = await _layananLaporan.DapatkanLaporanLemburAsync(tMulai, tSelesai, departemenId, halaman, 15);
        return View(model);
    }
    #endregion

    #region 3. Cuti
    // GET: /Laporan/Cuti
    public async Task<IActionResult> Cuti(int? tahun, Guid? departemenId, Guid? jenisCutiId, int halaman = 1)
    {
        var thn = tahun ?? DateTime.Today.Year;
        await SiapkanDropdownDepartemenAsync(departemenId);
        await SiapkanDropdownJenisCutiAsync(jenisCutiId);
        ViewBag.Tahun = thn;

        var model = await _layananLaporan.DapatkanLaporanCutiAsync(thn, departemenId, jenisCutiId, halaman, 15);
        return View(model);
    }
    #endregion

    #region 4. Penggajian
    // GET: /Laporan/Penggajian
    public async Task<IActionResult> Penggajian(int? tahun, int? bulan, Guid? departemenId, int halaman = 1)
    {
        var thn = tahun ?? DateTime.Today.Year;
        await SiapkanDropdownDepartemenAsync(departemenId);
        ViewBag.Tahun = thn;
        ViewBag.Bulan = bulan;

        var model = await _layananLaporan.DapatkanLaporanPenggajianAsync(thn, bulan, departemenId, halaman, 15);
        return View(model);
    }

    // GET: /Laporan/EksporPenggajian
    public async Task<IActionResult> EksporPenggajian(int? tahun, int? bulan, Guid? departemenId)
    {
        var thn = tahun ?? DateTime.Today.Year;
        var model = await _layananLaporan.DapatkanLaporanPenggajianAsync(thn, bulan, departemenId, 1, 10000);
        var bytes = _layananLaporan.EksporPenggajianCsv(model.DataPenggajian);
        var fileName = $"Laporan_Penggajian_{thn}_{bulan?.ToString("00") ?? "Semua"}_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
        return File(bytes, "text/csv; charset=utf-8", fileName);
    }
    #endregion

    #region 5. Rekrutmen
    // GET: /Laporan/Rekrutmen
    public async Task<IActionResult> Rekrutmen(DateTime? tanggalMulai, DateTime? tanggalSelesai, Guid? lowonganId, int halaman = 1)
    {
        var daftarLowongan = await _context.LowonganKerja
            .Where(l => !l.IsDeleted)
            .OrderByDescending(l => l.CreatedAt)
            .Select(l => new { l.Id, l.JudulLowongan })
            .ToListAsync();
        ViewBag.DaftarLowongan = new SelectList(daftarLowongan, "Id", "JudulLowongan", lowonganId);

        var model = await _layananLaporan.DapatkanLaporanRekrutmenAsync(tanggalMulai, tanggalSelesai, lowonganId, halaman, 15);
        return View(model);
    }
    #endregion

    #region 6. Kinerja
    // GET: /Laporan/Kinerja
    public async Task<IActionResult> Kinerja(Guid? periodeId, Guid? departemenId, int halaman = 1)
    {
        var daftarPeriode = await _context.PeriodePenilaian
            .Where(p => !p.IsDeleted)
            .OrderByDescending(p => p.Tahun)
            .Select(p => new { p.Id, p.NamaPeriode })
            .ToListAsync();
        ViewBag.DaftarPeriode = new SelectList(daftarPeriode, "Id", "NamaPeriode", periodeId);
        await SiapkanDropdownDepartemenAsync(departemenId);

        var model = await _layananLaporan.DapatkanLaporanKinerjaAsync(periodeId, departemenId, halaman, 15);
        return View(model);
    }
    #endregion

    #region 7. Pelatihan
    // GET: /Laporan/Pelatihan
    public async Task<IActionResult> Pelatihan(DateTime? tanggalMulai, DateTime? tanggalSelesai, Guid? departemenId, int halaman = 1)
    {
        await SiapkanDropdownDepartemenAsync(departemenId);
        var model = await _layananLaporan.DapatkanLaporanPelatihanAsync(tanggalMulai, tanggalSelesai, departemenId, halaman, 15);
        return View(model);
    }
    #endregion

    #region 8. Aset
    // GET: /Laporan/Aset
    public async Task<IActionResult> Aset(KategoriAset? kategori, StatusAset? status, KondisiAset? kondisi, int halaman = 1)
    {
        ViewBag.KategoriTerpilih = kategori;
        ViewBag.StatusTerpilih = status;
        ViewBag.KondisiTerpilih = kondisi;

        var model = await _layananLaporan.DapatkanLaporanAsetAsync(kategori, status, kondisi, halaman, 15);
        return View(model);
    }

    // GET: /Laporan/EksporAset
    public async Task<IActionResult> EksporAset(KategoriAset? kategori, StatusAset? status, KondisiAset? kondisi)
    {
        var model = await _layananLaporan.DapatkanLaporanAsetAsync(kategori, status, kondisi, 1, 10000);
        var bytes = _layananLaporan.EksporAsetCsv(model.DataAset);
        var fileName = $"Laporan_Aset_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
        return File(bytes, "text/csv; charset=utf-8", fileName);
    }
    #endregion

    #region Helpers
    private async Task SiapkanDropdownDepartemenAsync(Guid? departemenIdTerpilih)
    {
        var daftarDept = await _layananDepartemen.DapatkanSemuaAsync();
        ViewBag.DaftarDepartemen = new SelectList(daftarDept, "Id", "NamaDepartemen", departemenIdTerpilih);
    }

    private async Task SiapkanDropdownJenisCutiAsync(Guid? jenisCutiIdTerpilih)
    {
        var daftarJenis = await _context.JenisCuti
            .Where(j => !j.IsDeleted)
            .OrderBy(j => j.NamaJenisCuti)
            .Select(j => new { j.Id, j.NamaJenisCuti })
            .ToListAsync();
        ViewBag.DaftarJenisCuti = new SelectList(daftarJenis, "Id", "NamaJenisCuti", jenisCutiIdTerpilih);
    }
    #endregion
}
