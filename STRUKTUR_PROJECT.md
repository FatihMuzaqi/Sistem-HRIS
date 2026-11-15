# STRUKTUR PROJECT — SISTEMHRD (.NET 10 MVC)

## 1. Ikhtisar Struktur Direktori

Berikut adalah struktur folder terorganisir untuk aplikasi **SistemHRD**:

```
.NET HRIS/
├── Common/                          # Utilitas umum, konstanta, dan ekstensi
│   ├── Constants/                   # Nilai konstan (e.g., RoleNames, AppConstants)
│   └── Helpers/                     # Helper methods (e.g., DateHelper, CurrencyHelper, FileUploadHelper)
│
├── Controllers/                     # Controller MVC (Menangani routing & HTTP Request)
│   └── HomeController.cs            # Controller default sistem
│
├── Data/                            # Konfigurasi ORM & Akses Data (EF Core)
│   ├── AppDbContext.cs              # Entity Framework Core Database Context
│   └── Migrations/                  # Berkas migrasi database EF Core (dibuat via CLI)
│
├── Models/                          # Model Data Aplikasi
│   ├── Entities/                    # Domain Entity (Representasi tabel basis data)
│   ├── Enums/                       # Tipe data enumerasi (e.g., EmployeeStatus, LeaveStatus)
│   ├── ViewModels/                  # Model untuk data binding View dan Validasi UI
│   └── ErrorViewModel.cs            # View model penanganan error sistem
│
├── Properties/                      # Konfigurasi lingkungan & peluncuran
│   └── launchSettings.json          # Profil peluncuran development (HTTP: 5252, HTTPS: 7038)
│
├── Services/                        # Layer Logika Bisnis (Business Logic Layer)
│   ├── Interfaces/                  # Kontrak antarmuka logika bisnis (e.g., IEmployeeService)
│   └── Implementations/             # Implementasi logika bisnis konkret
│
├── Views/                           # Tampilan UI Razor Pages (.cshtml)
│   ├── Home/                        # Views untuk HomeController (Index, Privacy)
│   ├── Shared/                      # Layout bersama (_Layout.cshtml, _ValidationScriptsPartial, Error)
│   ├── _ViewImports.cshtml          # Import namespace global untuk seluruh Razor views
│   └── _ViewStart.cshtml            # Penentu layout default
│
├── wwwroot/                         # File aset statis web (Public assets)
│   ├── css/                         # Berkas stylesheet CSS kustom
│   ├── js/                          # Berkas script JavaScript kustom
│   ├── lib/                         # Vendor libraries (Bootstrap 5, jQuery, dll)
│   └── uploads/                     # Direktori penyimpanan file upload (foto, dokumen karyawan)
│
├── appsettings.json                 # Konfigurasi global aplikasi & Connection Strings
├── appsettings.Development.json     # Konfigurasi khusus environment Development
├── Program.cs                       # Titik masuk aplikasi, registrasi DI & middleware pipeline
├── SistemHRD.csproj                 # Berkas definisi project C# (.NET 10)
├── SistemHRD.slnx                   # Berkas solusi Visual Studio
├── RANCANGAN_SISTEM.md              # Dokumentasi rancangan arsitektur dan modul sistem
├── STRUKTUR_PROJECT.md              # Dokumentasi struktur direktori & konvensi (berkas ini)
└── ROADMAP_PENGEMBANGAN.md          # Rencana tahapan pengerjaan (Roadmap Phase 0 - Phase 6)
```

---

## 2. Penjelasan Peran Setiap Layer

### 2.1 `Data/`
- Menyimpan `AppDbContext` yang mewarisi `Microsoft.EntityFrameworkCore.DbContext`.
- Mengatur konfigurasi relasi antar tabel (Fluent API), indeks, cascade rules, dan data seeding awal.
- Folder `Migrations/` akan otomatis menampung riwayat migrasi skema database.

### 2.2 `Models/`
- **`Entities/`**: Kelas entitas murni yang dipetakan langsung ke tabel SQL Server (misalnya: `Employee.cs`, `Department.cs`, `Attendance.cs`, `LeaveRequest.cs`, `Payroll.cs`).
- **`ViewModels/`**: Kelas DTO khusus untuk tampilan UI, memisahkan entitas database dari form input view untuk keamanan (mencegah over-posting) dan menyediakan data validation annotations (`[Required]`, `[EmailAddress]`, `[StringLength]`).
- **`Enums/`**: Menyimpan seluruh representasi status atau tipe terdefinisi seperti `Gender`, `MaritalStatus`, `AttendanceStatus`, `LeaveType`, `ApprovalStatus`.

### 2.3 `Services/`
- Memisahkan logika bisnis dari Controllers. Controller hanya bertugas menerima request, memanggil service terkait, dan mengembalikan View atau JSON.
- Pola Interface & Implementation memudahkan pengujian (Unit Testing / Mocking) dan pemeliharaan kode.

### 2.4 `Controllers/`
- Mengimplementasikan routing RESTful standar MVC.
- Menggunakan Attribute Routing dan Dependency Injection untuk mengonsumsi layer Services.

### 2.5 `Views/`
- Menggunakan engine Razor (.cshtml) berbasis komponen Bootstrap 5 untuk tampilan UI responsif dan modern.
- Mengadopsi partial views untuk modul reusable seperti pagination, alerts/notifikasi, modal konfirmasi, dan navigation bar.

---

## 3. Standar & Konvensi Penamaan (Naming Conventions)

| Tipe | Konvensi | Contoh |
| :--- | :--- | :--- |
| **Kelas & Struct** | PascalCase | `EmployeeService`, `AppDbContext` |
| **Interface** | Prefiks `I` + PascalCase | `IEmployeeService`, `IAttendanceService` |
| **Method & Property** | PascalCase | `GetEmployeeByIdAsync()`, `CreatedAt` |
| **Private Field** | Prefiks `_` + camelCase | `_context`, `_employeeService` |
| **Parameter & Variabel Lokal** | camelCase | `employeeId`, `departmentList` |
| **Konstanta** | PascalCase / UPPER_CASE | `DefaultPageSize`, `RoleSuperAdmin` |
| **Async Methods** | Akhiran `Async` | `GetActiveEmployeesAsync()` |
| **Controller** | Akhiran `Controller` | `EmployeeController`, `AttendanceController` |
| **View Model** | Akhiran `ViewModel` | `EmployeeCreateViewModel`, `AttendanceFilterViewModel` |

---

## 4. Perintah Operasional Project

- **Membangun Project (Build):**
  ```powershell
  dotnet build
  ```

- **Menjalankan Project (Run):**
  ```powershell
  dotnet run --launch-profile http
  ```
  *Aplikasi akan aktif di:* `http://localhost:5252` (atau `https://localhost:7038` jika menggunakan HTTPS).

- **Menjalankan Mode Hot Reload (Watch Run):**
  ```powershell
  dotnet watch run
  ```

- **Menambahkan Migrasi Database (EF Core):**
  ```powershell
  dotnet ef migrations add <NamaMigrasi>
  ```

- **Menerapkan Migrasi ke Database:**
  ```powershell
  dotnet ef database update
  ```
