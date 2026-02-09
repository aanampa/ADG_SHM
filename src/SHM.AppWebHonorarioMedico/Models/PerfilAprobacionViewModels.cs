using System.ComponentModel.DataAnnotations;

namespace SHM.AppWebHonorarioMedico.Models;

/// <summary>
/// ViewModel para el listado de perfiles de aprobacion.
///
/// <author>ADG Antonio</author>
/// <created>2026-02-08</created>
/// </summary>
public class PerfilAprobacionListViewModel
{
    public List<PerfilAprobacionItemViewModel> Items { get; set; } = new();
}

/// <summary>
/// ViewModel para un item de perfil de aprobacion en el listado.
/// </summary>
public class PerfilAprobacionItemViewModel
{
    public string GuidRegistro { get; set; } = string.Empty;
    public string? GrupoFlujoTrabajo { get; set; }
    public string? Codigo { get; set; }
    public string? Descripcion { get; set; }
    public string? Nivel { get; set; }
    public int Orden { get; set; }
    public int Activo { get; set; }
}

/// <summary>
/// ViewModel para la creacion de un perfil de aprobacion.
/// </summary>
public class PerfilAprobacionCreateViewModel
{
    [Required(ErrorMessage = "El grupo de flujo de trabajo es requerido")]
    [StringLength(50, ErrorMessage = "El grupo no puede exceder 50 caracteres")]
    [Display(Name = "Grupo Flujo Trabajo")]
    public string? GrupoFlujoTrabajo { get; set; }

    [Required(ErrorMessage = "El codigo es requerido")]
    [StringLength(20, ErrorMessage = "El codigo no puede exceder 20 caracteres")]
    [Display(Name = "Codigo")]
    public string? Codigo { get; set; }

    [Required(ErrorMessage = "La descripcion es requerida")]
    [StringLength(200, ErrorMessage = "La descripcion no puede exceder 200 caracteres")]
    [Display(Name = "Descripcion")]
    public string? Descripcion { get; set; }

    [Required(ErrorMessage = "El nivel es requerido")]
    [StringLength(10, ErrorMessage = "El nivel no puede exceder 10 caracteres")]
    [Display(Name = "Nivel")]
    public string? Nivel { get; set; }

    [Required(ErrorMessage = "El orden es requerido")]
    [Range(1, 999, ErrorMessage = "El orden debe estar entre 1 y 999")]
    [Display(Name = "Orden")]
    public int Orden { get; set; }
}

/// <summary>
/// ViewModel para la edicion de un perfil de aprobacion.
/// </summary>
public class PerfilAprobacionEditViewModel
{
    public string GuidRegistro { get; set; } = string.Empty;

    [Required(ErrorMessage = "El grupo de flujo de trabajo es requerido")]
    [StringLength(50, ErrorMessage = "El grupo no puede exceder 50 caracteres")]
    [Display(Name = "Grupo Flujo Trabajo")]
    public string? GrupoFlujoTrabajo { get; set; }

    [Required(ErrorMessage = "El codigo es requerido")]
    [StringLength(20, ErrorMessage = "El codigo no puede exceder 20 caracteres")]
    [Display(Name = "Codigo")]
    public string? Codigo { get; set; }

    [Required(ErrorMessage = "La descripcion es requerida")]
    [StringLength(200, ErrorMessage = "La descripcion no puede exceder 200 caracteres")]
    [Display(Name = "Descripcion")]
    public string? Descripcion { get; set; }

    [Required(ErrorMessage = "El nivel es requerido")]
    [StringLength(10, ErrorMessage = "El nivel no puede exceder 10 caracteres")]
    [Display(Name = "Nivel")]
    public string? Nivel { get; set; }

    [Required(ErrorMessage = "El orden es requerido")]
    [Range(1, 999, ErrorMessage = "El orden debe estar entre 1 y 999")]
    [Display(Name = "Orden")]
    public int Orden { get; set; }

    [Display(Name = "Activo")]
    public int Activo { get; set; } = 1;
}

/// <summary>
/// ViewModel para la eliminacion de un perfil de aprobacion.
/// </summary>
public class PerfilAprobacionDeleteViewModel
{
    public string GuidRegistro { get; set; } = string.Empty;
    public string? GrupoFlujoTrabajo { get; set; }
    public string? Codigo { get; set; }
    public string? Descripcion { get; set; }
    public string? Nivel { get; set; }
    public int Orden { get; set; }
}
