using System.ComponentModel.DataAnnotations;
using SistemHRD.Models.Enums;

namespace SistemHRD.ViewModels;

#region Dashboard Aset
public class DashboardAsetViewModel
{
    public int TotalAset { get; set; }
    public int TotalAsetTersedia { get; set; }
    public int TotalAsetDigunakan { get; set; }
    public int TotalAsetDalamPerbaikan { get; set; }
    public decimal TotalNilaiInventaris { get; set; }

    public Dictionary<string, int> StatistikKategori { get; set; } = new();
    public Dictionary<string, int> StatistikKondisi { get; set; } = new();

    public List<AsetItemViewModel> AsetTerbaru { get; set; } = new();
    public List<PenyerahanAsetItemViewModel> PenyerahanTerbaru { get; set; } = new();
}
#endregion

#region Aset ViewModels
public class AsetItemViewModel
{
    public Guid Id { get; set; }
    public string NomorAset { get; set; } = string.Empty;
    public string NamaAset { get; set; } = string.Empty;
    public KategoriAset Kategori { get; set; }
    public string? MerkModel { get; set; }
    public string? NomorSeri { get; set; }
    public DateTime TanggalPembelian { get; set; }
    public decimal NilaiPerolehan { get; set; }
    public KondisiAset Kondisi { get; set; }
    public StatusAset Status { get; set; }
    public string LokasiPenyimpanan { get; set; } = string.Empty;
    public string? NamaPemegang { get; set; }
    public string? NIKPemegang { get; set; }
    public Guid? PemegangSaatIniId { get; set; }
}

public class DaftarAsetPagedViewModel
{
    public List<AsetItemViewModel> DaftarAset { get; set; } = new();
    public int HalamanSaatIni { get; set; } = 1;
    public int TotalHalaman { get; set; } = 1;
    public int UkuranHalaman { get; set; } = 10;
    public int TotalData { get; set; }

    public KategoriAset? KategoriTerpilih { get; set; }
    public StatusAset? StatusTerpilih { get; set; }
    public KondisiAset? KondisiTerpilih { get; set; }
    public string? KataKunci { get; set; }

    public bool HasPreviousPage => HalamanSaatIni > 1;
    public bool HasNextPage => HalamanSaatIni < TotalHalaman;
}

public class FormAsetViewModel
{
    public Guid? Id { get; set; }

    [Required(ErrorMessage = "Nomor aset wajib diisi")]
    [MaxLength(50)]
    public string NomorAset { get; set; } = string.Empty;

    [Required(ErrorMessage = "Nama aset wajib diisi")]
    [MaxLength(150)]
    public string NamaAset { get; set; } = string.Empty;

    [Required]
    public KategoriAset Kategori { get; set; } = KategoriAset.ElektronikLaptop;

    [MaxLength(100)]
    public string? MerkModel { get; set; }

    [MaxLength(100)]
    public string? NomorSeri { get; set; }

    [Required(ErrorMessage = "Tanggal pembelian wajib diisi")]
    public DateTime TanggalPembelian { get; set; } = DateTime.Today;

    [Range(0, 10000000000, ErrorMessage = "Nilai perolehan tidak valid")]
    public decimal NilaiPerolehan { get; set; } = 0;

    [Required]
    public KondisiAset Kondisi { get; set; } = KondisiAset.SangatBaik;

    [Required]
    public StatusAset Status { get; set; } = StatusAset.Tersedia;

    [Required(ErrorMessage = "Lokasi penyimpanan wajib diisi")]
    [MaxLength(100)]
    public string LokasiPenyimpanan { get; set; } = "Gudang IT / Operasional";

    public string? DeskripsiSpesifikasi { get; set; }
}

public class DetailAsetViewModel
{
    public Guid Id { get; set; }
    public string NomorAset { get; set; } = string.Empty;
    public string NamaAset { get; set; } = string.Empty;
    public KategoriAset Kategori { get; set; }
    public string? MerkModel { get; set; }
    public string? NomorSeri { get; set; }
    public DateTime TanggalPembelian { get; set; }
    public decimal NilaiPerolehan { get; set; }
    public KondisiAset Kondisi { get; set; }
    public StatusAset Status { get; set; }
    public string LokasiPenyimpanan { get; set; } = string.Empty;
    public string? DeskripsiSpesifikasi { get; set; }

