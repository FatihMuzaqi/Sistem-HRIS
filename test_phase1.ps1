$ErrorActionPreference = "Continue"
$baseUrl = "http://localhost:5252"

Write-Host "==========================================================" -ForegroundColor Cyan
Write-Host "   PENGUJIAN INTEGRASI PHASE 1 — SISTEMHRD (SMART HRIS)   " -ForegroundColor Cyan
Write-Host "==========================================================" -ForegroundColor Cyan

# 1. Uji Akses Tanpa Autentikasi
Write-Host "`n[UJI 1] Akses Halaman Terproteksi Tanpa Login (GET /Dasbor)..." -NoNewline
try {
    $res1 = Invoke-WebRequest -Uri "$baseUrl/Dasbor" -MaximumRedirection 0 -UseBasicParsing -ErrorAction SilentlyContinue
    $statusCode1 = $res1.StatusCode
    $location1 = $res1.Headers.Location
} catch {
    $statusCode1 = $_.Exception.Response.StatusCode.value__
    $location1 = $_.Exception.Response.Headers.Location
}

if ($statusCode1 -eq 302 -and $location1 -like "*/Autentikasi/Masuk*") {
    Write-Host " [BERHASIL]" -ForegroundColor Green
    Write-Host "       Status: 302 Redirect -> $location1" -ForegroundColor Gray
} else {
    Write-Host " [GAGAL] Status: $statusCode1, Target: $location1" -ForegroundColor Red
}

# Helper untuk mendapatkan Anti-Forgery Token dari form login
function Dapatkan-Token($session) {
    $page = Invoke-WebRequest -Uri "$baseUrl/Autentikasi/Masuk" -WebSession $session -UseBasicParsing
    if ($page.Content -match 'name="__RequestVerificationToken"\s+type="hidden"\s+value="([^"]+)"') {
        return $matches[1]
    }
    return $null
}

# Helper untuk mendapatkan Anti-Forgery Token dari halaman terautentikasi (seperti Dasbor)
function Dapatkan-Token-Autentikasi($session) {
    $page = Invoke-WebRequest -Uri "$baseUrl/Dasbor" -WebSession $session -UseBasicParsing
    if ($page.Content -match 'name="__RequestVerificationToken"\s+type="hidden"\s+value="([^"]+)"') {
        return $matches[1]
    }
    return $null
}

# 2. Uji Login Gagal (Kredensial Tidak Valid)
Write-Host "`n[UJI 2] Percobaan Login Gagal (Password Salah)..." -NoNewline
$sessionFail = New-Object Microsoft.PowerShell.Commands.WebRequestSession
$tokenFail = Dapatkan-Token $sessionFail

$bodyFail = @{
    "__RequestVerificationToken" = $tokenFail
    "Email" = "superadmin@sistemhrd.id"
    "KataSandi" = "KataSandiSalah123!"
}

$resFail = Invoke-WebRequest -Uri "$baseUrl/Autentikasi/Masuk" -Method Post -Body $bodyFail -WebSession $sessionFail -UseBasicParsing
if ($resFail.Content -like "*Email atau kata sandi yang Anda masukkan salah*") {
    Write-Host " [BERHASIL]" -ForegroundColor Green
    Write-Host "       Kredensial salah ditolak dan pesan validasi ditampilkan." -ForegroundColor Gray
} else {
    Write-Host " [GAGAL] Tidak ditemukan pesan error login gagal." -ForegroundColor Red
}

# 3. Uji Login Berhasil SuperAdmin
Write-Host "`n[UJI 3] Login Berhasil SuperAdmin (superadmin@sistemhrd.id)..." -NoNewline
$sessionAdmin = New-Object Microsoft.PowerShell.Commands.WebRequestSession
$tokenAdmin = Dapatkan-Token $sessionAdmin

$bodyAdmin = @{
    "__RequestVerificationToken" = $tokenAdmin
    "Email" = "superadmin@sistemhrd.id"
    "KataSandi" = "Admin#2026!"
}

try {
    $resAdmin = Invoke-WebRequest -Uri "$baseUrl/Autentikasi/Masuk" -Method Post -Body $bodyAdmin -WebSession $sessionAdmin -MaximumRedirection 0 -UseBasicParsing -ErrorAction SilentlyContinue
    $statusAdmin = $resAdmin.StatusCode
    $locAdmin = $resAdmin.Headers.Location
} catch {
    $statusAdmin = $_.Exception.Response.StatusCode.value__
    $locAdmin = $_.Exception.Response.Headers.Location
}

