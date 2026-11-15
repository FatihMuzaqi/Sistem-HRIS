using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SistemHRD.Data;
using SistemHRD.Models.Entitas;
using SistemHRD.Models.Enums;
using SistemHRD.Services.Interfaces;
using SistemHRD.ViewModels;

namespace SistemHRD.Services.Implementasi;

public class LayananKaryawan : ILayananKaryawan
{
    private readonly KonteksAplikasi _context;
    private readonly IWebHostEnvironment _webHostEnvironment;
    private readonly ILayananAudit _layananAudit;
    private readonly ILayananTenant _layananTenant;

    public LayananKaryawan(
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

    public async Task<KaryawanListViewModel> DapatkanDaftarAsync(
        string? cari,
        Guid? departemenId,
        Guid? jabatanId,
        StatusKaryawan? status,
        int halaman,
        int ukuranHalaman)
    {
        if (halaman < 1) halaman = 1;
        if (ukuranHalaman < 1) ukuranHalaman = 10;

        var query = _context.Karyawan
            .Include(k => k.Departemen)
            .Include(k => k.Jabatan)
            .AsQueryable();

        // 1. Filter Pencarian Teks
        if (!string.IsNullOrWhiteSpace(cari))
        {
            var keyword = cari.Trim().ToLower();
            query = query.Where(k =>
                k.NamaLengkap.ToLower().Contains(keyword) ||
                k.NomorIndukKaryawan.ToLower().Contains(keyword) ||
                k.Email.ToLower().Contains(keyword) ||
                (k.NomorTelepon != null && k.NomorTelepon.Contains(keyword)));
        }

        // 2. Filter Departemen
        if (departemenId.HasValue && departemenId.Value != Guid.Empty)
        {
            query = query.Where(k => k.DepartemenId == departemenId.Value);
        }

        // 3. Filter Jabatan
        if (jabatanId.HasValue && jabatanId.Value != Guid.Empty)
        {
            query = query.Where(k => k.JabatanId == jabatanId.Value);
        }

        // 4. Filter Status
        if (status.HasValue)
        {
            query = query.Where(k => k.StatusKaryawan == status.Value);
        }

        var totalData = await query.CountAsync();

        var daftarKaryawan = await query
            .OrderBy(k => k.NamaLengkap)
            .Skip((halaman - 1) * ukuranHalaman)
            .Take(ukuranHalaman)
            .ToListAsync();

        // Siapkan Opsi Filter Dropdown
        var departemenList = await _context.Departemen
            .Where(d => d.StatusAktif)
            .OrderBy(d => d.NamaDepartemen)
            .Select(d => new SelectListItem { Value = d.Id.ToString(), Text = d.NamaDepartemen })
            .ToListAsync();

        var jabatanList = await _context.Jabatan
            .Where(j => j.StatusAktif)
            .OrderBy(j => j.NamaJabatan)
            .Select(j => new SelectListItem { Value = j.Id.ToString(), Text = j.NamaJabatan })
            .ToListAsync();

        return new KaryawanListViewModel
        {
            DaftarKaryawan = daftarKaryawan,
            Cari = cari,
            DepartemenId = departemenId,
            JabatanId = jabatanId,
            Status = status,
            HalamanSaatIni = halaman,
            UkuranHalaman = ukuranHalaman,
            TotalData = totalData,
            OpsiDepartemen = departemenList,
            OpsiJabatan = jabatanList
        };
    }

    public async Task<Karyawan?> DapatkanBerdasarkanIdAsync(Guid id)
    {
        return await _context.Karyawan
            .Include(k => k.Departemen)
            .Include(k => k.Jabatan)
            .FirstOrDefaultAsync(k => k.Id == id);
    }

    public async Task<KaryawanDetailViewModel?> DapatkanDetailAsync(Guid id)
    {
        var karyawan = await _context.Karyawan
            .Include(k => k.Departemen)
            .Include(k => k.Jabatan)
            .Include(k => k.Pengguna)
            .FirstOrDefaultAsync(k => k.Id == id);

        if (karyawan == null) return null;

        var daftarKontrak = await _context.KontrakKerja
            .Where(c => c.KaryawanId == id)
            .OrderByDescending(c => c.TanggalMulai)
            .ToListAsync();

        var daftarDokumen = await _context.DokumenKaryawan
            .Where(d => d.KaryawanId == id)
            .OrderByDescending(d => d.CreatedAt)
            .ToListAsync();

        var daftarKontakDarurat = await _context.KontakDaruratKaryawan
            .Where(kd => kd.KaryawanId == id)
            .ToListAsync();

        var daftarRiwayatPekerjaan = await _context.RiwayatPekerjaan
            .Where(rp => rp.KaryawanId == id)
            .OrderByDescending(rp => rp.TahunSelesai)
            .ToListAsync();

        return new KaryawanDetailViewModel
        {
            Karyawan = karyawan,
            DaftarKontrakKerja = daftarKontrak,
            DaftarDokumen = daftarDokumen,
            DaftarKontakDarurat = daftarKontakDarurat,
            DaftarRiwayatPekerjaan = daftarRiwayatPekerjaan,
            FormKontrak = new KontrakKerjaFormViewModel { KaryawanId = id },
            FormDokumen = new DokumenUploadViewModel { KaryawanId = id },
            FormKontakDarurat = new KontakDaruratFormViewModel { KaryawanId = id },
            FormRiwayatPekerjaan = new RiwayatPekerjaanFormViewModel { KaryawanId = id }
        };
    }

    public async Task<(bool Sukses, string? PesanError)> SimpanKaryawanAsync(KaryawanFormViewModel model, IFormFile? berkasFoto)
    {
        // 1. Validasi Keunikan NIK dalam Tenant
        var nikSudahAda = await _context.Karyawan
            .AnyAsync(k => k.NomorIndukKaryawan == model.NomorIndukKaryawan && k.Id != model.Id);

        if (nikSudahAda)
        {
            return (false, $"Nomor Induk Karyawan (NIK) '{model.NomorIndukKaryawan}' sudah digunakan.");
        }

        // 2. Penanganan Upload Foto Profil
        string? namaBerkasFoto = model.FotoProfilSaatIni;
        if (berkasFoto != null && berkasFoto.Length > 0)
        {
            var validExt = new[] { ".jpg", ".jpeg", ".png" };
            var ext = Path.GetExtension(berkasFoto.FileName).ToLowerInvariant();
            if (!validExt.Contains(ext))
            {
                return (false, "Format foto profil hanya boleh .jpg, .jpeg, atau .png");
            }

            if (berkasFoto.Length > 2 * 1024 * 1024) // Maksimal 2MB
            {
                return (false, "Ukuran berkas foto maksimal 2MB.");
            }

            var folderUpload = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "profil");
            if (!Directory.Exists(folderUpload))
            {
                Directory.CreateDirectory(folderUpload);
            }

            namaBerkasFoto = $"foto_{Guid.NewGuid()}{ext}";
            var pathTujuan = Path.Combine(folderUpload, namaBerkasFoto);
            using (var stream = new FileStream(pathTujuan, FileMode.Create))
            {
                await berkasFoto.CopyToAsync(stream);
            }
        }

        if (model.Id.HasValue && model.Id.Value != Guid.Empty)
        {
            // Mode Update
            var karyawan = await _context.Karyawan.FindAsync(model.Id.Value);
            if (karyawan == null) return (false, "Data karyawan tidak ditemukan.");

            karyawan.NomorIndukKaryawan = model.NomorIndukKaryawan.Trim();
            karyawan.NamaLengkap = model.NamaLengkap.Trim();
            karyawan.TempatLahir = model.TempatLahir?.Trim();
            karyawan.TanggalLahir = model.TanggalLahir;
            karyawan.JenisKelamin = model.JenisKelamin;
            karyawan.Alamat = model.Alamat?.Trim();
            karyawan.NomorTelepon = model.NomorTelepon?.Trim();
            karyawan.Email = model.Email.Trim();
            karyawan.TanggalMasuk = model.TanggalMasuk;
            karyawan.DepartemenId = model.DepartemenId;
            karyawan.JabatanId = model.JabatanId;
            karyawan.StatusKaryawan = model.StatusKaryawan;
            if (!string.IsNullOrEmpty(namaBerkasFoto))
            {
                karyawan.FotoProfil = namaBerkasFoto;
            }

            await _context.SaveChangesAsync();
            await _layananAudit.CatatAsync("UPDATE_KARYAWAN", "Karyawan", karyawan.Id.ToString(), null, $"Pembaruan data karyawan {karyawan.NamaLengkap} (NIK: {karyawan.NomorIndukKaryawan})");
        }
        else
        {
            // Mode Create
            var karyawan = new Karyawan
            {
                Id = Guid.NewGuid(),
                NomorIndukKaryawan = model.NomorIndukKaryawan.Trim(),
                NamaLengkap = model.NamaLengkap.Trim(),
                TempatLahir = model.TempatLahir?.Trim(),
                TanggalLahir = model.TanggalLahir,
                JenisKelamin = model.JenisKelamin,
                Alamat = model.Alamat?.Trim(),
                NomorTelepon = model.NomorTelepon?.Trim(),
                Email = model.Email.Trim(),
                TanggalMasuk = model.TanggalMasuk,
                DepartemenId = model.DepartemenId,
                JabatanId = model.JabatanId,
                StatusKaryawan = model.StatusKaryawan,
                FotoProfil = namaBerkasFoto
            };

            await _context.Karyawan.AddAsync(karyawan);
            await _context.SaveChangesAsync();
            await _layananAudit.CatatAsync("TAMBAH_KARYAWAN", "Karyawan", karyawan.Id.ToString(), null, $"Penambahan karyawan baru {karyawan.NamaLengkap} (NIK: {karyawan.NomorIndukKaryawan})");
        }

        return (true, null);
    }

