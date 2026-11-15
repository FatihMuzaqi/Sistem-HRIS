using Microsoft.EntityFrameworkCore;
using SistemHRD.Data;
using SistemHRD.Models.Entitas;
using SistemHRD.Services.Interfaces;
using SistemHRD.ViewModels;

namespace SistemHRD.Services.Implementasi;

public class LayananDepartemen : ILayananDepartemen
{
    private readonly KonteksAplikasi _context;
    private readonly ILayananAudit _layananAudit;

    public LayananDepartemen(KonteksAplikasi context, ILayananAudit layananAudit)
    {
        _context = context;
        _layananAudit = layananAudit;
    }

    public async Task<List<Departemen>> DapatkanSemuaAsync()
    {
        return await _context.Departemen
            .Include(d => d.KepalaDepartemen)
            .Include(d => d.DaftarJabatan)
            .Include(d => d.DaftarKaryawan)
            .OrderBy(d => d.NamaDepartemen)
            .ToListAsync();
    }

    public async Task<Departemen?> DapatkanBerdasarkanIdAsync(Guid id)
    {
        return await _context.Departemen
            .Include(d => d.KepalaDepartemen)
            .Include(d => d.DaftarJabatan)
            .FirstOrDefaultAsync(d => d.Id == id);
    }

    public async Task<(bool Sukses, string? PesanError)> SimpanAsync(DepartemenFormViewModel model)
    {
        // Validasi keunikan kode departemen di dalam tenant aktif
        var kodeSudahAda = await _context.Departemen
            .AnyAsync(d => d.KodeDepartemen == model.KodeDepartemen && d.Id != model.Id);

        if (kodeSudahAda)
        {
            return (false, $"Kode Departemen '{model.KodeDepartemen}' sudah digunakan di perusahaan ini.");
        }

        if (model.Id.HasValue && model.Id.Value != Guid.Empty)
        {
            // Update
            var departemen = await _context.Departemen.FindAsync(model.Id.Value);
            if (departemen == null) return (false, "Departemen tidak ditemukan.");

            departemen.KodeDepartemen = model.KodeDepartemen.Trim().ToUpperInvariant();
            departemen.NamaDepartemen = model.NamaDepartemen.Trim();
            departemen.Deskripsi = model.Deskripsi?.Trim();
            departemen.KepalaDepartemenId = model.KepalaDepartemenId;
            departemen.StatusAktif = model.StatusAktif;

            await _context.SaveChangesAsync();
            await _layananAudit.CatatAsync("UPDATE_DEPARTEMEN", "Departemen", departemen.Id.ToString(), null, $"Pembaruan departemen {departemen.NamaDepartemen}");
        }
        else
        {
            // Create
            var departemen = new Departemen
            {
                Id = Guid.NewGuid(),
                KodeDepartemen = model.KodeDepartemen.Trim().ToUpperInvariant(),
                NamaDepartemen = model.NamaDepartemen.Trim(),
                Deskripsi = model.Deskripsi?.Trim(),
                KepalaDepartemenId = model.KepalaDepartemenId,
                StatusAktif = model.StatusAktif
            };

            await _context.Departemen.AddAsync(departemen);
            await _context.SaveChangesAsync();
            await _layananAudit.CatatAsync("TAMBAH_DEPARTEMEN", "Departemen", departemen.Id.ToString(), null, $"Penambahan departemen {departemen.NamaDepartemen}");
        }

        return (true, null);
    }

    public async Task<(bool Sukses, string? PesanError)> HapusAsync(Guid id)
    {
        var departemen = await _context.Departemen
            .Include(d => d.DaftarKaryawan)
            .FirstOrDefaultAsync(d => d.Id == id);

        if (departemen == null) return (false, "Departemen tidak ditemukan.");

        if (departemen.DaftarKaryawan.Any(k => !k.IsDeleted))
        {
            return (false, "Departemen tidak dapat dihapus karena masih memiliki data karyawan yang terdaftar.");
        }

        _context.Departemen.Remove(departemen); // Akan dialihkan ke Soft Delete oleh KonteksAplikasi
        await _context.SaveChangesAsync();
        await _layananAudit.CatatAsync("HAPUS_DEPARTEMEN", "Departemen", departemen.Id.ToString(), null, $"Penghapusan departemen {departemen.NamaDepartemen}");

        return (true, null);
    }
}
