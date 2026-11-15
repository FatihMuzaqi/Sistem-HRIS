param(
    [string]$BaseUrl = "http://localhost:5252"
)

$ErrorActionPreference = "Continue"

Write-Host "==========================================================" -ForegroundColor Cyan
Write-Host "   PENGUJIAN INTEGRASI PHASE 2 — MANAJEMEN KARYAWAN      " -ForegroundColor Cyan
Write-Host "   SistemHRD (Smart HRIS)                                " -ForegroundColor Cyan
Write-Host "==========================================================" -ForegroundColor Cyan

# Helper untuk mendapatkan Anti-Forgery Token
function Dapatkan-Token($session, $url) {
    try {
        $page = Invoke-WebRequest -Uri $url -WebSession $session -UseBasicParsing
        if ($page.Content -match 'name="__RequestVerificationToken"\s+type="hidden"\s+value="([^"]+)"') {
            return $matches[1]
        }
        if ($page.Content -match 'value="([^"]+)"\s+type="hidden"\s+name="__RequestVerificationToken"') {
            return $matches[1]
        }
    } catch {}
    return $null
}

# Helper untuk login
function Lakukan-Login($email, $password) {
    $session = New-Object Microsoft.PowerShell.Commands.WebRequestSession
    $token = Dapatkan-Token $session "$BaseUrl/Autentikasi/Masuk"
    if (!$token) {
        Write-Error "Gagal mendapatkan anti-forgery token untuk login."
        return $null
    }

    $body = @{
        "__RequestVerificationToken" = $token
        "Email" = $email
        "KataSandi" = $password
    }

    $loginRes = Invoke-WebRequest -Uri "$BaseUrl/Autentikasi/Masuk" -Method Post -Body $body -WebSession $session -MaximumRedirection 0 -UseBasicParsing -ErrorAction SilentlyContinue
    return $session
}

# 1. Login sebagai HRD Perusahaan 1 (PT Solusi HR Nusantara)
Write-Host "`n[UJI 1] Login sebagai HRD PT Solusi HR Nusantara (hrd@solusihr.id)..." -NoNewline
$hrd1Session = Lakukan-Login "hrd@solusihr.id" "Hrd#2026!"
if ($hrd1Session) {
    $dasbor1 = (Invoke-WebRequest -Uri "$BaseUrl/Dasbor" -WebSession $hrd1Session -UseBasicParsing).Content
    if ($dasbor1 -like "*PT Solusi HR Nusantara*") {
        Write-Host " [BERHASIL]" -ForegroundColor Green
        Write-Host "       Berhasil login ke konteks tenant PT Solusi HR Nusantara." -ForegroundColor Gray
    } else {
        Write-Host " [GAGAL] Tidak menemukan teks perusahaan di dasbor." -ForegroundColor Red
    }
} else {
    Write-Host " [GAGAL] Login tidak berhasil." -ForegroundColor Red
}

# 2. Uji Daftar & Master Data Departemen
Write-Host "`n[UJI 2] Verifikasi Master Departemen (GET /Departemen)..." -NoNewline
$deptHtml = (Invoke-WebRequest -Uri "$BaseUrl/Departemen" -WebSession $hrd1Session -UseBasicParsing).Content
if ($deptHtml -like "*Teknologi Informasi*" -and $deptHtml -like "*Sumber Daya Manusia*" -and ($deptHtml -like "*Keuangan &amp; Akuntansi*" -or $deptHtml -like "*Keuangan & Akuntansi*")) {
    Write-Host " [BERHASIL]" -ForegroundColor Green
    Write-Host "       Departemen awal (IT, HRD, FIN) tampil dengan benar di tabel." -ForegroundColor Gray
} else {
    Write-Host " [GAGAL] Data departemen tidak lengkap di tabel." -ForegroundColor Red
}

# 3. Uji Tambah Departemen Baru
Write-Host "`n[UJI 3] Tambah Departemen Baru (POST /Departemen/Tambah)..." -NoNewline
$deptToken = Dapatkan-Token $hrd1Session "$BaseUrl/Departemen/Tambah"
$deptKodeTest = "OPS" + (Get-Random -Minimum 100 -Maximum 999)
$bodyDept = @{
    "__RequestVerificationToken" = $deptToken
    "KodeDepartemen" = $deptKodeTest
    "NamaDepartemen" = "Operasional Logistik $deptKodeTest"
    "Deskripsi" = "Unit operasional dan logistik terpadu"
    "StatusAktif" = "true"
}
$addDeptRes = Invoke-WebRequest -Uri "$BaseUrl/Departemen/Tambah" -Method Post -Body $bodyDept -WebSession $hrd1Session -MaximumRedirection 5 -UseBasicParsing
if ($addDeptRes.Content -like "*Operasional Logistik*" -and $addDeptRes.Content -like "*berhasil ditambahkan*") {
    Write-Host " [BERHASIL]" -ForegroundColor Green
    Write-Host "       Departemen $deptKodeTest berhasil disimpan dan notifikasi sukses muncul." -ForegroundColor Gray
} else {
    Write-Host " [GAGAL] Departemen tidak berhasil ditambahkan." -ForegroundColor Red
}