    public async Task<(bool Sukses, string? PesanError)> UbahStatusAktifAsync(Guid id)
    {
        var karyawan = await _context.Karyawan.FindAsync(id);
        if (karyawan == null) return (false, "Data karyawan tidak ditemukan.");

        if (karyawan.StatusKaryawan == StatusKaryawan.Nonaktif)
        {
            karyawan.StatusKaryawan = StatusKaryawan.Tetap;
        }
        else
        {
            karyawan.StatusKaryawan = StatusKaryawan.Nonaktif;
        }

        await _context.SaveChangesAsync();
        await _layananAudit.CatatAsync("UBAH_STATUS_KARYAWAN", "Karyawan", karyawan.Id.ToString(), null, $"Ubah status karyawan {karyawan.NamaLengkap} menjadi {karyawan.StatusKaryawan}");

        return (true, null);
    }

    public async Task<(bool Sukses, string? PesanError)> TambahKontrakAsync(KontrakKerjaFormViewModel model)
    {
        var kontrak = new KontrakKerja
        {
            Id = Guid.NewGuid(),
            KaryawanId = model.KaryawanId,
            NomorKontrak = model.NomorKontrak.Trim(),
            JenisKontrak = model.JenisKontrak,
            TanggalMulai = model.TanggalMulai,
            TanggalBerakhir = model.TanggalBerakhir,
            StatusKontrak = model.StatusKontrak,
            Keterangan = model.Keterangan?.Trim()
        };

        await _context.KontrakKerja.AddAsync(kontrak);
        await _context.SaveChangesAsync();
        await _layananAudit.CatatAsync("TAMBAH_KONTRAK", "KontrakKerja", kontrak.Id.ToString(), null, $"Tambah kontrak {kontrak.NomorKontrak} untuk karyawan {model.KaryawanId}");

        return (true, null);
    }

