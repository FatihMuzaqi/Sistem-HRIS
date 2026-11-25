# RIWAYAT PENGEMBANGAN — SISTEMHRD

Dokumen ini mencatat log kronologis tahapan implementasi, perubahan arsitektur, dan pencapaian milestone dalam pengembangan sistem **SistemHRD**.

---

## [Fase 0] — Analisis Workspace & Inisialisasi Fondasi
**Tanggal**: 15 November 2026

### Catatan & Hasil Analisis:
* Memeriksa kondisi awal workspace `c:\Users\ACER\Documents\PEMROGRAMMAN\.NET HRIS` (kondisi awal kosong).
* Mendeteksi dan memvalidasi SDK `.NET 10.0.404` dengan runtime `Microsoft.AspNetCore.App 10.0.15`.
* Membuat project ASP.NET Core MVC dengan target `net10.0` (`SistemHRD.csproj` & `SistemHRD.slnx`).
* Menganalisis ketersediaan database: Mengidentifikasi instance aktif **MySQL / MariaDB via XAMPP** pada port `3306` (`10.4.32-MariaDB`).
* Menyiapkan dokumentasi awal dan pengujian build pertama.

---

## [Fase 1] — Fondasi Sistem, Autentikasi, Multi-Tenant, Role, Dashboard & UI Emerald (SELESAI)
**Tanggal**: 15 November 2026

### Rincian Implementasi:
1. **Database & Entity Framework Core**:
   * Mengonfigurasi `KonteksAplikasi` mewarisi `IdentityDbContext<Pengguna, Peran, Guid>`.
   * Menerapkan model `Perusahaan` (Master Tenant), `Pengguna` (Identity), `Peran` (Roles).
   * Menerapkan relasi `PenggunaPerusahaan` (Many-to-Many antara akun pengguna dan entitas bisnis).
   * Menerapkan model `AuditLog` untuk pencatatan otomatis aktivitas penting (Login, Logout, Gagal, Modifikasi).
   * Mengonfigurasi Global Query Filter otomatis untuk `ITenantEntity` dan `ISoftDelete`.
   * Menerapkan migration EF Core:
     - `20261115082106_InisialisasiAwal`
     - `20261115083704_TambahAuditLogDanPenggunaPerusahaan`
   * Menerapkan data seeder (`InisialisasiData.cs`) yang menginisialisasi 5 peran pengguna, 2 perusahaan (`PRSH001` dan `PRSH002`), serta 6 akun pengguna uji coba.

2. **Authentication & Authorization**:
   * Mengembangkan `AutentikasiController` dengan rute `/Autentikasi/Masuk`, `/Autentikasi/Keluar`, `/Autentikasi/AksesDitolak`.
   * Menerapkan proteksi CSRF (Anti-Forgery Token).
   * Menerapkan password hashing menggunakan ASP.NET Core Identity.
   * Redirect dinamis ke `/Dasbor` sesuai peran pengguna.
   * Proteksi seluruh endpoint privat menggunakan atribut `[Authorize]`.

3. **Multi-Tenant Engine**:
   * Mekanisme `ILayananTenant` dan `LayananTenant` untuk mengekstrak dan mengisolasi konteks perusahaan aktif.
   * `TenantMiddleware` memvalidasi konteks tenant pada setiap daur hidup request HTTP.
   * Pengguna tenant hanya dapat mengakses data perusahaan miliknya; query EF Core secara otomatis membatasi data via server-side filter.
   * `SuperAdmin` memiliki akses platform-level lintas perusahaan serta fitur alih konteks (`GantiPerusahaanAktif`).

4. **Role-Based System**:
   * 5 Peran terdaftar dan aktif: `SuperAdmin`, `HRD`, `Manager`, `Finance`, `Karyawan`.

5. **Dashboard, Layout & UI**:
   * Layout responsif `_Layout.cshtml`, `_SidebarPartial.cshtml`, `_NavbarPartial.cshtml`, `_NotifikasiPartial.cshtml`.
   * Standar desain profesional: Emerald Green (`#059669` / `#10b981`), putih, dan abu-abu terang (`#f8fafc`).
   * Integrasi **Lucide Icons** dan **Chart.js**.
   * Kartu statistik dinamis berbasis peran (SuperAdmin Platform vs Tenant HRD/Manager/Finance/Karyawan).
   * Halaman login modern `/Autentikasi/Masuk` dilengkapi tombol uji coba cepat (One-Click Testing Helpers).
   * Halaman manajemen profil dan ganti kata sandi `/Pengguna/Profil`.

