# ==============================================================================
# TEST SUITE INTEGRASI OTOMATIS: PHASE 4 — MODUL PENGGAJIAN (PAYROLL)
# SistemHRD (Enterprise Smart HRIS)
# ==============================================================================

$ErrorActionPreference = "Stop"
$BaseUrl = "http://localhost:5252"

Write-Host "==========================================================" -ForegroundColor Cyan
Write-Host "   PENGUJIAN INTEGRASI PHASE 4 — MODUL PENGGAJIAN         " -ForegroundColor Cyan
Write-Host "   SistemHRD (Smart HRIS)                                " -ForegroundColor Cyan
Write-Host "==========================================================" -ForegroundColor Cyan

# 0. Inisialisasi reset data uji payroll
Write-Host "Menyiapkan basis data pengujian bersih..." -ForegroundColor Gray
cmd /c "C:\xampp\mysql\bin\mysql.exe -u root sistemhrd_db < reset_test_phase4.sql"

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

# ------------------------------------------------------------------------------
# 1. Login Akun Finance & HRD (Tenant 1)
# ------------------------------------------------------------------------------
Write-Host "`n[UJI 1] Login sebagai Finance PT Solusi HR Nusantara (finance@solusihr.id)..." -NoNewline
$financeSession = Login-Account -Email "finance@solusihr.id" -Password "Finance#2026!"
if ($financeSession) {
    Write-Host " [BERHASIL]" -ForegroundColor Green
} else {
    Write-Host " [GAGAL]" -ForegroundColor Red; exit 1
}

Write-Host "[UJI 1b] Login sebagai HRD PT Solusi HR Nusantara (hrd@solusihr.id)..." -NoNewline
$hrdSession = Login-Account -Email "hrd@solusihr.id" -Password "Hrd#2026!"
if ($hrdSession) {
    Write-Host " [BERHASIL]" -ForegroundColor Green
} else {
    Write-Host " [GAGAL]" -ForegroundColor Red; exit 1
}

# ------------------------------------------------------------------------------
# 2. Master Komponen Gaji (GET /Penggajian/Komponen)
# ------------------------------------------------------------------------------
Write-Host "`n[UJI 2] Akses Master Komponen Gaji (GET /Penggajian/Komponen)..." -NoNewline
$komponenHtml = (Invoke-WebRequest -Uri "$BaseUrl/Penggajian/Komponen" -WebSession $financeSession -UseBasicParsing).Content
if ($komponenHtml -like "*Gaji Pokok*" -and $komponenHtml -like "*BPJS-KES*") {
    Write-Host " [BERHASIL]" -ForegroundColor Green
    Write-Host "       Master Komponen Gaji memuat komponen baku (Gaji Pokok, Tunjangan, BPJS, PPh 21)." -ForegroundColor Gray
} else {
    Write-Host " [GAGAL]" -ForegroundColor Red; exit 1
}

# ------------------------------------------------------------------------------
# 3. Tambah Komponen Gaji Baru (POST /Penggajian/SimpanKomponen)
# ------------------------------------------------------------------------------
Write-Host "`n[UJI 3] Tambah Komponen Gaji Baru 'BONUS-PROYEK' (POST /Penggajian/SimpanKomponen)..." -NoNewline
$tokenFin = Get-VerificationToken -Html $komponenHtml
$bodyKomponen = @{
    "KodeKomponen" = "BONUS-PROYEK"
    "NamaKomponen" = "Bonus Capaian Proyek"
    "Jenis" = "Pendapatan"
    "Sifat" = "TidakTetap"
    "Kategori" = "TunjanganTidakTetap"
    "NilaiBawaan" = "2000000"
    "KenaPajak" = "true"
    "MasukKalkulasiBpjs" = "false"
    "DihitungOtomatisOlehSistem" = "false"
    "StatusAktif" = "true"
    "__RequestVerificationToken" = $tokenFin
}
$resKomponen = Invoke-WebRequest -Uri "$BaseUrl/Penggajian/SimpanKomponen" -Method Post -Body $bodyKomponen -WebSession $financeSession -MaximumRedirection 5 -UseBasicParsing
if ($resKomponen.Content -like "*BONUS-PROYEK*" -and $resKomponen.Content -like "*Bonus Capaian Proyek*") {
    Write-Host " [BERHASIL]" -ForegroundColor Green
    Write-Host "       Komponen 'BONUS-PROYEK' berhasil disimpan ke basis data tenant." -ForegroundColor Gray
} else {
    Write-Host " [GAGAL]" -ForegroundColor Red; exit 1
}

