using Microsoft.Extensions.Logging;
using SHM.AppDomain.Constants;
using SHM.AppDomain.DTOs.EntidadMedica;
using SHM.AppDomain.DTOs.Produccion;
using SHM.AppDomain.Entities;
using SHM.AppDomain.Interfaces.Repositories;
using SHM.AppDomain.Interfaces.Services;
using System.Globalization;

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
/// <modified>ADG Antonio - 2026-02-24 - Auto-registro de sedes desde API San Pablo</modified>
/// <modified>ADG Antonio - 2026-02-25 - Anulacion de comprobante (EstadoProduccion=9)</modified>
/// <modified>ADG Antonio - 2026-03-15 - Sincronizacion de cuentas bancarias desde SAP al registrar entidad</modified>
/// <modified>ADG Antonio - 2026-03-17 - Validacion de disponibilidad de servicios externos antes de procesar</modified>
/// </summary>
public class ProduccionInterfaceService : IProduccionInterfaceService
{
    private readonly IProduccionRepository _produccionRepository;
    private readonly ISedeRepository _sedeRepository;
    private readonly IEntidadMedicaRepository _entidadMedicaRepository;
    private readonly IEntidadMedicaService _entidadMedicaService;
    private readonly IArchivoComprobanteRepository _archivoComprobanteRepository;
    private readonly ISanPabloApiService _sanPabloApiService;
    private readonly ITablaDetalleRepository _tablaDetalleRepository;
    private readonly ISapApiService _sapApiService;
    private readonly IEntidadCuentaBancariaRepository _entidadCuentaBancariaRepository;
    private readonly IBancoRepository _bancoRepository;
    private readonly IParametroService _parametroService;
    private readonly ILogger<ProduccionInterfaceService> _logger;

