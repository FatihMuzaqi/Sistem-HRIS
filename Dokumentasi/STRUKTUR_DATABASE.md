# STRUKTUR DATABASE — SISTEMHRD

## 1. Spesifikasi Teknis Basis Data
* **Engine Basis Data**: MySQL 8.x / MariaDB 10.x (melalui XAMPP phpMyAdmin) dengan kompatibilitas SQL Server.
* **ORM**: Entity Framework Core 10 (Code-First dengan Migrations).
* **Karakter Set & Collation**: `utf8mb4` / `utf8mb4_unicode_ci` (mendukung penuh multibyte, emoji, dan simbol internasional).
* **Skema Penamaan**: Bahasa Indonesia dengan kaidah PascalCase untuk Entitas dan snake_case/PascalCase konsisten pada tabel fisik.

---

## 2. Standar Kolom Fondasi (Base Columns)

Setiap entitas bisnis mewarisi kolom-kolom standar berikut:

### 2.1 Entitas Tenant (`ITenantEntity`)
* `PerusahaanId` (CHAR(36) GUID, Foreign Key ke tabel `Perusahaan`, NOT NULL).

### 2.2 Entitas Audit (`IAuditEntity`)
* `CreatedAt` (DATETIME / TIMESTAMP, NOT NULL, default: waktu pembuatan UTC).
* `CreatedBy` (VARCHAR(100), NULL, username / ID pembuat).
* `UpdatedAt` (DATETIME / TIMESTAMP, NULL, waktu modifikasi terakhir UTC).
* `UpdatedBy` (VARCHAR(100), NULL, username / ID pengubah).

### 2.3 Entitas Soft Delete (`ISoftDelete`)
* `IsDeleted` (TINYINT(1), NOT NULL, default: 0).
* `DeletedAt` (DATETIME / TIMESTAMP, NULL, waktu penghapusan logis).
* `DeletedBy` (VARCHAR(100), NULL, username / ID penghapus).

---

## 3. Kamus Tabel Utama

### 3.1 Tabel Fondasi & Autentikasi (Identity)
| Nama Tabel | Deskripsi | Kolom Kunci / Penting |
| :--- | :--- | :--- |
| `Perusahaan` | Master data entitas penyewa (Tenant) | `Id` (PK), `KodePerusahaan` (Unique), `NamaPerusahaan`, `StatusAktif` |
| `Pengguna` | Data akun pengguna sistem (Identity) | `Id` (PK), `UserName`, `Email`, `NamaLengkap`, `PerusahaanId` (FK, Nullable) |
| `Peran` | Master data peran pengguna (Roles) | `Id` (PK), `Name` (Unique), `Deskripsi` |
| `PenggunaPeran` | Relasi many-to-many pengguna dan peran | `UserId` (PK/FK), `RoleId` (PK/FK) |
| `PenggunaKlaim` | Klaim otorisasi spesifik pengguna | `Id` (PK), `UserId` (FK), `ClaimType`, `ClaimValue` |
| `PeranKlaim` | Klaim otorisasi spesifik peran | `Id` (PK), `RoleId` (FK), `ClaimType`, `ClaimValue` |

---

### 3.2 Tabel Modul Manajemen Karyawan (Fase 2 - Terimplementasi)

```
┌────────────────────────────────────────────────────────┐
│                      Perusahaan                        │
└───────┬────────────────────────┬───────────────────────┘
        │ 1:N                    │ 1:N
        ▼                        ▼
┌───────────────┐        ┌───────────────┐
│  Departemen   │◄───────┤    Jabatan    │
└───────┬───────┘ 1:N    └───────┬───────┘
        │                        │
        │ 1:N                    │ 1:N
        └───────────┬────────────┘
                    ▼
          ┌───────────────────┐
          │     Karyawan      │
          └─────────┬─────────┘
                    │
   ┌────────────────┼────────────────┬────────────────┐
   │ 1:N            │ 1:N            │ 1:N            │ 1:N
   ▼                ▼                ▼                ▼
┌─────────────┐  ┌─────────────┐  ┌─────────────┐  ┌──────────────────┐
│KontrakKerja │  │Dokumen      │  │KontakDarurat│  │RiwayatPekerjaan  │
└─────────────┘  └─────────────┘  └─────────────┘  └──────────────────┘
```