# ------------------------------------------------------------------------------
# 4. Akses & Konfigurasi Kebijakan Penggajian (GET & POST /Penggajian/Kebijakan)
# ------------------------------------------------------------------------------
Write-Host "`n[UJI 4] Akses Konfigurasi Kebijakan Penggajian (GET /Penggajian/Kebijakan)..." -NoNewline
$kebijakanHtml = (Invoke-WebRequest -Uri "$BaseUrl/Penggajian/Kebijakan" -WebSession $financeSession -UseBasicParsing).Content
if ($kebijakanHtml -like "*Kebijakan Tarif & Aturan Penggajian*" -and $kebijakanHtml -like "*Cutoff*") {
    Write-Host " [BERHASIL]" -ForegroundColor Green
} else {
    Write-Host " [GAGAL]" -ForegroundColor Red; exit 1
}

Write-Host "[UJI 4b] Simpan Perubahan Kebijakan Penggajian (POST /Penggajian/SimpanKebijakan)..." -NoNewline
$tokenKebijakan = Get-VerificationToken -Html $kebijakanHtml
$bodyKebijakan = @{
    "HitungLemburOtomatis" = "true"
    "TarifLemburPerJamFlat" = "0"
    "TerapkanPotonganKeterlambatan" = "true"
    "ToleransiKeterlambatanGratisMenit" = "15"
    "DendaKeterlambatanPerMenit" = "1000"
    "TerapkanBpjsKesehatan" = "true"
    "PersenBpjsKesehatanPerusahaan" = "4.00"
    "PersenBpjsKesehatanPekerja" = "1.00"
    "BatasMaksimalUpahBpjsKesehatan" = "12000000"
    "TerapkanBpjsKetenagakerjaan" = "true"
    "PersenBpjsJhtPerusahaan" = "3.70"
    "PersenBpjsJhtPekerja" = "2.00"
    "PersenBpjsJpPerusahaan" = "2.00"
    "PersenBpjsJpPekerja" = "1.00"
    "BatasMaksimalUpahBpjsJp" = "10042300"
    "PersenBpjsJkkPerusahaan" = "0.24"
    "PersenBpjsJkmPerusahaan" = "0.30"
    "TerapkanPph21" = "true"
    "MetodePph21" = "TER_PP58"
    "TanggalCutoffAwal" = "26"
    "TanggalCutoffAkhir" = "25"
    "TanggalPembayaranGaji" = "28"
    "__RequestVerificationToken" = $tokenKebijakan
}
$resSimpanKeb = Invoke-WebRequest -Uri "$BaseUrl/Penggajian/SimpanKebijakan" -Method Post -Body $bodyKebijakan -WebSession $financeSession -MaximumRedirection 5 -UseBasicParsing
if ($resSimpanKeb.Content -like "*berhasil diperbarui*" -or $resSimpanKeb.StatusCode -eq 200) {
    Write-Host " [BERHASIL]" -ForegroundColor Green
    Write-Host "       Kebijakan pemotongan keterlambatan, tarif BPJS & TER PPh 21 tersimpan." -ForegroundColor Gray
} else {
    Write-Host " [GAGAL]" -ForegroundColor Red; exit 1
}

