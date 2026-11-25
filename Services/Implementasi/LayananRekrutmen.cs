using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using SistemHRD.Data;
using SistemHRD.Models.Entitas;
using SistemHRD.Models.Enums;
using SistemHRD.Services.Interfaces;
using SistemHRD.ViewModels;

namespace SistemHRD.Services.Implementasi;

public class LayananRekrutmen : ILayananRekrutmen
{
    private readonly KonteksAplikasi _context;
    private readonly IWebHostEnvironment _webHostEnvironment;
    private readonly ILayananAudit _layananAudit;
    private readonly ILayananTenant _layananTenant;

    public LayananRekrutmen(
        KonteksAplikasi context,
        IWebHostEnvironment webHostEnvironment,
        ILayananAudit layananAudit,
        ILayananTenant layananTenant)
    {
        _context = context;
        _webHostEnvironment = webHostEnvironment;
        _layananAudit = layananAudit;
        _layananTenant = layananTenant;
    }

    #region Dashboard Rekrutmen
    public async Task<DashboardRekrutmenViewModel> DapatkanDashboardAsync()
    {
        var lowonganAktif = await _context.LowonganKerja
            .Where(l => l.Status == StatusLowongan.Dibuka)
            .CountAsync();

        var pelamarList = await _context.Pelamar
            .Include(p => p.LowonganKerja)
            .ToListAsync();

        var totalPelamarBaru = pelamarList.Count(p => p.Status == StatusPelamar.Baru);
        var totalDalamProses = pelamarList.Count(p =>
            p.Status == StatusPelamar.SeleksiAdministrasi ||
            p.Status == StatusPelamar.LolosAdministrasi ||
            p.Status == StatusPelamar.Wawancara ||
            p.Status == StatusPelamar.PenawaranKerja);
        var totalDiterima = pelamarList.Count(p => p.Status == StatusPelamar.Diterima);

        // Statistik Status Pelamar untuk Chart.js (Doughnut)
        var statStatus = new Dictionary<string, int>
        {
            ["Baru"] = totalPelamarBaru,
            ["Administrasi"] = pelamarList.Count(p => p.Status == StatusPelamar.SeleksiAdministrasi || p.Status == StatusPelamar.LolosAdministrasi),
            ["Wawancara"] = pelamarList.Count(p => p.Status == StatusPelamar.Wawancara),
            ["Offering"] = pelamarList.Count(p => p.Status == StatusPelamar.PenawaranKerja),
            ["Diterima"] = totalDiterima,
            ["Ditolak"] = pelamarList.Count(p => p.Status == StatusPelamar.Ditolak)
        };

        // Statistik Pelamar per Lowongan untuk Chart.js (Bar)
        var statLowongan = pelamarList
            .GroupBy(p => p.LowonganKerja?.JudulLowongan ?? "Lainnya")
            .Take(7)
            .ToDictionary(g => g.Key, g => g.Count());

        // Lowongan Terbaru
        var lowonganTerbaru = await _context.LowonganKerja
            .Include(l => l.Departemen)
            .Include(l => l.Jabatan)
            .Include(l => l.DaftarPelamar)
            .OrderByDescending(l => l.CreatedAt)
            .Take(5)
            .Select(l => new LowonganKerjaItemViewModel
            {
                Id = l.Id,
                KodeLowongan = l.KodeLowongan,
                JudulLowongan = l.JudulLowongan,
                NamaDepartemen = l.Departemen != null ? l.Departemen.NamaDepartemen : "-",
                NamaJabatan = l.Jabatan != null ? l.Jabatan.NamaJabatan : "-",
                TipePekerjaan = l.TipePekerjaan,
                LokasiKerja = l.LokasiKerja,
                JumlahDibutuhkan = l.JumlahDibutuhkan,
                JumlahPelamar = l.DaftarPelamar.Count,
                GajiMin = l.GajiMin,
                GajiMax = l.GajiMax,
                TanggalDibuka = l.TanggalDibuka,
                TanggalDitutup = l.TanggalDitutup,
                Status = l.Status
            })
            .ToListAsync();

        // Wawancara Mendatang
        var wawancaraMendatang = await _context.JadwalWawancara
            .Include(j => j.Pelamar!)
                .ThenInclude(p => p.LowonganKerja)
            .Include(j => j.TahapanSeleksi)
            .Where(j => j.Status == StatusWawancara.Dijadwalkan && j.TanggalWawancara >= DateTime.Today)
            .OrderBy(j => j.TanggalWawancara)
            .ThenBy(j => j.JamMulai)
            .Take(5)
            .Select(j => new JadwalWawancaraItemViewModel
            {
                Id = j.Id,
                PelamarId = j.PelamarId,
                NamaPelamar = j.Pelamar != null ? j.Pelamar.NamaLengkap : "-",
                JudulLowongan = j.Pelamar != null && j.Pelamar.LowonganKerja != null ? j.Pelamar.LowonganKerja.JudulLowongan : "-",
                NamaTahapan = j.TahapanSeleksi != null ? j.TahapanSeleksi.NamaTahapan : "Wawancara",
                NamaPewawancara = j.NamaPewawancara,
                TanggalWawancara = j.TanggalWawancara,
                JamMulai = j.JamMulai,
                JamSelesai = j.JamSelesai,
                Metode = j.Metode,
                LokasiAtauTautan = j.LokasiAtauTautan,
                Status = j.Status,
                CatatanUndangan = j.CatatanUndangan
            })
            .ToListAsync();

        return new DashboardRekrutmenViewModel
        {
            TotalLowonganAktif = lowonganAktif,
            TotalPelamarBaru = totalPelamarBaru,
            TotalDalamProses = totalDalamProses,
            TotalDiterima = totalDiterima,
            StatistikStatusPelamar = statStatus,
            StatistikPelamarPerLowongan = statLowongan,
            LowonganTerbaru = lowonganTerbaru,
            WawancaraMendatang = wawancaraMendatang
        };
    }
    #endregion

