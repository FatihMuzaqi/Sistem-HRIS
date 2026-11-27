# ==============================================================================
# TEST SUITE INTEGRASI OTOMATIS: PHASE 6 — PELATIHAN, ASET DAN LAPORAN
# SistemHRD (Enterprise Smart HRIS)
# ==============================================================================

$ErrorActionPreference = "Stop"
$BaseUrl = "http://localhost:5252"

Write-Host "==========================================================" -ForegroundColor Cyan
Write-Host "   PENGUJIAN INTEGRASI PHASE 6: PELATIHAN, ASET & LAPORAN " -ForegroundColor Cyan
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

# -------------------------------------------------------------
# 1. LOGIN SEBAGAI HRD TENANT 1
# -------------------------------------------------------------
Write-Host "`n>>> [1/6] LOGIN SEBAGAI HRD (PT Solusi HR Nusantara)" -ForegroundColor Yellow
$sessionHrd = Login-Account "hrd@solusihr.id" "Hrd#2026!"
Report-Test "Login HRD Berhasil" ($sessionHrd -ne $null)

# -------------------------------------------------------------
# 2. PENGUJIAN MODUL PELATIHAN & SERTIFIKAT
# -------------------------------------------------------------
Write-Host "`n>>> [2/6] PENGUJIAN MODUL PELATIHAN & SERTIFIKASI" -ForegroundColor Yellow

# 2.1 Dashboard Pelatihan
$dashPelatihan = Invoke-WebRequest -Uri "$BaseUrl/Pelatihan" -WebSession $sessionHrd -UseBasicParsing
Report-Test "Akses Dashboard Pelatihan (HTTP 200)" ($dashPelatihan.StatusCode -eq 200 -and $dashPelatihan.Content -match "Pusat Pelatihan & Pengembangan SDM")

# 2.2 Katalog Program
$katalogPelatihan = Invoke-WebRequest -Uri "$BaseUrl/Pelatihan/Program" -WebSession $sessionHrd -UseBasicParsing
Report-Test "Akses Katalog Program Pelatihan (HTTP 200)" ($katalogPelatihan.StatusCode -eq 200 -and $katalogPelatihan.Content -match "Katalog Program Pelatihan")

# 2.3 Tambah Program Pelatihan Baru
$tokenProgram = Dapatkan-Token $sessionHrd "$BaseUrl/Pelatihan/TambahProgram"
$kodeBaru = "TRN-TEST-" + (Get-Random -Minimum 1000 -Maximum 9999)
$bodyTambahProgram = @{
    "__RequestVerificationToken" = $tokenProgram
    "KodeProgram" = $kodeBaru
    "NamaProgram" = "Advanced Cloud & DevOps Engineering"
    "Deskripsi" = "Pelatihan implementasi Docker container, Kubernetes clustering, dan pipeline CI/CD."
    "Penyelenggara" = "Divisi People Development"
    "Instruktur" = "Budi Hartono, M.T."
    "Metode" = "InHouse"
    "LokasiAtauTautan" = "Lab Komputer Lt. 15"
    "TanggalMulai" = (Get-Date).AddDays(5).ToString("yyyy-MM-dd")
    "TanggalSelesai" = (Get-Date).AddDays(8).ToString("yyyy-MM-dd")
    "TotalJam" = "16.0"
    "MaksimalPeserta" = "20"
    "BiayaPerPeserta" = "750000"
    "Status" = "PendaftaranDibuka"
}

$resTambahProgram = Invoke-WebRequest -Uri "$BaseUrl/Pelatihan/TambahProgram" -Method Post -Body $bodyTambahProgram -WebSession $sessionHrd -MaximumRedirection 0 -UseBasicParsing -ErrorAction SilentlyContinue
Report-Test "Tambah Program Pelatihan Baru (302 Redirect)" ($resTambahProgram.StatusCode -eq 302)

$locProgram = $resTambahProgram.Headers.Location
$programId = $null
if ($locProgram -match '/Pelatihan/DetailProgram/([a-fA-F0-9-]+)') {
    $programId = $matches[1]
}

# 2.4 Detail Program & Daftarkan Peserta
Report-Test "Dapatkan ID Program Pelatihan Baru" ($programId -ne $null, "ID: $programId")
$detailProgram = Invoke-WebRequest -Uri "$BaseUrl/Pelatihan/DetailProgram/$programId" -WebSession $sessionHrd -UseBasicParsing
Report-Test "Akses Detail Program Pelatihan (HTTP 200)" ($detailProgram.StatusCode -eq 200 -and $detailProgram.Content -match "Advanced Cloud & DevOps Engineering")

