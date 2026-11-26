using System.Linq.Expressions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SistemHRD.Models.Entitas;
using SistemHRD.Services.Interfaces;

namespace SistemHRD.Data;

/// <summary>
/// DbContext utama aplikasi SistemHRD yang mengelola ASP.NET Core Identity,
/// Modul Karyawan, Departemen, Jabatan, Global Query Filter (Multi-Tenant & Soft Delete),
/// dan otomatisasi Audit Trail.
/// </summary>
public class KonteksAplikasi : IdentityDbContext<Pengguna, Peran, Guid>
{
    private readonly ILayananTenant? _layananTenant;

    public KonteksAplikasi(
        DbContextOptions<KonteksAplikasi> options,
        ILayananTenant? layananTenant = null)
        : base(options)
    {
        _layananTenant = layananTenant;
    }

    // Master Tenant & Relasi
    public DbSet<Perusahaan> Perusahaan => Set<Perusahaan>();
    public DbSet<PenggunaPerusahaan> PenggunaPerusahaan => Set<PenggunaPerusahaan>();
    public DbSet<AuditLog> AuditLog => Set<AuditLog>();

    // Modul Karyawan & Organisasi
    public DbSet<Departemen> Departemen => Set<Departemen>();
    public DbSet<Jabatan> Jabatan => Set<Jabatan>();
    public DbSet<Karyawan> Karyawan => Set<Karyawan>();
    public DbSet<KontrakKerja> KontrakKerja => Set<KontrakKerja>();
    public DbSet<DokumenKaryawan> DokumenKaryawan => Set<DokumenKaryawan>();
    public DbSet<KontakDaruratKaryawan> KontakDaruratKaryawan => Set<KontakDaruratKaryawan>();
    public DbSet<RiwayatPekerjaan> RiwayatPekerjaan => Set<RiwayatPekerjaan>();

    // Modul Kehadiran
    public DbSet<ShiftKerja> ShiftKerja => Set<ShiftKerja>();
    public DbSet<JadwalKerja> JadwalKerja => Set<JadwalKerja>();
    public DbSet<Kehadiran> Kehadiran => Set<Kehadiran>();
    public DbSet<PengajuanLembur> PengajuanLembur => Set<PengajuanLembur>();

    // Modul Cuti
    public DbSet<JenisCuti> JenisCuti => Set<JenisCuti>();
    public DbSet<SaldoCuti> SaldoCuti => Set<SaldoCuti>();
    public DbSet<PengajuanCuti> PengajuanCuti => Set<PengajuanCuti>();
    public DbSet<PersetujuanCuti> PersetujuanCuti => Set<PersetujuanCuti>();

    // Modul Penggajian
    public DbSet<KomponenGaji> KomponenGaji => Set<KomponenGaji>();
    public DbSet<GajiKaryawan> GajiKaryawan => Set<GajiKaryawan>();
    public DbSet<KebijakanPenggajianPerusahaan> KebijakanPenggajianPerusahaan => Set<KebijakanPenggajianPerusahaan>();
    public DbSet<PeriodePenggajian> PeriodePenggajian => Set<PeriodePenggajian>();
    public DbSet<Penggajian> Penggajian => Set<Penggajian>();
    public DbSet<DetailPenggajian> DetailPenggajian => Set<DetailPenggajian>();
    public DbSet<SlipGaji> SlipGaji => Set<SlipGaji>();

    // Modul Rekrutmen
    public DbSet<LowonganKerja> LowonganKerja => Set<LowonganKerja>();
    public DbSet<Pelamar> Pelamar => Set<Pelamar>();
    public DbSet<TahapanSeleksi> TahapanSeleksi => Set<TahapanSeleksi>();
    public DbSet<JadwalWawancara> JadwalWawancara => Set<JadwalWawancara>();
    public DbSet<HasilSeleksi> HasilSeleksi => Set<HasilSeleksi>();

    // Modul Penilaian Kinerja
    public DbSet<PeriodePenilaian> PeriodePenilaian => Set<PeriodePenilaian>();
    public DbSet<IndikatorKPI> IndikatorKPI => Set<IndikatorKPI>();
    public DbSet<PenilaianKinerja> PenilaianKinerja => Set<PenilaianKinerja>();
    public DbSet<DetailPenilaian> DetailPenilaian => Set<DetailPenilaian>();

    // Modul Pelatihan
    public DbSet<ProgramPelatihan> ProgramPelatihan => Set<ProgramPelatihan>();
    public DbSet<PesertaPelatihan> PesertaPelatihan => Set<PesertaPelatihan>();
    public DbSet<SertifikatPelatihan> SertifikatPelatihan => Set<SertifikatPelatihan>();

    // Modul Manajemen Aset
    public DbSet<AsetPerusahaan> AsetPerusahaan => Set<AsetPerusahaan>();
    public DbSet<PenyerahanAset> PenyerahanAset => Set<PenyerahanAset>();
    public DbSet<RiwayatAset> RiwayatAset => Set<RiwayatAset>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Kustomisasi Penamaan Tabel Identity ke Bahasa Indonesia
        builder.Entity<Pengguna>(b =>
        {
            b.ToTable("Pengguna");
            b.Property(u => u.NamaLengkap).IsRequired().HasMaxLength(150);
            b.HasIndex(u => u.Email);
            b.HasIndex(u => u.PerusahaanId);

            b.HasQueryFilter(u =>
                !u.IsDeleted &&
                (_layananTenant == null ||
                 _layananTenant.IsSuperAdmin ||
                 _layananTenant.PerusahaanIdSaatIni == null ||
                 u.PerusahaanId == _layananTenant.PerusahaanIdSaatIni));
        });

