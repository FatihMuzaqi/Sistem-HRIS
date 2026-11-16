using System.ComponentModel.DataAnnotations;
using SistemHRD.Models.Enums;

namespace SistemHRD.ViewModels;

public class CheckInRequestViewModel
{
    [MaxLength(255)]
    public string? CatatanMasuk { get; set; }

    [MaxLength(255)]
    public string? LokasiMasuk { get; set; }

    public string? FotoMasuk { get; set; }
}

public class CheckOutRequestViewModel
{
    [MaxLength(255)]
    public string? CatatanPulang { get; set; }

    [MaxLength(255)]
    public string? LokasiPulang { get; set; }
}

public class KehadiranHariIniViewModel
{
    public Guid KaryawanId { get; set; }
    public string NomorIndukKaryawan { get; set; } = string.Empty;
    public string NamaKaryawan { get; set; } = string.Empty;
    public string NamaDepartemen { get; set; } = string.Empty;
    public string NamaJabatan { get; set; } = string.Empty;
    public DateTime Tanggal { get; set; } = DateTime.Today;

    public Guid? ShiftKerjaId { get; set; }
    public string NamaShift { get; set; } = "Shift Pagi (Regular)";
    public TimeSpan JamMasukShift { get; set; } = new TimeSpan(8, 0, 0);
    public TimeSpan JamPulangShift { get; set; } = new TimeSpan(17, 0, 0);
    public int ToleransiMenit { get; set; } = 15;

    public bool SudahCheckIn { get; set; }
    public DateTime? WaktuMasuk { get; set; }
    public StatusKehadiran? Status { get; set; }
    public int MenitKeterlambatan { get; set; }
    public string? LokasiMasuk { get; set; }

    public bool SudahCheckOut { get; set; }
    public DateTime? WaktuPulang { get; set; }
    public int MenitPulangCepat { get; set; }
    public decimal TotalJamKerja { get; set; }
    public string? LokasiPulang { get; set; }

    public bool BisaCheckIn => !SudahCheckIn;
    public bool BisaCheckOut => SudahCheckIn && !SudahCheckOut;
}

public class FilterKehadiranViewModel
{
    public DateTime? TanggalMulai { get; set; }
    public DateTime? TanggalSelesai { get; set; }
    public Guid? DepartemenId { get; set; }
    public Guid? KaryawanId { get; set; }
    public StatusKehadiran? Status { get; set; }
    public string? Cari { get; set; }
    public int Halaman { get; set; } = 1;
    public int UkuranHalaman { get; set; } = 15;
}

public class ItemKehadiranViewModel
{
    public Guid Id { get; set; }
    public Guid KaryawanId { get; set; }
    public string NomorIndukKaryawan { get; set; } = string.Empty;
    public string NamaKaryawan { get; set; } = string.Empty;
    public string NamaDepartemen { get; set; } = string.Empty;
    public string NamaJabatan { get; set; } = string.Empty;
    public DateTime Tanggal { get; set; }
    public string NamaShift { get; set; } = "-";
    public DateTime? WaktuMasuk { get; set; }
    public DateTime? WaktuPulang { get; set; }
    public StatusKehadiran Status { get; set; }
    public int MenitKeterlambatan { get; set; }
    public int MenitPulangCepat { get; set; }
    public decimal TotalJamKerja { get; set; }
    public string? CatatanMasuk { get; set; }
    public string? CatatanPulang { get; set; }
    public string? LokasiMasuk { get; set; }
}

public class DaftarKehadiranViewModel
{
    public FilterKehadiranViewModel Filter { get; set; } = new();
    public List<ItemKehadiranViewModel> DaftarKehadiran { get; set; } = new();
    public int TotalItem { get; set; }
    public int TotalHalaman => (int)Math.Ceiling((double)TotalItem / (Filter.UkuranHalaman > 0 ? Filter.UkuranHalaman : 15));
    public int HalamanSaatIni => Filter.Halaman;

    public int TotalHadir { get; set; }
    public int TotalTerlambat { get; set; }
    public int TotalIzin { get; set; }
    public int TotalSakit { get; set; }
    public int TotalCuti { get; set; }
    public int TotalAlpa { get; set; }
}

public class RekapKehadiranViewModel
{
    public int Bulan { get; set; } = DateTime.Today.Month;
    public int Tahun { get; set; } = DateTime.Today.Year;
    public Guid? DepartemenId { get; set; }
    public Guid? KaryawanId { get; set; }
    public List<ItemRekapKehadiranViewModel> DaftarRekap { get; set; } = new();
}

public class ItemRekapKehadiranViewModel
{
    public Guid KaryawanId { get; set; }
    public string NomorIndukKaryawan { get; set; } = string.Empty;
    public string NamaKaryawan { get; set; } = string.Empty;
    public string NamaDepartemen { get; set; } = string.Empty;
    public int TotalHadir { get; set; }
    public int TotalTerlambat { get; set; }
    public int TotalIzin { get; set; }
    public int TotalSakit { get; set; }
    public int TotalCuti { get; set; }
    public int TotalAlpa { get; set; }
    public int TotalMenitKeterlambatan { get; set; }
    public decimal TotalJamKerja { get; set; }
    public decimal TotalJamLembur { get; set; }
}

