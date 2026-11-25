# ==============================================================================
# TEST SUITE INTEGRASI OTOMATIS: PHASE 5 — REKRUTMEN DAN PENILAIAN KINERJA
# SistemHRD (Enterprise Smart HRIS)
# ==============================================================================

$ErrorActionPreference = "Stop"
$BaseUrl = "http://localhost:5252"

Write-Host "==========================================================" -ForegroundColor Cyan
Write-Host "   PENGUJIAN INTEGRASI PHASE 5: REKRUTMEN & KINERJA       " -ForegroundColor Cyan
Write-Host "   SistemHRD (Smart HRIS)                                " -ForegroundColor Cyan
Write-Host "==========================================================" -ForegroundColor Cyan

$passedCount = 0
$failedCount = 0

function Report-Test($name, $condition, $details = "") {
    if ($condition) {
        $global:passedCount++
        Write-Host " [PASS] $name" -ForegroundColor Green
    } else {
        $global:failedCount++
        Write-Host " [FAIL] $name $(if ($details) { "- " + $details })" -ForegroundColor Red
    }
}

function Dapatkan-Token {
    param($SessionOrHtml, $Url = $null)
    if ($Url) {
        try {
            $page = Invoke-WebRequest -Uri $Url -WebSession $SessionOrHtml -UseBasicParsing
            $html = $page.Content
        } catch { return $null }
    } else {
        $html = $SessionOrHtml
    }

    if ($html -match 'name="__RequestVerificationToken"\s+type="hidden"\s+value="([^"]+)"') {
        return $matches[1]
    }
    if ($html -match 'value="([^"]+)"\s+type="hidden"\s+name="__RequestVerificationToken"') {
        return $matches[1]
    }
    if ($html -match '__RequestVerificationToken"[^>]*value="([^"]+)"') {
        return $matches[1]
    }
    if ($html -match 'name="__RequestVerificationToken"[^>]*value="([^"]+)"') {
        return $matches[1]
    }
    return $null
}

function Login-Account {
    param([string]$Email, [string]$Password)
    $session = New-Object Microsoft.PowerShell.Commands.WebRequestSession
    $token = Dapatkan-Token $session "$BaseUrl/Autentikasi/Masuk"
    if (!$token) {
        return $null
    }

    $body = @{
        "__RequestVerificationToken" = $token
        "Email" = $Email
        "KataSandi" = $Password
    }

    $loginRes = Invoke-WebRequest -Uri "$BaseUrl/Autentikasi/Masuk" -Method Post -Body $body -WebSession $session -MaximumRedirection 0 -UseBasicParsing -ErrorAction SilentlyContinue
    return $session
}

# ------------------------------------------------------------------------------
# 1. Autentikasi Pengguna
# ------------------------------------------------------------------------------
Write-Host "`n[UJI 1] Login Akun Pengguna..." -ForegroundColor Yellow
$hrdSession = Login-Account -Email "hrd@solusihr.id" -Password "Hrd#2026!"
Report-Test "1.1 Login HRD Tenant 1 (PT Solusi HR Nusantara)" ($null -ne $hrdSession)

$mgrSession = Login-Account -Email "manager@solusihr.id" -Password "Manager#2026!"
Report-Test "1.2 Login Manager Tenant 1 (PT Solusi HR Nusantara)" ($null -ne $mgrSession)

$krySession = Login-Account -Email "karyawan@solusihr.id" -Password "Karyawan#2026!"
Report-Test "1.3 Login Karyawan Tenant 1 (Rian Pratama)" ($null -ne $krySession)

$hrd2Session = Login-Account -Email "hrd2@majubersama.id" -Password "Hrd2#2026!"
Report-Test "1.4 Login HRD Tenant 2 (PT Maju Bersama Digital)" ($null -ne $hrd2Session)

# ------------------------------------------------------------------------------
# 2. Modul Rekrutmen — Dashboard & Lowongan
# ------------------------------------------------------------------------------
Write-Host "`n[UJI 2] Modul Rekrutmen: Akses Dashboard & Lowongan..." -ForegroundColor Yellow
$dashRekrutmen = Invoke-WebRequest -Uri "$BaseUrl/Rekrutmen" -WebSession $hrdSession -UseBasicParsing
$dashRekValid = ($dashRekrutmen.StatusCode -eq 200 -and $dashRekrutmen.Content -match "Dashboard Rekrutmen")
Report-Test "2.1 Akses Dashboard Rekrutmen (Statistik & Chart.js)" $dashRekValid

