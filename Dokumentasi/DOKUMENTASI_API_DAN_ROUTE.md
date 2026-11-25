# DOKUMENTASI ROUTE & ENDPOINT — SISTEMHRD

## 1. Konvensi Routing
* Aplikasi menggunakan pola URL berbasis **Bahasa Indonesia** yang ramah pengguna (human-readable SEO-friendly URLs).
* Setiap Controller memiliki rute dasar yang jelas, misalnya `/Autentikasi`, `/Dasbor`, `/Karyawan`, `/Departemen`, `/Kehadiran`, `/Cuti`, `/Penggajian`.
* Seluruh form mutasi data (POST / PUT / DELETE) dilindungi dengan **Anti-Forgery Token (`@Html.AntiForgeryToken()`)**.
* Otorisasi server-side diterapkan menggunakan atribut `[Authorize(Roles = "...")]`.

---

## 2. Pemetaan Rute Modul Utama

### 2.1 Modul Autentikasi (`AutentikasiController`)
| HTTP Method | Route URL | Hak Akses | Deskripsi |
| :--- | :--- | :--- | :--- |
| `GET` | `/Autentikasi/Masuk` | Publik (Anonim) | Menampilkan halaman login |
| `POST` | `/Autentikasi/Masuk` | Publik (Anonim) | Memproses autentikasi kredensial pengguna |
| `POST` | `/Autentikasi/Keluar` | Terautentikasi | Mengakhiri sesi pengguna & menghapus cookie |
| `GET` | `/Autentikasi/AksesDitolak` | Terautentikasi | Halaman peringatan hak akses tidak mencukupi (403) |

---

### 2.2 Modul Dashboard (`DasborController`)
| HTTP Method | Route URL | Hak Akses | Deskripsi |
| :--- | :--- | :--- | :--- |
| `GET` | `/Dasbor` | Terautentikasi | Halaman utama ringkasan statistik sesuai peran pengguna |
| `GET` | `/Dasbor/RingkasanStatistik` | Terautentikasi | Endpoint data JSON statistik kartu & grafik Chart.js |

---

### 2.3 Modul Perusahaan (`PerusahaanController`)
| HTTP Method | Route URL | Hak Akses | Deskripsi |
| :--- | :--- | :--- | :--- |
| `GET` | `/Perusahaan` | `SuperAdmin` | Daftar seluruh perusahaan terdaftar (multi-tenant) |
| `GET` | `/Perusahaan/Tambah` | `SuperAdmin` | Form pembuatan perusahaan baru |
| `POST` | `/Perusahaan/Tambah` | `SuperAdmin` | Menyimpan perusahaan baru ke basis data |
| `GET` | `/Perusahaan/Ubah/{id}` | `SuperAdmin`, `HRD` | Form pengubahan profil perusahaan |
| `POST` | `/Perusahaan/Ubah/{id}` | `SuperAdmin`, `HRD` | Menyimpan perubahan data perusahaan |

---

### 2.4 Modul Departemen (`DepartemenController`)
| HTTP Method | Route URL | Hak Akses | Deskripsi |
| :--- | :--- | :--- | :--- |
| `GET` | `/Departemen` | `SuperAdmin`, `HRD`, `Manager` | Daftar master departemen per tenant |
| `GET` | `/Departemen/Tambah` | `SuperAdmin`, `HRD` | Form tambah departemen baru |
| `POST` | `/Departemen/Tambah` | `SuperAdmin`, `HRD` | Menyimpan departemen baru dengan validasi kode unik |
| `GET` | `/Departemen/Ubah/{id}` | `SuperAdmin`, `HRD` | Form ubah data master departemen |
| `POST` | `/Departemen/Ubah/{id}` | `SuperAdmin`, `HRD` | Menyimpan perubahan data departemen |
| `POST` | `/Departemen/Hapus/{id}` | `SuperAdmin`, `HRD` | Menghapus (soft delete) departemen jika tidak ada relasi aktif |

---