    #region Lowongan Kerja
    public async Task<List<LowonganKerjaItemViewModel>> DapatkanSemuaLowonganAsync(StatusLowongan? status = null)
    {
        var query = _context.LowonganKerja
            .Include(l => l.Departemen)
            .Include(l => l.Jabatan)
            .Include(l => l.DaftarPelamar)
            .AsQueryable();

        if (status.HasValue)
        {
            query = query.Where(l => l.Status == status.Value);
        }

        return await query
            .OrderByDescending(l => l.CreatedAt)
            .Select(l => new LowonganKerjaItemViewModel
            {
                Id = l.Id,
                KodeLowongan = l.KodeLowongan,
                JudulLowongan = l.JudulLowongan,
                NamaDepartemen = l.Departemen != null ? l.Departemen.NamaDepartemen : "-",
                NamaJabatan = l.Jabatan != null ? l.Jabatan.NamaJabatan : "-",
                TipePekerjaan = l.TipePekerjaan,
                LokasiKerja = l.LokasiKerja,
                JumlahDibutuhkan = l.JumlahDibutuhkan,
                JumlahPelamar = l.DaftarPelamar.Count,
                GajiMin = l.GajiMin,
                GajiMax = l.GajiMax,
                TanggalDibuka = l.TanggalDibuka,
                TanggalDitutup = l.TanggalDitutup,
                Status = l.Status
            })
            .ToListAsync();
    }

