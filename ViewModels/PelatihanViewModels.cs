using System.ComponentModel.DataAnnotations;
using SistemHRD.Models.Enums;

namespace SistemHRD.ViewModels;

#region Dashboard Pelatihan
public class DashboardPelatihanViewModel
{
    public int TotalProgramAktif { get; set; }
    public int TotalPesertaTerdaftar { get; set; }
    public int TotalPesertaLulus { get; set; }
    public int TotalSertifikatTerbit { get; set; }

    public Dictionary<string, int> StatistikStatusProgram { get; set; } = new();
    public Dictionary<string, int> StatistikMetodePelatihan { get; set; } = new();

    public List<ProgramPelatihanItemViewModel> ProgramTerbaru { get; set; } = new();
    public List<PesertaPelatihanItemViewModel> PesertaTerbaru { get; set; } = new();
}
#endregion

#region Program Pelatihan ViewModels
public class ProgramPelatihanItemViewModel
{
    public Guid Id { get; set; }
    public string KodeProgram { get; set; } = string.Empty;
    public string NamaProgram { get; set; } = string.Empty;
    public string Penyelenggara { get; set; } = string.Empty;
    public string? Instruktur { get; set; }
    public MetodePelatihan Metode { get; set; }
    public string LokasiAtauTautan { get; set; } = string.Empty;
    public DateTime TanggalMulai { get; set; }
    public DateTime TanggalSelesai { get; set; }
    public decimal TotalJam { get; set; }
    public decimal BiayaPerPeserta { get; set; }
    public int MaksimalPeserta { get; set; }
    public int JumlahPesertaTerdaftar { get; set; }
    public StatusPelatihan Status { get; set; }
    public string NamaDepartemen { get; set; } = "Semua Departemen (Umum)";
}

public class FormProgramPelatihanViewModel
{
    public Guid? Id { get; set; }

    [Required(ErrorMessage = "Kode program wajib diisi")]
    [MaxLength(50)]
    public string KodeProgram { get; set; } = string.Empty;

    [Required(ErrorMessage = "Nama program pelatihan wajib diisi")]
    [MaxLength(150)]
    public string NamaProgram { get; set; } = string.Empty;

    [Required(ErrorMessage = "Deskripsi pelatihan wajib diisi")]
    public string Deskripsi { get; set; } = string.Empty;

    [Required(ErrorMessage = "Penyelenggara wajib diisi")]
    [MaxLength(150)]
    public string Penyelenggara { get; set; } = "Divisi People Development";

    [MaxLength(150)]
    public string? Instruktur { get; set; }

    [Required]
    public MetodePelatihan Metode { get; set; } = MetodePelatihan.TatapMuka;

    [Required(ErrorMessage = "Lokasi atau tautan wajib diisi")]
    [MaxLength(255)]
    public string LokasiAtauTautan { get; set; } = "Ruang Pelatihan Utama";

    [Required(ErrorMessage = "Tanggal mulai wajib diisi")]
    public DateTime TanggalMulai { get; set; } = DateTime.Today.AddDays(7);

    [Required(ErrorMessage = "Tanggal selesai wajib diisi")]
    public DateTime TanggalSelesai { get; set; } = DateTime.Today.AddDays(8);

    [Range(0.5, 500, ErrorMessage = "Total jam antara 0.5 s/d 500 jam")]
    public decimal TotalJam { get; set; } = 8.00m;

    [Range(0, 1000000000, ErrorMessage = "Biaya tidak valid")]
    public decimal BiayaPerPeserta { get; set; } = 0;

    [Range(1, 1000, ErrorMessage = "Maksimal peserta minimal 1")]
    public int MaksimalPeserta { get; set; } = 25;

    public StatusPelatihan Status { get; set; } = StatusPelatihan.PendaftaranDibuka;

    public Guid? DepartemenId { get; set; }
}