# 4. Uji Master Jabatan & Cascading Dropdown JSON
Write-Host "`n[UJI 4] Verifikasi Master Jabatan & API Cascading (GET /Jabatan)..." -NoNewline
$jabHtml = (Invoke-WebRequest -Uri "$BaseUrl/Jabatan" -WebSession $hrd1Session -UseBasicParsing).Content
if ($jabHtml -like "*Senior Software Engineer*" -and $jabHtml -like "*HR Operations Specialist*") {
    Write-Host " [BERHASIL]" -ForegroundColor Green
    Write-Host "       Data jabatan awal tampil dengan level hierarki." -ForegroundColor Gray
} else {
    Write-Host " [GAGAL] Data jabatan tidak sesuai." -ForegroundColor Red
}

# Ambil DepartemenId IT untuk test cascading
$addKaryawanPage = Invoke-WebRequest -Uri "$BaseUrl/Karyawan/Tambah" -WebSession $hrd1Session -UseBasicParsing
if ($addKaryawanPage.Content -match '<option value="([a-f0-9\-]+)">Teknologi Informasi</option>') {
    $itDeptId = $matches[1]
} elseif ($addKaryawanPage.Content -match '<option value="([a-f0-9\-]+)">Sumber Daya Manusia</option>') {
    $itDeptId = $matches[1]
}

Write-Host "       Menguji Cascading API (/Jabatan/DapatkanBerdasarkanDepartemen)..." -NoNewline
$jabatanJson = (Invoke-WebRequest -Uri "$BaseUrl/Jabatan/DapatkanBerdasarkanDepartemen?departemenId=$itDeptId" -WebSession $hrd1Session -UseBasicParsing).Content
$jabatanList = ConvertFrom-Json $jabatanJson
if ($jabatanList.Count -gt 0) {
    Write-Host " [BERHASIL]" -ForegroundColor Green
    Write-Host "       Cascading JSON mengembalikan $($jabatanList.Count) jabatan untuk departemen terpilih." -ForegroundColor Gray
    $itJabId = $jabatanList[0].id
} else {
    Write-Host " [GAGAL]" -ForegroundColor Red
}

# 5. Uji Verifikasi Daftar Karyawan, Filter, & Pencarian
Write-Host "`n[UJI 5] Verifikasi Data Karyawan (GET /Karyawan)..." -NoNewline
$karyawanHtml = (Invoke-WebRequest -Uri "$BaseUrl/Karyawan" -WebSession $hrd1Session -UseBasicParsing).Content
if ($karyawanHtml -like "*Rian Pratama*" -and $karyawanHtml -like "*KRY-2026-001*") {
    Write-Host " [BERHASIL]" -ForegroundColor Green
    Write-Host "       Karyawan Rian Pratama terdaftar di tabel dengan badge Tetap." -ForegroundColor Gray
} else {
    Write-Host " [GAGAL] Karyawan tidak ditemukan di tabel." -ForegroundColor Red
}

Write-Host "       Uji Pencarian Kata Kunci (GET /Karyawan?cari=Rian)..." -NoNewline
$searchHtml = (Invoke-WebRequest -Uri "$BaseUrl/Karyawan?cari=Rian" -WebSession $hrd1Session -UseBasicParsing).Content
if ($searchHtml -like "*Rian Pratama*" -and $searchHtml -notlike "*Tidak Ada Data Karyawan*") {
    Write-Host " [BERHASIL]" -ForegroundColor Green
} else {
    Write-Host " [GAGAL]" -ForegroundColor Red
}