    public async Task<DetailLowonganKerjaViewModel?> DapatkanDetailLowonganAsync(Guid id)
    {
        var l = await _context.LowonganKerja
            .Include(x => x.Departemen)
            .Include(x => x.Jabatan)
            .Include(x => x.DaftarTahapanSeleksi)
            .Include(x => x.DaftarPelamar)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (l == null) return null;

        return new DetailLowonganKerjaViewModel
        {
            Id = l.Id,
            KodeLowongan = l.KodeLowongan,
            JudulLowongan = l.JudulLowongan,
            NamaDepartemen = l.Departemen?.NamaDepartemen ?? "-",
            NamaJabatan = l.Jabatan?.NamaJabatan ?? "-",
            DeskripsiPekerjaan = l.DeskripsiPekerjaan,
            Kualifikasi = l.Kualifikasi,
            TipePekerjaan = l.TipePekerjaan,
            LokasiKerja = l.LokasiKerja,
            JumlahDibutuhkan = l.JumlahDibutuhkan,
            GajiMin = l.GajiMin,
            GajiMax = l.GajiMax,
            TanggalDibuka = l.TanggalDibuka,
            TanggalDitutup = l.TanggalDitutup,
            Status = l.Status,
            DaftarTahapan = l.DaftarTahapanSeleksi
                .OrderBy(t => t.Urutan)
                .Select(t => new TahapanSeleksiItemViewModel
                {
                    Id = t.Id,
                    NamaTahapan = t.NamaTahapan,
                    JenisTahapan = t.JenisTahapan,
                    Urutan = t.Urutan,
                    Keterangan = t.Keterangan,
                    StatusAktif = t.StatusAktif
                })
                .ToList(),
            DaftarPelamar = l.DaftarPelamar
                .OrderByDescending(p => p.CreatedAt)
                .Select(p => new PelamarItemViewModel
                {
                    Id = p.Id,
                    LowonganKerjaId = p.LowonganKerjaId,
                    NomorRegistrasi = p.NomorRegistrasi,
                    JudulLowongan = l.JudulLowongan,
                    NamaLengkap = p.NamaLengkap,
                    Email = p.Email,
                    NomorTelepon = p.NomorTelepon,
                    PendidikanTerakhir = p.PendidikanTerakhir,
                    NamaInstitusi = p.NamaInstitusi,
                    PengalamanKerjaTahun = p.PengalamanKerjaTahun,
                    GajiDiharapkan = p.GajiDiharapkan,
                    NamaFileCV = p.NamaFileCV,
                    Status = p.Status,
                    CreatedAt = p.CreatedAt
                })
                .ToList()
        };
    }

    public async Task<(bool Sukses, string Pesan, Guid? Id)> SimpanLowonganAsync(FormLowonganKerjaViewModel model)
    {
        var tenantId = _layananTenant.PerusahaanIdSaatIni ?? Guid.Empty;

        // Cek duplikasi kode lowongan
        var duplikat = await _context.LowonganKerja
            .AnyAsync(l => l.KodeLowongan == model.KodeLowongan && (!model.Id.HasValue || l.Id != model.Id.Value));

        if (duplikat)
        {
            return (false, $"Kode lowongan '{model.KodeLowongan}' sudah digunakan oleh lowongan lain.", null);
        }

        LowonganKerja entity;
        bool isNew = !model.Id.HasValue;

        if (isNew)
        {
            entity = new LowonganKerja
            {
                Id = Guid.NewGuid(),
                PerusahaanId = tenantId
            };
            await _context.LowonganKerja.AddAsync(entity);

            // Tambahkan tahapan seleksi bawaan standar otomatis
            var defaultTahapan = new (string Nama, JenisTahapanSeleksi Jenis, int Urutan)[]
            {
                ("Seleksi Administrasi & Berkas", JenisTahapanSeleksi.Administrasi, 1),
                ("Wawancara Awal HRD", JenisTahapanSeleksi.WawancaraHR, 2),
                ("Wawancara Teknis / User", JenisTahapanSeleksi.WawancaraUser, 3),
                ("Penawaran Kerja (Offering)", JenisTahapanSeleksi.Offering, 4)
            };

            foreach (var t in defaultTahapan)
            {
                entity.DaftarTahapanSeleksi.Add(new TahapanSeleksi
                {
                    Id = Guid.NewGuid(),
                    PerusahaanId = tenantId,
                    LowonganKerjaId = entity.Id,
                    NamaTahapan = t.Nama,
                    JenisTahapan = t.Jenis,
                    Urutan = t.Urutan,
                    StatusAktif = true
                });
            }
        }
        else
        {
            var existing = await _context.LowonganKerja.FindAsync(model.Id!.Value);
            if (existing == null) return (false, "Data lowongan kerja tidak ditemukan.", null);
            entity = existing;
        }

        entity.KodeLowongan = model.KodeLowongan.Trim().ToUpper();
        entity.JudulLowongan = model.JudulLowongan.Trim();
        entity.DepartemenId = model.DepartemenId;
        entity.JabatanId = model.JabatanId;
        entity.DeskripsiPekerjaan = model.DeskripsiPekerjaan.Trim();
        entity.Kualifikasi = model.Kualifikasi.Trim();
        entity.TipePekerjaan = model.TipePekerjaan;
        entity.LokasiKerja = model.LokasiKerja.Trim();
        entity.JumlahDibutuhkan = model.JumlahDibutuhkan;
        entity.GajiMin = model.GajiMin;
        entity.GajiMax = model.GajiMax;
        entity.TanggalDibuka = model.TanggalDibuka;
        entity.TanggalDitutup = model.TanggalDitutup;
        entity.Status = model.Status;

        await _context.SaveChangesAsync();

        await _layananAudit.CatatAsync(
            isNew ? "BUAT_LOWONGAN" : "UBAH_LOWONGAN",
            "LowonganKerja",
            $"{(isNew ? "Membuat" : "Memperbarui")} lowongan kerja: {entity.KodeLowongan} - {entity.JudulLowongan}");

        return (true, $"Lowongan kerja '{entity.JudulLowongan}' berhasil disimpan.", entity.Id);
    }