    public async Task<(bool Sukses, string? PesanError)> TambahKontakDaruratAsync(KontakDaruratFormViewModel model)
    {
        var kontak = new KontakDaruratKaryawan
        {
            Id = Guid.NewGuid(),
            KaryawanId = model.KaryawanId,
            NamaLengkap = model.NamaLengkap.Trim(),
            Hubungan = model.Hubungan.Trim(),
            NomorTelepon = model.NomorTelepon.Trim(),
            Alamat = model.Alamat?.Trim()
        };

        await _context.KontakDaruratKaryawan.AddAsync(kontak);
        await _context.SaveChangesAsync();
        await _layananAudit.CatatAsync("TAMBAH_KONTAK_DARURAT", "KontakDaruratKaryawan", kontak.Id.ToString(), null, $"Tambah kontak darurat {kontak.NamaLengkap}");

        return (true, null);
    }

    public async Task<(bool Sukses, string? PesanError)> TambahRiwayatPekerjaanAsync(RiwayatPekerjaanFormViewModel model)
    {
        var riwayat = new RiwayatPekerjaan
        {
            Id = Guid.NewGuid(),
            KaryawanId = model.KaryawanId,
            NamaPerusahaanSebelumnya = model.NamaPerusahaanSebelumnya.Trim(),
            JabatanTerakhir = model.JabatanTerakhir.Trim(),
            TahunMulai = model.TahunMulai,
            TahunSelesai = model.TahunSelesai,
            DeskripsiTugas = model.DeskripsiTugas?.Trim()
        };

        await _context.RiwayatPekerjaan.AddAsync(riwayat);
        await _context.SaveChangesAsync();
        await _layananAudit.CatatAsync("TAMBAH_RIWAYAT_PEKERJAAN", "RiwayatPekerjaan", riwayat.Id.ToString(), null, $"Tambah riwayat pekerjaan di {riwayat.NamaPerusahaanSebelumnya}");

        return (true, null);
    }