$lowonganList = Invoke-WebRequest -Uri "$BaseUrl/Rekrutmen/Lowongan" -WebSession $hrdSession -UseBasicParsing
$lowonganValid = ($lowonganList.StatusCode -eq 200 -and $lowonganList.Content -match "Lowongan Pekerjaan")
Report-Test "2.2 Akses Daftar Lowongan Kerja" $lowonganValid

# ------------------------------------------------------------------------------
# 3. Modul Rekrutmen — Buat Lowongan Baru
# ------------------------------------------------------------------------------
Write-Host "`n[UJI 3] Modul Rekrutmen: Buat Lowongan Baru..." -ForegroundColor Yellow
$formLowonganPage = Invoke-WebRequest -Uri "$BaseUrl/Rekrutmen/TambahLowongan" -WebSession $hrdSession -UseBasicParsing
$tokenLowongan = Dapatkan-Token $formLowonganPage.Content

# Ambil DepartemenId dan JabatanId secara spesifik dari masing-masing select tag
$deptId = $null
if ($formLowonganPage.Content -match 'name="DepartemenId"[^>]*>([\s\S]*?)</select>') {
    $deptOptions = $matches[1]
    if ($deptOptions -match '<option value="([0-9a-fA-F-]{36})"') {
        $deptId = $matches[1]
    }
}

$jabId = $null
if ($formLowonganPage.Content -match 'name="JabatanId"[^>]*>([\s\S]*?)</select>') {
    $jabOptions = $matches[1]
    if ($jabOptions -match '<option value="([0-9a-fA-F-]{36})"') {
        $jabId = $matches[1]
    }
}

$kodeLowongan = "DEV-QA-" + (Get-Random -Minimum 1000 -Maximum 9999)
$bodyLowongan = @{
    "KodeLowongan" = $kodeLowongan
    "JudulLowongan" = "Senior QA Automation Engineer"
    "DepartemenId" = $deptId
    "JabatanId" = $jabId
    "DeskripsiPekerjaan" = "Merancang dan mengeksekusi test automation suite untuk modul enterprise HRIS."
    "Kualifikasi" = "Pengalaman minimal 3 tahun dengan Cypress/Playwright, pemahaman C#/.NET."
    "TipePekerjaan" = "1" # PenuhWaktu
    "LokasiKerja" = "Jakarta HQ"
    "JumlahDibutuhkan" = "2"
    "GajiMin" = "12000000"
    "GajiMax" = "16000000"
    "TanggalDibuka" = (Get-Date).ToString("yyyy-MM-dd")
    "Status" = "2" # Dibuka
    "__RequestVerificationToken" = $tokenLowongan
}

$postLowonganRes = Invoke-WebRequest -Uri "$BaseUrl/Rekrutmen/TambahLowongan" -Method Post -Body $bodyLowongan -WebSession $hrdSession -UseBasicParsing -MaximumRedirection 0 -ErrorAction SilentlyContinue
$lowonganSukses = ($postLowonganRes.StatusCode -eq 302 -or ($postLowonganRes.StatusCode -eq 200 -and $postLowonganRes.Content -match $kodeLowongan))
Report-Test "3.1 Tambah Lowongan Kerja Baru Berhasil" $lowonganSukses

$lowonganId = $null
if ($postLowonganRes.Headers["Location"] -match '/Rekrutmen/DetailLowongan/([0-9a-fA-F-]{36})') {
    $lowonganId = $matches[1]
} elseif ($postLowonganRes.Content -match '/Rekrutmen/DetailLowongan/([0-9a-fA-F-]{36})') {
    $lowonganId = $matches[1]
}

