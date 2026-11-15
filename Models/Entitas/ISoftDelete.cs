namespace SistemHRD.Models.Entitas;

/// <summary>
/// Kontrak antarmuka untuk mendukung penghapusan logis (Soft Delete).
/// </summary>
public interface ISoftDelete
{
    bool IsDeleted { get; set; }
    DateTimeOffset? DeletedAt { get; set; }
    string? DeletedBy { get; set; }
}
