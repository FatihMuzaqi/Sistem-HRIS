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
| `GET` | `/Kehadiran` | Semua Peran | Halaman rekapitulasi kehadiran |
| `POST` | `/Kehadiran/PresensiMasuk` | Karyawan | Rekam absensi masuk (Clock In) |
| `POST` | `/Kehadiran/PresensiPulang` | Karyawan | Rekam absensi pulang (Clock Out) |
| `GET` | `/Kehadiran/Rekapitulasi` | `HRD`, `Manager`, `Finance` | Laporan absensi bulanan untuk proses payroll |

---

### 2.8 Modul Cuti & Perizinan (`CutiController`)
| HTTP Method | Route URL | Hak Akses | Deskripsi |
| :--- | :--- | :--- | :--- |
| `GET` | `/Cuti` | Semua Peran | Daftar pengajuan cuti & saldo cuti tahunan |
| `GET` | `/Cuti/Pengajuan` | Karyawan | Form pengajuan cuti baru |
| `POST` | `/Cuti/Pengajuan` | Karyawan | Submit pengajuan cuti beserta lampiran |
| `POST` | `/Cuti/Setujui/{id}` | `Manager`, `HRD` | Aksi persetujuan (approval) pengajuan cuti |
| `POST` | `/Cuti/Tolak/{id}` | `Manager`, `HRD` | Aksi penolakan pengajuan cuti |

---

### 2.9 Modul Penggajian (`PenggajianController`)
| HTTP Method | Route URL | Hak Akses | Deskripsi |
| :--- | :--- | :--- | :--- |
| `GET` | `/Penggajian` | `HRD`, `Finance` | Ringkasan proses penggajian periode berjalan |
| `POST` | `/Penggajian/HitungBatch` | `HRD`, `Finance` | Kalkulasi otomatis gaji, lembur, potongan & BPJS |
| `GET` | `/Penggajian/Slip/{id}` | Terautentikasi | Cetak / Tampilkan slip gaji individual |
| `GET` | `/Penggajian/EksporTransfer` | `Finance` | Unduh berkas rekapitulasi transfer bank (Excel) |
