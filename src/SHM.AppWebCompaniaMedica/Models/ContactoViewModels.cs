using System.ComponentModel.DataAnnotations;

namespace SHM.AppWebCompaniaMedica.Models;

public class ContactoItemViewModel
{
    public string GuidRegistro { get; set; } = string.Empty;
    public string? ApellidoPaterno { get; set; }
    public string? ApellidoMaterno { get; set; }
    public string? Nombres { get; set; }
    public string NombreCompleto => $"{ApellidoPaterno} {ApellidoMaterno}, {Nombres}".Trim().Trim(',').Trim();
    public string Email { get; set; } = string.Empty;
    public string? Celular { get; set; }
    public string? Cargo { get; set; }
    public int Activo { get; set; }
}

public class ContactoCreateViewModel
{
    [Required(ErrorMessage = "Los nombres son requeridos")]
    [StringLength(200)]
    public string? Nombres { get; set; }

    [StringLength(100)]
    public string? ApellidoPaterno { get; set; }

    [StringLength(100)]
    public string? ApellidoMaterno { get; set; }

    [Required(ErrorMessage = "El correo es requerido")]
    [StringLength(100)]
    [EmailAddress]
    public string? Email { get; set; }

    [StringLength(10)]
    public string? Celular { get; set; }

    [StringLength(120)]
    public string? Cargo { get; set; }
}

public class ContactoEditViewModel
{
    public string GuidRegistro { get; set; } = string.Empty;
    public int IdEntidadContacto { get; set; }

    [Required(ErrorMessage = "Los nombres son requeridos")]
    [StringLength(200)]
    public string? Nombres { get; set; }

    [StringLength(100)]
    public string? ApellidoPaterno { get; set; }

    [StringLength(100)]
    public string? ApellidoMaterno { get; set; }

    [Required(ErrorMessage = "El correo es requerido")]
    [StringLength(100)]
    [EmailAddress]
    public string? Email { get; set; }

    [StringLength(10)]
    public string? Celular { get; set; }

    [StringLength(120)]
    public string? Cargo { get; set; }

    public int Activo { get; set; } = 1;
}

public class ContactoDeleteViewModel
{
    public string GuidRegistro { get; set; } = string.Empty;
    public string? NombreCompleto { get; set; }
    public string? Email { get; set; }
}
