using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace SHM.AppWebHonorarioMedico.Models;

public class UsuarioInternoListViewModel
{
    public List<UsuarioInternoItemViewModel> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? SearchTerm { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
}

public class UsuarioInternoItemViewModel
{
    public string GuidRegistro { get; set; } = string.Empty;
    public string? Login { get; set; }
    public string? NombreCompleto { get; set; }
    public string? Email { get; set; }
    public string? NumeroDocumento { get; set; }
    public string? Celular { get; set; }
    public string? RolDescripcion { get; set; }
    public int Activo { get; set; }
    public DateTime FechaCreacion { get; set; }
}

public class UsuarioInternoCreateViewModel
{
    [Required(ErrorMessage = "El login es requerido")]
    [StringLength(50, ErrorMessage = "El login no puede exceder 50 caracteres")]
    [Display(Name = "Login")]
    public string? Login { get; set; }

    [Required(ErrorMessage = "El correo electronico es requerido")]
    [EmailAddress(ErrorMessage = "Formato de correo electronico invalido")]
    [StringLength(150, ErrorMessage = "El correo no puede exceder 150 caracteres")]
    [Display(Name = "Correo Electronico")]
    public string? Email { get; set; }

    [Required(ErrorMessage = "Los nombres son requeridos")]
    [StringLength(100, ErrorMessage = "Los nombres no pueden exceder 100 caracteres")]
    [Display(Name = "Nombres")]
    public string? Nombres { get; set; }

    [Required(ErrorMessage = "El apellido paterno es requerido")]
    [StringLength(100, ErrorMessage = "El apellido paterno no puede exceder 100 caracteres")]
    [Display(Name = "Apellido Paterno")]
    public string? ApellidoPaterno { get; set; }

    [StringLength(100, ErrorMessage = "El apellido materno no puede exceder 100 caracteres")]
    [Display(Name = "Apellido Materno")]
    public string? ApellidoMaterno { get; set; }

    [StringLength(20, ErrorMessage = "El numero de documento no puede exceder 20 caracteres")]
    [Display(Name = "Numero Documento")]
    public string? NumeroDocumento { get; set; }

    [StringLength(20, ErrorMessage = "El celular no puede exceder 20 caracteres")]
    [RegularExpression(@"^\d*$", ErrorMessage = "El celular solo debe contener numeros")]
    [Display(Name = "Celular")]
    public string? Celular { get; set; }

    [Required(ErrorMessage = "Debe seleccionar un rol")]
    [Display(Name = "Rol")]
    public int? IdRol { get; set; }

    [Display(Name = "Enviar credenciales por correo")]
    public bool EnviarCorreo { get; set; } = true;

    public List<SelectListItem> Roles { get; set; } = new();

    [Required(ErrorMessage = "Debe seleccionar al menos una sede")]
    [Display(Name = "Sedes")]
    public List<int> IdsSedesSeleccionadas { get; set; } = new();

    public List<SelectListItem> SedesDisponibles { get; set; } = new();
}

public class UsuarioInternoEditViewModel
{
    public string GuidRegistro { get; set; } = string.Empty;

    [Required(ErrorMessage = "El login es requerido")]
    [StringLength(50, ErrorMessage = "El login no puede exceder 50 caracteres")]
    [Display(Name = "Login")]
    public string? Login { get; set; }

    [Required(ErrorMessage = "El correo electronico es requerido")]
    [EmailAddress(ErrorMessage = "Formato de correo electronico invalido")]
    [StringLength(150, ErrorMessage = "El correo no puede exceder 150 caracteres")]
    [Display(Name = "Correo Electronico")]
    public string? Email { get; set; }

    [Required(ErrorMessage = "Los nombres son requeridos")]
    [StringLength(100, ErrorMessage = "Los nombres no pueden exceder 100 caracteres")]
    [Display(Name = "Nombres")]
    public string? Nombres { get; set; }

    [Required(ErrorMessage = "El apellido paterno es requerido")]
    [StringLength(100, ErrorMessage = "El apellido paterno no puede exceder 100 caracteres")]
    [Display(Name = "Apellido Paterno")]
    public string? ApellidoPaterno { get; set; }

    [StringLength(100, ErrorMessage = "El apellido materno no puede exceder 100 caracteres")]
    [Display(Name = "Apellido Materno")]
    public string? ApellidoMaterno { get; set; }

    [StringLength(20, ErrorMessage = "El numero de documento no puede exceder 20 caracteres")]
    [Display(Name = "Numero Documento")]
    public string? NumeroDocumento { get; set; }

    [StringLength(20, ErrorMessage = "El celular no puede exceder 20 caracteres")]
    [RegularExpression(@"^\d*$", ErrorMessage = "El celular solo debe contener numeros")]
    [Display(Name = "Celular")]
    public string? Celular { get; set; }

    [Required(ErrorMessage = "Debe seleccionar un rol")]
    [Display(Name = "Rol")]
    public int? IdRol { get; set; }

    [Display(Name = "Activo")]
    public int Activo { get; set; } = 1;

    public List<SelectListItem> Roles { get; set; } = new();

    [Required(ErrorMessage = "Debe seleccionar al menos una sede")]
    [Display(Name = "Sedes")]
    public List<int> IdsSedesSeleccionadas { get; set; } = new();

    public List<SelectListItem> SedesDisponibles { get; set; } = new();
}

public class UsuarioInternoDeleteViewModel
{
    public string GuidRegistro { get; set; } = string.Empty;
    public string? NombreCompleto { get; set; }
    public string? Login { get; set; }
    public string? Email { get; set; }
}

public class UsuarioInternoResetClaveViewModel
{
    public string GuidRegistro { get; set; } = string.Empty;
    public string? NombreCompleto { get; set; }
    public string? Login { get; set; }
    public string? Email { get; set; }
    public bool EnviarCorreo { get; set; } = true;
}
