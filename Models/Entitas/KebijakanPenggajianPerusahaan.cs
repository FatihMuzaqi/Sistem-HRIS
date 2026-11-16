using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SistemHRD.Models.Enums;

namespace SistemHRD.Models.Entitas;

/// <summary>
/// Konfigurasi kebijakan penggajian, perpajakan, lembur, dan BPJS spesifik per perusahaan (Multi-Tenant).
/// </summary>
public class KebijakanPenggajianPerusahaan : EntitasTenantDasar
{
    // Kebijakan Lembur & Absensi
    public bool HitungLemburOtomatis { get; set; } = true;

    [Column(TypeName = "decimal(18,2)")]
    public decimal TarifLemburPerJamFlat { get; set; } = 0; // Jika 0, gunakan formula standar Kemnaker (1/173 x GP)

    public bool TerapkanPotonganKeterlambatan { get; set; } = true;

    public int ToleransiKeterlambatanGratisMenit { get; set; } = 15;

    [Column(TypeName = "decimal(18,2)")]
    public decimal DendaKeterlambatanPerMenit { get; set; } = 1000;

    // Kebijakan BPJS Kesehatan (Regulasi PP: 4% pemberi kerja, 1% pekerja, batas upah maks Rp 12.000.000)
    public bool TerapkanBpjsKesehatan { get; set; } = true;

    [Column(TypeName = "decimal(5,2)")]
    public decimal PersenBpjsKesehatanPerusahaan { get; set; } = 4.00m;

    [Column(TypeName = "decimal(5,2)")]
    public decimal PersenBpjsKesehatanPekerja { get; set; } = 1.00m;

    [Column(TypeName = "decimal(18,2)")]
    public decimal BatasMaksimalUpahBpjsKesehatan { get; set; } = 12000000m;

    // Kebijakan BPJS Ketenagakerjaan
    public bool TerapkanBpjsKetenagakerjaan { get; set; } = true;

    // Jaminan Hari Tua (JHT): 3.7% perusahaan, 2% pekerja
    [Column(TypeName = "decimal(5,2)")]
    public decimal PersenBpjsJhtPerusahaan { get; set; } = 3.70m;

    [Column(TypeName = "decimal(5,2)")]
    public decimal PersenBpjsJhtPekerja { get; set; } = 2.00m;

    // Jaminan Pensiun (JP): 2% perusahaan, 1% pekerja, batas upah maks Rp 10.042.300
    [Column(TypeName = "decimal(5,2)")]
    public decimal PersenBpjsJpPerusahaan { get; set; } = 2.00m;

    [Column(TypeName = "decimal(5,2)")]
    public decimal PersenBpjsJpPekerja { get; set; } = 1.00m;

    [Column(TypeName = "decimal(18,2)")]
    public decimal BatasMaksimalUpahBpjsJp { get; set; } = 10042300m;

    // Jaminan Kecelakaan Kerja (JKK): ditanggung perusahaan (tingkat risiko standar 0.24%)
    [Column(TypeName = "decimal(5,2)")]
    public decimal PersenBpjsJkkPerusahaan { get; set; } = 0.24m;

    // Jaminan Kematian (JKM): ditanggung perusahaan (0.30%)
    [Column(TypeName = "decimal(5,2)")]
    public decimal PersenBpjsJkmPerusahaan { get; set; } = 0.30m;

    // Kebijakan Pajak PPh 21
    public bool TerapkanPph21 { get; set; } = true;

    public MetodePajakPPh21 MetodePph21 { get; set; } = MetodePajakPPh21.TER_PP58;

    // Konfigurasi Cutoff Siklus Penggajian
    public int TanggalCutoffAwal { get; set; } = 26; // Misal tgl 26 bulan lalu

    public int TanggalCutoffAkhir { get; set; } = 25; // s/d tgl 25 bulan berjalan

    public int TanggalPembayaranGaji { get; set; } = 28; // Pembayaran tgl 28

    [MaxLength(255)]
    public string? CatatanSlipGajiDefault { get; set; } = "Slip gaji ini diterbitkan otomatis oleh SistemHRD.";
}
