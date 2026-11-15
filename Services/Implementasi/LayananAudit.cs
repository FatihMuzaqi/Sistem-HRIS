using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using SistemHRD.Data;
using SistemHRD.Models.Entitas;
using SistemHRD.Services.Interfaces;

namespace SistemHRD.Services.Implementasi;

/// <summary>
/// Implementasi layanan audit log yang mengekstrak metadata pengguna, IP, User-Agent,
/// dan tenant untuk disimpan ke tabel AuditLog.
/// </summary>
public class LayananAudit : ILayananAudit
{
    private readonly KonteksAplikasi _context;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILayananTenant _layananTenant;

    public LayananAudit(
        KonteksAplikasi context,
        IHttpContextAccessor httpContextAccessor,
        ILayananTenant layananTenant)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
        _layananTenant = layananTenant;
    }

    public async Task CatatAsync(
        string aksi,
        string? namaEntitas = null,
        string? kunciEntitas = null,
        string? dataLama = null,
        string? dataBaru = null)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        Guid? penggunaId = null;
        string? namaPengguna = null;

        if (httpContext?.User?.Identity?.IsAuthenticated == true)
        {
            var idClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(idClaim) && Guid.TryParse(idClaim, out var uId))
            {
                penggunaId = uId;
            }
            namaPengguna = httpContext.User.Identity.Name;
        }

        var ip = httpContext?.Connection?.RemoteIpAddress?.ToString();
        var userAgent = httpContext?.Request?.Headers["User-Agent"].ToString();
        if (userAgent?.Length > 255) userAgent = userAgent.Substring(0, 255);

        var log = new AuditLog
        {
            Id = Guid.NewGuid(),
            PerusahaanId = _layananTenant.PerusahaanIdSaatIni,
            PenggunaId = penggunaId,
            NamaPengguna = namaPengguna ?? "Anonim/Sistem",
            Aksi = aksi,
            NamaEntitas = namaEntitas,
            KunciEntitas = kunciEntitas,
            DataLama = dataLama,
            DataBaru = dataBaru,
            AlamatIP = ip,
            UserAgent = userAgent,
            Waktu = DateTimeOffset.UtcNow
        };

        await _context.AuditLog.AddAsync(log);
        await _context.SaveChangesAsync();
    }
}
