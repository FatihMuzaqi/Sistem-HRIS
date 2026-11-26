using SistemHRD.Models.Entitas;
using SistemHRD.Models.Enums;
using SistemHRD.ViewModels;

namespace SistemHRD.Services.Interfaces;

public interface ILayananPelatihan
{
    Task<DashboardPelatihanViewModel> DapatkanDashboardAsync();
    Task<List<ProgramPelatihanItemViewModel>> DapatkanSemuaProgramAsync(StatusPelatihan? status = null);
    Task<ProgramPelatihan?> DapatkanProgramByIdAsync(Guid id);
    Task<DetailProgramPelatihanViewModel?> DapatkanDetailProgramAsync(Guid id);
    Task<(bool Sukses, string Pesan, Guid? Id)> SimpanProgramAsync(FormProgramPelatihanViewModel model);
    Task<(bool Sukses, string Pesan)> HapusProgramAsync(Guid id);

    Task<(bool Sukses, string Pesan)> DaftarkanPesertaAsync(FormDaftarPesertaViewModel model);
    Task<(bool Sukses, string Pesan, int JumlahBerhasil)> DaftarkanPesertaMassalAsync(FormDaftarPesertaMassalViewModel model);
    Task<(bool Sukses, string Pesan)> PerbaruiStatusPesertaAsync(FormUpdateStatusPesertaViewModel model);
    Task<(bool Sukses, string Pesan)> HapusPesertaAsync(Guid pesertaId);

    Task<(bool Sukses, string Pesan, Guid? SertifikatId)> TerbitkanSertifikatAsync(Guid pesertaId, string penerbit, DateTime? tglKedaluwarsa, string? catatan);
    Task<LembarSertifikatViewModel?> DapatkanSertifikatByIdAsync(Guid sertifikatId);
    Task<LembarSertifikatViewModel?> DapatkanSertifikatByPesertaIdAsync(Guid pesertaId);
    Task<List<PesertaPelatihanItemViewModel>> DapatkanRiwayatPelatihanKaryawanAsync(Guid karyawanId);
}
