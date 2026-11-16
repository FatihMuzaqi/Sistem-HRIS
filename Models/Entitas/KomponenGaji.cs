using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SistemHRD.Models.Enums;

namespace SistemHRD.Models.Entitas;

/// <summary>
/// Master komponen gaji yang berlaku pada perusahaan (Pendapatan maupun Potongan).
/// </summary>
public class KomponenGaji : EntitasTenantDasar
{
    [Required]
    [MaxLength(50)]
    public string KodeKomponen { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string NamaKomponen { get; set; } = string.Empty;

    public JenisKomponenGaji Jenis { get; set; } = JenisKomponenGaji.Pendapatan;

    public SifatKomponenGaji Sifat { get; set; } = SifatKomponenGaji.Tetap;

    public KategoriKomponenGaji Kategori { get; set; } = KategoriKomponenGaji.TunjanganTetap;

    [Column(TypeName = "decimal(18,2)")]
    public decimal NilaiBawaan { get; set; } = 0;

    /// <summary>
    /// Menentukan apakah komponen ini diperhitungkan dalam penghasilan bruto kena pajak (PPh 21).
    /// </summary>
    public bool KenaPajak { get; set; } = true;

    /// <summary>
    /// Menentukan apakah komponen ini masuk ke dalam basis perhitungan iuran BPJS.
    /// </summary>
    public bool MasukKalkulasiBpjs { get; set; } = true;

    /// <summary>
    /// Menandakan apakah komponen ini dihitung otomatis oleh sistem (Lembur, Potongan Telat, BPJS, Pajak).
    /// </summary>
    public bool DihitungOtomatisOlehSistem { get; set; } = false;

    public bool StatusAktif { get; set; } = true;

    [MaxLength(255)]
    public string? Keterangan { get; set; }

    // Relasi navigasi
    public virtual ICollection<GajiKaryawan> DaftarGajiKaryawan { get; set; } = new List<GajiKaryawan>();
    public virtual ICollection<DetailPenggajian> DaftarDetailPenggajian { get; set; } = new List<DetailPenggajian>();
}
