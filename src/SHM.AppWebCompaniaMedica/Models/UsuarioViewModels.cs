namespace SHM.AppWebCompaniaMedica.Models;

public class PerfilViewModel
{
    // Informacion del Usuario
    public string? Nombres { get; set; }
    public string? ApellidoPaterno { get; set; }
    public string? ApellidoMaterno { get; set; }
    public string? NombreCompleto { get; set; }
    public string? Iniciales { get; set; }
    public string? Email { get; set; }
    public string? NumeroDocumento { get; set; }
    public string? Celular { get; set; }
    public string? Telefono { get; set; }
    public string? Cargo { get; set; }
    public DateTime FechaCreacion { get; set; }

    // Informacion de la Entidad Medica
    public string? RazonSocial { get; set; }
    public string? Ruc { get; set; }
    public string? DireccionEntidad { get; set; }
    public string? TelefonoEntidad { get; set; }
    public string? CelularEntidad { get; set; }

    // Informacion de Cuentas Bancarias
    public List<CuentaBancariaViewModel> CuentasBancarias { get; set; } = new();
}

public class CuentaBancariaViewModel
{
    public string? NombreBanco { get; set; }
    public string? CuentaCorriente { get; set; }
    public string? CuentaCci { get; set; }
    public string? Moneda { get; set; }
}

public class ActualizarPerfilViewModel
{
    public string? Nombres { get; set; }
    public string? ApellidoPaterno { get; set; }
    public string? ApellidoMaterno { get; set; }
    public string? Email { get; set; }
    public string? NumeroDocumento { get; set; }
    public string? Celular { get; set; }
    public string? Telefono { get; set; }
    public string? Cargo { get; set; }
}

public class CambiarPasswordViewModel
{
    public string PasswordActual { get; set; } = string.Empty;
    public string PasswordNueva { get; set; } = string.Empty;
    public string ConfirmarPassword { get; set; } = string.Empty;
}