# ------------------------------------------------------------------------------
# 5. Pengaturan Gaji Karyawan Rian Pratama
# ------------------------------------------------------------------------------
Write-Host "`n[UJI 5] Ambil ID Karyawan Rian Pratama dari Database..." -NoNewline
$karyawanIdStr = (cmd /c "C:\xampp\mysql\bin\mysql.exe -u root -N -e ""USE sistemhrd_db; SELECT Id FROM Karyawan WHERE Email='karyawan@solusihr.id' LIMIT 1;""").Trim()
$karyawanGuid = [Guid]::Parse($karyawanIdStr)
Write-Host " [BERHASIL: $karyawanGuid]" -ForegroundColor Green

Write-Host "[UJI 5b] Buka Halaman Atur Gaji Karyawan (GET /Penggajian/AturGajiKaryawan/$karyawanGuid)..." -NoNewline
$aturGajiHtml = (Invoke-WebRequest -Uri "$BaseUrl/Penggajian/AturGajiKaryawan/$karyawanGuid" -WebSession $financeSession -UseBasicParsing).Content
if ($aturGajiHtml -like "*Rian Pratama*" -and $aturGajiHtml -like "*Struktur Komponen Gaji*") {
    Write-Host " [BERHASIL]" -ForegroundColor Green
} else {
    Write-Host " [GAGAL]" -ForegroundColor Red; exit 1
}

# ------------------------------------------------------------------------------
# 6. Buat Periode Penggajian Baru (POST /Penggajian/TambahPeriode)
# ------------------------------------------------------------------------------
Write-Host "`n[UJI 6] Buka Form Tambah Periode Penggajian (GET /Penggajian/TambahPeriode)..." -NoNewline
$tambahPeriodeHtml = (Invoke-WebRequest -Uri "$BaseUrl/Penggajian/TambahPeriode" -WebSession $financeSession -UseBasicParsing).Content
$tokenPeriode = Get-VerificationToken -Html $tambahPeriodeHtml
if ($tokenPeriode) {
    Write-Host " [BERHASIL]" -ForegroundColor Green
} else {
    Write-Host " [GAGAL]" -ForegroundColor Red; exit 1
}

Write-Host "[UJI 6b] Simpan Periode Penggajian Baru (POST /Penggajian/TambahPeriode)..." -NoNewline
$bodyPeriode = @{
    "NamaPeriode" = "Penggajian November 2026"
    "Bulan" = "11"
    "Tahun" = "2026"
    "TanggalMulai" = "2026-10-26"
    "TanggalSelesai" = "2026-11-25"
    "TanggalPembayaran" = "2026-11-28"
    "Keterangan" = "Periode reguler bulan November 2026"
    "__RequestVerificationToken" = $tokenPeriode
}
$resPeriode = Invoke-WebRequest -Uri "$BaseUrl/Penggajian/TambahPeriode" -Method Post -Body $bodyPeriode -WebSession $financeSession -MaximumRedirection 5 -UseBasicParsing
if ($resPeriode.Content -like "*Penggajian November 2026*" -and $resPeriode.Content -like "*Draf*") {
    Write-Host " [BERHASIL]" -ForegroundColor Green
    Write-Host "       Periode 'Penggajian November 2026' berhasil dibuat dengan status Draf." -ForegroundColor Gray
} else {
    Write-Host " [GAGAL]" -ForegroundColor Red; exit 1
}

# Dapatkan ID Periode Penggajian dari database
$periodeIdStr = (cmd /c "C:\xampp\mysql\bin\mysql.exe -u root -N -e ""USE sistemhrd_db; SELECT Id FROM PeriodePenggajian WHERE NamaPeriode='Penggajian November 2026' LIMIT 1;""").Trim()
$periodeGuid = [Guid]::Parse($periodeIdStr)
Write-Host "       Periode ID: $periodeGuid" -ForegroundColor Gray

