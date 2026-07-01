using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace SHM.AppWebHonorarioMedico.Models;

public class EntidadMedicaListViewModel
{
    public List<EntidadMedicaItemViewModel> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? SearchTerm { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
}

public class EntidadMedicaItemViewModel
{
    public string GuidRegistro { get; set; } = string.Empty;
    public string? CodigoEntidad { get; set; }
    public string? RazonSocial { get; set; }
    public string? Ruc { get; set; }
    public string? TipoEntidadMedica { get; set; }
    public string? TipoEntidadMedicaDescripcion { get; set; }
    public string? Telefono { get; set; }
    public string? Celular { get; set; }
    public string? CodigoAcreedor { get; set; }
    public int Activo { get; set; }
    public DateTime FechaCreacion { get; set; }
}

public class EntidadMedicaCreateViewModel
{
    [Required(ErrorMessage = "El codigo de entidad es requerido")]
    [StringLength(20, ErrorMessage = "El codigo no puede exceder 20 caracteres")]
    [Display(Name = "Codigo de Entidad")]
    public string? CodigoEntidad { get; set; }

    [Required(ErrorMessage = "La razon social es requerida")]
    [StringLength(200, ErrorMessage = "La razon social no puede exceder 200 caracteres")]
    [Display(Name = "Razon Social")]
    public string? RazonSocial { get; set; }

    [Required(ErrorMessage = "El RUC es requerido")]
    [StringLength(11, MinimumLength = 11, ErrorMessage = "El RUC debe tener 11 digitos")]
    [RegularExpression(@"^\d{11}$", ErrorMessage = "El RUC solo debe contener numeros")]
    [Display(Name = "RUC")]
    public string? Ruc { get; set; }

    [Required(ErrorMessage = "El tipo de entidad es requerido")]
    [Display(Name = "Tipo de Entidad")]
    public string? TipoEntidadMedica { get; set; }

    [StringLength(20, ErrorMessage = "El telefono no puede exceder 20 caracteres")]
    [RegularExpression(@"^\d*$", ErrorMessage = "El telefono solo debe contener numeros")]
    [Display(Name = "Telefono")]
    public string? Telefono { get; set; }

    [StringLength(20, ErrorMessage = "El celular no puede exceder 20 caracteres")]
    [RegularExpression(@"^\d*$", ErrorMessage = "El celular solo debe contener numeros")]
    [Display(Name = "Celular")]
    public string? Celular { get; set; }

    [StringLength(50, ErrorMessage = "El codigo de acreedor no puede exceder 50 caracteres")]
    [Display(Name = "Codigo Acreedor")]
    public string? CodigoAcreedor { get; set; }

    public List<SelectListItem> TiposEntidadMedica { get; set; } = new();
}

public class EntidadMedicaEditViewModel
{
    public string GuidRegistro { get; set; } = string.Empty;
    public int IdEntidadMedica { get; set; }

    [Required(ErrorMessage = "El codigo de entidad es requerido")]
    [StringLength(20, ErrorMessage = "El codigo no puede exceder 20 caracteres")]
    [Display(Name = "Codigo de Entidad")]
    public string? CodigoEntidad { get; set; }

    [Required(ErrorMessage = "La razon social es requerida")]
    [StringLength(200, ErrorMessage = "La razon social no puede exceder 200 caracteres")]
    [Display(Name = "Razon Social")]
    public string? RazonSocial { get; set; }

    [Required(ErrorMessage = "El RUC es requerido")]
    [StringLength(11, MinimumLength = 11, ErrorMessage = "El RUC debe tener 11 digitos")]
    [RegularExpression(@"^\d{11}$", ErrorMessage = "El RUC solo debe contener numeros")]
    [Display(Name = "RUC")]
    public string? Ruc { get; set; }

    [Required(ErrorMessage = "El tipo de entidad es requerido")]
    [Display(Name = "Tipo de Entidad")]
    public string? TipoEntidadMedica { get; set; }

    [StringLength(20, ErrorMessage = "El telefono no puede exceder 20 caracteres")]
    [RegularExpression(@"^\d*$", ErrorMessage = "El telefono solo debe contener numeros")]
    [Display(Name = "Telefono")]
    public string? Telefono { get; set; }

    [StringLength(20, ErrorMessage = "El celular no puede exceder 20 caracteres")]
    [RegularExpression(@"^\d*$", ErrorMessage = "El celular solo debe contener numeros")]
    [Display(Name = "Celular")]
    public string? Celular { get; set; }

    [StringLength(50, ErrorMessage = "El codigo de acreedor no puede exceder 50 caracteres")]
    [Display(Name = "Codigo Acreedor")]
    public string? CodigoAcreedor { get; set; }

