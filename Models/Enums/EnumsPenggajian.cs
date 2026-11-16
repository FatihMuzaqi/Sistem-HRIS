namespace SistemHRD.Models.Enums;

/// <summary>
/// Jenis komponen gaji: Pendapatan (Penambah) atau Potongan (Pengurang).
/// </summary>
public enum JenisKomponenGaji
{
    Pendapatan = 1,
    Potongan = 2
}

/// <summary>
/// Sifat komponen gaji: Tetap (Fixed setiap bulan) atau Tidak Tetap (Variable/Fluktuatif).
/// </summary>
public enum SifatKomponenGaji
{
    Tetap = 1,
    TidakTetap = 2
}

/// <summary>
/// Kategori komponen gaji untuk standardisasi kalkulasi pajak dan BPJS.
/// </summary>
public enum KategoriKomponenGaji
{
    GajiPokok = 1,
    TunjanganTetap = 2,
    TunjanganTidakTetap = 3,
    UangLembur = 4,
    BonusInsentif = 5,
    TunjanganHariRaya = 6,

    PotonganKeterlambatan = 10,
    PotonganAbsensi = 11,

    BpjsKesehatan = 20,
    BpjsKetenagakerjaanJHT = 21,
    BpjsKetenagakerjaanJP = 22,
    BpjsKetenagakerjaanJKK = 23,
    BpjsKetenagakerjaanJKM = 24,

    PajakPPh21 = 30,
    KasbonPinjaman = 40,
    PotonganKoperasi = 41,
    PotonganLainnya = 99
}

/// <summary>
/// Status siklus proses periode penggajian bulanan.
/// </summary>
public enum StatusPeriodePenggajian
{
    Draf = 1,
    Diproses = 2,
    MenungguPersetujuan = 3,
    Disetujui = 4,
    Selesai = 5,
    Dibatalkan = 6
}

/// <summary>
/// Status individual payroll karyawan dalam satu periode.
/// </summary>
public enum StatusPenggajian
{
    Draf = 1,
    Dihitung = 2,
    Disetujui = 3,
    Dibayarkan = 4,
    Dibatalkan = 5
}

/// <summary>
/// Metode perhitungan pajak penghasilan pasal 21.
/// </summary>
public enum MetodePajakPPh21
{
    TER_PP58 = 1,          // Tarif Efektif Rata-rata Bulanan PP 58/2023
    ProgresifPasal17 = 2,  // Tarif Progresif Pasal 17 UU HPP
    GrossUp = 3,           // Ditunjang Perusahaan (Gross-up)
    Nett = 4               // Bebas Pajak / Pajak Ditanggung Perusahaan Penuh
}

/// <summary>
/// Status Penghasilan Tidak Kena Pajak (PTKP) sesuai peraturan perpajakan Indonesia.
/// </summary>
public enum StatusPTKP
{
    TK0 = 1, // Tidak Kawin, 0 Tanggungan (Rp 54.000.000) -> TER A
    TK1 = 2, // Tidak Kawin, 1 Tanggungan (Rp 58.500.000) -> TER A
    TK2 = 3, // Tidak Kawin, 2 Tanggungan (Rp 63.000.000) -> TER B
    TK3 = 4, // Tidak Kawin, 3 Tanggungan (Rp 67.500.000) -> TER B
    K0 = 5,  // Kawin, 0 Tanggungan (Rp 58.500.000) -> TER A
    K1 = 6,  // Kawin, 1 Tanggungan (Rp 63.000.000) -> TER B
    K2 = 7,  // Kawin, 2 Tanggungan (Rp 67.500.000) -> TER B
    K3 = 8   // Kawin, 3 Tanggungan (Rp 72.000.000) -> TER C
}
