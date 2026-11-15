# RANCANGAN SISTEM — SISTEMHRD (HRIS ENTERPRISE)

## 1. Pendahuluan
**SistemHRD** adalah aplikasi Human Resource Information System (HRIS) berbasis web modular yang dirancang untuk mengelola seluruh siklus hidup karyawan (employee lifecycle), pencatatan kehadiran (attendance), manajemen cuti dan izin (leave management), penggajian terotomatisasi (payroll), hingga pelaporan analitik SDM secara komprehensif, aman, dan scalable.

Dokumentasi ini disusun sebagai acuan teknis dan arsitektural implementasi sistem berdasarkan analisis awal workspace pada **Phase 0**.

---

## 2. Spesifikasi Teknologi
| Komponen | Spesifikasi / Tools | Keterangan |
| :--- | :--- | :--- |
| **Framework Web** | ASP.NET Core 10.0 (MVC) | Target Framework `net10.0` |
| **Bahasa Pemrograman** | C# 13/14 | Top-level statements, Nullable Reference Types, Record types |
| **ORM / Data Access** | Entity Framework Core 10.0.15 | Code-First Migrations, LINQ, Change Tracking |
| **Database Server** | Microsoft SQL Server | LocalDB / SQL Server Express / Remote SQL Server / Azure SQL |
| **Frontend UI** | Razor Views (.cshtml), Bootstrap 5, Bootstrap Icons | Responsive Web Design, Modular Layouts |
| **Client Scripting** | Vanilla JavaScript (ES6+), jQuery, DataTables | Interaktivitas UI, AJAX request, DOM manipulation |
| **Authentication & Auth** | ASP.NET Core Cookie Authentication / Identity | Role-Based Access Control (RBAC) & Claims-Based Auth |

---

## 3. Analisis Modul Bisnis Utama

### 3.1 Modul Manajemen Akun & Otorisasi (Auth & RBAC)
- **Role Hierarchy**:
  - `SuperAdmin`: Pengaturan sistem penuh, kelola konfigurasi database, kelola user & role.
  - `HR Admin`: Pengelolaan data master, data karyawan, cuti, absensi, kalkulasi payroll, dan laporan.
  - `Manager / Supervisor`: Penyetujuan (approval) izin, cuti, lembur bawahan, evaluasi kinerja tim.
  - `Employee`: Self-service portal (ESS - Employee Self Service): melihat slip gaji, pengajuan cuti/izin, presensi kehadiran harian.
- **Fitur Keamanan**:
  - Enkripsi password menggunakan hash standar industri (BCrypt / PBKDF2 ASP.NET Core PasswordHasher).
  - Proteksi CSRF (Anti-Forgery Token pada seluruh form input).
  - Session timeout & Audit Logging pada aksi kritikal.

### 3.2 Modul Data Master (Master Data Management)
- **Struktur Organisasi**:
  - Manajemen Departemen / Divisi (Department).
  - Manajemen Jabatan / Level Posisi (Position / Job Grade).
  - Manajemen Tipe Karyawan (Permanent, Contract/PKWT, Probation, Intern).
  - Manajemen Shift Kerja & Jadwal Kerja (Shift Name, Jam Masuk, Jam Pulang, Grace Period).
  - Manajemen Hari Libur Nasional & Kalender Kerja.

### 3.3 Modul Karyawan (Employee Lifecycle Management)
- **Informasi Biodata**: Nomor Induk Karyawan (NIK), Nama Lengkap, Tempat/Tanggal Lahir, Jenis Kelamin, Kontak, Alamat Domisili, Kontak Darurat.
- **Informasi Pekerjaan**: Departemen, Jabatan, Atasan Langsung (Manager ID), Tanggal Bergabung, Tanggal Akhir Kontrak, Status Kepegawaian.
- **Informasi Finansial & Legal**: Nomor Rekening Bank, Nama Bank, NPWP, Nomor BPJS Ketenagakerjaan, Nomor BPJS Kesehatan.
- **Manajemen Dokumen**: Upload KTP, Foto Profil, CV, Ijazah, Surat Kontrak Kerja.

### 3.4 Modul Presensi & Kehadiran (Attendance & Overtime)
- Pencatatan absensi harian (Clock-in / Clock-out).
- Validasi kehadiran (Jam masuk, keterlambatan/late arrival, early departure).
- Manajemen Lembur (Overtime): Pengajuan lembur, approval atasan, kalkulasi upah lembur sesuai regulasi Depnaker.
- Rekapitulasi absensi bulanan untuk integrasi ke modul payroll.