    public async Task<(bool Sukses, string Pesan)> HapusLowonganAsync(Guid id)
    {
        var lowongan = await _context.LowonganKerja
            .Include(l => l.DaftarPelamar)
            .FirstOrDefaultAsync(l => l.Id == id);

        if (lowongan == null) return (false, "Data lowongan kerja tidak ditemukan.");

        if (lowongan.DaftarPelamar.Any(p => p.Status != StatusPelamar.Baru && p.Status != StatusPelamar.Ditolak))
        {
            return (false, "Lowongan tidak dapat dihapus karena memiliki pelamar yang sedang dalam proses seleksi aktif.");
        }

        _context.LowonganKerja.Remove(lowongan);
        await _context.SaveChangesAsync();

        await _layananAudit.CatatAsync("HAPUS_LOWONGAN", "LowonganKerja", $"Menghapus lowongan kerja {lowongan.KodeLowongan} - {lowongan.JudulLowongan}");
        return (true, "Lowongan kerja berhasil dihapus.");
    }
    #endregion

    #region Pelamar
    public async Task<List<PelamarItemViewModel>> DapatkanSemuaPelamarAsync(Guid? lowonganId = null, StatusPelamar? status = null)
    {
        var query = _context.Pelamar
            .Include(p => p.LowonganKerja)
            .AsQueryable();

        if (lowonganId.HasValue) query = query.Where(p => p.LowonganKerjaId == lowonganId.Value);
        if (status.HasValue) query = query.Where(p => p.Status == status.Value);

        return await query
            .OrderByDescending(p => p.CreatedAt)
            .Select(p => new PelamarItemViewModel
            {
                Id = p.Id,
                LowonganKerjaId = p.LowonganKerjaId,
                NomorRegistrasi = p.NomorRegistrasi,
                JudulLowongan = p.LowonganKerja != null ? p.LowonganKerja.JudulLowongan : "-",
                NamaLengkap = p.NamaLengkap,
                Email = p.Email,
                NomorTelepon = p.NomorTelepon,
                PendidikanTerakhir = p.PendidikanTerakhir,
                NamaInstitusi = p.NamaInstitusi,
                PengalamanKerjaTahun = p.PengalamanKerjaTahun,
                GajiDiharapkan = p.GajiDiharapkan,
                NamaFileCV = p.NamaFileCV,
                Status = p.Status,
                CreatedAt = p.CreatedAt
            })
            .ToListAsync();
    }