    public ProduccionInterfaceService(
        IProduccionRepository produccionRepository,
        ISedeRepository sedeRepository,
        IEntidadMedicaRepository entidadMedicaRepository,
        IEntidadMedicaService entidadMedicaService,
        IArchivoComprobanteRepository archivoComprobanteRepository,
        ISanPabloApiService sanPabloApiService,
        ITablaDetalleRepository tablaDetalleRepository,
        ISapApiService sapApiService,
        IEntidadCuentaBancariaRepository entidadCuentaBancariaRepository,
        IBancoRepository bancoRepository,
        IParametroService parametroService,
        ILogger<ProduccionInterfaceService> logger)
    {
        _produccionRepository = produccionRepository;
        _sedeRepository = sedeRepository;
        _entidadMedicaRepository = entidadMedicaRepository;
        _entidadMedicaService = entidadMedicaService;
        _archivoComprobanteRepository = archivoComprobanteRepository;
        _sanPabloApiService = sanPabloApiService;
        _tablaDetalleRepository = tablaDetalleRepository;
        _sapApiService = sapApiService;
        _entidadCuentaBancariaRepository = entidadCuentaBancariaRepository;
        _bancoRepository = bancoRepository;
        _parametroService = parametroService;
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

        // Sincronizar bancos desde SAP antes de procesar el lote.
        // Garantiza que los bancos esten actualizados para la resolucion de cuentas bancarias.
        await SyncBancosFromSapAsync(idCreador);

        // Sincronizar todas las sedes desde API San Pablo antes de procesar el lote.
        // Las sedes son estables, por lo que esta llamada es eficiente: 1 sola llamada por lote.
        await SyncSedesFromApiAsync(idCreador);

        // Cache local de entidades medicas para el lote actual.
        // Evita consultas repetidas a BD cuando la misma entidad aparece en multiples producciones.
        var cacheEntidades = new Dictionary<string, EntidadMedica>(StringComparer.OrdinalIgnoreCase);

        foreach (var createDto in dtosList)
        {
            // Normalizar campos string: eliminar espacios en blanco al inicio y al final
            createDto.CodigoSede        = createDto.CodigoSede?.Trim() ?? "";
            createDto.CodigoEntidad     = createDto.CodigoEntidad?.Trim() ?? "";
            createDto.CodigoProduccion  = createDto.CodigoProduccion?.Trim() ?? "";
            createDto.NumeroProduccion  = createDto.NumeroProduccion?.Trim();
            createDto.TipoProduccion    = createDto.TipoProduccion?.Trim() ?? "";
            createDto.TipoEntidadMedica = createDto.TipoEntidadMedica?.Trim() ?? "";
            createDto.TipoMedico        = createDto.TipoMedico?.Trim() ?? "";
            createDto.TipoRubro         = createDto.TipoRubro?.Trim() ?? "";
            createDto.Descripcion       = createDto.Descripcion?.Trim() ?? "";
            createDto.Periodo           = createDto.Periodo?.Trim() ?? "";
            createDto.FechaProduccion   = createDto.FechaProduccion?.Trim() ?? "";
            createDto.EstadoProduccion  = createDto.EstadoProduccion?.Trim() ?? "";

            var detalle = new InterfaceProduccionDetalleDto
            {
                CodigoSede = createDto.CodigoSede,
                CodigoEntidad = createDto.CodigoEntidad,
                CodigoProduccion = createDto.CodigoProduccion,
                TipoEntidadMedica = createDto.TipoEntidadMedica
            };

            try
            {
                // Solo se aceptan producciones con EstadoProduccion 0 (anular produccion), 2 (crear) o 9 (anular comprobante)
                if (createDto.EstadoProduccion != "0" && createDto.EstadoProduccion != "2" && createDto.EstadoProduccion != "9")
                {
                    detalle.Estado = "ER";
                    detalle.Mensaje = $"EstadoProduccion '{createDto.EstadoProduccion}' no valido. Solo se acepta 0 (anular produccion), 2 (crear) o 9 (anular comprobante)";
                    result.CantidadErrores++;
                    result.Detalle.Add(detalle);
                    continue;
                }

                // Obtener IdSede a partir del CodigoSede (ya sincronizadas al inicio del lote)
                var sede = await _sedeRepository.GetByCodigoAsync(createDto.CodigoSede);

                // Fallback individual comentado: las sedes son estables y se sincronizan en bloque al inicio.
                // Descomentar si se requiere registrar sedes nuevas de forma dinamica por registro.
                //if (sede == null)
                //{
                //    _logger.LogInformation(
                //        "Sede '{CodigoSede}' no encontrada localmente. Consultando API San Pablo...",
                //        createDto.CodigoSede);
                //    sede = await GetOrCreateSedeFromApiAsync(createDto.CodigoSede, idCreador);
                //}

                if (sede == null)
                {
                    detalle.Estado = "ER";
                    detalle.Mensaje = $"Sede con codigo '{createDto.CodigoSede}' no encontrada";
                    result.CantidadErrores++;
                    result.Detalle.Add(detalle);
                    continue;
                }

                // Obtener IdEntidadMedica a partir del CodigoEntidad (con cache local del lote)
                if (!cacheEntidades.TryGetValue(createDto.CodigoEntidad, out var entidadMedica))
                {
                    entidadMedica = await _entidadMedicaRepository.GetByCodigoAsync(createDto.CodigoEntidad);

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
                    }

                    if (entidadMedica == null)
                    {
                        detalle.Estado = "ER";
                        detalle.Mensaje = $"Entidad medica con codigo '{createDto.CodigoEntidad}' no encontrada localmente ni en el API de San Pablo";
                        result.CantidadErrores++;
                        result.Detalle.Add(detalle);
                        continue;
                    }

                    cacheEntidades[createDto.CodigoEntidad] = entidadMedica;
                }

                // Sincronizar cuentas bancarias del acreedor desde SAP (best-effort, no aborta el flujo)
                await SyncCuentasBancariasFromSapAsync(entidadMedica.IdEntidadMedica, entidadMedica.CodigoAcreedor, idCreador);

                // Si EstadoProduccion = 0, anular la produccion existente
                if (createDto.EstadoProduccion == "0")
                {
                    var existeParaAnular = await _produccionRepository.ExistsByKeyAsync(
                        sede.IdSede,
                        entidadMedica.IdEntidadMedica,
                        createDto.CodigoProduccion,
                        createDto.NumeroProduccion,
                        createDto.TipoEntidadMedica);

                    if (existeParaAnular)
                    {
                        var anulado = await _produccionRepository.UpdateEstadoByKeyAsync(
                            sede.IdSede,
                            entidadMedica.IdEntidadMedica,
                            createDto.CodigoProduccion,
                            createDto.NumeroProduccion,
                            createDto.TipoEntidadMedica,
                            EstadoDescripcion.Produccion.ProduccionAnulada,
                            idCreador);

                        detalle.Estado = anulado ? "OK" : "ER";
                        detalle.Mensaje = anulado ? "Produccion anulada exitosamente" : "No se pudo anular la produccion";
                        if (anulado)
                            result.CantidadCreados++;
                        else
                            result.CantidadErrores++;
                    }
                    else
                    {
                        detalle.Estado = "OK";
                        detalle.Mensaje = "Produccion a anular no encontrada, obviado";
                        result.CantidadObviados++;
                    }

                    result.Detalle.Add(detalle);
                    continue;
                }

                // Si EstadoProduccion = 9, anular comprobante de la produccion existente
                if (createDto.EstadoProduccion == "9")
                {
                    var idProduccion = await _produccionRepository.AnularComprobanteByKeyAsync(
                        sede.IdSede,
                        entidadMedica.IdEntidadMedica,
                        createDto.CodigoProduccion,
                        createDto.NumeroProduccion,
                        createDto.TipoEntidadMedica,
                        EstadoDescripcion.Produccion.FacturaPendiente,
                        idCreador);

                    if (idProduccion.HasValue)
                    {
                        // Desactivar archivos comprobantes asociados
                        var archivosDesactivados = await _archivoComprobanteRepository.DeactivateByProduccionIdAsync(
                            idProduccion.Value, idCreador);

                        detalle.Estado = "OK";
                        detalle.Mensaje = $"Comprobante anulado exitosamente. {archivosDesactivados} archivo(s) desactivado(s)";
                        result.CantidadCreados++;
                    }
                    else
                    {
                        detalle.Estado = "OK";
                        detalle.Mensaje = "Produccion para anular comprobante no encontrada, obviado";
                        result.CantidadObviados++;
                    }

                    result.Detalle.Add(detalle);
                    continue;
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

                // Calcular TipoComprobante segun TipoEntidadMedica: 1=Factura(1), 0=RHE(22)
                var tipoComprobante = createDto.TipoEntidadMedica == "1" ? "1" : "22";

                // Calcular Concepto: "PRODUCCION {NumeroProduccion} - {descripcionTipoProduccion}"
                var detalleTipoProd = await _tablaDetalleRepository.GetByCodigoAsync("TIPO_PRODUCCION", createDto.TipoProduccion ?? "");
                var descripcionTipoProd = detalleTipoProd?.Descripcion?.ToUpper() ?? createDto.TipoProduccion ?? "";
                var concepto = $"PRODUCCION {createDto.NumeroProduccion} - {descripcionTipoProd}";

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
                    Estado = EstadoDescripcion.Produccion.FacturaPendiente,
                    TipoComprobante = tipoComprobante,
                    Concepto = concepto,
                    MtoConsumo = createDto.MtoConsumo,
                    MtoDescuento = createDto.MtoDescuento,
                    MtoSubtotal = createDto.MtoSubtotal,
                    MtoRenta = createDto.MtoRenta,
                    MtoIgv = createDto.MtoIgv,
                    MtoTotal = createDto.MtoTotal,
                    MtoDetraccion = createDto.MtoDetraccion,
                    PorcDetraccion = createDto.PorcDetraccion,
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
                var sede = await _sedeRepository.GetByCodigoAsync(updateDto.CodigoSede.Trim());
                if (sede == null)
                {
                    detalle.Estado = "ER";
                    detalle.Mensaje = $"Sede con codigo '{updateDto.CodigoSede}' no encontrada";
                    result.CantidadErrores++;
                    result.Detalle.Add(detalle);
                    continue;
                }

                // Obtener IdEntidadMedica a partir del CodigoEntidad
                var entidadMedica = await _entidadMedicaRepository.GetByCodigoAsync(updateDto.CodigoEntidad.Trim());
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
                    updateDto.CodigoProduccion.Trim(),
                    updateDto.NumeroProduccion.Trim(),
                    updateDto.TipoEntidadMedica.Trim());

                if (!existe)
                {
                    detalle.Estado = "ER";
                    detalle.Mensaje = "Produccion no encontrada";
                    result.CantidadErrores++;
                    result.Detalle.Add(detalle);
                    continue;
                }

                var actualizado = await _produccionRepository.UpdateLiquidacionByKeyAsync(
                    sede.IdSede,
                    entidadMedica.IdEntidadMedica,
                    updateDto.CodigoProduccion.Trim(),
                    updateDto.NumeroProduccion.Trim(),
                    updateDto.TipoEntidadMedica.Trim(),
                    updateDto.NumeroLiquidacion.Trim(),
                    updateDto.CodigoLiquidacion.Trim(),
                    updateDto.PeriodoLiquidacion.Trim(),
                    updateDto.EstadoLiquidacion.Trim(),
                    fechaLiquidacion,
                    updateDto.DescripcionLiquidacion.Trim(),
                    updateDto.TipoLiquidacion ?? "".Trim(),
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
                    detalle.Mensaje = "No se pudo actualizar la produccion. Solo se pueden liquidar producciones en estado FACTURA_ENVIADA_HHMM";
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
    /// Sincroniza los bancos desde SAP (COD_BANCOSet) y registra los que no existan localmente.
    /// Operacion best-effort: los errores se logean pero no abortan el flujo principal.
    ///
    /// Mapeo de campos SAP -> SHM:
    /// - CodigoBanco -> CODIGO_BANCO
    /// - DescripcionBanco -> NOMBRE_BANCO
    ///
    /// <author>ADG Antonio</author>
    /// <created>2026-03-17</created>
    /// </summary>
    private async Task SyncBancosFromSapAsync(int idCreador)
    {
        try
        {
            var bancosSap = await _sapApiService.GetBancosAsync();

            if (bancosSap == null || bancosSap.Count == 0)
            {
                _logger.LogWarning("No se obtuvieron bancos desde SAP para sincronizar");
                return;
            }

            _logger.LogInformation("Sincronizando {Count} bancos desde SAP", bancosSap.Count);

            int bancosCreados = 0;
            int bancosOmitidos = 0;

            foreach (var bancoSap in bancosSap)
            {
                if (string.IsNullOrWhiteSpace(bancoSap.CodigoBanco))
                    continue;

                var bancoLocal = await _bancoRepository.GetByCodigoAsync(bancoSap.CodigoBanco);
                if (bancoLocal != null)
                {
                    bancosOmitidos++;
                    continue;
                }

                var nuevoBanco = new Banco
                {
                    CodigoBanco = bancoSap.CodigoBanco,
                    NombreBanco = bancoSap.DescripcionBanco ?? string.Empty,
                    GuidRegistro = Guid.NewGuid().ToString(),
                    Activo = 1,
                    IdCreador = idCreador
                };

                var idBanco = await _bancoRepository.CreateAsync(nuevoBanco);
                if (idBanco > 0)
                {
                    bancosCreados++;
                    _logger.LogInformation("Banco sincronizado. ID: {Id}, Codigo: {Codigo}, Nombre: {Nombre}",
                        idBanco, bancoSap.CodigoBanco, bancoSap.DescripcionBanco);
                }
            }

            _logger.LogInformation("Sincronizacion de bancos completada. {Creados} nuevos, {Omitidos} ya existentes",
                bancosCreados, bancosOmitidos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al sincronizar bancos desde SAP");
        }
    }

    /// <summary>
    /// Verifica la disponibilidad de los servicios externos (SAP y San Pablo)
    /// intentando obtener sus tokens de acceso en paralelo.
    /// Retorna true si ambos servicios estan disponibles, junto con la lista de errores si alguno falla.
    ///
    /// <author>ADG Antonio</author>
    /// <created>2026-03-17</created>
    /// </summary>
    public async Task<(bool IsAvailable, List<string> Errors)> CheckExternalServicesAsync()
    {
        var errors = new List<string>();

        // Verificar ambos servicios en paralelo
        var taskSap = _sapApiService.CheckConnectionAsync();
        var taskSanPablo = _sanPabloApiService.CheckConnectionAsync();

        await Task.WhenAll(taskSap, taskSanPablo);

        if (!taskSap.Result.Ok)
        {
            errors.Add($"SAP: {taskSap.Result.Mensaje}");
            _logger.LogWarning("Servicio SAP no disponible: {Mensaje}", taskSap.Result.Mensaje);
        }

        if (!taskSanPablo.Result.Ok)
        {
            errors.Add($"San Pablo: {taskSanPablo.Result.Mensaje}");
            _logger.LogWarning("Servicio San Pablo no disponible: {Mensaje}", taskSanPablo.Result.Mensaje);
        }

        return (errors.Count == 0, errors);
    }

    /// <summary>
    /// Sincroniza todas las sedes desde el API de San Pablo.
    /// Consulta con Codigo=X para obtener las 32 sedes y registra las que no existan localmente.
    /// Retorna la cantidad de sedes nuevas registradas.
    ///
    /// <author>ADG Antonio</author>
    /// <created>2026-02-24</created>
    /// <modified>ADG Antonio - 2026-02-25 - Cambiado a publico para uso desde SedeInterfaceController</modified>
    /// </summary>
    public async Task<int> SyncSedesFromApiAsync(int idCreador)
    {
        try
        {
            var sedesApi = await _sanPabloApiService.GetAllSedesAsync();

            if (sedesApi.Count == 0)
            {
                _logger.LogWarning("No se obtuvieron sedes desde API San Pablo para sincronizar");
                return 0;
            }

            _logger.LogInformation("Sincronizando {Count} sedes desde API San Pablo", sedesApi.Count);

            int sedesCreadas = 0;
            int sedesActualizadas = 0;
            foreach (var sedeApi in sedesApi)
            {
                if (string.IsNullOrEmpty(sedeApi.CODIGO))
                    continue;

                // Verificar si ya existe localmente
                var sedeLocal = await _sedeRepository.GetByCodigoAsync(sedeApi.CODIGO);
                if (sedeLocal != null)
                {
                    // Actualizar nombre o RUC si cambiaron
                    if (sedeLocal.Nombre != sedeApi.DESCRIPCION || sedeLocal.Ruc != sedeApi.RUC)
                    {
                        sedeLocal.Nombre = sedeApi.DESCRIPCION;
                        sedeLocal.Ruc = sedeApi.RUC;
                        sedeLocal.IdModificador = idCreador;
                        await _sedeRepository.UpdateAsync(sedeLocal.IdSede, sedeLocal);
                        sedesActualizadas++;
                        _logger.LogInformation("Sede actualizada. ID: {Id}, Codigo: {Codigo}, Nombre: {Nombre}, Ruc: {Ruc}",
                            sedeLocal.IdSede, sedeApi.CODIGO, sedeApi.DESCRIPCION, sedeApi.RUC);
                    }
                    continue;
                }

                // Registrar sede nueva
                var sede = new Sede
                {
                    Codigo = sedeApi.CODIGO,
                    Nombre = sedeApi.DESCRIPCION,
                    Ruc = sedeApi.RUC,
                    Activo = 1,
                    IdCreador = idCreador
                };

                var idSede = await _sedeRepository.CreateAsync(sede);
                if (idSede > 0)
                {
                    sedesCreadas++;
                    _logger.LogInformation("Sede sincronizada. ID: {Id}, Codigo: {Codigo}, Nombre: {Nombre}",
                        idSede, sedeApi.CODIGO, sedeApi.DESCRIPCION);
                }
            }

            _logger.LogInformation("Sincronizacion de sedes completada. {Creadas} nuevas, {Actualizadas} actualizadas",
                sedesCreadas, sedesActualizadas);

            return sedesCreadas;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al sincronizar sedes desde API San Pablo");
            return 0;
        }
    }

    /// <summary>
    /// Obtiene una sede del API de San Pablo y la registra localmente si no existe.
    ///
    /// Mapeo de campos API San Pablo -> SHM:
    /// - CODIGO -> CODIGO (Codigo)
    /// - DESCRIPCION -> NOMBRE (Nombre)
    ///
    /// <author>ADG Antonio</author>
    /// <created>2026-02-24</created>
    /// </summary>
    private async Task<Sede?> GetOrCreateSedeFromApiAsync(string codigoSede, int idCreador)
    {
        try
        {
            var sedeApi = await _sanPabloApiService.GetSedeAsync(codigoSede);

            if (sedeApi == null)
            {
                _logger.LogWarning("Sede '{CodigoSede}' no encontrada en API San Pablo", codigoSede);
                return null;
            }

            _logger.LogInformation(
                "Registrando nueva sede desde API San Pablo. Codigo: {Codigo}, Descripcion: {Descripcion}",
                sedeApi.CODIGO, sedeApi.DESCRIPCION);

            var sede = new Sede
            {
                Codigo = sedeApi.CODIGO ?? codigoSede,
                Nombre = sedeApi.DESCRIPCION,
                Activo = 1,
                IdCreador = idCreador
            };

            var idSede = await _sedeRepository.CreateAsync(sede);

            if (idSede <= 0)
            {
                _logger.LogError("Error al crear sede localmente. Codigo: {Codigo}", codigoSede);
                return null;
            }

            _logger.LogInformation(
                "Sede registrada exitosamente. ID: {Id}, Codigo: {Codigo}",
                idSede, sede.Codigo);

            return await _sedeRepository.GetByIdAsync(idSede);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error al obtener/crear sede desde API San Pablo. Codigo: {CodigoSede}",
                codigoSede);
            return null;
        }
    }

    /// <summary>
    /// Sincroniza las cuentas bancarias de un acreedor desde SAP (CTA_ACREEDORSet)
    /// y las registra en SHM_ENTIDAD_CUENTA_BANCO si no existen.
    /// Operacion best-effort: los errores se logean pero no abortan el flujo principal.
    ///
    /// <author>ADG Antonio</author>
    /// <created>2026-03-15</created>
    /// </summary>
    private async Task SyncCuentasBancariasFromSapAsync(int idEntidadMedica, string? codigoAcreedor, int idCreador)
    {
        if (string.IsNullOrWhiteSpace(codigoAcreedor))
        {
            _logger.LogDebug("EntidadMedica {IdEntidad} no tiene CodigoAcreedor, se omite sincronizacion de cuentas bancarias", idEntidadMedica);
            return;
        }

        try
        {
            var cuentasSap = await _sapApiService.GetCuentasBancariasByAcreedorAsync(codigoAcreedor);

            if (cuentasSap == null || cuentasSap.Count == 0)
            {
                _logger.LogDebug("SAP no retorno cuentas bancarias para acreedor {CodigoAcreedor}", codigoAcreedor);
                return;
            }

            _logger.LogInformation("Sincronizando {Count} cuentas bancarias del acreedor {CodigoAcreedor} para EntidadMedica {IdEntidad}",
                cuentasSap.Count, codigoAcreedor, idEntidadMedica);

            // Obtener cuentas ya registradas localmente para esta entidad
            var cuentasLocales = (await _entidadCuentaBancariaRepository.GetByEntidadIdAsync(idEntidadMedica)).ToList();

            // Obtener codigos de bancos excluidos desde parametro SHM_EXCLUYE_BANCO
            var parametroExcluye = await _parametroService.GetParametroByCodigoAsync("SHM_EXCLUYE_BANCO");
            var bancosExcluidos = parametroExcluye?.Valor?
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .ToHashSet(StringComparer.OrdinalIgnoreCase)
                ?? new HashSet<string>();

            if (bancosExcluidos.Count > 0)
                _logger.LogDebug("Bancos excluidos de sincronizacion (SHM_EXCLUYE_BANCO): {Bancos}", string.Join(", ", bancosExcluidos));

            int creadas = 0;
            int omitidas = 0;

            foreach (var cuentaSap in cuentasSap)
            {
                // Excluir si el banco esta en la lista del parametro SHM_EXCLUYE_BANCO
                if (!string.IsNullOrWhiteSpace(cuentaSap.CodigoBanco) && bancosExcluidos.Contains(cuentaSap.CodigoBanco))
                {
                    _logger.LogDebug("Cuenta {NroCuenta} excluida por parametro SHM_EXCLUYE_BANCO. Banco: {CodigoBanco}",
                        cuentaSap.NroCuenta, cuentaSap.CodigoBanco);
                    omitidas++;
                    continue;
                }

                // Verificar si ya existe por NroCuenta
                var yaExiste = cuentasLocales.Any(c =>
                    string.Equals(c.CuentaCorriente, cuentaSap.NroCuenta, StringComparison.OrdinalIgnoreCase));

                if (yaExiste)
                {
                    omitidas++;
                    continue;
                }

                // Resolver CodigoBanco -> IdBanco
                int? idBanco = null;
                if (!string.IsNullOrWhiteSpace(cuentaSap.CodigoBanco))
                {
                    var banco = await _bancoRepository.GetByCodigoAsync(cuentaSap.CodigoBanco);
                    idBanco = banco?.IdBanco;

                    if (idBanco == null)
                        _logger.LogWarning("Banco con codigo '{CodigoBanco}' no encontrado localmente para acreedor {CodigoAcreedor}",
                            cuentaSap.CodigoBanco, codigoAcreedor);
                }

                var nuevaCuenta = new EntidadCuentaBancaria
                {
                    IdEntidad = idEntidadMedica,
                    IdBanco = idBanco,
                    CuentaCorriente = cuentaSap.NroCuenta,
                    CuentaCci = string.IsNullOrWhiteSpace(cuentaSap.NroCtaInterbancaria) ? null : cuentaSap.NroCtaInterbancaria,
                    Moneda = cuentaSap.Moneda,
                    Activo = 1,
                    IdCreador = idCreador
                };

                await _entidadCuentaBancariaRepository.CreateAsync(nuevaCuenta);
                creadas++;

                _logger.LogInformation("Cuenta bancaria creada. EntidadMedica: {IdEntidad}, NroCuenta: {NroCuenta}, Banco: {CodigoBanco}",
                    idEntidadMedica, cuentaSap.NroCuenta, cuentaSap.CodigoBanco);
            }

            _logger.LogInformation("Sincronizacion de cuentas bancarias completada. Acreedor: {CodigoAcreedor}, Creadas: {Creadas}, Omitidas: {Omitidas}",
                codigoAcreedor, creadas, omitidas);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al sincronizar cuentas bancarias del acreedor {CodigoAcreedor} para EntidadMedica {IdEntidad}",
                codigoAcreedor, idEntidadMedica);
        }
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