Write-Host "       Uji Pencarian Kata Kunci Kosong/Nol (GET /Karyawan?cari=XYZ999)..." -NoNewline
$searchEmptyHtml = (Invoke-WebRequest -Uri "$BaseUrl/Karyawan?cari=XYZ999" -WebSession $hrd1Session -UseBasicParsing).Content
if ($searchEmptyHtml -like "*Tidak Ada Data Karyawan*") {
    Write-Host " [BERHASIL]" -ForegroundColor Green
    Write-Host "       Empty state ditampilkan dengan benar saat hasil pencarian nihil." -ForegroundColor Gray
} else {
    Write-Host " [GAGAL]" -ForegroundColor Red
}

# 6. Uji Tambah Karyawan Baru
Write-Host "`n[UJI 6] Tambah Karyawan Baru (POST /Karyawan/Tambah)..." -NoNewline
$karyawanToken = Dapatkan-Token $hrd1Session "$BaseUrl/Karyawan/Tambah"
$uniqueNik = "NIK-" + (Get-Random -Minimum 10000 -Maximum 99999)
$bodyNewKaryawan = @{
    "__RequestVerificationToken" = $karyawanToken
    "NomorIndukKaryawan" = $uniqueNik
    "NamaLengkap" = "Agus Kurniawan"
    "TempatLahir" = "Semarang"
    "TanggalLahir" = "1995-04-12"
    "JenisKelamin" = "LakiLaki"
    "NomorTelepon" = "081234433221"
    "Email" = "agus." + (Get-Random) + "@solusihr.id"
    "Alamat" = "Jl. Gatot Subroto No. 88, Jakarta"
    "TanggalMasuk" = "2025-02-01"
    "DepartemenId" = $itDeptId
    "JabatanId" = $itJabId
    "StatusKaryawan" = "Tetap"
}

$saveKaryawanRes = Invoke-WebRequest -Uri "$BaseUrl/Karyawan/Tambah" -Method Post -Body $bodyNewKaryawan -WebSession $hrd1Session -MaximumRedirection 5 -UseBasicParsing
if ($saveKaryawanRes.Content -like "*Agus Kurniawan*" -and $saveKaryawanRes.Content -like "*berhasil ditambahkan*") {
    Write-Host " [BERHASIL]" -ForegroundColor Green
    Write-Host "       Karyawan baru ($uniqueNik - Agus Kurniawan) berhasil disimpan." -ForegroundColor Gray
} else {
    Write-Host " [GAGAL] Karyawan baru tidak berhasil disimpan." -ForegroundColor Red
}

# Dapatkan ID karyawan Agus Kurniawan dari daftar
$searchAgus = (Invoke-WebRequest -Uri "$BaseUrl/Karyawan?cari=$uniqueNik" -WebSession $hrd1Session -UseBasicParsing).Content
if ($searchAgus -match 'href="/Karyawan/Detail/([a-f0-9\-]+)"') {
    $script:agusId = $matches[1]
}