# Ambil KaryawanId Rian Pratama dari dropdown
$karyawanId = $null
if ($detailProgram.Content -match '<option value="([a-fA-F0-9-]+)">[^<]*KRY001') {
    $karyawanId = $matches[1]
}
Report-Test "Dapatkan KaryawanId Rian Pratama" ($karyawanId -ne $null, "KaryawanId: $karyawanId")

# 2.5 Daftarkan Peserta
$tokenDaftar = Dapatkan-Token $detailProgram.Content
$bodyDaftarPeserta = @{
    "__RequestVerificationToken" = $tokenDaftar
    "ProgramPelatihanId" = $programId
    "KaryawanId" = $karyawanId
    "Catatan" = "Ditugaskan mengikuti sertifikasi DevOps."
}
$resDaftarPeserta = Invoke-WebRequest -Uri "$BaseUrl/Pelatihan/DaftarPeserta" -Method Post -Body $bodyDaftarPeserta -WebSession $sessionHrd -MaximumRedirection 0 -UseBasicParsing -ErrorAction SilentlyContinue
Report-Test "Daftarkan Karyawan ke Program Pelatihan (302 Redirect)" ($resDaftarPeserta.StatusCode -eq 302)

# 2.6 Evaluasi Kelulusan Peserta & Terbitkan Sertifikat
$detailAfterEnroll = Invoke-WebRequest -Uri "$BaseUrl/Pelatihan/DetailProgram/$programId" -WebSession $sessionHrd -UseBasicParsing
$pesertaId = $null
if ($detailAfterEnroll.Content -match "bukaModalNilai\('([a-fA-F0-9-]+)'") {
    $pesertaId = $matches[1]
}
Report-Test "Dapatkan PesertaId Terdaftar" ($pesertaId -ne $null, "PesertaId: $pesertaId")

$tokenNilai = Dapatkan-Token $detailAfterEnroll.Content
$bodyUpdateNilai = @{
    "__RequestVerificationToken" = $tokenNilai
    "programId" = $programId
    "PesertaId" = $pesertaId
    "StatusKeikutsertaan" = "3" # Lulus
    "NilaiAkhir" = "95.5"
    "Catatan" = "Lulus dengan predikat sangat memuaskan."
}
$resUpdateNilai = Invoke-WebRequest -Uri "$BaseUrl/Pelatihan/PerbaruiStatusPeserta" -Method Post -Body $bodyUpdateNilai -WebSession $sessionHrd -MaximumRedirection 0 -UseBasicParsing -ErrorAction SilentlyContinue
Report-Test "Evaluasi Peserta Dinyatakan Lulus (302 Redirect)" ($resUpdateNilai.StatusCode -eq 302)

# 2.7 Akses Lembar Sertifikat
$detailAfterPass = Invoke-WebRequest -Uri "$BaseUrl/Pelatihan/DetailProgram/$programId" -WebSession $sessionHrd -UseBasicParsing
$sertifikatId = $null
if ($detailAfterPass.Content -match '/Pelatihan/Sertifikat/([a-fA-F0-9-]+)') {
    $sertifikatId = $matches[1]
}
Report-Test "Sertifikat Kelulusan Resmi Otomatis Terbit" ($sertifikatId -ne $null, "SertifikatId: $sertifikatId")

if ($sertifikatId) {
    $resSertifikat = Invoke-WebRequest -Uri "$BaseUrl/Pelatihan/Sertifikat/$sertifikatId" -WebSession $sessionHrd -UseBasicParsing
    Report-Test "Tampilan Lembar Sertifikat Resmi Printable (HTTP 200)" ($resSertifikat.StatusCode -eq 200 -and $resSertifikat.Content -match "SERTIFIKAT KELULUSAN" -and $resSertifikat.Content -match "KODE VALIDASI")
}

# -------------------------------------------------------------
# 3. PENGUJIAN MODUL MANAJEMEN ASET
# -------------------------------------------------------------
Write-Host "`n>>> [3/6] PENGUJIAN MODUL MANAJEMEN ASET" -ForegroundColor Yellow