### Hasil Pengujian Otomatis (100% Lulus):
1. **Uji 1 (Akses Tanpa Login)**: Akses `/Dasbor` tanpa autentikasi langsung dialihkan dengan HTTP `302 Redirect` ke `/Autentikasi/Masuk?ReturnUrl=%2FDasbor`. *(LULUS)*
2. **Uji 2 (Login Gagal)**: Input kata sandi salah berhasil ditolak, form menampilkan pesan kesalahan, dan audit log mencatat `LOGIN_GAGAL`. *(LULUS)*
3. **Uji 3 (Login SuperAdmin)**: Berhasil login, menerima cookie auth, dan dasbor memuat statistik seluruh platform. *(LULUS)*
4. **Uji 4 (Logout)**: Sesi auth berhasil diakhiri dan dialihkan kembali ke login. *(LULUS)*
5. **Uji 5 (Isolasi Dua Perusahaan)**:
   - HRD 1 (`PRSH001`) hanya melihat perusahaannya sendiri dan akun miliknya. Data `PRSH002` (Siti Rahma) 100% terisolasi dan tidak bocor.
   - HRD 2 (`PRSH002`) hanya melihat perusahaannya sendiri. Data `PRSH001` (Budi Santoso) 100% terisolasi. *(LULUS)*
6. **Uji 6 (Manipulasi Otorisasi / Cross-Tenant)**: Percobaan HRD memanggil endpoint pergantian tenant platform langsung diblokir dan dialihkan ke `/Autentikasi/AksesDitolak`. *(LULUS)*
7. **Uji 7 (Profil Pengguna)**: Halaman profil `/Pengguna/Profil` memuat data diri, status tenant, dan form ubah kata sandi dengan benar. *(LULUS)*

---

## [Fase 2] — Modul Manajemen Karyawan, Departemen & Jabatan (SELESAI)
**Tanggal**: 15 November 2026

### Rincian Implementasi:
1. **Model & Entitas Bisnis**:
   * Menambahkan 7 entitas utama dengan dukungan multi-tenant (`ITenantEntity`), audit (`IAuditEntity`), dan soft delete (`ISoftDelete`):
     - `Departemen`: Master unit kerja per perusahaan dengan kepala departemen dan deskripsi.
     - `Jabatan`: Master posisi/jabatan dengan level hierarki (1-5: Direksi s/d Staf) dan relasi ke departemen.
     - `Karyawan`: Data profil lengkap pegawai (NIK unik per tenant, identitas diri, status kepegawaian, kontak, foto profil).
     - `KontrakKerja`: Manajemen riwayat dan status kontrak (PKWT, PKWTT, Magang, Percobaan).
     - `DokumenKaryawan`: Manajemen berkas digital (KTP, NPWP, Ijazah, CV, Sertifikat, Kontrak) dengan penyimpanan aman di luar web root.
     - `KontakDaruratKaryawan`: Daftar nomor dan relasi keluarga/kontak darurat.
     - `RiwayatPekerjaan`: Rekam jejak karier sebelumnya atau promosi/mutasi internal.
   * Menambahkan Enum pendukung di `Models/Enums/EnumsKaryawan.cs`: `JenisKelamin`, `Agama`, `StatusPernikahan`, `StatusKepegawaian`, `JenisKontrak`, `StatusKontrak`, `JenisDokumenKaryawan`, `LevelJabatan`.

2. **Entity Framework Core & Database**:
   * Konfigurasi Fluent API di `Data/KonteksAplikasi.cs` dengan indeks unik:
     - `(PerusahaanId, KodeDepartemen)` Unique.
     - `(PerusahaanId, NIK)` Unique.
     - `(PerusahaanId, Email)` Index pencarian cepat.
   * Penerapan Global Query Filter eksplisit yang aman dan translatable ke SQL untuk seluruh 7 entitas tenant.
   * Migrasi EF Core: `20261115112102_TambahModulKaryawanLengkap`.
   * Data Seeder: Menginisialisasi departemen, jabatan, dan profil karyawan contoh untuk PT Solusi HR Nusantara (`PRSH001`) dan PT Maju Bersama Digital (`PRSH002`).

3. **Service Layer & Arsitektur Bisnis**:
   * `ILayananDepartemen` & `LayananDepartemen`: Operasi CRUD departemen dengan audit log otomatis.
   * `ILayananJabatan` & `LayananJabatan`: Operasi CRUD jabatan, filter cascading berdasarkan departemen.
   * `ILayananKaryawan` & `LayananKaryawan`: Operasi komprehensif data karyawan, manajemen sub-entitas (kontrak, dokumen, kontak darurat, riwayat pekerjaan), penyimpanan berkas aman di `App_Data/DokumenKaryawan`, validasi format/ukuran file, dan unduhan berotorisasi.

