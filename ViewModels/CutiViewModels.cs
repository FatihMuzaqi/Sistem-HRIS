using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using SistemHRD.Models.Enums;

namespace SistemHRD.ViewModels;

public class JenisCutiViewModel
{
    public Guid? Id { get; set; }

    [Required(ErrorMessage = "Nama jenis cuti wajib diisi")]
    [MaxLength(100)]
    public string NamaJenisCuti { get; set; } = string.Empty;

    [Required(ErrorMessage = "Kode jenis cuti wajib diisi")]
    [MaxLength(20)]
    public string KodeJenisCuti { get; set; } = string.Empty;

    [Range(1, 365, ErrorMessage = "Jatah hari berkisar antara 1 s/d 365 hari")]
    public int JatahHariPerTahun { get; set; } = 12;

    public bool PotongCutiTahunan { get; set; } = true;

    public bool WajibLampiran { get; set; } = false;

    [MaxLength(255)]
    public string? Keterangan { get; set; }

    public bool StatusAktif { get; set; } = true;
}

public class SaldoCutiViewModel
{
    public Guid Id { get; set; }
    public Guid KaryawanId { get; set; }
    public string NomorIndukKaryawan { get; set; } = string.Empty;
    public string NamaKaryawan { get; set; } = string.Empty;
    public string NamaDepartemen { get; set; } = string.Empty;
    public Guid JenisCutiId { get; set; }
    public string NamaJenisCuti { get; set; } = string.Empty;
    public string KodeJenisCuti { get; set; } = string.Empty;
    public int PeriodeTahun { get; set; }
    public int JumlahHakCuti { get; set; }
    public int CutiTerpakai { get; set; }
    public int SisaCuti { get; set; }
    public string? Catatan { get; set; }
}

public class AturSaldoCutiViewModel
{
    [Required(ErrorMessage = "Karyawan wajib dipilih")]
    public Guid KaryawanId { get; set; }

    [Required(ErrorMessage = "Jenis cuti wajib dipilih")]
    public Guid JenisCutiId { get; set; }

    [Required(ErrorMessage = "Periode tahun wajib diisi")]
    public int PeriodeTahun { get; set; } = DateTime.Today.Year;

    [Range(0, 365, ErrorMessage = "Jumlah hak cuti antara 0 s/d 365 hari")]
    public int JumlahHakCuti { get; set; } = 12;

    [MaxLength(255)]
    public string? Catatan { get; set; }
}

public class FormPengajuanCutiViewModel
{
    public Guid? KaryawanId { get; set; }

    [Required(ErrorMessage = "Jenis cuti wajib dipilih")]
    public Guid JenisCutiId { get; set; }

    [Required(ErrorMessage = "Tanggal mulai cuti wajib diisi")]
    public DateTime TanggalMulai { get; set; } = DateTime.Today.AddDays(1);

    [Required(ErrorMessage = "Tanggal selesai cuti wajib diisi")]
    public DateTime TanggalSelesai { get; set; } = DateTime.Today.AddDays(1);

    public int JumlahHari { get; set; } = 1;

    [Required(ErrorMessage = "Alasan cuti wajib diisi")]
    [MaxLength(500, ErrorMessage = "Maksimal 500 karakter")]
    public string Alasan { get; set; } = string.Empty;

    [MaxLength(255)]
    public string? AlamatSelamaCuti { get; set; }

    [MaxLength(50)]
    public string? NomorTeleponDarurat { get; set; }

    public IFormFile? BerkasDokumen { get; set; }
}

public class FilterPengajuanCutiViewModel
{
    public int? Bulan { get; set; }
    public int? Tahun { get; set; }
    public Guid? DepartemenId { get; set; }
    public Guid? KaryawanId { get; set; }
    public StatusPengajuanCuti? Status { get; set; }
    public Guid? JenisCutiId { get; set; }
    public string? Cari { get; set; }
    public int Halaman { get; set; } = 1;
    public int UkuranHalaman { get; set; } = 15;
}

