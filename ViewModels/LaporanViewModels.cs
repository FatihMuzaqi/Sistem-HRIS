using SistemHRD.Models.Enums;

namespace SistemHRD.ViewModels;

#region Hub Laporan
public class LaporanHubViewModel
{
    public int TotalKaryawanAktif { get; set; }
    public decimal PersentaseKehadiranBulanIni { get; set; }
    public decimal TotalBebanPayrollBulanIni { get; set; }
    public int TotalLowonganAktif { get; set; }
    public decimal RataRataSkorKinerja { get; set; }
    public int TotalPelatihanBerjalan { get; set; }
    public int TotalAsetTercatat { get; set; }
    public decimal TotalNilaiAset { get; set; }
}
#endregion

#region Laporan Karyawan & Departemen
public class LaporanKaryawanViewModel
{
    public Guid? DepartemenIdTerpilih { get; set; }
    public StatusKaryawan? StatusTerpilih { get; set; }
    public string? KataKunci { get; set; }

    public List<BarisLaporanKaryawanViewModel> DataKaryawan { get; set; } = new();

    public int HalamanSaatIni { get; set; } = 1;
    public int TotalHalaman { get; set; } = 1;
    public int TotalData { get; set; }
    public int UkuranHalaman { get; set; } = 15;

    public Dictionary<string, int> StatistikDepartemen { get; set; } = new();
    public Dictionary<string, int> StatistikStatus { get; set; } = new();
}

public class BarisLaporanKaryawanViewModel
{
    public string NIK { get; set; } = string.Empty;
    public string NamaLengkap { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string NomorTelepon { get; set; } = string.Empty;
    public string NamaDepartemen { get; set; } = "-";
    public string NamaJabatan { get; set; } = "-";
    public string JenisKelamin { get; set; } = "-";
    public DateTime TanggalMasuk { get; set; }
    public StatusKaryawan Status { get; set; }
}

public class LaporanDistribusiDepartemenViewModel
{
    public List<BarisLaporanDepartemenViewModel> RekapDepartemen { get; set; } = new();
    public int TotalSeluruhKaryawan { get; set; }
}

public class BarisLaporanDepartemenViewModel
{
    public Guid DepartemenId { get; set; }
    public string KodeDepartemen { get; set; } = string.Empty;
    public string NamaDepartemen { get; set; } = string.Empty;
    public int TotalKaryawan { get; set; }
    public int TotalKaryawanTetap { get; set; }
    public int TotalKaryawanKontrak { get; set; }
    public int TotalLakiLaki { get; set; }
    public int TotalPerempuan { get; set; }
}
#endregion

#region Laporan Kehadiran, Keterlambatan, & Lembur
public class LaporanKehadiranViewModel
{
    public DateTime TanggalMulai { get; set; }
    public DateTime TanggalSelesai { get; set; }
    public Guid? DepartemenIdTerpilih { get; set; }

    public List<BarisLaporanKehadiranViewModel> DataKehadiran { get; set; } = new();

    public int HalamanSaatIni { get; set; } = 1;
    public int TotalHalaman { get; set; } = 1;
    public int TotalData { get; set; }

    public int TotalHadir { get; set; }
    public int TotalIzin { get; set; }
    public int TotalSakit { get; set; }
    public int TotalAlpa { get; set; }
    public int TotalTerlambat { get; set; }
}

public class BarisLaporanKehadiranViewModel
{
    public DateTime Tanggal { get; set; }
    public string NIK { get; set; } = string.Empty;
    public string NamaKaryawan { get; set; } = string.Empty;
    public string NamaDepartemen { get; set; } = string.Empty;
    public TimeSpan? JamMasuk { get; set; }
    public TimeSpan? JamKeluar { get; set; }
    public string StatusKehadiran { get; set; } = string.Empty;
    public int MenitKeterlambatan { get; set; }
    public decimal TotalJamKerja { get; set; }
}

public class LaporanKeterlambatanViewModel
{
    public DateTime TanggalMulai { get; set; }
    public DateTime TanggalSelesai { get; set; }
    public Guid? DepartemenIdTerpilih { get; set; }

    public List<BarisLaporanKeterlambatanViewModel> DataKeterlambatan { get; set; } = new();

    public int HalamanSaatIni { get; set; } = 1;
    public int TotalHalaman { get; set; } = 1;
    public int TotalData { get; set; }