# 3.1 Dashboard Aset
$dashAset = Invoke-WebRequest -Uri "$BaseUrl/Aset" -WebSession $sessionHrd -UseBasicParsing
Report-Test "Akses Dashboard Inventaris Aset (HTTP 200)" ($dashAset.StatusCode -eq 200 -and $dashAset.Content -match "Pusat Inventaris & Manajemen Aset")

# 3.2 Daftar Aset
$daftarAset = Invoke-WebRequest -Uri "$BaseUrl/Aset/Daftar" -WebSession $sessionHrd -UseBasicParsing
Report-Test "Akses Daftar Inventaris Aset (HTTP 200)" ($daftarAset.StatusCode -eq 200 -and $daftarAset.Content -match "Daftar Inventaris Aset")

# 3.3 Tambah Aset Baru
$tokenAset = Dapatkan-Token $sessionHrd "$BaseUrl/Aset/TambahAset"
$nomorAsetBaru = "AST-TEST-" + (Get-Random -Minimum 1000 -Maximum 9999)
$bodyTambahAset = @{
    "__RequestVerificationToken" = $tokenAset
    "NomorAset" = $nomorAsetBaru
    "NamaAset" = "Apple MacBook Pro M3 Max 16 Inch"
    "Kategori" = "ElektronikLaptop"
    "MerkModel" = "Apple MacBook Pro 16"
    "NomorSeri" = "C02XYZ123ABC"
    "TanggalPembelian" = (Get-Date).ToString("yyyy-MM-dd")
    "NilaiPerolehan" = "45000000"
    "Kondisi" = "SangatBaik"
    "Status" = "Tersedia"
    "LokasiPenyimpanan" = "Gudang IT VIP Lt. 15"
    "DeskripsiSpesifikasi" = "M3 Max 16-Core CPU, 40-Core GPU, 64GB Unified Memory, 1TB SSD Space Black."
}

$resTambahAset = Invoke-WebRequest -Uri "$BaseUrl/Aset/TambahAset" -Method Post -Body $bodyTambahAset -WebSession $sessionHrd -MaximumRedirection 0 -UseBasicParsing -ErrorAction SilentlyContinue
Report-Test "Tambah Aset Inventaris Baru (302 Redirect)" ($resTambahAset.StatusCode -eq 302)

$locAset = $resTambahAset.Headers.Location
$asetId = $null
if ($locAset -match '/Aset/DetailAset/([a-fA-F0-9-]+)') {
    $asetId = $matches[1]
}
Report-Test "Dapatkan ID Aset Inventaris Baru" ($asetId -ne $null, "AsetId: $asetId")

# 3.4 Detail Aset
$detailAset = Invoke-WebRequest -Uri "$BaseUrl/Aset/DetailAset/$asetId" -WebSession $sessionHrd -UseBasicParsing
Report-Test "Akses Detail Aset (HTTP 200)" ($detailAset.StatusCode -eq 200 -and $detailAset.Content -match "Apple MacBook Pro M3 Max")

# 3.5 Serahkan Aset ke Karyawan (BAST)
$tokenSerahkan = Dapatkan-Token $detailAset.Content
$nomorBast = "BAST-" + (Get-Date).ToString("yyyyMMdd") + "-" + (Get-Random -Minimum 100 -Maximum 999)
$bodySerahkan = @{
    "__RequestVerificationToken" = $tokenSerahkan
    "AsetPerusahaanId" = $asetId
    "KaryawanId" = $karyawanId
    "NomorDokumen" = $nomorBast
    "TanggalPenyerahan" = (Get-Date).ToString("yyyy-MM-dd")
    "KondisiSaatPenyerahan" = "SangatBaik"
    "KelengkapanAset" = "Unit MacBook, MagSafe Charger 140W, Sleeve Bag"
    "CatatanPenyerahan" = "Penyerahan fasilitas laptop operasional high performance."
}
$resSerahkan = Invoke-WebRequest -Uri "$BaseUrl/Aset/SerahkanAset" -Method Post -Body $bodySerahkan -WebSession $sessionHrd -MaximumRedirection 0 -UseBasicParsing -ErrorAction SilentlyContinue
Report-Test "Serahkan Aset ke Karyawan BAST (302 Redirect)" ($resSerahkan.StatusCode -eq 302)

