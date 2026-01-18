using Microsoft.Extensions.Configuration;

namespace SHM.AppInfrastructure.Configurations;

/// <summary>
/// Clase de configuracion para la conexion a base de datos Oracle.
/// Gestiona la cadena de conexion obtenida desde la configuracion de la aplicacion.
///
/// <author>ADG Antonio</author>
/// <created>2026-01-02</created>
/// </summary>
public class DatabaseConfig
{
    private readonly string _oraConnectionString;

    /// <summary>
    /// Constructor que inicializa la configuracion de base de datos.
    /// </summary>
    public DatabaseConfig(IConfiguration configuration)
    {
        _oraConnectionString = configuration.GetConnectionString("OracleConnection") ?? "";
    }

    /// <summary>
    /// Obtiene la cadena de conexion a la base de datos Oracle.
    /// </summary>
    public string GetOraConnectionString()
    {
        return _oraConnectionString;
    }
}