4. **Controllers & Endpoints**:
   * `DepartemenController`: Indeks, Tambah, Ubah, Hapus (soft-delete).
   * `JabatanController`: Indeks, Tambah, Ubah, Hapus (soft-delete), dan API Cascading `/Jabatan/DapatkanBerdasarkanDepartemen` via JSON AJAX.
   * `KaryawanController`: Indeks (pencarian, multi-filter, pagination), Tambah (upload foto), Ubah, Detail (tabbed view), UbahStatus, TambahKontrak, TambahKontakDarurat, TambahRiwayatPekerjaan, UploadDokumen, UnduhDokumen (secure streaming), HapusDokumen.

5. **Antarmuka Pengguna (UI/UX Emerald Theme)**:
   * View Razor responsif dengan gaya Emerald Green (`#059669`), kartu metrik, avatar inisial/foto profil, badge status interaktif.
   * Tampilan detail karyawan dengan 5 tab interaktif: *Data Pribadi, Kontrak Kerja, Dokumen Digital, Kontak Darurat, Riwayat Pekerjaan*.
   * Form modal dinamis untuk penambahan kontrak, dokumen, kontak, dan riwayat pekerjaan.
   * Navigasi sidebar terintegrasi dengan penanda menu aktif otomatis.

### Hasil Pengujian Integrasi Otomatis (13 Skenario Lulus 100%):
1. **Uji 1 (Login HRD 1)**: Login HRD PT Solusi HR Nusantara (`hrd@solusihr.id`) berhasil terotentikasi. *(LULUS)*
2. **Uji 2 (Master Departemen)**: Data master departemen awal (IT, HRD, FIN) tampil dengan benar. *(LULUS)*
3. **Uji 3 (Tambah Departemen)**: Departemen baru berhasil disimpan dengan validasi kode unik per tenant. *(LULUS)*
4. **Uji 4 (Master Jabatan & Cascading API)**: Daftar jabatan tampil terstruktur; endpoint AJAX cascading mengembalikan JSON yang tepat untuk dropdown dinamis. *(LULUS)*
5. **Uji 5 (Daftar Karyawan, Pencarian & Filter)**: Pencarian kata kunci, filter multi-kriteria, dan tampilan empty state saat hasil nihil berfungsi normal. *(LULUS)*
6. **Uji 6 (Tambah Karyawan)**: Registrasi karyawan baru berhasil disimpan ke basis data dengan NIK unik. *(LULUS)*
7. **Uji 7 (Halaman Detail Karyawan)**: Halaman detail karyawan dengan tab informasi ter-render lengkap dan akurat. *(LULUS)*
8. **Uji 8 (Tambah Kontrak Kerja)**: Kontrak kerja baru berhasil dicatat dan terikat ke karyawan. *(LULUS)*
9. **Uji 9 (Tambah Kontak Darurat)**: Kontak darurat keluarga berhasil ditambahkan. *(LULUS)*
10. **Uji 10 (Tambah Riwayat Pekerjaan)**: Riwayat karier sebelumnya berhasil dicatat. *(LULUS)*
11. **Uji 11 (Unggah & Unduh Dokumen)**: Dokumen digital berhasil diunggah dengan validasi tipe berkas dan dapat diunduh kembali dengan integritas byte valid. *(LULUS)*
12. **Uji 12 (Ubah Status Aktif/Nonaktif)**: Status karyawan berhasil diperbarui dari aktif ke nonaktif dan sebaliknya. *(LULUS)*
13. **Uji 13 (Keamanan Multi-Tenant & Isolasi Data)**:
    - HRD Perusahaan 2 hanya melihat karyawannya sendiri di tabel karyawan.
    - HRD Perusahaan 2 tidak dapat melihat departemen Perusahaan 1.
    - Akses langsung via URL ke ID karyawan milik Perusahaan 1 ditolak oleh Global Query Filter (Data tidak ditemukan).
    - Percobaan unduh dokumen milik Perusahaan 1 oleh HRD Perusahaan 2 ditolak dengan status HTTP 404/403. *(LULUS)*

---

## [Fase 3] — Modul Kehadiran dan Pengajuan Cuti (Attendance & Leave Management)
**Status**: ✅ SELESAI (100% Lulus Pengujian Integrasi Otomatis)

### Ringkasan Pekerjaan:
1. **Model & Enums**:
   * Enums (`Models/Enums/EnumsKehadiranDanCuti.cs`): `StatusKehadiran`, `StatusPengajuanLembur`, `StatusPengajuanCuti`, `TahapPersetujuanCuti`, `KeputusanPersetujuan`.
   * Entitas Kehadiran: `ShiftKerja`, `JadwalKerja`, `Kehadiran`, `PengajuanLembur`.
   * Entitas Cuti: `JenisCuti`, `SaldoCuti`, `PengajuanCuti`, `PersetujuanCuti`.
   * Relasi & Navigation properties pada `Karyawan` (`DaftarKehadiran`, `DaftarJadwalKerja`, `DaftarPengajuanLembur`, `DaftarSaldoCuti`, `DaftarPengajuanCuti`).

