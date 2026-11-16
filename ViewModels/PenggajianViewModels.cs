using System.ComponentModel.DataAnnotations;
using SistemHRD.Models.Enums;

namespace SistemHRD.ViewModels;

#region Master Komponen Gaji ViewModels

public class KomponenGajiViewModel
{
    public Guid? Id { get; set; }

    [Required(ErrorMessage = "Kode komponen wajib diisi")]
    [MaxLength(50)]
    public string KodeKomponen { get; set; } = string.Empty;

    [Required(ErrorMessage = "Nama komponen wajib diisi")]
    [MaxLength(100)]
    public string NamaKomponen { get; set; } = string.Empty;

    public JenisKomponenGaji Jenis { get; set; } = JenisKomponenGaji.Pendapatan;

    public SifatKomponenGaji Sifat { get; set; } = SifatKomponenGaji.Tetap;

    public KategoriKomponenGaji Kategori { get; set; } = KategoriKomponenGaji.TunjanganTetap;

    [Range(0, 1000000000, ErrorMessage = "Nominal tidak valid")]
    public decimal NilaiBawaan { get; set; } = 0;

    public bool KenaPajak { get; set; } = true;

    public bool MasukKalkulasiBpjs { get; set; } = true;

    public bool DihitungOtomatisOlehSistem { get; set; } = false;

    public bool StatusAktif { get; set; } = true;

    [MaxLength(255)]
    public string? Keterangan { get; set; }
}

#endregion

#region Pengaturan Gaji Karyawan ViewModels

public class AturGajiKaryawanViewModel
{
    public Guid KaryawanId { get; set; }
    public string NomorIndukKaryawan { get; set; } = string.Empty;
    public string NamaKaryawan { get; set; } = string.Empty;
    public string NamaDepartemen { get; set; } = string.Empty;
    public string NamaJabatan { get; set; } = string.Empty;

    public StatusPTKP StatusPTKP { get; set; } = StatusPTKP.TK0;

    [MaxLength(50)]
    public string? NamaBank { get; set; }

    [MaxLength(50)]
    public string? NomorRekeningBank { get; set; }

    [MaxLength(50)]
    public string? NomorNPWP { get; set; }

    [MaxLength(50)]
    public string? NomorBpjsKesehatan { get; set; }

    [MaxLength(50)]
    public string? NomorBpjsKetenagakerjaan { get; set; }

    public List<ItemKomponenGajiKaryawanViewModel> DaftarKomponen { get; set; } = new();

    public decimal TotalGajiPokok { get; set; }
    public decimal TotalTunjanganTetap { get; set; }
    public decimal TotalEstimasiTakeHomePay { get; set; }
}

public class ItemKomponenGajiKaryawanViewModel
{
    public Guid KomponenGajiId { get; set; }
    public string KodeKomponen { get; set; } = string.Empty;
    public string NamaKomponen { get; set; } = string.Empty;
    public JenisKomponenGaji Jenis { get; set; }
    public SifatKomponenGaji Sifat { get; set; }
    public KategoriKomponenGaji Kategori { get; set; }
    public decimal Nominal { get; set; }
    public bool StatusAktif { get; set; }
    public bool DihitungOtomatisOlehSistem { get; set; }
    public string? Keterangan { get; set; }
}

public class FormSimpanGajiKaryawanViewModel
{
    [Required]
    public Guid KaryawanId { get; set; }

    public StatusPTKP StatusPTKP { get; set; } = StatusPTKP.TK0;

    [MaxLength(50)]
    public string? NamaBank { get; set; }

    [MaxLength(50)]
    public string? NomorRekeningBank { get; set; }

    [MaxLength(50)]
    public string? NomorNPWP { get; set; }

    [MaxLength(50)]
    public string? NomorBpjsKesehatan { get; set; }

    [MaxLength(50)]
    public string? NomorBpjsKetenagakerjaan { get; set; }

    public List<InputKomponenGajiItemViewModel> Komponen { get; set; } = new();
}

public class InputKomponenGajiItemViewModel
{
    public Guid KomponenGajiId { get; set; }
    public decimal Nominal { get; set; }
    public bool StatusAktif { get; set; } = true;
    public string? Keterangan { get; set; }
}

#endregion

#region Kebijakan Penggajian ViewModels

public class KebijakanPenggajianViewModel
{
    public Guid Id { get; set; }
    public bool HitungLemburOtomatis { get; set; } = true;
    public decimal TarifLemburPerJamFlat { get; set; } = 0;
    public bool TerapkanPotonganKeterlambatan { get; set; } = true;
    public int ToleransiKeterlambatanGratisMenit { get; set; } = 15;
    public decimal DendaKeterlambatanPerMenit { get; set; } = 1000;

    public bool TerapkanBpjsKesehatan { get; set; } = true;
    public decimal PersenBpjsKesehatanPerusahaan { get; set; } = 4.00m;
    public decimal PersenBpjsKesehatanPekerja { get; set; } = 1.00m;
    public decimal BatasMaksimalUpahBpjsKesehatan { get; set; } = 12000000m;