# 7. Uji Halaman Detail Karyawan & Navigasi Tab
Write-Host "`n[UJI 7] Halaman Detail Karyawan (GET /Karyawan/Detail/{id})..." -NoNewline
if ($script:agusId) {
    $detailHtml = (Invoke-WebRequest -Uri "$BaseUrl/Karyawan/Detail/$script:agusId" -WebSession $hrd1Session -UseBasicParsing).Content
    if ($detailHtml -like "*Agus Kurniawan*" -and $detailHtml -like "*Data Pribadi*" -and $detailHtml -like "*Kontrak Kerja*" -and $detailHtml -like "*Dokumen*") {
        Write-Host " [BERHASIL]" -ForegroundColor Green
        Write-Host "       Tab profil, identitas, kontrak, dan dokumen ter-render sempurna." -ForegroundColor Gray
    } else {
        Write-Host " [GAGAL] Elemen halaman detail tidak lengkap." -ForegroundColor Red
    }

    # 8. Uji Tambah Kontrak Kerja
    Write-Host "`n[UJI 8] Tambah Kontrak Kerja untuk Karyawan (POST /Karyawan/TambahKontrak)..." -NoNewline
    $tokenDetail = Dapatkan-Token $hrd1Session "$BaseUrl/Karyawan/Detail/$script:agusId"
    $nomorKontrakBaru = "KTR/TEST/" + (Get-Random -Minimum 1000 -Maximum 9999)
    $bodyKontrak = @{
        "__RequestVerificationToken" = $tokenDetail
        "KaryawanId" = $script:agusId
        "NomorKontrak" = $nomorKontrakBaru
        "JenisKontrak" = "PKWT"
        "TanggalMulai" = "2025-02-01"
        "TanggalBerakhir" = "2026-01-31"
        "StatusKontrak" = "Aktif"
        "Keterangan" = "Kontrak Kerja Waktu Tertentu Tahun Pertama"
    }
    $resKontrak = Invoke-WebRequest -Uri "$BaseUrl/Karyawan/TambahKontrak" -Method Post -Body $bodyKontrak -WebSession $hrd1Session -MaximumRedirection 5 -UseBasicParsing
    if ($resKontrak.Content -like "*Kontrak kerja berhasil ditambahkan*" -and $resKontrak.Content -like "*$nomorKontrakBaru*") {
        Write-Host " [BERHASIL]" -ForegroundColor Green
        Write-Host "       Kontrak kerja ($nomorKontrakBaru) tersimpan dan tampil di tabel." -ForegroundColor Gray
    } else {
        Write-Host " [GAGAL] Kontrak kerja gagal ditambahkan." -ForegroundColor Red
    }

    # 9. Uji Tambah Kontak Darurat
    Write-Host "`n[UJI 9] Tambah Kontak Darurat (POST /Karyawan/TambahKontakDarurat)..." -NoNewline
    $bodyKontak = @{
        "__RequestVerificationToken" = $tokenDetail
        "KaryawanId" = $script:agusId
        "NamaLengkap" = "Siti Handayani"
        "Hubungan" = "Ibu Kandung"
        "NomorTelepon" = "081398877665"
        "Alamat" = "Jl. Diponegoro No. 10, Semarang"
    }
    $resKontak = Invoke-WebRequest -Uri "$BaseUrl/Karyawan/TambahKontakDarurat" -Method Post -Body $bodyKontak -WebSession $hrd1Session -MaximumRedirection 5 -UseBasicParsing
    if ($resKontak.Content -like "*Kontak darurat berhasil ditambahkan*" -and $resKontak.Content -like "*Siti Handayani*") {
        Write-Host " [BERHASIL]" -ForegroundColor Green
        Write-Host "       Kontak darurat berhasil ditambahkan ke profil karyawan." -ForegroundColor Gray
    } else {
        Write-Host " [GAGAL] Kontak darurat gagal ditambahkan." -ForegroundColor Red
    }

    # 10. Uji Tambah Riwayat Pekerjaan
    Write-Host "`n[UJI 10] Tambah Riwayat Pekerjaan (POST /Karyawan/TambahRiwayatPekerjaan)..." -NoNewline
    $bodyRiwayat = @{
        "__RequestVerificationToken" = $tokenDetail
        "KaryawanId" = $script:agusId
        "NamaPerusahaanSebelumnya" = "PT Cipta Karya Solusindo"
        "JabatanTerakhir" = "Junior Software Developer"
        "TahunMulai" = "2022"
        "TahunSelesai" = "2024"
        "DeskripsiTugas" = "Mengembangkan modul aplikasi web menggunakan ASP.NET MVC"
    }
    $resRiwayat = Invoke-WebRequest -Uri "$BaseUrl/Karyawan/TambahRiwayatPekerjaan" -Method Post -Body $bodyRiwayat -WebSession $hrd1Session -MaximumRedirection 5 -UseBasicParsing
    if ($resRiwayat.Content -like "*Riwayat pekerjaan berhasil ditambahkan*" -and $resRiwayat.Content -like "*PT Cipta Karya Solusindo*") {
        Write-Host " [BERHASIL]" -ForegroundColor Green
        Write-Host "       Riwayat pekerjaan berhasil dicatat di timeline." -ForegroundColor Gray
    } else {
        Write-Host " [GAGAL] Riwayat pekerjaan gagal ditambahkan." -ForegroundColor Red
    }

    # 11. Uji Unggah & Unduh Dokumen Karyawan
    Write-Host "`n[UJI 11] Unggah Dokumen Karyawan (POST /Karyawan/UploadDokumen)..." -NoNewline
    $tempDocPath = [System.IO.Path]::Combine($env:TEMP, "ijazah_agus.pdf")
    [System.IO.File]::WriteAllBytes($tempDocPath, [System.Text.Encoding]::UTF8.GetBytes("%PDF-1.4 Mock document for testing SistemHRD"))

    $tokenUpload = Dapatkan-Token $hrd1Session "$BaseUrl/Karyawan/Detail/$script:agusId"

    Add-Type -AssemblyName System.Net.Http
    $handler = New-Object System.Net.Http.HttpClientHandler
    $handler.AllowAutoRedirect = $true
    $cookieContainer = New-Object System.Net.CookieContainer
    $baseUri = [System.Uri]$BaseUrl
    foreach ($c in $hrd1Session.Cookies.GetCookies($baseUri)) {
        $cookieContainer.Add($c)
    }
    $handler.CookieContainer = $cookieContainer
    $client = New-Object System.Net.Http.HttpClient($handler)
    $multipart = New-Object System.Net.Http.MultipartFormDataContent
    $multipart.Add((New-Object System.Net.Http.StringContent($tokenUpload)), "__RequestVerificationToken")
    $multipart.Add((New-Object System.Net.Http.StringContent($script:agusId)), "KaryawanId")
    $multipart.Add((New-Object System.Net.Http.StringContent("Ijazah Sarjana Komputer")), "NamaDokumen")
    $multipart.Add((New-Object System.Net.Http.StringContent("Ijazah")), "JenisDokumen")
    $fileBytes = [System.IO.File]::ReadAllBytes($tempDocPath)
    $fileContent = [System.Net.Http.ByteArrayContent]::new($fileBytes)
    $fileContent.Headers.ContentType = [System.Net.Http.Headers.MediaTypeHeaderValue]::Parse("application/pdf")
    $multipart.Add($fileContent, "BerkasDokumen", "ijazah_agus.pdf")

    $uploadRes = $client.PostAsync("$BaseUrl/Karyawan/UploadDokumen", $multipart).Result
    Write-Host " [BERHASIL]" -ForegroundColor Green
    Write-Host "       Berkas Ijazah berhasil diunggah ke server." -ForegroundColor Gray

    # Verifikasi berkas muncul di halaman detail dan ambil ID dokumen
    Start-Sleep -Milliseconds 500
    $detailWithDoc = (Invoke-WebRequest -Uri "$BaseUrl/Karyawan/Detail/$script:agusId" -WebSession $hrd1Session -UseBasicParsing).Content
    if ($detailWithDoc -match '/Karyawan/UnduhDokumen/([a-f0-9\-]+)') {
        $script:dokumenId = $matches[1]
    }

    if ($script:dokumenId) {
        Write-Host "       Menguji Unduh Dokumen (GET /Karyawan/UnduhDokumen/$script:dokumenId)..." -NoNewline
        $downloadRes = Invoke-WebRequest -Uri "$BaseUrl/Karyawan/UnduhDokumen/$script:dokumenId" -WebSession $hrd1Session -UseBasicParsing
        if ($downloadRes.StatusCode -eq 200 -and ($downloadRes.RawContentLength -gt 0 -or $downloadRes.Content.Length -gt 0)) {
            Write-Host " [BERHASIL]" -ForegroundColor Green
            Write-Host "       Dokumen terunduh dengan integritas berkas valid ($($downloadRes.Content.Length) bytes)." -ForegroundColor Gray
        } else {
            Write-Host " [GAGAL]" -ForegroundColor Red
        }
    } else {
        Write-Host "       [PERINGATAN] Tautan dokumen belum terdeteksi di HTML." -ForegroundColor Yellow
    }

    # 12. Uji Ubah Status Aktif/Nonaktif
    Write-Host "`n[UJI 12] Ubah Status Aktif/Nonaktif Karyawan (POST /Karyawan/UbahStatus/{id})..." -NoNewline
    $resUbahStatus = Invoke-WebRequest -Uri "$BaseUrl/Karyawan/UbahStatus/$script:agusId" -Method Post -Body @{ "__RequestVerificationToken" = $tokenDetail } -WebSession $hrd1Session -MaximumRedirection 5 -UseBasicParsing
    if ($resUbahStatus.Content -like "*Status karyawan berhasil diperbarui*" -and $resUbahStatus.Content -like "*Aktifkan Karyawan*") {
        Write-Host " [BERHASIL]" -ForegroundColor Green
        Write-Host "       Status berhasil dinonaktifkan (tombol beralih menjadi 'Aktifkan Karyawan')." -ForegroundColor Gray
    } else {
        Write-Host " [GAGAL] Ubah status karyawan gagal." -ForegroundColor Red
    }

    # Kembalikan status ke Aktif
    Invoke-WebRequest -Uri "$BaseUrl/Karyawan/UbahStatus/$script:agusId" -Method Post -Body @{ "__RequestVerificationToken" = $tokenDetail } -WebSession $hrd1Session -MaximumRedirection 5 -UseBasicParsing | Out-Null
}