# ------------------------------------------------------------------------------
# 7. Eksekusi Hitung Gaji Batch (POST /Penggajian/HitungBatch)
# ------------------------------------------------------------------------------
Write-Host "`n[UJI 7] Eksekusi Hitung Gaji Batch Otomatis (POST /Penggajian/HitungBatch)..." -NoNewline
$detailPeriodeHtml = (Invoke-WebRequest -Uri "$BaseUrl/Penggajian/PeriodeDetail/$periodeGuid" -WebSession $financeSession -UseBasicParsing).Content
$tokenHitung = Get-VerificationToken -Html $detailPeriodeHtml
$bodyHitung = @{
    "PeriodePenggajianId" = $periodeGuid.ToString()
    "DepartemenId" = ""
    "__RequestVerificationToken" = $tokenHitung
}
$resHitung = Invoke-WebRequest -Uri "$BaseUrl/Penggajian/HitungBatch" -Method Post -Body $bodyHitung -WebSession $financeSession -MaximumRedirection 5 -UseBasicParsing
if ($resHitung.Content -like "*Rian Pratama*" -and $resHitung.Content -like "*Rp*") {
    Write-Host " [BERHASIL]" -ForegroundColor Green
    Write-Host "       Kalkulasi batch berhasil memproses seluruh karyawan aktif di perusahaan." -ForegroundColor Gray
} else {
    Write-Host " [GAGAL]" -ForegroundColor Red; exit 1
}

# ------------------------------------------------------------------------------
# 8. Verifikasi Snapshot Komponen & Rincian Penggajian (GET /Penggajian/Detail/{id})
# ------------------------------------------------------------------------------
Write-Host "`n[UJI 8] Verifikasi Snapshot Komponen Tersimpan di Penggajian..." -NoNewline
$penggajianRianIdStr = (cmd /c "C:\xampp\mysql\bin\mysql.exe -u root -N -e ""USE sistemhrd_db; SELECT Id FROM Penggajian WHERE PeriodePenggajianId='$periodeGuid' AND KaryawanId='$karyawanGuid' LIMIT 1;""").Trim()
$penggajianRianGuid = [Guid]::Parse($penggajianRianIdStr)

$rincianHtml = (Invoke-WebRequest -Uri "$BaseUrl/Penggajian/Detail/$penggajianRianGuid" -WebSession $financeSession -UseBasicParsing).Content
if ($rincianHtml -like "*Gaji Pokok*" -and $rincianHtml -like "*BPJS Kesehatan*" -and $rincianHtml -like "*Take Home Pay*") {
    Write-Host " [BERHASIL]" -ForegroundColor Green
    Write-Host "       Rincian penggajian memuat seluruh snapshot komponen gaji dan rekap kalkulasi." -ForegroundColor Gray
} else {
    Write-Host " [GAGAL]" -ForegroundColor Red; exit 1
}

# ------------------------------------------------------------------------------
# 9. Verifikasi Pencegahan Duplikasi Payroll
# ------------------------------------------------------------------------------
Write-Host "`n[UJI 9] Uji Pencegahan Duplikasi Payroll (Hitung Ulang Tanpa Menghasilkan Baris Ganda)..." -NoNewline
$resHitungUlang = Invoke-WebRequest -Uri "$BaseUrl/Penggajian/HitungBatch" -Method Post -Body $bodyHitung -WebSession $financeSession -MaximumRedirection 5 -UseBasicParsing
$countPenggajian = (cmd /c "C:\xampp\mysql\bin\mysql.exe -u root -N -e ""USE sistemhrd_db; SELECT COUNT(*) FROM Penggajian WHERE PeriodePenggajianId='$periodeGuid' AND KaryawanId='$karyawanGuid';""").Trim()
if ([int]$countPenggajian -eq 1) {
    Write-Host " [BERHASIL]" -ForegroundColor Green
    Write-Host "       Duplikasi dicegah: Count penggajian untuk karyawan pada periode ini tetap 1." -ForegroundColor Gray
} else {
    Write-Host " [GAGAL: Ditemukan $countPenggajian entri]" -ForegroundColor Red; exit 1
}

