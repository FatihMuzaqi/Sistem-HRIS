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

### 3.3 Tabel Modul Operasional SDM (Fase Lanjutan)
1. **`Kehadiran`**:
   * `Id`, `PerusahaanId` (FK), `KaryawanId` (FK), `Tanggal`, `JamMasuk`, `JamKeluar`, `StatusKehadiran`, `MenitKeterlambatan`.
2. **`PengajuanCuti`**:
   * `Id`, `PerusahaanId` (FK), `KaryawanId` (FK), `JenisCutiId` (FK), `TanggalMulai`, `TanggalSelesai`, `JumlahHari`, `StatusPersetujuan`.
3. **`Penggajian`**:
   * `Id`, `PerusahaanId` (FK), `KaryawanId` (FK), `PeriodeBulan`, `PeriodeTahun`, `GajiPokok`, `TotalTunjangan`, `TotalPotongan`, `GajiBersih`.

---

## 4. Integritas Relasi & Constraint
* **Foreign Key**: Menjaga integritas referensial data.
* **Cascade Delete Rules**: Restrict / No Action pada entitas data master utama guna mencegah penghapusan data transaksi historis saat master dihapus.
* **Unique Constraints**:
  * `Perusahaan.KodePerusahaan`
  * `Karyawan.(PerusahaanId, NIK)`
  * `Departemen.(PerusahaanId, KodeDepartemen)`
