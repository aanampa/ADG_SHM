using System.ComponentModel.DataAnnotations;

namespace SHM.AppWebHonorarioMedico.Models;

public class ParametroListViewModel
{
    public List<ParametroItemViewModel> Items { get; set; } = new();
}

public class ParametroItemViewModel
{
    public string GuidRegistro { get; set; } = string.Empty;
    public string? Codigo { get; set; }
    public string? Valor { get; set; }
    public int Activo { get; set; }
}

public class ParametroCreateViewModel
{
    [Required(ErrorMessage = "El codigo es requerido")]
    [MaxLength(100, ErrorMessage = "El codigo no puede exceder 100 caracteres")]
    public string? Codigo { get; set; }

    [MaxLength(512, ErrorMessage = "El valor no puede exceder 512 caracteres")]
    public string? Valor { get; set; }
}

public class ParametroEditViewModel
{
    public string GuidRegistro { get; set; } = string.Empty;

    [Required(ErrorMessage = "El codigo es requerido")]
    [MaxLength(100, ErrorMessage = "El codigo no puede exceder 100 caracteres")]
    public string? Codigo { get; set; }

    [MaxLength(512, ErrorMessage = "El valor no puede exceder 512 caracteres")]
    public string? Valor { get; set; }

    public int Activo { get; set; }
}

public class ParametroDeleteViewModel
{
    public string GuidRegistro { get; set; } = string.Empty;
    public string? Codigo { get; set; }
    public string? Valor { get; set; }
}