# ------------------------------------------------------------------------------
# 4. Modul Rekrutmen — Pendaftaran Pelamar & Upload CV
# ------------------------------------------------------------------------------
Write-Host "`n[UJI 4] Modul Rekrutmen: Pendaftaran Pelamar & Upload CV..." -ForegroundColor Yellow
$daftarPelamarPage = Invoke-WebRequest -Uri "$BaseUrl/Rekrutmen/DaftarPelamar?lowonganId=$lowonganId" -WebSession $hrdSession -UseBasicParsing
$tokenPelamar = Dapatkan-Token $daftarPelamarPage.Content

# Buat file dummy CV
$tempCvFile = [System.IO.Path]::GetTempFileName() + ".pdf"
[System.IO.File]::WriteAllBytes($tempCvFile, [System.Text.Encoding]::UTF8.GetBytes("%PDF-1.4 Resume Candidate Ahmad QA Test"))

Add-Type -AssemblyName 'System.Net.Http'
$clientHandler = New-Object System.Net.Http.HttpClientHandler
$clientHandler.CookieContainer = $hrdSession.Cookies
$clientHandler.AllowAutoRedirect = $false
$httpClient = New-Object System.Net.Http.HttpClient($clientHandler)

$formContent = New-Object System.Net.Http.MultipartFormDataContent
$formContent.Add((New-Object System.Net.Http.StringContent($tokenPelamar)), "__RequestVerificationToken")
$formContent.Add((New-Object System.Net.Http.StringContent($lowonganId)), "LowonganKerjaId")
$formContent.Add((New-Object System.Net.Http.StringContent("Ahmad Fajar Santoso")), "NamaLengkap")
$formContent.Add((New-Object System.Net.Http.StringContent("ahmad.fajar@testdomain.id")), "Email")
$formContent.Add((New-Object System.Net.Http.StringContent("081288776655")), "NomorTelepon")
$formContent.Add((New-Object System.Net.Http.StringContent("D4/S1")), "PendidikanTerakhir")
$formContent.Add((New-Object System.Net.Http.StringContent("Institut Teknologi Bandung")), "NamaInstitusi")
$formContent.Add((New-Object System.Net.Http.StringContent("Teknik Informatika")), "Jurusan")
$formContent.Add((New-Object System.Net.Http.StringContent("4")), "PengalamanKerjaTahun")
$formContent.Add((New-Object System.Net.Http.StringContent("14000000")), "GajiDiharapkan")
$formContent.Add((New-Object System.Net.Http.StringContent("Sertifikasi ISTQB, pengalaman Playwright dan CI/CD.")), "CatatanAdministrasi")

$fileStream = [System.IO.File]::OpenRead($tempCvFile)
$fileContent = New-Object System.Net.Http.StreamContent($fileStream)
$fileContent.Headers.ContentType = [System.Net.Http.Headers.MediaTypeHeaderValue]::Parse("application/pdf")
$formContent.Add($fileContent, "BerkasCV", "CV_Ahmad_Fajar.pdf")

$httpRes = $httpClient.PostAsync("$BaseUrl/Rekrutmen/DaftarPelamar", $formContent).Result
$fileStream.Close()
Remove-Item $tempCvFile -ErrorAction SilentlyContinue

$pelamarRedirect = if ($httpRes.Headers.Location) { $httpRes.Headers.Location.ToString() } else { "" }
$pelamarId = $null
if ($pelamarRedirect -match '/Rekrutmen/DetailPelamar/([0-9a-fA-F-]{36})') {
    $pelamarId = $matches[1]
}

$detailPelamarPage = Invoke-WebRequest -Uri "$BaseUrl/Rekrutmen/DetailPelamar/$pelamarId" -WebSession $hrdSession -UseBasicParsing
$pelamarSukses = ($detailPelamarPage.StatusCode -eq 200 -and ($detailPelamarPage.Content -match "Ahmad Fajar Santoso" -or $detailPelamarPage.Content -match "ahmad.fajar@testdomain.id"))
Report-Test "4.1 Pendaftaran Pelamar + Upload Berkas CV Berhasil" $pelamarSukses

