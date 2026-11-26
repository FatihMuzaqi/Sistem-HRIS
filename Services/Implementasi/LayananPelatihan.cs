using Microsoft.EntityFrameworkCore;
using SistemHRD.Data;
using SistemHRD.Models.Entitas;
using SistemHRD.Models.Enums;
using SistemHRD.Services.Interfaces;
using SistemHRD.ViewModels;

namespace SistemHRD.Services.Implementasi;

public class LayananPelatihan : ILayananPelatihan
{
    private readonly KonteksAplikasi _context;
    private readonly ILayananTenant _layananTenant;

    public LayananPelatihan(KonteksAplikasi context, ILayananTenant layananTenant)
    {
        _context = context;
        _layananTenant = layananTenant;
    }

    public async Task<DashboardPelatihanViewModel> DapatkanDashboardAsync()
    {
        var model = new DashboardPelatihanViewModel();

        model.TotalProgramAktif = await _context.ProgramPelatihan
            .CountAsync(p => p.Status == StatusPelatihan.PendaftaranDibuka || p.Status == StatusPelatihan.Berlangsung);

        model.TotalPesertaTerdaftar = await _context.PesertaPelatihan.CountAsync();
        model.TotalPesertaLulus = await _context.PesertaPelatihan
            .CountAsync(p => p.StatusKeikutsertaan == StatusKeikutsertaan.Lulus);

        model.TotalSertifikatTerbit = await _context.SertifikatPelatihan.CountAsync();

        // Statistik Status
        var statusList = await _context.ProgramPelatihan
            .GroupBy(p => p.Status)
            .Select(g => new { Status = g.Key.ToString(), Count = g.Count() })
            .ToListAsync();
        foreach (var s in statusList) model.StatistikStatusProgram[s.Status] = s.Count;

        // Statistik Metode
        var metodeList = await _context.ProgramPelatihan
            .GroupBy(p => p.Metode)
            .Select(g => new { Metode = g.Key.ToString(), Count = g.Count() })
            .ToListAsync();
        foreach (var m in metodeList) model.StatistikMetodePelatihan[m.Metode] = m.Count;

        // Program Terbaru
        model.ProgramTerbaru = await _context.ProgramPelatihan
            .Include(p => p.Departemen)
            .Include(p => p.DaftarPeserta)
            .OrderByDescending(p => p.CreatedAt)
            .Take(5)
            .Select(p => new ProgramPelatihanItemViewModel
            {
                Id = p.Id,
                KodeProgram = p.KodeProgram,
                NamaProgram = p.NamaProgram,
                Penyelenggara = p.Penyelenggara,
                Instruktur = p.Instruktur,
                Metode = p.Metode,
                LokasiAtauTautan = p.LokasiAtauTautan,
                TanggalMulai = p.TanggalMulai,
                TanggalSelesai = p.TanggalSelesai,
                TotalJam = p.TotalJam,
                BiayaPerPeserta = p.BiayaPerPeserta,
                MaksimalPeserta = p.MaksimalPeserta,
                JumlahPesertaTerdaftar = p.DaftarPeserta.Count,
                Status = p.Status,
                NamaDepartemen = p.Departemen != null ? p.Departemen.NamaDepartemen : "Semua Departemen (Umum)"
            })
            .ToListAsync();

        // Peserta Terbaru
        model.PesertaTerbaru = await _context.PesertaPelatihan
            .Include(p => p.ProgramPelatihan)
            .Include(p => p.Karyawan)
            .ThenInclude(k => k.Departemen)
            .Include(p => p.Sertifikat)
            .OrderByDescending(p => p.CreatedAt)
            .Take(5)
            .Select(p => new PesertaPelatihanItemViewModel
            {
                Id = p.Id,
                ProgramPelatihanId = p.ProgramPelatihanId,
                NamaProgram = p.ProgramPelatihan.NamaProgram,
                KaryawanId = p.KaryawanId,
                NIK = p.Karyawan.NomorIndukKaryawan,
                NamaKaryawan = p.Karyawan.NamaLengkap,
                NamaDepartemen = p.Karyawan.Departemen != null ? p.Karyawan.Departemen.NamaDepartemen : "-",
                Status = p.StatusKeikutsertaan,
                NilaiAkhir = p.NilaiAkhir,
                TanggalDidaftarkan = p.TanggalDidaftarkan,
                SertifikatId = p.Sertifikat != null ? p.Sertifikat.Id : null,
                NomorSertifikat = p.Sertifikat != null ? p.Sertifikat.NomorSertifikat : null
            })
            .ToListAsync();

        return model;
    }