1. **`Departemen`**:
   * `Id` (GUID, PK)
   * `PerusahaanId` (GUID, FK `Perusahaan`)
   * `KodeDepartemen` (VARCHAR(20), Unique per `PerusahaanId`)
   * `NamaDepartemen` (VARCHAR(100), NOT NULL)
   * `Deskripsi` (VARCHAR(500), NULL)
   * `KepalaDepartemenId` (GUID, FK `Karyawan`, Nullable, `OnDelete: SetNull`)
   * `StatusAktif` (BOOLEAN)
   * Kolom Audit (`CreatedAt`, `CreatedBy`, `UpdatedAt`, `UpdatedBy`)
   * Kolom Soft Delete (`IsDeleted`, `DeletedAt`, `DeletedBy`)

2. **`Jabatan`**:
   * `Id` (GUID, PK)
   * `PerusahaanId` (GUID, FK `Perusahaan`)
   * `DepartemenId` (GUID, FK `Departemen`, NOT NULL, `OnDelete: Cascade`)
   * `NamaJabatan` (VARCHAR(100), NOT NULL)
   * `Deskripsi` (VARCHAR(500), NULL)
   * `Level` (INT: 1=Direksi, 2=Manajer, 3=Supervisor, 4=Senior, 5=Staf)
   * `StatusAktif` (BOOLEAN)
   * Kolom Audit & Soft Delete

3. **`Karyawan`**:
   * `Id` (GUID, PK)
   * `PerusahaanId` (GUID, FK `Perusahaan`)
   * `NomorIndukKaryawan` (VARCHAR(50), Unique per `PerusahaanId`)
   * `NamaLengkap` (VARCHAR(150), NOT NULL)
   * `TempatLahir` (VARCHAR(100), NULL)
   * `TanggalLahir` (DATE, NULL)
   * `JenisKelamin` (INT: 1=Laki-laki, 2=Perempuan)
   * `Agama` (INT) & `StatusPernikahan` (INT)
   * `Alamat` (VARCHAR(300), NULL)
   * `NomorTelepon` (VARCHAR(20), NULL)
   * `Email` (VARCHAR(100), NULL, Indexed)
   * `TanggalMasuk` (DATE, NOT NULL)
   * `TanggalKeluar` (DATE, NULL)
   * `DepartemenId` (GUID, FK `Departemen`, `OnDelete: Restrict`)
   * `JabatanId` (GUID, FK `Jabatan`, `OnDelete: Restrict`)
   * `PenggunaId` (GUID, FK `Pengguna`, Nullable, `OnDelete: SetNull`)
   * `StatusKaryawan` (INT: 1=Aktif, 2=Cuti, 3=Skorsing, 4=Resign, 5=PHK, 6=Pensiun)
   * `FotoProfil` (VARCHAR(255), Path avatar lokal)
   * Kolom Audit & Soft Delete

4. **`KontrakKerja`**:
   * `Id` (GUID, PK)
   * `PerusahaanId` (GUID, FK `Perusahaan`)
   * `KaryawanId` (GUID, FK `Karyawan`, `OnDelete: Cascade`)
   * `NomorKontrak` (VARCHAR(100), NOT NULL)
   * `JenisKontrak` (INT: 1=PKWT, 2=PKWTT, 3=Magang, 4=Percobaan, 5=Konsultan)
   * `TanggalMulai` (DATE, NOT NULL)
   * `TanggalSelesai` (DATE, Nullable untuk PKWTT)
   * `Status` (INT: 1=Draf, 2=Aktif, 3=Selesai, 4=Diperpanjang, 5=Dibatalkan)
   * `Catatan` (VARCHAR(500), NULL)
   * Kolom Audit & Soft Delete

5. **`DokumenKaryawan`**:
   * `Id` (GUID, PK)
   * `PerusahaanId` (GUID, FK `Perusahaan`)
   * `KaryawanId` (GUID, FK `Karyawan`, `OnDelete: Cascade`)
   * `JenisDokumen` (INT: 1=KTP, 2=NPWP, 3=Ijazah, 4=CV, 5=Kontrak, 6=Sertifikat, 7=Lainnya)
   * `NamaDokumen` (VARCHAR(150), NOT NULL)
   * `NamaFileAsli` (VARCHAR(255), NOT NULL)
   * `NamaFileSimpan` (VARCHAR(255), Hash nama file tersimpan di server)
   * `JalurFile` (VARCHAR(500), Path fisik aman: `App_Data/DokumenKaryawan/...`)
   * `UkuranByte` (BIGINT)
   * `TipeKonten` (VARCHAR(100), e.g. `application/pdf`, `image/jpeg`)
   * Kolom Audit & Soft Delete