    public Guid? PemegangSaatIniId { get; set; }
    public string? NamaPemegang { get; set; }
    public string? NIKPemegang { get; set; }
    public string? DepartemenPemegang { get; set; }
    public string? JabatanPemegang { get; set; }

    public List<PenyerahanAsetItemViewModel> RiwayatPenyerahan { get; set; } = new();
    public List<RiwayatAsetItemViewModel> RiwayatAktivitas { get; set; } = new();
}
#endregion

#region Penyerahan & Pengembalian Aset ViewModels
public class FormPenyerahanAsetViewModel
{
    [Required]
    public Guid AsetPerusahaanId { get; set; }
    public string? NamaAset { get; set; }
    public string? NomorAset { get; set; }

    [Required(ErrorMessage = "Karyawan penerima wajib dipilih")]
    public Guid KaryawanId { get; set; }

    [Required(ErrorMessage = "Nomor dokumen BAST wajib diisi")]
    [MaxLength(50)]
    public string NomorDokumen { get; set; } = string.Empty;

    [Required(ErrorMessage = "Tanggal penyerahan wajib diisi")]
    public DateTime TanggalPenyerahan { get; set; } = DateTime.Today;

    public DateTime? TanggalTargetPengembalian { get; set; }

    public KondisiAset KondisiSaatPenyerahan { get; set; } = KondisiAset.SangatBaik;

    [MaxLength(255)]
    public string? KelengkapanAset { get; set; } = "Charger, Tas, Mouse";

    [MaxLength(500)]
    public string? CatatanPenyerahan { get; set; }
}

public class FormPengembalianAsetViewModel
{
    [Required]
    public Guid PenyerahanAsetId { get; set; }

    [Required]
    public Guid AsetPerusahaanId { get; set; }
    public string? NamaAset { get; set; }

    [Required(ErrorMessage = "Tanggal pengembalian wajib diisi")]
    public DateTime TanggalRealisasiPengembalian { get; set; } = DateTime.Today;

    [Required]
    public KondisiAset KondisiSaatPengembalian { get; set; } = KondisiAset.Baik;

    [Required]
    [MaxLength(50)]
    public string StatusPeminjaman { get; set; } = "Dikembalikan";

    [MaxLength(500)]
    public string? CatatanPengembalian { get; set; }
}

public class PenyerahanAsetItemViewModel
{
    public Guid Id { get; set; }
    public Guid AsetPerusahaanId { get; set; }
    public string NomorAset { get; set; } = string.Empty;
    public string NamaAset { get; set; } = string.Empty;
    public Guid KaryawanId { get; set; }
    public string NIK { get; set; } = string.Empty;
    public string NamaKaryawan { get; set; } = string.Empty;
    public string? Departemen { get; set; }
    public string NomorDokumen { get; set; } = string.Empty;
    public DateTime TanggalPenyerahan { get; set; }
    public DateTime? TanggalTargetPengembalian { get; set; }
    public DateTime? TanggalRealisasiPengembalian { get; set; }
    public KondisiAset KondisiSaatPenyerahan { get; set; }
    public KondisiAset? KondisiSaatPengembalian { get; set; }
    public string StatusPeminjaman { get; set; } = string.Empty;
    public string? KelengkapanAset { get; set; }
    public string? CatatanPenyerahan { get; set; }
    public string? CatatanPengembalian { get; set; }
    public string? DiserahkanOleh { get; set; }
    public string? DiterimaKembaliOleh { get; set; }
}

public class RiwayatAsetItemViewModel
{
    public Guid Id { get; set; }
    public TipeAktivitasAset TipeAktivitas { get; set; }
    public DateTime TanggalAktivitas { get; set; }
    public string? NamaKaryawanTerkait { get; set; }
    public string NamaPetugas { get; set; } = string.Empty;
    public KondisiAset KondisiSebelumnya { get; set; }
    public KondisiAset KondisiTerbaru { get; set; }
    public string Deskripsi { get; set; } = string.Empty;
    public decimal? BiayaPerbaikan { get; set; }
}
#endregion