### 3.5 Modul Manajemen Cuti & Izin (Leave & Absence)
- Master jenis cuti (Cuti Tahunan, Cuti Sakit, Cuti Bersalin/Melahirkan, Cuti Alasan Penting, Izin Terlambat, dll).
- Kuota cuti tahunan (Annual leave balance) dan perhitungan pemakaian otomatis.
- Workflow persetujuan (Approval Workflow): Karyawan -> Approval Supervisor -> HR Verification.
- Lampiran bukti izin/sakit (Medical Certificate upload).

### 3.6 Modul Penggajian (Payroll & Compensation)
- Komponen Gaji:
  - Gaji Pokok (Base Salary).
  - Tunjangan Tetap & Tidak Tetap (Tunjangan Jabatan, Transport, Makan, Kehadiran).
  - Upah Lembur (Overtime Pay).
  - Potongan (Deductions): Potongan Keterlambatan, BPJS Ketenagakerjaan (JHT, JKK, JKM, JP), BPJS Kesehatan, Potongan PPh 21, Kasbon/Pinjaman.
- Pembuatan Slip Gaji (PDF Generator) & Rekapitulasi Penggajian Bulanan.
- Ekspor daftar transfer bank (format Excel/CSV).

### 3.7 Modul Laporan & Analitik (Reporting & Analytics)
- Dashboard KPI Ringkasan: Total Karyawan Aktif, Rasio Kehadiran Hari Ini, Total Pengajuan Cuti Pending, Estimasi Biaya Payroll Bulan Berjalan.
- Ekspor Laporan: Format PDF dan Excel untuk Data Karyawan, Rekap Presensi, Laporan Cuti, dan Laporan Payroll.
- Audit Trail Log: Riwayat log perubahan data sensitif (gaji, perubahan status kepegawaian).

---

## 4. Arsitektur Software
Aplikasi mengadopsi pola **Clean Layered MVC Architecture** yang memisahkan tanggung jawab (Separation of Concerns) secara tegas:

```
┌─────────────────────────────────────────────────────────────┐
│                       PRESENTATION LAYER                    │
│   Controllers (Handling HTTP requests, Action responses)    │
│   Views (Razor .cshtml, Layouts, Partials, ViewComponents)  │
│   ViewModels (UI Data Contracts & Validations)              │
└──────────────────────────────┬──────────────────────────────┘
                               │
                               ▼
┌─────────────────────────────────────────────────────────────┐
│                         SERVICE LAYER                       │
│   Business Logic & Orchestration                            │
│   Interfaces: IEmployeeService, IAttendanceService, etc.    │
│   Implementations: EmployeeService, AttendanceService, etc. │
└──────────────────────────────┬──────────────────────────────┘
                               │
                               ▼
┌─────────────────────────────────────────────────────────────┐
│                          DATA LAYER                         │
│   Entity Framework Core DbContext (AppDbContext)            │
│   Domain Entities (Employee, Department, Attendance, etc.)  │
│   Database Migrations & Seeders                             │
└──────────────────────────────┬──────────────────────────────┘
                               │
                               ▼
┌─────────────────────────────────────────────────────────────┐
│                       INFRASTRUCTURE / DB                   │
│   Microsoft SQL Server Database Engine                      │
└─────────────────────────────────────────────────────────────┘
```

---

## 5. Hasil Pemeriksaan Lingkungan & Database
Berdasarkan verifikasi pada tahap awal (Phase 0):
1. **.NET SDK & Runtime**:
   - .NET SDK `10.0.404` terpasang dengan baik.
   - ASP.NET Core Runtime `10.0.15` aktif dan mendukung kompilasi dan hot reload.
2. **Koneksi SQL Server**:
   - Status: SQL Server lokal (LocalDB / Service MSSQL) belum berjalan atau belum terpasang di port default `1433`.
   - Di host lokal terdapat layanan PostgreSQL 15 & 18.
   - Konfigurasi `appsettings.json` telah disiapkan menggunakan format SQL Server standar (`Server=localhost;Database=SistemHRD_Db;...`).
   - Tindak Lanjut: Aplikasi siap dikoneksikan ke instance SQL Server lokal (SQLEXPRESS / LocalDB) maupun remote database SQL Server yang ditentukan pengguna.
