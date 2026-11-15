namespace SistemHRD.Common.Constants;

/// <summary>
/// Konstanta nama peran pengguna (Role-Based Access Control) dalam SistemHRD.
/// </summary>
public static class NamaPeran
{
    public const string SuperAdmin = "SuperAdmin";
    public const string HRD = "HRD";
    public const string Manager = "Manager";
    public const string Finance = "Finance";
    public const string Karyawan = "Karyawan";

    public static readonly IReadOnlyList<string> SemuaPeran = new[]
    {
        SuperAdmin,
        HRD,
        Manager,
        Finance,
        Karyawan
    };
}