### 2.5 Modul Jabatan (`JabatanController`)
| HTTP Method | Route URL | Hak Akses | Deskripsi |
| :--- | :--- | :--- | :--- |
| `GET` | `/Jabatan` | `SuperAdmin`, `HRD`, `Manager` | Daftar master jabatan dan level hierarki |
| `GET` | `/Jabatan/Tambah` | `SuperAdmin`, `HRD` | Form tambah jabatan baru |
| `POST` | `/Jabatan/Tambah` | `SuperAdmin`, `HRD` | Menyimpan jabatan baru terikat departemen |
| `GET` | `/Jabatan/Ubah/{id}` | `SuperAdmin`, `HRD` | Form ubah data master jabatan |
| `POST` | `/Jabatan/Ubah/{id}` | `SuperAdmin`, `HRD` | Menyimpan perubahan data jabatan |
| `POST` | `/Jabatan/Hapus/{id}` | `SuperAdmin`, `HRD` | Menghapus (soft delete) jabatan jika tidak ada relasi aktif |
| `GET` | `/Jabatan/DapatkanBerdasarkanDepartemen` | `SuperAdmin`, `HRD`, `Manager` | AJAX API: Mengembalikan daftar JSON jabatan untuk dropdown bertingkat |

---

### 2.6 Modul Karyawan (`KaryawanController`)
| HTTP Method | Route URL | Hak Akses | Deskripsi |
| :--- | :--- | :--- | :--- |
| `GET` | `/Karyawan` | `SuperAdmin`, `HRD`, `Manager` | Daftar karyawan dengan pencarian, filter departemen, jabatan, status & pagination |
| `GET` | `/Karyawan/Detail/{id}` | `SuperAdmin`, `HRD`, `Manager` | Halaman tab profil karyawan (Data Diri, Kontrak, Dokumen, Kontak, Riwayat) |
| `GET` | `/Karyawan/Tambah` | `SuperAdmin`, `HRD` | Form pendaftaran karyawan baru dengan upload foto profil |
| `POST` | `/Karyawan/Tambah` | `SuperAdmin`, `HRD` | Menyimpan data karyawan baru dengan validasi NIK unik |
| `GET` | `/Karyawan/Ubah/{id}` | `SuperAdmin`, `HRD` | Form ubah data karyawan & perbarui foto profil |
| `POST` | `/Karyawan/Ubah/{id}` | `SuperAdmin`, `HRD` | Menyimpan pembaruan profil karyawan |
| `POST` | `/Karyawan/UbahStatus/{id}` | `SuperAdmin`, `HRD` | Mengubah status keaktifan karyawan (Aktif / Nonaktif) |
| `POST` | `/Karyawan/Hapus/{id}` | `SuperAdmin`, `HRD` | Menjalankan soft-delete karyawan |
| `POST` | `/Karyawan/TambahKontrak` | `SuperAdmin`, `HRD` | Menambahkan riwayat kontrak kerja karyawan (PKWT, PKWTT, dll) |
| `POST` | `/Karyawan/TambahKontakDarurat` | `SuperAdmin`, `HRD` | Menambahkan data kontak darurat / keluarga |
| `POST` | `/Karyawan/TambahRiwayatPekerjaan` | `SuperAdmin`, `HRD` | Menambahkan riwayat pekerjaan sebelumnya / jejak karier internal |
| `POST` | `/Karyawan/UploadDokumen` | `SuperAdmin`, `HRD` | Mengunggah dokumen digital (KTP, NPWP, Ijazah, Kontrak) dengan validasi berkas |
| `GET` | `/Karyawan/UnduhDokumen/{id}` | `SuperAdmin`, `HRD`, `Manager` | Mengunduh berkas digital dengan verifikasi kepemilikan tenant aman |
| `POST` | `/Karyawan/HapusDokumen/{id}` | `SuperAdmin`, `HRD` | Menghapus dokumen digital karyawan secara logis |

---