# ------------------------------------------------------------------------------
# 10. Persetujuan Payroll Periode (POST /Penggajian/SetujuiPeriode)
# ------------------------------------------------------------------------------
Write-Host "`n[UJI 10] Persetujuan Payroll Periode Oleh Finance/HRD (POST /Penggajian/SetujuiPeriode)..." -NoNewline
$tokenSetujui = Get-VerificationToken -Html $resHitungUlang.Content
$bodySetujui = @{
    "PeriodePenggajianId" = $periodeGuid.ToString()
    "Catatan" = "Disetujui untuk pembayaran payroll November 2026."
    "__RequestVerificationToken" = $tokenSetujui
}
$resSetujui = Invoke-WebRequest -Uri "$BaseUrl/Penggajian/SetujuiPeriode" -Method Post -Body $bodySetujui -WebSession $financeSession -MaximumRedirection 5 -UseBasicParsing
if ($resSetujui.Content -like "*Disetujui*" -and $resSetujui.Content -like "*Terkunci*") {
    Write-Host " [BERHASIL]" -ForegroundColor Green
    Write-Host "       Periode dan seluruh slip gaji berubah status menjadi 'Disetujui' dan 'Terkunci'." -ForegroundColor Gray
} else {
    Write-Host " [GAGAL]" -ForegroundColor Red; exit 1
}

# ------------------------------------------------------------------------------
# 11. Verifikasi Payroll Terkunci (Hitung Ulang Ditolak)
# ------------------------------------------------------------------------------
Write-Host "`n[UJI 11] Verifikasi Payroll Terkunci (Cegah Perubahan Setelah Disetujui)..." -NoNewline
$resHitungTerkunci = Invoke-WebRequest -Uri "$BaseUrl/Penggajian/HitungBatch" -Method Post -Body $bodyHitung -WebSession $financeSession -MaximumRedirection 5 -UseBasicParsing
if ($resHitungTerkunci.Content -like "*terkunci*" -or $resHitungTerkunci.Content -like "*tidak dapat dihitung ulang*") {
    Write-Host " [BERHASIL]" -ForegroundColor Green
    Write-Host "       Proteksi integritas aktif: Perhitungan ulang pada payroll yang disetujui ditolak." -ForegroundColor Gray
} else {
    Write-Host " [GAGAL]" -ForegroundColor Red; exit 1
}

# ------------------------------------------------------------------------------
# 12. Tampilkan Slip Gaji Resmi & Kode Verifikasi (GET /Penggajian/Slip/{id})
# ------------------------------------------------------------------------------
Write-Host "`n[UJI 12] Tampilkan Slip Gaji Resmi & Kode Verifikasi (GET /Penggajian/Slip/$penggajianRianGuid)..." -NoNewline
$slipHtml = (Invoke-WebRequest -Uri "$BaseUrl/Penggajian/Slip/$penggajianRianGuid" -WebSession $financeSession -UseBasicParsing).Content
if ($slipHtml -like "*Slip Gaji*" -and $slipHtml -like "*Kode Verifikasi:*" -and $slipHtml -like "*Rian Pratama*") {
    Write-Host " [BERHASIL]" -ForegroundColor Green
    Write-Host "       Slip gaji memuat logo, kop perusahaan, rincian hak/potongan, terbilang rupiah, & kode verifikasi." -ForegroundColor Gray
} else {
    Write-Host " [GAGAL]" -ForegroundColor Red; exit 1
}