6. **`KontakDaruratKaryawan`**:
   * `Id` (GUID, PK)
   * `PerusahaanId` (GUID, FK `Perusahaan`)
   * `KaryawanId` (GUID, FK `Karyawan`, `OnDelete: Cascade`)
   * `Nama` (VARCHAR(100), NOT NULL)
   * `Hubungan` (VARCHAR(50), NOT NULL, e.g. Orang Tua, Pasangan, Saudara)
   * `NomorTelepon` (VARCHAR(20), NOT NULL)
   * `Alamat` (VARCHAR(255), NULL)
   * Kolom Audit & Soft Delete

7. **`RiwayatPekerjaan`**:
   * `Id` (GUID, PK)
   * `PerusahaanId` (GUID, FK `Perusahaan`)
   * `KaryawanId` (GUID, FK `Karyawan`, `OnDelete: Cascade`)
   * `NamaPerusahaan` (VARCHAR(150), NOT NULL)
   * `PosisiTerakhir` (VARCHAR(100), NOT NULL)
   * `TanggalMulai` (DATE, NOT NULL)
   * `TanggalSelesai` (DATE, NULL)
   * `GajiTerakhir` (DECIMAL(18,2), NULL)
   * `AlasanBerhenti` (VARCHAR(300), NULL)
   * `DeskripsiTugas` (VARCHAR(1000), NULL)
   * Kolom Audit & Soft Delete

---

### 3.3 Tabel Modul Kehadiran dan Pengajuan Cuti (Fase 3)
1. **`ShiftKerja`**:
   * `Id` (GUID, PK)
   * `PerusahaanId` (GUID, FK `Perusahaan`)
   * `NamaShift` (VARCHAR(100), NOT NULL)
   * `KodeShift` (VARCHAR(50), NOT NULL)
   * `JamMasuk` (TIME, NOT NULL)
   * `JamPulang` (TIME, NOT NULL)
   * `ToleransiKeterlambatanMenit` (INT, Default 0)
   * `Keterangan` (VARCHAR(255), NULL)
   * `StatusAktif` (BOOLEAN, Default TRUE)
   * Kolom Audit & Soft Delete
   * Unique Index: `(PerusahaanId, KodeShift)`

2. **`JadwalKerja`**:
   * `Id` (GUID, PK)
   * `PerusahaanId` (GUID, FK `Perusahaan`)
   * `KaryawanId` (GUID, FK `Karyawan`, `OnDelete: Cascade`)
   * `ShiftKerjaId` (GUID, FK `ShiftKerja`, `OnDelete: Restrict`)
   * `Tanggal` (DATE, NOT NULL)
   * `Catatan` (VARCHAR(255), NULL)
   * Kolom Audit & Soft Delete
   * Index: `(PerusahaanId, KaryawanId, Tanggal)`

3. **`Kehadiran`**:
   * `Id` (GUID, PK)
   * `PerusahaanId` (GUID, FK `Perusahaan`)
   * `KaryawanId` (GUID, FK `Karyawan`, `OnDelete: Cascade`)
   * `ShiftKerjaId` (GUID, FK `ShiftKerja`, `OnDelete: SetNull`)
   * `Tanggal` (DATE, NOT NULL)
   * `WaktuMasuk` (DATETIMEOFFSET, NULL)
   * `WaktuPulang` (DATETIMEOFFSET, NULL)
   * `Status` (INT: 1=Hadir, 2=Terlambat, 3=PulangCepat, 4=Izin, 5=Sakit, 6=Cuti, 7=Alpa)
   * `MenitKeterlambatan` (INT, Default 0)
   * `MenitPulangCepat` (INT, Default 0)
   * `TotalJamKerja` (DECIMAL(4,2), Default 0)
   * `LokasiMasuk` (VARCHAR(255), NULL)
   * `LokasiPulang` (VARCHAR(255), NULL)
   * `CatatanMasuk` (VARCHAR(255), NULL)
   * `CatatanPulang` (VARCHAR(255), NULL)
   * Kolom Audit & Soft Delete
   * Unique Index: `(PerusahaanId, KaryawanId, Tanggal)`