# Verifikasi status berubah menjadi 'Digunakan'
$detailAfterSerah = Invoke-WebRequest -Uri "$BaseUrl/Aset/DetailAset/$asetId" -WebSession $sessionHrd -UseBasicParsing
Report-Test "Status Aset Terkini 'Digunakan'" ($detailAfterSerah.Content -match "Aset Sedang Dipinjamkan" -and $detailAfterSerah.Content -match $nomorBast)

# 3.6 Kembalikan Aset ke Gudang
$penyerahanId = $null
if ($detailAfterSerah.Content -match 'name="PenyerahanAsetId"\s+value="([a-fA-F0-9-]+)"') {
    $penyerahanId = $matches[1]
}
Report-Test "Dapatkan PenyerahanAsetId Aktif" ($penyerahanId -ne $null, "PenyerahanId: $penyerahanId")

$tokenKembali = Dapatkan-Token $detailAfterSerah.Content
$bodyKembali = @{
    "__RequestVerificationToken" = $tokenKembali
    "PenyerahanAsetId" = $penyerahanId
    "AsetPerusahaanId" = $asetId
    "TanggalRealisasiPengembalian" = (Get-Date).ToString("yyyy-MM-dd")
    "KondisiSaatPengembalian" = "Baik"
    "StatusPeminjaman" = "Dikembalikan"
    "CatatanPengembalian" = "Dikembalikan lengkap dalam kondisi prima tanpa kerusakan."
}
$resKembali = Invoke-WebRequest -Uri "$BaseUrl/Aset/KembalikanAset" -Method Post -Body $bodyKembali -WebSession $sessionHrd -MaximumRedirection 0 -UseBasicParsing -ErrorAction SilentlyContinue
Report-Test "Kembalikan Aset ke Gudang (302 Redirect)" ($resKembali.StatusCode -eq 302)

# Verifikasi status kembali menjadi 'Tersedia di Gudang'
$detailAfterKembali = Invoke-WebRequest -Uri "$BaseUrl/Aset/DetailAset/$asetId" -WebSession $sessionHrd -UseBasicParsing
Report-Test "Status Aset Terkini Kembali 'Tersedia di Gudang'" ($detailAfterKembali.Content -match "Aset Berada di Gudang")

# -------------------------------------------------------------
# 4. PENGUJIAN PUSAT LAPORAN & EKSPOR CSV
# -------------------------------------------------------------
Write-Host "`n>>> [4/6] PENGUJIAN PUSAT LAPORAN & EKSPOR EXCEL/CSV" -ForegroundColor Yellow

# 4.1 Hub Laporan
$hubLaporan = Invoke-WebRequest -Uri "$BaseUrl/Laporan" -WebSession $sessionHrd -UseBasicParsing
Report-Test "Akses Pusat Laporan Eksekutif (HTTP 200)" ($hubLaporan.StatusCode -eq 200 -and $hubLaporan.Content -match "Pusat Laporan & Analitik Eksekutif")

# 4.2 Laporan Karyawan & Ekspor CSV
$lapKaryawan = Invoke-WebRequest -Uri "$BaseUrl/Laporan/Karyawan" -WebSession $sessionHrd -UseBasicParsing
Report-Test "Akses Laporan Data Karyawan (HTTP 200)" ($lapKaryawan.StatusCode -eq 200 -and $lapKaryawan.Content -match "Laporan Data & Master Karyawan")

$csvKaryawan = Invoke-WebRequest -Uri "$BaseUrl/Laporan/EksporKaryawan" -WebSession $sessionHrd -UseBasicParsing
Report-Test "Ekspor CSV Data Karyawan (text/csv & Valid Header)" ($csvKaryawan.Headers["Content-Type"] -match "text/csv" -and $csvKaryawan.Content -match "NIK,Nama Lengkap,Email")

# 4.3 Laporan Departemen
$lapDept = Invoke-WebRequest -Uri "$BaseUrl/Laporan/Departemen" -WebSession $sessionHrd -UseBasicParsing
Report-Test "Akses Laporan Distribusi Departemen (HTTP 200)" ($lapDept.StatusCode -eq 200 -and $lapDept.Content -match "Distribusi Organisasi")

# 4.4 Laporan Kehadiran & Ekspor CSV
$lapKehadiran = Invoke-WebRequest -Uri "$BaseUrl/Laporan/Kehadiran" -WebSession $sessionHrd -UseBasicParsing
Report-Test "Akses Laporan Kehadiran (HTTP 200)" ($lapKehadiran.StatusCode -eq 200 -and $lapKehadiran.Content -match "Laporan Rekapitulasi Presensi")