    public async Task<List<ProgramPelatihanItemViewModel>> DapatkanSemuaProgramAsync(StatusPelatihan? status = null)
    {
        var query = _context.ProgramPelatihan
            .Include(p => p.Departemen)
            .Include(p => p.DaftarPeserta)
            .AsQueryable();

        if (status.HasValue)
        {
            query = query.Where(p => p.Status == status.Value);
        }

        return await query
            .OrderByDescending(p => p.TanggalMulai)
            .Select(p => new ProgramPelatihanItemViewModel
            {
                Id = p.Id,
                KodeProgram = p.KodeProgram,
                NamaProgram = p.NamaProgram,
                Penyelenggara = p.Penyelenggara,
                Instruktur = p.Instruktur,
                Metode = p.Metode,
                LokasiAtauTautan = p.LokasiAtauTautan,
                TanggalMulai = p.TanggalMulai,
                TanggalSelesai = p.TanggalSelesai,
                TotalJam = p.TotalJam,
                BiayaPerPeserta = p.BiayaPerPeserta,
                MaksimalPeserta = p.MaksimalPeserta,
                JumlahPesertaTerdaftar = p.DaftarPeserta.Count,
                Status = p.Status,
                NamaDepartemen = p.Departemen != null ? p.Departemen.NamaDepartemen : "Semua Departemen (Umum)"
            })
            .ToListAsync();
    }