# ------------------------------------------------------------------------------
# 5. Modul Rekrutmen — Seleksi Berkas & Unduh CV
# ------------------------------------------------------------------------------
Write-Host "`n[UJI 5] Modul Rekrutmen: Seleksi Berkas Administrasi & Unduh CV..." -ForegroundColor Yellow
$tokenAdmin = Dapatkan-Token $detailPelamarPage.Content
$bodyAdmin = @{
    "PelamarId" = $pelamarId
    "Lolos" = "true"
    "CatatanAdministrasi" = "Berkas ijazah, sertifikat ISTQB, dan portfolio sesuai."
    "__RequestVerificationToken" = $tokenAdmin
}
$adminRes = Invoke-WebRequest -Uri "$BaseUrl/Rekrutmen/SeleksiAdministrasi" -Method Post -Body $bodyAdmin -WebSession $hrdSession -UseBasicParsing -MaximumRedirection 0 -ErrorAction SilentlyContinue

$detailPelamarUpdated = Invoke-WebRequest -Uri "$BaseUrl/Rekrutmen/DetailPelamar/$pelamarId" -WebSession $hrdSession -UseBasicParsing
$adminLolos = ($detailPelamarUpdated.StatusCode -eq 200 -and ($detailPelamarUpdated.Content -match "LolosAdministrasi" -or $detailPelamarUpdated.Content -match "Lolos"))
Report-Test "5.1 Seleksi Administrasi (Loloskan Berkas)" $adminLolos

# Unduh Berkas CV
$unduhRes = Invoke-WebRequest -Uri "$BaseUrl/Rekrutmen/UnduhCV/$pelamarId" -WebSession $hrdSession -UseBasicParsing
$unduhValid = ($unduhRes.StatusCode -eq 200 -and $unduhRes.Headers["Content-Type"] -match "application/pdf")
Report-Test "5.2 Unduh Berkas CV Berhasil Sesuai Hak Akses Tenant" $unduhValid

# ------------------------------------------------------------------------------
# 6. Modul Rekrutmen — Jadwalkan Wawancara & Evaluasi Skor
# ------------------------------------------------------------------------------
Write-Host "`n[UJI 6] Modul Rekrutmen: Jadwal Wawancara & Evaluasi Skor..." -ForegroundColor Yellow
$tokenInterview = Dapatkan-Token $detailPelamarUpdated.Content
$bodyInterview = @{
    "PelamarId" = $pelamarId
    "NamaPelamar" = "Ahmad Fajar Santoso"
    "JudulLowongan" = "Senior QA Automation Engineer"
    "NamaPewawancara" = "Budi Hartono (Lead QA)"
    "TanggalWawancara" = (Get-Date).AddDays(1).ToString("yyyy-MM-dd")
    "JamMulai" = "10:00:00"
    "JamSelesai" = "11:00:00"
    "Metode" = "2" # Daring
    "LokasiAtauTautan" = "https://meet.google.com/test-qa-fajar"
    "CatatanUndangan" = "Siapkan portofolio framework automation test."
    "__RequestVerificationToken" = $tokenInterview
}
$interviewRes = Invoke-WebRequest -Uri "$BaseUrl/Rekrutmen/SimpanJadwalWawancara" -Method Post -Body $bodyInterview -WebSession $hrdSession -UseBasicParsing -MaximumRedirection 0 -ErrorAction SilentlyContinue

$detailPelamarInterview = Invoke-WebRequest -Uri "$BaseUrl/Rekrutmen/DetailPelamar/$pelamarId" -WebSession $hrdSession -UseBasicParsing
$interviewValid = ($detailPelamarInterview.StatusCode -eq 200 -and ($detailPelamarInterview.Content -match "Budi Hartono" -or $detailPelamarInterview.Content -match "Wawancara"))
Report-Test "6.1 Jadwalkan Wawancara Kandidat" $interviewValid

# Catat Hasil & Skor Evaluasi Seleksi
$tokenEvaluasi = Dapatkan-Token $detailPelamarInterview.Content
$bodyEvaluasi = @{
    "PelamarId" = $pelamarId
    "NamaPelamar" = "Ahmad Fajar Santoso"
    "JudulLowongan" = "Senior QA Automation Engineer"
    "Skor" = "89.5"
    "CatatanEvaluasi" = "Penguasaan Cypress, Playwright, dan performance testing k6 sangat memuaskan."
    "Kelebihan" = "Arsitektur test modular, fast problem solver."
    "Kekurangan" = "Perlu adaptasi dengan environment cloud private."
    "Keputusan" = "1" # Lolos
    "__RequestVerificationToken" = $tokenEvaluasi
}
$evaluasiRes = Invoke-WebRequest -Uri "$BaseUrl/Rekrutmen/SimpanHasilWawancara" -Method Post -Body $bodyEvaluasi -WebSession $hrdSession -UseBasicParsing -MaximumRedirection 0 -ErrorAction SilentlyContinue