2. **Entity Framework Core & Database**:
   * Migrasi EF Core: `TambahModulKehadiranDanCuti` diterapkan ke MySQL / MariaDB (`sistemhrd_db`).
   * Konfigurasi Fluent API: Relasi foreign key, composite indexes unik (`PerusahaanId, KodeShift`, `PerusahaanId, KaryawanId, Tanggal`, `PerusahaanId, KodeJenisCuti`, `PerusahaanId, KaryawanId, JenisCutiId, PeriodeTahun`).
   * Global Query Filter untuk isolasi data tenant pada seluruh 8 entitas baru.
   * Data Seeding: Inisialisasi shift kerja baku, master jenis cuti reguler, dan inisialisasi kuota saldo cuti 2026.

3. **Service Layer**:
   * `ILayananKehadiran` & `LayananKehadiran`: Kalkulasi presensi masuk/pulang, toleransi keterlambatan menit, jam kerja riil, CRUD shift, pengajuan lembur, dan verifikasi lembur oleh Manager.
   * `ILayananCuti` & `LayananCuti`: Validasi bentrok jadwal, pengecekan ketersediaan saldo cuti, unggah dokumen pendukung berukuran s/d 5MB dengan ekstensi terverifikasi, alur persetujuan bertingkat (Manager -> HRD), transaksi database atomik (`BeginTransactionAsync`), audit log, dan pencegahan pemotongan ganda (idempotency).

4. **Controllers & Razor Views**:
   * `KehadiranController`: Halaman utama presensi, Employee Self-Service (ESS) clock-in/clock-out interaktif, rekapitulasi kehadiran bulanan, master shift kerja, dan manajemen lembur.
   * `CutiController`: Pengajuan cuti karyawan (dengan upload berkas), detail permohonan dengan riwayat alur persetujuan, otorisasi persetujuan Manager & verifikasi HRD, unduh dokumen berotorisasi, master jenis cuti, dan manajemen saldo cuti tahunan.
   * UI Emerald Green (`#059669`) responsif, kartu statistik antrean, badge status, modal interaktif, dan notifikasi flash alert.

### Hasil Pengujian Integrasi Otomatis (19 Skenario Lulus 100%):
1. **Uji 1 (Login HRD)**: Autentikasi HRD PT Solusi HR Nusantara (`hrd@solusihr.id`) berhasil. *(LULUS)*
2. **Uji 2 (Master Shift Kerja)**: Tampilan daftar master shift kerja aktif (REG 08:00 - 17:00). *(LULUS)*
3. **Uji 3 (Tambah Shift Baru)**: Registrasi shift malam operasional (`SFT-MALAM`) berhasil disimpan. *(LULUS)*
4. **Uji 4 (Master Jenis Cuti)**: Jenis cuti reguler (CT-THN, CT-SKT, CT-MLH, CT-NIKAH, CT-DUKA) terkonfigurasi. *(LULUS)*
5. **Uji 5 (Saldo Cuti 2026)**: Saldo hak cuti tahunan karyawan Rian Pratama terverifikasi (12 Hari). *(LULUS)*
6. **Uji 6 (Login Karyawan)**: Autentikasi akun ESS karyawan (`karyawan@solusihr.id`) berhasil. *(LULUS)*
7. **Uji 7 (Layar Presensi ESS)**: Profil karyawan, jam shift aktif, dan widget jam real-time ter-render. *(LULUS)*
8. **Uji 8 (Check-In Presensi)**: Check-In tercatat beserta lokasi, waktu, dan kalkulasi status kehadiran. *(LULUS)*
9. **Uji 9 (Pencegahan Check-In Ganda)**: Sistem menolak percobaan check-in ganda di hari yang sama. *(LULUS)*
10. **Uji 10 (Check-Out Presensi)**: Check-Out tersimpan beserta penghitungan durasi jam kerja riil. *(LULUS)*
11. **Uji 11 (Pengajuan Lembur)**: Pengajuan lembur 3.0 jam berhasil didaftarkan karyawan. *(LULUS)*
12. **Uji 12 (Persetujuan Lembur Manager)**: Manager login dan menyetujui lembur secara instan. *(LULUS)*
13. **Uji 13 (Pengajuan Cuti Karyawan)**: Permohonan cuti 2 hari beserta lampiran dokumen berhasil dibuat. *(LULUS)*
14. **Uji 14 (Validasi Bentrok Jadwal Cuti)**: Sistem menolak pengajuan baru yang bertabrakan dengan rentang tanggal cuti aktif. *(LULUS)*
15. **Uji 15 (Persetujuan Manager Tahap 1)**: Manager meninjau dan menyetujui cuti; status beralih ke `Menunggu HRD`. *(LULUS)*
16. **Uji 16 (Verifikasi HRD & Potong Saldo Atomik)**: HRD memverifikasi permohonan cuti; transaksi database memotong saldo cuti dari 12 menjadi 10 hari dan status beralih ke `Disetujui (Selesai)`. *(LULUS)*
17. **Uji 17 (Uji Idempotensi Approval HRD)**: Persetujuan ulang tidak memotong saldo cuti dua kali (saldo tetap 10 hari). *(LULUS)*
18. **Uji 18 (Unduh Berkas Lampiran)**: Pengunduhan berkas dokumen cuti berhasil dengan integritas byte valid. *(LULUS)*
19. **Uji 19 (Isolasi Multi-Tenant)**:
    - HRD Perusahaan 2 (`hrd2@majubersama.id`) tidak melihat pengajuan cuti Perusahaan 1 di daftar.
    - Percobaan akses langsung via URL ke ID cuti Perusahaan 1 ditolak oleh Global Query Filter.
    - Percobaan unduh berkas lampiran cuti Perusahaan 1 oleh HRD 2 menghasilkan HTTP 404 (Not Found / Protected). *(LULUS)*

