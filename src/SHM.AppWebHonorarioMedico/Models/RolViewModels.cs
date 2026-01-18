using System.ComponentModel.DataAnnotations;

namespace SHM.AppWebHonorarioMedico.Models;

// ==================== ViewModels para Rol ====================

public class RolIndexViewModel
{
}

public class RolListViewModel
{
    public List<RolItemViewModel> Items { get; set; } = new();
}

public class RolItemViewModel
{
    public string GuidRegistro { get; set; } = string.Empty;
    public string? Codigo { get; set; }
    public string? Descripcion { get; set; }
    public int Activo { get; set; }
}

public class RolCreateViewModel
{
    [Required(ErrorMessage = "El codigo es requerido")]
    [MaxLength(100, ErrorMessage = "El codigo no puede exceder 100 caracteres")]
    public string? Codigo { get; set; }

    [Required(ErrorMessage = "La descripcion es requerida")]
    [MaxLength(100, ErrorMessage = "La descripcion no puede exceder 100 caracteres")]
    public string? Descripcion { get; set; }
}

public class RolEditViewModel
{
    public string GuidRegistro { get; set; } = string.Empty;

    [Required(ErrorMessage = "El codigo es requerido")]
    [MaxLength(100, ErrorMessage = "El codigo no puede exceder 100 caracteres")]
    public string? Codigo { get; set; }

    [Required(ErrorMessage = "La descripcion es requerida")]
    [MaxLength(100, ErrorMessage = "La descripcion no puede exceder 100 caracteres")]
    public string? Descripcion { get; set; }

    public int Activo { get; set; }
}

public class RolDeleteViewModel
{
    public string GuidRegistro { get; set; } = string.Empty;
    public string? Codigo { get; set; }
    public string? Descripcion { get; set; }
}

// ==================== ViewModels para Asignacion de Opciones ====================

public class RolOpcionesAssignViewModel
{
    public string GuidRegistro { get; set; } = string.Empty;
    public string? Codigo { get; set; }
    public string? Descripcion { get; set; }
    public int IdRol { get; set; }
    public List<OpcionMenuItemViewModel> Opciones { get; set; } = new();
}

public class OpcionMenuItemViewModel
{
    public int IdOpcion { get; set; }
    public string? Nombre { get; set; }
    public string? Icono { get; set; }
    public int? IdOpcionPadre { get; set; }
    public int? Orden { get; set; }
    public bool Asignado { get; set; }
    public List<OpcionMenuItemViewModel> Hijos { get; set; } = new();
}

public class SaveRolOpcionesRequest
{
    public string GuidRegistro { get; set; } = string.Empty;
    public List<int> OpcionesSeleccionadas { get; set; } = new();
}
