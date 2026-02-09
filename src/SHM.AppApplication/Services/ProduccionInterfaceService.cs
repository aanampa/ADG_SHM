using System.Globalization;
using System.Transactions;
using Microsoft.Extensions.Logging;
using SHM.AppDomain.DTOs.EntidadMedica;
using SHM.AppDomain.DTOs.Produccion;
using SHM.AppDomain.Entities;
using SHM.AppDomain.Interfaces.Repositories;
using SHM.AppDomain.Interfaces.Services;

namespace SHM.AppApplication.Services;

/// <summary>
/// Servicio para la creacion masiva de producciones a traves de interface.
/// Resuelve codigos a IDs y crea las producciones en el sistema.
/// Valida duplicados por llave compuesta y maneja transacciones.
/// Si una entidad medica no existe localmente, la consulta del API externo de San Pablo y la registra.
///
/// <author>ADG Antonio</author>
/// <created>2026-01-19</created>
/// <modified>ADG Antonio - 2026-01-31 - Nueva llave compuesta, quitado Concepto y liquidacion, formato fecha dd/MM/yyyy HH:mm:ss</modified>
/// <modified>ADG Antonio - 2026-02-02 - Auto-registro de entidades medicas desde API San Pablo</modified>
/// <modified>ADG Antonio - 2026-02-08 - Detalle de estado por registro, sin abortar ante errores individuales</modified>
/// </summary>
public class ProduccionInterfaceService : IProduccionInterfaceService
{
    private readonly IProduccionRepository _produccionRepository;
    private readonly ISedeRepository _sedeRepository;
    private readonly IEntidadMedicaRepository _entidadMedicaRepository;
    private readonly IEntidadMedicaService _entidadMedicaService;
    private readonly ISanPabloApiService _sanPabloApiService;
    private readonly ILogger<ProduccionInterfaceService> _logger;

    public ProduccionInterfaceService(
        IProduccionRepository produccionRepository,
        ISedeRepository sedeRepository,
        IEntidadMedicaRepository entidadMedicaRepository,
        IEntidadMedicaService entidadMedicaService,
        ISanPabloApiService sanPabloApiService,
        ILogger<ProduccionInterfaceService> logger)
    {
        _produccionRepository = produccionRepository;
        _sedeRepository = sedeRepository;
        _entidadMedicaRepository = entidadMedicaRepository;
        _entidadMedicaService = entidadMedicaService;
        _sanPabloApiService = sanPabloApiService;
        _logger = logger;
    }