    public int TotalKasusKeterlambatan { get; set; }
    public int TotalAkumulasiMenitTerlambat { get; set; }
}

public class BarisLaporanKeterlambatanViewModel
{
    public DateTime Tanggal { get; set; }
    public string NIK { get; set; } = string.Empty;
    public string NamaKaryawan { get; set; } = string.Empty;
    public string NamaDepartemen { get; set; } = string.Empty;
    public TimeSpan? JamJadwalMasuk { get; set; }
    public TimeSpan? JamRealisasiMasuk { get; set; }
    public int MenitTerlambat { get; set; }
    public decimal EstimasiDenda { get; set; }
}

public class LaporanLemburViewModel
{
    public DateTime TanggalMulai { get; set; }
    public DateTime TanggalSelesai { get; set; }
    public Guid? DepartemenIdTerpilih { get; set; }

    public List<BarisLaporanLemburViewModel> DataLembur { get; set; } = new();

    public int HalamanSaatIni { get; set; } = 1;
    public int TotalHalaman { get; set; } = 1;
    public int TotalData { get; set; }

    public decimal TotalJamLemburDisetujui { get; set; }
    public decimal TotalEstimasiUangLembur { get; set; }
}

public class BarisLaporanLemburViewModel
{
    public DateTime Tanggal { get; set; }
    public string NIK { get; set; } = string.Empty;
    public string NamaKaryawan { get; set; } = string.Empty;
    public string NamaDepartemen { get; set; } = string.Empty;
    public TimeSpan JamMulai { get; set; }
    public TimeSpan JamSelesai { get; set; }
    public decimal DurasiJam { get; set; }
    public string AlasanLembur { get; set; } = string.Empty;
    public string StatusPersetujuan { get; set; } = string.Empty;
}
#endregion

#region Laporan Cuti
public class LaporanCutiViewModel
{
    public int TahunTerpilih { get; set; }
    public Guid? DepartemenIdTerpilih { get; set; }
    public Guid? JenisCutiIdTerpilih { get; set; }

    public List<BarisLaporanCutiViewModel> DataCuti { get; set; } = new();

    public int HalamanSaatIni { get; set; } = 1;
    public int TotalHalaman { get; set; } = 1;
    public int TotalData { get; set; }

    public int TotalHariCutiDisetujui { get; set; }
    public Dictionary<string, int> RekapPerJenisCuti { get; set; } = new();
}

public class BarisLaporanCutiViewModel
{
    public string NomorPengajuan { get; set; } = string.Empty;
    public string NIK { get; set; } = string.Empty;
    public string NamaKaryawan { get; set; } = string.Empty;
    public string NamaDepartemen { get; set; } = string.Empty;
    public string JenisCuti { get; set; } = string.Empty;
    public DateTime TanggalMulai { get; set; }
    public DateTime TanggalSelesai { get; set; }
    public int DurasiHari { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Alasan { get; set; } = string.Empty;
}
#endregion

#region Laporan Penggajian
public class LaporanPenggajianViewModel
{
    public int TahunTerpilih { get; set; }
    public int? BulanTerpilih { get; set; }
    public Guid? DepartemenIdTerpilih { get; set; }

    public List<BarisLaporanPenggajianViewModel> DataPenggajian { get; set; } = new();

    public int HalamanSaatIni { get; set; } = 1;
    public int TotalHalaman { get; set; } = 1;
    public int TotalData { get; set; }

    public decimal TotalGajiPokok { get; set; }
    public decimal TotalTunjangan { get; set; }
    public decimal TotalLembur { get; set; }
    public decimal TotalPotongan { get; set; }
    public decimal TotalGajiBersih { get; set; }
    public decimal TotalBebanPerusahaan { get; set; }
}

public class BarisLaporanPenggajianViewModel
{
    public string PeriodeNama { get; set; } = string.Empty;
    public string NIK { get; set; } = string.Empty;
    public string NamaKaryawan { get; set; } = string.Empty;
    public string NamaDepartemen { get; set; } = string.Empty;
    public string NamaJabatan { get; set; } = string.Empty;
    public decimal GajiPokok { get; set; }
    public decimal TotalTunjangan { get; set; }
    public decimal UangLembur { get; set; }
    public decimal PotonganBpjsPph { get; set; }
    public decimal GajiBersih { get; set; }
    public string Status { get; set; } = string.Empty;
}
#endregion

#region Laporan Rekrutmen & Kinerja
public class LaporanRekrutmenViewModel
{
    public DateTime? TanggalMulai { get; set; }
    public DateTime? TanggalSelesai { get; set; }
    public Guid? LowonganIdTerpilih { get; set; }

    public List<BarisLaporanRekrutmenViewModel> DataRekrutmen { get; set; } = new();