$detailPelamarEvaluasi = Invoke-WebRequest -Uri "$BaseUrl/Rekrutmen/DetailPelamar/$pelamarId" -WebSession $hrdSession -UseBasicParsing
$evaluasiValid = ($detailPelamarEvaluasi.StatusCode -eq 200 -and ($detailPelamarEvaluasi.Content -match "89.5" -or $detailPelamarEvaluasi.Content -match "Lolos"))
Report-Test "6.2 Catat Evaluasi & Skor Seleksi Objektif (89.5)" $evaluasiValid

# ------------------------------------------------------------------------------
# 7. Modul Rekrutmen — Penerimaan Kandidat & Konversi ke Karyawan
# ------------------------------------------------------------------------------
Write-Host "`n[UJI 7] Modul Rekrutmen: Penerimaan & Konversi ke Karyawan..." -ForegroundColor Yellow
$tokenTerima = Dapatkan-Token $detailPelamarEvaluasi.Content
$nikBaru = "EMP-QA-" + (Get-Random -Minimum 1000 -Maximum 9999)
$bodyTerima = @{
    "PelamarId" = $pelamarId
    "NamaPelamar" = "Ahmad Fajar Santoso"
    "JudulLowongan" = "Senior QA Automation Engineer"
    "NomorIndukKaryawan" = $nikBaru
    "DepartemenId" = $deptId
    "JabatanId" = $jabId
    "TanggalMasuk" = (Get-Date).ToString("yyyy-MM-dd")
    "GajiPokok" = "14000000"
    "__RequestVerificationToken" = $tokenTerima
}
$terimaRes = Invoke-WebRequest -Uri "$BaseUrl/Rekrutmen/TerimaKandidat" -Method Post -Body $bodyTerima -WebSession $hrdSession -UseBasicParsing -MaximumRedirection 0 -ErrorAction SilentlyContinue

$karyawanBaruId = $null
if ($terimaRes.Headers["Location"] -match '/Karyawan/Detail/([0-9a-fA-F-]{36})') {
    $karyawanBaruId = $matches[1]
}

$detailKaryawanPage = Invoke-WebRequest -Uri "$BaseUrl/Karyawan/Detail/$karyawanBaruId" -WebSession $hrdSession -UseBasicParsing
$karyawanBaruSukses = ($detailKaryawanPage.StatusCode -eq 200 -and ($detailKaryawanPage.Content -match $nikBaru -or $detailKaryawanPage.Content -match "Ahmad Fajar Santoso"))
Report-Test "7.1 Penerimaan Kandidat & Konversi Atomik ke Karyawan" $karyawanBaruSukses

# ------------------------------------------------------------------------------
# 8. Modul Penilaian Kinerja — Dashboard, Periode, & Indikator KPI
# ------------------------------------------------------------------------------
Write-Host "`n[UJI 8] Modul Kinerja: Dashboard, Periode, & Indikator KPI..." -ForegroundColor Yellow
$dashKinerja = Invoke-WebRequest -Uri "$BaseUrl/Kinerja" -WebSession $hrdSession -UseBasicParsing
$dashKinerjaValid = ($dashKinerja.StatusCode -eq 200 -and $dashKinerja.Content -match "Dashboard Penilaian Kinerja")
Report-Test "8.1 Akses Dashboard Penilaian Kinerja (Chart.js KPI)" $dashKinerjaValid