4. **`PengajuanLembur`**:
   * `Id` (GUID, PK)
   * `PerusahaanId` (GUID, FK `Perusahaan`)
   * `KaryawanId` (GUID, FK `Karyawan`, `OnDelete: Cascade`)
   * `TanggalLembur` (DATE, NOT NULL)
   * `JamMulai` (TIME, NOT NULL)
   * `JamSelesai` (TIME, NOT NULL)
   * `TotalJam` (DECIMAL(4,2), NOT NULL)
   * `AlasanLembur` (VARCHAR(500), NOT NULL)
   * `Status` (INT: 1=MenungguPersetujuan, 2=Disetujui, 3=Ditolak, 4=Dibatalkan)
   * `DisetujuiOlehId` (GUID, FK `Pengguna`, `OnDelete: SetNull`)
   * `WaktuPersetujuan` (DATETIMEOFFSET, NULL)
   * `CatatanPersetujuan` (VARCHAR(255), NULL)
   * Kolom Audit & Soft Delete
   * Index: `(PerusahaanId, KaryawanId, TanggalLembur)`, `(PerusahaanId, Status)`

5. **`JenisCuti`**:
   * `Id` (GUID, PK)
   * `PerusahaanId` (GUID, FK `Perusahaan`)
   * `KodeJenisCuti` (VARCHAR(50), NOT NULL)
   * `NamaJenisCuti` (VARCHAR(100), NOT NULL)
   * `JatahHariPerTahun` (INT, Default 0)
   * `PotongCutiTahunan` (BOOLEAN, Default TRUE)
   * `WajibLampiran` (BOOLEAN, Default FALSE)
   * `Keterangan` (VARCHAR(255), NULL)
   * `StatusAktif` (BOOLEAN, Default TRUE)
   * Kolom Audit & Soft Delete
   * Unique Index: `(PerusahaanId, KodeJenisCuti)`

6. **`SaldoCuti`**:
   * `Id` (GUID, PK)
   * `PerusahaanId` (GUID, FK `Perusahaan`)
   * `KaryawanId` (GUID, FK `Karyawan`, `OnDelete: Cascade`)
   * `JenisCutiId` (GUID, FK `JenisCuti`, `OnDelete: Cascade`)
   * `PeriodeTahun` (INT, NOT NULL)
   * `JumlahHakCuti` (INT, NOT NULL)
   * `CutiTerpakai` (INT, Default 0)
   * `SisaCuti` (INT, NOT NULL)
   * `Catatan` (VARCHAR(255), NULL)
   * Kolom Audit & Soft Delete
   * Unique Index: `(PerusahaanId, KaryawanId, JenisCutiId, PeriodeTahun)`

7. **`PengajuanCuti`**:
   * `Id` (GUID, PK)
   * `PerusahaanId` (GUID, FK `Perusahaan`)
   * `KaryawanId` (GUID, FK `Karyawan`, `OnDelete: Cascade`)
   * `JenisCutiId` (GUID, FK `JenisCuti`, `OnDelete: Restrict`)
   * `NomorPengajuan` (VARCHAR(50), NOT NULL)
   * `TanggalMulai` (DATE, NOT NULL)
   * `TanggalSelesai` (DATE, NOT NULL)
   * `JumlahHari` (INT, NOT NULL)
   * `Alasan` (VARCHAR(500), NOT NULL)
   * `AlamatSelamaCuti` (VARCHAR(255), NULL)
   * `NomorTeleponDarurat` (VARCHAR(50), NULL)
   * `NamaDokumenPendukung` (VARCHAR(255), NULL)
   * `JalurDokumenPendukung` (VARCHAR(500), NULL)
   * `Status` (INT: 1=MenungguPersetujuanManager, 2=DisetujuiManager, 3=Disetujui, 4=Ditolak, 5=Dibatalkan)
   * Kolom Audit & Soft Delete
   * Index: `(PerusahaanId, KaryawanId)`, `(PerusahaanId, Status)`

