using SistemHRD.Models.Entitas;
using SistemHRD.ViewModels;

namespace SistemHRD.Services.Interfaces;

public interface ILayananDepartemen
{
    Task<List<Departemen>> DapatkanSemuaAsync();
    Task<Departemen?> DapatkanBerdasarkanIdAsync(Guid id);
    Task<(bool Sukses, string? PesanError)> SimpanAsync(DepartemenFormViewModel model);
    Task<(bool Sukses, string? PesanError)> HapusAsync(Guid id);
}