$csvKehadiran = Invoke-WebRequest -Uri "$BaseUrl/Laporan/EksporKehadiran" -WebSession $sessionHrd -UseBasicParsing
Report-Test "Ekspor CSV Kehadiran (text/csv & Valid Header)" ($csvKehadiran.Headers["Content-Type"] -match "text/csv" -and $csvKehadiran.Content -match "Tanggal,NIK,Nama Karyawan")

# 4.5 Laporan Keterlambatan & Lembur
$lapTelat = Invoke-WebRequest -Uri "$BaseUrl/Laporan/Keterlambatan" -WebSession $sessionHrd -UseBasicParsing
Report-Test "Akses Laporan Keterlambatan (HTTP 200)" ($lapTelat.StatusCode -eq 200 -and $lapTelat.Content -match "Analisis Keterlambatan")

$lapLembur = Invoke-WebRequest -Uri "$BaseUrl/Laporan/Lembur" -WebSession $sessionHrd -UseBasicParsing
Report-Test "Akses Laporan Lembur (HTTP 200)" ($lapLembur.StatusCode -eq 200 -and $lapLembur.Content -match "Akumulasi Kerja Lembur")

# 4.6 Laporan Cuti
$lapCuti = Invoke-WebRequest -Uri "$BaseUrl/Laporan/Cuti" -WebSession $sessionHrd -UseBasicParsing
Report-Test "Akses Laporan Penggunaan Cuti (HTTP 200)" ($lapCuti.StatusCode -eq 200 -and $lapCuti.Content -match "Penggunaan Cuti Karyawan")

# 4.7 Laporan Penggajian & Ekspor CSV
$lapGaji = Invoke-WebRequest -Uri "$BaseUrl/Laporan/Penggajian" -WebSession $sessionHrd -UseBasicParsing
Report-Test "Akses Laporan Penggajian / Payroll (HTTP 200)" ($lapGaji.StatusCode -eq 200 -and $lapGaji.Content -match "Kompensasi & Penggajian")

$csvGaji = Invoke-WebRequest -Uri "$BaseUrl/Laporan/EksporPenggajian" -WebSession $sessionHrd -UseBasicParsing
Report-Test "Ekspor CSV Penggajian (text/csv & Valid Header)" ($csvGaji.Headers["Content-Type"] -match "text/csv" -and $csvGaji.Content -match "Periode,NIK,Nama Karyawan")

# 4.8 Laporan Rekrutmen, Kinerja, Pelatihan & Aset
$lapRekrutmen = Invoke-WebRequest -Uri "$BaseUrl/Laporan/Rekrutmen" -WebSession $sessionHrd -UseBasicParsing
Report-Test "Akses Laporan Rekrutmen (HTTP 200)" ($lapRekrutmen.StatusCode -eq 200 -and $lapRekrutmen.Content -match "Pengadaan SDM & Rekrutmen")

$lapKinerja = Invoke-WebRequest -Uri "$BaseUrl/Laporan/Kinerja" -WebSession $sessionHrd -UseBasicParsing
Report-Test "Akses Laporan Evaluasi Kinerja KPI (HTTP 200)" ($lapKinerja.StatusCode -eq 200 -and $lapKinerja.Content -match "Penilaian Kinerja Karyawan")

$lapPelatihan = Invoke-WebRequest -Uri "$BaseUrl/Laporan/Pelatihan" -WebSession $sessionHrd -UseBasicParsing
Report-Test "Akses Laporan Pelatihan (HTTP 200)" ($lapPelatihan.StatusCode -eq 200 -and $lapPelatihan.Content -match "Program Pelatihan & Pengembangan")

$lapAset = Invoke-WebRequest -Uri "$BaseUrl/Laporan/Aset" -WebSession $sessionHrd -UseBasicParsing
Report-Test "Akses Laporan Inventaris Aset (HTTP 200)" ($lapAset.StatusCode -eq 200 -and $lapAset.Content -match "Inventaris & Nilai Aset")

$csvAset = Invoke-WebRequest -Uri "$BaseUrl/Laporan/EksporAset" -WebSession $sessionHrd -UseBasicParsing
Report-Test "Ekspor CSV Aset (text/csv & Valid Header)" ($csvAset.Headers["Content-Type"] -match "text/csv" -and $csvAset.Content -match "Nomor Aset,Nama Aset,Kategori")

