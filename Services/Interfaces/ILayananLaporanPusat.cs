using SistemHRD.Models.Enums;
using SistemHRD.ViewModels;

namespace SistemHRD.Services.Interfaces;

public interface ILayananLaporanPusat
{
    Task<LaporanHubViewModel> DapatkanRingkasanEksekutifAsync();

    // 1. Karyawan & Departemen
    Task<LaporanKaryawanViewModel> DapatkanLaporanKaryawanAsync(Guid? departemenId, StatusKaryawan? status, string? kataKunci, int halaman = 1, int ukuranHalaman = 15);
    Task<LaporanDistribusiDepartemenViewModel> DapatkanLaporanDepartemenAsync();

    // 2. Kehadiran, Keterlambatan, & Lembur
    Task<LaporanKehadiranViewModel> DapatkanLaporanKehadiranAsync(DateTime tanggalMulai, DateTime tanggalSelesai, Guid? departemenId, int halaman = 1, int ukuranHalaman = 15);
    Task<LaporanKeterlambatanViewModel> DapatkanLaporanKeterlambatanAsync(DateTime tanggalMulai, DateTime tanggalSelesai, Guid? departemenId, int halaman = 1, int ukuranHalaman = 15);
    Task<LaporanLemburViewModel> DapatkanLaporanLemburAsync(DateTime tanggalMulai, DateTime tanggalSelesai, Guid? departemenId, int halaman = 1, int ukuranHalaman = 15);

    // 3. Cuti
    Task<LaporanCutiViewModel> DapatkanLaporanCutiAsync(int tahun, Guid? departemenId, Guid? jenisCutiId, int halaman = 1, int ukuranHalaman = 15);

    // 4. Penggajian
    Task<LaporanPenggajianViewModel> DapatkanLaporanPenggajianAsync(int tahun, int? bulan, Guid? departemenId, int halaman = 1, int ukuranHalaman = 15);

    // 5. Rekrutmen
    Task<LaporanRekrutmenViewModel> DapatkanLaporanRekrutmenAsync(DateTime? tanggalMulai, DateTime? tanggalSelesai, Guid? lowonganId, int halaman = 1, int ukuranHalaman = 15);

    // 6. Kinerja
    Task<LaporanKinerjaPusatViewModel> DapatkanLaporanKinerjaAsync(Guid? periodeId, Guid? departemenId, int halaman = 1, int ukuranHalaman = 15);

    // 7. Pelatihan
    Task<LaporanPelatihanPusatViewModel> DapatkanLaporanPelatihanAsync(DateTime? tanggalMulai, DateTime? tanggalSelesai, Guid? departemenId, int halaman = 1, int ukuranHalaman = 15);

    // 8. Aset
    Task<LaporanAsetPusatViewModel> DapatkanLaporanAsetAsync(KategoriAset? kategori, StatusAset? status, KondisiAset? kondisi, int halaman = 1, int ukuranHalaman = 15);

    // Helper Ekspor CSV
    byte[] EksporKaryawanCsv(List<BarisLaporanKaryawanViewModel> data);
    byte[] EksporKehadiranCsv(List<BarisLaporanKehadiranViewModel> data);
    byte[] EksporPenggajianCsv(List<BarisLaporanPenggajianViewModel> data);
    byte[] EksporAsetCsv(List<BarisLaporanAsetViewModel> data);
}