    public int HalamanSaatIni { get; set; } = 1;
    public int TotalHalaman { get; set; } = 1;
    public int TotalData { get; set; }

    public int TotalPelamarMasuk { get; set; }
    public int TotalLolosWawancara { get; set; }
    public int TotalKandidatDiterima { get; set; }
    public Dictionary<string, int> RekapStatusPelamar { get; set; } = new();
}

public class BarisLaporanRekrutmenViewModel
{
    public string NomorRegistrasi { get; set; } = string.Empty;
    public string NamaPelamar { get; set; } = string.Empty;
    public string PosisiLowongan { get; set; } = string.Empty;
    public string Pendidikan { get; set; } = string.Empty;
    public decimal PengalamanTahun { get; set; }
    public DateTime TanggalMelamar { get; set; }
    public StatusPelamar Status { get; set; }
}

public class LaporanKinerjaPusatViewModel
{
    public Guid? PeriodeIdTerpilih { get; set; }
    public Guid? DepartemenIdTerpilih { get; set; }

    public List<BarisLaporanKinerjaViewModel> DataKinerja { get; set; } = new();

    public int HalamanSaatIni { get; set; } = 1;
    public int TotalHalaman { get; set; } = 1;
    public int TotalData { get; set; }

    public decimal RataRataSkorPerusahaan { get; set; }
    public Dictionary<string, int> DistribusiPredikat { get; set; } = new();
    public Dictionary<string, decimal> RataRataSkorPerDepartemen { get; set; } = new();
}

public class BarisLaporanKinerjaViewModel
{
    public string NamaPeriode { get; set; } = string.Empty;
    public string NIK { get; set; } = string.Empty;
    public string NamaKaryawan { get; set; } = string.Empty;
    public string NamaDepartemen { get; set; } = string.Empty;
    public string NamaPenilai { get; set; } = string.Empty;
    public DateTime TanggalPenilaian { get; set; }
    public decimal TotalSkorAkhir { get; set; }
    public KategoriKinerja PredikatKinerja { get; set; }
}
#endregion

#region Laporan Pelatihan & Aset
public class LaporanPelatihanPusatViewModel
{
    public DateTime? TanggalMulai { get; set; }
    public DateTime? TanggalSelesai { get; set; }
    public Guid? DepartemenIdTerpilih { get; set; }

    public List<BarisLaporanPelatihanViewModel> DataPelatihan { get; set; } = new();

    public int HalamanSaatIni { get; set; } = 1;
    public int TotalHalaman { get; set; } = 1;
    public int TotalData { get; set; }

    public int TotalProgram { get; set; }
    public int TotalPeserta { get; set; }
    public int TotalPesertaLulus { get; set; }
    public decimal TotalBiayaPelatihan { get; set; }
}

public class BarisLaporanPelatihanViewModel
{
    public string KodeProgram { get; set; } = string.Empty;
    public string NamaProgram { get; set; } = string.Empty;
    public string Penyelenggara { get; set; } = string.Empty;
    public MetodePelatihan Metode { get; set; }
    public DateTime TanggalMulai { get; set; }
    public DateTime TanggalSelesai { get; set; }
    public int JumlahPeserta { get; set; }
    public int JumlahLulus { get; set; }
    public StatusPelatihan Status { get; set; }
}

public class LaporanAsetPusatViewModel
{
    public KategoriAset? KategoriTerpilih { get; set; }
    public StatusAset? StatusTerpilih { get; set; }
    public KondisiAset? KondisiTerpilih { get; set; }

    public List<BarisLaporanAsetViewModel> DataAset { get; set; } = new();

    public int HalamanSaatIni { get; set; } = 1;
    public int TotalHalaman { get; set; } = 1;
    public int TotalData { get; set; }

    public int TotalUnitAset { get; set; }
    public decimal TotalNilaiInventaris { get; set; }
    public Dictionary<string, int> RekapKondisi { get; set; } = new();
    public Dictionary<string, int> RekapKategori { get; set; } = new();
}

public class BarisLaporanAsetViewModel
{
    public string NomorAset { get; set; } = string.Empty;
    public string NamaAset { get; set; } = string.Empty;
    public KategoriAset Kategori { get; set; }
    public string? MerkModel { get; set; }
    public DateTime TanggalBeli { get; set; }
    public decimal NilaiPerolehan { get; set; }
    public KondisiAset Kondisi { get; set; }
    public StatusAset Status { get; set; }
    public string? NamaPemegang { get; set; }
    public string? DepartemenPemegang { get; set; }
}
#endregion
