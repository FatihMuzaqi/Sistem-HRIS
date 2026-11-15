namespace SistemHRD.Models.Entitas;

/// <summary>
/// Kontrak antarmuka untuk mencatat jejak audit waktu dan pengguna pembuatan/perubahan data.
/// </summary>
public interface IAuditEntity
{
    DateTimeOffset CreatedAt { get; set; }
    string? CreatedBy { get; set; }
    DateTimeOffset? UpdatedAt { get; set; }
    string? UpdatedBy { get; set; }
}