8. **`PersetujuanCuti`**:
   * `Id` (GUID, PK)
   * `PerusahaanId` (GUID, FK `Perusahaan`)
   * `PengajuanCutiId` (GUID, FK `PengajuanCuti`, `OnDelete: Cascade`)
   * `Tahap` (INT: 1=Manager, 2=HRD)
   * `PenyetujuId` (GUID, FK `Pengguna`, `OnDelete: Restrict`)
   * `Keputusan` (INT: 1=Disetujui, 2=Ditolak)
   * `Catatan` (VARCHAR(500), NULL)
   * `WaktuPersetujuan` (DATETIMEOFFSET, NOT NULL)
   * Kolom Audit & Soft Delete
   * Index: `(PerusahaanId, PengajuanCutiId)`

---

### 3.4 Tabel Modul Penggajian (Fase 4 - Terimplementasi)

```
┌────────────────────────────────────────────────────────────────────────┐
│                              Perusahaan                                │
└───────┬──────────────────────────┬───────────────────────────┬─────────┘
        │ 1:N                      │ 1:N                       │ 1:1
        ▼                          ▼                           ▼
┌───────────────┐          ┌───────────────┐         ┌───────────────────────┐
│ KomponenGaji  │          │PeriodePenggaji│         │KebijakanPenggajianPer │
└───────┬───────┘          └───────┬───────┘         └───────────────────────┘
        │ 1:N                      │ 1:N
        ▼                          ▼
┌───────────────┐          ┌───────────────┐
│ GajiKaryawan  │          │  Penggajian   │◄─── (Karyawan)
└───────────────┘          └───────┬───────┘
                                   │ 1:N
                   ┌───────────────┴───────────────┐
                   ▼                               ▼
         ┌───────────────────┐           ┌───────────────────┐
         │ DetailPenggajian  │           │     SlipGaji      │
         └───────────────────┘           └───────────────────┘
```

1. **`KomponenGaji`**:
   * `Id` (GUID, PK)
   * `PerusahaanId` (GUID, FK `Perusahaan`)
   * `KodeKomponen` (VARCHAR(50), Unique per `PerusahaanId`)
   * `NamaKomponen` (VARCHAR(100), NOT NULL)
   * `Jenis` (INT: 1=Pendapatan, 2=Potongan)
   * `Sifat` (INT: 1=Tetap, 2=TidakTetap)
   * `Kategori` (INT: 1=GajiPokok, 2=TunjanganTetap, 3=TunjanganTidakTetap, 4=UangLembur, 5=BpjsKesehatan, 6=BpjsKetenagakerjaanJHT, 7=BpjsKetenagakerjaanJP, 8=BpjsKetenagakerjaanJKK, 9=BpjsKetenagakerjaanJKM, 10=PajakPPh21, 11=PotonganKeterlambatan, 12=KasbonPinjaman, 13=PotonganLainnya)
   * `NilaiBawaan` (DECIMAL(18,2), Default 0)
   * `KenaPajak` (BOOLEAN, Default TRUE)
   * `MasukKalkulasiBpjs` (BOOLEAN, Default FALSE)
   * `DihitungOtomatisOlehSistem` (BOOLEAN, Default FALSE)
   * `StatusAktif` (BOOLEAN, Default TRUE)
   * Kolom Audit & Soft Delete
   * Unique Index: `(PerusahaanId, KodeKomponen)`

2. **`GajiKaryawan`**:
   * `Id` (GUID, PK)
   * `PerusahaanId` (GUID, FK `Perusahaan`)
   * `KaryawanId` (GUID, FK `Karyawan`, `OnDelete: Cascade`)
   * `KomponenGajiId` (GUID, FK `KomponenGaji`, `OnDelete: Restrict`)
   * `Nominal` (DECIMAL(18,2), NOT NULL)
   * `StatusAktif` (BOOLEAN, Default TRUE)
   * `Keterangan` (VARCHAR(255), NULL)
   * Kolom Audit & Soft Delete
   * Unique Index: `(PerusahaanId, KaryawanId, KomponenGajiId)`

