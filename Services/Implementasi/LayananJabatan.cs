using Microsoft.EntityFrameworkCore;
using SistemHRD.Data;
using SistemHRD.Models.Entitas;
using SistemHRD.Services.Interfaces;
using SistemHRD.ViewModels;

namespace SistemHRD.Services.Implementasi;

public class LayananJabatan : ILayananJabatan
{
    private readonly KonteksAplikasi _context;
    private readonly ILayananAudit _layananAudit;

    public LayananJabatan(KonteksAplikasi context, ILayananAudit layananAudit)
    {
        _context = context;
        _layananAudit = layananAudit;
    }

    public async Task<List<Jabatan>> DapatkanSemuaAsync()
    {
        return await _context.Jabatan
            .Include(j => j.Departemen)
            .Include(j => j.DaftarKaryawan)
            .OrderBy(j => j.Departemen != null ? j.Departemen.NamaDepartemen : "")
            .ThenBy(j => j.LevelJabatan)
            .ToListAsync();
    }

    public async Task<List<Jabatan>> DapatkanBerdasarkanDepartemenAsync(Guid departemenId)
    {
        return await _context.Jabatan
            .Where(j => j.DepartemenId == departemenId && j.StatusAktif)
            .OrderBy(j => j.LevelJabatan)
            .ThenBy(j => j.NamaJabatan)
            .ToListAsync();
    }

    public async Task<Jabatan?> DapatkanBerdasarkanIdAsync(Guid id)
    {
        return await _context.Jabatan
            .Include(j => j.Departemen)
            .FirstOrDefaultAsync(j => j.Id == id);
    }

    public async Task<(bool Sukses, string? PesanError)> SimpanAsync(JabatanFormViewModel model)
    {
        if (model.Id.HasValue && model.Id.Value != Guid.Empty)
        {
            // Update
            var jabatan = await _context.Jabatan.FindAsync(model.Id.Value);
            if (jabatan == null) return (false, "Jabatan tidak ditemukan.");

            jabatan.NamaJabatan = model.NamaJabatan.Trim();
            jabatan.DepartemenId = model.DepartemenId;
            jabatan.Deskripsi = model.Deskripsi?.Trim();
            jabatan.LevelJabatan = model.LevelJabatan;
            jabatan.StatusAktif = model.StatusAktif;

            await _context.SaveChangesAsync();
            await _layananAudit.CatatAsync("UPDATE_JABATAN", "Jabatan", jabatan.Id.ToString(), null, $"Pembaruan jabatan {jabatan.NamaJabatan}");
        }
        else
        {
            // Create
            var jabatan = new Jabatan
            {
                Id = Guid.NewGuid(),
                NamaJabatan = model.NamaJabatan.Trim(),
                DepartemenId = model.DepartemenId,
                Deskripsi = model.Deskripsi?.Trim(),
                LevelJabatan = model.LevelJabatan,
                StatusAktif = model.StatusAktif
            };

            await _context.Jabatan.AddAsync(jabatan);
            await _context.SaveChangesAsync();
            await _layananAudit.CatatAsync("TAMBAH_JABATAN", "Jabatan", jabatan.Id.ToString(), null, $"Penambahan jabatan {jabatan.NamaJabatan}");
        }

        return (true, null);
    }

    public async Task<(bool Sukses, string? PesanError)> HapusAsync(Guid id)
    {
        var jabatan = await _context.Jabatan
            .Include(j => j.DaftarKaryawan)
            .FirstOrDefaultAsync(j => j.Id == id);

        if (jabatan == null) return (false, "Jabatan tidak ditemukan.");

        if (jabatan.DaftarKaryawan.Any(k => !k.IsDeleted))
        {
            return (false, "Jabatan tidak dapat dihapus karena masih digunakan oleh karyawan terdaftar.");
        }

        _context.Jabatan.Remove(jabatan);
        await _context.SaveChangesAsync();
        await _layananAudit.CatatAsync("HAPUS_JABATAN", "Jabatan", jabatan.Id.ToString(), null, $"Penghapusan jabatan {jabatan.NamaJabatan}");

        return (true, null);
    }
}
