using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace SHM.AppWebHonorarioMedico.Models;

public class TablaDetalleIndexViewModel
{
    public List<SelectListItem> Tablas { get; set; } = new();
    public int? IdTablaSeleccionada { get; set; }
}

public class TablaDetalleListViewModel
{
    public List<TablaDetalleItemViewModel> Items { get; set; } = new();
}

public class TablaDetalleItemViewModel
{
    public string GuidRegistro { get; set; } = string.Empty;
    public int IdTabla { get; set; }
    public string? NombreTabla { get; set; }
    public string? Codigo { get; set; }
    public string? Descripcion { get; set; }
    public int? Orden { get; set; }
    public int Activo { get; set; }
}

public class TablaDetalleCreateViewModel
{
    public List<SelectListItem> Tablas { get; set; } = new();

    [Required(ErrorMessage = "La tabla es requerida")]
    public int IdTabla { get; set; }

    [Required(ErrorMessage = "El codigo es requerido")]
    [MaxLength(50, ErrorMessage = "El codigo no puede exceder 50 caracteres")]
    public string? Codigo { get; set; }

    [MaxLength(250, ErrorMessage = "La descripcion no puede exceder 250 caracteres")]
    public string? Descripcion { get; set; }

    public int? Orden { get; set; }
}

public class TablaDetalleEditViewModel
{
    public string GuidRegistro { get; set; } = string.Empty;

    public List<SelectListItem> Tablas { get; set; } = new();

    [Required(ErrorMessage = "La tabla es requerida")]
    public int IdTabla { get; set; }

    [Required(ErrorMessage = "El codigo es requerido")]
    [MaxLength(50, ErrorMessage = "El codigo no puede exceder 50 caracteres")]
    public string? Codigo { get; set; }

    [MaxLength(250, ErrorMessage = "La descripcion no puede exceder 250 caracteres")]
    public string? Descripcion { get; set; }

    public int? Orden { get; set; }

    public int Activo { get; set; }
}

public class TablaDetalleDeleteViewModel
{
    public string GuidRegistro { get; set; } = string.Empty;
    public string? NombreTabla { get; set; }
    public string? Codigo { get; set; }
    public string? Descripcion { get; set; }
}

// ViewModels para Tabla (CRUD dentro de la misma pantalla)
public class TablaCreateViewModel
{
    [Required(ErrorMessage = "El codigo es requerido")]
    [MaxLength(50, ErrorMessage = "El codigo no puede exceder 50 caracteres")]
    public string? Codigo { get; set; }

    [MaxLength(250, ErrorMessage = "La descripcion no puede exceder 250 caracteres")]
    public string? Descripcion { get; set; }
}

public class TablaEditViewModel
{
    public string GuidRegistro { get; set; } = string.Empty;

    [Required(ErrorMessage = "El codigo es requerido")]
    [MaxLength(50, ErrorMessage = "El codigo no puede exceder 50 caracteres")]
    public string? Codigo { get; set; }

    [MaxLength(250, ErrorMessage = "La descripcion no puede exceder 250 caracteres")]
    public string? Descripcion { get; set; }

    public int Activo { get; set; }
}

public class TablaDeleteViewModel
{
    public string GuidRegistro { get; set; } = string.Empty;
    public string? Codigo { get; set; }
    public string? Descripcion { get; set; }
}

public class TablaListViewModel
{
    public List<TablaItemViewModel> Items { get; set; } = new();
}

public class TablaItemViewModel
{
    public string GuidRegistro { get; set; } = string.Empty;
    public string? Codigo { get; set; }
    public string? Descripcion { get; set; }
    public int Activo { get; set; }
}

// ==================== ViewModels para Mantenimiento de Tablas Maestras ====================

/// <summary>
/// ViewModel para la pantalla de Mantenimiento de Tablas Maestras (solo lectura para Tabla).
/// </summary>
/// <author>Vladimir</author>
/// <created>2026-01-29</created>
public class TablaMantenimientoViewModel
{
    public List<TablaMantenimientoItemViewModel> Tablas { get; set; } = new();
}

/// <summary>
/// ViewModel para cada item de tabla en la pantalla de mantenimiento.
/// </summary>
public class TablaMantenimientoItemViewModel
{
    public int IdTabla { get; set; }
    public string GuidRegistro { get; set; } = string.Empty;
    public string? Codigo { get; set; }
    public string? Descripcion { get; set; }
    public int Activo { get; set; }
    public int CantidadDetalles { get; set; }
}

/// <summary>
/// ViewModel para el modal de administrar detalles de una tabla.
/// </summary>
public class TablaDetallesModalViewModel
{
    public int IdTabla { get; set; }
    public string? CodigoTabla { get; set; }
    public string? DescripcionTabla { get; set; }
    public List<TablaDetalleItemViewModel> Detalles { get; set; } = new();
}
