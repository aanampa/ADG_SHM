using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace SHM.AppWebHonorarioMedico.Models;

public class UsuarioExternoListViewModel
{
    public List<UsuarioExternoItemViewModel> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? SearchTerm { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
}

public class UsuarioExternoItemViewModel
{
    public string GuidRegistro { get; set; } = string.Empty;
    public string? Login { get; set; }
    public string? NombreCompleto { get; set; }
    public string? Email { get; set; }
    public string? NumeroDocumento { get; set; }
    public string? Celular { get; set; }
    public string? EntidadMedicaNombre { get; set; }
    public string? RolDescripcion { get; set; }
    public int Activo { get; set; }
    public DateTime FechaCreacion { get; set; }
}

public class UsuarioExternoCreateViewModel
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

    [Required(ErrorMessage = "Debe seleccionar una entidad medica")]
    [Display(Name = "Entidad Medica")]
    public int? IdEntidadMedica { get; set; }

    [Required(ErrorMessage = "Debe seleccionar un rol")]
    [Display(Name = "Rol")]
    public int? IdRol { get; set; }

    [Display(Name = "Enviar credenciales por correo")]
    public bool EnviarCorreo { get; set; } = true;

    public List<SelectListItem> Roles { get; set; } = new();
}

public class UsuarioExternoEditViewModel
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

    [Required(ErrorMessage = "Debe seleccionar una entidad medica")]
    [Display(Name = "Entidad Medica")]
    public int? IdEntidadMedica { get; set; }

    public string? EntidadMedicaNombre { get; set; }

    [Required(ErrorMessage = "Debe seleccionar un rol")]
    [Display(Name = "Rol")]
    public int? IdRol { get; set; }

    [Display(Name = "Activo")]
    public int Activo { get; set; } = 1;

    public List<SelectListItem> Roles { get; set; } = new();
}

public class UsuarioExternoDeleteViewModel
{
    public string GuidRegistro { get; set; } = string.Empty;
    public string? NombreCompleto { get; set; }
    public string? Login { get; set; }
    public string? Email { get; set; }
}

public class UsuarioExternoResetClaveViewModel
{
    public string GuidRegistro { get; set; } = string.Empty;
    public string? NombreCompleto { get; set; }
    public string? Login { get; set; }
    public string? Email { get; set; }
    public bool EnviarCorreo { get; set; } = true;
}

public class EntidadMedicaSelectItem
{
    public int Id { get; set; }
    public string Text { get; set; } = string.Empty;
}
