using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using SistemHRD.Models.Enums;

namespace SistemHRD.ViewModels;

#region Dashboard Rekrutmen
public class DashboardRekrutmenViewModel
{
    public int TotalLowonganAktif { get; set; }
    public int TotalPelamarBaru { get; set; }
    public int TotalDalamProses { get; set; }
    public int TotalDiterima { get; set; }

    public Dictionary<string, int> StatistikStatusPelamar { get; set; } = new();
    public Dictionary<string, int> StatistikPelamarPerLowongan { get; set; } = new();

    public List<LowonganKerjaItemViewModel> LowonganTerbaru { get; set; } = new();
    public List<JadwalWawancaraItemViewModel> WawancaraMendatang { get; set; } = new();
}
#endregion

#region Lowongan Kerja ViewModels
public class LowonganKerjaItemViewModel
{
    public Guid Id { get; set; }
    public string KodeLowongan { get; set; } = string.Empty;
    public string JudulLowongan { get; set; } = string.Empty;
    public string NamaDepartemen { get; set; } = "-";
    public string NamaJabatan { get; set; } = "-";
    public TipePekerjaan TipePekerjaan { get; set; }
    public string LokasiKerja { get; set; } = string.Empty;
    public int JumlahDibutuhkan { get; set; }
    public int JumlahPelamar { get; set; }
    public decimal? GajiMin { get; set; }
    public decimal? GajiMax { get; set; }
    public DateTime TanggalDibuka { get; set; }
    public DateTime? TanggalDitutup { get; set; }
    public StatusLowongan Status { get; set; }
}

public class FormLowonganKerjaViewModel
{
    public Guid? Id { get; set; }

    [Required(ErrorMessage = "Kode lowongan wajib diisi")]
    [MaxLength(50)]
    public string KodeLowongan { get; set; } = string.Empty;

    [Required(ErrorMessage = "Judul lowongan wajib diisi")]
    [MaxLength(150)]
    public string JudulLowongan { get; set; } = string.Empty;

    public Guid? DepartemenId { get; set; }
    public Guid? JabatanId { get; set; }

    [Required(ErrorMessage = "Deskripsi pekerjaan wajib diisi")]
    public string DeskripsiPekerjaan { get; set; } = string.Empty;

    [Required(ErrorMessage = "Kualifikasi pekerjaan wajib diisi")]
    public string Kualifikasi { get; set; } = string.Empty;

    public TipePekerjaan TipePekerjaan { get; set; } = TipePekerjaan.PenuhWaktu;

    [Required(ErrorMessage = "Lokasi kerja wajib diisi")]
    [MaxLength(100)]
    public string LokasiKerja { get; set; } = "Head Office";

    [Range(1, 1000, ErrorMessage = "Jumlah dibutuhkan minimal 1")]
    public int JumlahDibutuhkan { get; set; } = 1;

    public decimal? GajiMin { get; set; }
    public decimal? GajiMax { get; set; }

    [Required(ErrorMessage = "Tanggal dibuka wajib diisi")]
    public DateTime TanggalDibuka { get; set; } = DateTime.Today;

    public DateTime? TanggalDitutup { get; set; }

    public StatusLowongan Status { get; set; } = StatusLowongan.Dibuka;
}

public class DetailLowonganKerjaViewModel
{
    public Guid Id { get; set; }
    public string KodeLowongan { get; set; } = string.Empty;
    public string JudulLowongan { get; set; } = string.Empty;
    public string NamaDepartemen { get; set; } = "-";
    public string NamaJabatan { get; set; } = "-";
    public string DeskripsiPekerjaan { get; set; } = string.Empty;
    public string Kualifikasi { get; set; } = string.Empty;
    public TipePekerjaan TipePekerjaan { get; set; }
    public string LokasiKerja { get; set; } = string.Empty;
    public int JumlahDibutuhkan { get; set; }
    public decimal? GajiMin { get; set; }
    public decimal? GajiMax { get; set; }
    public DateTime TanggalDibuka { get; set; }
    public DateTime? TanggalDitutup { get; set; }
    public StatusLowongan Status { get; set; }

    public List<TahapanSeleksiItemViewModel> DaftarTahapan { get; set; } = new();
    public List<PelamarItemViewModel> DaftarPelamar { get; set; } = new();
}

