using SistemHRD.ViewModels;

namespace SistemHRD.Services.Interfaces;

public interface ILayananPenilaianKinerja
{
    Task<DashboardKinerjaViewModel> DapatkanDashboardAsync();

    // Periode Penilaian
    Task<List<PeriodePenilaianItemViewModel>> DapatkanSemuaPeriodeAsync();
    Task<PeriodePenilaianItemViewModel?> DapatkanPeriodeByIdAsync(Guid id);
    Task<(bool Sukses, string Pesan, Guid? Id)> SimpanPeriodeAsync(FormPeriodePenilaianViewModel model);
    Task<(bool Sukses, string Pesan)> HapusPeriodeAsync(Guid id);

    // Indikator KPI
    Task<List<IndikatorKPIItemViewModel>> DapatkanSemuaIndikatorAsync(Guid? departemenId = null);
    Task<(bool Sukses, string Pesan, Guid? Id)> SimpanIndikatorAsync(FormIndikatorKPIViewModel model);
    Task<(bool Sukses, string Pesan)> HapusIndikatorAsync(Guid id);

    // Penilaian Kinerja
    Task<List<PenilaianKinerjaItemViewModel>> DapatkanSemuaPenilaianAsync(Guid? periodeId = null, Guid? karyawanId = null);
    Task<FormInputPenilaianKinerjaViewModel?> SiapkanFormPenilaianAsync(Guid periodeId, Guid karyawanId);
    Task<DetailPenilaianKinerjaViewModel?> DapatkanDetailPenilaianAsync(Guid id);
    Task<(bool Sukses, string Pesan, Guid? Id)> SimpanPenilaianAsync(FormInputPenilaianKinerjaViewModel model, Guid penilaiId, string namaPenilai);

    // Laporan & Analitik Kinerja
    Task<LaporanPerformaKaryawanViewModel> DapatkanLaporanPerformaAsync(Guid? karyawanId, Guid? departemenId, int? tahun);
}