---

## [Fase 4] — Modul Penggajian (Payroll) Lengkap (SELESAI)
**Tanggal**: 16 November 2026

### Rincian Implementasi:
1. **Model & Entitas Database**:
   * Menambahkan 7 entitas penggajian komprehensif (`ITenantEntity`, `IAuditEntity`, `ISoftDelete`):
     - `KomponenGaji`: Master komponen gaji (Pendapatan/Potongan, Tetap/Tidak Tetap, Kategori, Kena Pajak, Masuk BPJS, Otomatis Sistem).
     - `GajiKaryawan`: Struktur gaji dasar per karyawan (Gaji Pokok, Tunjangan Jabatan, Transport, Makan, Bonus).
     - `KebijakanPenggajianPerusahaan`: Konfigurasi per tenant (rumus lembur flat vs Kemnaker 1/173, toleransi & denda keterlambatan per menit, tarif persentase & plafon upah BPJS Kesehatan/JHT/JP/JKK/JKM, dan TER PPh 21 PP 58/2023).
     - `PeriodePenggajian`: Siklus cutoff bulanan (tanggal cutoff awal/akhir, tanggal pembayaran gaji, status Draf/Diproses/MenungguPersetujuan/Disetujui/Selesai).
     - `Penggajian`: Rekap payroll karyawan per periode dengan field nominal `decimal(18,2)`, rekap kehadiran/lembur/terlambat, status, dan flag penguncian (`ApakahTerkunci`).
     - `DetailPenggajian`: Snapshot historis seluruh komponen pendapatan & potongan yang dibekukan saat payroll diproses.
     - `SlipGaji`: Metadata penerbitan slip resmi, kode verifikasi unik (`VERIF-XXXXXXXX`), timestamp penerbitan, tracking waktu lihat ESS karyawan, dan catatan kaki.
   * Enums Pendukung: `JenisKomponenGaji`, `SifatKomponenGaji`, `KategoriKomponenGaji`, `StatusPeriodePenggajian`, `StatusPenggajian`, `MetodePajakPPh21`, `StatusPTKP`.
   * Migrasi EF Core: `TambahModulPenggajianLengkap` (diterapkan bersih ke MariaDB `sistemhrd_db`).

2. **Mesin Kalkulasi Gaji Murni (`ILayananKalkulatorGaji`)**:
   * Terpisah dari layer controller dan tampilan (Clean Separation of Concerns).
   * Menghitung upah lembur Kemnaker: `(1.5 x JamPertama + 2.0 x JamBerikutnya) x (1/173 x UpahDasar)`.
   * Menghitung denda keterlambatan dengan toleransi gratis.
   * Menghitung BPJS Kesehatan: 1% pekerja, 4% pemberi kerja, dengan batas plafon upah Rp 12.000.000.
   * Menghitung BPJS Ketenagakerjaan: JHT (2% pekerja, 3.7% perusahaan), JP (1% pekerja, 2% perusahaan, plafon Rp 10.042.300), JKK (0.24%), JKM (0.30%).
   * Menghitung Pajak Penghasilan PPh 21 menggunakan regulasi resmi **PP No. 58 Tahun 2023** (Tabel Tarif Efektif Rata-Rata Bulanan TER A, TER B, dan TER C berdasarkan status PTKP TK/0 s/d K/3).

