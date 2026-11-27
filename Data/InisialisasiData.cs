using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SistemHRD.Common.Constants;
using SistemHRD.Models.Entitas;
using SistemHRD.Models.Enums;

namespace SistemHRD.Data;

/// <summary>
/// Layanan inisialisasi awal (Seeder) untuk membuat Peran default, Perusahaan default,
/// relasi PenggunaPerusahaan, dan akun uji coba untuk setiap peran.
/// </summary>
public static class InisialisasiData
{
    public static async Task InisialisasiAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<KonteksAplikasi>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<Peran>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<Pengguna>>();

        // 1. Inisialisasi Peran Pengguna (Roles)
        foreach (var namaPeran in NamaPeran.SemuaPeran)
        {
            if (!await roleManager.RoleExistsAsync(namaPeran))
            {
                await roleManager.CreateAsync(new Peran(namaPeran)
                {
                    Deskripsi = $"Peran hak akses {namaPeran} dalam SistemHRD"
                });
            }
        }

        // 2. Inisialisasi Perusahaan 1 (PT Solusi HR Nusantara)
        var perusahaan1 = await context.Perusahaan
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(p => p.KodePerusahaan == "PRSH001");

        if (perusahaan1 == null)
        {
            perusahaan1 = new Perusahaan
            {
                Id = Guid.NewGuid(),
                KodePerusahaan = "PRSH001",
                NamaPerusahaan = "PT Solusi HR Nusantara",
                Email = "kontak@solusihr.id",
                NomorTelepon = "021-5551234",
                Alamat = "Gedung Cyber 2 Lantai 15, Jl. HR Rasuna Said, Jakarta Selatan",
                NPWP = "01.234.567.8-012.000",
                Website = "https://solusihr.id",
                StatusAktif = true,
                CreatedAt = DateTimeOffset.UtcNow,
                CreatedBy = "Sistem"
            };
            await context.Perusahaan.AddAsync(perusahaan1);
            await context.SaveChangesAsync();
        }

        // 3. Inisialisasi Perusahaan 2 (PT Maju Bersama Digital) - untuk pengujian isolasi multi-tenant
        var perusahaan2 = await context.Perusahaan
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(p => p.KodePerusahaan == "PRSH002");

        if (perusahaan2 == null)
        {
            perusahaan2 = new Perusahaan
            {
                Id = Guid.NewGuid(),
                KodePerusahaan = "PRSH002",
                NamaPerusahaan = "PT Maju Bersama Digital",
                Email = "halo@majubersama.id",
                NomorTelepon = "021-7778899",
                Alamat = "Wisma Nusantara Lt. 8, Jl. MH Thamrin, Jakarta Pusat",
                NPWP = "02.888.999.1-013.000",
                Website = "https://majubersama.id",
                StatusAktif = true,
                CreatedAt = DateTimeOffset.UtcNow,
                CreatedBy = "Sistem"
            };
            await context.Perusahaan.AddAsync(perusahaan2);
            await context.SaveChangesAsync();
        }

        // 4. Inisialisasi Akun SuperAdmin (Platform Level)
        await BuatPenggunaJikaBelumAdaAsync(
            userManager,
            context,
            "superadmin@sistemhrd.id",
            "Super Administrator",
            "Admin#2026!",
            NamaPeran.SuperAdmin,
            perusahaanId: null
        );

        // 5. Inisialisasi Akun HRD Perusahaan 1
        await BuatPenggunaJikaBelumAdaAsync(
            userManager,
            context,
            "hrd@solusihr.id",
            "Budi Santoso (HRD Manager)",
            "Hrd#2026!",
            NamaPeran.HRD,
            perusahaanId: perusahaan1.Id
        );

        // 6. Inisialisasi Akun HRD Perusahaan 2 (Untuk Uji Isolasi Tenant)
        await BuatPenggunaJikaBelumAdaAsync(
            userManager,
            context,
            "hrd2@majubersama.id",
            "Siti Rahma (HRD Maju Bersama)",
            "Hrd2#2026!",
            NamaPeran.HRD,
            perusahaanId: perusahaan2.Id
        );

        // 7. Inisialisasi Akun Manager Perusahaan 1
        await BuatPenggunaJikaBelumAdaAsync(
            userManager,
            context,
            "manager@solusihr.id",
            "Ahmad Fauzi (Engineering Manager)",
            "Manager#2026!",
            NamaPeran.Manager,
            perusahaanId: perusahaan1.Id
        );

        // 8. Inisialisasi Akun Finance Perusahaan 1
        await BuatPenggunaJikaBelumAdaAsync(
            userManager,
            context,
            "finance@solusihr.id",
            "Dewi Lestari (Finance Specialist)",
            "Finance#2026!",
            NamaPeran.Finance,
            perusahaanId: perusahaan1.Id
        );

        // 9. Inisialisasi Akun Karyawan Perusahaan 1
        await BuatPenggunaJikaBelumAdaAsync(
            userManager,
            context,
            "karyawan@solusihr.id",
            "Rian Pratama (Software Engineer)",
            "Karyawan#2026!",
            NamaPeran.Karyawan,
            perusahaanId: perusahaan1.Id
        );

        // 10. Inisialisasi Data Master Departemen, Jabatan, dan Karyawan Contoh
        await InisialisasiDataKaryawanAsync(context, perusahaan1.Id, perusahaan2.Id);

