using SistemHRD.Models.Entitas;
using SistemHRD.ViewModels;

namespace SistemHRD.Services.Interfaces;

public interface ILayananKehadiran
{
    Task<KehadiranHariIniViewModel> DapatkanKehadiranHariIniAsync(Guid karyawanId, DateTime tanggal);
    Task<(bool Sukses, string Pesan, Kehadiran? Data)> CheckInAsync(Guid karyawanId, CheckInRequestViewModel model, string? ipAddress);
    Task<(bool Sukses, string Pesan, Kehadiran? Data)> CheckOutAsync(Guid karyawanId, CheckOutRequestViewModel model, string? ipAddress);
    Task<DaftarKehadiranViewModel> DapatkanDaftarKehadiranAsync(FilterKehadiranViewModel filter);
    Task<RekapKehadiranViewModel> DapatkanRekapitulasiAsync(int bulan, int tahun, Guid? departemenId, Guid? karyawanId);

    // Shift Kerja
    Task<List<ShiftKerja>> DapatkanDaftarShiftAsync();
    Task<ShiftKerja?> DapatkanShiftByIdAsync(Guid id);
    Task<(bool Sukses, string Pesan)> SimpanShiftAsync(ShiftKerjaViewModel model);
    Task<(bool Sukses, string Pesan)> HapusShiftAsync(Guid id);

    // Jadwal Kerja
    Task<List<JadwalKerjaViewModel>> DapatkanJadwalKerjaAsync(DateTime tanggalMulai, DateTime tanggalSelesai, Guid? departemenId, Guid? karyawanId);
    Task<(bool Sukses, string Pesan)> SimpanJadwalKerjaAsync(FormJadwalKerjaViewModel model);

    // Lembur
    Task<(bool Sukses, string Pesan)> AjukanLemburAsync(Guid karyawanId, FormPengajuanLemburViewModel model);
    Task<DaftarLemburViewModel> DapatkanDaftarLemburAsync(FilterLemburViewModel filter, Guid? karyawanId = null);
    Task<(bool Sukses, string Pesan)> ProsesPersetujuanLemburAsync(Guid pengajuanLemburId, bool disetujui, string? catatan, Guid userId);
}
