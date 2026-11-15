# PANDUAN INSTALASI & PENGOPERASIAN — SISTEMHRD

## 1. Prasyarat Sistem (Prerequisites)
Sebelum menjalankan aplikasi, pastikan komputer telah terpasang:
1. **.NET SDK 10.0** (Terverifikasi versi `10.0.404`).
2. **Server Basis Data**:
   * **MySQL / MariaDB** (melalui **XAMPP** yang sudah aktif pada port `3306`), ATAU
   * **Microsoft SQL Server** (LocalDB / Express / Remote).
3. **Browser Modern**: Google Chrome, Mozilla Firefox, atau Microsoft Edge.

---

## 2. Konfigurasi Basis Data
Buka berkas `appsettings.json` dan pastikan konfigurasi connection string telah sesuai:

```json
{
  "DatabaseProvider": "MySQL",
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Port=3306;Database=sistemhrd_db;Uid=root;Pwd=;CharSet=utf8mb4;",
    "SqlServerConnection": "Server=localhost;Database=SistemHRD_Db;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
  }
}
```

*Jika menggunakan XAMPP, pastikan modul **Apache** dan **MySQL** berstatus Running di XAMPP Control Panel.*

---

## 3. Langkah Menjalankan Aplikasi

### 3.1 Restore Paket & Build Project
Buka terminal PowerShell pada direktori project:
```powershell
dotnet restore
dotnet build
```

### 3.2 Menjalankan Aplikasi
Jalankan aplikasi dengan profil HTTP:
```powershell
dotnet run --launch-profile http
```
Aplikasi akan aktif dan dapat diakses melalui browser pada alamat:
👉 **`http://localhost:5252`** (atau `https://localhost:7038` jika menggunakan HTTPS).

### 3.3 Menjalankan Mode Hot Reload (Watch Run)
Untuk kemudahan pengembangan dengan auto-reload saat kode diperbarui:
```powershell
dotnet watch run
```

---

## 4. Akun Bawaan Hasil Seeder (Default Accounts)

Saat aplikasi pertama kali dijalankan, sistem secara otomatis mengeksekusi seeder untuk membuat data awal:

### 4.1 Akun Super Administrator (Tingkat Platform)
* **Email**: `superadmin@sistemhrd.id`
* **Password**: `Admin#2026!`
* **Peran**: `SuperAdmin`
* **Cakupan Akses**: Seluruh perusahaan tenant.

### 4.2 Akun HRD Manager (Tingkat Perusahaan)
* **Email**: `hrd@solusihr.id`
* **Password**: `Hrd#2026!`
* **Peran**: `HRD`
* **Perusahaan**: `PT Solusi HR Nusantara` (`PRSH001`).