# -------------------------------------------------------------
# 5. PENGUJIAN EMPLOYEE SELF SERVICE (ESS)
# -------------------------------------------------------------
Write-Host "`n>>> [5/6] PENGUJIAN EMPLOYEE SELF SERVICE (ESS)" -ForegroundColor Yellow
$sessionKaryawan = Login-Account "karyawan@solusihr.id" "Karyawan#2026!"
Report-Test "Login Karyawan Rian Pratama Berhasil" ($sessionKaryawan -ne $null)

$essPelatihan = Invoke-WebRequest -Uri "$BaseUrl/Pelatihan/RiwayatSaya" -WebSession $sessionKaryawan -UseBasicParsing
Report-Test "Akses ESS Riwayat Pelatihan & Sertifikat Saya (HTTP 200)" ($essPelatihan.StatusCode -eq 200 -and $essPelatihan.Content -match "Riwayat Pelatihan & Sertifikasi Saya")

$essAset = Invoke-WebRequest -Uri "$BaseUrl/Aset/RiwayatSaya" -WebSession $sessionKaryawan -UseBasicParsing
Report-Test "Akses ESS Aset Pegangan Saya (HTTP 200)" ($essAset.StatusCode -eq 200 -and $essAset.Content -match "Aset Inventaris Yang Ditugaskan")

# -------------------------------------------------------------
# 6. PENGUJIAN ISOLASI MULTI-TENANT (TENANT 2)
# -------------------------------------------------------------
Write-Host "`n>>> [6/6] PENGUJIAN ISOLASI MULTI-TENANT" -ForegroundColor Yellow
$sessionHrd2 = Login-Account "hrd2@majubersama.id" "Hrd2#2026!"
Report-Test "Login HRD Tenant 2 (PT Maju Bersama Digital) Berhasil" ($sessionHrd2 -ne $null)

$tenant2Program = Invoke-WebRequest -Uri "$BaseUrl/Pelatihan/Program" -WebSession $sessionHrd2 -UseBasicParsing
Report-Test "Isolasi Program: Tenant 2 Tidak Dapat Melihat Program Tenant 1" ($tenant2Program.Content -notmatch "Advanced Cloud & DevOps Engineering" -and $tenant2Program.Content -notmatch "TRN-2026-LDR01")

$tenant2Aset = Invoke-WebRequest -Uri "$BaseUrl/Aset/Daftar" -WebSession $sessionHrd2 -UseBasicParsing
Report-Test "Isolasi Aset: Tenant 2 Tidak Dapat Melihat Aset Tenant 1" ($tenant2Aset.Content -notmatch "AST-2026-NB001" -and $tenant2Aset.Content -notmatch $nomorAsetBaru)

# Coba akses URL langsung detail program Tenant 1 dari Tenant 2
$resTamper = Invoke-WebRequest -Uri "$BaseUrl/Pelatihan/DetailProgram/$programId" -WebSession $sessionHrd2 -MaximumRedirection 0 -UseBasicParsing -ErrorAction SilentlyContinue
Report-Test "Manipulasi URL: Akses Lintas Perusahaan Ditolak / Dialihkan" ($resTamper.StatusCode -eq 302 -or $resTamper.StatusCode -eq 404 -or $resTamper.StatusCode -eq 403)

# -------------------------------------------------------------
# REKAPITULASI AKHIR
# -------------------------------------------------------------
Write-Host "`n==========================================================" -ForegroundColor Cyan
Write-Host "                REKAPITULASI PENGUJIAN PHASE 6            " -ForegroundColor Cyan
Write-Host "==========================================================" -ForegroundColor Cyan
Write-Host " TOTAL PENGUJIAN: $($passedCount + $failedCount)"
Write-Host " BERHASIL       : $passedCount" -ForegroundColor Green
Write-Host " GAGAL          : $failedCount" -ForegroundColor $(if ($failedCount -eq 0) { "Green" } else { "Red" })

if ($failedCount -eq 0) {
    Write-Host "`n>>> SELURUH FITUR PHASE 6 DINYATAKAN LULUS 100%! <<<`n" -ForegroundColor Green
} else {
    Write-Host "`n>>> TERDAPAT PENGUJIAN YANG GAGAL. SILAKAN PERIKSA DETAIL DI ATAS. <<<`n" -ForegroundColor Red
}