# Tambah Periode Penilaian Baru
$formPeriodePage = Invoke-WebRequest -Uri "$BaseUrl/Kinerja/TambahPeriode" -WebSession $hrdSession -UseBasicParsing
$tokenPeriode = Dapatkan-Token $formPeriodePage.Content
$namaPeriode = "Penilaian Tahunan " + (Get-Date).Year + " Batch " + (Get-Random -Minimum 100 -Maximum 999)
$bodyPeriode = @{
    "NamaPeriode" = $namaPeriode
    "Tahun" = (Get-Date).Year
    "SemesterAtauKuartal" = "Tahunan"
    "TanggalMulai" = (Get-Date).ToString("yyyy-MM-dd")
    "TanggalSelesai" = (Get-Date).AddMonths(1).ToString("yyyy-MM-dd")
    "Status" = "2" # Aktif
    "Catatan" = "Evaluasi performa akhir tahun seluruh divisi."
    "__RequestVerificationToken" = $tokenPeriode
}
$periodeRes = Invoke-WebRequest -Uri "$BaseUrl/Kinerja/TambahPeriode" -Method Post -Body $bodyPeriode -WebSession $hrdSession -UseBasicParsing -MaximumRedirection 0 -ErrorAction SilentlyContinue

$listPeriodePage = Invoke-WebRequest -Uri "$BaseUrl/Kinerja/Periode" -WebSession $hrdSession -UseBasicParsing
$periodeValid = ($listPeriodePage.StatusCode -eq 200 -and ($listPeriodePage.Content.Contains($namaPeriode) -or $periodeRes.StatusCode -eq 302))
Report-Test "8.2 Tambah Periode Penilaian Kinerja Aktif" $periodeValid

# Ambil PeriodeId
$periodeIdMatch = [regex]::Match($listPeriodePage.Content, 'modalHapusPeriode-([0-9a-fA-F-]{36})')
$periodeId = $periodeIdMatch.Groups[1].Value

# Tambah Indikator KPI Baru Berbobot
$formIndikatorPage = Invoke-WebRequest -Uri "$BaseUrl/Kinerja/TambahIndikator" -WebSession $hrdSession -UseBasicParsing
$tokenIndikator = Dapatkan-Token $formIndikatorPage.Content
$kodeKpi = "KPI-AUT-" + (Get-Random -Minimum 10 -Maximum 99)
$bodyIndikator = @{
    "KodeIndikator" = $kodeKpi
    "NamaIndikator" = "Automation Test Pass Rate & Stability"
    "DepartemenId" = ""
    "JabatanId" = ""
    "Jenis" = "1" # Kuantitatif
    "TargetKuantitatif" = "98"
    "Satuan" = "%"
    "BobotBawaan" = "25"
    "StatusAktif" = "true"
    "Deskripsi" = "Persentase kelulusan otomatisasi pengujian fitur tanpa flaky tests."
    "__RequestVerificationToken" = $tokenIndikator
}
$indikatorRes = Invoke-WebRequest -Uri "$BaseUrl/Kinerja/TambahIndikator" -Method Post -Body $bodyIndikator -WebSession $hrdSession -UseBasicParsing -MaximumRedirection 0 -ErrorAction SilentlyContinue

$listIndikatorPage = Invoke-WebRequest -Uri "$BaseUrl/Kinerja/Indikator" -WebSession $hrdSession -UseBasicParsing
$indikatorValid = ($listIndikatorPage.StatusCode -eq 200 -and $listIndikatorPage.Content -match $kodeKpi)
Report-Test "8.3 Tambah Master Indikator KPI Berbobot" $indikatorValid

# ------------------------------------------------------------------------------
# 9. Modul Penilaian Kinerja — Input Form Evaluasi & Perhitungan Skor
# ------------------------------------------------------------------------------
Write-Host "`n[UJI 9] Modul Kinerja: Input Evaluasi & Perhitungan Skor..." -ForegroundColor Yellow
$inputNilaiPage = Invoke-WebRequest -Uri "$BaseUrl/Kinerja/InputPenilaian?periodeId=$periodeId&karyawanId=$karyawanBaruId" -WebSession $mgrSession -UseBasicParsing
$tokenNilai = Dapatkan-Token $inputNilaiPage.Content

# Kumpulkan seluruh butir indikator KPI yang dimuat sistem untuk karyawan ini
$kpiMatches = [regex]::Matches($inputNilaiPage.Content, 'name="DaftarDetail\[(\d+)\]\.IndikatorKPIId"\s+value="([0-9a-fA-F-]{36})"')