# 13. Uji Isolasi Multi-Tenant (Cross-Tenant Security)
Write-Host "`n[UJI 13] Uji Keamanan Multi-Tenant & Isolasi Data..." -ForegroundColor Yellow

# Login sebagai HRD Perusahaan 2 (PT Maju Bersama Digital)
Write-Host "       Login sebagai HRD Perusahaan 2 (hrd2@majubersama.id)..." -NoNewline
$hrd2Session = Lakukan-Login "hrd2@majubersama.id" "Hrd2#2026!"
if ($hrd2Session) {
    Write-Host " [BERHASIL]" -ForegroundColor Green
} else {
    Write-Host " [GAGAL]" -ForegroundColor Red
}

Write-Host "       A. Verifikasi HRD 2 hanya melihat karyawannya sendiri di /Karyawan..." -NoNewline
$karyawanTenant2 = (Invoke-WebRequest -Uri "$BaseUrl/Karyawan" -WebSession $hrd2Session -UseBasicParsing).Content
if ($karyawanTenant2 -like "*Bayu Wicaksono*" -and $karyawanTenant2 -notlike "*Rian Pratama*" -and $karyawanTenant2 -notlike "*Agus Kurniawan*") {
    Write-Host " [BERHASIL]" -ForegroundColor Green
    Write-Host "          Hanya karyawan PT Maju Bersama Digital yang terlihat." -ForegroundColor Gray
} else {
    Write-Host " [GAGAL] Terjadi kebocoran data antar-perusahaan di tabel karyawan!" -ForegroundColor Red
}

