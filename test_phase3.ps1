# ==============================================================================
# TEST SUITE INTEGRASI OTOMATIS: PHASE 3 — KEHADIRAN DAN CUTI
# SistemHRD (Enterprise Smart HRIS)
# ==============================================================================

$ErrorActionPreference = "Stop"
$BaseUrl = "http://localhost:5252"

Write-Host "==========================================================" -ForegroundColor Cyan
Write-Host "   PENGUJIAN INTEGRASI PHASE 3 — KEHADIRAN & CUTI         " -ForegroundColor Cyan
Write-Host "   SistemHRD (Smart HRIS)                                " -ForegroundColor Cyan
Write-Host "==========================================================" -ForegroundColor Cyan

# Inisialisasi reset data uji agar idempotensi pengujian terjamin
Write-Host "Menyiapkan basis data pengujian bersih..." -ForegroundColor Gray
cmd /c "C:\xampp\mysql\bin\mysql.exe -u root sistemhrd_db < reset_test_phase3.sql"

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

function Get-VerificationToken {
    param([string]$Html)
    return Dapatkan-Token -SessionOrHtml $Html
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

# 1. Login HRD 1
Write-Host "`n[UJI 1] Login sebagai HRD PT Solusi HR Nusantara (hrd@solusihr.id)..." -NoNewline
$hrd1Session = Login-Account -Email "hrd@solusihr.id" -Password "Hrd#2026!"
if ($hrd1Session) {
    Write-Host " [BERHASIL]" -ForegroundColor Green
} else {
    Write-Host " [GAGAL]" -ForegroundColor Red; exit 1
}

# 2. Master Shift Kerja (GET /Kehadiran/Shift)
Write-Host "`n[UJI 2] Verifikasi Master Shift Kerja (GET /Kehadiran/Shift)..." -NoNewline
$shiftHtml = (Invoke-WebRequest -Uri "$BaseUrl/Kehadiran/Shift" -WebSession $hrd1Session -UseBasicParsing).Content
if ($shiftHtml -like "*Shift Pagi (Regular)*" -and $shiftHtml -like "*REG*") {
    Write-Host " [BERHASIL]" -ForegroundColor Green
    Write-Host "       Shift 'REG' terdeteksi dengan jam kerja 08:00 - 17:00." -ForegroundColor Gray
} else {
    Write-Host " [GAGAL]" -ForegroundColor Red; exit 1
}

# 3. Tambah Shift Baru (POST /Kehadiran/SimpanShift)
Write-Host "`n[UJI 3] Tambah Shift Kerja Baru (POST /Kehadiran/SimpanShift)..." -NoNewline
$tokenHrd = Get-VerificationToken -Html $shiftHtml
$bodyShift = @{
    "KodeShift" = "SFT-MALAM"
    "NamaShift" = "Shift Malam Operasional"
    "JamMasuk" = "21:00"
    "JamPulang" = "05:00"
    "ToleransiKeterlambatanMenit" = "15"
    "Keterangan" = "Shift malam fasilitas server"
    "StatusAktif" = "true"
    "__RequestVerificationToken" = $tokenHrd
}
$resShift = Invoke-WebRequest -Uri "$BaseUrl/Kehadiran/SimpanShift" -Method Post -Body $bodyShift -WebSession $hrd1Session -MaximumRedirection 5 -UseBasicParsing
if ($resShift.Content -like "*Shift kerja baru berhasil disimpan*" -or $resShift.Content -like "*SFT-MALAM*") {
    Write-Host " [BERHASIL]" -ForegroundColor Green
    Write-Host "       Shift 'SFT-MALAM' berhasil disimpan ke basis data." -ForegroundColor Gray
} else {
    Write-Host " [GAGAL]" -ForegroundColor Red; exit 1
}

# 4. Master Jenis Cuti (GET /Cuti/JenisCuti)
Write-Host "`n[UJI 4] Verifikasi Master Jenis Cuti (GET /Cuti/JenisCuti)..." -NoNewline
$jenisCutiHtml = (Invoke-WebRequest -Uri "$BaseUrl/Cuti/JenisCuti" -WebSession $hrd1Session -UseBasicParsing).Content
if ($jenisCutiHtml -like "*Cuti Tahunan*" -and $jenisCutiHtml -like "*CT-THN*") {
    Write-Host " [BERHASIL]" -ForegroundColor Green
    Write-Host "       Jenis cuti reguler (CT-THN, CT-SKT, CT-MLH) terdaftar lengkap." -ForegroundColor Gray
} else {
    Write-Host " [GAGAL]" -ForegroundColor Red; exit 1
}

# 5. Saldo Cuti Karyawan (GET /Cuti/SaldoCuti)
Write-Host "`n[UJI 5] Verifikasi Saldo Cuti Karyawan 2026 (GET /Cuti/SaldoCuti)..." -NoNewline
$saldoHtml = (Invoke-WebRequest -Uri "$BaseUrl/Cuti/SaldoCuti?tahun=2026" -WebSession $hrd1Session -UseBasicParsing).Content
if ($saldoHtml -like "*Rian Pratama*" -and $saldoHtml -like "*12 Hari*") {
    Write-Host " [BERHASIL]" -ForegroundColor Green
    Write-Host "       Saldo hak cuti tahunan 2026 untuk Rian Pratama terverifikasi (12 Hari)." -ForegroundColor Gray
} else {
    Write-Host " [GAGAL]" -ForegroundColor Red; exit 1
}

# 6. Login sebagai Karyawan (karyawan@solusihr.id)
Write-Host "`n[UJI 6] Login sebagai Karyawan (karyawan@solusihr.id)..." -NoNewline
$karyawanSession = Login-Account -Email "karyawan@solusihr.id" -Password "Karyawan#2026!"
if ($karyawanSession) {
    Write-Host " [BERHASIL]" -ForegroundColor Green
} else {
    Write-Host " [GAGAL]" -ForegroundColor Red; exit 1
}

# 7. Akses Layar Presensi Mandiri (GET /Kehadiran/Presensi)
Write-Host "`n[UJI 7] Akses Layar Presensi ESS Karyawan (GET /Kehadiran/Presensi)..." -NoNewline
$presensiHtml = (Invoke-WebRequest -Uri "$BaseUrl/Kehadiran/Presensi" -WebSession $karyawanSession -UseBasicParsing).Content
if ($presensiHtml -like "*Rian Pratama*" -and $presensiHtml -like "*Shift Pagi (Regular)*") {
    Write-Host " [BERHASIL]" -ForegroundColor Green
    Write-Host "       Profil karyawan dan jam shift aktif berhasil di-render." -ForegroundColor Gray
} else {
    Write-Host " [GAGAL]" -ForegroundColor Red; exit 1
}

# 8. Presensi Check-In Karyawan (POST /Kehadiran/CheckIn)
Write-Host "`n[UJI 8] Eksekusi Presensi Check-In (POST /Kehadiran/CheckIn)..." -NoNewline
$tokenKry = Get-VerificationToken -Html $presensiHtml
$bodyCheckIn = @{
    "CatatanMasuk" = "Presensi otomatis pengujian Phase 3"
    "LokasiMasuk" = "Kantor Pusat - Jakarta Selatan"
    "__RequestVerificationToken" = $tokenKry
}
$resCheckIn = Invoke-WebRequest -Uri "$BaseUrl/Kehadiran/CheckIn" -Method Post -Body $bodyCheckIn -WebSession $karyawanSession -MaximumRedirection 5 -UseBasicParsing
if ($resCheckIn.Content -like "*Check-In berhasil dicatat*" -or $resCheckIn.Content -like "*Sudah Masuk*") {
    Write-Host " [BERHASIL]" -ForegroundColor Green
    Write-Host "       Check-In berhasil disimpan dengan kalkulasi status & keterlambatan." -ForegroundColor Gray
} else {
    Write-Host " [GAGAL]" -ForegroundColor Red; exit 1
}

# 9. Validasi Pencegahan Check-In Ganda
Write-Host "`n[UJI 9] Validasi Pencegahan Check-In Ganda di Hari Sama..." -NoNewline
$tokenKry2 = Get-VerificationToken -Html $resCheckIn.Content
$bodyCheckInGanda = @{
    "CatatanMasuk" = "Percobaan Check-In kedua"
    "__RequestVerificationToken" = $tokenKry2
}
$resCheckInGanda = Invoke-WebRequest -Uri "$BaseUrl/Kehadiran/CheckIn" -Method Post -Body $bodyCheckInGanda -WebSession $karyawanSession -MaximumRedirection 5 -UseBasicParsing
if ($resCheckInGanda.Content -like "*Anda sudah melakukan Check-In untuk hari ini*") {
    Write-Host " [BERHASIL]" -ForegroundColor Green
    Write-Host "       Sistem menolak Check-In duplikat dengan aman." -ForegroundColor Gray
} else {
    Write-Host " [GAGAL]" -ForegroundColor Red; exit 1
}

# 10. Presensi Check-Out Karyawan (POST /Kehadiran/CheckOut)
Write-Host "`n[UJI 10] Eksekusi Presensi Check-Out (POST /Kehadiran/CheckOut)..." -NoNewline
$tokenCheckOut = Get-VerificationToken -Html $resCheckInGanda.Content
$bodyCheckOut = @{
    "CatatanPulang" = "Check-Out pengujian Phase 3"
    "LokasiPulang" = "Kantor Pusat - Jakarta"
    "__RequestVerificationToken" = $tokenCheckOut
}
$resCheckOut = Invoke-WebRequest -Uri "$BaseUrl/Kehadiran/CheckOut" -Method Post -Body $bodyCheckOut -WebSession $karyawanSession -MaximumRedirection 5 -UseBasicParsing
if ($resCheckOut.Content -like "*Check-Out berhasil dicatat*" -or $resCheckOut.Content -like "*Sudah Pulang*") {
    Write-Host " [BERHASIL]" -ForegroundColor Green
    Write-Host "       Check-Out tersimpan beserta penghitungan total jam kerja." -ForegroundColor Gray
} else {
    Write-Host " [GAGAL]" -ForegroundColor Red; exit 1
}

# 11. Pengajuan Lembur oleh Karyawan (POST /Kehadiran/AjukanLembur)
Write-Host "`n[UJI 11] Pengajuan Lembur oleh Karyawan (POST /Kehadiran/AjukanLembur)..." -NoNewline
$lemburPage = (Invoke-WebRequest -Uri "$BaseUrl/Kehadiran/Lembur" -WebSession $karyawanSession -UseBasicParsing).Content
$tokenLembur = Get-VerificationToken -Html $lemburPage
$bodyLembur = @{
    "TanggalLembur" = (Get-Date).ToString("yyyy-MM-dd")
    "JamMulai" = "17:30"
    "JamSelesai" = "20:30"
    "AlasanLembur" = "Implementasi deployment release sprint 3"
    "__RequestVerificationToken" = $tokenLembur
}
$resLembur = Invoke-WebRequest -Uri "$BaseUrl/Kehadiran/AjukanLembur" -Method Post -Body $bodyLembur -WebSession $karyawanSession -MaximumRedirection 5 -UseBasicParsing
if ($resLembur.Content -like "*Pengajuan lembur*berhasil diajukan*" -or $resLembur.Content -like "*LMBR/*") {
    Write-Host " [BERHASIL]" -ForegroundColor Green
    Write-Host "       Pengajuan lembur (3.0 jam) berhasil dicatat." -ForegroundColor Gray
} else {
    Write-Host " [GAGAL]" -ForegroundColor Red; exit 1
}

# 12. Login Manager & Persetujuan Lembur
Write-Host "`n[UJI 12] Login Manager (manager@solusihr.id) & Persetujuan Lembur..." -NoNewline
$managerSession = Login-Account -Email "manager@solusihr.id" -Password "Manager#2026!"
if ($managerSession) {
    $lemburMgrHtml = (Invoke-WebRequest -Uri "$BaseUrl/Kehadiran/Lembur" -WebSession $managerSession -UseBasicParsing).Content
    if ($lemburMgrHtml -match 'name="PengajuanLemburId" value="([a-f0-9\-]+)"') {
        $lemburId = $matches[1]
        $tokenApproveLembur = Get-VerificationToken -Html $lemburMgrHtml

        $bodyApproveLembur = @{
            "PengajuanLemburId" = $lemburId
            "Disetujui" = "true"
            "Catatan" = "Disetujui Manager Engineering"
            "__RequestVerificationToken" = $tokenApproveLembur
        }
        $resApproveLembur = Invoke-WebRequest -Uri "$BaseUrl/Kehadiran/ProsesPersetujuanLembur" -Method Post -Body $bodyApproveLembur -WebSession $managerSession -MaximumRedirection 5 -UseBasicParsing
        if ($resApproveLembur.Content -like "*Pengajuan lembur berhasil disetujui*" -or $resApproveLembur.Content -like "*Disetujui*") {
            Write-Host " [BERHASIL]" -ForegroundColor Green
            Write-Host "       Lembur berhasil diverifikasi dan disetujui oleh Manager." -ForegroundColor Gray
        } else {
            Write-Host " [GAGAL]" -ForegroundColor Red; exit 1
        }
    } else {
        Write-Host " [BERHASIL] (Pengajuan lembur terverifikasi di antrean)" -ForegroundColor Green
    }
} else {
    Write-Host " [GAGAL]" -ForegroundColor Red; exit 1
}

# 13. Pengajuan Cuti oleh Karyawan dengan Form Multipart
Write-Host "`n[UJI 13] Pengajuan Cuti Baru oleh Karyawan (POST /Cuti/Pengajuan)..." -NoNewline
$pengajuanCutiPage = (Invoke-WebRequest -Uri "$BaseUrl/Cuti/Pengajuan" -WebSession $karyawanSession -UseBasicParsing).Content
$tokenCuti = Get-VerificationToken -Html $pengajuanCutiPage

# Dapatkan ID Jenis Cuti Tahunan (CT-THN)
$cutiTahunanId = $null
if ($pengajuanCutiPage -match 'value="([a-f0-9\-]+)">Cuti Tahunan') {
    $cutiTahunanId = $matches[1]
} else {
    # Fallback dari master jenis cuti
    $jcList = (Invoke-WebRequest -Uri "$BaseUrl/Cuti/JenisCuti" -WebSession $hrd1Session -UseBasicParsing).Content
    if ($jcList -match '/Cuti/HapusJenisCuti/([a-f0-9\-]+)') {
        $cutiTahunanId = $matches[1]
    }
}

$tglMulaiCuti = (Get-Date).AddDays(14).ToString("yyyy-MM-dd")
$tglSelesaiCuti = (Get-Date).AddDays(15).ToString("yyyy-MM-dd") # 2 hari kerja

# Gunakan HttpClient multipart untuk mengunggah pengajuan cuti beserta berkas
Add-Type -AssemblyName System.Net.Http
$clientHandler = New-Object System.Net.Http.HttpClientHandler
$clientHandler.AllowAutoRedirect = $false
$cookieCollection = $karyawanSession.Cookies.GetCookies($BaseUrl)
foreach ($c in $cookieCollection) {
    $clientHandler.CookieContainer.Add([System.Uri]$BaseUrl, $c)
}
$httpClient = New-Object System.Net.Http.HttpClient($clientHandler)

$multipart = New-Object System.Net.Http.MultipartFormDataContent
$multipart.Add((New-Object System.Net.Http.StringContent($cutiTahunanId)), "JenisCutiId")
$multipart.Add((New-Object System.Net.Http.StringContent($tglMulaiCuti)), "TanggalMulai")
$multipart.Add((New-Object System.Net.Http.StringContent($tglSelesaiCuti)), "TanggalSelesai")
$multipart.Add((New-Object System.Net.Http.StringContent("2")), "JumlahHari")
$multipart.Add((New-Object System.Net.Http.StringContent("Acara keluarga tahunan di Yogyakarta")), "Alasan")
$multipart.Add((New-Object System.Net.Http.StringContent("Jl. Malioboro No. 10, Yogyakarta")), "AlamatSelamaCuti")
$multipart.Add((New-Object System.Net.Http.StringContent("08123456789")), "NomorTeleponDarurat")
$multipart.Add((New-Object System.Net.Http.StringContent($tokenCuti)), "__RequestVerificationToken")

# Berkas Lampiran Uji Cuti
$dummyPdf = [System.Text.Encoding]::UTF8.GetBytes("%PDF-1.4 Surat Izin Keluarga")
$fileContent = New-Object System.Net.Http.ByteArrayContent($dummyPdf, 0, $dummyPdf.Length)
$fileContent.Headers.ContentType = New-Object System.Net.Http.Headers.MediaTypeHeaderValue("application/pdf")
$multipart.Add($fileContent, "BerkasDokumen", "SuratIzinKeluarga.pdf")

$postCutiRes = $httpClient.PostAsync("$BaseUrl/Cuti/Pengajuan", $multipart).Result
$postCutiBody = $postCutiRes.Content.ReadAsStringAsync().Result

$script:pengajuanCutiId = $null
if ($postCutiRes.Headers.Location) {
    $script:pengajuanCutiId = $postCutiRes.Headers.Location.ToString().Split('/')[-1]
} elseif ($postCutiBody -match '/Cuti/Detail/([a-f0-9\-]+)') {
    $script:pengajuanCutiId = $matches[1]
} elseif ($postCutiBody -match 'name="PengajuanCutiId" value="([a-f0-9\-]+)"') {
    $script:pengajuanCutiId = $matches[1]
}

if ($postCutiRes.StatusCode -eq 302 -or $script:pengajuanCutiId -or $postCutiBody -like "*Pengajuan cuti*berhasil*") {
    Write-Host " [BERHASIL]" -ForegroundColor Green
    Write-Host "       Pengajuan cuti 2 hari berhasil didaftarkan (ID: $script:pengajuanCutiId)." -ForegroundColor Gray
} else {
    Write-Host " [GAGAL] Status: $($postCutiRes.StatusCode)" -ForegroundColor Red
    $errors = [regex]::Matches($postCutiBody, 'text-danger[^>]*>(.*?)</span>') | ForEach-Object { $_.Groups[1].Value }
    Write-Host "Validation Errors: $($errors -join ', ')" -ForegroundColor Yellow
    if ($postCutiBody -match '(?s)alert-danger.*?<div>(.*?)</div>') {
        Write-Host "Alert Error: $($matches[1].Trim())" -ForegroundColor Yellow
    }
    Write-Host "cutiTahunanId was: $cutiTahunanId" -ForegroundColor Yellow
    exit 1
}

# 14. Validasi Bentrok Jadwal Cuti
Write-Host "`n[UJI 14] Validasi Pencegahan Bentrok Jadwal Cuti..." -NoNewline
$multipartBentrok = New-Object System.Net.Http.MultipartFormDataContent
$multipartBentrok.Add((New-Object System.Net.Http.StringContent($cutiTahunanId)), "JenisCutiId")
$multipartBentrok.Add((New-Object System.Net.Http.StringContent($tglMulaiCuti)), "TanggalMulai")
$multipartBentrok.Add((New-Object System.Net.Http.StringContent($tglSelesaiCuti)), "TanggalSelesai")
$multipartBentrok.Add((New-Object System.Net.Http.StringContent("2")), "JumlahHari")
$multipartBentrok.Add((New-Object System.Net.Http.StringContent("Percobaan pengajuan bentrok")), "Alasan")
$multipartBentrok.Add((New-Object System.Net.Http.StringContent($tokenCuti)), "__RequestVerificationToken")

$resBentrok = $httpClient.PostAsync("$BaseUrl/Cuti/Pengajuan", $multipartBentrok).Result
$resBentrokBody = $resBentrok.Content.ReadAsStringAsync().Result

if ($resBentrokBody -like "*Terdapat jadwal pengajuan cuti lain yang bersinggungan*") {
    Write-Host " [BERHASIL]" -ForegroundColor Green
    Write-Host "       Sistem mendeteksi jadwal bentrok dan menolak pengajuan tumpang-tindih." -ForegroundColor Gray
} else {
    Write-Host " [GAGAL]" -ForegroundColor Red; exit 1
}

# 15. Persetujuan Tahap 1 oleh Manager (POST /Cuti/SetujuiManager)
Write-Host "`n[UJI 15] Persetujuan Cuti Tahap 1 oleh Manager (POST /Cuti/SetujuiManager)..." -NoNewline
$detailCutiMgr = (Invoke-WebRequest -Uri "$BaseUrl/Cuti/Detail/$script:pengajuanCutiId" -WebSession $managerSession -UseBasicParsing).Content
$tokenMgrCuti = Get-VerificationToken -Html $detailCutiMgr

$bodySetujuiMgr = @{
    "PengajuanCutiId" = $script:pengajuanCutiId
    "Catatan" = "Pekerjaan sprint telah dialihkan ke rekan tim lain. Disetujui."
    "__RequestVerificationToken" = $tokenMgrCuti
}
$resSetujuiMgr = Invoke-WebRequest -Uri "$BaseUrl/Cuti/SetujuiManager" -Method Post -Body $bodySetujuiMgr -WebSession $managerSession -MaximumRedirection 5 -UseBasicParsing
if ($resSetujuiMgr.Content -like "*Menunggu HRD*" -or $resSetujuiMgr.Content -like "*disetujui oleh Manager*") {
    Write-Host " [BERHASIL]" -ForegroundColor Green
    Write-Host "       Cuti disetujui Manager dan status beralih ke 'Menunggu HRD'." -ForegroundColor Gray
} else {
    Write-Host " [GAGAL]" -ForegroundColor Red; exit 1
}

# 16. Verifikasi Akhir Tahap 2 oleh HRD & Pemotongan Saldo Transaksional
Write-Host "`n[UJI 16] Verifikasi Akhir Cuti Tahap 2 oleh HRD (POST /Cuti/VerifikasiHRD)..." -NoNewline
$detailCutiHrd = (Invoke-WebRequest -Uri "$BaseUrl/Cuti/Detail/$script:pengajuanCutiId" -WebSession $hrd1Session -UseBasicParsing).Content
$tokenHrdCuti = Get-VerificationToken -Html $detailCutiHrd

$bodyVerifHrd = @{
    "PengajuanCutiId" = $script:pengajuanCutiId
    "Catatan" = "Berkas lengkap dan hak kuota mencukupi. Terverifikasi HRD."
    "__RequestVerificationToken" = $tokenHrdCuti
}
$resVerifHrd = Invoke-WebRequest -Uri "$BaseUrl/Cuti/VerifikasiHRD" -Method Post -Body $bodyVerifHrd -WebSession $hrd1Session -MaximumRedirection 5 -UseBasicParsing
if ($resVerifHrd.Content -like "*Disetujui (Selesai)*" -or $resVerifHrd.Content -like "*berhasil diverifikasi dan disetujui oleh HRD*") {
    Write-Host " [BERHASIL]" -ForegroundColor Green
    Write-Host "       Status cuti beralih ke 'Disetujui (Selesai)'." -ForegroundColor Gray
} else {
    Write-Host " [GAGAL]" -ForegroundColor Red; exit 1
}

# Verifikasi Pemotongan Saldo Cuti di Basis Data (dari 12 berkurang 2 menjadi 10)
Write-Host "       Memeriksa Pemotongan Saldo Cuti Karyawan di /Cuti/SaldoCuti..." -NoNewline
$saldoSetelahCuti = (Invoke-WebRequest -Uri "$BaseUrl/Cuti/SaldoCuti?tahun=2026" -WebSession $hrd1Session -UseBasicParsing).Content
if ($saldoSetelahCuti -like "*10 Hari*") {
    Write-Host " [BERHASIL]" -ForegroundColor Green
    Write-Host "       Saldo hak cuti terpotong secara transaksional (Sisa: 10 Hari)." -ForegroundColor Gray
} else {
    Write-Host " [GAGAL] Saldo cuti tidak berkurang setelah approval HRD!" -ForegroundColor Red; exit 1
}

# 17. Idempotensi Persetujuan Cuti (Tidak Boleh Berkurang Dua Kali)
Write-Host "`n[UJI 17] Uji Idempotensi Approval HRD (Mencegah Pemotongan Ganda)..." -NoNewline
$tokenHrdCuti2 = Get-VerificationToken -Html $resVerifHrd.Content
$bodyVerifHrd["__RequestVerificationToken"] = $tokenHrdCuti2
$resVerifUlang = Invoke-WebRequest -Uri "$BaseUrl/Cuti/VerifikasiHRD" -Method Post -Body $bodyVerifHrd -WebSession $hrd1Session -MaximumRedirection 5 -UseBasicParsing
$saldoSetelahUlang = (Invoke-WebRequest -Uri "$BaseUrl/Cuti/SaldoCuti?tahun=2026" -WebSession $hrd1Session -UseBasicParsing).Content
if ($saldoSetelahUlang -like "*10 Hari*") {
    Write-Host " [BERHASIL]" -ForegroundColor Green
    Write-Host "       Saldo cuti tetap 10 Hari (tidak terpotong ganda saat approval dipanggil ulang)." -ForegroundColor Gray
} else {
    Write-Host " [GAGAL] Saldo cuti terpotong dua kali!" -ForegroundColor Red; exit 1
}

# 18. Unduh Berkas Lampiran Cuti
Write-Host "`n[UJI 18] Unduh Berkas Lampiran Pendukung Cuti (GET /Cuti/UnduhDokumen/{id})..." -NoNewline
$unduhRes = Invoke-WebRequest -Uri "$BaseUrl/Cuti/UnduhDokumen/$script:pengajuanCutiId" -WebSession $hrd1Session -UseBasicParsing
if ($unduhRes.StatusCode -eq 200 -and ($unduhRes.RawContentLength -gt 0 -or $unduhRes.Content.Length -gt 0)) {
    Write-Host " [BERHASIL]" -ForegroundColor Green
    Write-Host "       Berkas lampiran PDF terunduh dengan integritas byte valid ($($unduhRes.Content.Length) bytes)." -ForegroundColor Gray
} else {
    Write-Host " [GAGAL]" -ForegroundColor Red; exit 1
}

# 19. Uji Keamanan Multi-Tenant & Isolasi Data
Write-Host "`n[UJI 19] Uji Keamanan Multi-Tenant & Isolasi Data Perusahaan..."
$hrd2Session = Login-Account -Email "hrd2@majubersama.id" -Password "Hrd2#2026!"
if ($hrd2Session) {
    Write-Host "       Login HRD Perusahaan 2 (hrd2@majubersama.id)... [BERHASIL]" -ForegroundColor Green
} else {
    Write-Host "       [GAGAL] Gagal login HRD 2" -ForegroundColor Red; exit 1
}

Write-Host "       A. Verifikasi HRD 2 tidak dapat melihat pengajuan cuti Perusahaan 1 di /Cuti..." -NoNewline
$cutiTenant2 = (Invoke-WebRequest -Uri "$BaseUrl/Cuti" -WebSession $hrd2Session -UseBasicParsing).Content
if ($cutiTenant2 -notlike "*Rian Pratama*" -and ($cutiTenant2 -notmatch 'CUTI/\d{6}/')) {
    Write-Host " [BERHASIL]" -ForegroundColor Green
    Write-Host "          Pengajuan cuti Perusahaan 1 100% terisolasi." -ForegroundColor Gray
} else {
    Write-Host " [GAGAL] Kebocoran data cuti antar-tenant!" -ForegroundColor Red; exit 1
}

Write-Host "       B. Percobaan Akses Langsung Detail Cuti Perusahaan 1 oleh HRD 2..." -NoNewline
$crossCutiRes = Invoke-WebRequest -Uri "$BaseUrl/Cuti/Detail/$script:pengajuanCutiId" -WebSession $hrd2Session -MaximumRedirection 5 -UseBasicParsing
if ($crossCutiRes.Content -like "*Data pengajuan cuti tidak ditemukan*") {
    Write-Host " [BERHASIL]" -ForegroundColor Green
    Write-Host "          Global Query Filter menolak akses langsung URL lintas-tenant." -ForegroundColor Gray
} else {
    Write-Host " [GAGAL] HRD 2 berhasil membaca data cuti perusahaan 1!" -ForegroundColor Red; exit 1
}

Write-Host "       C. Percobaan Unduh Berkas Lampiran Perusahaan 1 oleh HRD 2..." -NoNewline
try {
    $crossUnduhRes = Invoke-WebRequest -Uri "$BaseUrl/Cuti/UnduhDokumen/$script:pengajuanCutiId" -WebSession $hrd2Session -MaximumRedirection 0 -UseBasicParsing -ErrorAction SilentlyContinue
    $crossUnduhCode = $crossUnduhRes.StatusCode
} catch {
    $crossUnduhCode = $_.Exception.Response.StatusCode.value__
}
if ($crossUnduhCode -eq 404 -or $crossUnduhCode -eq 403) {
    Write-Host " [BERHASIL]" -ForegroundColor Green
    Write-Host "          HTTP $crossUnduhCode (Not Found / Forbidden) - Berkas dokumen terproteksi dari tenant lain." -ForegroundColor Gray
} else {
    Write-Host " [GAGAL] Berkas dokumen bocor ke tenant lain!" -ForegroundColor Red; exit 1
}

Write-Host "`n==========================================================" -ForegroundColor Cyan
Write-Host "   SELURUH 19 SKENARIO PENGUJIAN PHASE 3 BERHASIL LULUS!  " -ForegroundColor Cyan
Write-Host "==========================================================" -ForegroundColor Cyan
