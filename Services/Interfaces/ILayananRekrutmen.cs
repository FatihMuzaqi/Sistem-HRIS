using SistemHRD.Models.Enums;
using SistemHRD.ViewModels;

namespace SistemHRD.Services.Interfaces;

public interface ILayananRekrutmen
{
    Task<DashboardRekrutmenViewModel> DapatkanDashboardAsync();

    // Lowongan Kerja
    Task<List<LowonganKerjaItemViewModel>> DapatkanSemuaLowonganAsync(StatusLowongan? status = null);
    Task<DetailLowonganKerjaViewModel?> DapatkanDetailLowonganAsync(Guid id);
    Task<(bool Sukses, string Pesan, Guid? Id)> SimpanLowonganAsync(FormLowonganKerjaViewModel model);
    Task<(bool Sukses, string Pesan)> HapusLowonganAsync(Guid id);

    // Pelamar
    Task<List<PelamarItemViewModel>> DapatkanSemuaPelamarAsync(Guid? lowonganId = null, StatusPelamar? status = null);
    Task<DetailPelamarViewModel?> DapatkanDetailPelamarAsync(Guid id);
    Task<(bool Sukses, string Pesan, Guid? Id)> DaftarPelamarAsync(FormPelamarViewModel model);
    Task<(bool Sukses, string Pesan)> ProsesSeleksiAdministrasiAsync(FormSeleksiAdministrasiViewModel model);
    Task<(bool Sukses, string Pesan)> UbahStatusPelamarAsync(Guid pelamarId, StatusPelamar statusBaru, string? catatan = null);
    Task<(byte[]? FileBytes, string ContentType, string FileName)> UnduhCVAsync(Guid pelamarId);

    // Wawancara & Hasil Seleksi
    Task<(bool Sukses, string Pesan, Guid? Id)> SimpanJadwalWawancaraAsync(FormJadwalWawancaraViewModel model);
    Task<(bool Sukses, string Pesan, Guid? Id)> SimpanHasilSeleksiAsync(FormHasilSeleksiViewModel model, Guid penilaiId, string namaPenilai);

    // Penerimaan Kandidat (Konversi jadi Karyawan)
    Task<(bool Sukses, string Pesan, Guid? KaryawanId)> TerimaDanKonversiKaryawanAsync(FormTerimaPelamarViewModel model);
}
