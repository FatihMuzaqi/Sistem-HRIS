using Microsoft.AspNetCore.Http;
using SistemHRD.Models.Entitas;
using SistemHRD.ViewModels;

namespace SistemHRD.Services.Interfaces;

public interface ILayananCuti
{
    // Jenis Cuti
    Task<List<JenisCuti>> DapatkanDaftarJenisCutiAsync();
    Task<JenisCuti?> DapatkanJenisCutiByIdAsync(Guid id);
    Task<(bool Sukses, string Pesan)> SimpanJenisCutiAsync(JenisCutiViewModel model);
    Task<(bool Sukses, string Pesan)> HapusJenisCutiAsync(Guid id);

    // Saldo Cuti
    Task<List<SaldoCutiViewModel>> DapatkanDaftarSaldoCutiAsync(int tahun, Guid? departemenId, Guid? karyawanId);
    Task<SaldoCuti?> DapatkanSaldoCutiKaryawanAsync(Guid karyawanId, Guid jenisCutiId, int tahun);
    Task<(bool Sukses, string Pesan)> AturSaldoCutiAsync(AturSaldoCutiViewModel model);
    Task<(bool Sukses, string Pesan)> InisialisasiSaldoCutiTahunanMassalAsync(int tahun);

    // Pengajuan Cuti
    Task<(bool Sukses, string Pesan, Guid? PengajuanId)> AjukanCutiAsync(Guid karyawanId, FormPengajuanCutiViewModel model, IFormFile? berkasDokumen);
    Task<DaftarPengajuanCutiViewModel> DapatkanDaftarPengajuanCutiAsync(FilterPengajuanCutiViewModel filter, Guid? karyawanId = null);
    Task<PengajuanCutiDetailViewModel?> DapatkanDetailPengajuanCutiAsync(Guid id);

    // Persetujuan Cuti (Alur 2 Tahap: Manager -> HRD)
    Task<(bool Sukses, string Pesan)> ProsesPersetujuanManagerAsync(Guid pengajuanCutiId, bool disetujui, string? catatan, Guid userId);
    Task<(bool Sukses, string Pesan)> ProsesVerifikasiHRDAsync(Guid pengajuanCutiId, bool disetujui, string? catatan, Guid userId);

    // Unduh Dokumen Pendukung Cuti
    Task<(byte[]? Konten, string? TipeKonten, string? NamaFile)> UnduhDokumenCutiAsync(Guid pengajuanCutiId);
}