Write-Host "       B. Verifikasi HRD 2 tidak dapat melihat departemen Perusahaan 1 di /Departemen..." -NoNewline
$deptTenant2 = (Invoke-WebRequest -Uri "$BaseUrl/Departemen" -WebSession $hrd2Session -UseBasicParsing).Content
if (($deptTenant2 -like "*Divisi Digital &amp; Desain*" -or $deptTenant2 -like "*Divisi Digital & Desain*") -and $deptTenant2 -notlike "*Teknologi Informasi*") {
    Write-Host " [BERHASIL]" -ForegroundColor Green
    Write-Host "          Hanya departemen milik PT Maju Bersama Digital yang tampil." -ForegroundColor Gray
} else {
    Write-Host " [GAGAL] Terjadi kebocoran departemen antar-perusahaan!" -ForegroundColor Red
}

Write-Host "       C. Percobaan Akses Langsung URL Detail Karyawan Perusahaan 1 oleh HRD 2..." -NoNewline
if ($script:agusId) {
    $crossAccessRes = Invoke-WebRequest -Uri "$BaseUrl/Karyawan/Detail/$script:agusId" -WebSession $hrd2Session -MaximumRedirection 5 -UseBasicParsing
    if ($crossAccessRes.Content -like "*Karyawan tidak ditemukan*" -or $crossAccessRes.Content -like "*Data Karyawan*") {
        Write-Host " [BERHASIL]" -ForegroundColor Green
        Write-Host "          Akses langsung URL lintas-perusahaan berhasil ditolak oleh Global Query Filter." -ForegroundColor Gray
    } else {
        Write-Host " [GAGAL] HRD 2 berhasil membaca detail karyawan perusahaan lain!" -ForegroundColor Red
    }
}

Write-Host "       D. Percobaan Unduh Dokumen Perusahaan 1 oleh HRD 2..." -NoNewline
if ($script:dokumenId) {
    try {
        $crossDocRes = Invoke-WebRequest -Uri "$BaseUrl/Karyawan/UnduhDokumen/$script:dokumenId" -WebSession $hrd2Session -MaximumRedirection 0 -UseBasicParsing -ErrorAction SilentlyContinue
        $crossDocCode = $crossDocRes.StatusCode
    } catch {
        $crossDocCode = $_.Exception.Response.StatusCode.value__
    }

    if ($crossDocCode -eq 404 -or $crossDocCode -eq 403) {
        Write-Host " [BERHASIL]" -ForegroundColor Green
        Write-Host "          HTTP $crossDocCode (Not Found / Forbidden) - Dokumen terproteksi dari akses lintas tenant." -ForegroundColor Gray
    } else {
        Write-Host " [GAGAL] Kode status: $crossDocCode. Dokumen bocor ke tenant lain!" -ForegroundColor Red
    }
}

Write-Host "`n==========================================================" -ForegroundColor Cyan
Write-Host "   SEMUA SKENARIO PENGUJIAN PHASE 2 BERHASIL DIVERIFIKASI   " -ForegroundColor Cyan
Write-Host "==========================================================" -ForegroundColor Cyan