3. **Service Layer & Arsitektur Transaksi (`ILayananPenggajian`)**:
   * Pengelolaan master komponen gaji & audit log otomatis.
   * Konfigurasi aturan tarif dan siklus payroll per tenant.
   * Pengaturan struktur gaji per karyawan (NIK, PTKP, NPWP, Bank, Rekening, BPJS).
   * Pembuatan siklus periode penggajian bulanan.
   * Kalkulasi batch otomatis terintegrasi langsung dengan database presensi, lembur disetujui, dan denda terlambat.
   * Transaksi database atomik (`BeginTransactionAsync`) menjamin konsistensi data saat generate batch dan approval.
   * Pencegahan duplikasi payroll (kombinasi `PeriodePenggajianId` dan `KaryawanId` unik, re-kalkulasi memperbarui data eksisting).
   * Workflow persetujuan & penguncian mutlak: payroll yang telah disetujui dibekukan (`ApakahTerkunci = true`), penerbitan slip resmi, dan penolakan kalkulasi ulang.
   * Ekspor laporan rekapitulasi penggajian dalam format CSV standar transfer perbankan.

4. **Controllers & Razor Views**:
   * `PenggajianController`: Dashboard periode payroll, form tambah periode, detail review rincian per karyawan, eksekusi batch, approval & lock, cetak slip resmi, riwayat ESS karyawan, ekspor CSV, master komponen, form atur gaji karyawan, dan konfigurasi kebijakan tenant.
   * Desain slip gaji profesional responsif, dilengkapi kop perusahaan, logo, watermark confidential, tabel rincian pendapatan & potongan, fungsi konversi bilangan rupiah ke teks (`HelperTerbilang`), kode verifikasi, serta optimasi cetak browser `@media print` untuk simpan ke PDF.

5. **Unit Tests (`SistemHRD.Tests`)**:
   * 9 Unit test kalkulasi murni menggunakan xUnit (`UjiKalkulatorGaji.cs`):
     - Kemnaker overtime formula (1/173).
     - Flat overtime rate.
     - Denda keterlambatan dengan toleransi gratis.
     - BPJS Kesehatan normal & penegakan plafon upah (12 juta).
     - BPJS Ketenagakerjaan JHT & JP (plafon 10.042.300).
     - PPh 21 TER A (penghasilan kena tarif 0% s/d tarif bertingkat).
     - PPh 21 TER B.
     - PPh 21 TER C.
     - Rekap total take home pay.
   * Hasil: **Passed: 9, Failed: 0, Skipped: 0 (100% Lulus)**.

6. **Pengujian Integrasi End-to-End (`test_phase4.ps1`)**:
   * 17 Skenario Pengujian Otomatis:
     1. Login Finance & HRD Tenant 1. *(LULUS)*
     2. Akses Master Komponen Gaji. *(LULUS)*
     3. Tambah Komponen Gaji Baru (`BONUS-PROYEK`). *(LULUS)*
     4. Akses & Simpan Kebijakan Penggajian Tenant. *(LULUS)*
     5. Konfigurasi Gaji Karyawan Rian Pratama. *(LULUS)*
     6. Buat Periode Penggajian November 2026. *(LULUS)*
     7. Eksekusi Hitung Gaji Batch Otomatis. *(LULUS)*
     8. Verifikasi Snapshot Komponen & Rincian Penggajian. *(LULUS)*
     9. Verifikasi Pencegahan Duplikasi Payroll. *(LULUS)*
     10. Persetujuan & Penerbitan Payroll Periode. *(LULUS)*
     11. Verifikasi Payroll Terkunci (Perhitungan ulang ditolak). *(LULUS)*
     12. Tampilkan Slip Gaji Resmi & Kode Verifikasi. *(LULUS)*
     13. Ekspor Laporan Rekap Payroll CSV. *(LULUS)*
     14. Login Karyawan ESS & Akses Riwayat Slip Gaji. *(LULUS)*
     15. Karyawan Membuka Slip Gaji Milik Sendiri. *(LULUS)*
     16. Proteksi ESS: Karyawan Dilarang Mengintip Slip Rekan Lain. *(LULUS)*
     17. Uji Isolasi Multi-Tenant: Tenant 2 Dilarang Akses Periode & Ekspor Payroll Tenant 1. *(LULUS)*

---

## [Fase 5] — Modul Rekrutmen & Penilaian Kinerja (SELESAI)
**Tanggal**: 26 November 2026

