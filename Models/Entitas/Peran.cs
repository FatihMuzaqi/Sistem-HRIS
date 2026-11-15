using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace SistemHRD.Models.Entitas;

/// <summary>
/// Entitas Peran otorisasi berbasis ASP.NET Core Identity.
/// </summary>
public class Peran : IdentityRole<Guid>
{
    public Peran() : base()
    {
    }

    public Peran(string roleName) : base(roleName)
    {
    }

    [MaxLength(255)]
    public string? Deskripsi { get; set; }
}