public class PengajuanCutiItemViewModel
{
    public Guid Id { get; set; }
    public string NomorPengajuan { get; set; } = string.Empty;
    public Guid KaryawanId { get; set; }
    public string NomorIndukKaryawan { get; set; } = string.Empty;
    public string NamaKaryawan { get; set; } = string.Empty;
    public string NamaDepartemen { get; set; } = string.Empty;
    public Guid JenisCutiId { get; set; }
    public string NamaJenisCuti { get; set; } = string.Empty;
    public DateTime TanggalMulai { get; set; }
    public DateTime TanggalSelesai { get; set; }
    public int JumlahHari { get; set; }
    public string Alasan { get; set; } = string.Empty;
    public StatusPengajuanCuti Status { get; set; }
    public bool AdaDokumen { get; set; }
    public DateTimeOffset TanggalDiajukan { get; set; }
}

public class DaftarPengajuanCutiViewModel
{
    public FilterPengajuanCutiViewModel Filter { get; set; } = new();
    public List<PengajuanCutiItemViewModel> DaftarPengajuan { get; set; } = new();
    public int TotalItem { get; set; }
    public int TotalHalaman => (int)Math.Ceiling((double)TotalItem / (Filter.UkuranHalaman > 0 ? Filter.UkuranHalaman : 15));
    public int HalamanSaatIni => Filter.Halaman;

    public int TotalMenungguManager { get; set; }
    public int TotalMenungguHRD { get; set; }
    public int TotalDisetujui { get; set; }
    public int TotalDitolak { get; set; }

    // Saldo cuti tahunan user aktif (untuk tampilan ESS)
    public int? SaldoCutiPribadi { get; set; }
    public int? CutiTerpakaiPribadi { get; set; }
    public int? SisaCutiPribadi { get; set; }
}

public class ItemRiwayatPersetujuanViewModel
{
    public TahapPersetujuanCuti Tahap { get; set; }
    public string NamaPenyetuju { get; set; } = string.Empty;
    public KeputusanPersetujuan Keputusan { get; set; }
    public string? Catatan { get; set; }
    public DateTime WaktuPersetujuan { get; set; }
}

public class PengajuanCutiDetailViewModel
{
    public Guid Id { get; set; }
    public string NomorPengajuan { get; set; } = string.Empty;
    public Guid KaryawanId { get; set; }
    public string NomorIndukKaryawan { get; set; } = string.Empty;
    public string NamaKaryawan { get; set; } = string.Empty;
    public string NamaDepartemen { get; set; } = string.Empty;
    public string NamaJabatan { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? NomorTelepon { get; set; }

    public Guid JenisCutiId { get; set; }
    public string NamaJenisCuti { get; set; } = string.Empty;
    public bool PotongCutiTahunan { get; set; }

    public DateTime TanggalMulai { get; set; }
    public DateTime TanggalSelesai { get; set; }
    public int JumlahHari { get; set; }

    public string Alasan { get; set; } = string.Empty;
    public string? AlamatSelamaCuti { get; set; }
    public string? NomorTeleponDarurat { get; set; }

    public string? NamaDokumenPendukung { get; set; }
    public bool AdaDokumen => !string.IsNullOrEmpty(NamaDokumenPendukung);

    public StatusPengajuanCuti Status { get; set; }
    public DateTimeOffset TanggalDiajukan { get; set; }

    public int? SisaSaldoCuti { get; set; }

    public List<ItemRiwayatPersetujuanViewModel> DaftarPersetujuan { get; set; } = new();

    public bool BisaDisetujuiManager => Status == StatusPengajuanCuti.MenungguPersetujuanManager;
    public bool BisaDiverifikasiHRD => Status == StatusPengajuanCuti.DisetujuiManager || Status == StatusPengajuanCuti.MenungguPersetujuanManager;
}

public class PersetujuanCutiRequestViewModel
{
    public Guid PengajuanCutiId { get; set; }
    public bool Disetujui { get; set; }
    [MaxLength(500)]
    public string? Catatan { get; set; }
}