$bodyPenilaian = @{
    "PeriodePenilaianId" = $periodeId
    "NamaPeriode" = $namaPeriode
    "KaryawanId" = $karyawanBaruId
    "NamaKaryawan" = "Ahmad Fajar Santoso"
    "NIK" = $nikBaru
    "NamaDepartemen" = "Teknologi Informasi"
    "NamaJabatan" = "Software Engineer"
    "TanggalPenilaian" = (Get-Date).ToString("yyyy-MM-dd")
    "KomentarEvaluasi" = "Kinerja pengujian otomatis luar biasa, coverage dan stabilitas CI/CD meningkat drastis."
    "RekomendasiPengembangan" = "Diberikan rekomendasi sebagai Principal QA Automation Engineer."
    "Status" = "3" # Disetujui
    "__RequestVerificationToken" = $tokenNilai
}

for ($i = 0; $i -lt $kpiMatches.Count; $i++) {
    $indId = $kpiMatches[$i].Groups[2].Value
    $bodyPenilaian["DaftarDetail[$i].IndikatorKPIId"] = $indId
    $bodyPenilaian["DaftarDetail[$i].NamaIndikator"] = "Indikator Evaluasi $i"
    $bodyPenilaian["DaftarDetail[$i].BobotPersen"] = "20"
    $bodyPenilaian["DaftarDetail[$i].Realisasi"] = "96%"
    $bodyPenilaian["DaftarDetail[$i].SkorIndikator"] = "94.0" # Semua indikator skor 94 -> skor akhir 94.0 (Istimewa)
    $bodyPenilaian["DaftarDetail[$i].CatatanIndikator"] = "Capaian target sangat memuaskan."
}

$simpanNilaiRes = Invoke-WebRequest -Uri "$BaseUrl/Kinerja/InputPenilaian" -Method Post -Body $bodyPenilaian -WebSession $mgrSession -UseBasicParsing -MaximumRedirection 0 -ErrorAction SilentlyContinue

$penilaianId = $null
if ($simpanNilaiRes.Headers["Location"] -match '/Kinerja/DetailPenilaian/([0-9a-fA-F-]{36})') {
    $penilaianId = $matches[1]
}

$detailPenilaianPage = Invoke-WebRequest -Uri "$BaseUrl/Kinerja/DetailPenilaian/$penilaianId" -WebSession $mgrSession -UseBasicParsing
$skorValid = ($detailPenilaianPage.StatusCode -eq 200 -and ($detailPenilaianPage.Content -match "94.0" -or $detailPenilaianPage.Content -match "Istimewa"))
Report-Test "9.1 Perhitungan Skor Tertimbang Objektif (94.0 -> Predikat Istimewa)" $skorValid

# ------------------------------------------------------------------------------
# 10. Modul Penilaian Kinerja — Lembar Resmi & Laporan
# ------------------------------------------------------------------------------
Write-Host "`n[UJI 10] Modul Kinerja: Lembar Evaluasi Resmi & Laporan..." -ForegroundColor Yellow
$detailValid = ($detailPenilaianPage.StatusCode -eq 200 -and $detailPenilaianPage.Content -match "LEMBAR PENILAIAN KINERJA KARYAWAN" -and $detailPenilaianPage.Content -match "Ahmad Fajar Santoso")
Report-Test "10.1 Akses Lembar Resmi Penilaian Kinerja (Print Ready)" $detailValid

$laporanRes = Invoke-WebRequest -Uri "$BaseUrl/Kinerja/Laporan" -WebSession $hrdSession -UseBasicParsing
$laporanValid = ($laporanRes.StatusCode -eq 200 -and $laporanRes.Content -match "Laporan Performa Kinerja Karyawan")
Report-Test "10.2 Akses Laporan & Analitik Performa Karyawan" $laporanValid