    public async Task<DetailPelamarViewModel?> DapatkanDetailPelamarAsync(Guid id)
    {
        var p = await _context.Pelamar
            .Include(x => x.LowonganKerja!)
                .ThenInclude(l => l.DaftarTahapanSeleksi)
            .Include(x => x.Karyawan)
            .Include(x => x.DaftarJadwalWawancara!)
                .ThenInclude(j => j.TahapanSeleksi)
            .Include(x => x.DaftarHasilSeleksi!)
                .ThenInclude(h => h.TahapanSeleksi)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (p == null) return null;

        return new DetailPelamarViewModel
        {
            Id = p.Id,
            LowonganKerjaId = p.LowonganKerjaId,
            JudulLowongan = p.LowonganKerja?.JudulLowongan ?? "-",
            NomorRegistrasi = p.NomorRegistrasi,
            NamaLengkap = p.NamaLengkap,
            Email = p.Email,
            NomorTelepon = p.NomorTelepon,
            PendidikanTerakhir = p.PendidikanTerakhir,
            NamaInstitusi = p.NamaInstitusi,
            Jurusan = p.Jurusan,
            PengalamanKerjaTahun = p.PengalamanKerjaTahun,
            GajiDiharapkan = p.GajiDiharapkan,
            NamaFileCV = p.NamaFileCV,
            JalurFileCV = p.JalurFileCV,
            CatatanAdministrasi = p.CatatanAdministrasi,
            Status = p.Status,
            CreatedAt = p.CreatedAt,
            KaryawanId = p.KaryawanId,
            NamaKaryawanHasilKonversi = p.Karyawan?.NamaLengkap,
            DaftarJadwalWawancara = p.DaftarJadwalWawancara
                .OrderBy(j => j.TanggalWawancara)
                .Select(j => new JadwalWawancaraItemViewModel
                {
                    Id = j.Id,
                    PelamarId = j.PelamarId,
                    NamaPelamar = p.NamaLengkap,
                    JudulLowongan = p.LowonganKerja?.JudulLowongan ?? "-",
                    NamaTahapan = j.TahapanSeleksi?.NamaTahapan ?? "Wawancara",
                    NamaPewawancara = j.NamaPewawancara,
                    TanggalWawancara = j.TanggalWawancara,
                    JamMulai = j.JamMulai,
                    JamSelesai = j.JamSelesai,
                    Metode = j.Metode,
                    LokasiAtauTautan = j.LokasiAtauTautan,
                    Status = j.Status,
                    CatatanUndangan = j.CatatanUndangan
                })
                .ToList(),
            DaftarHasilSeleksi = p.DaftarHasilSeleksi
                .OrderByDescending(h => h.TanggalPenilaian)
                .Select(h => new HasilSeleksiItemViewModel
                {
                    Id = h.Id,
                    PelamarId = h.PelamarId,
                    NamaTahapan = h.TahapanSeleksi?.NamaTahapan ?? "Evaluasi",
                    NamaPenilai = h.NamaPenilai,
                    TanggalPenilaian = h.TanggalPenilaian,
                    Skor = h.Skor,
                    CatatanEvaluasi = h.CatatanEvaluasi,
                    Kelebihan = h.Kelebihan,
                    Kekurangan = h.Kekurangan,
                    Keputusan = h.Keputusan
                })
                .ToList(),
            DaftarTahapanLowongan = p.LowonganKerja?.DaftarTahapanSeleksi
                .OrderBy(t => t.Urutan)
                .Select(t => new TahapanSeleksiItemViewModel
                {
                    Id = t.Id,
                    NamaTahapan = t.NamaTahapan,
                    JenisTahapan = t.JenisTahapan,
                    Urutan = t.Urutan,
                    Keterangan = t.Keterangan,
                    StatusAktif = t.StatusAktif
                })
                .ToList() ?? new()
        };
    }

    public async Task<(bool Sukses, string Pesan, Guid? Id)> DaftarPelamarAsync(FormPelamarViewModel model)
    {
        var tenantId = _layananTenant.PerusahaanIdSaatIni ?? Guid.Empty;

        var lowongan = await _context.LowonganKerja.FindAsync(model.LowonganKerjaId);
        if (lowongan == null) return (false, "Lowongan kerja yang dipilih tidak valid.", null);

        if (model.BerkasCV == null || model.BerkasCV.Length == 0)
        {
            return (false, "File CV wajib diunggah.", null);
        }

        var ext = Path.GetExtension(model.BerkasCV.FileName).ToLowerInvariant();
        var allowedExt = new[] { ".pdf", ".doc", ".docx" };
        if (!allowedExt.Contains(ext))
        {
            return (false, "Format berkas CV tidak valid. Hanya berkas PDF, DOC, atau DOCX yang diperbolehkan.", null);
        }

        if (model.BerkasCV.Length > 5 * 1024 * 1024) // 5MB
        {
            return (false, "Ukuran berkas CV maksimal 5MB.", null);
        }

        // Simpan File CV
        var folderUpload = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "cv");
        if (!Directory.Exists(folderUpload)) Directory.CreateDirectory(folderUpload);

        var namaFileSimpan = $"CV_{DateTime.Now:yyyyMMddHHmmss}_{Guid.NewGuid().ToString()[..6]}{ext}";
        var pathSimpan = Path.Combine(folderUpload, namaFileSimpan);

        using (var stream = new FileStream(pathSimpan, FileMode.Create))
        {
            await model.BerkasCV.CopyToAsync(stream);
        }

        // Generate Nomor Registrasi
        var urutan = await _context.Pelamar.CountAsync() + 1;
        var nomorReg = $"PLM/{DateTime.Today:yyyyMM}/{urutan:D4}";

