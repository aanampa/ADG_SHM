using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace SHM.AppWebHonorarioMedico.Models;

public class CuentaBancariaListViewModel
{
    public List<CuentaBancariaItemViewModel> Items { get; set; } = new();
    public string EntidadGuid { get; set; } = string.Empty;
    public string EntidadRazonSocial { get; set; } = string.Empty;
    public int IdEntidadMedica { get; set; }
}

public class CuentaBancariaItemViewModel
{
    public string GuidRegistro { get; set; } = string.Empty;
    public string? BancoNombre { get; set; }
    public string? CuentaCorriente { get; set; }
    public string? CuentaCci { get; set; }
    public string? Moneda { get; set; }
    public string? MonedaDescripcion { get; set; }
    public int Activo { get; set; }
}

public class CuentaBancariaCreateViewModel
{
    public string EntidadGuid { get; set; } = string.Empty;

    [Required(ErrorMessage = "Debe seleccionar un banco")]
    [Display(Name = "Banco")]
    public int? IdBanco { get; set; }

    [Required(ErrorMessage = "La cuenta corriente es requerida")]
    [StringLength(30, ErrorMessage = "La cuenta corriente no puede exceder 30 caracteres")]
    [Display(Name = "Cuenta Corriente")]
    public string? CuentaCorriente { get; set; }

    [StringLength(30, ErrorMessage = "La cuenta CCI no puede exceder 30 caracteres")]
    [Display(Name = "Cuenta CCI")]
    public string? CuentaCci { get; set; }

    [Required(ErrorMessage = "Debe seleccionar la moneda")]
    [Display(Name = "Moneda")]
    public string? Moneda { get; set; }

    public List<SelectListItem> Bancos { get; set; } = new();
    public List<SelectListItem> Monedas { get; set; } = new();
}

public class CuentaBancariaEditViewModel
{
    public string GuidRegistro { get; set; } = string.Empty;
    public string EntidadGuid { get; set; } = string.Empty;

    [Required(ErrorMessage = "Debe seleccionar un banco")]
    [Display(Name = "Banco")]
    public int? IdBanco { get; set; }

    [Required(ErrorMessage = "La cuenta corriente es requerida")]
    [StringLength(30, ErrorMessage = "La cuenta corriente no puede exceder 30 caracteres")]
    [Display(Name = "Cuenta Corriente")]
    public string? CuentaCorriente { get; set; }

    [StringLength(30, ErrorMessage = "La cuenta CCI no puede exceder 30 caracteres")]
    [Display(Name = "Cuenta CCI")]
    public string? CuentaCci { get; set; }

    [Required(ErrorMessage = "Debe seleccionar la moneda")]
    [Display(Name = "Moneda")]
    public string? Moneda { get; set; }

    [Display(Name = "Activo")]
    public int Activo { get; set; } = 1;

    public List<SelectListItem> Bancos { get; set; } = new();
    public List<SelectListItem> Monedas { get; set; } = new();
}

public class CuentaBancariaDeleteViewModel
{
    public string GuidRegistro { get; set; } = string.Empty;
    public string EntidadGuid { get; set; } = string.Empty;
    public string? BancoNombre { get; set; }
    public string? CuentaCorriente { get; set; }
    public string? CuentaCci { get; set; }
    public string? MonedaDescripcion { get; set; }
}
