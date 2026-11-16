using SistemHRD.Models.Entitas;
using SistemHRD.Models.Enums;

namespace SistemHRD.Services.Interfaces;

/// <summary>
/// Data transfer object yang berisi parameter input untuk perhitungan gaji karyawan.
/// </summary>
public class ParameterKalkulasiGajiDto
{
    public Guid KaryawanId { get; set; }
    public string NomorIndukKaryawan { get; set; } = string.Empty;
    public string NamaKaryawan { get; set; } = string.Empty;
    public StatusPTKP StatusPTKP { get; set; } = StatusPTKP.TK0;

    // Rekapitulasi Presensi pada Periode Penggajian
    public int HariKerjaEfektif { get; set; } = 22;
    public int JumlahHadir { get; set; } = 22;
    public int JumlahTerlambat { get; set; } = 0;
    public int TotalMenitTerlambat { get; set; } = 0;
    public decimal JumlahJamLembur { get; set; } = 0;

    // Komponen Gaji yang Terkonfigurasi untuk Karyawan
    public List<ItemGajiKaryawanDto> DaftarKomponenGaji { get; set; } = new();

    // Kebijakan Perusahaan
    public KebijakanPenggajianPerusahaan Kebijakan { get; set; } = new();
}

public class ItemGajiKaryawanDto
{
    public Guid KomponenGajiId { get; set; }
    public string KodeKomponen { get; set; } = string.Empty;
    public string NamaKomponen { get; set; } = string.Empty;
    public JenisKomponenGaji Jenis { get; set; }
    public SifatKomponenGaji Sifat { get; set; }
    public KategoriKomponenGaji Kategori { get; set; }
    public decimal Nominal { get; set; }
    public bool KenaPajak { get; set; } = true;
    public bool MasukKalkulasiBpjs { get; set; } = true;
    public bool DihitungOtomatisOlehSistem { get; set; } = false;
    public string? Keterangan { get; set; }
}

/// <summary>
/// Output hasil kalkulasi gaji lengkap beserta rincian snapshot baris komponen.
/// </summary>
public class HasilKalkulasiGajiDto
{
    public Guid KaryawanId { get; set; }
    public StatusPTKP StatusPTKP { get; set; }
    public MetodePajakPPh21 MetodePajak { get; set; }

    public int HariKerjaEfektif { get; set; }
    public int JumlahHadir { get; set; }
    public int JumlahTerlambat { get; set; }
    public int TotalMenitTerlambat { get; set; }
    public decimal JumlahJamLembur { get; set; }

    // Rincian Akumulasi
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

    public decimal GajiBersih { get; set; } // Take Home Pay

    public decimal TotalBpjsDitanggungPerusahaan { get; set; }

    // Snapshot Baris Detail Komponen untuk disimpan ke DetailPenggajian
    public List<BarisDetailPenggajianDto> DaftarDetail { get; set; } = new();
}

public class BarisDetailPenggajianDto
{
    public Guid? KomponenGajiId { get; set; }
    public string KodeKomponen { get; set; } = string.Empty;
    public string NamaKomponen { get; set; } = string.Empty;
    public JenisKomponenGaji Jenis { get; set; }
    public SifatKomponenGaji Sifat { get; set; }
    public KategoriKomponenGaji Kategori { get; set; }
    public decimal Nominal { get; set; }
    public decimal NominalDitanggungPerusahaan { get; set; } = 0;
    public string? Keterangan { get; set; }
    public int Urutan { get; set; }
}

/// <summary>
/// Layanan engine murni untuk menghitung gaji, lembur, BPJS, dan PPh 21 tanpa ketergantungan HTTP/UI.
/// </summary>
public interface ILayananKalkulatorGaji
{
    HasilKalkulasiGajiDto HitungGaji(ParameterKalkulasiGajiDto parameter);
    decimal HitungUpahLembur(decimal gajiPokok, decimal tunjanganTetap, decimal jamLembur, decimal tarifFlat = 0);
    decimal HitungDendaKeterlambatan(int totalMenitTerlambat, int toleransiGratisMenit, decimal dendaPerMenit);
    (decimal Pekerja, decimal Perusahaan) HitungBpjsKesehatan(decimal basisUpah, KebijakanPenggajianPerusahaan kebijakan);
    (decimal JhtPekerja, decimal JhtPerusahaan, decimal JpPekerja, decimal JpPerusahaan, decimal JkkPerusahaan, decimal JkmPerusahaan) HitungBpjsKetenagakerjaan(decimal basisUpah, KebijakanPenggajianPerusahaan kebijakan);
    decimal HitungPph21Bulanan(decimal penghasilanBruto, StatusPTKP statusPtkp, MetodePajakPPh21 metode);
}