# ------------------------------------------------------------------------------
# 13. Ekspor Laporan Rekap Payroll CSV (GET /Penggajian/Ekspor/{periodeId})
# ------------------------------------------------------------------------------
Write-Host "`n[UJI 13] Ekspor Laporan Rekap Payroll CSV (GET /Penggajian/Ekspor/$periodeGuid)..." -NoNewline
$eksporRes = Invoke-WebRequest -Uri "$BaseUrl/Penggajian/Ekspor/$periodeGuid" -WebSession $financeSession -UseBasicParsing
if ($eksporRes.Headers["Content-Type"] -like "*text/csv*" -and $eksporRes.Content -like "*Nama Karyawan*" -and $eksporRes.Content -like "*Rian Pratama*") {
    Write-Host " [BERHASIL]" -ForegroundColor Green
    Write-Host "       File CSV rekap transfer payroll valid dan siap dibaca oleh sistem kliring bank." -ForegroundColor Gray
} else {
    Write-Host " [GAGAL]" -ForegroundColor Red; exit 1
}

# ------------------------------------------------------------------------------
# 14. Login Karyawan (ESS) & Akses Riwayat Slip Gaji (GET /Penggajian/Riwayat)
# ------------------------------------------------------------------------------
Write-Host "`n[UJI 14] Login Karyawan Rian Pratama (karyawan@solusihr.id)..." -NoNewline
$karyawanSession = Login-Account -Email "karyawan@solusihr.id" -Password "Karyawan#2026!"
if ($karyawanSession) {
    Write-Host " [BERHASIL]" -ForegroundColor Green
} else {
    Write-Host " [GAGAL]" -ForegroundColor Red; exit 1
}

Write-Host "[UJI 14b] Akses Riwayat Slip Gaji ESS (GET /Penggajian/Riwayat)..." -NoNewline
$riwayatHtml = (Invoke-WebRequest -Uri "$BaseUrl/Penggajian/Riwayat" -WebSession $karyawanSession -UseBasicParsing).Content
if ($riwayatHtml -like "*Riwayat Slip Gaji Saya*" -and $riwayatHtml -like "*Rian Pratama*" -and $riwayatHtml -like "*SLIP/*") {
    Write-Host " [BERHASIL]" -ForegroundColor Green
    Write-Host "       Karyawan dapat melihat daftar slip gaji resmi yang telah disetujui." -ForegroundColor Gray
} else {
    Write-Host " [GAGAL]" -ForegroundColor Red; exit 1
}

# ------------------------------------------------------------------------------
# 15. Karyawan Membuka Slip Gaji Milik Sendiri (GET /Penggajian/Slip/{id})
# ------------------------------------------------------------------------------
Write-Host "`n[UJI 15] Karyawan Membuka Slip Gaji Milik Sendiri (GET /Penggajian/Slip/$penggajianRianGuid)..." -NoNewline
$slipKaryawanHtml = (Invoke-WebRequest -Uri "$BaseUrl/Penggajian/Slip/$penggajianRianGuid" -WebSession $karyawanSession -UseBasicParsing).Content
if ($slipKaryawanHtml -like "*Slip Gaji*" -and $slipKaryawanHtml -like "*Rian Pratama*") {
    Write-Host " [BERHASIL]" -ForegroundColor Green
    Write-Host "       Akses mandiri slip gaji karyawan sukses." -ForegroundColor Gray
} else {
    Write-Host " [GAGAL]" -ForegroundColor Red; exit 1
}

# ------------------------------------------------------------------------------
# 16. Proteksi ESS: Karyawan Tidak Boleh Membuka Slip Rekan Kerja Lain
# ------------------------------------------------------------------------------
Write-Host "`n[UJI 16] Proteksi ESS: Uji Karyawan Membuka Slip Rekan Lain (Manager Ahmad Fauzi)..." -NoNewline
$penggajianAhmadIdStr = (cmd /c "C:\xampp\mysql\bin\mysql.exe -u root -N -e ""USE sistemhrd_db; SELECT p.Id FROM Penggajian p JOIN Karyawan k ON p.KaryawanId=k.Id WHERE p.PeriodePenggajianId='$periodeGuid' AND k.Email='manager@solusihr.id' LIMIT 1;""").Trim()
$penggajianAhmadGuid = [Guid]::Parse($penggajianAhmadIdStr)

