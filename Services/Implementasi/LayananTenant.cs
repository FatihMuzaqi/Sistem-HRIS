using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using SistemHRD.Common.Constants;
using SistemHRD.Data;
using SistemHRD.Models.Entitas;
using SistemHRD.Services.Interfaces;

namespace SistemHRD.Services.Implementasi;

/// <summary>
/// Implementasi layanan tenant untuk mengisolasi data per perusahaan dalam request HTTP.
/// </summary>
public class LayananTenant : ILayananTenant
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IServiceProvider _serviceProvider;
    private Guid? _perusahaanId;
    private string? _kodePerusahaan;
    private bool? _isSuperAdmin;

    public LayananTenant(IHttpContextAccessor httpContextAccessor, IServiceProvider serviceProvider)
    {
        _httpContextAccessor = httpContextAccessor;
        _serviceProvider = serviceProvider;
        InisialisasiDariKonteksHttp();
    }

    public Guid? PerusahaanIdSaatIni => _perusahaanId;
    public string? KodePerusahaanSaatIni => _kodePerusahaan;
    public bool IsSuperAdmin => _isSuperAdmin ?? false;

    public void SetTenant(Guid perusahaanId, string? kodePerusahaan = null)
    {
        _perusahaanId = perusahaanId;
        _kodePerusahaan = kodePerusahaan;
    }

    public void SetIsSuperAdmin(bool isSuperAdmin)
    {
        _isSuperAdmin = isSuperAdmin;
    }

    public async Task<Perusahaan?> DapatkanPerusahaanAktifAsync()
    {
        if (!_perusahaanId.HasValue) return null;

        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetService<KonteksAplikasi>();
        if (context == null) return null;

        return await context.Perusahaan.FindAsync(_perusahaanId.Value);
    }

    private void InisialisasiDariKonteksHttp()
    {
        var context = _httpContextAccessor.HttpContext;
        if (context?.User?.Identity?.IsAuthenticated == true)
        {
            var user = context.User;
            _isSuperAdmin = user.IsInRole(NamaPeran.SuperAdmin);

            var claimTenant = user.FindFirst("PerusahaanId")?.Value;
            if (!string.IsNullOrEmpty(claimTenant) && Guid.TryParse(claimTenant, out var pId))
            {
                _perusahaanId = pId;
            }

            _kodePerusahaan = user.FindFirst("KodePerusahaan")?.Value;
        }
    }
}
