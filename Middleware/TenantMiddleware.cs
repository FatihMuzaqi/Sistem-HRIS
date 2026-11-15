using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using SistemHRD.Common.Constants;
using SistemHRD.Services.Interfaces;

namespace SistemHRD.Middleware;

/// <summary>
/// Middleware untuk memastikan konteks perusahaan (Multi-Tenant) tervalidasi pada setiap request.
/// </summary>
public class TenantMiddleware
{
    private readonly RequestDelegate _next;

    public TenantMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, ILayananTenant layananTenant)
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var isSuperAdmin = context.User.IsInRole(NamaPeran.SuperAdmin);
            layananTenant.SetIsSuperAdmin(isSuperAdmin);

            var perusahaanIdClaim = context.User.FindFirst("PerusahaanId")?.Value;
            var kodePerusahaanClaim = context.User.FindFirst("KodePerusahaan")?.Value;

            if (!string.IsNullOrEmpty(perusahaanIdClaim) && Guid.TryParse(perusahaanIdClaim, out var perusahaanId))
            {
                layananTenant.SetTenant(perusahaanId, kodePerusahaanClaim);
            }
        }

        await _next(context);
    }
}
