# ROADMAP PENGEMBANGAN SISTEMHRD (.NET 10 MVC)

Dokumen ini memuat roadmap pengembangan bertahap sistem HRIS enterprise **SistemHRD**. Pendekatan bertahap memastikan setiap modul teruji, stabil, dan terintegrasi sebelum melanjutkan ke modul berikutnya.

---

## 📌 Status Fase Pengembangan

| Fase | Nama Fase & Fokus | Estimasi Cakupan | Status |
| :---: | :--- | :--- | :---: |
| **Fase 0** | **Analisis Workspace & Fondasi Awal** | Setup project .NET 10 MVC, EF Core, struktur folder, dokumentasi | ✅ Selesai |
| **Fase 1** | **Arsitektur Database & Entity Modeling** | Desain tabel, entitas EF Core, relasi, Fluent API, migrations, seeder | ⏳ Siap Dikerjakan |
| **Fase 2** | **Autentikasi, Otorisasi (RBAC) & Layout Admin** | Login, logout, manajemen role, UI Admin Template & dashboard shell | ⏳ Menunggu |
| **Fase 3** | **Modul Data Master & Manajemen Karyawan** | Departemen, jabatan, master karyawan, profil, upload dokumen | ⏳ Menunggu |
| **Fase 4** | **Modul Presensi (Attendance) & Manajemen Cuti** | Clock in/out, rekap absensi, pengajuan cuti, approval workflow | ⏳ Menunggu |
| **Fase 5** | **Modul Payroll (Penggajian) & Kompensasi** | Struktur gaji, kalkulasi tunjangan/potongan, BPJS, PPh 21, slip gaji | ⏳ Menunggu |
| **Fase 6** | **Pelaporan, Analitik & Ekspor Data (Excel/PDF)** | Rekapitulasi laporan HR, audit log, visualisasi grafik, ekspor | ⏳ Menunggu |
| **Fase 7** | **Testing, Hardening Keamanan & Finalisasi** | Unit testing, validasi keamanan, optimasi query, panduan deployment | ⏳ Menunggu |

---

## 📋 Rincian Tahapan & Deliverables

### ✅ FASE 0 — Analisis Workspace & Fondasi Awal (SELESAI)
- [x] Analisis kesiapan workspace dan lingkungan development.
- [x] Pemeriksaan versi .NET SDK (`.NET 10.0.404`) dan runtime ASP.NET Core (`10.0.15`).
- [x] Inisialisasi project ASP.NET Core 10 MVC (`SistemHRD.csproj` & `SistemHRD.slnx`).
- [x] Instalasi paket NuGet Entity Framework Core SQL Server 10.0.15 & Tools.
- [x] Pemeriksaan status koneksi SQL Server dan dokumentasi konfigurasi host.
- [x] Persiapan struktur folder modular (Clean Layered MVC).
- [x] Penyusunan dokumen arsitektur: `RANCANGAN_SISTEM.md`.
- [x] Penyusunan dokumen struktur project: `STRUKTUR_PROJECT.md`.
- [x] Penyusunan dokumen roadmap: `ROADMAP_PENGEMBANGAN.md`.
- [x] Verifikasi build (`dotnet build`) dan verifikasi jalannya server HTTP (`dotnet run` / `dotnet watch run`).

---

### ⏳ FASE 1 — Arsitektur Database & Entity Modeling
- [ ] **Desain Entitas Inti**:
  - `User`, `Role`, `UserRole` (Autentikasi & RBAC).
  - `Department`, `Position` (Master Struktur Organisasi).
  - `Employee`, `EmployeeDocument` (Data Karyawan & Berkas).
  - `Shift`, `Attendance`, `Overtime` (Presensi & Lembur).
  - `LeaveType`, `LeaveRequest`, `LeaveBalance` (Cuti & Izin).
  - `SalaryComponent`, `Payroll`, `PayrollDetail` (Penggajian).
  - `AuditLog` (Pencatatan riwayat audit).
- [ ] **Konfigurasi Fluent API di `AppDbContext`**:
  - Konfigurasi primary key, foreign key, cascade delete, dan index.
- [ ] **Initial Database Migration & Seed Data**:
  - Generate initial migration EF Core.
  - Seeder akun SuperAdmin default dan data master awal (departemen & jabatan contoh).

---

### ⏳ FASE 2 — Autentikasi, Otorisasi (RBAC) & Layout Admin
- [ ] Implementasi sistem autentikasi cookie berbasis claims atau ASP.NET Core Identity.
- [ ] Halaman Login & Logout dengan proteksi CSRF dan validasi kredensial.
- [ ] Middleware otorisasi berbasis Role (`SuperAdmin`, `HRAdmin`, `Manager`, `Employee`).
- [ ] Integrasi template UI Admin responsif (Sidebar, Navbar, User Profile dropdown, Breadcrumbs, Flash messages).
- [ ] Dashboard shell beranda dengan navigasi dinamis sesuai hak akses role.

---

### ⏳ FASE 3 — Modul Data Master & Manajemen Karyawan
- [ ] **CRUD Master Departemen & Jabatan**:
  - Validasi kode unik, nama, status aktif.
- [ ] **CRUD Data Karyawan**:
  - Form multi-tab/wizard: Data Diri, Data Kepegawaian, Rekening & Pajak, Berkas.
  - Generator otomatis NIK Karyawan sesuai format tahun/departemen.
  - Upload foto profil dan berkas dokumen pendukung (KTP, Kontrak Kerja).
- [ ] Filter, pencarian, dan pagination data karyawan.

---

### ⏳ FASE 4 — Modul Presensi & Manajemen Cuti
- [ ] **Presensi / Kehadiran**:
  - Pencatatan Clock In dan Clock Out harian.
  - Perhitungan otomatis keterlambatan (late check-in) dan jam kerja efektif.
  - Form pengajuan dan approval lembur.
- [ ] **Cuti & Izin**:
  - Pengaturan kuota cuti tahunan karyawan.
  - Form pengajuan cuti beserta upload surat keterangan (surat dokter untuk izin sakit).
  - Multi-tier approval (Karyawan -> Manager/Atasan -> HRD).
  - Kalender cuti bersama tim/perusahaan.

---

### ⏳ FASE 5 — Modul Payroll (Penggajian) & Kompensasi
- [ ] Formula kalkulasi gaji pokok dan tunjangan tetap/variabel.
- [ ] Kalkulasi potongan otomatis: BPJS Ketenagakerjaan, BPJS Kesehatan, keterlambatan.
- [ ] Kalkulasi upah lembur berdasarkan rekap presensi.
- [ ] Siklus penggajian bulanan (Batch Payroll Processing).
- [ ] Pembuatan Slip Gaji individual karyawan format cetak / PDF.

---

### ⏳ FASE 6 — Pelaporan, Analitik & Ekspor Data
- [ ] Dashboard analitik HR (Grafik tren kehadiran, rasio cuti, headcount karyawan).
- [ ] Fitur ekspor laporan presensi karyawan ke format Excel (.xlsx) dan PDF.
- [ ] Fitur ekspor rekapitulasi payroll bulanan dan daftar transfer bank.
- [ ] Audit Trail Viewer untuk memantau perubahan data penting oleh admin.

---

### ⏳ FASE 7 — Testing, Hardening & Finalisasi
- [ ] Unit testing logika kalkulasi payroll dan kuota cuti.
- [ ] Security hardening (Header keamanan HTTP, Anti-XSS, SQL Injection prevention via EF Core Parameterization).
- [ ] Optimasi query database (Indexing, AsNoTracking pada query read-only).
- [ ] Panduan deployment dan konfigurasi server produksi.