### Rincian Implementasi:
1. **Model Database & Entitas (`Models/Entitas/`)**:
   * Modul Rekrutmen:
     - `LowonganKerja`: Posisi lowongan pekerjaan terbuka, kode lowongan unik, departemen, jabatan, kuota rekrutmen, rentang estimasi gaji, kualifikasi, deskripsi kerja, lokasi kerja, tanggal buka/tutup, status (`Draf`, `Dibuka`, `Ditutup`, `Dibatalkan`).
     - `TahapanSeleksi`: Rangkaian alur seleksi hiring per lowongan kerja (`Administrasi`, `TesTertulis`, `WawancaraHR`, `WawancaraUser`, `TesTeknis`, `MedicalCheckUp`, `Offering`), urutan tahapan, status aktif.
     - `Pelamar`: Profil kandidat, nomor registrasi unik (`REG-YYYYMM-XXXX`), lowongan dituju, data kontak, pendidikan, institusi, jurusan, tahun pengalaman, ekspektasi gaji, berkas CV (PDF/DOCX max 5MB tersimpan aman di `uploads/cv/{CompanyId}`), status kandidat (`Baru`, `SeleksiAdministrasi`, `LolosAdministrasi`, `Wawancara`, `PenawaranKerja`, `Diterima`, `Ditolak`).
     - `JadwalWawancara`: Undangan wawancara terstruktur, pewawancara, tanggal, jam mulai/selesai, metode (`TatapMuka`, `Daring`), lokasi atau link Google Meet/Zoom, catatan undangan, status jadwal (`Dijadwalkan`, `Selesai`, `Dibatalkan`, `Reschedule`).
     - `HasilSeleksi`: Catatan ulasan penilai, skor evaluasi seleksi (0 - 100), kelebihan, kekurangan, dan keputusan (`Lolos`, `Dipertimbangkan`, `TidakLolos`).
   * Modul Penilaian Kinerja (KPI):
     - `PeriodePenilaian`: Siklus evaluasi performa berkala per tenant (`Semester 1`, `Semester 2`, `Tahunan`, `Kuartalan`), rentang tanggal mulai & selesai, status periode (`Draf`, `Aktif`, `PenilaianSelesai`, `Ditutup`), instruksi/catatan.
     - `IndikatorKPI`: Master tolok ukur evaluasi kinerja objektif per departemen/jabatan atau umum, kode indikator, jenis (`Kuantitatif`, `Kualitatif`, `Perilaku`, `Proyek`), target capaian, satuan, bobot default (%), deskripsi panduan evaluasi.
     - `PenilaianKinerja`: Lembar evaluasi resmi performa karyawan per periode, NIK, penilai, tanggal evaluasi, total skor akhir tertimbang (0.00 - 100.00), predikat kinerja (`Istimewa` [A: $\ge 90$], `Baik` [B: $\ge 80$], `Cukup` [C: $\ge 70$], `Kurang` [D: $\ge 60$], `SangatKurang` [E: $< 60$]), ulasan evaluasi, rekomendasi pengembangan, status (`Draf`, `MenungguPersetujuan`, `Disetujui`).
     - `DetailPenilaian`: Rincian penilaian butir indikator KPI (target, realisasi, bobot %, skor indikator, skor tertimbang, catatan capaian).
   * Migrasi EF Core: `TambahModulRekrutmenDanPenilaianKinerja` diterapkan bersih ke database MySQL/MariaDB `sistemhrd_db` (9 tabel baru, foreign keys terisolasi per `PerusahaanId`, unique indexes).

2. **Service Layer & Logika Bisnis Terisolasi**:
   * `ILayananRekrutmen` & `LayananRekrutmen`:
     - CRUD lowongan kerja, pengelolaan tahapan pipeline seleksi, registrasi pelamar dengan validasi format & ukuran file CV, download berkas CV aman berbasis otorisasi tenant, alur seleksi administrasi, penjadwalan interview, perekaman skor wawancara, dan konversi atomik kandidat diterima menjadi entitas `Karyawan` dalam transaksi database (`BeginTransactionAsync`).
   * `ILayananPenilaianKinerja` & `LayananPenilaianKinerja`:
     - Pengaturan periode penilaian dan master indikator KPI.
     - **Mesin Perhitungan Skor Tertimbang Objektif**:
       $$\text{SkorTertimbang}_i = \frac{\text{BobotPersen}_i}{\text{TotalBobot}} \times \text{SkorIndikator}_i$$
       $$\text{TotalSkorAkhir} = \sum \text{SkorTertimbang}_i$$
     - Penegakan aturan: *"Jangan membuat skor kinerja yang tidak memiliki dasar indikator penilaian"* (pemberian nilai wajib menyertakan rincian indikator KPI aktif).
     - Otomasi penentuan predikat kinerja resmi (Istimewa/Baik/Cukup/Kurang/Sangat Kurang).
     - Dashboard analitik KPI terintegrasi dengan ringkasan metrik perusahaan.

