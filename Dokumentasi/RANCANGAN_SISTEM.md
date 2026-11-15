# RANCANGAN SISTEM — SISTEMHRD (SMART HRIS)

## 1. Pendahuluan
**SistemHRD (Smart HRIS)** adalah sistem informasi manajemen sumber daya manusia enterprise berbasis web yang dirancang dengan arsitektur **Modular Monolith** dan **Multi-Tenant**. Sistem ini ditujukan untuk mengelola siklus kepegawaian secara terpusat, fleksibel, aman, serta siap diskalakan menjadi solusi Software-as-a-Service (SaaS).

---

## 2. Arsitektur Software & Pola Desain

### 2.1 Pola Modular Monolith MVC
Aplikasi dibangun menggunakan ASP.NET Core MVC (.NET 10 LTS) tanpa memisahkan backend API dan frontend SPA terpisah (menghindari kompleksitas React/Node.js). Seluruh logika rendering ditangani secara efisien oleh Razor Views (.cshtml) di server, didukung oleh Bootstrap 5, Chart.js, dan Lucide Icons.

```
┌─────────────────────────────────────────────────────────────┐
│                       PRESENTATION LAYER                    │
│   Controllers (Handling HTTP request & Response)            │
│   ViewModels (Data Binding, UI Validation, Form Contracts)  │
│   Views (Razor .cshtml, Layouts, Partials, UI Emerald)      │
└──────────────────────────────┬──────────────────────────────┘
                               │
                               ▼
┌─────────────────────────────────────────────────────────────┐
│                         SERVICE LAYER                       │
│   Business Logic Orchestration & Domain Rules               │
│   Interfaces: ILayananTenant, ILayananKaryawan, dll.        │
│   Implementasi: LayananTenant, LayananKaryawan, dll.        │
└──────────────────────────────┬──────────────────────────────┘
                               │
                               ▼
┌─────────────────────────────────────────────────────────────┐
│                      DATA & CONTEXT LAYER                   │
│   KonteksAplikasi (Inherits IdentityDbContext)              │
│   Domain Entities: Perusahaan, Pengguna, Karyawan, dll.     │
│   Global Query Filters (Multi-Tenant & Soft Delete)         │
└──────────────────────────────┬──────────────────────────────┘
                               │
                               ▼
┌─────────────────────────────────────────────────────────────┐
│                    DATABASE INFRASTRUCTURE                  │
│   MySQL / MariaDB (phpMyAdmin / XAMPP) & SQL Server Support │
└─────────────────────────────────────────────────────────────┘
```

---

## 3. Desain Arsitektur Multi-Tenant

### 3.1 Isolasi Data Berbasis Perusahaan
* Setiap perusahaan memiliki entitas terpisah (`Perusahaan`) dengan identifier unik `Id` (GUID) dan `KodePerusahaan`.
* Seluruh entitas data bisnis mengimplementasikan `ITenantEntity` dan mewarisi `EntitasTenantDasar`.
* **Global Query Filter**: Diatur secara otomatis pada level `KonteksAplikasi`:
  ```csharp
  // Query hanya mengambil data milik perusahaan yang sedang aktif
  e => _layananTenant.IsSuperAdmin || e.PerusahaanId == _layananTenant.PerusahaanIdSaatIni
  ```
* **Pencegahan Kebocoran Lintas Tenant**:
  * Nilai `PerusahaanId` divalidasi dan diinjeksi di sisi server melalui `TenantMiddleware` dan `KonteksAplikasi.SaveChangesAsync`.
  * Input dari browser tidak pernah dipercaya secara langsung.
  * `SuperAdmin` memiliki hak akses istimewa lintas perusahaan untuk kebutuhan pemeliharaan platform.

---

## 4. Matriks Peran Pengguna (Role-Based Access Control)

| Peran | Deskripsi & Batas Akses |
| :--- | :--- |
| **SuperAdmin** | Akses penuh tingkat platform: mengelola seluruh perusahaan (tenant), memantau log sistem, mengonfigurasi pengaturan global. |
| **HRD** | Mengelola seluruh operasional SDM pada perusahaannya: data karyawan, departemen, presensi, cuti, penggajian, dan rekrutmen. |
| **Manager** | Mengelola dan menyetujui (approval) cuti, izin, lembur bawahan, serta evaluasi kinerja tim di departemennya. |
| **Finance** | Mengelola komponen penggajian, verifikasi klaim biaya/reimbursement, serta ekspor rekapitulasi transfer gaji. |
| **Karyawan** | Portal mandiri (Employee Self Service): melakukan presensi, mengajukan cuti/izin/lembur, dan mengunduh slip gaji. |

---

## 5. Cakupan 14 Modul Aplikasi
1. **Autentikasi & Hak Akses**: Login, logout, registrasi terproteksi, proteksi brute-force lockout, manajemen peran.
2. **Dashboard**: Ringkasan statistik operasional, visualisasi Chart.js, kartu KPI cepat.
3. **Manajemen Perusahaan**: Pengaturan identitas perusahaan, legalitas, kuota karyawan, logo, multi-cabang.
4. **Manajemen Karyawan**: Biodata lengkap, NIK otomatis, riwayat kontrak, status kerja, dokumen berkas.
5. **Departemen & Jabatan**: Struktur hierarki organisasi, level jabatan, grade gaji.
6. **Kehadiran**: Pencatatan clock in/out, kalkulasi keterlambatan, manajemen jadwal shift, rekap bulanan.
7. **Cuti & Perizinan**: Kuota tahunan, multi-tier approval workflow, upload surat dokter/bukti izin.
8. **Penggajian**: Formula gaji pokok, tunjangan, potongan BPJS, PPh 21, generate slip gaji PDF.
9. **Rekrutmen**: Lowongan kerja, pelamar, tahapan seleksi/interview, onboarding karyawan baru.
10. **Penilaian Kinerja**: KPI karyawan, review berkala, scoring performa, rekomendasi promosi.
11. **Pelatihan & Pengembangan**: Katalog training, penugasan karyawan, riwayat sertifikasi.
12. **Manajemen Aset**: Inventaris barang kantor, peminjaman aset laptop/kendaraan, pengembalian.
13. **Laporan & Analitik**: Rekapitulasi absensi, payroll summary, turnover karyawan, ekspor Excel & PDF.
14. **Audit Log**: Pencatatan otomatis riwayat aksi sensitif (create/update/delete) demi kepatuhan & transparansi.
