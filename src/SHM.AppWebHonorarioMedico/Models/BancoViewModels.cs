using System.ComponentModel.DataAnnotations;

namespace SHM.AppWebHonorarioMedico.Models;

public class BancoListViewModel
{
    public List<BancoItemViewModel> Items { get; set; } = new();
}

public class BancoItemViewModel
{
    public string GuidRegistro { get; set; } = string.Empty;
    public string? CodigoBanco { get; set; }
    public string? NombreBanco { get; set; }
    public int Activo { get; set; }
}

public class BancoCreateViewModel
{
    [Required(ErrorMessage = "El codigo del banco es requerido")]
    [StringLength(10, ErrorMessage = "El codigo no puede exceder 10 caracteres")]
    [Display(Name = "Codigo")]
    public string? CodigoBanco { get; set; }

    [Required(ErrorMessage = "El nombre del banco es requerido")]
    [StringLength(100, ErrorMessage = "El nombre no puede exceder 100 caracteres")]
    [Display(Name = "Nombre")]
    public string? NombreBanco { get; set; }
}

public class BancoEditViewModel
{
    public string GuidRegistro { get; set; } = string.Empty;

    [Required(ErrorMessage = "El codigo del banco es requerido")]
    [StringLength(10, ErrorMessage = "El codigo no puede exceder 10 caracteres")]
    [Display(Name = "Codigo")]
    public string? CodigoBanco { get; set; }

    [Required(ErrorMessage = "El nombre del banco es requerido")]
    [StringLength(100, ErrorMessage = "El nombre no puede exceder 100 caracteres")]
    [Display(Name = "Nombre")]
    public string? NombreBanco { get; set; }

    [Display(Name = "Activo")]
    public int Activo { get; set; } = 1;
}

public class BancoDeleteViewModel
{
    public string GuidRegistro { get; set; } = string.Empty;
    public string? CodigoBanco { get; set; }
    public string? NombreBanco { get; set; }
}