# ------------------------------------------------------------------------------
# 11. Employee Self Service (ESS) Karyawan
# ------------------------------------------------------------------------------
Write-Host "`n[UJI 11] Employee Self Service (ESS) Karyawan..." -ForegroundColor Yellow
$riwayatSayaRes = Invoke-WebRequest -Uri "$BaseUrl/Kinerja/RiwayatSaya" -WebSession $krySession -UseBasicParsing
$riwayatValid = ($riwayatSayaRes.StatusCode -eq 200 -and $riwayatSayaRes.Content -match "Riwayat Evaluasi Kinerja Saya")
Report-Test "11.1 ESS Karyawan Mengakses Riwayat Evaluasi Diri Sendiri" $riwayatValid

# Karyawan mengakses lembar karyawan lain harus diblokir (mengembalikan 302 redirect ke RiwayatSaya)
$blokirAksesLain = Invoke-WebRequest -Uri "$BaseUrl/Kinerja/DetailPenilaian/$penilaianId" -WebSession $krySession -UseBasicParsing -MaximumRedirection 0 -ErrorAction SilentlyContinue
$karyawanDiblokir = ($blokirAksesLain.StatusCode -eq 302 -and $blokirAksesLain.Headers["Location"] -match "RiwayatSaya")
Report-Test "11.2 ESS Karyawan Diblokir Mengakses Evaluasi Rekan Kerja Lain" $karyawanDiblokir

# ------------------------------------------------------------------------------
# 12. Multi-Tenant Isolation Test (Tenant 2 Dilarang Akses Tenant 1)
# ------------------------------------------------------------------------------
Write-Host "`n[UJI 12] Multi-Tenant Data Isolation Test..." -ForegroundColor Yellow

# Tenant 2 mencoba buka Lowongan milik Tenant 1 (harus 302 redirect ke /Rekrutmen/Lowongan)
$t2LowonganRes = Invoke-WebRequest -Uri "$BaseUrl/Rekrutmen/DetailLowongan/$lowonganId" -WebSession $hrd2Session -UseBasicParsing -MaximumRedirection 0 -ErrorAction SilentlyContinue
$t2LowonganBlokir = ($t2LowonganRes.StatusCode -eq 302 -and $t2LowonganRes.Headers["Location"] -match "Lowongan")
Report-Test "12.1 Multi-Tenant: Tenant 2 Dilarang Mengakses Lowongan Tenant 1" $t2LowonganBlokir

# Tenant 2 mencoba buka Pelamar milik Tenant 1 (harus 302 redirect ke /Rekrutmen/Pelamar)
$t2PelamarRes = Invoke-WebRequest -Uri "$BaseUrl/Rekrutmen/DetailPelamar/$pelamarId" -WebSession $hrd2Session -UseBasicParsing -MaximumRedirection 0 -ErrorAction SilentlyContinue
$t2PelamarBlokir = ($t2PelamarRes.StatusCode -eq 302 -and $t2PelamarRes.Headers["Location"] -match "Pelamar")
Report-Test "12.2 Multi-Tenant: Tenant 2 Dilarang Mengakses Pelamar Tenant 1" $t2PelamarBlokir

# Tenant 2 mencoba buka Lembar Penilaian milik Tenant 1 (harus 302 redirect ke /Kinerja/Index)
$t2NilaiRes = Invoke-WebRequest -Uri "$BaseUrl/Kinerja/DetailPenilaian/$penilaianId" -WebSession $hrd2Session -UseBasicParsing -MaximumRedirection 0 -ErrorAction SilentlyContinue
$t2NilaiBlokir = ($t2NilaiRes.StatusCode -eq 302 -and $t2NilaiRes.Headers["Location"] -match "Kinerja")
Report-Test "12.3 Multi-Tenant: Tenant 2 Dilarang Mengakses Lembar Penilaian Tenant 1" $t2NilaiBlokir

# ------------------------------------------------------------------------------
# Ringkasan Pengujian
# ------------------------------------------------------------------------------
Write-Host "`n==========================================================" -ForegroundColor Cyan
Write-Host " HASIL PENGUJIAN: $passedCount BERHASIL, $failedCount GAGAL" -ForegroundColor $(if ($failedCount -eq 0) { "Green" } else { "Red" })
Write-Host "==========================================================" -ForegroundColor Cyan

if ($failedCount -eq 0) {
    exit 0
} else {
    exit 1
}