### 2.7 Modul Presensi & Kehadiran (`KehadiranController`)
| HTTP Method | Route URL | Hak Akses | Deskripsi |
| :--- | :--- | :--- | :--- |
| `GET` | `/Kehadiran` | Terautentikasi | Halaman riwayat log kehadiran dengan filter tanggal, departemen & karyawan |
| `GET` | `/Kehadiran/Presensi` | Terautentikasi (ESS Karyawan) | Layar presensi mandiri karyawan: jam real-time, info shift & status harian |
| `POST` | `/Kehadiran/CheckIn` | Terautentikasi (ESS Karyawan) | Rekam absensi masuk (Clock In) dengan hitung keterlambatan otomatis |
| `POST` | `/Kehadiran/CheckOut` | Terautentikasi (ESS Karyawan) | Rekam absensi pulang (Clock Out) dengan kalkulasi jam kerja riil |
| `GET` | `/Kehadiran/Rekap` | `SuperAdmin`, `HRD`, `Manager`, `Finance` | Laporan rekapitulasi absensi bulanan matriks per departemen |
| `GET` | `/Kehadiran/Shift` | `SuperAdmin`, `HRD` | Master pengaturan shift kerja perusahaan |
| `POST` | `/Kehadiran/SimpanShift` | `SuperAdmin`, `HRD` | Tambah atau ubah master shift kerja dengan validasi kode unik |
| `POST` | `/Kehadiran/HapusShift/{id}` | `SuperAdmin`, `HRD` | Soft delete shift kerja jika tidak digunakan dalam jadwal aktif |
| `GET` | `/Kehadiran/Lembur` | Terautentikasi | Monitoring antrean dan riwayat pengajuan lembur |
| `POST` | `/Kehadiran/AjukanLembur` | Terautentikasi (ESS Karyawan) | Pengajuan lembur mandiri oleh karyawan |
| `POST` | `/Kehadiran/ProsesPersetujuanLembur` | `SuperAdmin`, `HRD`, `Manager` | Eksekusi persetujuan atau penolakan pengajuan lembur |

---

### 2.8 Modul Cuti & Perizinan (`CutiController`)
| HTTP Method | Route URL | Hak Akses | Deskripsi |
| :--- | :--- | :--- | :--- |
| `GET` | `/Cuti` | Terautentikasi | Daftar pengajuan cuti, kartu ringkasan status & saldo cuti pribadi |
| `GET` | `/Cuti/Pengajuan` | Terautentikasi (ESS Karyawan) | Form pengajuan cuti baru mandiri dengan kalkulasi durasi hari kerja |
| `POST` | `/Cuti/Pengajuan` | Terautentikasi (ESS Karyawan) | Submit permohonan cuti (multipart form) beserta berkas dokumen lampiran |
| `GET` | `/Cuti/Detail/{id}` | Terautentikasi | Halaman detail permohonan cuti, riwayat approval & aksi otorisasi |
| `POST` | `/Cuti/SetujuiManager` | `SuperAdmin`, `HRD`, `Manager` | Persetujuan Cuti Tingkat 1 (Manager) -> status `Menunggu HRD` |
| `POST` | `/Cuti/TolakManager` | `SuperAdmin`, `HRD`, `Manager` | Penolakan Cuti Tingkat 1 (Manager) beserta pencatatan alasan |
| `POST` | `/Cuti/VerifikasiHRD` | `SuperAdmin`, `HRD` | Verifikasi Akhir Tingkat 2 (HRD) & pemotongan saldo cuti atomik |
| `POST` | `/Cuti/TolakHRD` | `SuperAdmin`, `HRD` | Penolakan Cuti Tingkat 2 (HRD) beserta pencatatan alasan |
| `GET` | `/Cuti/UnduhDokumen/{id}` | Terautentikasi | Streaming unduh berkas lampiran pendukung cuti terisolasi tenant |
| `GET` | `/Cuti/JenisCuti` | `SuperAdmin`, `HRD` | Master pengelolaan jenis cuti perusahaan (jatah hari, potong saldo, wajib lampiran) |
| `POST` | `/Cuti/SimpanJenisCuti` | `SuperAdmin`, `HRD` | Simpan / perbarui master jenis cuti |
| `POST` | `/Cuti/HapusJenisCuti/{id}` | `SuperAdmin`, `HRD` | Soft delete master jenis cuti |
| `GET` | `/Cuti/SaldoCuti` | `SuperAdmin`, `HRD`, `Manager` | Monitoring saldo kuota hak cuti tahunan seluruh karyawan |
| `POST` | `/Cuti/AturSaldoCuti` | `SuperAdmin`, `HRD` | Atur / sesuaikan saldo cuti per karyawan dan periode tahun |
| `POST` | `/Cuti/InisialisasiSaldoMassal` | `SuperAdmin`, `HRD` | Inisialisasi massal saldo cuti tahunan awal tahun untuk seluruh karyawan aktif |

