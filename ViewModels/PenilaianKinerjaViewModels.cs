using System.ComponentModel.DataAnnotations;
using SistemHRD.Models.Enums;

namespace SistemHRD.ViewModels;

#region Dashboard Kinerja
public class DashboardKinerjaViewModel
{
    public int TotalPeriodeAktif { get; set; }
    public int TotalKaryawanTelahDinilai { get; set; }
    public decimal RataRataSkorPerusahaan { get; set; }
    public int TotalIndikatorKPIAktif { get; set; }

    public Dictionary<string, int> DistribusiPredikatKinerja { get; set; } = new();
    public Dictionary<string, decimal> RataRataSkorPerDepartemen { get; set; } = new();

    public List<PeriodePenilaianItemViewModel> PeriodeAktif { get; set; } = new();
    public List<PenilaianKinerjaItemViewModel> PenilaianTerbaru { get; set; } = new();
}
#endregion

#region Periode Penilaian ViewModels
public class PeriodePenilaianItemViewModel
{
    public Guid Id { get; set; }
    public string NamaPeriode { get; set; } = string.Empty;
    public int Tahun { get; set; }
    public string? SemesterAtauKuartal { get; set; }
    public DateTime TanggalMulai { get; set; }
    public DateTime TanggalSelesai { get; set; }
    public StatusPeriodePenilaian Status { get; set; }
    public string? Catatan { get; set; }
    public int TotalKaryawanDinilai { get; set; }
    public decimal RataRataSkor { get; set; }
}

public class FormPeriodePenilaianViewModel
{
    public Guid? Id { get; set; }

    [Required(ErrorMessage = "Nama periode penilaian wajib diisi")]
    [MaxLength(100)]
    public string NamaPeriode { get; set; } = string.Empty;

    [Range(2020, 2100, ErrorMessage = "Tahun tidak valid")]
    public int Tahun { get; set; } = DateTime.Today.Year;

    [MaxLength(50)]
    public string? SemesterAtauKuartal { get; set; } = "Tahunan";

    [Required(ErrorMessage = "Tanggal mulai wajib diisi")]
    public DateTime TanggalMulai { get; set; } = DateTime.Today;

    [Required(ErrorMessage = "Tanggal selesai wajib diisi")]
    public DateTime TanggalSelesai { get; set; } = DateTime.Today.AddMonths(1);

    public StatusPeriodePenilaian Status { get; set; } = StatusPeriodePenilaian.Aktif;

    [MaxLength(500)]
    public string? Catatan { get; set; }
}
#endregion

#region Indikator KPI ViewModels
public class IndikatorKPIItemViewModel
{
    public Guid Id { get; set; }
    public Guid? DepartemenId { get; set; }
    public string NamaDepartemen { get; set; } = "Semua Departemen (Umum)";
    public Guid? JabatanId { get; set; }
    public string NamaJabatan { get; set; } = "Semua Jabatan";
    public string KodeIndikator { get; set; } = string.Empty;
    public string NamaIndikator { get; set; } = string.Empty;
    public string? Deskripsi { get; set; }
    public JenisIndikatorKPI Jenis { get; set; }
    public decimal? TargetKuantitatif { get; set; }
    public string? Satuan { get; set; }
    public decimal BobotBawaan { get; set; }
    public bool StatusAktif { get; set; }
}

public class FormIndikatorKPIViewModel
{
    public Guid? Id { get; set; }

    public Guid? DepartemenId { get; set; }
    public Guid? JabatanId { get; set; }

    [Required(ErrorMessage = "Kode indikator wajib diisi")]
    [MaxLength(50)]
    public string KodeIndikator { get; set; } = string.Empty;

