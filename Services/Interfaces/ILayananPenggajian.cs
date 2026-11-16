using SistemHRD.Models.Entitas;
using SistemHRD.ViewModels;

namespace SistemHRD.Services.Interfaces;

public interface ILayananPenggajian
{
    // Master Komponen Gaji
    Task<List<KomponenGajiViewModel>> DapatkanDaftarKomponenAsync();
    Task<KomponenGaji?> DapatkanKomponenByIdAsync(Guid id);
    Task<(bool Sukses, string Pesan)> SimpanKomponenAsync(KomponenGajiViewModel model);
    Task<(bool Sukses, string Pesan)> HapusKomponenAsync(Guid id);

    // Pengaturan Gaji Karyawan
    Task<AturGajiKaryawanViewModel?> DapatkanAturGajiKaryawanAsync(Guid karyawanId);
    Task<(bool Sukses, string Pesan)> SimpanGajiKaryawanAsync(FormSimpanGajiKaryawanViewModel model);

    // Kebijakan Penggajian Perusahaan
    Task<KebijakanPenggajianViewModel> DapatkanKebijakanPerusahaanAsync();
    Task<(bool Sukses, string Pesan)> SimpanKebijakanPerusahaanAsync(KebijakanPenggajianViewModel model);

    // Periode Penggajian
    Task<List<PeriodePenggajianItemViewModel>> DapatkanDaftarPeriodeAsync();
    Task<PeriodePenggajian?> DapatkanPeriodeByIdAsync(Guid id);
    Task<DetailPeriodePenggajianViewModel?> DapatkanDetailPeriodeAsync(Guid id);
    Task<(bool Sukses, string Pesan, Guid? PeriodeId)> SimpanPeriodeAsync(FormPeriodePenggajianViewModel model);
    Task<(bool Sukses, string Pesan)> HapusPeriodeAsync(Guid id);

    // Proses Kalkulasi & Approval Payroll
    Task<(bool Sukses, string Pesan, int TotalKaryawanDihitung)> HitungPenggajianBatchAsync(Guid periodeId, Guid? departemenId, Guid userId);
    Task<(bool Sukses, string Pesan)> SetujuiPenggajianPeriodeAsync(Guid periodeId, Guid userId, string? catatan);

    // Detail & Slip Gaji
    Task<RincianPenggajianViewModel?> DapatkanRincianPenggajianAsync(Guid penggajianId);
    Task<SlipGajiViewModel?> DapatkanSlipGajiAsync(Guid penggajianId, Guid? karyawanIdPengguna = null);
    Task<List<ItemPayrollKaryawanViewModel>> DapatkanRiwayatPenggajianKaryawanAsync(Guid karyawanId);

    // Ekspor Data
    Task<(byte[]? FileBytes, string NamaFile)> EksporLaporanPayrollCsvAsync(Guid periodeId);
}
