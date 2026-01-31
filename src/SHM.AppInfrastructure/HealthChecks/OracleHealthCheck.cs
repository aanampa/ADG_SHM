using Microsoft.Extensions.Diagnostics.HealthChecks;
using Oracle.ManagedDataAccess.Client;
using SHM.AppInfrastructure.Configurations;

namespace SHM.AppInfrastructure.HealthChecks;

/// <summary>
/// Health Check personalizado para verificar la conexion a Oracle Database.
/// Ejecuta una consulta simple para validar que la base de datos esta disponible.
///
/// <author>ADG Antonio</author>
/// <created>2026-01-30</created>
/// </summary>
public class OracleHealthCheck : IHealthCheck
{
    private readonly string _connectionString;

    public OracleHealthCheck(DatabaseConfig databaseConfig)
    {
        _connectionString = databaseConfig.GetOraConnectionString();
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            using var connection = new OracleConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);

            using var command = connection.CreateCommand();
            command.CommandText = "SELECT SYSDATE FROM DUAL";
            var result = await command.ExecuteScalarAsync(cancellationToken);

            var serverTime = result?.ToString() ?? "N/A";

            return HealthCheckResult.Healthy($"Oracle Database conectada. Hora servidor: {serverTime}");
        }
        catch (OracleException ex)
        {
            return HealthCheckResult.Unhealthy(
                $"Error de conexion a Oracle: {ex.Message}",
                exception: ex);
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy(
                $"Error inesperado: {ex.Message}",
                exception: ex);
        }
    }
}
