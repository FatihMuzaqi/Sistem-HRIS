namespace SistemHRD.Models.Entitas;

/// <summary>
/// Kontrak antarmuka untuk entitas bisnis yang terikat dengan Perusahaan (Multi-Tenant).
/// </summary>
public interface ITenantEntity
{
    Guid PerusahaanId { get; set; }
}
