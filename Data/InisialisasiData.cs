using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SistemHRD.Common.Constants;
using SistemHRD.Models.Entitas;

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
}