---

### 2.9 Modul Penggajian / Payroll (`PenggajianController`)
| HTTP Method | Route URL | Hak Akses | Deskripsi |
| :--- | :--- | :--- | :--- |
| `GET` | `/Penggajian` | `SuperAdmin`, `HRD`, `Finance`, `Manager` *(Karyawan -> redirect Riwayat)* | Daftar riwayat siklus periode penggajian bulanan |
| `GET` | `/Penggajian/TambahPeriode` | `SuperAdmin`, `HRD`, `Finance` | Form pembuatan siklus periode penggajian baru dengan default cutoff |
| `POST` | `/Penggajian/TambahPeriode` | `SuperAdmin`, `HRD`, `Finance` | Menyimpan siklus periode penggajian baru dengan status Draf |
| `GET` | `/Penggajian/PeriodeDetail/{id}` | `SuperAdmin`, `HRD`, `Finance`, `Manager` | Halaman kerja payroll periode: daftar karyawan, review THP, dan rekap summary |
| `POST` | `/Penggajian/HitungBatch` | `SuperAdmin`, `HRD`, `Finance` | Menjalankan kalkulasi batch otomatis (Gaji Pokok, Tunjangan, Lembur Kemnaker, Telat, BPJS Kes/TK, TER PPh 21) |
| `POST` | `/Penggajian/SetujuiPeriode` | `SuperAdmin`, `HRD`, `Finance` | Persetujuan resmi (approval) & pembekuan (lock) seluruh payroll periode |
| `POST` | `/Penggajian/HapusPeriode/{id}` | `SuperAdmin`, `HRD`, `Finance` | Menghapus periode penggajian (hanya jika status draf dan belum disetujui) |
| `GET` | `/Penggajian/Detail/{id}` | `SuperAdmin`, `HRD`, `Finance`, `Manager` | Rincian komprehensif payroll karyawan beserta snapshot seluruh komponen |
| `GET` | `/Penggajian/Slip/{id}` | Terautentikasi (HRD, Finance, Manager, Karyawan Bersangkutan) | Tampilan lembar slip gaji resmi (kode verifikasi, terbilang rupiah, kop perusahaan, siap print/PDF) |
| `GET` | `/Penggajian/Riwayat` | Terautentikasi (ESS Karyawan) | Riwayat slip gaji mandiri milik karyawan yang telah disetujui |
| `GET` | `/Penggajian/Ekspor/{id}` | `SuperAdmin`, `HRD`, `Finance` | Unduh file CSV rekapitulasi pembayaran payroll untuk kliring/transfer bank |
| `GET` | `/Penggajian/Komponen` | `SuperAdmin`, `HRD`, `Finance` | Master daftar komponen gaji perusahaan (pendapatan, potongan, tetap/tidak tetap) |
| `POST` | `/Penggajian/SimpanKomponen` | `SuperAdmin`, `HRD`, `Finance` | Tambah atau perbarui master komponen gaji tenant |
| `POST` | `/Penggajian/HapusKomponen/{id}` | `SuperAdmin`, `HRD`, `Finance` | Hapus komponen gaji (komponen otomatis sistem dilindungi) |
| `GET` | `/Penggajian/AturGajiKaryawan/{id}` | `SuperAdmin`, `HRD`, `Finance` | Form konfigurasi struktur gaji, status PTKP, nomor NPWP, BPJS, & rekening bank per karyawan |
| `POST` | `/Penggajian/SimpanGajiKaryawan` | `SuperAdmin`, `HRD`, `Finance` | Simpan struktur gaji rutin dan data perpajakan karyawan |
| `GET` | `/Penggajian/Kebijakan` | `SuperAdmin`, `HRD`, `Finance` | Form konfigurasi kebijakan tarif & aturan penggajian tenant (lembur, denda telat, BPJS, TER PPh 21, cutoff) |
| `POST` | `/Penggajian/SimpanKebijakan` | `SuperAdmin`, `HRD`, `Finance` | Simpan perubahan kebijakan penggajian tenant |

