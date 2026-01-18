using System.ComponentModel.DataAnnotations;

namespace SHM.AppWebHonorarioMedico.Models;

public class MisDatosViewModel
{
    public int IdUsuario { get; set; }

    [Display(Name = "Usuario")]
    public string Login { get; set; } = string.Empty;

    [Display(Name = "Tipo de Usuario")]
    public string TipoUsuario { get; set; } = string.Empty;

    [Required(ErrorMessage = "El nombre es requerido")]
    [StringLength(100, ErrorMessage = "El nombre no puede exceder 100 caracteres")]
    [Display(Name = "Nombres")]
    public string? Nombres { get; set; }

    [Required(ErrorMessage = "El apellido paterno es requerido")]
    [StringLength(100, ErrorMessage = "El apellido paterno no puede exceder 100 caracteres")]
    [Display(Name = "Apellido Paterno")]
    public string? ApellidoPaterno { get; set; }

    [StringLength(100, ErrorMessage = "El apellido materno no puede exceder 100 caracteres")]
    [Display(Name = "Apellido Materno")]
    public string? ApellidoMaterno { get; set; }

    [Required(ErrorMessage = "El correo electrónico es requerido")]
    [EmailAddress(ErrorMessage = "El formato del correo electrónico no es válido")]
    [StringLength(150, ErrorMessage = "El correo no puede exceder 150 caracteres")]
    [Display(Name = "Correo Electrónico")]
    public string? Email { get; set; }

    [StringLength(20, ErrorMessage = "El número de documento no puede exceder 20 caracteres")]
    [Display(Name = "Número de Documento")]
    public string? NumeroDocumento { get; set; }

    [StringLength(20, ErrorMessage = "El celular no puede exceder 20 caracteres")]
    [Display(Name = "Celular")]
    public string? Celular { get; set; }

    // Campos para cambio de contraseña
    [DataType(DataType.Password)]
    [Display(Name = "Contraseña Actual")]
    public string? PasswordActual { get; set; }

    [DataType(DataType.Password)]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "La contraseña debe tener entre 6 y 100 caracteres")]
    [Display(Name = "Nueva Contraseña")]
    public string? PasswordNueva { get; set; }

    [DataType(DataType.Password)]
    [Compare("PasswordNueva", ErrorMessage = "Las contraseñas no coinciden")]
    [Display(Name = "Confirmar Nueva Contraseña")]
    public string? ConfirmarPassword { get; set; }

    // Campo para indicar si se está cambiando la contraseña
    public bool CambiarPassword { get; set; }

    // Campos de auditoría (solo lectura)
    [Display(Name = "Fecha de Creación")]
    public DateTime FechaCreacion { get; set; }

    [Display(Name = "Última Modificación")]
    public DateTime? FechaModificacion { get; set; }
}