    [Required(ErrorMessage = "Nama indikator KPI wajib diisi")]
    [MaxLength(150)]
    public string NamaIndikator { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Deskripsi { get; set; }

    public JenisIndikatorKPI Jenis { get; set; } = JenisIndikatorKPI.Kuantitatif;

    public decimal? TargetKuantitatif { get; set; } = 100;

    [MaxLength(50)]
    public string? Satuan { get; set; } = "%";

    [Required(ErrorMessage = "Bobot persentase wajib diisi")]
    [Range(1, 100, ErrorMessage = "Bobot antara 1% s/d 100%")]
    public decimal BobotBawaan { get; set; } = 20.00m;

    public bool StatusAktif { get; set; } = true;
}
#endregion

#region Penilaian Kinerja ViewModels
public class PenilaianKinerjaItemViewModel
{
    public Guid Id { get; set; }
    public Guid PeriodePenilaianId { get; set; }
    public string NamaPeriode { get; set; } = string.Empty;
    public Guid KaryawanId { get; set; }
    public string NIK { get; set; } = string.Empty;
    public string NamaKaryawan { get; set; } = string.Empty;
    public string NamaDepartemen { get; set; } = "-";
    public string NamaJabatan { get; set; } = "-";
    public string NamaPenilai { get; set; } = string.Empty;
    public DateTime TanggalPenilaian { get; set; }
    public decimal TotalSkorAkhir { get; set; }
    public KategoriKinerja PredikatKinerja { get; set; }
    public StatusPenilaianKinerja Status { get; set; }
}

public class FormInputPenilaianKinerjaViewModel
{
    public Guid? Id { get; set; }

    [Required(ErrorMessage = "Periode penilaian wajib dipilih")]
    public Guid PeriodePenilaianId { get; set; }
    public string? NamaPeriode { get; set; }

    [Required(ErrorMessage = "Karyawan yang dinilai wajib dipilih")]
    public Guid KaryawanId { get; set; }
    public string? NamaKaryawan { get; set; }
    public string? NIK { get; set; }
    public string? NamaDepartemen { get; set; }
    public string? NamaJabatan { get; set; }

    [Required(ErrorMessage = "Tanggal penilaian wajib diisi")]
    public DateTime TanggalPenilaian { get; set; } = DateTime.Today;

    [Required(ErrorMessage = "Komentar evaluasi wajib diisi")]
    public string KomentarEvaluasi { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? RekomendasiPengembangan { get; set; }

    public StatusPenilaianKinerja Status { get; set; } = StatusPenilaianKinerja.Disetujui;

    // Rincian Butir Indikator Penilaian
    public List<ItemInputDetailPenilaianViewModel> DaftarDetail { get; set; } = new();
}

public class ItemInputDetailPenilaianViewModel
{
    public Guid IndikatorKPIId { get; set; }
    public string NamaIndikator { get; set; } = string.Empty;
    public string? Target { get; set; }
    public string? Satuan { get; set; }
    public decimal BobotPersen { get; set; }
    public string? Realisasi { get; set; }
    public decimal SkorIndikator { get; set; }
    public string? CatatanIndikator { get; set; }
}

public class DetailPenilaianKinerjaViewModel
{
    public Guid Id { get; set; }
    public Guid PeriodePenilaianId { get; set; }
    public string NamaPeriode { get; set; } = string.Empty;
    public int TahunPeriode { get; set; }
    public Guid KaryawanId { get; set; }
    public string NIK { get; set; } = string.Empty;
    public string NamaKaryawan { get; set; } = string.Empty;
    public string NamaDepartemen { get; set; } = "-";
    public string NamaJabatan { get; set; } = "-";
    public string NamaPenilai { get; set; } = string.Empty;
    public DateTime TanggalPenilaian { get; set; }
    public decimal TotalSkorAkhir { get; set; }
    public KategoriKinerja PredikatKinerja { get; set; }
    public string KomentarEvaluasi { get; set; } = string.Empty;
    public string? RekomendasiPengembangan { get; set; }
    public StatusPenilaianKinerja Status { get; set; }

    public List<ItemDetailPenilaianTampilViewModel> DaftarDetail { get; set; } = new();
}

public class ItemDetailPenilaianTampilViewModel
{
    public string NamaIndikator { get; set; } = string.Empty;
    public string? Target { get; set; }
    public string? Realisasi { get; set; }
    public decimal BobotPersen { get; set; }
    public decimal SkorIndikator { get; set; }
    public decimal SkorTertimbang { get; set; }
    public string? CatatanIndikator { get; set; }
}

public class LaporanPerformaKaryawanViewModel
{
    public Guid? KaryawanIdTerpilih { get; set; }
    public Guid? DepartemenIdTerpilih { get; set; }
    public int? TahunTerpilih { get; set; }

    public List<PenilaianKinerjaItemViewModel> DaftarPenilaian { get; set; } = new();
    public decimal RataRataSkorTotal { get; set; }
    public int TotalPenilaian { get; set; }
}
#endregion
