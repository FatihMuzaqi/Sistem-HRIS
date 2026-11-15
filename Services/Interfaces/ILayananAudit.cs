namespace SistemHRD.Services.Interfaces;

/// <summary>
/// Kontrak layanan pencatatan audit log untuk keamanan dan kepatuhan sistem.
/// </summary>
public interface ILayananAudit
{
    Task CatatAsync(
        string aksi,
        string? namaEntitas = null,
        string? kunciEntitas = null,
        string? dataLama = null,
        string? dataBaru = null);
}
