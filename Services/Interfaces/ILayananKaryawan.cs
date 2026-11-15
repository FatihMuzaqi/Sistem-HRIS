using Microsoft.AspNetCore.Http;
using SistemHRD.Models.Entitas;
using SistemHRD.Models.Enums;
using SistemHRD.ViewModels;

namespace SistemHRD.Services.Interfaces;

public interface ILayananKaryawan
{
    Task<KaryawanListViewModel> DapatkanDaftarAsync(string? cari, Guid? departemenId, Guid? jabatanId, StatusKaryawan? status, int halaman, int ukuranHalaman);
    Task<Karyawan?> DapatkanBerdasarkanIdAsync(Guid id);
    Task<KaryawanDetailViewModel?> DapatkanDetailAsync(Guid id);
    Task<(bool Sukses, string? PesanError)> SimpanKaryawanAsync(KaryawanFormViewModel model, IFormFile? berkasFoto);
    Task<(bool Sukses, string? PesanError)> UbahStatusAktifAsync(Guid id);

    Task<(bool Sukses, string? PesanError)> TambahKontrakAsync(KontrakKerjaFormViewModel model);
    Task<(bool Sukses, string? PesanError)> TambahKontakDaruratAsync(KontakDaruratFormViewModel model);
    Task<(bool Sukses, string? PesanError)> TambahRiwayatPekerjaanAsync(RiwayatPekerjaanFormViewModel model);

    Task<(bool Sukses, string? PesanError)> UploadDokumenAsync(DokumenUploadViewModel model);
    Task<DokumenKaryawan?> DapatkanDokumenAsync(Guid id);
    Task<(bool Sukses, string? PesanError)> HapusDokumenAsync(Guid id);
}