    /// <summary>
    /// Crea multiples producciones en el sistema a partir de una coleccion de DTOs.
    /// Valida duplicados por llave compuesta (IdSede, IdEntidadMedica, CodigoProduccion, NumeroProduccion, TipoEntidadMedica).
    /// Registra el estado individual de cada registro procesado.
    ///
    /// <modified>ADG Antonio - 2026-02-08 - Detalle de estado por registro, sin abortar ante errores individuales</modified>
    /// </summary>
    public async Task<InterfaceProduccionResultDto> CreateProduccionesAsync(IEnumerable<CreateInterfaceProduccionDto> createDtos, int idCreador)
    {
        var result = new InterfaceProduccionResultDto();
        var dtosList = createDtos.ToList();

        foreach (var createDto in dtosList)
        {
            var detalle = new InterfaceProduccionDetalleDto
            {
                CodigoSede = createDto.CodigoSede,
                CodigoEntidad = createDto.CodigoEntidad,
                CodigoProduccion = createDto.CodigoProduccion,
                TipoEntidadMedica = createDto.TipoEntidadMedica
            };

            try
            {
                // Obtener IdSede a partir del CodigoSede
                var sede = await _sedeRepository.GetByCodigoAsync(createDto.CodigoSede);
                if (sede == null)
                {
                    detalle.Estado = "ER";
                    detalle.Mensaje = $"Sede con codigo '{createDto.CodigoSede}' no encontrada";
                    result.CantidadErrores++;
                    result.Detalle.Add(detalle);
                    continue;
                }

                // Obtener IdEntidadMedica a partir del CodigoEntidad
                var entidadMedica = await _entidadMedicaRepository.GetByCodigoAsync(createDto.CodigoEntidad);

                // Si no existe localmente, intentar obtenerla del API de San Pablo y registrarla
                if (entidadMedica == null)
                {
                    _logger.LogInformation(
                        "Entidad medica '{CodigoEntidad}' no encontrada localmente. Consultando API San Pablo...",
                        createDto.CodigoEntidad);

                    entidadMedica = await GetOrCreateEntidadMedicaFromApiAsync(
                        createDto.CodigoSede,
                        createDto.TipoEntidadMedica,
                        createDto.CodigoEntidad,
                        idCreador);

                    if (entidadMedica == null)
                    {
                        detalle.Estado = "ER";
                        detalle.Mensaje = $"Entidad medica con codigo '{createDto.CodigoEntidad}' no encontrada localmente ni en el API de San Pablo";
                        result.CantidadErrores++;
                        result.Detalle.Add(detalle);
                        continue;
                    }
                }

                // Verificar si ya existe por llave compuesta
                var existe = await _produccionRepository.ExistsByKeyAsync(
                    sede.IdSede,
                    entidadMedica.IdEntidadMedica,
                    createDto.CodigoProduccion,
                    createDto.NumeroProduccion,
                    createDto.TipoEntidadMedica);

                if (existe)
                {
                    detalle.Estado = "OK";
                    detalle.Mensaje = "Registro ya existe, obviado";
                    result.CantidadObviados++;
                    result.Detalle.Add(detalle);
                    continue;
                }

                // Parsear fechas si vienen en el DTO (formato: dd/MM/yyyy HH:mm:ss)
                DateTime? fechaProduccion = null;
                if (!string.IsNullOrEmpty(createDto.FechaProduccion))
                {
                    if (DateTime.TryParseExact(createDto.FechaProduccion, "dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out var fechaProdParsed))
                    {
                        fechaProduccion = fechaProdParsed;
                    }
                    else if (DateTime.TryParse(createDto.FechaProduccion, out var fechaProdFallback))
                    {
                        fechaProduccion = fechaProdFallback;
                    }
                }

                var produccion = new Produccion
                {
                    IdSede = sede.IdSede,
                    IdEntidadMedica = entidadMedica.IdEntidadMedica,
                    CodigoProduccion = createDto.CodigoProduccion,
                    NumeroProduccion = createDto.NumeroProduccion,
                    TipoProduccion = createDto.TipoProduccion,
                    TipoEntidadMedica = createDto.TipoEntidadMedica,
                    TipoMedico = createDto.TipoMedico,
                    TipoRubro = createDto.TipoRubro,
                    Descripcion = createDto.Descripcion,
                    Periodo = createDto.Periodo,
                    FechaProduccion = fechaProduccion,
                    EstadoProduccion = createDto.EstadoProduccion,
                    MtoConsumo = createDto.MtoConsumo,
                    MtoDescuento = createDto.MtoDescuento,
                    MtoSubtotal = createDto.MtoSubtotal,
                    MtoRenta = createDto.MtoRenta,
                    MtoIgv = createDto.MtoIgv,
                    MtoTotal = createDto.MtoTotal,
                    IdCreador = idCreador,
                    Activo = 1
                };

                await _produccionRepository.CreateAsync(produccion);
                result.CantidadCreados++;

                detalle.Estado = "OK";
                detalle.Mensaje = "Creado exitosamente";
                result.Detalle.Add(detalle);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al procesar produccion: Sede={CodigoSede}, Entidad={CodigoEntidad}, Produccion={CodigoProduccion}",
                    createDto.CodigoSede, createDto.CodigoEntidad, createDto.CodigoProduccion);

                detalle.Estado = "ER";
                detalle.Mensaje = ex.Message;
                result.CantidadErrores++;
                result.Detalle.Add(detalle);
            }
        }

        return result;
    }

    /// <summary>
    /// Actualiza los datos de liquidacion de multiples producciones.
    /// Busca por llave compuesta (IdSede, IdEntidadMedica, CodigoProduccion, NumeroProduccion, TipoEntidadMedica).
    /// Registra el estado individual de cada registro procesado.
    ///
    /// <author>ADG Antonio</author>
    /// <created>2026-01-31</created>
    /// <modified>ADG Antonio - 2026-02-08 - Detalle de estado por registro, sin abortar ante errores individuales</modified>
    /// </summary>
    public async Task<InterfaceProduccionResultDto> UpdateLiquidacionesAsync(IEnumerable<UpdateInterfaceLiquidacionDto> updateDtos, int idModificador)
    {
        var result = new InterfaceProduccionResultDto();
        var dtosList = updateDtos.ToList();

        foreach (var updateDto in dtosList)
        {
            var detalle = new InterfaceProduccionDetalleDto
            {
                CodigoSede = updateDto.CodigoSede,
                CodigoEntidad = updateDto.CodigoEntidad,
                CodigoProduccion = updateDto.CodigoProduccion,
                TipoEntidadMedica = updateDto.TipoEntidadMedica
            };

            try
            {
                // Obtener IdSede a partir del CodigoSede
                var sede = await _sedeRepository.GetByCodigoAsync(updateDto.CodigoSede);
                if (sede == null)
                {
                    detalle.Estado = "ER";
                    detalle.Mensaje = $"Sede con codigo '{updateDto.CodigoSede}' no encontrada";
                    result.CantidadErrores++;
                    result.Detalle.Add(detalle);
                    continue;
                }

                // Obtener IdEntidadMedica a partir del CodigoEntidad
                var entidadMedica = await _entidadMedicaRepository.GetByCodigoAsync(updateDto.CodigoEntidad);
                if (entidadMedica == null)
                {
                    detalle.Estado = "ER";
                    detalle.Mensaje = $"Entidad medica con codigo '{updateDto.CodigoEntidad}' no encontrada";
                    result.CantidadErrores++;
                    result.Detalle.Add(detalle);
                    continue;
                }

                // Parsear FechaLiquidacion (formato: dd/MM/yyyy HH:mm:ss)
                DateTime fechaLiquidacion;
                if (!DateTime.TryParseExact(updateDto.FechaLiquidacion, "dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out fechaLiquidacion))
                {
                    if (!DateTime.TryParse(updateDto.FechaLiquidacion, out fechaLiquidacion))
                    {
                        detalle.Estado = "ER";
                        detalle.Mensaje = $"Formato de fecha invalido para FechaLiquidacion: '{updateDto.FechaLiquidacion}'. Use formato dd/MM/yyyy HH:mm:ss";
                        result.CantidadErrores++;
                        result.Detalle.Add(detalle);
                        continue;
                    }
                }

                // Verificar si existe la produccion por llave compuesta
                var existe = await _produccionRepository.ExistsByKeyAsync(
                    sede.IdSede,
                    entidadMedica.IdEntidadMedica,
                    updateDto.CodigoProduccion,
                    updateDto.NumeroProduccion,
                    updateDto.TipoEntidadMedica);

                if (!existe)
                {
                    detalle.Estado = "OK";
                    detalle.Mensaje = "Produccion no encontrada, obviado";
                    result.CantidadObviados++;
                    result.Detalle.Add(detalle);
                    continue;
                }

                var actualizado = await _produccionRepository.UpdateLiquidacionByKeyAsync(
                    sede.IdSede,
                    entidadMedica.IdEntidadMedica,
                    updateDto.CodigoProduccion,
                    updateDto.NumeroProduccion,
                    updateDto.TipoEntidadMedica,
                    updateDto.NumeroLiquidacion,
                    updateDto.CodigoLiquidacion,
                    updateDto.PeriodoLiquidacion,
                    updateDto.EstadoLiquidacion,
                    fechaLiquidacion,
                    updateDto.DescripcionLiquidacion,
                    updateDto.TipoLiquidacion,
                    idModificador);

                if (actualizado)
                {
                    result.CantidadCreados++;
                    detalle.Estado = "OK";
                    detalle.Mensaje = "Actualizado exitosamente";
                }
                else
                {
                    detalle.Estado = "ER";
                    detalle.Mensaje = "No se pudo actualizar la produccion";
                    result.CantidadErrores++;
                }

                result.Detalle.Add(detalle);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al procesar liquidacion: Sede={CodigoSede}, Entidad={CodigoEntidad}, Produccion={CodigoProduccion}",
                    updateDto.CodigoSede, updateDto.CodigoEntidad, updateDto.CodigoProduccion);

                detalle.Estado = "ER";
                detalle.Mensaje = ex.Message;
                result.CantidadErrores++;
                result.Detalle.Add(detalle);
            }
        }

        return result;
    }

    /// <summary>
    /// Obtiene una entidad medica del API de San Pablo y la registra localmente si no existe.
    /// </summary>
    /// <param name="codigoSede">Codigo de la sede.</param>
    /// <param name="tipoEntidad">Tipo de entidad (C=Compania, M=Medico).</param>
    /// <param name="codigoEntidad">Codigo de la entidad medica.</param>
    /// <param name="idCreador">ID del usuario creador.</param>
    /// <returns>La entidad medica creada o null si no se pudo obtener.</returns>
    private async Task<EntidadMedica?> GetOrCreateEntidadMedicaFromApiAsync(
        string codigoSede,
        string tipoEntidad,
        string codigoEntidad,
        int idCreador)
    {
        try
        {
            // Consultar el API de San Pablo
            var entidadApi = await _sanPabloApiService.GetEntidadMedicaAsync(codigoSede, tipoEntidad, codigoEntidad);

            if (entidadApi == null)
            {
                _logger.LogWarning(
                    "Entidad medica '{CodigoEntidad}' no encontrada en API San Pablo",
                    codigoEntidad);
                return null;
            }

            // Crear la entidad medica localmente
            // Mapeo de campos API San Pablo -> SHM:
            // - CODIGO -> CODIGO_ENTIDAD (CodigoEntidad)
            // - CODIGO_TIPOENTIDAD -> TIPO_ENTIDAD_MEDICA (TipoEntidadMedica)
            // - NOMBRE -> RAZON_SOCIAL (RazonSocial)
            // - RUC -> RUC (Ruc)
            // - DIRECCION -> DIRECCION (Direccion)
            // - CODIGO_SAP -> CODIGO_ACREEDOR (CodigoAcreedor)
            // - CODIGO_CORRENTISTA -> CODIGO_CORRIENTISTA (CodigoCorrentista)
            _logger.LogInformation(
                "Registrando nueva entidad medica desde API San Pablo. Codigo: {Codigo}, Nombre: {Nombre}",
                entidadApi.CODIGO, entidadApi.NOMBRE);

            var createDto = new CreateEntidadMedicaDto
            {
                CodigoEntidad = entidadApi.CODIGO ?? codigoEntidad,
                RazonSocial = entidadApi.NOMBRE,
                Ruc = entidadApi.RUC,
                TipoEntidadMedica = entidadApi.CODIGO_TIPOENTIDAD ?? tipoEntidad,
                Direccion = entidadApi.DIRECCION,
                CodigoAcreedor = entidadApi.CODIGO_SAP,
                CodigoCorrentista = entidadApi.CODIGO_CORRENTISTA
            };

            var entidadCreada = await _entidadMedicaService.CreateEntidadMedicaAsync(createDto, idCreador);

            if (entidadCreada == null)
            {
                _logger.LogError(
                    "Error al crear entidad medica localmente. Codigo: {Codigo}",
                    codigoEntidad);
                return null;
            }

            _logger.LogInformation(
                "Entidad medica registrada exitosamente. ID: {Id}, Codigo: {Codigo}",
                entidadCreada.IdEntidadMedica, entidadCreada.CodigoEntidad);

            // Obtener la entidad recien creada desde el repositorio usando el ID
            return await _entidadMedicaRepository.GetByIdAsync(entidadCreada.IdEntidadMedica);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error al obtener/crear entidad medica desde API San Pablo. Codigo: {CodigoEntidad}",
                codigoEntidad);
            return null;
        }
    }
}