---

### 2.10 Modul Rekrutmen (`RekrutmenController`)
| HTTP Method | Route URL | Hak Akses | Deskripsi |
| :--- | :--- | :--- | :--- |
| `GET` | `/Rekrutmen` | `SuperAdmin`, `HRD`, `Manager` | Dashboard analitik rekrutmen: ringkasan metrik, grafik Chart.js status pelamar, pipeline |
| `GET` | `/Rekrutmen/Lowongan` | `SuperAdmin`, `HRD`, `Manager` | Daftar posisi lowongan kerja dengan filter status & tab |
| `GET` | `/Rekrutmen/TambahLowongan` | `SuperAdmin`, `HRD` | Form pembuatan lowongan kerja baru |
| `POST` | `/Rekrutmen/TambahLowongan` | `SuperAdmin`, `HRD` | Menyimpan lowongan baru dengan validasi kode lowongan unik |
| `GET` | `/Rekrutmen/UbahLowongan/{id}` | `SuperAdmin`, `HRD` | Form ubah data dan kuota lowongan kerja |
| `POST` | `/Rekrutmen/UbahLowongan/{id}` | `SuperAdmin`, `HRD` | Menyimpan perubahan lowongan kerja |
| `GET` | `/Rekrutmen/DetailLowongan/{id}` | `SuperAdmin`, `HRD`, `Manager` | Halaman tab lowongan: Deskripsi & Kualifikasi, Daftar Pelamar, Pipeline Tahapan Seleksi |
| `POST` | `/Rekrutmen/HapusLowongan/{id}` | `SuperAdmin`, `HRD` | Soft delete lowongan kerja |
| `GET` | `/Rekrutmen/Pelamar` | `SuperAdmin`, `HRD`, `Manager` | Daftar seluruh kandidat pelamar dengan filter posisi lowongan & status |
| `GET` | `/Rekrutmen/DaftarPelamar` | `SuperAdmin`, `HRD` | Form pendaftaran kandidat pelamar kerja dengan upload berkas CV (PDF/DOCX max 5MB) |
| `POST` | `/Rekrutmen/DaftarPelamar` | `SuperAdmin`, `HRD` | Memproses registrasi pelamar dan menyimpan berkas CV terisolasi tenant |
| `GET` | `/Rekrutmen/DetailPelamar/{id}` | `SuperAdmin`, `HRD`, `Manager` | Profil lengkap pelamar, berkas CV, riwayat interview, evaluasi skor, dan aksi pipeline |
| `POST` | `/Rekrutmen/SeleksiAdministrasi` | `SuperAdmin`, `HRD` | Putusan kelayakan berkas CV (Lolos / Gugur Berkas) |
| `GET` | `/Rekrutmen/UnduhCV/{id}` | `SuperAdmin`, `HRD`, `Manager` | Mengunduh file CV kandidat dengan otorisasi tenant terverifikasi |
| `POST` | `/Rekrutmen/SimpanJadwalWawancara` | `SuperAdmin`, `HRD`, `Manager` | Menjadwalkan undangan interview (tatap muka atau online link Meet/Zoom) |
| `POST` | `/Rekrutmen/SimpanHasilWawancara` | `SuperAdmin`, `HRD`, `Manager` | Mencatat skor evaluasi penilaian kandidat (0 - 100), kelebihan, kekurangan, dan keputusan |
| `POST` | `/Rekrutmen/UbahStatusPelamar` | `SuperAdmin`, `HRD` | Memperbarui status alur pipeline kandidat secara manual |
| `POST` | `/Rekrutmen/TerimaKandidat` | `SuperAdmin`, `HRD` | Menerima kandidat dan melakukan konversi atomik menjadi entitas Karyawan aktif |

