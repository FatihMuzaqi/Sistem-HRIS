using SistemHRD.Models.Entitas;
using SistemHRD.ViewModels;

namespace SistemHRD.Services.Interfaces;

public interface ILayananJabatan
{
    Task<List<Jabatan>> DapatkanSemuaAsync();
    Task<List<Jabatan>> DapatkanBerdasarkanDepartemenAsync(Guid departemenId);
    Task<Jabatan?> DapatkanBerdasarkanIdAsync(Guid id);
    Task<(bool Sukses, string? PesanError)> SimpanAsync(JabatanFormViewModel model);
    Task<(bool Sukses, string? PesanError)> HapusAsync(Guid id);
}