        var pelamar = new Pelamar
        {
            Id = Guid.NewGuid(),
            PerusahaanId = tenantId,
            LowonganKerjaId = model.LowonganKerjaId,
            NomorRegistrasi = nomorReg,
            NamaLengkap = model.NamaLengkap.Trim(),
            Email = model.Email.Trim().ToLower(),
            NomorTelepon = model.NomorTelepon.Trim(),
            PendidikanTerakhir = model.PendidikanTerakhir?.Trim(),
            NamaInstitusi = model.NamaInstitusi?.Trim(),
            Jurusan = model.Jurusan?.Trim(),
            PengalamanKerjaTahun = model.PengalamanKerjaTahun,
            GajiDiharapkan = model.GajiDiharapkan,
            NamaFileCV = model.BerkasCV.FileName,
            JalurFileCV = Path.Combine("uploads", "cv", namaFileSimpan),
            CatatanAdministrasi = model.CatatanAdministrasi,
            Status = StatusPelamar.Baru
        };

        await _context.Pelamar.AddAsync(pelamar);
        await _context.SaveChangesAsync();

        await _layananAudit.CatatAsync("DAFTAR_PELAMAR", "Pelamar", $"Mendaftarkan pelamar {pelamar.NomorRegistrasi} - {pelamar.NamaLengkap} pada lowongan {lowongan.JudulLowongan}");

