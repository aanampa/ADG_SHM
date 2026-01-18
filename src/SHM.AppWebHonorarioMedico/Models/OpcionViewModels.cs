using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace SHM.AppWebHonorarioMedico.Models;

public class OpcionIndexViewModel
{
}

public class OpcionListViewModel
{
    public List<OpcionTreeItemViewModel> Items { get; set; } = new();
}

public class OpcionTreeItemViewModel
{
    public int IdOpcion { get; set; }
    public string GuidRegistro { get; set; } = string.Empty;
    public string? Nombre { get; set; }
    public string? Url { get; set; }
    public string? Icono { get; set; }
    public int? Orden { get; set; }
    public int? IdOpcionPadre { get; set; }
    public string? NombrePadre { get; set; }
    public int Activo { get; set; }
    public List<OpcionTreeItemViewModel> Hijos { get; set; } = new();
}

public class OpcionCreateViewModel
{
    [Required(ErrorMessage = "El nombre es requerido")]
    [MaxLength(100, ErrorMessage = "El nombre no puede exceder 100 caracteres")]
    public string? Nombre { get; set; }

    [MaxLength(255, ErrorMessage = "La URL no puede exceder 255 caracteres")]
    public string? Url { get; set; }

    [MaxLength(255, ErrorMessage = "El icono no puede exceder 255 caracteres")]
    public string? Icono { get; set; }

    public int? Orden { get; set; }

    public int? IdOpcionPadre { get; set; }

    public List<SelectListItem> OpcionesPadre { get; set; } = new();
}

public class OpcionEditViewModel
{
    public string GuidRegistro { get; set; } = string.Empty;

    [Required(ErrorMessage = "El nombre es requerido")]
    [MaxLength(100, ErrorMessage = "El nombre no puede exceder 100 caracteres")]
    public string? Nombre { get; set; }

    [MaxLength(255, ErrorMessage = "La URL no puede exceder 255 caracteres")]
    public string? Url { get; set; }

    [MaxLength(255, ErrorMessage = "El icono no puede exceder 255 caracteres")]
    public string? Icono { get; set; }

    public int? Orden { get; set; }

    public int? IdOpcionPadre { get; set; }

    public int Activo { get; set; }

    public List<SelectListItem> OpcionesPadre { get; set; } = new();
}

public class OpcionDeleteViewModel
{
    public string GuidRegistro { get; set; } = string.Empty;
    public string? Nombre { get; set; }
    public string? Url { get; set; }
    public string? NombrePadre { get; set; }
    public int CantidadHijos { get; set; }
}