    [Display(Name = "Activo")]
    public int Activo { get; set; } = 1;

    public List<SelectListItem> TiposEntidadMedica { get; set; } = new();
}

public class EntidadMedicaDeleteViewModel
{
    public string GuidRegistro { get; set; } = string.Empty;
    public string? RazonSocial { get; set; }
    public string? Ruc { get; set; }
    public string? CodigoEntidad { get; set; }
}

// ─── Contactos de Entidad Medica ───────────────────────────────────────────

public class ContactoListViewModel
{
    public string EntidadGuid { get; set; } = string.Empty;
    public int IdEntidadMedica { get; set; }
    public string EntidadRazonSocial { get; set; } = string.Empty;
    public List<ContactoItemViewModel> Items { get; set; } = new();
}

public class ContactoItemViewModel
{
    public string GuidRegistro { get; set; } = string.Empty;
    public string? ApellidoPaterno { get; set; }
    public string? ApellidoMaterno { get; set; }
    public string? Nombres { get; set; }
    public string NombreCompleto => $"{ApellidoPaterno} {ApellidoMaterno}, {Nombres}".Trim().Trim(',').Trim();
    public string? Celular { get; set; }
    public string Email { get; set; } = string.Empty;
    public string? Cargo { get; set; }
    public int Activo { get; set; }
}

public class ContactoCreateViewModel
{
    public string EntidadGuid { get; set; } = string.Empty;
    public int IdEntidadMedica { get; set; }

    [Required(ErrorMessage = "Los nombres son requeridos")]
    [StringLength(200, ErrorMessage = "Los nombres no pueden exceder 200 caracteres")]
    [Display(Name = "Nombres")]
    public string? Nombres { get; set; }

    [StringLength(100, ErrorMessage = "El apellido paterno no puede exceder 100 caracteres")]
    [Display(Name = "Apellido Paterno")]
    public string? ApellidoPaterno { get; set; }

    [StringLength(100, ErrorMessage = "El apellido materno no puede exceder 100 caracteres")]
    [Display(Name = "Apellido Materno")]
    public string? ApellidoMaterno { get; set; }

    [Required(ErrorMessage = "El correo electronico es requerido")]
    [StringLength(100, ErrorMessage = "El correo no puede exceder 100 caracteres")]
    [EmailAddress(ErrorMessage = "Formato de correo invalido")]
    [Display(Name = "Correo Electronico")]
    public string? Email { get; set; }

    [StringLength(10, ErrorMessage = "El celular no puede exceder 10 caracteres")]
    [Display(Name = "Celular")]
    public string? Celular { get; set; }

    [StringLength(120, ErrorMessage = "El cargo no puede exceder 120 caracteres")]
    [Display(Name = "Cargo")]
    public string? Cargo { get; set; }
}

public class ContactoEditViewModel
{
    public string GuidRegistro { get; set; } = string.Empty;
    public string EntidadGuid { get; set; } = string.Empty;
    public int IdEntidadContacto { get; set; }
    public int IdEntidadMedica { get; set; }

    [Required(ErrorMessage = "Los nombres son requeridos")]
    [StringLength(200, ErrorMessage = "Los nombres no pueden exceder 200 caracteres")]
    [Display(Name = "Nombres")]
    public string? Nombres { get; set; }

    [StringLength(100, ErrorMessage = "El apellido paterno no puede exceder 100 caracteres")]
    [Display(Name = "Apellido Paterno")]
    public string? ApellidoPaterno { get; set; }

    [StringLength(100, ErrorMessage = "El apellido materno no puede exceder 100 caracteres")]
    [Display(Name = "Apellido Materno")]
    public string? ApellidoMaterno { get; set; }

    [Required(ErrorMessage = "El correo electronico es requerido")]
    [StringLength(100, ErrorMessage = "El correo no puede exceder 100 caracteres")]
    [EmailAddress(ErrorMessage = "Formato de correo invalido")]
    [Display(Name = "Correo Electronico")]
    public string? Email { get; set; }

    [StringLength(10, ErrorMessage = "El celular no puede exceder 10 caracteres")]
    [Display(Name = "Celular")]
    public string? Celular { get; set; }

    [StringLength(120, ErrorMessage = "El cargo no puede exceder 120 caracteres")]
    [Display(Name = "Cargo")]
    public string? Cargo { get; set; }

    [Display(Name = "Activo")]
    public int Activo { get; set; } = 1;
}

public class ContactoDeleteViewModel
{
    public string GuidRegistro { get; set; } = string.Empty;
    public string EntidadGuid { get; set; } = string.Empty;
    public string? NombreCompleto { get; set; }
    public string? Email { get; set; }
}
