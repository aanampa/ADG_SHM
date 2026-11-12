using Dapper;
using Oracle.ManagedDataAccess.Client;
using SHM.AppDomain.Entities;
using SHM.AppDomain.Interfaces.Repositories;
using SHM.AppInfrastructure.Configurations;

namespace SHM.AppInfrastructure.Repositories;

/// <summary>
/// Repositorio del resultado de la liberacion de pagos en SAP (SHM_ORDEN_PAGO_LIBERACION).
/// Utiliza Dapper con Oracle Database.
///
/// <author>ADG Antonio</author>
/// <created>2026-11-11</created>
/// </summary>
public class OrdenPagoLiberacionRepository : IOrdenPagoLiberacionRepository
{
    private readonly string _connectionString;

    public OrdenPagoLiberacionRepository(DatabaseConfig databaseConfig)
    {
        _connectionString = databaseConfig.GetOraConnectionString();

        if (string.IsNullOrEmpty(_connectionString))
            throw new InvalidOperationException("La cadena de conexion de Oracle no esta configurada.");
    }

    /// <summary>
    /// Obtiene el resultado de liberacion de todos los comprobantes de una orden de pago.
    /// </summary>
    public async Task<IEnumerable<OrdenPagoLiberacion>> GetByOrdenPagoIdAsync(int idOrdenPago)
    {
        using var connection = new OracleConnection(_connectionString);

        var sql = @"
            SELECT
                ID_ORDEN_PAGO_LIBERACION as IdOrdenPagoLiberacion,
                ID_ORDEN_PAGO as IdOrdenPago,
                ID_PRODUCCION as IdProduccion,
                SOCIEDAD as Sociedad,
                DOCUMENTO as Documento,
                EJERCICIO as Ejercicio,
                DOCUMENTO_DT as DocumentoDt,
                ESTADO_LIBERACION as EstadoLiberacion,
                INTENTOS as Intentos,
                FECHA_LIBERACION as FechaLiberacion,
                MENSAJE_SAP as MensajeSap,
                RESPUESTA_SAP as RespuestaSap,
                GUID_REGISTRO as GuidRegistro,
                ACTIVO as Activo,
                ID_CREADOR as IdCreador,
                FECHA_CREACION as FechaCreacion,
                ID_MODIFICADOR as IdModificador,
                FECHA_MODIFICACION as FechaModificacion
            FROM SHM_ORDEN_PAGO_LIBERACION
            WHERE ID_ORDEN_PAGO = :IdOrdenPago
              AND ACTIVO = 1
            ORDER BY ID_PRODUCCION";

        return await connection.QueryAsync<OrdenPagoLiberacion>(sql, new { IdOrdenPago = idOrdenPago });
    }

    /// <summary>
    /// Inserta o actualiza (por orden de pago y produccion) el resultado de la liberacion.
    /// </summary>
    public async Task SaveResultadoAsync(OrdenPagoLiberacion item, int intento, int idUsuario)
    {
        using var connection = new OracleConnection(_connectionString);

        var sql = @"
            MERGE INTO SHM_ORDEN_PAGO_LIBERACION t
            USING (
                SELECT
                    :IdOrdenPago AS ID_ORDEN_PAGO,
                    :IdProduccion AS ID_PRODUCCION,
                    :Sociedad AS SOCIEDAD,
                    :Documento AS DOCUMENTO,
                    :Ejercicio AS EJERCICIO,
                    :DocumentoDt AS DOCUMENTO_DT,
                    :EstadoLiberacion AS ESTADO_LIBERACION,
                    :Intento AS INTENTO,
                    :FechaLiberacion AS FECHA_LIBERACION,
                    :MensajeSap AS MENSAJE_SAP,
                    :RespuestaSap AS RESPUESTA_SAP,
                    :IdUsuario AS ID_USUARIO
                FROM DUAL
            ) s
            ON (t.ID_ORDEN_PAGO = s.ID_ORDEN_PAGO AND t.ID_PRODUCCION = s.ID_PRODUCCION)
            WHEN MATCHED THEN UPDATE SET
                t.SOCIEDAD = s.SOCIEDAD,
                t.DOCUMENTO = s.DOCUMENTO,
                t.EJERCICIO = s.EJERCICIO,
                t.DOCUMENTO_DT = s.DOCUMENTO_DT,
                t.ESTADO_LIBERACION = s.ESTADO_LIBERACION,
                t.INTENTOS = t.INTENTOS + s.INTENTO,
                t.FECHA_LIBERACION = s.FECHA_LIBERACION,
                t.MENSAJE_SAP = s.MENSAJE_SAP,
                t.RESPUESTA_SAP = s.RESPUESTA_SAP,
                t.ACTIVO = 1,
                t.ID_MODIFICADOR = s.ID_USUARIO,
                t.FECHA_MODIFICACION = SYSDATE
            WHEN NOT MATCHED THEN INSERT (
                ID_ORDEN_PAGO_LIBERACION, ID_ORDEN_PAGO, ID_PRODUCCION, SOCIEDAD, DOCUMENTO,
                EJERCICIO, DOCUMENTO_DT, ESTADO_LIBERACION, INTENTOS, FECHA_LIBERACION,
                MENSAJE_SAP, RESPUESTA_SAP, GUID_REGISTRO, ACTIVO, ID_CREADOR, FECHA_CREACION
            ) VALUES (
                SHM_ORDEN_PAGO_LIBERACION_SEQ.NEXTVAL, s.ID_ORDEN_PAGO, s.ID_PRODUCCION, s.SOCIEDAD, s.DOCUMENTO,
                s.EJERCICIO, s.DOCUMENTO_DT, s.ESTADO_LIBERACION, s.INTENTO, s.FECHA_LIBERACION,
                s.MENSAJE_SAP, s.RESPUESTA_SAP, SYS_GUID(), 1, s.ID_USUARIO, SYSDATE
            )";

        await connection.ExecuteAsync(sql, new
        {
            item.IdOrdenPago,
            item.IdProduccion,
            item.Sociedad,
            item.Documento,
            item.Ejercicio,
            item.DocumentoDt,
            item.EstadoLiberacion,
            Intento = intento,
            item.FechaLiberacion,
            item.MensajeSap,
            item.RespuestaSap,
            IdUsuario = idUsuario
        });
    }
}
