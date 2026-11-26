using SistemHRD.Models.Entitas;
using SistemHRD.Models.Enums;
using SistemHRD.ViewModels;

namespace SistemHRD.Services.Interfaces;

public interface ILayananManajemenAset
{
    Task<DashboardAsetViewModel> DapatkanDashboardAsync();
    Task<DaftarAsetPagedViewModel> DapatkanDaftarAsetAsync(
        KategoriAset? kategori = null,
        StatusAset? status = null,
        KondisiAset? kondisi = null,
        string? kataKunci = null,
        int halaman = 1,
        int ukuranHalaman = 10);

    Task<AsetPerusahaan?> DapatkanAsetByIdAsync(Guid id);
    Task<DetailAsetViewModel?> DapatkanDetailAsetAsync(Guid id);
    Task<(bool Sukses, string Pesan, Guid? Id)> SimpanAsetAsync(FormAsetViewModel model);
    Task<(bool Sukses, string Pesan)> HapusAsetAsync(Guid id);

    Task<(bool Sukses, string Pesan)> SerahkanAsetAsync(FormPenyerahanAsetViewModel model, Guid diserahkanOlehId, string namaPetugas);
    Task<(bool Sukses, string Pesan)> KembalikanAsetAsync(FormPengembalianAsetViewModel model, Guid diterimaKembaliOlehId, string namaPetugas);

    Task<List<PenyerahanAsetItemViewModel>> DapatkanAsetKaryawanAktifAsync(Guid karyawanId);
    Task<List<RiwayatAsetItemViewModel>> DapatkanRiwayatAsetAsync(Guid asetId);
}