    public async Task<(bool Sukses, string? PesanError)> UploadDokumenAsync(DokumenUploadViewModel model)
    {
        if (model.BerkasDokumen == null || model.BerkasDokumen.Length == 0)
        {
            return (false, "Berkas dokumen tidak boleh kosong.");
        }

        var validExt = new[] { ".pdf", ".jpg", ".jpeg", ".png", ".doc", ".docx" };
        var ext = Path.GetExtension(model.BerkasDokumen.FileName).ToLowerInvariant();
        if (!validExt.Contains(ext))
        {
            return (false, "Format berkas tidak diizinkan. Hanya mendukung PDF, JPG, PNG, DOC, DOCX.");
        }

        if (model.BerkasDokumen.Length > 5 * 1024 * 1024) // Maksimal 5MB
        {
            return (false, "Ukuran berkas melebihi batas maksimal 5MB.");
        }

        var folderUpload = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "dokumen");
        if (!Directory.Exists(folderUpload))
        {
            Directory.CreateDirectory(folderUpload);
        }

        var namaSimpan = $"dok_{Guid.NewGuid()}{ext}";
        var pathSimpan = Path.Combine(folderUpload, namaSimpan);

        using (var stream = new FileStream(pathSimpan, FileMode.Create))
        {
            await model.BerkasDokumen.CopyToAsync(stream);
        }

        var dokumen = new DokumenKaryawan
        {
            Id = Guid.NewGuid(),
            KaryawanId = model.KaryawanId,
            NamaDokumen = model.NamaDokumen.Trim(),
            JenisDokumen = model.JenisDokumen,
            NamaFile = model.BerkasDokumen.FileName,
            UkuranFile = model.BerkasDokumen.Length,
            TipeFile = model.BerkasDokumen.ContentType,
            PathFile = Path.Combine("uploads", "dokumen", namaSimpan),
            Keterangan = model.Keterangan?.Trim()
        };

        await _context.DokumenKaryawan.AddAsync(dokumen);
        await _context.SaveChangesAsync();
        await _layananAudit.CatatAsync("UPLOAD_DOKUMEN", "DokumenKaryawan", dokumen.Id.ToString(), null, $"Unggah dokumen {dokumen.NamaDokumen}");

        return (true, null);
    }

    public async Task<DokumenKaryawan?> DapatkanDokumenAsync(Guid id)
    {
        return await _context.DokumenKaryawan
            .Include(d => d.Karyawan)
            .FirstOrDefaultAsync(d => d.Id == id);
    }

    public async Task<(bool Sukses, string? PesanError)> HapusDokumenAsync(Guid id)
    {
        var dokumen = await _context.DokumenKaryawan.FindAsync(id);
        if (dokumen == null) return (false, "Dokumen tidak ditemukan.");

        _context.DokumenKaryawan.Remove(dokumen);
        await _context.SaveChangesAsync();
        await _layananAudit.CatatAsync("HAPUS_DOKUMEN", "DokumenKaryawan", dokumen.Id.ToString(), null, $"Hapus dokumen {dokumen.NamaDokumen}");

        return (true, null);
    }
}