3. **`KebijakanPenggajianPerusahaan`**:
   * `Id` (GUID, PK)
   * `PerusahaanId` (GUID, FK `Perusahaan`, Unique 1:1)
   * `HitungLemburOtomatis` (BOOLEAN, Default TRUE)
   * `TarifLemburPerJamFlat` (DECIMAL(18,2), Default 0; 0=Rumus Kemnaker 1/173)
   * `TerapkanPotonganKeterlambatan` (BOOLEAN, Default TRUE)
   * `ToleransiKeterlambatanGratisMenit` (INT, Default 15)
   * `DendaKeterlambatanPerMenit` (DECIMAL(18,2), Default 1000)
   * `TerapkanBpjsKesehatan` (BOOLEAN, Default TRUE)
   * `PersenBpjsKesehatanPerusahaan` (DECIMAL(5,2), Default 4.00)
   * `PersenBpjsKesehatanPekerja` (DECIMAL(5,2), Default 1.00)
   * `BatasMaksimalUpahBpjsKesehatan` (DECIMAL(18,2), Default 12000000)
   * `TerapkanBpjsKetenagakerjaan` (BOOLEAN, Default TRUE)
   * `PersenBpjsJhtPerusahaan` (DECIMAL(5,2), Default 3.70)
   * `PersenBpjsJhtPekerja` (DECIMAL(5,2), Default 2.00)
   * `PersenBpjsJpPerusahaan` (DECIMAL(5,2), Default 2.00)
   * `PersenBpjsJpPekerja` (DECIMAL(5,2), Default 1.00)
   * `BatasMaksimalUpahBpjsJp` (DECIMAL(18,2), Default 10042300)
   * `PersenBpjsJkkPerusahaan` (DECIMAL(5,2), Default 0.24)
   * `PersenBpjsJkmPerusahaan` (DECIMAL(5,2), Default 0.30)
   * `TerapkanPph21` (BOOLEAN, Default TRUE)
   * `MetodePph21` (INT: 1=TER_PP58, 2=ProgresifPasal17, 3=GrossUp, 4=Nett)
   * `TanggalCutoffAwal` (INT, Default 26)
   * `TanggalCutoffAkhir` (INT, Default 25)
   * `TanggalPembayaranGaji` (INT, Default 28)
   * `CatatanSlipGajiDefault` (VARCHAR(500), NULL)
   * Kolom Audit & Soft Delete

4. **`PeriodePenggajian`**:
   * `Id` (GUID, PK)
   * `PerusahaanId` (GUID, FK `Perusahaan`)
   * `NamaPeriode` (VARCHAR(100), NOT NULL)
   * `Bulan` (INT, 1-12)
   * `Tahun` (INT, misal 2026)
   * `TanggalMulai` (DATE, NOT NULL)
   * `TanggalSelesai` (DATE, NOT NULL)
   * `TanggalPembayaran` (DATE, NOT NULL)
   * `Status` (INT: 1=Draf, 2=Diproses, 3=MenungguPersetujuan, 4=Disetujui, 5=Selesai, 6=Dibatalkan)
   * `TotalKaryawan` (INT, Default 0)
   * `TotalPengeluaranGaji` (DECIMAL(18,2), Default 0)
   * `Catatan` (VARCHAR(500), NULL)
   * `DisetujuiOlehId` (GUID, FK `Pengguna`, `OnDelete: SetNull`)
   * `WaktuDisetujui` (DATETIMEOFFSET, NULL)
   * Kolom Audit & Soft Delete
   * Unique Index: `(PerusahaanId, Tahun, Bulan)`