public class TahapanSeleksiItemViewModel
{
    public Guid Id { get; set; }
    public string NamaTahapan { get; set; } = string.Empty;
    public JenisTahapanSeleksi JenisTahapan { get; set; }
    public int Urutan { get; set; }
    public string? Keterangan { get; set; }
    public bool StatusAktif { get; set; }
}
#endregion

#region Pelamar ViewModels
public class PelamarItemViewModel
{
    public Guid Id { get; set; }
    public Guid LowonganKerjaId { get; set; }
    public string NomorRegistrasi { get; set; } = string.Empty;
    public string JudulLowongan { get; set; } = string.Empty;
    public string NamaLengkap { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string NomorTelepon { get; set; } = string.Empty;
    public string? PendidikanTerakhir { get; set; }
    public string? NamaInstitusi { get; set; }
    public decimal PengalamanKerjaTahun { get; set; }
    public decimal? GajiDiharapkan { get; set; }
    public string NamaFileCV { get; set; } = string.Empty;
    public StatusPelamar Status { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}

public class FormPelamarViewModel
{
    [Required(ErrorMessage = "Lowongan kerja wajib dipilih")]
    public Guid LowonganKerjaId { get; set; }

    [Required(ErrorMessage = "Nama lengkap wajib diisi")]
    [MaxLength(150)]
    public string NamaLengkap { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email wajib diisi")]
    [EmailAddress(ErrorMessage = "Format email tidak valid")]
    [MaxLength(100)]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Nomor telepon wajib diisi")]
    [MaxLength(30)]
    public string NomorTelepon { get; set; } = string.Empty;

    [MaxLength(50)]
    public string? PendidikanTerakhir { get; set; }

    [MaxLength(150)]
    public string? NamaInstitusi { get; set; }

    [MaxLength(100)]
    public string? Jurusan { get; set; }

    [Range(0, 50, ErrorMessage = "Pengalaman kerja antara 0 s/d 50 tahun")]
    public decimal PengalamanKerjaTahun { get; set; }

    public decimal? GajiDiharapkan { get; set; }

    [MaxLength(500)]
    public string? CatatanAdministrasi { get; set; }

    [Required(ErrorMessage = "File CV/Resume wajib diunggah")]
    public IFormFile? BerkasCV { get; set; }
}

public class DetailPelamarViewModel
{
    public Guid Id { get; set; }
    public Guid LowonganKerjaId { get; set; }
    public string JudulLowongan { get; set; } = string.Empty;
    public string NomorRegistrasi { get; set; } = string.Empty;
    public string NamaLengkap { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string NomorTelepon { get; set; } = string.Empty;
    public string? PendidikanTerakhir { get; set; }
    public string? NamaInstitusi { get; set; }
    public string? Jurusan { get; set; }
    public decimal PengalamanKerjaTahun { get; set; }
    public decimal? GajiDiharapkan { get; set; }
    public string NamaFileCV { get; set; } = string.Empty;
    public string JalurFileCV { get; set; } = string.Empty;
    public string? CatatanAdministrasi { get; set; }
    public StatusPelamar Status { get; set; }
    public DateTimeOffset CreatedAt { get; set; }

    public Guid? KaryawanId { get; set; }
    public string? NamaKaryawanHasilKonversi { get; set; }

    public List<JadwalWawancaraItemViewModel> DaftarJadwalWawancara { get; set; } = new();
    public List<HasilSeleksiItemViewModel> DaftarHasilSeleksi { get; set; } = new();
    public List<TahapanSeleksiItemViewModel> DaftarTahapanLowongan { get; set; } = new();
}

public class FormSeleksiAdministrasiViewModel
{
    [Required]
    public Guid PelamarId { get; set; }

    public bool Lolos { get; set; }

    [MaxLength(500)]
    public string? CatatanAdministrasi { get; set; }
}
#endregion

#region Jadwal & Hasil Wawancara ViewModels
public class JadwalWawancaraItemViewModel
{
    public Guid Id { get; set; }
    public Guid PelamarId { get; set; }
    public string NamaPelamar { get; set; } = string.Empty;
    public string JudulLowongan { get; set; } = string.Empty;
    public string? NamaTahapan { get; set; }
    public string NamaPewawancara { get; set; } = string.Empty;
    public DateTime TanggalWawancara { get; set; }
    public TimeSpan JamMulai { get; set; }
    public TimeSpan JamSelesai { get; set; }
    public MetodeWawancara Metode { get; set; }
    public string LokasiAtauTautan { get; set; } = string.Empty;
    public StatusWawancara Status { get; set; }
    public string? CatatanUndangan { get; set; }
}

public class FormJadwalWawancaraViewModel
{
    [Required]
    public Guid PelamarId { get; set; }
    public string? NamaPelamar { get; set; }
    public string? JudulLowongan { get; set; }

    public Guid? TahapanSeleksiId { get; set; }
    public Guid? PewawancaraId { get; set; }

    [Required(ErrorMessage = "Nama pewawancara wajib diisi")]
    [MaxLength(150)]
    public string NamaPewawancara { get; set; } = string.Empty;

    [Required(ErrorMessage = "Tanggal wawancara wajib diisi")]
    public DateTime TanggalWawancara { get; set; } = DateTime.Today.AddDays(1);

    [Required(ErrorMessage = "Jam mulai wajib diisi")]
    public TimeSpan JamMulai { get; set; } = new TimeSpan(10, 0, 0);

    [Required(ErrorMessage = "Jam selesai wajib diisi")]
    public TimeSpan JamSelesai { get; set; } = new TimeSpan(11, 0, 0);

    public MetodeWawancara Metode { get; set; } = MetodeWawancara.Daring;

    [Required(ErrorMessage = "Lokasi atau tautan meeting wajib diisi")]
    [MaxLength(255)]
    public string LokasiAtauTautan { get; set; } = "Google Meet / Zoom";

    [MaxLength(500)]
    public string? CatatanUndangan { get; set; }
}

public class HasilSeleksiItemViewModel
{
    public Guid Id { get; set; }
    public Guid PelamarId { get; set; }
    public string? NamaTahapan { get; set; }
    public string NamaPenilai { get; set; } = string.Empty;
    public DateTime TanggalPenilaian { get; set; }
    public decimal Skor { get; set; }
    public string CatatanEvaluasi { get; set; } = string.Empty;
    public string? Kelebihan { get; set; }
    public string? Kekurangan { get; set; }
    public HasilKeputusanSeleksi Keputusan { get; set; }
}

public class FormHasilSeleksiViewModel
{
    [Required]
    public Guid PelamarId { get; set; }
    public string? NamaPelamar { get; set; }
    public string? JudulLowongan { get; set; }

    public Guid? TahapanSeleksiId { get; set; }
    public Guid? JadwalWawancaraId { get; set; }

    [Required(ErrorMessage = "Skor evaluasi wajib diisi (0 - 100)")]
    [Range(0, 100, ErrorMessage = "Skor antara 0 s/d 100")]
    public decimal Skor { get; set; } = 80;

    [Required(ErrorMessage = "Catatan evaluasi wajib diisi")]
    public string CatatanEvaluasi { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Kelebihan { get; set; }

    [MaxLength(500)]
    public string? Kekurangan { get; set; }

    public HasilKeputusanSeleksi Keputusan { get; set; } = HasilKeputusanSeleksi.Lolos;
}

public class FormTerimaPelamarViewModel
{
    [Required]
    public Guid PelamarId { get; set; }
    public string? NamaPelamar { get; set; }
    public string? JudulLowongan { get; set; }

    [Required(ErrorMessage = "Nomor Induk Karyawan (NIK) wajib diisi")]
    [MaxLength(50)]
    public string NomorIndukKaryawan { get; set; } = string.Empty;

    [Required(ErrorMessage = "Departemen wajib dipilih")]
    public Guid DepartemenId { get; set; }

    [Required(ErrorMessage = "Jabatan wajib dipilih")]
    public Guid JabatanId { get; set; }

    [Required(ErrorMessage = "Tanggal masuk kerja wajib diisi")]
    public DateTime TanggalMasuk { get; set; } = DateTime.Today;

    [Range(1000000, 1000000000, ErrorMessage = "Gaji pokok tidak valid")]
    public decimal GajiPokok { get; set; } = 8000000;
}
#endregion
