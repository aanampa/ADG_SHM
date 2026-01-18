using System.ComponentModel.DataAnnotations;

namespace SHM.AppWebHonorarioMedico.Models;

public class SedeListViewModel
{
    public List<SedeItemViewModel> Items { get; set; } = new();
}

public class SedeItemViewModel
{
    public string GuidRegistro { get; set; } = string.Empty;
    public string? Codigo { get; set; }
    public string? Nombre { get; set; }
    public string? Ruc { get; set; }
    public string? Direccion { get; set; }
    public int Activo { get; set; }
}

public class SedeCreateViewModel
{
    [Required(ErrorMessage = "El codigo es requerido")]
    [StringLength(5, ErrorMessage = "El codigo no puede exceder 5 caracteres")]
    [Display(Name = "Codigo")]
    public string? Codigo { get; set; }

    [Required(ErrorMessage = "El nombre es requerido")]
    [StringLength(100, ErrorMessage = "El nombre no puede exceder 100 caracteres")]
    [Display(Name = "Nombre")]
    public string? Nombre { get; set; }

    [StringLength(20, ErrorMessage = "El RUC no puede exceder 20 caracteres")]
    [Display(Name = "RUC")]
    public string? Ruc { get; set; }

    [StringLength(300, ErrorMessage = "La direccion no puede exceder 300 caracteres")]
    [Display(Name = "Direccion")]
    public string? Direccion { get; set; }
}

public class SedeEditViewModel
{
    public string GuidRegistro { get; set; } = string.Empty;

    [Required(ErrorMessage = "El codigo es requerido")]
    [StringLength(5, ErrorMessage = "El codigo no puede exceder 5 caracteres")]
    [Display(Name = "Codigo")]
    public string? Codigo { get; set; }

    [Required(ErrorMessage = "El nombre es requerido")]
    [StringLength(100, ErrorMessage = "El nombre no puede exceder 100 caracteres")]
    [Display(Name = "Nombre")]
    public string? Nombre { get; set; }

    [StringLength(20, ErrorMessage = "El RUC no puede exceder 20 caracteres")]
    [Display(Name = "RUC")]
    public string? Ruc { get; set; }

    [StringLength(300, ErrorMessage = "La direccion no puede exceder 300 caracteres")]
    [Display(Name = "Direccion")]
    public string? Direccion { get; set; }

    [Display(Name = "Activo")]
    public int Activo { get; set; } = 1;
}

public class SedeDeleteViewModel
{
    public string GuidRegistro { get; set; } = string.Empty;
    public string? Codigo { get; set; }
    public string? Nombre { get; set; }
    public string? Ruc { get; set; }
    public string? Direccion { get; set; }
}
