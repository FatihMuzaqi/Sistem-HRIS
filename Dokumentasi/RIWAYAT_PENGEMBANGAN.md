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

## [Fase 3] — Modul Presensi & Kehadiran (Attendance)
**Status**: ⏳ Siap Dikerjakan
* Perancangan model `JadwalKerja`, `ShiftKerja`, `Kehadiran`, `LokasiPresensi`.
* Fitur Presensi Masuk (Clock In) dan Presensi Pulang (Clock Out) dengan geolokasi / validasi IP.
* Rekapitulasi absensi harian dan bulanan per karyawan dan departemen.
