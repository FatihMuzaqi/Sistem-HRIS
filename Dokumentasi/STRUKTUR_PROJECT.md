# STRUKTUR PROJECT — SISTEMHRD (.NET 10 MVC)

## 1. Tata Letak Direktori

Sesuai aturan Master Prompt SistemHRD, struktur direktori diorganisir sebagai berikut:

```
.NET HRIS/
├── Controllers/                     # Controller MVC (Routing & penanganan aksi pengguna)
│   └── HomeController.cs            # Controller beranda default
│
├── Models/                          # Model domain aplikasi
│   ├── Entitas/                     # Entitas tabel database (KonteksAplikasi)
│   │   ├── ITenantEntity.cs         # Kontrak entitas multi-tenant
│   │   ├── ISoftDelete.cs           # Kontrak entitas soft delete
│   │   ├── IAuditEntity.cs          # Kontrak entitas jejak audit
│   │   ├── EntitasDasar.cs          # Kelas dasar entitas non-tenant
│   │   ├── EntitasTenantDasar.cs    # Kelas dasar entitas tenant
│   │   ├── Perusahaan.cs            # Entitas Tenant Perusahaan
│   │   ├── Pengguna.cs              # Entitas User ASP.NET Core Identity
│   │   └── Peran.cs                 # Entitas Role ASP.NET Core Identity
│   └── Enums/                       # Tipe data enumerasi status, tipe, dll.
│
├── Data/                            # Konfigurasi akses data EF Core
│   ├── KonteksAplikasi.cs           # DbContext utama dengan Global Query Filter & Audit
│   └── InisialisasiData.cs          # Data Seeder peran, perusahaan & admin default
│
├── ViewModels/                      # Model khusus untuk binding tampilan View & form validasi
│
├── Views/                           # Tampilan UI Razor (.cshtml)
│   ├── Home/                        # View halaman awal / beranda
│   ├── Shared/                      # Master Layout, Partial Views, Navigasi
│   ├── _ViewImports.cshtml
│   └── _ViewStart.cshtml
│
├── Services/                        # Business Logic Layer
│   ├── Interfaces/                  # Kontrak interface service
│   │   └── ILayananTenant.cs        # Kontrak deteksi dan isolasi tenant aktif
│   └── Implementasi/                # Implementasi konkret logika bisnis
│       └── LayananTenant.cs         # Implementasi pembaca tenant dari HttpContext & Claim
│
├── Middleware/                      # Middleware pipeline ASP.NET Core
│   └── TenantMiddleware.cs          # Middleware validasi tenant pada setiap request
│
├── Migrations/                      # Riwayat migrasi database EF Core
│   ├── 20261115082106_InisialisasiAwal.cs
│   └── KonteksAplikasiModelSnapshot.cs
│
├── Dokumentasi/                     # Dokumentasi komprehensif sistem
│   ├── RANCANGAN_SISTEM.md
│   ├── STRUKTUR_DATABASE.md
│   ├── STRUKTUR_PROJECT.md
│   ├── PANDUAN_INSTALASI.md
│   ├── DOKUMENTASI_API_DAN_ROUTE.md
│   └── RIWAYAT_PENGEMBANGAN.md
│
├── wwwroot/                         # Aset statis web publik
│   ├── css/                         # File styling CSS kustom (Warna Emerald #059669)
│   ├── js/                          # File Javascript interaktif
│   ├── lib/                         # Bootstrap 5, jQuery
│   └── uploads/                     # Direktori upload berkas karyawan
│
├── Common/                          # Konstanta & helper bersama
│   └── Constants/
│       └── NamaPeran.cs             # Daftar peran resmi: SuperAdmin, HRD, Manager, Finance, Karyawan
│
├── appsettings.json                 # Konfigurasi aplikasi & database
├── appsettings.Development.json     # Konfigurasi mode development
├── Program.cs                       # Titik masuk aplikasi & konfigurasi container DI
├── SistemHRD.csproj                 # Definisi project .NET 10 LTS
└── SistemHRD.slnx                   # File solusi Visual Studio
```

---

## 2. Aturan Penamaan (Naming Conventions)
1. **Bahasa**: Seluruh kode yang dibuat sendiri wajib menggunakan **Bahasa Indonesia**.
   * Controller: `KaryawanController`, `DepartemenController`, `PenggajianController`.
   * Entitas: `Karyawan`, `Departemen`, `Kehadiran`, `PengajuanCuti`.
   * DbContext: `KonteksAplikasi`.
2. **Karakter & Casing**:
   * Nama Class / Struct / Interface: PascalCase (`LayananTenant`, `KonteksAplikasi`).
   * Nama Interface: Didahului huruf `I` (`ILayananTenant`, `ITenantEntity`).
   * Nama Method & Property: PascalCase (`DapatkanPerusahaanAktifAsync`, `PerusahaanIdSaatIni`).
   * Variabel Lokal & Parameter: camelCase (`perusahaanId`, `waktuSekarang`).
   * Private Field: Prefiks `_` + camelCase (`_layananTenant`, `_httpContextAccessor`).