public class DetailProgramPelatihanViewModel
{
    public Guid Id { get; set; }
    public string KodeProgram { get; set; } = string.Empty;
    public string NamaProgram { get; set; } = string.Empty;
    public string Deskripsi { get; set; } = string.Empty;
    public string Penyelenggara { get; set; } = string.Empty;
    public string? Instruktur { get; set; }
    public MetodePelatihan Metode { get; set; }
    public string LokasiAtauTautan { get; set; } = string.Empty;
    public DateTime TanggalMulai { get; set; }
    public DateTime TanggalSelesai { get; set; }
    public decimal TotalJam { get; set; }
    public decimal BiayaPerPeserta { get; set; }
    public int MaksimalPeserta { get; set; }
    public StatusPelatihan Status { get; set; }
    public string NamaDepartemen { get; set; } = "Semua Departemen (Umum)";

    public List<PesertaPelatihanItemViewModel> DaftarPeserta { get; set; } = new();
}
#endregion

#region Peserta & Sertifikat ViewModels
public class PesertaPelatihanItemViewModel
{
    public Guid Id { get; set; }
    public Guid ProgramPelatihanId { get; set; }
    public string NamaProgram { get; set; } = string.Empty;
    public Guid KaryawanId { get; set; }
    public string NIK { get; set; } = string.Empty;
    public string NamaKaryawan { get; set; } = string.Empty;
    public string NamaDepartemen { get; set; } = "-";
    public string NamaJabatan { get; set; } = "-";
    public StatusKeikutsertaan Status { get; set; }
    public decimal? NilaiAkhir { get; set; }
    public string? Catatan { get; set; }
    public DateTime TanggalDidaftarkan { get; set; }
    public DateTime? TanggalSelesai { get; set; }

    public Guid? SertifikatId { get; set; }
    public string? NomorSertifikat { get; set; }
    public string? KodeVerifikasiSertifikat { get; set; }
}

public class FormDaftarPesertaViewModel
{
    [Required]
    public Guid ProgramPelatihanId { get; set; }

    [Required(ErrorMessage = "Karyawan wajib dipilih")]
    public Guid KaryawanId { get; set; }

    [MaxLength(500)]
    public string? Catatan { get; set; }
}

public class FormDaftarPesertaMassalViewModel
{
    [Required]
    public Guid ProgramPelatihanId { get; set; }

    public List<Guid> DaftarKaryawanId { get; set; } = new();
}

public class FormUpdateStatusPesertaViewModel
{
    [Required]
    public Guid PesertaId { get; set; }

    public StatusKeikutsertaan StatusKeikutsertaan { get; set; } = StatusKeikutsertaan.Lulus;

    [Range(0, 100, ErrorMessage = "Nilai akhir antara 0 s/d 100")]
    public decimal? NilaiAkhir { get; set; }

    [MaxLength(500)]
    public string? Catatan { get; set; }
}

public class LembarSertifikatViewModel
{
    public Guid Id { get; set; }
    public string NomorSertifikat { get; set; } = string.Empty;
    public string JudulSertifikat { get; set; } = string.Empty;
    public string NamaKaryawan { get; set; } = string.Empty;
    public string NIK { get; set; } = string.Empty;
    public string NamaDepartemen { get; set; } = string.Empty;
    public string NamaProgram { get; set; } = string.Empty;
    public string DeskripsiPelatihan { get; set; } = string.Empty;
    public DateTime TanggalMulai { get; set; }
    public DateTime TanggalSelesai { get; set; }
    public decimal TotalJam { get; set; }
    public decimal? NilaiAkhir { get; set; }
    public DateTime TanggalTerbit { get; set; }
    public DateTime? TanggalKedaluwarsa { get; set; }
    public string PenerbitSertifikat { get; set; } = string.Empty;
    public string KodeVerifikasi { get; set; } = string.Empty;
    public string NamaPerusahaan { get; set; } = string.Empty;
}
#endregion