        // 11. Inisialisasi Data Modul Kehadiran dan Cuti (Shift, Jenis Cuti, Saldo Cuti)
        await InisialisasiDataKehadiranDanCutiAsync(context, perusahaan1.Id, perusahaan2.Id);
    }

    private static async Task InisialisasiDataKaryawanAsync(KonteksAplikasi context, Guid perusahaanId1, Guid perusahaanId2)
    {
        // === PERUSAHAAN 1: PT Solusi HR Nusantara ===
        var deptIT1 = await context.Departemen
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(d => d.PerusahaanId == perusahaanId1 && d.KodeDepartemen == "IT");

        if (deptIT1 == null)
        {
            deptIT1 = new Departemen
            {
                Id = Guid.NewGuid(),
                PerusahaanId = perusahaanId1,
                KodeDepartemen = "IT",
                NamaDepartemen = "Teknologi Informasi",
                Deskripsi = "Mengelola infrastruktur IT, sistem rekayasa perangkat lunak, dan keamanan siber.",
                StatusAktif = true,
                CreatedAt = DateTimeOffset.UtcNow,
                CreatedBy = "Sistem"
            };
            await context.Departemen.AddAsync(deptIT1);
        }

        var deptHR1 = await context.Departemen
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(d => d.PerusahaanId == perusahaanId1 && d.KodeDepartemen == "HRD");

        if (deptHR1 == null)
        {
            deptHR1 = new Departemen
            {
                Id = Guid.NewGuid(),
                PerusahaanId = perusahaanId1,
                KodeDepartemen = "HRD",
                NamaDepartemen = "Sumber Daya Manusia",
                Deskripsi = "Pengembangan talenta, hubungan industrial, dan operasional kepegawaian.",
                StatusAktif = true,
                CreatedAt = DateTimeOffset.UtcNow,
                CreatedBy = "Sistem"
            };
            await context.Departemen.AddAsync(deptHR1);
        }

        var deptFIN1 = await context.Departemen
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(d => d.PerusahaanId == perusahaanId1 && d.KodeDepartemen == "FIN");

        if (deptFIN1 == null)
        {
            deptFIN1 = new Departemen
            {
                Id = Guid.NewGuid(),
                PerusahaanId = perusahaanId1,
                KodeDepartemen = "FIN",
                NamaDepartemen = "Keuangan & Akuntansi",
                Deskripsi = "Pengelolaan anggaran, pembukuan keuangan, dan perpajakan perusahaan.",
                StatusAktif = true,
                CreatedAt = DateTimeOffset.UtcNow,
                CreatedBy = "Sistem"
            };
            await context.Departemen.AddAsync(deptFIN1);
        }

        await context.SaveChangesAsync();

        // Inisialisasi Jabatan Perusahaan 1
        var jabIT1 = await context.Jabatan
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(j => j.PerusahaanId == perusahaanId1 && j.NamaJabatan == "Senior Software Engineer");

        if (jabIT1 == null)
        {
            jabIT1 = new Jabatan
            {
                Id = Guid.NewGuid(),
                PerusahaanId = perusahaanId1,
                DepartemenId = deptIT1.Id,
                NamaJabatan = "Senior Software Engineer",
                Deskripsi = "Merancang arsitektur backend dan memimpin pengembangan aplikasi inti.",
                LevelJabatan = 2,
                StatusAktif = true,
                CreatedAt = DateTimeOffset.UtcNow,
                CreatedBy = "Sistem"
            };
            await context.Jabatan.AddAsync(jabIT1);
        }

        var jabHR1 = await context.Jabatan
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(j => j.PerusahaanId == perusahaanId1 && j.NamaJabatan == "HR Operations Specialist");

        if (jabHR1 == null)
        {
            jabHR1 = new Jabatan
            {
                Id = Guid.NewGuid(),
                PerusahaanId = perusahaanId1,
                DepartemenId = deptHR1.Id,
                NamaJabatan = "HR Operations Specialist",
                Deskripsi = "Mengurus kepatuhan administrasi, kontrak kerja, dan absensi karyawan.",
                LevelJabatan = 1,
                StatusAktif = true,
                CreatedAt = DateTimeOffset.UtcNow,
                CreatedBy = "Sistem"
            };
            await context.Jabatan.AddAsync(jabHR1);
        }

        await context.SaveChangesAsync();

        // Inisialisasi Karyawan Perusahaan 1
        var kry1 = await context.Karyawan
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(k => k.PerusahaanId == perusahaanId1 && k.NomorIndukKaryawan == "KRY-2026-001");

        if (kry1 == null)
        {
            kry1 = new Karyawan
            {
                Id = Guid.NewGuid(),
                PerusahaanId = perusahaanId1,
                NomorIndukKaryawan = "KRY-2026-001",
                NamaLengkap = "Rian Pratama",
                TempatLahir = "Jakarta",
                TanggalLahir = new DateTime(1996, 5, 14),
                JenisKelamin = Models.Enums.JenisKelamin.LakiLaki,
                Alamat = "Jl. Melati No. 42, Tebet, Jakarta Selatan",
                NomorTelepon = "081234567890",
                Email = "karyawan@solusihr.id",
                TanggalMasuk = new DateTime(2023, 2, 1),
                DepartemenId = deptIT1.Id,
                JabatanId = jabIT1.Id,
                StatusKaryawan = Models.Enums.StatusKaryawan.Tetap,
                CreatedAt = DateTimeOffset.UtcNow,
                CreatedBy = "Sistem"
            };
            await context.Karyawan.AddAsync(kry1);
            await context.SaveChangesAsync();

            // Tambah Kontrak Awal
            var ktr1 = new KontrakKerja
            {
                Id = Guid.NewGuid(),
                PerusahaanId = perusahaanId1,
                KaryawanId = kry1.Id,
                NomorKontrak = "KTR/SHR/2023/001",
                JenisKontrak = Models.Enums.JenisKontrak.PKWTT,
                TanggalMulai = kry1.TanggalMasuk,
                TanggalBerakhir = null,
                StatusKontrak = Models.Enums.StatusKontrak.Aktif,
                Keterangan = "Perjanjian Kerja Waktu Tidak Tertentu (Karyawan Tetap)",
                CreatedAt = DateTimeOffset.UtcNow,
                CreatedBy = "Sistem"
            };
            await context.KontrakKerja.AddAsync(ktr1);

            // Tambah Kontak Darurat
            var kd1 = new KontakDaruratKaryawan
            {
                Id = Guid.NewGuid(),
                PerusahaanId = perusahaanId1,
                KaryawanId = kry1.Id,
                NamaLengkap = "Nurul Aini",
                Hubungan = "Istri",
                NomorTelepon = "081298765432",
                Alamat = "Jl. Melati No. 42, Tebet, Jakarta Selatan",
                CreatedAt = DateTimeOffset.UtcNow,
                CreatedBy = "Sistem"
            };
            await context.KontakDaruratKaryawan.AddAsync(kd1);

            // Tambah Riwayat Pekerjaan
            var rw1 = new RiwayatPekerjaan
            {
                Id = Guid.NewGuid(),
                PerusahaanId = perusahaanId1,
                KaryawanId = kry1.Id,
                NamaPerusahaanSebelumnya = "PT Solusi Teknologi Cerdas",
                JabatanTerakhir = "Junior Web Developer",
                TahunMulai = 2020,
                TahunSelesai = 2023,
                DeskripsiTugas = "Mengembangkan fitur frontend dan backend REST API menggunakan .NET.",
                CreatedAt = DateTimeOffset.UtcNow,
                CreatedBy = "Sistem"
            };
            await context.RiwayatPekerjaan.AddAsync(rw1);
            await context.SaveChangesAsync();
        }

        // === PERUSAHAAN 2: PT Maju Bersama Digital (Uji Isolasi Multi-Tenant) ===
        var dept2 = await context.Departemen
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(d => d.PerusahaanId == perusahaanId2 && d.KodeDepartemen == "DGT");

        if (dept2 == null)
        {
            dept2 = new Departemen
            {
                Id = Guid.NewGuid(),
                PerusahaanId = perusahaanId2,
                KodeDepartemen = "DGT",
                NamaDepartemen = "Divisi Digital & Desain",
                Deskripsi = "Unit inovasi produk digital dan UI/UX design.",
                StatusAktif = true,
                CreatedAt = DateTimeOffset.UtcNow,
                CreatedBy = "Sistem"
            };
            await context.Departemen.AddAsync(dept2);
            await context.SaveChangesAsync();
        }

        var jab2 = await context.Jabatan
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(j => j.PerusahaanId == perusahaanId2 && j.NamaJabatan == "Product Design Lead");

        if (jab2 == null)
        {
            jab2 = new Jabatan
            {
                Id = Guid.NewGuid(),
                PerusahaanId = perusahaanId2,
                DepartemenId = dept2.Id,
                NamaJabatan = "Product Design Lead",
                Deskripsi = "Memimpin riset pengguna dan desain antarmuka aplikasi.",
                LevelJabatan = 2,
                StatusAktif = true,
                CreatedAt = DateTimeOffset.UtcNow,
                CreatedBy = "Sistem"
            };
            await context.Jabatan.AddAsync(jab2);
            await context.SaveChangesAsync();
        }

        var kry2 = await context.Karyawan
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(k => k.PerusahaanId == perusahaanId2 && k.NomorIndukKaryawan == "MBD-2026-001");

        if (kry2 == null)
        {
            kry2 = new Karyawan
            {
                Id = Guid.NewGuid(),
                PerusahaanId = perusahaanId2,
                NomorIndukKaryawan = "MBD-2026-001",
                NamaLengkap = "Bayu Wicaksono",
                TempatLahir = "Bandung",
                TanggalLahir = new DateTime(1998, 8, 20),
                JenisKelamin = Models.Enums.JenisKelamin.LakiLaki,
                Alamat = "Jl. Dago Asri No. 12, Bandung",
                NomorTelepon = "081987654321",
                Email = "bayu@majubersama.id",
                TanggalMasuk = new DateTime(2024, 1, 10),
                DepartemenId = dept2.Id,
                JabatanId = jab2.Id,
                StatusKaryawan = Models.Enums.StatusKaryawan.Tetap,
                CreatedAt = DateTimeOffset.UtcNow,
                CreatedBy = "Sistem"
            };
            await context.Karyawan.AddAsync(kry2);
            await context.SaveChangesAsync();
        }
    }

    private static async Task<Pengguna> BuatPenggunaJikaBelumAdaAsync(
        UserManager<Pengguna> userManager,
        KonteksAplikasi context,
        string email,
        string namaLengkap,
        string password,
        string namaPeran,
        Guid? perusahaanId)
    {
        var user = await context.Users
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Email == email);

        if (user == null)
        {
            user = new Pengguna
            {
                Id = Guid.NewGuid(),
                UserName = email,
                Email = email,
                NamaLengkap = namaLengkap,
                PerusahaanId = perusahaanId,
                StatusAktif = true,
                EmailConfirmed = true,
                CreatedAt = DateTimeOffset.UtcNow,
                CreatedBy = "Sistem"
            };

            var result = await userManager.CreateAsync(user, password);
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(user, namaPeran);

                // Tambahkan ke relasi PenggunaPerusahaan jika terikat perusahaan
                if (perusahaanId.HasValue)
                {
                    var relasi = new PenggunaPerusahaan
                    {
                        Id = Guid.NewGuid(),
                        PenggunaId = user.Id,
                        PerusahaanId = perusahaanId.Value,
                        PeranDiPerusahaan = namaPeran,
                        IsDefault = true,
                        StatusAktif = true,
                        JoinedAt = DateTimeOffset.UtcNow
                    };
                    await context.PenggunaPerusahaan.AddAsync(relasi);
                    await context.SaveChangesAsync();
                }
            }
        }
        else
        {
            // Pastikan relasi PenggunaPerusahaan ada
            if (perusahaanId.HasValue)
            {
                var relasiAda = await context.PenggunaPerusahaan
                    .IgnoreQueryFilters()
                    .AnyAsync(pp => pp.PenggunaId == user.Id && pp.PerusahaanId == perusahaanId.Value);

                if (!relasiAda)
                {
                    var relasi = new PenggunaPerusahaan
                    {
                        Id = Guid.NewGuid(),
                        PenggunaId = user.Id,
                        PerusahaanId = perusahaanId.Value,
                        PeranDiPerusahaan = namaPeran,
                        IsDefault = true,
                        StatusAktif = true,
                        JoinedAt = DateTimeOffset.UtcNow
                    };
                    await context.PenggunaPerusahaan.AddAsync(relasi);
                    await context.SaveChangesAsync();
                }
            }
        }

        return user;
    }

    private static async Task InisialisasiDataKehadiranDanCutiAsync(KonteksAplikasi context, Guid perusahaanId1, Guid perusahaanId2)
    {
        // 1. Inisialisasi Shift Kerja Perusahaan 1
        var shiftReg1 = await context.ShiftKerja
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(s => s.PerusahaanId == perusahaanId1 && s.KodeShift == "REG");

        if (shiftReg1 == null)
        {
            shiftReg1 = new ShiftKerja
            {
                Id = Guid.NewGuid(),
                PerusahaanId = perusahaanId1,
                NamaShift = "Shift Pagi (Regular)",
                KodeShift = "REG",
                JamMasuk = new TimeSpan(8, 0, 0),
                JamPulang = new TimeSpan(17, 0, 0),
                ToleransiKeterlambatanMenit = 15,
                Keterangan = "Jam kerja standar operasional kantor (08:00 - 17:00)",
                StatusAktif = true,
                CreatedAt = DateTimeOffset.UtcNow,
                CreatedBy = "Sistem"
            };
            await context.ShiftKerja.AddAsync(shiftReg1);
        }

        var shiftSiang1 = await context.ShiftKerja
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(s => s.PerusahaanId == perusahaanId1 && s.KodeShift == "SIANG");

        if (shiftSiang1 == null)
        {
            shiftSiang1 = new ShiftKerja
            {
                Id = Guid.NewGuid(),
                PerusahaanId = perusahaanId1,
                NamaShift = "Shift Siang",
                KodeShift = "SIANG",
                JamMasuk = new TimeSpan(13, 0, 0),
                JamPulang = new TimeSpan(21, 0, 0),
                ToleransiKeterlambatanMenit = 15,
                Keterangan = "Shift operasional siang (13:00 - 21:00)",
                StatusAktif = true,
                CreatedAt = DateTimeOffset.UtcNow,
                CreatedBy = "Sistem"
            };
            await context.ShiftKerja.AddAsync(shiftSiang1);
        }

        // Shift Perusahaan 2
        var shiftReg2 = await context.ShiftKerja
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(s => s.PerusahaanId == perusahaanId2 && s.KodeShift == "MBD-REG");

        if (shiftReg2 == null)
        {
            shiftReg2 = new ShiftKerja
            {
                Id = Guid.NewGuid(),
                PerusahaanId = perusahaanId2,
                NamaShift = "Shift Regular Digital",
                KodeShift = "MBD-REG",
                JamMasuk = new TimeSpan(8, 30, 0),
                JamPulang = new TimeSpan(17, 30, 0),
                ToleransiKeterlambatanMenit = 15,
                Keterangan = "Jam operasional kantor kreatif (08:30 - 17:30)",
                StatusAktif = true,
                CreatedAt = DateTimeOffset.UtcNow,
                CreatedBy = "Sistem"
            };
            await context.ShiftKerja.AddAsync(shiftReg2);
        }

        await context.SaveChangesAsync();

        // 2. Inisialisasi Jenis Cuti Perusahaan 1
        var jenisCutiList1 = new[]
        {
            new { Kode = "CT-THN", Nama = "Cuti Tahunan", Jatah = 12, Potong = true, Lampiran = false, Ket = "Hak cuti tahunan reguler karyawan" },
            new { Kode = "CT-SKT", Nama = "Cuti Sakit", Jatah = 14, Potong = false, Lampiran = true, Ket = "Cuti alasan medis dengan surat keterangan dokter" },
            new { Kode = "CT-MLH", Nama = "Cuti Melahirkan", Jatah = 90, Potong = false, Lampiran = true, Ket = "Hak cuti bersalin bagi karyawati" },
            new { Kode = "CT-NIKAH", Nama = "Cuti Menikah", Jatah = 3, Potong = false, Lampiran = false, Ket = "Cuti khusus pernikahan karyawan" },
            new { Kode = "CT-DUKA", Nama = "Cuti Duka Cita", Jatah = 2, Potong = false, Lampiran = false, Ket = "Cuti kedukaan anggota keluarga inti" }
        };

        foreach (var jc in jenisCutiList1)
        {
            var ada = await context.JenisCuti
                .IgnoreQueryFilters()
                .AnyAsync(j => j.PerusahaanId == perusahaanId1 && j.KodeJenisCuti == jc.Kode);

            if (!ada)
            {
                await context.JenisCuti.AddAsync(new JenisCuti
                {
                    Id = Guid.NewGuid(),
                    PerusahaanId = perusahaanId1,
                    KodeJenisCuti = jc.Kode,
                    NamaJenisCuti = jc.Nama,
                    JatahHariPerTahun = jc.Jatah,
                    PotongCutiTahunan = jc.Potong,
                    WajibLampiran = jc.Lampiran,
                    Keterangan = jc.Ket,
                    StatusAktif = true,
                    CreatedAt = DateTimeOffset.UtcNow,
                    CreatedBy = "Sistem"
                });
            }
        }

        // Inisialisasi Jenis Cuti Perusahaan 2
        var adaJc2 = await context.JenisCuti
            .IgnoreQueryFilters()
            .AnyAsync(j => j.PerusahaanId == perusahaanId2 && j.KodeJenisCuti == "CT-THN");

        if (!adaJc2)
        {
            await context.JenisCuti.AddAsync(new JenisCuti
            {
                Id = Guid.NewGuid(),
                PerusahaanId = perusahaanId2,
                KodeJenisCuti = "CT-THN",
                NamaJenisCuti = "Cuti Tahunan",
                JatahHariPerTahun = 12,
                PotongCutiTahunan = true,
                WajibLampiran = false,
                Keterangan = "Hak cuti tahunan PT Maju Bersama Digital",
                StatusAktif = true,
                CreatedAt = DateTimeOffset.UtcNow,
                CreatedBy = "Sistem"
            });
        }

        await context.SaveChangesAsync();

        // 3. Tautkan Akun Karyawan dan Manager ke Profil Karyawan (ESS)
        var userKry1 = await context.Users
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Email == "karyawan@solusihr.id");

        var kry1 = await context.Karyawan
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(k => k.PerusahaanId == perusahaanId1 && k.NomorIndukKaryawan == "KRY-2026-001");

        if (userKry1 != null && kry1 != null && kry1.PenggunaId == null)
        {
            kry1.PenggunaId = userKry1.Id;
            context.Karyawan.Update(kry1);
            await context.SaveChangesAsync();
        }

        var userMgr1 = await context.Users
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Email == "manager@solusihr.id");

        var kryMgr1 = await context.Karyawan
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(k => k.PerusahaanId == perusahaanId1 && k.Email == "manager@solusihr.id");

        var deptIT1 = await context.Departemen
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(d => d.PerusahaanId == perusahaanId1 && d.KodeDepartemen == "IT");

        var jabIT1 = await context.Jabatan
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(j => j.PerusahaanId == perusahaanId1 && j.NamaJabatan == "Senior Software Engineer");

        if (kryMgr1 == null && userMgr1 != null && deptIT1 != null && jabIT1 != null)
        {
            kryMgr1 = new Karyawan
            {
                Id = Guid.NewGuid(),
                PerusahaanId = perusahaanId1,
                NomorIndukKaryawan = "KRY-2026-002",
                NamaLengkap = "Ahmad Fauzi",
                TempatLahir = "Surabaya",
                TanggalLahir = new DateTime(1990, 3, 10),
                JenisKelamin = Models.Enums.JenisKelamin.LakiLaki,
                Alamat = "Jl. Gatot Subroto No. 88, Jakarta Selatan",
                NomorTelepon = "081398765432",
                Email = "manager@solusihr.id",
                TanggalMasuk = new DateTime(2022, 1, 15),
                DepartemenId = deptIT1.Id,
                JabatanId = jabIT1.Id,
                StatusKaryawan = Models.Enums.StatusKaryawan.Tetap,
                PenggunaId = userMgr1.Id,
                CreatedAt = DateTimeOffset.UtcNow,
                CreatedBy = "Sistem"
            };
            await context.Karyawan.AddAsync(kryMgr1);
            await context.SaveChangesAsync();
        }

        // Tautkan Karyawan Perusahaan 2
        var userHrd2 = await context.Users
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Email == "hrd2@majubersama.id");

        var kry2 = await context.Karyawan
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(k => k.PerusahaanId == perusahaanId2 && k.NomorIndukKaryawan == "MBD-2026-001");

        if (userHrd2 != null && kry2 != null && kry2.PenggunaId == null)
        {
            kry2.PenggunaId = userHrd2.Id;
            context.Karyawan.Update(kry2);
            await context.SaveChangesAsync();
        }

        // 4. Inisialisasi Saldo Cuti 2026
        var jenisCutiTahunan1 = await context.JenisCuti
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(j => j.PerusahaanId == perusahaanId1 && j.KodeJenisCuti == "CT-THN");

        if (jenisCutiTahunan1 != null && kry1 != null)
        {
            var adaSaldo1 = await context.SaldoCuti
                .IgnoreQueryFilters()
                .AnyAsync(s => s.PerusahaanId == perusahaanId1 && s.KaryawanId == kry1.Id && s.JenisCutiId == jenisCutiTahunan1.Id && s.PeriodeTahun == 2026);

            if (!adaSaldo1)
            {
                await context.SaldoCuti.AddAsync(new SaldoCuti
                {
                    Id = Guid.NewGuid(),
                    PerusahaanId = perusahaanId1,
                    KaryawanId = kry1.Id,
                    JenisCutiId = jenisCutiTahunan1.Id,
                    PeriodeTahun = 2026,
                    JumlahHakCuti = 12,
                    CutiTerpakai = 0,
                    SisaCuti = 12,
                    Catatan = "Hak cuti tahunan periode 2026",
                    CreatedAt = DateTimeOffset.UtcNow,
                    CreatedBy = "Sistem"
                });
            }
        }

        var jenisCutiTahunan2 = await context.JenisCuti
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(j => j.PerusahaanId == perusahaanId2 && j.KodeJenisCuti == "CT-THN");

        if (jenisCutiTahunan2 != null && kry2 != null)
        {
            var adaSaldo2 = await context.SaldoCuti
                .IgnoreQueryFilters()
                .AnyAsync(s => s.PerusahaanId == perusahaanId2 && s.KaryawanId == kry2.Id && s.JenisCutiId == jenisCutiTahunan2.Id && s.PeriodeTahun == 2026);

            if (!adaSaldo2)
            {
                await context.SaldoCuti.AddAsync(new SaldoCuti
                {
                    Id = Guid.NewGuid(),
                    PerusahaanId = perusahaanId2,
                    KaryawanId = kry2.Id,
                    JenisCutiId = jenisCutiTahunan2.Id,
                    PeriodeTahun = 2026,
                    JumlahHakCuti = 12,
                    CutiTerpakai = 0,
                    SisaCuti = 12,
                    Catatan = "Hak cuti tahunan periode 2026",
                    CreatedAt = DateTimeOffset.UtcNow,
                    CreatedBy = "Sistem"
                });
            }
        }

        await context.SaveChangesAsync();

        // 14. Inisialisasi Modul Penggajian (Kebijakan, Komponen Gaji, & Gaji Karyawan)
        await InisialisasiModulPenggajianAsync(context, perusahaanId1, perusahaanId2, kry1, kry2);
    }

    private static async Task InisialisasiModulPenggajianAsync(
        KonteksAplikasi context,
        Guid perusahaanId1,
        Guid perusahaanId2,
        Karyawan? kry1,
        Karyawan? kry2)
    {
        // 1. Kebijakan Penggajian Perusahaan 1
        var adaKebijakan1 = await context.KebijakanPenggajianPerusahaan
            .IgnoreQueryFilters()
            .AnyAsync(k => k.PerusahaanId == perusahaanId1);

        if (!adaKebijakan1)
        {
            await context.KebijakanPenggajianPerusahaan.AddAsync(new KebijakanPenggajianPerusahaan
            {
                Id = Guid.NewGuid(),
                PerusahaanId = perusahaanId1,
                HitungLemburOtomatis = true,
                TarifLemburPerJamFlat = 0,
                TerapkanPotonganKeterlambatan = true,
                ToleransiKeterlambatanGratisMenit = 15,
                DendaKeterlambatanPerMenit = 1000,
                TerapkanBpjsKesehatan = true,
                PersenBpjsKesehatanPerusahaan = 4.00m,
                PersenBpjsKesehatanPekerja = 1.00m,
                BatasMaksimalUpahBpjsKesehatan = 12000000m,
                TerapkanBpjsKetenagakerjaan = true,
                PersenBpjsJhtPerusahaan = 3.70m,
                PersenBpjsJhtPekerja = 2.00m,
                PersenBpjsJpPerusahaan = 2.00m,
                PersenBpjsJpPekerja = 1.00m,
                BatasMaksimalUpahBpjsJp = 10042300m,
                PersenBpjsJkkPerusahaan = 0.24m,
                PersenBpjsJkmPerusahaan = 0.30m,
                TerapkanPph21 = true,
                MetodePph21 = MetodePajakPPh21.TER_PP58,
                TanggalCutoffAwal = 26,
                TanggalCutoffAkhir = 25,
                TanggalPembayaranGaji = 28,
                CreatedAt = DateTimeOffset.UtcNow,
                CreatedBy = "Sistem"
            });
        }

        // Kebijakan Perusahaan 2
        var adaKebijakan2 = await context.KebijakanPenggajianPerusahaan
            .IgnoreQueryFilters()
            .AnyAsync(k => k.PerusahaanId == perusahaanId2);

        if (!adaKebijakan2)
        {
            await context.KebijakanPenggajianPerusahaan.AddAsync(new KebijakanPenggajianPerusahaan
            {
                Id = Guid.NewGuid(),
                PerusahaanId = perusahaanId2,
                HitungLemburOtomatis = true,
                TarifLemburPerJamFlat = 0,
                TerapkanPotonganKeterlambatan = false,
                ToleransiKeterlambatanGratisMenit = 30,
                DendaKeterlambatanPerMenit = 0,
                TerapkanBpjsKesehatan = true,
                TerapkanBpjsKetenagakerjaan = true,
                TerapkanPph21 = true,
                MetodePph21 = MetodePajakPPh21.TER_PP58,
                CreatedAt = DateTimeOffset.UtcNow,
                CreatedBy = "Sistem"
            });
        }

        // 2. Master Komponen Gaji Perusahaan 1
        var komponenBaku = new (string Kode, string Nama, JenisKomponenGaji Jenis, SifatKomponenGaji Sifat, KategoriKomponenGaji Kategori, decimal Nilai, bool KenaPajak, bool MasukBpjs, bool AutoSistem)[]
        {
            ("GP", "Gaji Pokok", JenisKomponenGaji.Pendapatan, SifatKomponenGaji.Tetap, KategoriKomponenGaji.GajiPokok, 0, true, true, false),
            ("TJ-JAB", "Tunjangan Jabatan", JenisKomponenGaji.Pendapatan, SifatKomponenGaji.Tetap, KategoriKomponenGaji.TunjanganTetap, 0, true, true, false),
            ("TJ-TRN", "Tunjangan Transportasi", JenisKomponenGaji.Pendapatan, SifatKomponenGaji.Tetap, KategoriKomponenGaji.TunjanganTetap, 0, true, false, false),
            ("TJ-MKN", "Tunjangan Makan", JenisKomponenGaji.Pendapatan, SifatKomponenGaji.TidakTetap, KategoriKomponenGaji.TunjanganTidakTetap, 0, true, false, false),
            ("LMBR", "Upah Lembur", JenisKomponenGaji.Pendapatan, SifatKomponenGaji.TidakTetap, KategoriKomponenGaji.UangLembur, 0, true, false, true),
            ("BPJS-KES", "BPJS Kesehatan (Pekerja 1%)", JenisKomponenGaji.Potongan, SifatKomponenGaji.Tetap, KategoriKomponenGaji.BpjsKesehatan, 0, false, false, true),
            ("BPJS-TK-JHT", "BPJS JHT (Pekerja 2%)", JenisKomponenGaji.Potongan, SifatKomponenGaji.Tetap, KategoriKomponenGaji.BpjsKetenagakerjaanJHT, 0, false, false, true),
            ("BPJS-TK-JP", "BPJS JP (Pekerja 1%)", JenisKomponenGaji.Potongan, SifatKomponenGaji.Tetap, KategoriKomponenGaji.BpjsKetenagakerjaanJP, 0, false, false, true),
            ("PPH21", "Pajak Penghasilan (PPh 21)", JenisKomponenGaji.Potongan, SifatKomponenGaji.TidakTetap, KategoriKomponenGaji.PajakPPh21, 0, false, false, true),
            ("POT-TELAT", "Potongan Keterlambatan", JenisKomponenGaji.Potongan, SifatKomponenGaji.TidakTetap, KategoriKomponenGaji.PotonganKeterlambatan, 0, false, false, true)
        };

        foreach (var k in komponenBaku)
        {
            var ada = await context.KomponenGaji
                .IgnoreQueryFilters()
                .AnyAsync(kg => kg.PerusahaanId == perusahaanId1 && kg.KodeKomponen == k.Kode);

            if (!ada)
            {
                await context.KomponenGaji.AddAsync(new KomponenGaji
                {
                    Id = Guid.NewGuid(),
                    PerusahaanId = perusahaanId1,
                    KodeKomponen = k.Kode,
                    NamaKomponen = k.Nama,
                    Jenis = k.Jenis,
                    Sifat = k.Sifat,
                    Kategori = k.Kategori,
                    NilaiBawaan = k.Nilai,
                    KenaPajak = k.KenaPajak,
                    MasukKalkulasiBpjs = k.MasukBpjs,
                    DihitungOtomatisOlehSistem = k.AutoSistem,
                    StatusAktif = true,
                    CreatedAt = DateTimeOffset.UtcNow,
                    CreatedBy = "Sistem"
                });
            }
        }

        // Simpan komponen gaji terlebih dahulu
        await context.SaveChangesAsync();

        // 3. Konfigurasi Gaji Karyawan Rian Pratama (Software Engineer)
        if (kry1 != null)
        {
            kry1.StatusPTKP = StatusPTKP.TK0;
            kry1.NamaBank = "Bank Central Asia (BCA)";
            kry1.NomorRekeningBank = "8830123456";
            kry1.NomorNPWP = "98.765.432.1-012.000";
            kry1.NomorBpjsKesehatan = "0001234567890";
            kry1.NomorBpjsKetenagakerjaan = "18012345678";
            context.Karyawan.Update(kry1);

            var komponenList = await context.KomponenGaji
                .IgnoreQueryFilters()
                .Where(kg => kg.PerusahaanId == perusahaanId1)
                .ToListAsync();

            var gp = komponenList.FirstOrDefault(k => k.KodeKomponen == "GP");
            var tjJab = komponenList.FirstOrDefault(k => k.KodeKomponen == "TJ-JAB");
            var tjTrn = komponenList.FirstOrDefault(k => k.KodeKomponen == "TJ-TRN");
            var tjMkn = komponenList.FirstOrDefault(k => k.KodeKomponen == "TJ-MKN");

            var konfigurasiKry1 = new (KomponenGaji? Komponen, decimal Nominal)[]
            {
                (gp, 8000000m),
                (tjJab, 1500000m),
                (tjTrn, 500000m),
                (tjMkn, 500000m)
            };

            foreach (var cfg in konfigurasiKry1)
            {
                if (cfg.Komponen == null) continue;
                var adaGaji = await context.GajiKaryawan
                    .IgnoreQueryFilters()
                    .AnyAsync(g => g.PerusahaanId == perusahaanId1 && g.KaryawanId == kry1.Id && g.KomponenGajiId == cfg.Komponen.Id);

                if (!adaGaji)
                {
                    await context.GajiKaryawan.AddAsync(new GajiKaryawan
                    {
                        Id = Guid.NewGuid(),
                        PerusahaanId = perusahaanId1,
                        KaryawanId = kry1.Id,
                        KomponenGajiId = cfg.Komponen.Id,
                        Nominal = cfg.Nominal,
                        StatusAktif = true,
                        CreatedAt = DateTimeOffset.UtcNow,
                        CreatedBy = "Sistem"
                    });
                }
            }
        }

        // Manager Engineering (Ahmad Fauzi)
        var mgr1 = await context.Karyawan
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(k => k.PerusahaanId == perusahaanId1 && k.Email == "manager@solusihr.id");

        if (mgr1 != null)
        {
            mgr1.StatusPTKP = StatusPTKP.K1;
            mgr1.NamaBank = "Bank Mandiri";
            mgr1.NomorRekeningBank = "1370001234567";
            mgr1.NomorNPWP = "87.654.321.0-012.000";
            mgr1.NomorBpjsKesehatan = "0009876543210";
            mgr1.NomorBpjsKetenagakerjaan = "18098765432";
            context.Karyawan.Update(mgr1);

            var komponenList = await context.KomponenGaji
                .IgnoreQueryFilters()
                .Where(kg => kg.PerusahaanId == perusahaanId1)
                .ToListAsync();

            var gp = komponenList.FirstOrDefault(k => k.KodeKomponen == "GP");
            var tjJab = komponenList.FirstOrDefault(k => k.KodeKomponen == "TJ-JAB");
            var tjTrn = komponenList.FirstOrDefault(k => k.KodeKomponen == "TJ-TRN");
            var tjMkn = komponenList.FirstOrDefault(k => k.KodeKomponen == "TJ-MKN");

            var konfigurasiMgr = new (KomponenGaji? Komponen, decimal Nominal)[]
            {
                (gp, 14000000m),
                (tjJab, 4000000m),
                (tjTrn, 1000000m),
                (tjMkn, 1000000m)
            };

            foreach (var cfg in konfigurasiMgr)
            {
                if (cfg.Komponen == null) continue;
                var adaGaji = await context.GajiKaryawan
                    .IgnoreQueryFilters()
                    .AnyAsync(g => g.PerusahaanId == perusahaanId1 && g.KaryawanId == mgr1.Id && g.KomponenGajiId == cfg.Komponen.Id);

                if (!adaGaji)
                {
                    await context.GajiKaryawan.AddAsync(new GajiKaryawan
                    {
                        Id = Guid.NewGuid(),
                        PerusahaanId = perusahaanId1,
                        KaryawanId = mgr1.Id,
                        KomponenGajiId = cfg.Komponen.Id,
                        Nominal = cfg.Nominal,
                        StatusAktif = true,
                        CreatedAt = DateTimeOffset.UtcNow,
                        CreatedBy = "Sistem"
                    });
                }
            }
        }

        // ==========================================
        // 4. Inisialisasi Data Modul Rekrutmen (Fase 5)
        // ==========================================
        var adaLowongan = await context.LowonganKerja
            .IgnoreQueryFilters()
            .AnyAsync(l => l.PerusahaanId == perusahaanId1 && l.KodeLowongan == "SR-DEV-2026");

        if (!adaLowongan)
        {
            var deptIT = await context.Departemen.IgnoreQueryFilters().FirstOrDefaultAsync(d => d.PerusahaanId == perusahaanId1 && d.KodeDepartemen == "IT");
            var jabIT = await context.Jabatan.IgnoreQueryFilters().FirstOrDefaultAsync(j => j.PerusahaanId == perusahaanId1 && j.NamaJabatan == "Senior Software Engineer");

            var lowonganSeed = new LowonganKerja
            {
                Id = Guid.NewGuid(),
                PerusahaanId = perusahaanId1,
                KodeLowongan = "SR-DEV-2026",
                JudulLowongan = "Senior .NET Core Backend Engineer",
                DepartemenId = deptIT?.Id,
                JabatanId = jabIT?.Id,
                DeskripsiPekerjaan = "Bertanggung jawab atas arsitektur backend, REST API, performa basis data, dan modul enterprise SistemHRD.",
                Kualifikasi = "Pengalaman minimal 3 tahun dengan C#, .NET Core/NET 10, Entity Framework Core, MySQL/SQL Server, serta arsitektur clean monolith.",
                TipePekerjaan = TipePekerjaan.PenuhWaktu,
                LokasiKerja = "Jakarta Selatan (Hybrid)",
                JumlahDibutuhkan = 2,
                GajiMin = 12000000m,
                GajiMax = 18000000m,
                TanggalDibuka = DateTime.Today.AddDays(-14),
                TanggalDitutup = DateTime.Today.AddMonths(1),
                Status = StatusLowongan.Dibuka,
                CreatedAt = DateTimeOffset.UtcNow,
                CreatedBy = "Sistem"
            };

            var tahapanBaku = new (string Nama, JenisTahapanSeleksi Jenis, int Urutan)[]
            {
                ("Seleksi Administrasi & Berkas", JenisTahapanSeleksi.Administrasi, 1),
                ("Wawancara HRD & Kepribadian", JenisTahapanSeleksi.WawancaraHR, 2),
                ("Wawancara Teknis / User", JenisTahapanSeleksi.WawancaraUser, 3),
                ("Penawaran Kerja (Offering)", JenisTahapanSeleksi.Offering, 4)
            };

            foreach (var t in tahapanBaku)
            {
                lowonganSeed.DaftarTahapanSeleksi.Add(new TahapanSeleksi
                {
                    Id = Guid.NewGuid(),
                    PerusahaanId = perusahaanId1,
                    LowonganKerjaId = lowonganSeed.Id,
                    NamaTahapan = t.Nama,
                    JenisTahapan = t.Jenis,
                    Urutan = t.Urutan,
                    StatusAktif = true,
                    CreatedAt = DateTimeOffset.UtcNow,
                    CreatedBy = "Sistem"
                });
            }

            await context.LowonganKerja.AddAsync(lowonganSeed);
        }

        // ==========================================
        // 5. Inisialisasi Data Modul Penilaian Kinerja (Fase 5)
        // ==========================================
        // A. Indikator KPI Baku Perusahaan 1
        var indikatorBaku = new (string Kode, string Nama, string Deskripsi, JenisIndikatorKPI Jenis, decimal Target, string Satuan, decimal Bobot)[]
        {
            ("KPI-PRD-01", "Pencapaian Target Kerja & Deliverable Proyek", "Menyelesaikan fitur aplikasi dan backlog tugas sesuai estimasi timeline sprint.", JenisIndikatorKPI.Kuantitatif, 100m, "%", 35.00m),
            ("KPI-QLT-02", "Kualitas Kode & Standar Pekerjaan", "Mematuhi clean code standards, bebas regresi kritis, dan melengkapi dokumentasi teknis.", JenisIndikatorKPI.Kualitatif, 95m, "Poin", 25.00m),
            ("KPI-DIS-03", "Kedisiplinan, Kehadiran & SLA Waktu", "Ketepatan waktu kehadiran presensi kerja serta responsivitas penanganan isu operasional.", JenisIndikatorKPI.Kuantitatif, 100m, "%", 20.00m),
            ("KPI-COL-04", "Kolaborasi Tim, Komunikasi & Inisiatif", "Keaktifan berkoordinasi antardivisi, proaktif memecahkan masalah, dan membantu rekan kerja.", JenisIndikatorKPI.Perilaku, 90m, "Skor", 20.00m)
        };

        foreach (var kpi in indikatorBaku)
        {
            var adaKpi = await context.IndikatorKPI
                .IgnoreQueryFilters()
                .AnyAsync(i => i.PerusahaanId == perusahaanId1 && i.KodeIndikator == kpi.Kode);

            if (!adaKpi)
            {
                await context.IndikatorKPI.AddAsync(new IndikatorKPI
                {
                    Id = Guid.NewGuid(),
                    PerusahaanId = perusahaanId1,
                    KodeIndikator = kpi.Kode,
                    NamaIndikator = kpi.Nama,
                    Deskripsi = kpi.Deskripsi,
                    Jenis = kpi.Jenis,
                    TargetKuantitatif = kpi.Target,
                    Satuan = kpi.Satuan,
                    BobotBawaan = kpi.Bobot,
                    StatusAktif = true,
                    CreatedAt = DateTimeOffset.UtcNow,
                    CreatedBy = "Sistem"
                });
            }
        }

        // B. Periode Penilaian Kinerja Aktif
        var adaPeriode = await context.PeriodePenilaian
            .IgnoreQueryFilters()
            .AnyAsync(p => p.PerusahaanId == perusahaanId1 && p.NamaPeriode == "Penilaian Kinerja Semester 2 - 2026");

        if (!adaPeriode)
        {
            await context.PeriodePenilaian.AddAsync(new PeriodePenilaian
            {
                Id = Guid.NewGuid(),
                PerusahaanId = perusahaanId1,
                NamaPeriode = "Penilaian Kinerja Semester 2 - 2026",
                Tahun = 2026,
                SemesterAtauKuartal = "Semester 2",
                TanggalMulai = new DateTime(2026, 7, 1),
                TanggalSelesai = new DateTime(2026, 12, 31),
                Status = StatusPeriodePenilaian.Aktif,
                Catatan = "Siklus evaluasi performa kerja reguler semester genap tahun 2026.",
                CreatedAt = DateTimeOffset.UtcNow,
                CreatedBy = "Sistem"
            });
        }

        // ==========================================
        // PHASE 6: INITIAL DATA PELATIHAN & ASET
        // ==========================================
        var karyawanRian = await context.Karyawan
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(k => k.PerusahaanId == perusahaanId1 && k.NomorIndukKaryawan == "KRY001");

        // 1. Program Pelatihan
        var program1 = await context.ProgramPelatihan
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(p => p.PerusahaanId == perusahaanId1 && p.KodeProgram == "TRN-2026-LDR01");

        if (program1 == null)
        {
            program1 = new ProgramPelatihan
            {
                Id = Guid.NewGuid(),
                PerusahaanId = perusahaanId1,
                KodeProgram = "TRN-2026-LDR01",
                NamaProgram = "Kepemimpinan & Manajemen Kolaborasi Tim",
                Deskripsi = "Pelatihan esensial peningkatan kompetensi manajerial, delegasi tugas efektif, dan resolusi konflik tim.",
                Penyelenggara = "Divisi People Development",
                Instruktur = "Dr. Budi Santoso, M.Psi",
                Metode = MetodePelatihan.InHouse,
                LokasiAtauTautan = "Ruang Pelatihan Utama Lt. 15",
                TanggalMulai = DateTime.Today.AddDays(-5),
                TanggalSelesai = DateTime.Today.AddDays(10),
                TotalJam = 16.0m,
                BiayaPerPeserta = 500000m,
                MaksimalPeserta = 20,
                Status = StatusPelatihan.Berlangsung,
                CreatedAt = DateTimeOffset.UtcNow,
                CreatedBy = "Sistem"
            };
            await context.ProgramPelatihan.AddAsync(program1);
        }

        var program2 = await context.ProgramPelatihan
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(p => p.PerusahaanId == perusahaanId1 && p.KodeProgram == "TRN-2026-SEC02");

        if (program2 == null)
        {
            program2 = new ProgramPelatihan
            {
                Id = Guid.NewGuid(),
                PerusahaanId = perusahaanId1,
                KodeProgram = "TRN-2026-SEC02",
                NamaProgram = "Keamanan Informasi & Kepatuhan ISO 27001",
                Deskripsi = "Sosialisasi keamanan siber, manajemen kata sandi, pencegahan phishing, dan kepatuhan privasi data perusahaan.",
                Penyelenggara = "Divisi Teknologi & Keamanan",
                Instruktur = "Agus Pratama, CISSP",
                Metode = MetodePelatihan.Daring,
                LokasiAtauTautan = "https://meet.google.com/solusi-security",
                TanggalMulai = DateTime.Today.AddDays(-30),
                TanggalSelesai = DateTime.Today.AddDays(-28),
                TotalJam = 8.0m,
                BiayaPerPeserta = 0m,
                MaksimalPeserta = 50,
                Status = StatusPelatihan.Selesai,
                CreatedAt = DateTimeOffset.UtcNow,
                CreatedBy = "Sistem"
            };
            await context.ProgramPelatihan.AddAsync(program2);

            // Daftarkan karyawan Rian dan terbitkan sertifikat kelulusan
            if (karyawanRian != null)
            {
                var pesertaRian = new PesertaPelatihan
                {
                    Id = Guid.NewGuid(),
                    PerusahaanId = perusahaanId1,
                    ProgramPelatihanId = program2.Id,
                    KaryawanId = karyawanRian.Id,
                    StatusKeikutsertaan = StatusKeikutsertaan.Lulus,
                    NilaiAkhir = 92.5m,
                    Catatan = "Sangat aktif berdiskusi dan lulus ujian teknis keamanan informasi dengan predikat memuaskan.",
                    TanggalDidaftarkan = DateTime.Today.AddDays(-35),
                    TanggalSelesai = DateTime.Today.AddDays(-28),
                    CreatedAt = DateTimeOffset.UtcNow,
                    CreatedBy = "Sistem"
                };
                await context.PesertaPelatihan.AddAsync(pesertaRian);

                var sertifikatRian = new SertifikatPelatihan
                {
                    Id = Guid.NewGuid(),
                    PerusahaanId = perusahaanId1,
                    PesertaPelatihanId = pesertaRian.Id,
                    NomorSertifikat = "CERT-202610-0001",
                    JudulSertifikat = "Sertifikat Kelulusan Keamanan Informasi & ISO 27001",
                    TanggalTerbit = DateTime.Today.AddDays(-27),
                    PenerbitSertifikat = "Divisi People Development",
                    KodeVerifikasi = "VERIF-SEC-928371",
                    Catatan = "Sertifikat resmi kompetensi keamanan digital karyawan.",
                    CreatedAt = DateTimeOffset.UtcNow,
                    CreatedBy = "Sistem"
                };
                await context.SertifikatPelatihan.AddAsync(sertifikatRian);
            }
        }

        // 2. Inventaris Aset Perusahaan
        var asetLaptop = await context.AsetPerusahaan
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(a => a.PerusahaanId == perusahaanId1 && a.NomorAset == "AST-2026-NB001");

        if (asetLaptop == null)
        {
            asetLaptop = new AsetPerusahaan
            {
                Id = Guid.NewGuid(),
                PerusahaanId = perusahaanId1,
                NomorAset = "AST-2026-NB001",
                NamaAset = "Laptop Lenovo ThinkPad T14 Gen 4",
                Kategori = KategoriAset.ElektronikLaptop,
                MerkModel = "Lenovo ThinkPad T14",
                NomorSeri = "PF49XK21",
                TanggalPembelian = new DateTime(2026, 1, 15),
                NilaiPerolehan = 18500000m,
                Kondisi = KondisiAset.SangatBaik,
                Status = StatusAset.Digunakan,
                LokasiPenyimpanan = "Gudang IT Lt. 2",
                DeskripsiSpesifikasi = "Core i7-1355U, 32GB DDR5, 1TB NVMe, Layar 14 Inch WUXGA IPS, Windows 11 Pro.",
                PemegangSaatIniId = karyawanRian?.Id,
                CreatedAt = DateTimeOffset.UtcNow,
                CreatedBy = "Sistem"
            };
            await context.AsetPerusahaan.AddAsync(asetLaptop);

            if (karyawanRian != null)
            {
                var userHrd = await userManager.FindByEmailAsync("hrd@solusihr.id");
                var penyerahanLaptop = new PenyerahanAset
                {
                    Id = Guid.NewGuid(),
                    PerusahaanId = perusahaanId1,
                    AsetPerusahaanId = asetLaptop.Id,
                    KaryawanId = karyawanRian.Id,
                    NomorDokumen = "BAST-20260120-001",
                    TanggalPenyerahan = new DateTime(2026, 1, 20),
                    KondisiSaatPenyerahan = KondisiAset.SangatBaik,
                    StatusPeminjaman = "Aktif",
                    KelengkapanAset = "Unit Laptop, Adaptor 65W USB-C, Tas Lenovo ThinkPad, Mouse Wireless",
                    CatatanPenyerahan = "Penyerahan fasilitas laptop kerja utama karyawan.",
                    DiserahkanOlehId = userHrd?.Id ?? Guid.NewGuid(),
                    CreatedAt = DateTimeOffset.UtcNow,
                    CreatedBy = "Sistem"
                };
                await context.PenyerahanAset.AddAsync(penyerahanLaptop);

                await context.RiwayatAset.AddAsync(new RiwayatAset
                {
                    Id = Guid.NewGuid(),
                    PerusahaanId = perusahaanId1,
                    AsetPerusahaanId = asetLaptop.Id,
                    TipeAktivitas = TipeAktivitasAset.Penyerahan,
                    TanggalAktivitas = new DateTime(2026, 1, 20),
                    KaryawanTerkaitId = karyawanRian.Id,
                    NamaPetugas = "HRD & IT Support",
                    KondisiSebelumnya = KondisiAset.SangatBaik,
                    KondisiTerbaru = KondisiAset.SangatBaik,
                    Deskripsi = $"Penyerahan aset kepada {karyawanRian.NamaLengkap} (BAST: BAST-20260120-001).",
                    CreatedAt = DateTimeOffset.UtcNow,
                    CreatedBy = "Sistem"
                });
            }
        }

        var asetMonitor = await context.AsetPerusahaan
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(a => a.PerusahaanId == perusahaanId1 && a.NomorAset == "AST-2026-MON01");

        if (asetMonitor == null)
        {
            await context.AsetPerusahaan.AddAsync(new AsetPerusahaan
            {
                Id = Guid.NewGuid(),
                PerusahaanId = perusahaanId1,
                NomorAset = "AST-2026-MON01",
                NamaAset = "Monitor Dell UltraSharp 27 Inch 4K",
                Kategori = KategoriAset.ElektronikLaptop,
                MerkModel = "Dell U2723QE",
                NomorSeri = "CN-08K912-74261",
                TanggalPembelian = new DateTime(2026, 2, 10),
                NilaiPerolehan = 8200000m,
                Kondisi = KondisiAset.SangatBaik,
                Status = StatusAset.Tersedia,
                LokasiPenyimpanan = "Gudang IT Lt. 2, Rak Aksesoris B",
                DeskripsiSpesifikasi = "Panel IPS Black 4K UHD 3840x2160, USB-C Hub 90W PD, RJ45 Ethernet.",
                CreatedAt = DateTimeOffset.UtcNow,
                CreatedBy = "Sistem"
            });
        }

        var asetMobil = await context.AsetPerusahaan
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(a => a.PerusahaanId == perusahaanId1 && a.NomorAset == "AST-2026-VEH01");

        if (asetMobil == null)
        {
            await context.AsetPerusahaan.AddAsync(new AsetPerusahaan
            {
                Id = Guid.NewGuid(),
                PerusahaanId = perusahaanId1,
                NomorAset = "AST-2026-VEH01",
                NamaAset = "Mobil Operasional Toyota Veloz 1.5 Q CVT",
                Kategori = KategoriAset.KendaraanOperasional,
                MerkModel = "Toyota Veloz 1.5 Q CVT",
                NomorSeri = "B 1234 SOL",
                TanggalPembelian = new DateTime(2025, 11, 20),
                NilaiPerolehan = 295000000m,
                Kondisi = KondisiAset.Baik,
                Status = StatusAset.Tersedia,
                LokasiPenyimpanan = "Parkir Basemen Gedung Cyber 2 No. B1-14",
                DeskripsiSpesifikasi = "Warna Putih Mutiara, Nomor Polisi B 1234 SOL, Asuransi All Risk aktif hingga Nov 2027.",
                CreatedAt = DateTimeOffset.UtcNow,
                CreatedBy = "Sistem"
            });
        }

        await context.SaveChangesAsync();
    }
}