        builder.Entity<Peran>(b =>
        {
            b.ToTable("Peran");
            b.Property(r => r.Deskripsi).HasMaxLength(255);
        });

        builder.Entity<IdentityUserRole<Guid>>().ToTable("PenggunaPeran");
        builder.Entity<IdentityUserClaim<Guid>>().ToTable("PenggunaKlaim");
        builder.Entity<IdentityUserLogin<Guid>>().ToTable("PenggunaLogin");
        builder.Entity<IdentityRoleClaim<Guid>>().ToTable("PeranKlaim");
        builder.Entity<IdentityUserToken<Guid>>().ToTable("PenggunaToken");

        // Konfigurasi Tabel Perusahaan
        builder.Entity<Perusahaan>(b =>
        {
            b.ToTable("Perusahaan");
            b.HasIndex(p => p.KodePerusahaan).IsUnique();
            b.HasIndex(p => p.StatusAktif);
            b.HasQueryFilter(p => !p.IsDeleted);
        });

        // Konfigurasi Tabel PenggunaPerusahaan
        builder.Entity<PenggunaPerusahaan>(b =>
        {
            b.ToTable("PenggunaPerusahaan");
            b.HasIndex(pp => new { pp.PenggunaId, pp.PerusahaanId }).IsUnique();
            b.HasOne(pp => pp.Pengguna)
                .WithMany(p => p.DaftarPenggunaPerusahaan)
                .HasForeignKey(pp => pp.PenggunaId)
                .OnDelete(DeleteBehavior.Cascade);
            b.HasOne(pp => pp.Perusahaan)
                .WithMany(p => p.DaftarPenggunaPerusahaan)
                .HasForeignKey(pp => pp.PerusahaanId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasQueryFilter(pp =>
                _layananTenant == null ||
                _layananTenant.IsSuperAdmin ||
                _layananTenant.PerusahaanIdSaatIni == null ||
                pp.PerusahaanId == _layananTenant.PerusahaanIdSaatIni);
        });

        // Konfigurasi Tabel AuditLog
        builder.Entity<AuditLog>(b =>
        {
            b.ToTable("AuditLog");
            b.HasIndex(a => a.PerusahaanId);
            b.HasIndex(a => a.Waktu);
            b.HasIndex(a => a.Aksi);
            b.HasOne(a => a.Perusahaan)
                .WithMany(p => p.DaftarAuditLog)
                .HasForeignKey(a => a.PerusahaanId)
                .OnDelete(DeleteBehavior.SetNull);
            b.HasOne(a => a.Pengguna)
                .WithMany()
                .HasForeignKey(a => a.PenggunaId)
                .OnDelete(DeleteBehavior.SetNull);

            b.HasQueryFilter(a =>
                _layananTenant == null ||
                _layananTenant.IsSuperAdmin ||
                _layananTenant.PerusahaanIdSaatIni == null ||
                a.PerusahaanId == _layananTenant.PerusahaanIdSaatIni);
        });

        // Konfigurasi Tabel Departemen
        builder.Entity<Departemen>(b =>
        {
            b.ToTable("Departemen");
            b.HasIndex(d => new { d.PerusahaanId, d.KodeDepartemen }).IsUnique();
            b.HasOne(d => d.KepalaDepartemen)
                .WithMany()
                .HasForeignKey(d => d.KepalaDepartemenId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // Konfigurasi Tabel Jabatan
        builder.Entity<Jabatan>(b =>
        {
            b.ToTable("Jabatan");
            b.HasIndex(j => new { j.PerusahaanId, j.DepartemenId, j.NamaJabatan });
            b.HasOne(j => j.Departemen)
                .WithMany(d => d.DaftarJabatan)
                .HasForeignKey(j => j.DepartemenId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Konfigurasi Tabel Karyawan
        builder.Entity<Karyawan>(b =>
        {
            b.ToTable("Karyawan");
            b.HasIndex(k => new { k.PerusahaanId, k.NomorIndukKaryawan }).IsUnique();
            b.HasIndex(k => new { k.PerusahaanId, k.DepartemenId });
            b.HasIndex(k => new { k.PerusahaanId, k.JabatanId });
            b.HasIndex(k => new { k.PerusahaanId, k.StatusKaryawan });

            b.HasOne(k => k.Departemen)
                .WithMany(d => d.DaftarKaryawan)
                .HasForeignKey(k => k.DepartemenId)
                .OnDelete(DeleteBehavior.Restrict);

            b.HasOne(k => k.Jabatan)
                .WithMany(j => j.DaftarKaryawan)
                .HasForeignKey(k => k.JabatanId)
                .OnDelete(DeleteBehavior.Restrict);

            b.HasOne(k => k.Pengguna)
                .WithMany()
                .HasForeignKey(k => k.PenggunaId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // Konfigurasi Tabel KontrakKerja
        builder.Entity<KontrakKerja>(b =>
        {
            b.ToTable("KontrakKerja");
            b.HasIndex(c => new { c.PerusahaanId, c.KaryawanId });
            b.HasOne(c => c.Karyawan)
                .WithMany(k => k.DaftarKontrakKerja)
                .HasForeignKey(c => c.KaryawanId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Konfigurasi Tabel DokumenKaryawan
        builder.Entity<DokumenKaryawan>(b =>
        {
            b.ToTable("DokumenKaryawan");
            b.HasIndex(d => new { d.PerusahaanId, d.KaryawanId });
            b.HasOne(d => d.Karyawan)
                .WithMany(k => k.DaftarDokumen)
                .HasForeignKey(d => d.KaryawanId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Konfigurasi Tabel KontakDaruratKaryawan
        builder.Entity<KontakDaruratKaryawan>(b =>
        {
            b.ToTable("KontakDaruratKaryawan");
            b.HasIndex(kd => new { kd.PerusahaanId, kd.KaryawanId });
            b.HasOne(kd => kd.Karyawan)
                .WithMany(k => k.DaftarKontakDarurat)
                .HasForeignKey(kd => kd.KaryawanId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Konfigurasi Tabel RiwayatPekerjaan
        builder.Entity<RiwayatPekerjaan>(b =>
        {
            b.ToTable("RiwayatPekerjaan");
            b.HasIndex(rp => new { rp.PerusahaanId, rp.KaryawanId });
            b.HasOne(rp => rp.Karyawan)
                .WithMany(k => k.DaftarRiwayatPekerjaan)
                .HasForeignKey(rp => rp.KaryawanId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Konfigurasi Tabel ShiftKerja
        builder.Entity<ShiftKerja>(b =>
        {
            b.ToTable("ShiftKerja");
            b.HasIndex(s => new { s.PerusahaanId, s.KodeShift }).IsUnique();
        });

        // Konfigurasi Tabel JadwalKerja
        builder.Entity<JadwalKerja>(b =>
        {
            b.ToTable("JadwalKerja");
            b.HasIndex(jk => new { jk.PerusahaanId, jk.KaryawanId, jk.Tanggal });
            b.HasOne(jk => jk.Karyawan)
                .WithMany(k => k.DaftarJadwalKerja)
                .HasForeignKey(jk => jk.KaryawanId)
                .OnDelete(DeleteBehavior.Cascade);
            b.HasOne(jk => jk.ShiftKerja)
                .WithMany(s => s.DaftarJadwalKerja)
                .HasForeignKey(jk => jk.ShiftKerjaId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Konfigurasi Tabel Kehadiran
        builder.Entity<Kehadiran>(b =>
        {
            b.ToTable("Kehadiran");
            b.HasIndex(kh => new { kh.PerusahaanId, kh.KaryawanId, kh.Tanggal }).IsUnique();
            b.HasIndex(kh => new { kh.PerusahaanId, kh.Tanggal });
            b.HasIndex(kh => new { kh.PerusahaanId, kh.Status });
            b.HasOne(kh => kh.Karyawan)
                .WithMany(k => k.DaftarKehadiran)
                .HasForeignKey(kh => kh.KaryawanId)
                .OnDelete(DeleteBehavior.Cascade);
            b.HasOne(kh => kh.ShiftKerja)
                .WithMany(s => s.DaftarKehadiran)
                .HasForeignKey(kh => kh.ShiftKerjaId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // Konfigurasi Tabel PengajuanLembur
        builder.Entity<PengajuanLembur>(b =>
        {
            b.ToTable("PengajuanLembur");
            b.HasIndex(l => new { l.PerusahaanId, l.KaryawanId, l.TanggalLembur });
            b.HasIndex(l => new { l.PerusahaanId, l.Status });
            b.HasOne(l => l.Karyawan)
                .WithMany(k => k.DaftarPengajuanLembur)
                .HasForeignKey(l => l.KaryawanId)
                .OnDelete(DeleteBehavior.Cascade);
            b.HasOne(l => l.DisetujuiOleh)
                .WithMany()
                .HasForeignKey(l => l.DisetujuiOlehId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // Konfigurasi Tabel JenisCuti
        builder.Entity<JenisCuti>(b =>
        {
            b.ToTable("JenisCuti");
            b.HasIndex(jc => new { jc.PerusahaanId, jc.KodeJenisCuti }).IsUnique();
        });

        // Konfigurasi Tabel SaldoCuti
        builder.Entity<SaldoCuti>(b =>
        {
            b.ToTable("SaldoCuti");
            b.HasIndex(sc => new { sc.PerusahaanId, sc.KaryawanId, sc.JenisCutiId, sc.PeriodeTahun }).IsUnique();
            b.HasOne(sc => sc.Karyawan)
                .WithMany(k => k.DaftarSaldoCuti)
                .HasForeignKey(sc => sc.KaryawanId)
                .OnDelete(DeleteBehavior.Cascade);
            b.HasOne(sc => sc.JenisCuti)
                .WithMany(jc => jc.DaftarSaldoCuti)
                .HasForeignKey(sc => sc.JenisCutiId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Konfigurasi Tabel PengajuanCuti
        builder.Entity<PengajuanCuti>(b =>
        {
            b.ToTable("PengajuanCuti");
            b.HasIndex(pc => new { pc.PerusahaanId, pc.KaryawanId });
            b.HasIndex(pc => new { pc.PerusahaanId, pc.Status });
            b.HasOne(pc => pc.Karyawan)
                .WithMany(k => k.DaftarPengajuanCuti)
                .HasForeignKey(pc => pc.KaryawanId)
                .OnDelete(DeleteBehavior.Cascade);
            b.HasOne(pc => pc.JenisCuti)
                .WithMany(jc => jc.DaftarPengajuanCuti)
                .HasForeignKey(pc => pc.JenisCutiId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Konfigurasi Tabel PersetujuanCuti
        builder.Entity<PersetujuanCuti>(b =>
        {
            b.ToTable("PersetujuanCuti");
            b.HasIndex(ap => new { ap.PerusahaanId, ap.PengajuanCutiId });
            b.HasOne(ap => ap.PengajuanCuti)
                .WithMany(pc => pc.DaftarPersetujuan)
                .HasForeignKey(ap => ap.PengajuanCutiId)
                .OnDelete(DeleteBehavior.Cascade);
            b.HasOne(ap => ap.Penyetuju)
                .WithMany()
                .HasForeignKey(ap => ap.PenyetujuId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Global Query Filter untuk Entitas Manajemen Karyawan (Soft Delete & Tenant Isolation)
        builder.Entity<Departemen>().HasQueryFilter(d =>
            !d.IsDeleted &&
            (_layananTenant == null || _layananTenant.IsSuperAdmin || _layananTenant.PerusahaanIdSaatIni == null || d.PerusahaanId == _layananTenant.PerusahaanIdSaatIni));

        builder.Entity<Jabatan>().HasQueryFilter(j =>
            !j.IsDeleted &&
            (_layananTenant == null || _layananTenant.IsSuperAdmin || _layananTenant.PerusahaanIdSaatIni == null || j.PerusahaanId == _layananTenant.PerusahaanIdSaatIni));

        builder.Entity<Karyawan>().HasQueryFilter(k =>
            !k.IsDeleted &&
            (_layananTenant == null || _layananTenant.IsSuperAdmin || _layananTenant.PerusahaanIdSaatIni == null || k.PerusahaanId == _layananTenant.PerusahaanIdSaatIni));

        builder.Entity<KontrakKerja>().HasQueryFilter(c =>
            !c.IsDeleted &&
            (_layananTenant == null || _layananTenant.IsSuperAdmin || _layananTenant.PerusahaanIdSaatIni == null || c.PerusahaanId == _layananTenant.PerusahaanIdSaatIni));

        builder.Entity<DokumenKaryawan>().HasQueryFilter(d =>
            !d.IsDeleted &&
            (_layananTenant == null || _layananTenant.IsSuperAdmin || _layananTenant.PerusahaanIdSaatIni == null || d.PerusahaanId == _layananTenant.PerusahaanIdSaatIni));

        builder.Entity<KontakDaruratKaryawan>().HasQueryFilter(kd =>
            !kd.IsDeleted &&
            (_layananTenant == null || _layananTenant.IsSuperAdmin || _layananTenant.PerusahaanIdSaatIni == null || kd.PerusahaanId == _layananTenant.PerusahaanIdSaatIni));

        builder.Entity<RiwayatPekerjaan>().HasQueryFilter(r =>
            !r.IsDeleted &&
            (_layananTenant == null || _layananTenant.IsSuperAdmin || _layananTenant.PerusahaanIdSaatIni == null || r.PerusahaanId == _layananTenant.PerusahaanIdSaatIni));

        // Global Query Filter untuk Modul Kehadiran & Cuti
        builder.Entity<ShiftKerja>().HasQueryFilter(s =>
            !s.IsDeleted &&
            (_layananTenant == null || _layananTenant.IsSuperAdmin || _layananTenant.PerusahaanIdSaatIni == null || s.PerusahaanId == _layananTenant.PerusahaanIdSaatIni));

        builder.Entity<JadwalKerja>().HasQueryFilter(jk =>
            !jk.IsDeleted &&
            (_layananTenant == null || _layananTenant.IsSuperAdmin || _layananTenant.PerusahaanIdSaatIni == null || jk.PerusahaanId == _layananTenant.PerusahaanIdSaatIni));

        builder.Entity<Kehadiran>().HasQueryFilter(kh =>
            !kh.IsDeleted &&
            (_layananTenant == null || _layananTenant.IsSuperAdmin || _layananTenant.PerusahaanIdSaatIni == null || kh.PerusahaanId == _layananTenant.PerusahaanIdSaatIni));

        builder.Entity<PengajuanLembur>().HasQueryFilter(l =>
            !l.IsDeleted &&
            (_layananTenant == null || _layananTenant.IsSuperAdmin || _layananTenant.PerusahaanIdSaatIni == null || l.PerusahaanId == _layananTenant.PerusahaanIdSaatIni));

        builder.Entity<JenisCuti>().HasQueryFilter(jc =>
            !jc.IsDeleted &&
            (_layananTenant == null || _layananTenant.IsSuperAdmin || _layananTenant.PerusahaanIdSaatIni == null || jc.PerusahaanId == _layananTenant.PerusahaanIdSaatIni));

        builder.Entity<SaldoCuti>().HasQueryFilter(sc =>
            !sc.IsDeleted &&
            (_layananTenant == null || _layananTenant.IsSuperAdmin || _layananTenant.PerusahaanIdSaatIni == null || sc.PerusahaanId == _layananTenant.PerusahaanIdSaatIni));

        builder.Entity<PengajuanCuti>().HasQueryFilter(pc =>
            !pc.IsDeleted &&
            (_layananTenant == null || _layananTenant.IsSuperAdmin || _layananTenant.PerusahaanIdSaatIni == null || pc.PerusahaanId == _layananTenant.PerusahaanIdSaatIni));

        builder.Entity<PersetujuanCuti>().HasQueryFilter(ap =>
            !ap.IsDeleted &&
            (_layananTenant == null || _layananTenant.IsSuperAdmin || _layananTenant.PerusahaanIdSaatIni == null || ap.PerusahaanId == _layananTenant.PerusahaanIdSaatIni));

        // Konfigurasi Tabel KomponenGaji
        builder.Entity<KomponenGaji>(b =>
        {
            b.ToTable("KomponenGaji");
            b.HasIndex(k => new { k.PerusahaanId, k.KodeKomponen }).IsUnique();
            b.HasIndex(k => new { k.PerusahaanId, k.Jenis });
        });

        // Konfigurasi Tabel GajiKaryawan
        builder.Entity<GajiKaryawan>(b =>
        {
            b.ToTable("GajiKaryawan");
            b.HasIndex(g => new { g.PerusahaanId, g.KaryawanId, g.KomponenGajiId }).IsUnique();
            b.HasOne(g => g.Karyawan)
                .WithMany(k => k.DaftarGajiKaryawan)
                .HasForeignKey(g => g.KaryawanId)
                .OnDelete(DeleteBehavior.Cascade);
            b.HasOne(g => g.KomponenGaji)
                .WithMany(k => k.DaftarGajiKaryawan)
                .HasForeignKey(g => g.KomponenGajiId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Konfigurasi Tabel KebijakanPenggajianPerusahaan
        builder.Entity<KebijakanPenggajianPerusahaan>(b =>
        {
            b.ToTable("KebijakanPenggajianPerusahaan");
            b.HasIndex(k => k.PerusahaanId).IsUnique();
        });

        // Konfigurasi Tabel PeriodePenggajian
        builder.Entity<PeriodePenggajian>(b =>
        {
            b.ToTable("PeriodePenggajian");
            b.HasIndex(p => new { p.PerusahaanId, p.Bulan, p.Tahun }).IsUnique();
            b.HasOne(p => p.DiajukanOleh)
                .WithMany()
                .HasForeignKey(p => p.DiajukanOlehId)
                .OnDelete(DeleteBehavior.SetNull);
            b.HasOne(p => p.DisetujuiOleh)
                .WithMany()
                .HasForeignKey(p => p.DisetujuiOlehId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // Konfigurasi Tabel Penggajian (Header Payroll)
        builder.Entity<Penggajian>(b =>
        {
            b.ToTable("Penggajian");
            b.HasIndex(p => new { p.PeriodePenggajianId, p.KaryawanId }).IsUnique();
            b.HasIndex(p => new { p.PerusahaanId, p.KaryawanId });
            b.HasIndex(p => new { p.PerusahaanId, p.NomorSlipGaji });
            b.HasOne(p => p.PeriodePenggajian)
                .WithMany(pp => pp.DaftarPenggajian)
                .HasForeignKey(p => p.PeriodePenggajianId)
                .OnDelete(DeleteBehavior.Cascade);
            b.HasOne(p => p.Karyawan)
                .WithMany(k => k.DaftarPenggajian)
                .HasForeignKey(p => p.KaryawanId)
                .OnDelete(DeleteBehavior.Restrict);
            b.HasOne(p => p.DisetujuiOleh)
                .WithMany()
                .HasForeignKey(p => p.DisetujuiOlehId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // Konfigurasi Tabel DetailPenggajian (Snapshot Komponen Payroll)
        builder.Entity<DetailPenggajian>(b =>
        {
            b.ToTable("DetailPenggajian");
            b.HasIndex(d => new { d.PenggajianId, d.Jenis });
            b.HasOne(d => d.Penggajian)
                .WithMany(p => p.DaftarDetailPenggajian)
                .HasForeignKey(d => d.PenggajianId)
                .OnDelete(DeleteBehavior.Cascade);
            b.HasOne(d => d.KomponenGaji)
                .WithMany(k => k.DaftarDetailPenggajian)
                .HasForeignKey(d => d.KomponenGajiId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // Konfigurasi Tabel SlipGaji
        builder.Entity<SlipGaji>(b =>
        {
            b.ToTable("SlipGaji");
            b.HasIndex(s => s.PenggajianId).IsUnique();
            b.HasIndex(s => new { s.PerusahaanId, s.NomorSlipGaji }).IsUnique();
            b.HasOne(s => s.Penggajian)
                .WithOne(p => p.SlipGaji)
                .HasForeignKey<SlipGaji>(s => s.PenggajianId)
                .OnDelete(DeleteBehavior.Cascade);
            b.HasOne(s => s.DiterbitkanOleh)
                .WithMany()
                .HasForeignKey(s => s.DiterbitkanOlehId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // Global Query Filter Modul Penggajian
        builder.Entity<KomponenGaji>().HasQueryFilter(k =>
            !k.IsDeleted &&
            (_layananTenant == null || _layananTenant.IsSuperAdmin || _layananTenant.PerusahaanIdSaatIni == null || k.PerusahaanId == _layananTenant.PerusahaanIdSaatIni));

        builder.Entity<GajiKaryawan>().HasQueryFilter(g =>
            !g.IsDeleted &&
            (_layananTenant == null || _layananTenant.IsSuperAdmin || _layananTenant.PerusahaanIdSaatIni == null || g.PerusahaanId == _layananTenant.PerusahaanIdSaatIni));

        builder.Entity<KebijakanPenggajianPerusahaan>().HasQueryFilter(k =>
            !k.IsDeleted &&
            (_layananTenant == null || _layananTenant.IsSuperAdmin || _layananTenant.PerusahaanIdSaatIni == null || k.PerusahaanId == _layananTenant.PerusahaanIdSaatIni));

        builder.Entity<PeriodePenggajian>().HasQueryFilter(p =>
            !p.IsDeleted &&
            (_layananTenant == null || _layananTenant.IsSuperAdmin || _layananTenant.PerusahaanIdSaatIni == null || p.PerusahaanId == _layananTenant.PerusahaanIdSaatIni));

        builder.Entity<Penggajian>().HasQueryFilter(p =>
            !p.IsDeleted &&
            (_layananTenant == null || _layananTenant.IsSuperAdmin || _layananTenant.PerusahaanIdSaatIni == null || p.PerusahaanId == _layananTenant.PerusahaanIdSaatIni));

        builder.Entity<DetailPenggajian>().HasQueryFilter(d =>
            !d.IsDeleted &&
            (_layananTenant == null || _layananTenant.IsSuperAdmin || _layananTenant.PerusahaanIdSaatIni == null || d.PerusahaanId == _layananTenant.PerusahaanIdSaatIni));

        builder.Entity<SlipGaji>().HasQueryFilter(s =>
            !s.IsDeleted &&
            (_layananTenant == null || _layananTenant.IsSuperAdmin || _layananTenant.PerusahaanIdSaatIni == null || s.PerusahaanId == _layananTenant.PerusahaanIdSaatIni));

        // ==========================================
        // Konfigurasi Modul Rekrutmen
        // ==========================================
        builder.Entity<LowonganKerja>(b =>
        {
            b.ToTable("LowonganKerja");
            b.HasIndex(l => new { l.PerusahaanId, l.KodeLowongan }).IsUnique();
            b.HasIndex(l => new { l.PerusahaanId, l.Status });

            b.HasOne(l => l.Departemen)
                .WithMany()
                .HasForeignKey(l => l.DepartemenId)
                .OnDelete(DeleteBehavior.SetNull);

            b.HasOne(l => l.Jabatan)
                .WithMany()
                .HasForeignKey(l => l.JabatanId)
                .OnDelete(DeleteBehavior.SetNull);

            b.HasQueryFilter(l =>
                !l.IsDeleted &&
                (_layananTenant == null || _layananTenant.IsSuperAdmin || _layananTenant.PerusahaanIdSaatIni == null || l.PerusahaanId == _layananTenant.PerusahaanIdSaatIni));
        });

        builder.Entity<Pelamar>(b =>
        {
            b.ToTable("Pelamar");
            b.HasIndex(p => new { p.PerusahaanId, p.NomorRegistrasi }).IsUnique();
            b.HasIndex(p => new { p.PerusahaanId, p.LowonganKerjaId });
            b.HasIndex(p => new { p.PerusahaanId, p.Status });

            b.HasOne(p => p.LowonganKerja)
                .WithMany(l => l.DaftarPelamar)
                .HasForeignKey(p => p.LowonganKerjaId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasOne(p => p.Karyawan)
                .WithMany()
                .HasForeignKey(p => p.KaryawanId)
                .OnDelete(DeleteBehavior.SetNull);

            b.HasQueryFilter(p =>
                !p.IsDeleted &&
                (_layananTenant == null || _layananTenant.IsSuperAdmin || _layananTenant.PerusahaanIdSaatIni == null || p.PerusahaanId == _layananTenant.PerusahaanIdSaatIni));
        });

        builder.Entity<TahapanSeleksi>(b =>
        {
            b.ToTable("TahapanSeleksi");
            b.HasIndex(t => new { t.PerusahaanId, t.LowonganKerjaId, t.Urutan });

            b.HasOne(t => t.LowonganKerja)
                .WithMany(l => l.DaftarTahapanSeleksi)
                .HasForeignKey(t => t.LowonganKerjaId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasQueryFilter(t =>
                !t.IsDeleted &&
                (_layananTenant == null || _layananTenant.IsSuperAdmin || _layananTenant.PerusahaanIdSaatIni == null || t.PerusahaanId == _layananTenant.PerusahaanIdSaatIni));
        });

        builder.Entity<JadwalWawancara>(b =>
        {
            b.ToTable("JadwalWawancara");
            b.HasIndex(j => new { j.PerusahaanId, j.PelamarId });
            b.HasIndex(j => new { j.PerusahaanId, j.TanggalWawancara });

            b.HasOne(j => j.Pelamar)
                .WithMany(p => p.DaftarJadwalWawancara)
                .HasForeignKey(j => j.PelamarId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasOne(j => j.TahapanSeleksi)
                .WithMany()
                .HasForeignKey(j => j.TahapanSeleksiId)
                .OnDelete(DeleteBehavior.SetNull);

            b.HasOne(j => j.Pewawancara)
                .WithMany()
                .HasForeignKey(j => j.PewawancaraId)
                .OnDelete(DeleteBehavior.SetNull);

            b.HasQueryFilter(j =>
                !j.IsDeleted &&
                (_layananTenant == null || _layananTenant.IsSuperAdmin || _layananTenant.PerusahaanIdSaatIni == null || j.PerusahaanId == _layananTenant.PerusahaanIdSaatIni));
        });

        builder.Entity<HasilSeleksi>(b =>
        {
            b.ToTable("HasilSeleksi");
            b.HasIndex(h => new { h.PerusahaanId, h.PelamarId });

            b.HasOne(h => h.Pelamar)
                .WithMany(p => p.DaftarHasilSeleksi)
                .HasForeignKey(h => h.PelamarId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasOne(h => h.TahapanSeleksi)
                .WithMany()
                .HasForeignKey(h => h.TahapanSeleksiId)
                .OnDelete(DeleteBehavior.SetNull);

            b.HasOne(h => h.JadwalWawancara)
                .WithMany()
                .HasForeignKey(h => h.JadwalWawancaraId)
                .OnDelete(DeleteBehavior.SetNull);

            b.HasOne(h => h.Penilai)
                .WithMany()
                .HasForeignKey(h => h.PenilaiId)
                .OnDelete(DeleteBehavior.Restrict);

            b.HasQueryFilter(h =>
                !h.IsDeleted &&
                (_layananTenant == null || _layananTenant.IsSuperAdmin || _layananTenant.PerusahaanIdSaatIni == null || h.PerusahaanId == _layananTenant.PerusahaanIdSaatIni));
        });

        // ==========================================
        // Konfigurasi Modul Penilaian Kinerja
        // ==========================================
        builder.Entity<PeriodePenilaian>(b =>
        {
            b.ToTable("PeriodePenilaian");
            b.HasIndex(p => new { p.PerusahaanId, p.Tahun, p.NamaPeriode }).IsUnique();
            b.HasIndex(p => new { p.PerusahaanId, p.Status });

            b.HasQueryFilter(p =>
                !p.IsDeleted &&
                (_layananTenant == null || _layananTenant.IsSuperAdmin || _layananTenant.PerusahaanIdSaatIni == null || p.PerusahaanId == _layananTenant.PerusahaanIdSaatIni));
        });

        builder.Entity<IndikatorKPI>(b =>
        {
            b.ToTable("IndikatorKPI");
            b.HasIndex(i => new { i.PerusahaanId, i.KodeIndikator }).IsUnique();
            b.HasIndex(i => new { i.PerusahaanId, i.DepartemenId });

            b.HasOne(i => i.Departemen)
                .WithMany()
                .HasForeignKey(i => i.DepartemenId)
                .OnDelete(DeleteBehavior.SetNull);

            b.HasOne(i => i.Jabatan)
                .WithMany()
                .HasForeignKey(i => i.JabatanId)
                .OnDelete(DeleteBehavior.SetNull);

            b.HasQueryFilter(i =>
                !i.IsDeleted &&
                (_layananTenant == null || _layananTenant.IsSuperAdmin || _layananTenant.PerusahaanIdSaatIni == null || i.PerusahaanId == _layananTenant.PerusahaanIdSaatIni));
        });

        builder.Entity<PenilaianKinerja>(b =>
        {
            b.ToTable("PenilaianKinerja");
            b.HasIndex(p => new { p.PeriodePenilaianId, p.KaryawanId }).IsUnique();
            b.HasIndex(p => new { p.PerusahaanId, p.PeriodePenilaianId });
            b.HasIndex(p => new { p.PerusahaanId, p.KaryawanId });

            b.HasOne(p => p.PeriodePenilaian)
                .WithMany(pr => pr.DaftarPenilaianKinerja)
                .HasForeignKey(p => p.PeriodePenilaianId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasOne(p => p.Karyawan)
                .WithMany()
                .HasForeignKey(p => p.KaryawanId)
                .OnDelete(DeleteBehavior.Restrict);

            b.HasOne(p => p.Penilai)
                .WithMany()
                .HasForeignKey(p => p.PenilaiId)
                .OnDelete(DeleteBehavior.Restrict);

            b.HasQueryFilter(p =>
                !p.IsDeleted &&
                (_layananTenant == null || _layananTenant.IsSuperAdmin || _layananTenant.PerusahaanIdSaatIni == null || p.PerusahaanId == _layananTenant.PerusahaanIdSaatIni));
        });

        builder.Entity<DetailPenilaian>(b =>
        {
            b.ToTable("DetailPenilaian");
            b.HasIndex(d => new { d.PenilaianKinerjaId, d.IndikatorKPIId }).IsUnique();

            b.HasOne(d => d.PenilaianKinerja)
                .WithMany(p => p.DaftarDetailPenilaian)
                .HasForeignKey(d => d.PenilaianKinerjaId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasOne(d => d.IndikatorKPI)
                .WithMany()
                .HasForeignKey(d => d.IndikatorKPIId)
                .OnDelete(DeleteBehavior.Restrict);

            b.HasQueryFilter(d =>
                !d.IsDeleted &&
                (_layananTenant == null || _layananTenant.IsSuperAdmin || _layananTenant.PerusahaanIdSaatIni == null || d.PerusahaanId == _layananTenant.PerusahaanIdSaatIni));
        });

        // ==========================================
        // FASE 6: MODUL PELATIHAN & MANAJEMEN ASET
        // ==========================================

        builder.Entity<ProgramPelatihan>(b =>
        {
            b.ToTable("ProgramPelatihan");
            b.HasIndex(p => new { p.PerusahaanId, p.KodeProgram }).IsUnique();
            b.HasIndex(p => new { p.PerusahaanId, p.Status });
            b.HasIndex(p => new { p.PerusahaanId, p.TanggalMulai });

            b.HasOne(p => p.Departemen)
                .WithMany()
                .HasForeignKey(p => p.DepartemenId)
                .OnDelete(DeleteBehavior.SetNull);

            b.HasQueryFilter(p =>
                !p.IsDeleted &&
                (_layananTenant == null || _layananTenant.IsSuperAdmin || _layananTenant.PerusahaanIdSaatIni == null || p.PerusahaanId == _layananTenant.PerusahaanIdSaatIni));
        });

        builder.Entity<PesertaPelatihan>(b =>
        {
            b.ToTable("PesertaPelatihan");
            b.HasIndex(p => new { p.ProgramPelatihanId, p.KaryawanId }).IsUnique();
            b.HasIndex(p => new { p.PerusahaanId, p.KaryawanId });
            b.HasIndex(p => new { p.PerusahaanId, p.StatusKeikutsertaan });

            b.HasOne(p => p.ProgramPelatihan)
                .WithMany(pr => pr.DaftarPeserta)
                .HasForeignKey(p => p.ProgramPelatihanId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasOne(p => p.Karyawan)
                .WithMany()
                .HasForeignKey(p => p.KaryawanId)
                .OnDelete(DeleteBehavior.Restrict);

            b.HasQueryFilter(p =>
                !p.IsDeleted &&
                (_layananTenant == null || _layananTenant.IsSuperAdmin || _layananTenant.PerusahaanIdSaatIni == null || p.PerusahaanId == _layananTenant.PerusahaanIdSaatIni));
        });

        builder.Entity<SertifikatPelatihan>(b =>
        {
            b.ToTable("SertifikatPelatihan");
            b.HasIndex(s => new { s.PerusahaanId, s.NomorSertifikat }).IsUnique();
            b.HasIndex(s => new { s.PerusahaanId, s.KodeVerifikasi }).IsUnique();
            b.HasIndex(s => s.PesertaPelatihanId).IsUnique();

            b.HasOne(s => s.PesertaPelatihan)
                .WithOne(p => p.Sertifikat)
                .HasForeignKey<SertifikatPelatihan>(s => s.PesertaPelatihanId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasQueryFilter(s =>
                !s.IsDeleted &&
                (_layananTenant == null || _layananTenant.IsSuperAdmin || _layananTenant.PerusahaanIdSaatIni == null || s.PerusahaanId == _layananTenant.PerusahaanIdSaatIni));
        });

        builder.Entity<AsetPerusahaan>(b =>
        {
            b.ToTable("AsetPerusahaan");
            b.HasIndex(a => new { a.PerusahaanId, a.NomorAset }).IsUnique();
            b.HasIndex(a => new { a.PerusahaanId, a.Kategori });
            b.HasIndex(a => new { a.PerusahaanId, a.Status });
            b.HasIndex(a => new { a.PerusahaanId, a.Kondisi });

            b.HasOne(a => a.PemegangSaatIni)
                .WithMany()
                .HasForeignKey(a => a.PemegangSaatIniId)
                .OnDelete(DeleteBehavior.SetNull);

            b.HasQueryFilter(a =>
                !a.IsDeleted &&
                (_layananTenant == null || _layananTenant.IsSuperAdmin || _layananTenant.PerusahaanIdSaatIni == null || a.PerusahaanId == _layananTenant.PerusahaanIdSaatIni));
        });

        builder.Entity<PenyerahanAset>(b =>
        {
            b.ToTable("PenyerahanAset");
            b.HasIndex(p => new { p.PerusahaanId, p.NomorDokumen }).IsUnique();
            b.HasIndex(p => new { p.PerusahaanId, p.AsetPerusahaanId });
            b.HasIndex(p => new { p.PerusahaanId, p.KaryawanId });

            b.HasOne(p => p.AsetPerusahaan)
                .WithMany(a => a.DaftarPenyerahan)
                .HasForeignKey(p => p.AsetPerusahaanId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasOne(p => p.Karyawan)
                .WithMany()
                .HasForeignKey(p => p.KaryawanId)
                .OnDelete(DeleteBehavior.Restrict);

            b.HasOne(p => p.DiserahkanOleh)
                .WithMany()
                .HasForeignKey(p => p.DiserahkanOlehId)
                .OnDelete(DeleteBehavior.Restrict);

            b.HasOne(p => p.DiterimaKembaliOleh)
                .WithMany()
                .HasForeignKey(p => p.DiterimaKembaliOlehId)
                .OnDelete(DeleteBehavior.SetNull);

            b.HasQueryFilter(p =>
                !p.IsDeleted &&
                (_layananTenant == null || _layananTenant.IsSuperAdmin || _layananTenant.PerusahaanIdSaatIni == null || p.PerusahaanId == _layananTenant.PerusahaanIdSaatIni));
        });

        builder.Entity<RiwayatAset>(b =>
        {
            b.ToTable("RiwayatAset");
            b.HasIndex(r => new { r.PerusahaanId, r.AsetPerusahaanId });
            b.HasIndex(r => new { r.PerusahaanId, r.TanggalAktivitas });

            b.HasOne(r => r.AsetPerusahaan)
                .WithMany(a => a.DaftarRiwayat)
                .HasForeignKey(r => r.AsetPerusahaanId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasOne(r => r.KaryawanTerkait)
                .WithMany()
                .HasForeignKey(r => r.KaryawanTerkaitId)
                .OnDelete(DeleteBehavior.SetNull);

            b.HasQueryFilter(r =>
                !r.IsDeleted &&
                (_layananTenant == null || _layananTenant.IsSuperAdmin || _layananTenant.PerusahaanIdSaatIni == null || r.PerusahaanId == _layananTenant.PerusahaanIdSaatIni));
        });
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        TerapkanOtomatisasiAuditDanTenant();
        return base.SaveChangesAsync(cancellationToken);
    }

    public override int SaveChanges()
    {
        TerapkanOtomatisasiAuditDanTenant();
        return base.SaveChanges();
    }

    /// <summary>
    /// Mengotomatiskan pengisian tanggal audit (CreatedAt/UpdatedAt), penanda Soft Delete,
    /// dan inject PerusahaanId pada entitas baru.
    /// </summary>
    private void TerapkanOtomatisasiAuditDanTenant()
    {
        var entries = ChangeTracker.Entries();
        var waktuSekarang = DateTimeOffset.UtcNow;
        var tenantAktif = _layananTenant?.PerusahaanIdSaatIni;

        foreach (var entry in entries)
        {
            // Otomatisasi PerusahaanId untuk entitas baru
            if (entry.Entity is ITenantEntity tenantEntity && entry.State == EntityState.Added)
            {
                if (tenantEntity.PerusahaanId == Guid.Empty && tenantAktif.HasValue)
                {
                    tenantEntity.PerusahaanId = tenantAktif.Value;
                }
            }

            // Otomatisasi Audit Jejak Waktu
            if (entry.Entity is IAuditEntity auditEntity)
            {
                if (entry.State == EntityState.Added)
                {
                    auditEntity.CreatedAt = waktuSekarang;
                }
                else if (entry.State == EntityState.Modified)
                {
                    auditEntity.UpdatedAt = waktuSekarang;
                }
            }

            // Otomatisasi Soft Delete
            if (entry.Entity is ISoftDelete softDeleteEntity && entry.State == EntityState.Deleted)
            {
                entry.State = EntityState.Modified;
                softDeleteEntity.IsDeleted = true;
                softDeleteEntity.DeletedAt = waktuSekarang;
            }
        }
    }
}
