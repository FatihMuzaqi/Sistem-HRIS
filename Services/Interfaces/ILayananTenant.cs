using SistemHRD.Models.Entitas;

namespace SistemHRD.Services.Interfaces;

/// <summary>
/// Kontrak layanan penanganan konteks perusahaan aktif (Multi-Tenant).
/// </summary>
public interface ILayananTenant
{
    Guid? PerusahaanIdSaatIni { get; }
    string? KodePerusahaanSaatIni { get; }
    bool IsSuperAdmin { get; }

    void SetTenant(Guid perusahaanId, string? kodePerusahaan = null);
    void SetIsSuperAdmin(bool isSuperAdmin);
    Task<Perusahaan?> DapatkanPerusahaanAktifAsync();
}