    public async Task<ProgramPelatihan?> DapatkanProgramByIdAsync(Guid id)
    {
        return await _context.ProgramPelatihan
            .Include(p => p.Departemen)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<DetailProgramPelatihanViewModel?> DapatkanDetailProgramAsync(Guid id)
    {
        var p = await _context.ProgramPelatihan
            .Include(pr => pr.Departemen)
            .Include(pr => pr.DaftarPeserta)
                .ThenInclude(ps => ps.Karyawan)
                    .ThenInclude(k => k.Departemen)
            .Include(pr => pr.DaftarPeserta)
                .ThenInclude(ps => ps.Karyawan)
                    .ThenInclude(k => k.Jabatan)
            .Include(pr => pr.DaftarPeserta)
                .ThenInclude(ps => ps.Sertifikat)
            .FirstOrDefaultAsync(pr => pr.Id == id);

        if (p == null) return null;

        return new DetailProgramPelatihanViewModel
        {
            Id = p.Id,
            KodeProgram = p.KodeProgram,
            NamaProgram = p.NamaProgram,
            Deskripsi = p.Deskripsi,
            Penyelenggara = p.Penyelenggara,
            Instruktur = p.Instruktur,
            Metode = p.Metode,
            LokasiAtauTautan = p.LokasiAtauTautan,
            TanggalMulai = p.TanggalMulai,
            TanggalSelesai = p.TanggalSelesai,
            TotalJam = p.TotalJam,
            BiayaPerPeserta = p.BiayaPerPeserta,
            MaksimalPeserta = p.MaksimalPeserta,
            Status = p.Status,
            NamaDepartemen = p.Departemen != null ? p.Departemen.NamaDepartemen : "Semua Departemen (Umum)",
            DaftarPeserta = p.DaftarPeserta.Select(ps => new PesertaPelatihanItemViewModel
            {
                Id = ps.Id,
                ProgramPelatihanId = ps.ProgramPelatihanId,
                NamaProgram = p.NamaProgram,
                KaryawanId = ps.KaryawanId,
                NIK = ps.Karyawan.NomorIndukKaryawan,
                NamaKaryawan = ps.Karyawan.NamaLengkap,
                NamaDepartemen = ps.Karyawan.Departemen != null ? ps.Karyawan.Departemen.NamaDepartemen : "-",
                NamaJabatan = ps.Karyawan.Jabatan != null ? ps.Karyawan.Jabatan.NamaJabatan : "-",
                Status = ps.StatusKeikutsertaan,
                NilaiAkhir = ps.NilaiAkhir,
                Catatan = ps.Catatan,
                TanggalDidaftarkan = ps.TanggalDidaftarkan,
                TanggalSelesai = ps.TanggalSelesai,
                SertifikatId = ps.Sertifikat != null ? ps.Sertifikat.Id : null,
                NomorSertifikat = ps.Sertifikat != null ? ps.Sertifikat.NomorSertifikat : null,
                KodeVerifikasiSertifikat = ps.Sertifikat != null ? ps.Sertifikat.KodeVerifikasi : null
            }).OrderBy(ps => ps.NamaKaryawan).ToList()
        };
    }

    public async Task<(bool Sukses, string Pesan, Guid? Id)> SimpanProgramAsync(FormProgramPelatihanViewModel model)
    {
        var perusahaanId = _layananTenant.PerusahaanIdSaatIni;
        if (!perusahaanId.HasValue) return (false, "Sesi perusahaan tidak valid.", null);

        if (model.TanggalSelesai < model.TanggalMulai)
        {
            return (false, "Tanggal selesai tidak boleh lebih awal dari tanggal mulai.", null);
        }

        // Cek keunikan kode program
        var duplikat = await _context.ProgramPelatihan
            .AnyAsync(p => p.KodeProgram == model.KodeProgram && (!model.Id.HasValue || p.Id != model.Id.Value));

        if (duplikat)
        {
            return (false, $"Kode program pelatihan '{model.KodeProgram}' sudah digunakan dalam perusahaan Anda.", null);
        }

        ProgramPelatihan program;
        if (model.Id.HasValue && model.Id.Value != Guid.Empty)
        {
            var existing = await _context.ProgramPelatihan.FindAsync(model.Id.Value);
            if (existing == null) return (false, "Program pelatihan tidak ditemukan.", null);
            program = existing;
        }
        else
        {
            program = new ProgramPelatihan { PerusahaanId = perusahaanId.Value };
            _context.ProgramPelatihan.Add(program);
        }

        program.KodeProgram = model.KodeProgram;
        program.NamaProgram = model.NamaProgram;
        program.Deskripsi = model.Deskripsi;
        program.Penyelenggara = model.Penyelenggara;
        program.Instruktur = model.Instruktur;
        program.Metode = model.Metode;
        program.LokasiAtauTautan = model.LokasiAtauTautan;
        program.TanggalMulai = model.TanggalMulai;
        program.TanggalSelesai = model.TanggalSelesai;
        program.TotalJam = model.TotalJam;
        program.BiayaPerPeserta = model.BiayaPerPeserta;
        program.MaksimalPeserta = model.MaksimalPeserta;
        program.Status = model.Status;
        program.DepartemenId = model.DepartemenId;

        await _context.SaveChangesAsync();
        return (true, "Program pelatihan berhasil disimpan.", program.Id);
    }

    public async Task<(bool Sukses, string Pesan)> HapusProgramAsync(Guid id)
    {
        var program = await _context.ProgramPelatihan
            .Include(p => p.DaftarPeserta)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (program == null) return (false, "Program pelatihan tidak ditemukan.");

        if (program.DaftarPeserta.Any(ps => ps.StatusKeikutsertaan == StatusKeikutsertaan.Lulus))
        {
            return (false, "Program pelatihan tidak dapat dihapus karena sudah memiliki peserta yang lulus.");
        }

        _context.ProgramPelatihan.Remove(program);
        await _context.SaveChangesAsync();
        return (true, "Program pelatihan berhasil dihapus.");
    }

    public async Task<(bool Sukses, string Pesan)> DaftarkanPesertaAsync(FormDaftarPesertaViewModel model)
    {
        var program = await _context.ProgramPelatihan
            .Include(p => p.DaftarPeserta)
            .FirstOrDefaultAsync(p => p.Id == model.ProgramPelatihanId);

        if (program == null) return (false, "Program pelatihan tidak ditemukan.");

        if (program.Status == StatusPelatihan.Selesai || program.Status == StatusPelatihan.Dibatalkan)
        {
            return (false, "Pendaftaran tidak dapat dilakukan karena status pelatihan sudah selesai atau dibatalkan.");
        }

        if (program.DaftarPeserta.Count >= program.MaksimalPeserta)
        {
            return (false, $"Kuota peserta program pelatihan sudah penuh (Maksimal: {program.MaksimalPeserta} orang).");
        }

        var sudahTerdaftar = await _context.PesertaPelatihan
            .AnyAsync(ps => ps.ProgramPelatihanId == model.ProgramPelatihanId && ps.KaryawanId == model.KaryawanId);

        if (sudahTerdaftar)
        {
            return (false, "Karyawan ini sudah terdaftar dalam program pelatihan yang sama.");
        }

        var peserta = new PesertaPelatihan
        {
            PerusahaanId = program.PerusahaanId,
            ProgramPelatihanId = model.ProgramPelatihanId,
            KaryawanId = model.KaryawanId,
            StatusKeikutsertaan = StatusKeikutsertaan.Terdaftar,
            Catatan = model.Catatan,
            TanggalDidaftarkan = DateTime.Today
        };

        _context.PesertaPelatihan.Add(peserta);
        await _context.SaveChangesAsync();

        return (true, "Karyawan berhasil didaftarkan sebagai peserta pelatihan.");
    }

    public async Task<(bool Sukses, string Pesan, int JumlahBerhasil)> DaftarkanPesertaMassalAsync(FormDaftarPesertaMassalViewModel model)
    {
        var program = await _context.ProgramPelatihan
            .Include(p => p.DaftarPeserta)
            .FirstOrDefaultAsync(p => p.Id == model.ProgramPelatihanId);

        if (program == null) return (false, "Program pelatihan tidak ditemukan.", 0);

        int count = 0;
        foreach (var kryId in model.DaftarKaryawanId)
        {
            if (program.DaftarPeserta.Count >= program.MaksimalPeserta) break;

            bool exists = await _context.PesertaPelatihan
                .AnyAsync(ps => ps.ProgramPelatihanId == model.ProgramPelatihanId && ps.KaryawanId == kryId);

            if (!exists)
            {
                var peserta = new PesertaPelatihan
                {
                    PerusahaanId = program.PerusahaanId,
                    ProgramPelatihanId = model.ProgramPelatihanId,
                    KaryawanId = kryId,
                    StatusKeikutsertaan = StatusKeikutsertaan.Terdaftar,
                    TanggalDidaftarkan = DateTime.Today
                };
                _context.PesertaPelatihan.Add(peserta);
                program.DaftarPeserta.Add(peserta);
                count++;
            }
        }

        await _context.SaveChangesAsync();
        return (true, $"Berhasil mendaftarkan {count} karyawan ke dalam program pelatihan.", count);
    }

    public async Task<(bool Sukses, string Pesan)> PerbaruiStatusPesertaAsync(FormUpdateStatusPesertaViewModel model)
    {
        var peserta = await _context.PesertaPelatihan
            .Include(ps => ps.ProgramPelatihan)
            .Include(ps => ps.Sertifikat)
            .FirstOrDefaultAsync(ps => ps.Id == model.PesertaId);

        if (peserta == null) return (false, "Data peserta pelatihan tidak ditemukan.");

        peserta.StatusKeikutsertaan = model.StatusKeikutsertaan;
        peserta.NilaiAkhir = model.NilaiAkhir;
        if (!string.IsNullOrWhiteSpace(model.Catatan)) peserta.Catatan = model.Catatan;

        if (model.StatusKeikutsertaan == StatusKeikutsertaan.Lulus)
        {
            peserta.TanggalSelesai = DateTime.Today;

            // Jika lulus dan belum ada sertifikat, otomatis terbitkan sertifikat awal
            if (peserta.Sertifikat == null)
            {
                var random = new Random();
                var nomorSertif = $"CERT-{DateTime.Today:yyyyMM}-{random.Next(100000, 999999)}";
                var kodeVerif = $"VERIF-TRN-{Guid.NewGuid().ToString("N")[..8].ToUpper()}";

                var sertifikat = new SertifikatPelatihan
                {
                    PerusahaanId = peserta.PerusahaanId,
                    PesertaPelatihanId = peserta.Id,
                    NomorSertifikat = nomorSertif,
                    JudulSertifikat = $"Sertifikat Kelulusan: {peserta.ProgramPelatihan.NamaProgram}",
                    TanggalTerbit = DateTime.Today,
                    PenerbitSertifikat = peserta.ProgramPelatihan.Penyelenggara,
                    KodeVerifikasi = kodeVerif,
                    Catatan = $"Diberikan atas kelulusan program {peserta.ProgramPelatihan.NamaProgram} dengan nilai {model.NilaiAkhir?.ToString("F1") ?? "Memuaskan"}."
                };
                _context.SertifikatPelatihan.Add(sertifikat);
            }
        }

        await _context.SaveChangesAsync();
        return (true, "Status keikutsertaan peserta berhasil diperbarui.");
    }

    public async Task<(bool Sukses, string Pesan)> HapusPesertaAsync(Guid pesertaId)
    {
        var peserta = await _context.PesertaPelatihan.FindAsync(pesertaId);
        if (peserta == null) return (false, "Data peserta tidak ditemukan.");

        _context.PesertaPelatihan.Remove(peserta);
        await _context.SaveChangesAsync();
        return (true, "Peserta berhasil dihapus dari pelatihan.");
    }

    public async Task<(bool Sukses, string Pesan, Guid? SertifikatId)> TerbitkanSertifikatAsync(Guid pesertaId, string penerbit, DateTime? tglKedaluwarsa, string? catatan)
    {
        var peserta = await _context.PesertaPelatihan
            .Include(ps => ps.ProgramPelatihan)
            .Include(ps => ps.Sertifikat)
            .FirstOrDefaultAsync(ps => ps.Id == pesertaId);

        if (peserta == null) return (false, "Data peserta pelatihan tidak ditemukan.", null);

        if (peserta.StatusKeikutsertaan != StatusKeikutsertaan.Lulus)
        {
            return (false, "Sertifikat hanya dapat diterbitkan untuk peserta yang berstatus LULUS.", null);
        }

        if (peserta.Sertifikat != null)
        {
            return (true, "Sertifikat sudah pernah diterbitkan sebelumnya.", peserta.Sertifikat.Id);
        }

        var random = new Random();
        var nomorSertif = $"CERT-{DateTime.Today:yyyyMM}-{random.Next(100000, 999999)}";
        var kodeVerif = $"VERIF-TRN-{Guid.NewGuid().ToString("N")[..8].ToUpper()}";

        var sertifikat = new SertifikatPelatihan
        {
            PerusahaanId = peserta.PerusahaanId,
            PesertaPelatihanId = peserta.Id,
            NomorSertifikat = nomorSertif,
            JudulSertifikat = $"Sertifikat Kelulusan: {peserta.ProgramPelatihan.NamaProgram}",
            TanggalTerbit = DateTime.Today,
            TanggalKedaluwarsa = tglKedaluwarsa,
            PenerbitSertifikat = string.IsNullOrWhiteSpace(penerbit) ? peserta.ProgramPelatihan.Penyelenggara : penerbit,
            KodeVerifikasi = kodeVerif,
            Catatan = catatan ?? $"Sertifikat resmi kompetensi kelulusan {peserta.ProgramPelatihan.NamaProgram}."
        };

        _context.SertifikatPelatihan.Add(sertifikat);
        await _context.SaveChangesAsync();

        return (true, "Sertifikat pelatihan berhasil diterbitkan.", sertifikat.Id);
    }

    public async Task<LembarSertifikatViewModel?> DapatkanSertifikatByIdAsync(Guid sertifikatId)
    {
        var s = await _context.SertifikatPelatihan
            .Include(st => st.PesertaPelatihan)
                .ThenInclude(ps => ps.Karyawan)
                    .ThenInclude(k => k.Departemen)
            .Include(st => st.PesertaPelatihan)
                .ThenInclude(ps => ps.ProgramPelatihan)
            .Include(st => st.PesertaPelatihan)
                .ThenInclude(ps => ps.Karyawan)
                    .ThenInclude(k => k.Perusahaan)
            .FirstOrDefaultAsync(st => st.Id == sertifikatId);

        if (s == null) return null;

        var ps = s.PesertaPelatihan;
        var prg = ps.ProgramPelatihan;
        var kry = ps.Karyawan;

        return new LembarSertifikatViewModel
        {
            Id = s.Id,
            NomorSertifikat = s.NomorSertifikat,
            JudulSertifikat = s.JudulSertifikat,
            NamaKaryawan = kry.NamaLengkap,
            NIK = kry.NomorIndukKaryawan,
            NamaDepartemen = kry.Departemen != null ? kry.Departemen.NamaDepartemen : "-",
            NamaProgram = prg.NamaProgram,
            DeskripsiPelatihan = prg.Deskripsi,
            TanggalMulai = prg.TanggalMulai,
            TanggalSelesai = prg.TanggalSelesai,
            TotalJam = prg.TotalJam,
            NilaiAkhir = ps.NilaiAkhir,
            TanggalTerbit = s.TanggalTerbit,
            TanggalKedaluwarsa = s.TanggalKedaluwarsa,
            PenerbitSertifikat = s.PenerbitSertifikat,
            KodeVerifikasi = s.KodeVerifikasi,
            NamaPerusahaan = kry.Perusahaan != null ? kry.Perusahaan.NamaPerusahaan : "SistemHRD Enterprise"
        };
    }

    public async Task<LembarSertifikatViewModel?> DapatkanSertifikatByPesertaIdAsync(Guid pesertaId)
    {
        var sertif = await _context.SertifikatPelatihan.FirstOrDefaultAsync(s => s.PesertaPelatihanId == pesertaId);
        if (sertif == null) return null;
        return await DapatkanSertifikatByIdAsync(sertif.Id);
    }

    public async Task<List<PesertaPelatihanItemViewModel>> DapatkanRiwayatPelatihanKaryawanAsync(Guid karyawanId)
    {
        return await _context.PesertaPelatihan
            .Include(ps => ps.ProgramPelatihan)
            .Include(ps => ps.Sertifikat)
            .Where(ps => ps.KaryawanId == karyawanId)
            .OrderByDescending(ps => ps.TanggalDidaftarkan)
            .Select(ps => new PesertaPelatihanItemViewModel
            {
                Id = ps.Id,
                ProgramPelatihanId = ps.ProgramPelatihanId,
                NamaProgram = ps.ProgramPelatihan.NamaProgram,
                KaryawanId = ps.KaryawanId,
                Status = ps.StatusKeikutsertaan,
                NilaiAkhir = ps.NilaiAkhir,
                Catatan = ps.Catatan,
                TanggalDidaftarkan = ps.TanggalDidaftarkan,
                TanggalSelesai = ps.TanggalSelesai,
                SertifikatId = ps.Sertifikat != null ? ps.Sertifikat.Id : null,
                NomorSertifikat = ps.Sertifikat != null ? ps.Sertifikat.NomorSertifikat : null,
                KodeVerifikasiSertifikat = ps.Sertifikat != null ? ps.Sertifikat.KodeVerifikasi : null
            })
            .ToListAsync();
    }
}
