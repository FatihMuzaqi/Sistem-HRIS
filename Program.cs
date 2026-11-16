using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MySql.EntityFrameworkCore.Extensions;
using SistemHRD.Common.Constants;
using SistemHRD.Data;
using SistemHRD.Middleware;
using SistemHRD.Models.Entitas;
using SistemHRD.Services.Implementasi;
using SistemHRD.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// 1. Registrasi Layanan MVC & Razor Views
builder.Services.AddControllersWithViews();
builder.Services.AddHttpContextAccessor();

// 2. Konfigurasi Database (MySQL via phpMYSQL / XAMPP MariaDB)
var defaultConnection = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' belum dikonfigurasi.");

var databaseProvider = builder.Configuration.GetValue<string>("DatabaseProvider") ?? "MySQL";

builder.Services.AddDbContext<KonteksAplikasi>((serviceProvider, options) =>
{
    if (databaseProvider.Equals("SqlServer", StringComparison.OrdinalIgnoreCase))
    {
        var sqlServerConnection = builder.Configuration.GetConnectionString("SqlServerConnection") ?? defaultConnection;
        options.UseSqlServer(sqlServerConnection);
    }
    else
    {
        // MySQL / MariaDB (XAMPP phpMyAdmin)
        options.UseMySQL(defaultConnection);
    }
});

// 3. Registrasi ASP.NET Core Identity untuk Autentikasi & RBAC
builder.Services.AddIdentity<Pengguna, Peran>(options =>
{
    // Konfigurasi Keamanan Password
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;

    // Konfigurasi Penguncian Akun (Lockout)
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;

    // Konfigurasi Pengguna
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<KonteksAplikasi>()
.AddDefaultTokenProviders();

// 4. Konfigurasi Cookie Autentikasi
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.Name = "SistemHRD.SesiAuth";
    options.ExpireTimeSpan = TimeSpan.FromHours(8);
    options.LoginPath = "/Autentikasi/Masuk";
    options.LogoutPath = "/Autentikasi/Keluar";
    options.AccessDeniedPath = "/Autentikasi/AksesDitolak";
    options.SlidingExpiration = true;
});

// 5. Registrasi Layanan Multi-Tenant & Audit Log
builder.Services.AddScoped<ILayananTenant, LayananTenant>();
builder.Services.AddScoped<ILayananAudit, LayananAudit>();
builder.Services.AddScoped<ILayananDepartemen, LayananDepartemen>();
builder.Services.AddScoped<ILayananJabatan, LayananJabatan>();
builder.Services.AddScoped<ILayananKaryawan, LayananKaryawan>();
builder.Services.AddScoped<ILayananKehadiran, LayananKehadiran>();
builder.Services.AddScoped<ILayananCuti, LayananCuti>();
builder.Services.AddScoped<ILayananKalkulatorGaji, LayananKalkulatorGaji>();
builder.Services.AddScoped<ILayananPenggajian, LayananPenggajian>();

// 6. Proteksi CSRF (Anti-Forgery Token)
builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "X-CSRF-TOKEN";
});

var app = builder.Build();

// 7. Eksekusi Inisialisasi Data Awal (Seeder)
try
{
    await InisialisasiData.InisialisasiAsync(app.Services);
}
catch (Exception ex)
{
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    logger.LogError(ex, "Terjadi kesalahan saat mengeksekusi inisialisasi data awal (seeder).");
}

// 8. Pipeline Penanganan Request HTTP
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

// 9. Middleware Autentikasi, Otorisasi, dan Multi-Tenant
app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<TenantMiddleware>();

app.UseStaticFiles();
app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