public class ShiftKerjaViewModel
{
    public Guid? Id { get; set; }

    [Required(ErrorMessage = "Nama shift wajib diisi")]
    [MaxLength(50)]
    public string NamaShift { get; set; } = string.Empty;

    [Required(ErrorMessage = "Kode shift wajib diisi")]
    [MaxLength(20)]
    public string KodeShift { get; set; } = string.Empty;

    [Required(ErrorMessage = "Jam masuk wajib diisi")]
    public TimeSpan JamMasuk { get; set; } = new TimeSpan(8, 0, 0);

    [Required(ErrorMessage = "Jam pulang wajib diisi")]
    public TimeSpan JamPulang { get; set; } = new TimeSpan(17, 0, 0);

    [Range(0, 120, ErrorMessage = "Toleransi keterlambatan antara 0 s/d 120 menit")]
    public int ToleransiKeterlambatanMenit { get; set; } = 15;

    [MaxLength(255)]
    public string? Keterangan { get; set; }

    public bool StatusAktif { get; set; } = true;
}

public class FormJadwalKerjaViewModel
{
    [Required(ErrorMessage = "Karyawan wajib dipilih")]
    public Guid KaryawanId { get; set; }

    [Required(ErrorMessage = "Shift kerja wajib dipilih")]
    public Guid ShiftKerjaId { get; set; }

    [Required(ErrorMessage = "Tanggal kerja wajib diisi")]
    public DateTime Tanggal { get; set; } = DateTime.Today;

    public bool StatusHariLibur { get; set; } = false;

    [MaxLength(255)]
    public string? Catatan { get; set; }
}

public class JadwalKerjaViewModel
{
    public Guid Id { get; set; }
    public Guid KaryawanId { get; set; }
    public string NomorIndukKaryawan { get; set; } = string.Empty;
    public string NamaKaryawan { get; set; } = string.Empty;
    public string NamaDepartemen { get; set; } = string.Empty;
    public Guid ShiftKerjaId { get; set; }
    public string NamaShift { get; set; } = string.Empty;
    public TimeSpan JamMasuk { get; set; }
    public TimeSpan JamPulang { get; set; }
    public DateTime Tanggal { get; set; }
    public bool StatusHariLibur { get; set; }
    public string? Catatan { get; set; }
}

public class FormPengajuanLemburViewModel
{
    public Guid? KaryawanId { get; set; }

    [Required(ErrorMessage = "Tanggal lembur wajib diisi")]
    public DateTime TanggalLembur { get; set; } = DateTime.Today;

    [Required(ErrorMessage = "Jam mulai wajib diisi")]
    public TimeSpan JamMulai { get; set; } = new TimeSpan(17, 0, 0);

    [Required(ErrorMessage = "Jam selesai wajib diisi")]
    public TimeSpan JamSelesai { get; set; } = new TimeSpan(20, 0, 0);

    [Required(ErrorMessage = "Alasan lembur wajib diisi")]
    [MaxLength(500, ErrorMessage = "Maksimal 500 karakter")]
    public string AlasanLembur { get; set; } = string.Empty;
}

public class FilterLemburViewModel
{
    public int? Bulan { get; set; }
    public int? Tahun { get; set; }
    public Guid? KaryawanId { get; set; }
    public StatusPengajuanLembur? Status { get; set; }
    public int Halaman { get; set; } = 1;
    public int UkuranHalaman { get; set; } = 15;
}

public class ItemLemburViewModel
{
    public Guid Id { get; set; }
    public Guid KaryawanId { get; set; }
    public string NomorIndukKaryawan { get; set; } = string.Empty;
    public string NamaKaryawan { get; set; } = string.Empty;
    public string NamaDepartemen { get; set; } = string.Empty;
    public string NomorPengajuan { get; set; } = string.Empty;
    public DateTime TanggalLembur { get; set; }
    public TimeSpan JamMulai { get; set; }
    public TimeSpan JamSelesai { get; set; }
    public decimal TotalJam { get; set; }
    public string AlasanLembur { get; set; } = string.Empty;
    public StatusPengajuanLembur Status { get; set; }
    public string? DisetujuiOleh { get; set; }
    public DateTime? WaktuPersetujuan { get; set; }
    public string? CatatanPersetujuan { get; set; }
}

public class DaftarLemburViewModel
{
    public FilterLemburViewModel Filter { get; set; } = new();
    public List<ItemLemburViewModel> DaftarLembur { get; set; } = new();
    public int TotalItem { get; set; }
    public int TotalHalaman => (int)Math.Ceiling((double)TotalItem / (Filter.UkuranHalaman > 0 ? Filter.UkuranHalaman : 15));
    public int HalamanSaatIni => Filter.Halaman;
}

public class PersetujuanLemburViewModel
{
    public Guid PengajuanLemburId { get; set; }
    public bool Disetujui { get; set; }
    [MaxLength(500)]
    public string? Catatan { get; set; }
}