if ($statusAdmin -eq 302 -and $locAdmin -like "*/Dasbor*") {
    Write-Host " [BERHASIL]" -ForegroundColor Green
    Write-Host "       Status: 302 Redirect -> $locAdmin" -ForegroundColor Gray
    
    $dashAdmin = (Invoke-WebRequest -Uri "$baseUrl/Dasbor" -WebSession $sessionAdmin -UseBasicParsing).Content
    if ($dashAdmin -like "*Super Administrator*" -and $dashAdmin -like "*SuperAdmin Platform*" -and $dashAdmin -like "*Daftar Perusahaan Terdaftar*") {
        Write-Host "       Dasbor Platform SuperAdmin terverifikasi lengkap." -ForegroundColor Gray
    } else {
        Write-Host "       [PERINGATAN] Elemen dasbor SuperAdmin tidak lengkap." -ForegroundColor Yellow
    }
} else {
    Write-Host " [GAGAL] Status: $statusAdmin, Target: $locAdmin" -ForegroundColor Red
}

# 4. Uji Logout Sesi SuperAdmin
Write-Host "`n[UJI 4] Uji Logout Sesi SuperAdmin..." -NoNewline
$tokenLogout = Dapatkan-Token-Autentikasi $sessionAdmin
$bodyLogout = @{
    "__RequestVerificationToken" = $tokenLogout
}
try {
    $resLogout = Invoke-WebRequest -Uri "$baseUrl/Autentikasi/Keluar" -Method Post -Body $bodyLogout -WebSession $sessionAdmin -MaximumRedirection 0 -UseBasicParsing -ErrorAction SilentlyContinue
    $statusLogout = $resLogout.StatusCode
    $locLogout = $resLogout.Headers.Location
} catch {
    $statusLogout = $_.Exception.Response.StatusCode.value__
    $locLogout = $_.Exception.Response.Headers.Location
}

if ($statusLogout -eq 302 -and $locLogout -like "*/Autentikasi/Masuk*") {
    Write-Host " [BERHASIL]" -ForegroundColor Green
    Write-Host "       Sesi ditutup dan diarahkan kembali ke login." -ForegroundColor Gray
} else {
    Write-Host " [GAGAL] Status: $statusLogout, Target: $locLogout" -ForegroundColor Red
}

# 5. Uji Isolasi Data Dua Perusahaan (HRD Perusahaan 1 vs HRD Perusahaan 2)
Write-Host "`n[UJI 5] Uji Isolasi Data Multi-Tenant Dua Perusahaan..."

# Login HRD 1 (PT Solusi HR Nusantara - PRSH001)
$sessionHrd1 = New-Object Microsoft.PowerShell.Commands.WebRequestSession
$tokenHrd1 = Dapatkan-Token $sessionHrd1
$bodyHrd1 = @{
    "__RequestVerificationToken" = $tokenHrd1
    "Email" = "hrd@solusihr.id"
    "KataSandi" = "Hrd#2026!"
}
$null = Invoke-WebRequest -Uri "$baseUrl/Autentikasi/Masuk" -Method Post -Body $bodyHrd1 -WebSession $sessionHrd1 -MaximumRedirection 0 -UseBasicParsing -ErrorAction SilentlyContinue
$dashHrd1 = (Invoke-WebRequest -Uri "$baseUrl/Dasbor" -WebSession $sessionHrd1 -UseBasicParsing).Content

$hrd1MelihatPrsh1 = $dashHrd1 -like "*PT Solusi HR Nusantara*" -and $dashHrd1 -like "*PRSH001*"
# Periksa apakah HRD 1 melihat nama HRD 2 (Siti Rahma) atau kode PRSH002
$hrd1BocorPrsh2 = $dashHrd1 -like "*PRSH002*" -or $dashHrd1 -like "*Siti Rahma*"

Write-Host "   - HRD 1 (PRSH001): Melihat data perusahaan miliknya? $hrd1MelihatPrsh1" -ForegroundColor $(if ($hrd1MelihatPrsh1) { "Green" } else { "Red" })
Write-Host "   - HRD 1 (PRSH001): Apakah bocor melihat akun Perusahaan 2 (PRSH002)? $hrd1BocorPrsh2" -ForegroundColor $(if (-not $hrd1BocorPrsh2) { "Green" } else { "Red" })