    public bool TerapkanBpjsKetenagakerjaan { get; set; } = true;
    public decimal PersenBpjsJhtPerusahaan { get; set; } = 3.70m;
    public decimal PersenBpjsJhtPekerja { get; set; } = 2.00m;
    public decimal PersenBpjsJpPerusahaan { get; set; } = 2.00m;
    public decimal PersenBpjsJpPekerja { get; set; } = 1.00m;
    public decimal BatasMaksimalUpahBpjsJp { get; set; } = 10042300m;
    public decimal PersenBpjsJkkPerusahaan { get; set; } = 0.24m;
    public decimal PersenBpjsJkmPerusahaan { get; set; } = 0.30m;

    public bool TerapkanPph21 { get; set; } = true;
    public MetodePajakPPh21 MetodePph21 { get; set; } = MetodePajakPPh21.TER_PP58;

    public int TanggalCutoffAwal { get; set; } = 26;
    public int TanggalCutoffAkhir { get; set; } = 25;
    public int TanggalPembayaranGaji { get; set; } = 28;

    [MaxLength(255)]
    public string? CatatanSlipGajiDefault { get; set; }
}

#endregion

#region Periode Penggajian ViewModels

public class FormPeriodePenggajianViewModel
{
    public Guid? Id { get; set; }

    [Required(ErrorMessage = "Nama periode wajib diisi")]
    [MaxLength(100)]
    public string NamaPeriode { get; set; } = string.Empty;

    [Range(1, 12, ErrorMessage = "Bulan antara 1 s/d 12")]
    public int Bulan { get; set; } = DateTime.Today.Month;

    [Range(2020, 2100, ErrorMessage = "Tahun tidak valid")]
    public int Tahun { get; set; } = DateTime.Today.Year;

    [Required(ErrorMessage = "Tanggal mulai cutoff wajib diisi")]
    public DateTime TanggalMulai { get; set; } = DateTime.Today.AddMonths(-1).Date;

    [Required(ErrorMessage = "Tanggal selesai cutoff wajib diisi")]
    public DateTime TanggalSelesai { get; set; } = DateTime.Today.Date;

    [Required(ErrorMessage = "Tanggal pembayaran gaji wajib diisi")]
    public DateTime TanggalPembayaran { get; set; } = DateTime.Today.Date;

    [MaxLength(500)]
    public string? Catatan { get; set; }
}

public class PeriodePenggajianItemViewModel
{
    public Guid Id { get; set; }
    public string NamaPeriode { get; set; } = string.Empty;
    public int Bulan { get; set; }
    public int Tahun { get; set; }
    public DateTime TanggalMulai { get; set; }
    public DateTime TanggalSelesai { get; set; }
    public DateTime TanggalPembayaran { get; set; }
    public StatusPeriodePenggajian Status { get; set; }
    public int TotalKaryawan { get; set; }
    public decimal TotalPengeluaranGaji { get; set; }
    public string? Catatan { get; set; }
    public string? DisetujuiOleh { get; set; }
    public DateTimeOffset? WaktuDisetujui { get; set; }
}

public class DetailPeriodePenggajianViewModel
{
    public Guid Id { get; set; }
    public string NamaPeriode { get; set; } = string.Empty;
    public int Bulan { get; set; }
    public int Tahun { get; set; }
    public DateTime TanggalMulai { get; set; }
    public DateTime TanggalSelesai { get; set; }
    public DateTime TanggalPembayaran { get; set; }
    public StatusPeriodePenggajian Status { get; set; }
    public int TotalKaryawan { get; set; }
    public decimal TotalPengeluaranGaji { get; set; }
    public string? Catatan { get; set; }
    public string? DisetujuiOleh { get; set; }
    public DateTimeOffset? WaktuDisetujui { get; set; }

    // Ringkasan Finansial Periode
    public decimal TotalGajiPokok { get; set; }
    public decimal TotalTunjangan { get; set; }
    public decimal TotalUangLembur { get; set; }
    public decimal TotalPotonganBpjs { get; set; }
    public decimal TotalPph21 { get; set; }
    public decimal TotalPotonganLainnya { get; set; }

    // Daftar Payroll Karyawan pada Periode Ini
    public List<ItemPayrollKaryawanViewModel> DaftarPenggajian { get; set; } = new();
}

public class ItemPayrollKaryawanViewModel
{
    public Guid PenggajianId { get; set; }
    public Guid KaryawanId { get; set; }
    public string NomorSlipGaji { get; set; } = string.Empty;
    public string NomorIndukKaryawan { get; set; } = string.Empty;
    public string NamaKaryawan { get; set; } = string.Empty;
    public string NamaDepartemen { get; set; } = string.Empty;
    public string NamaJabatan { get; set; } = string.Empty;
    public decimal GajiPokok { get; set; }
    public decimal TotalPendapatanKotor { get; set; }
    public decimal TotalPotongan { get; set; }
    public decimal GajiBersih { get; set; }
    public StatusPenggajian Status { get; set; }
    public bool ApakahTerkunci { get; set; }
}

#endregion