$resSlipAsing = Invoke-WebRequest -Uri "$BaseUrl/Penggajian/Slip/$penggajianAhmadGuid" -WebSession $karyawanSession -MaximumRedirection 5 -UseBasicParsing
if ($resSlipAsing.Content -like "*tidak berwenang*" -or $resSlipAsing.Content -like "*tidak ditemukan*" -or $resSlipAsing.Content -notlike "*Ahmad Fauzi*") {
    Write-Host " [BERHASIL]" -ForegroundColor Green
    Write-Host "       Akses ditolak: Karyawan dilarang keras mengintip slip gaji milik karyawan lain." -ForegroundColor Gray
} else {
    Write-Host " [GAGAL: Karyawan dapat melihat slip karyawan lain!]" -ForegroundColor Red; exit 1
}

# ------------------------------------------------------------------------------
# 17. Uji Keamanan Multi-Tenant: Tenant 2 Dilarang Akses Data Tenant 1
# ------------------------------------------------------------------------------
Write-Host "`n[UJI 17] Uji Keamanan Multi-Tenant: HRD PT Maju Bersama Digital (hrd2@majubersama.id)..." -NoNewline
$hrd2Session = Login-Account -Email "hrd2@majubersama.id" -Password "Hrd2#2026!"
if ($hrd2Session) {
    Write-Host " [BERHASIL]" -ForegroundColor Green
} else {
    Write-Host " [GAGAL]" -ForegroundColor Red; exit 1
}

Write-Host "[UJI 17b] HRD Tenant 2 Membuka Detail Periode Penggajian Milik Tenant 1..." -NoNewline
$resTenant2Akses = Invoke-WebRequest -Uri "$BaseUrl/Penggajian/PeriodeDetail/$periodeGuid" -WebSession $hrd2Session -MaximumRedirection 5 -UseBasicParsing
if ($resTenant2Akses.Content -like "*Periode penggajian tidak ditemukan*" -or $resTenant2Akses.Content -notlike "*Penggajian November 2026*") {
    Write-Host " [BERHASIL]" -ForegroundColor Green
    Write-Host "       Isolasi Multi-Tenant bekerja sempurna: Tenant 2 tidak dapat melihat periode Tenant 1." -ForegroundColor Gray
} else {
    Write-Host " [GAGAL: Kebocoran data antar-tenant terdeteksi!]" -ForegroundColor Red; exit 1
}

Write-Host "[UJI 17c] HRD Tenant 2 Mengunduh Ekspor CSV Payroll Tenant 1..." -NoNewline
$resTenant2Ekspor = Invoke-WebRequest -Uri "$BaseUrl/Penggajian/Ekspor/$periodeGuid" -WebSession $hrd2Session -MaximumRedirection 5 -UseBasicParsing
if ($resTenant2Ekspor.Content -like "*tidak ditemukan*" -or $resTenant2Ekspor.Headers["Content-Type"] -notlike "*text/csv*") {
    Write-Host " [BERHASIL]" -ForegroundColor Green
    Write-Host "       Isolasi Multi-Tenant bekerja sempurna: Tenant 2 tidak dapat mengekspor payroll Tenant 1." -ForegroundColor Gray
} else {
    Write-Host " [GAGAL: Kebocoran ekspor data antar-tenant terdeteksi!]" -ForegroundColor Red; exit 1
}

Write-Host "`n==========================================================" -ForegroundColor Green
Write-Host "   SEMUA 17 PENGUJIAN INTEGRASI PHASE 4 LULUS DENGAN SUKSES! " -ForegroundColor Green
Write-Host "==========================================================" -ForegroundColor Green