5. **`Penggajian`**:
   * `Id` (GUID, PK)
   * `PerusahaanId` (GUID, FK `Perusahaan`)
   * `PeriodePenggajianId` (GUID, FK `PeriodePenggajian`, `OnDelete: Cascade`)
   * `KaryawanId` (GUID, FK `Karyawan`, `OnDelete: Restrict`)
   * `NomorSlipGaji` (VARCHAR(50), NOT NULL)
   * `StatusPTKP` (INT: 1=TK0 s/d 8=K3)
   * `MetodePajak` (INT: 1=TER_PP58, dll)
   * `HariKerjaEfektif` (INT)
   * `JumlahHadir` (INT)
   * `JumlahTerlambat` (INT)
   * `TotalMenitTerlambat` (INT)
   * `JumlahJamLembur` (DECIMAL(5,2))
   * `GajiPokok` (DECIMAL(18,2), NOT NULL)
   * `TotalTunjanganTetap` (DECIMAL(18,2), NOT NULL)
   * `TotalTunjanganTidakTetap` (DECIMAL(18,2), NOT NULL)
   * `TotalUangLembur` (DECIMAL(18,2), NOT NULL)
   * `TotalPendapatanKotor` (DECIMAL(18,2), NOT NULL)
   * `TotalPotonganBpjs` (DECIMAL(18,2), NOT NULL)
   * `TotalPotonganPph21` (DECIMAL(18,2), NOT NULL)
   * `TotalPotonganKeterlambatan` (DECIMAL(18,2), NOT NULL)
   * `TotalPotonganLainnya` (DECIMAL(18,2), NOT NULL)
   * `TotalPotongan` (DECIMAL(18,2), NOT NULL)
   * `GajiBersih` (DECIMAL(18,2), NOT NULL)
   * `TotalBpjsDitanggungPerusahaan` (DECIMAL(18,2), NOT NULL)
   * `Status` (INT: 1=Draf, 2=Dihitung, 3=Disetujui, 4=Dibayarkan, 5=Dibatalkan)
   * `ApakahTerkunci` (BOOLEAN, Default FALSE)
   * `Catatan` (VARCHAR(500), NULL)
   * Kolom Audit & Soft Delete
   * Unique Index: `(PeriodePenggajianId, KaryawanId)`
   * Index: `(PerusahaanId, PeriodePenggajianId)`, `(PerusahaanId, KaryawanId)`

6. **`DetailPenggajian` (Snapshot Historis)**:
   * `Id` (GUID, PK)
   * `PerusahaanId` (GUID, FK `Perusahaan`)
   * `PenggajianId` (GUID, FK `Penggajian`, `OnDelete: Cascade`)
   * `KomponenGajiId` (GUID, FK `KomponenGaji`, `OnDelete: Restrict`)
   * `KodeKomponen` (VARCHAR(50), NOT NULL)
   * `NamaKomponen` (VARCHAR(100), NOT NULL)
   * `Jenis` (INT: Pendapatan / Potongan)
   * `Sifat` (INT: Tetap / TidakTetap)
   * `Kategori` (INT: Enum KategoriKomponenGaji)
   * `Nominal` (DECIMAL(18,2), NOT NULL)
   * `NominalDitanggungPerusahaan` (DECIMAL(18,2), Default 0)
   * `Keterangan` (VARCHAR(255), NULL)
   * `Urutan` (INT, Default 0)
   * Kolom Audit & Soft Delete
   * Index: `(PenggajianId)`

7. **`SlipGaji`**:
   * `Id` (GUID, PK)
   * `PerusahaanId` (GUID, FK `Perusahaan`)
   * `PenggajianId` (GUID, FK `Penggajian`, Unique 1:1, `OnDelete: Cascade`)
   * `NomorSlip` (VARCHAR(50), NOT NULL)
   * `KodeVerifikasi` (VARCHAR(50), NOT NULL, Unique)
   * `WaktuDiterbitkan` (DATETIMEOFFSET, NOT NULL)
   * `SudahDilihatKaryawan` (BOOLEAN, Default FALSE)
   * `WaktuDilihatKaryawan` (DATETIMEOFFSET, NULL)
   * `CatatanKaki` (VARCHAR(500), NULL)
   * Kolom Audit & Soft Delete
   * Unique Index: `(KodeVerifikasi)`, `(PenggajianId)`

---

## 4. Integritas Relasi & Constraint
* **Foreign Key**: Menjaga integritas referensial data.
* **Cascade Delete Rules**: Restrict / No Action pada entitas data master utama guna mencegah penghapusan data transaksi historis saat master dihapus.
* **Unique Constraints**:
  * `Perusahaan.KodePerusahaan`
  * `Karyawan.(PerusahaanId, NomorIndukKaryawan)`
  * `Departemen.(PerusahaanId, KodeDepartemen)`
  * `ShiftKerja.(PerusahaanId, KodeShift)`
  * `Kehadiran.(PerusahaanId, KaryawanId, Tanggal)`
  * `JenisCuti.(PerusahaanId, KodeJenisCuti)`
  * `SaldoCuti.(PerusahaanId, KaryawanId, JenisCutiId, PeriodeTahun)`