#region Slip Gaji & Detail Penggajian ViewModels

public class RincianPenggajianViewModel
{
    public Guid PenggajianId { get; set; }
    public Guid PeriodePenggajianId { get; set; }
    public string NamaPeriode { get; set; } = string.Empty;
    public string NomorSlipGaji { get; set; } = string.Empty;
    public DateTime TanggalPembayaran { get; set; }

    // Data Karyawan
    public Guid KaryawanId { get; set; }
    public string NomorIndukKaryawan { get; set; } = string.Empty;
    public string NamaKaryawan { get; set; } = string.Empty;
    public string NamaDepartemen { get; set; } = string.Empty;
    public string NamaJabatan { get; set; } = string.Empty;
    public string? NamaBank { get; set; }
    public string? NomorRekeningBank { get; set; }
    public string? NomorNPWP { get; set; }
    public StatusPTKP StatusPTKP { get; set; }
    public MetodePajakPPh21 MetodePajak { get; set; }

    // Kehadiran & Lembur
    public int HariKerjaEfektif { get; set; }
    public int JumlahHadir { get; set; }
    public int JumlahTerlambat { get; set; }
    public int TotalMenitTerlambat { get; set; }
    public decimal JumlahJamLembur { get; set; }

    // Finansial
    public decimal GajiPokok { get; set; }
    public decimal TotalTunjanganTetap { get; set; }
    public decimal TotalTunjanganTidakTetap { get; set; }
    public decimal TotalUangLembur { get; set; }
    public decimal TotalPendapatanKotor { get; set; }

    public decimal TotalPotonganBpjs { get; set; }
    public decimal TotalPotonganPph21 { get; set; }
    public decimal TotalPotonganKeterlambatan { get; set; }
    public decimal TotalPotonganLainnya { get; set; }
    public decimal TotalPotongan { get; set; }

    public decimal GajiBersih { get; set; }
    public decimal TotalBpjsDitanggungPerusahaan { get; set; }

    public StatusPenggajian Status { get; set; }
    public bool ApakahTerkunci { get; set; }
    public string? Catatan { get; set; }

    // Breakdown Komponen Snapshot
    public List<BarisDetailPenggajianViewModel> DaftarPendapatan { get; set; } = new();
    public List<BarisDetailPenggajianViewModel> DaftarPotongan { get; set; } = new();
}

public class BarisDetailPenggajianViewModel
{
    public string KodeKomponen { get; set; } = string.Empty;
    public string NamaKomponen { get; set; } = string.Empty;
    public decimal Nominal { get; set; }
    public decimal NominalDitanggungPerusahaan { get; set; } = 0;
    public string? Keterangan { get; set; }
}

public class SlipGajiViewModel
{
    public Guid PenggajianId { get; set; }
    public string NomorSlipGaji { get; set; } = string.Empty;
    public string KodeVerifikasi { get; set; } = string.Empty;
    public DateTimeOffset WaktuDiterbitkan { get; set; }

    // Data Perusahaan
    public string NamaPerusahaan { get; set; } = string.Empty;
    public string? AlamatPerusahaan { get; set; }
    public string? TeleponPerusahaan { get; set; }
    public string? NPWPPerusahaan { get; set; }

    // Data Periode & Karyawan
    public string NamaPeriode { get; set; } = string.Empty;
    public int Bulan { get; set; }
    public int Tahun { get; set; }
    public DateTime TanggalMulaiCutoff { get; set; }
    public DateTime TanggalSelesaiCutoff { get; set; }
    public DateTime TanggalPembayaran { get; set; }

    public string NomorIndukKaryawan { get; set; } = string.Empty;
    public string NamaKaryawan { get; set; } = string.Empty;
    public string NamaDepartemen { get; set; } = string.Empty;
    public string NamaJabatan { get; set; } = string.Empty;
    public string? NamaBank { get; set; }
    public string? NomorRekeningBank { get; set; }
    public string? NomorNPWP { get; set; }
    public StatusPTKP StatusPTKP { get; set; }

    // Kehadiran
    public int HariKerjaEfektif { get; set; }
    public int JumlahHadir { get; set; }
    public int JumlahTerlambat { get; set; }
    public decimal JumlahJamLembur { get; set; }

    // Finansial
    public List<BarisDetailPenggajianViewModel> DaftarPendapatan { get; set; } = new();
    public List<BarisDetailPenggajianViewModel> DaftarPotongan { get; set; } = new();

    public decimal TotalPendapatanKotor { get; set; }
    public decimal TotalPotongan { get; set; }
    public decimal GajiBersih { get; set; }
    public string TerbilangGajiBersih { get; set; } = string.Empty;

    public string? CatatanKaki { get; set; }
}

public class HitungGajiBatchRequestViewModel
{
    [Required]
    public Guid PeriodePenggajianId { get; set; }

    public Guid? DepartemenId { get; set; }
}

public class SetujuiPeriodeRequestViewModel
{
    [Required]
    public Guid PeriodePenggajianId { get; set; }

    [MaxLength(500)]
    public string? Catatan { get; set; }
}

#endregion