---

### 2.11 Modul Penilaian Kinerja / KPI (`KinerjaController`)
| HTTP Method | Route URL | Hak Akses | Deskripsi |
| :--- | :--- | :--- | :--- |
| `GET` | `/Kinerja` | `SuperAdmin`, `HRD`, `Manager` *(Karyawan -> redirect RiwayatSaya)* | Dashboard analitik KPI: ringkasan metrik, grafik Chart.js distribusi predikat A-E, skor per departemen |
| `GET` | `/Kinerja/Periode` | `SuperAdmin`, `HRD`, `Manager` | Daftar siklus periode penilaian performa (Semester, Tahunan, Kuartalan) |
| `GET` | `/Kinerja/TambahPeriode` | `SuperAdmin`, `HRD` | Form pembuatan siklus periode penilaian baru |
| `POST` | `/Kinerja/TambahPeriode` | `SuperAdmin`, `HRD` | Menyimpan siklus periode penilaian kinerja |
| `GET` | `/Kinerja/UbahPeriode/{id}` | `SuperAdmin`, `HRD` | Form ubah data periode penilaian |
| `POST` | `/Kinerja/UbahPeriode/{id}` | `SuperAdmin`, `HRD` | Menyimpan pembaruan periode penilaian |
| `POST` | `/Kinerja/HapusPeriode/{id}` | `SuperAdmin`, `HRD` | Hapus periode penilaian jika belum memiliki lembar evaluasi |
| `GET` | `/Kinerja/Indikator` | `SuperAdmin`, `HRD`, `Manager` | Master indikator KPI: tolak ukur, target, satuan, bobot bawaan, filter departemen |
| `GET` | `/Kinerja/TambahIndikator` | `SuperAdmin`, `HRD` | Form pembuatan indikator KPI baru |
| `POST` | `/Kinerja/TambahIndikator` | `SuperAdmin`, `HRD` | Menyimpan indikator KPI baru |
| `GET` | `/Kinerja/UbahIndikator/{id}` | `SuperAdmin`, `HRD` | Form ubah data indikator KPI |
| `POST` | `/Kinerja/UbahIndikator/{id}` | `SuperAdmin`, `HRD` | Menyimpan perubahan indikator KPI |
| `POST` | `/Kinerja/HapusIndikator/{id}` | `SuperAdmin`, `HRD` | Hapus indikator KPI jika belum pernah digunakan pada lembar penilaian |
| `GET` | `/Kinerja/Penilaian` | `SuperAdmin`, `HRD`, `Manager` | Daftar arsip seluruh lembar penilaian performa karyawan dengan filter periode & karyawan |
| `GET` | `/Kinerja/InputPenilaian` | `SuperAdmin`, `HRD`, `Manager` | Form penilaian kinerja interaktif: evaluasi skor objektif per butir KPI, kalkulasi skor live, ulasan & rekomendasi |
| `POST` | `/Kinerja/InputPenilaian` | `SuperAdmin`, `HRD`, `Manager` | Menyimpan penilaian kinerja, auto-kalkulasi skor tertimbang $\sum (Bobot \times Skor)$ dan penentuan predikat resmi |
| `GET` | `/Kinerja/DetailPenilaian/{id}` | Terautentikasi (HRD, Manager, Karyawan Bersangkutan) | Lembar resmi evaluasi kinerja karyawan, rincian skor indikator, tanda tangan evaluator, siap print/PDF |
| `GET` | `/Kinerja/Laporan` | `SuperAdmin`, `HRD`, `Manager` | Laporan analitik performa kerja karyawan dengan filter departemen, karyawan, dan tahun |
| `GET` | `/Kinerja/RiwayatSaya` | Terautentikasi (ESS Karyawan) | Portal riwayat evaluasi performa mandiri karyawan |
