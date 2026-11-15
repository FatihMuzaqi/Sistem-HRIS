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