3. **Controllers & Razor Views**:
   * `RekrutmenController`: Dashboard rekrutmen dengan Chart.js (Doughnut status pelamar & Bar kandidat per lowongan), daftar lowongan, form tambah/ubah lowongan, detail lowongan (tab deskripsi, pelamar, pipeline), daftar pelamar, registrasi pelamar + upload CV multipart, detail pelamar, modal seleksi berkas, modal jadwal wawancara, modal evaluasi seleksi, modal penerimaan & hiring karyawan, unduh CV.
   * `KinerjaController`: Dashboard kinerja dengan Chart.js (Doughnut distribusi predikat A-E & Bar rata-rata skor per departemen), kelola periode, form tambah/ubah periode, master indikator KPI, form input penilaian interaktif dengan kalkulasi skor live di sisi browser dan server, lembar hasil evaluasi resmi siap cetak (`@media print`), laporan analitik performa kerja, serta portal ESS `RiwayatSaya`.
   * Integrasi navigasi sidebar: Menu `Rekrutmen & Pelamar` dan `Penilaian Kinerja` untuk HRD/Manager, serta menu `Evaluasi Kinerja Saya` pada Employee Self Service.

4. **Unit Tests (`SistemHRD.Tests`)**:
   * Penambahan 14 test case baru di `UjiPenilaianKinerja.cs` melengkapi total 23 unit tests xUnit:
     - Skor seluruh indikator maksimal menghasilkan Predikat Istimewa (100.00 -> Istimewa).
     - Kalkulasi skor tertimbang dengan bobot beragam akurat secara matematis.
     - Normalisasi proporsional otomatis saat total bobot input tidak tepat 100%.
     - Penolakan & pelemparan exception jika evaluasi diajukan tanpa adanya butir indikator KPI.
     - Pengujian nilai ambang batas predikat (boundary tests): 95, 90, 89.9, 80, 79.9, 70, 69.9, 60, 59.9, 40.
   * Hasil: **Passed: 23, Failed: 0, Skipped: 0 (100% Lulus)**.

5. **Pengujian Integrasi End-to-End (`test_phase5.ps1`)**:
   * 24 Skenario Pengujian Otomatis:
     1. Login HRD Tenant 1 (PT Solusi HR Nusantara). *(LULUS)*
     2. Login Manager Tenant 1. *(LULUS)*
     3. Login Karyawan Tenant 1. *(LULUS)*
     4. Login HRD Tenant 2 (PT Maju Bersama Digital). *(LULUS)*
     5. Akses Dashboard Rekrutmen (Statistik & Chart.js). *(LULUS)*
     6. Akses Daftar Lowongan Kerja. *(LULUS)*
     7. Tambah Lowongan Kerja Baru Berhasil. *(LULUS)*
     8. Pendaftaran Pelamar + Upload Berkas CV Berhasil. *(LULUS)*
     9. Seleksi Administrasi (Loloskan Berkas). *(LULUS)*
     10. Unduh Berkas CV Berhasil Sesuai Hak Akses Tenant. *(LULUS)*
     11. Jadwalkan Wawancara Kandidat. *(LULUS)*
     12. Catat Evaluasi & Skor Seleksi Objektif (89.5). *(LULUS)*
     13. Penerimaan Kandidat & Konversi Atomik ke Karyawan. *(LULUS)*
     14. Akses Dashboard Penilaian Kinerja (Chart.js KPI). *(LULUS)*
     15. Tambah Periode Penilaian Kinerja Aktif. *(LULUS)*
     16. Tambah Master Indikator KPI Berbobot. *(LULUS)*
     17. Perhitungan Skor Tertimbang Objektif (94.0 -> Predikat Istimewa). *(LULUS)*
     18. Akses Lembar Resmi Penilaian Kinerja (Print Ready). *(LULUS)*
     19. Akses Laporan & Analitik Performa Karyawan. *(LULUS)*
     20. ESS Karyawan Mengakses Riwayat Evaluasi Diri Sendiri. *(LULUS)*
     21. ESS Karyawan Diblokir Mengakses Evaluasi Rekan Kerja Lain. *(LULUS)*
     22. Multi-Tenant: Tenant 2 Dilarang Mengakses Lowongan Tenant 1. *(LULUS)*
     23. Multi-Tenant: Tenant 2 Dilarang Mengakses Pelamar Tenant 1. *(LULUS)*
     24. Multi-Tenant: Tenant 2 Dilarang Mengakses Lembar Penilaian Tenant 1. *(LULUS)*
   * Hasil: **Passed: 24, Failed: 0 (100% Lulus)**.

---

## [Fase 6] — Modul Pelatihan, Sertifikasi & Offboarding
**Status**: ⏳ Siap Dikerjakan
* Modul Pelatihan & Sertifikasi Karyawan (Training & Development)
* Modul Offboarding & Resign / Terminasi Karyawan
* Surat Keterangan Kerja (Paklaring)
* Serah Terima Aset & Inventaris Perusahaan