        return (true, $"Pendaftaran pelamar '{pelamar.NamaLengkap}' berhasil dengan nomor registrasi {pelamar.NomorRegistrasi}.", pelamar.Id);
    }

    public async Task<(bool Sukses, string Pesan)> ProsesSeleksiAdministrasiAsync(FormSeleksiAdministrasiViewModel model)
    {
        var pelamar = await _context.Pelamar.FindAsync(model.PelamarId);
        if (pelamar == null) return (false, "Data pelamar tidak ditemukan.");

        pelamar.CatatanAdministrasi = model.CatatanAdministrasi;
        pelamar.Status = model.Lolos ? StatusPelamar.LolosAdministrasi : StatusPelamar.Ditolak;

        await _context.SaveChangesAsync();
        await _layananAudit.CatatAsync("SELEKSI_ADMINISTRASI", "Pelamar", $"Hasil seleksi berkas pelamar {pelamar.NamaLengkap}: {(model.Lolos ? "Lolos" : "Ditolak")}");

        return (true, $"Status seleksi berkas pelamar '{pelamar.NamaLengkap}' berhasil diperbarui menjadi {(model.Lolos ? "Lolos Administrasi" : "Ditolak")}.");
    }

    public async Task<(bool Sukses, string Pesan)> UbahStatusPelamarAsync(Guid pelamarId, StatusPelamar statusBaru, string? catatan = null)
    {
        var pelamar = await _context.Pelamar.FindAsync(pelamarId);
        if (pelamar == null) return (false, "Data pelamar tidak ditemukan.");

        var statusLama = pelamar.Status;
        pelamar.Status = statusBaru;
        if (!string.IsNullOrWhiteSpace(catatan))
        {
            pelamar.CatatanAdministrasi = (pelamar.CatatanAdministrasi != null ? pelamar.CatatanAdministrasi + " | " : "") + catatan;
        }

        await _context.SaveChangesAsync();
        await _layananAudit.CatatAsync("UBAH_STATUS_PELAMAR", "Pelamar", $"Mengubah status pelamar {pelamar.NamaLengkap} dari {statusLama} menjadi {statusBaru}");

        return (true, $"Status pelamar '{pelamar.NamaLengkap}' berhasil diubah menjadi {statusBaru}.");
    }

    public async Task<(byte[]? FileBytes, string ContentType, string FileName)> UnduhCVAsync(Guid pelamarId)
    {
        var pelamar = await _context.Pelamar.FindAsync(pelamarId);
        if (pelamar == null || string.IsNullOrWhiteSpace(pelamar.JalurFileCV))
        {
            return (null, string.Empty, string.Empty);
        }

        var fullPath = Path.Combine(_webHostEnvironment.WebRootPath, pelamar.JalurFileCV);
        if (!File.Exists(fullPath))
        {
            return (null, string.Empty, string.Empty);
        }

        var bytes = await File.ReadAllBytesAsync(fullPath);
        var ext = Path.GetExtension(fullPath).ToLowerInvariant();
        var contentType = ext switch
        {
            ".pdf" => "application/pdf",
            ".doc" => "application/msword",
            ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            _ => "application/octet-stream"
        };

        return (bytes, contentType, pelamar.NamaFileCV);
    }
    #endregion

    #region Wawancara & Hasil Seleksi
    public async Task<(bool Sukses, string Pesan, Guid? Id)> SimpanJadwalWawancaraAsync(FormJadwalWawancaraViewModel model)
    {
        var tenantId = _layananTenant.PerusahaanIdSaatIni ?? Guid.Empty;

        var pelamar = await _context.Pelamar
            .Include(p => p.LowonganKerja)
            .FirstOrDefaultAsync(p => p.Id == model.PelamarId);

        if (pelamar == null) return (false, "Data pelamar tidak ditemukan.", null);

        var jadwal = new JadwalWawancara
        {
            Id = Guid.NewGuid(),
            PerusahaanId = tenantId,
            PelamarId = model.PelamarId,
            TahapanSeleksiId = model.TahapanSeleksiId,
            PewawancaraId = model.PewawancaraId,
            NamaPewawancara = model.NamaPewawancara.Trim(),
            TanggalWawancara = model.TanggalWawancara,
            JamMulai = model.JamMulai,
            JamSelesai = model.JamSelesai,
            Metode = model.Metode,
            LokasiAtauTautan = model.LokasiAtauTautan.Trim(),
            Status = StatusWawancara.Dijadwalkan,
            CatatanUndangan = model.CatatanUndangan
        };

        await _context.JadwalWawancara.AddAsync(jadwal);

        // Update status pelamar menjadi Wawancara
        pelamar.Status = StatusPelamar.Wawancara;
        _context.Pelamar.Update(pelamar);

        await _context.SaveChangesAsync();

        await _layananAudit.CatatAsync("JADWAL_WAWANCARA", "JadwalWawancara", $"Menjadwalkan wawancara untuk {pelamar.NamaLengkap} pada {jadwal.TanggalWawancara:dd/MM/yyyy} pukul {jadwal.JamMulai}");

        return (true, $"Jadwal wawancara untuk '{pelamar.NamaLengkap}' berhasil disimpan.", jadwal.Id);
    }

    public async Task<(bool Sukses, string Pesan, Guid? Id)> SimpanHasilSeleksiAsync(FormHasilSeleksiViewModel model, Guid penilaiId, string namaPenilai)
    {
        var tenantId = _layananTenant.PerusahaanIdSaatIni ?? Guid.Empty;

        var pelamar = await _context.Pelamar.FindAsync(model.PelamarId);
        if (pelamar == null) return (false, "Data pelamar tidak ditemukan.", null);

        var hasil = new HasilSeleksi
        {
            Id = Guid.NewGuid(),
            PerusahaanId = tenantId,
            PelamarId = model.PelamarId,
            TahapanSeleksiId = model.TahapanSeleksiId,
            JadwalWawancaraId = model.JadwalWawancaraId,
            PenilaiId = penilaiId,
            NamaPenilai = namaPenilai,
            TanggalPenilaian = DateTime.Today,
            Skor = model.Skor,
            CatatanEvaluasi = model.CatatanEvaluasi.Trim(),
            Kelebihan = model.Kelebihan?.Trim(),
            Kekurangan = model.Kekurangan?.Trim(),
            Keputusan = model.Keputusan
        };

        await _context.HasilSeleksi.AddAsync(hasil);

        // Tandai jadwal wawancara selesai jika terkait
        if (model.JadwalWawancaraId.HasValue)
        {
            var jadwal = await _context.JadwalWawancara.FindAsync(model.JadwalWawancaraId.Value);
            if (jadwal != null)
            {
                jadwal.Status = StatusWawancara.Selesai;
                _context.JadwalWawancara.Update(jadwal);
            }
        }

        // Perbarui status pelamar berdasarkan hasil keputusan
        if (model.Keputusan == HasilKeputusanSeleksi.TidakLolos)
        {
            pelamar.Status = StatusPelamar.Ditolak;
            _context.Pelamar.Update(pelamar);
        }
        else if (model.Keputusan == HasilKeputusanSeleksi.Lolos && pelamar.Status != StatusPelamar.Diterima)
        {
            pelamar.Status = StatusPelamar.PenawaranKerja;
            _context.Pelamar.Update(pelamar);
        }

        await _context.SaveChangesAsync();

        await _layananAudit.CatatAsync("CATAT_HASIL_SELEKSI", "HasilSeleksi", $"Mencatat evaluasi seleksi {pelamar.NamaLengkap} dengan skor {hasil.Skor} ({hasil.Keputusan})");

        return (true, $"Catatan evaluasi seleksi untuk '{pelamar.NamaLengkap}' berhasil disimpan.", hasil.Id);
    }
    #endregion

    #region Penerimaan Kandidat & Konversi Karyawan
    public async Task<(bool Sukses, string Pesan, Guid? KaryawanId)> TerimaDanKonversiKaryawanAsync(FormTerimaPelamarViewModel model)
    {
        var tenantId = _layananTenant.PerusahaanIdSaatIni ?? Guid.Empty;

        var pelamar = await _context.Pelamar
            .Include(p => p.LowonganKerja)
            .FirstOrDefaultAsync(p => p.Id == model.PelamarId);

        if (pelamar == null) return (false, "Data pelamar tidak ditemukan.", null);

        if (pelamar.KaryawanId.HasValue)
        {
            return (false, "Pelamar ini sudah pernah diterima dan dikonversi menjadi karyawan.", pelamar.KaryawanId);
        }

        // Cek duplikasi NIK per tenant
        var adaNik = await _context.Karyawan
            .AnyAsync(k => k.NomorIndukKaryawan == model.NomorIndukKaryawan);

        if (adaNik)
        {
            return (false, $"Nomor Induk Karyawan '{model.NomorIndukKaryawan}' sudah terdaftar dalam sistem.", null);
        }

        using var transaksi = await _context.Database.BeginTransactionAsync();
        try
        {
            // 1. Buat data karyawan baru
            var karyawanBaru = new Karyawan
            {
                Id = Guid.NewGuid(),
                PerusahaanId = tenantId,
                NomorIndukKaryawan = model.NomorIndukKaryawan.Trim().ToUpper(),
                NamaLengkap = pelamar.NamaLengkap,
                Email = pelamar.Email,
                NomorTelepon = pelamar.NomorTelepon,
                DepartemenId = model.DepartemenId,
                JabatanId = model.JabatanId,
                TanggalMasuk = model.TanggalMasuk,
                StatusKaryawan = StatusKaryawan.Kontrak,
                StatusPTKP = StatusPTKP.TK0,
                JenisKelamin = JenisKelamin.LakiLaki
            };

            await _context.Karyawan.AddAsync(karyawanBaru);

            // 2. Kaitkan Gaji Pokok jika ada komponen Gaji Pokok baku
            var gpKomponen = await _context.KomponenGaji
                .FirstOrDefaultAsync(k => k.KodeKomponen == "GP" || k.Kategori == KategoriKomponenGaji.GajiPokok);

            if (gpKomponen != null && model.GajiPokok > 0)
            {
                await _context.GajiKaryawan.AddAsync(new GajiKaryawan
                {
                    Id = Guid.NewGuid(),
                    PerusahaanId = tenantId,
                    KaryawanId = karyawanBaru.Id,
                    KomponenGajiId = gpKomponen.Id,
                    Nominal = model.GajiPokok,
                    StatusAktif = true,
                    Keterangan = "Gaji pokok awal penerimaan rekrutmen"
                });
            }

            // 3. Perbarui status pelamar
            pelamar.Status = StatusPelamar.Diterima;
            pelamar.KaryawanId = karyawanBaru.Id;
            _context.Pelamar.Update(pelamar);

            await _context.SaveChangesAsync();
            await transaksi.CommitAsync();

            await _layananAudit.CatatAsync(
                "TERIMA_PELAMAR",
                "Pelamar",
                $"Menerima kandidat {pelamar.NamaLengkap} menjadi Karyawan baru (NIK: {karyawanBaru.NomorIndukKaryawan})");

            return (true, $"Kandidat '{pelamar.NamaLengkap}' berhasil diterima dan dikonversi menjadi karyawan dengan NIK {karyawanBaru.NomorIndukKaryawan}.", karyawanBaru.Id);
        }
        catch (Exception ex)
        {
            await transaksi.RollbackAsync();
            return (false, $"Gagal mengonversi kandidat menjadi karyawan: {ex.Message}", null);
        }
    }
    #endregion
}