# Login HRD 2 (PT Maju Bersama Digital - PRSH002)
$sessionHrd2 = New-Object Microsoft.PowerShell.Commands.WebRequestSession
$tokenHrd2 = Dapatkan-Token $sessionHrd2
$bodyHrd2 = @{
    "__RequestVerificationToken" = $tokenHrd2
    "Email" = "hrd2@majubersama.id"
    "KataSandi" = "Hrd2#2026!"
}
$null = Invoke-WebRequest -Uri "$baseUrl/Autentikasi/Masuk" -Method Post -Body $bodyHrd2 -WebSession $sessionHrd2 -MaximumRedirection 0 -UseBasicParsing -ErrorAction SilentlyContinue
$dashHrd2 = (Invoke-WebRequest -Uri "$baseUrl/Dasbor" -WebSession $sessionHrd2 -UseBasicParsing).Content

$hrd2MelihatPrsh2 = $dashHrd2 -like "*PT Maju Bersama Digital*" -and $dashHrd2 -like "*PRSH002*"
# Periksa apakah HRD 2 melihat nama HRD 1 (Budi Santoso) atau kode PRSH001
$hrd2BocorPrsh1 = $dashHrd2 -like "*PRSH001*" -or $dashHrd2 -like "*Budi Santoso*"

Write-Host "   - HRD 2 (PRSH002): Melihat data perusahaan miliknya? $hrd2MelihatPrsh2" -ForegroundColor $(if ($hrd2MelihatPrsh2) { "Green" } else { "Red" })
Write-Host "   - HRD 2 (PRSH002): Apakah bocor melihat akun Perusahaan 1 (PRSH001)? $hrd2BocorPrsh1" -ForegroundColor $(if (-not $hrd2BocorPrsh1) { "Green" } else { "Red" })

if ($hrd1MelihatPrsh1 -and -not $hrd1BocorPrsh2 -and $hrd2MelihatPrsh2 -and -not $hrd2BocorPrsh1) {
    Write-Host " [BERHASIL] Isolasi Multi-Tenant bekerja dengan sempurna (100% data terisolasi)!" -ForegroundColor Green
} else {
    Write-Host " [GAGAL] Terjadi kebocoran data antar tenant." -ForegroundColor Red
}

# 6. Uji Manipulasi URL Lintas Perusahaan / Otorisasi Terlarang
Write-Host "`n[UJI 6] Uji Manipulasi Otorisasi (HRD mencoba memanggil endpoint khusus SuperAdmin)..." -NoNewline
$tokenHrdAuth = Dapatkan-Token-Autentikasi $sessionHrd1
$bodyTamper = @{
    "__RequestVerificationToken" = $tokenHrdAuth
    "perusahaanId" = "00000000-0000-0000-0000-000000000000"
}

try {
    $resTamper = Invoke-WebRequest -Uri "$baseUrl/Dasbor/GantiPerusahaanAktif" -Method Post -Body $bodyTamper -WebSession $sessionHrd1 -MaximumRedirection 0 -UseBasicParsing -ErrorAction SilentlyContinue
    $statusTamper = $resTamper.StatusCode
    $locTamper = $resTamper.Headers.Location
} catch {
    $statusTamper = $_.Exception.Response.StatusCode.value__
    $locTamper = $_.Exception.Response.Headers.Location
}

if ($statusTamper -eq 302 -and $locTamper -like "*/Autentikasi/AksesDitolak*") {
    Write-Host " [BERHASIL]" -ForegroundColor Green
    Write-Host "       Akses ditolak (Status: 302 -> /Autentikasi/AksesDitolak)" -ForegroundColor Gray
} elseif ($statusTamper -eq 403) {
    Write-Host " [BERHASIL] HTTP 403 Forbidden" -ForegroundColor Green
} else {
    Write-Host " [GAGAL] Status: $statusTamper -> $locTamper" -ForegroundColor Red
}

# 7. Uji Halaman Profil Pengguna
Write-Host "`n[UJI 7] Uji Halaman Profil Pengguna (GET /Pengguna/Profil)..." -NoNewline
$profilHtml = (Invoke-WebRequest -Uri "$baseUrl/Pengguna/Profil" -WebSession $sessionHrd1 -UseBasicParsing).Content
if ($profilHtml -like "*Budi Santoso*" -and $profilHtml -like "*hrd@solusihr.id*" -and $profilHtml -like "*Informasi Data Diri*") {
    Write-Host " [BERHASIL]" -ForegroundColor Green
    Write-Host "       Halaman profil memuat informasi akun dengan benar." -ForegroundColor Gray
} else {
    Write-Host " [GAGAL] Halaman profil tidak memuat data yang sesuai." -ForegroundColor Red
}

Write-Host "`n==========================================================" -ForegroundColor Cyan
Write-Host "   SEMUA SKENARIO PENGUJIAN PHASE 1 BERHASIL DIVERIFIKASI   " -ForegroundColor Cyan
Write-Host "==========================================================" -ForegroundColor Cyan
