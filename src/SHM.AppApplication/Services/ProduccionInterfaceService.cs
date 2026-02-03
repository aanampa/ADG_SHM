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
    /// Si hay error, aborta toda la operacion.
    /// </summary>
    public async Task<InterfaceProduccionResultDto> CreateProduccionesAsync(IEnumerable<CreateInterfaceProduccionDto> createDtos, int idCreador)
    {
        var result = new InterfaceProduccionResultDto();
        var dtosList = createDtos.ToList();

        // Fase 1: Validar todos los datos antes de crear (resolver codigos y verificar existencia)
        var produccionesParaCrear = new List<(CreateInterfaceProduccionDto Dto, int IdSede, int IdEntidadMedica)>();

        foreach (var createDto in dtosList)
        {
            // Obtener IdSede a partir del CodigoSede
            var sede = await _sedeRepository.GetByCodigoAsync(createDto.CodigoSede);
            if (sede == null)
                throw new ArgumentException($"Sede con codigo '{createDto.CodigoSede}' no encontrada");

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
                    throw new ArgumentException(
                        $"Entidad medica con codigo '{createDto.CodigoEntidad}' no encontrada localmente ni en el API de San Pablo");
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
                result.CantidadObviados++;
            }
            else
            {
                produccionesParaCrear.Add((createDto, sede.IdSede, entidadMedica.IdEntidadMedica));
            }
        }

        // Fase 2: Crear las producciones dentro de una transaccion
        if (produccionesParaCrear.Count > 0)
        {
            using var transactionScope = new TransactionScope(
                TransactionScopeOption.Required,
                new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted },
                TransactionScopeAsyncFlowOption.Enabled);

            foreach (var (dto, idSede, idEntidadMedica) in produccionesParaCrear)
            {
                // Parsear fechas si vienen en el DTO (formato: dd/MM/yyyy HH:mm:ss)
                DateTime? fechaProduccion = null;
                if (!string.IsNullOrEmpty(dto.FechaProduccion))
                {
                    if (DateTime.TryParseExact(dto.FechaProduccion, "dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out var fechaProdParsed))
                    {
                        fechaProduccion = fechaProdParsed;
                    }
                    else if (DateTime.TryParse(dto.FechaProduccion, out var fechaProdFallback))
                    {
                        fechaProduccion = fechaProdFallback;
                    }
                }

                var produccion = new Produccion
                {
                    IdSede = idSede,
                    IdEntidadMedica = idEntidadMedica,
                    CodigoProduccion = dto.CodigoProduccion,
                    NumeroProduccion = dto.NumeroProduccion,
                    TipoProduccion = dto.TipoProduccion,
                    TipoEntidadMedica = dto.TipoEntidadMedica,
                    TipoMedico = dto.TipoMedico,
                    TipoRubro = dto.TipoRubro,
                    Descripcion = dto.Descripcion,
                    Periodo = dto.Periodo,
                    FechaProduccion = fechaProduccion,
                    EstadoProduccion = dto.EstadoProduccion,
                    MtoConsumo = dto.MtoConsumo,
                    MtoDescuento = dto.MtoDescuento,
                    MtoSubtotal = dto.MtoSubtotal,
                    MtoRenta = dto.MtoRenta,
                    MtoIgv = dto.MtoIgv,
                    MtoTotal = dto.MtoTotal,
                    IdCreador = idCreador,
                    Activo = 1
                };

                await _produccionRepository.CreateAsync(produccion);
                result.CantidadCreados++;
            }

            transactionScope.Complete();
        }

        return result;
    }

    /// <summary>
    /// Actualiza los datos de liquidacion de multiples producciones.
    /// Busca por llave compuesta (IdSede, IdEntidadMedica, CodigoProduccion, NumeroProduccion, TipoEntidadMedica).
    /// Si hay error, aborta toda la operacion.
    ///
    /// <author>ADG Antonio</author>
    /// <created>2026-01-31</created>
    /// </summary>
    public async Task<InterfaceProduccionResultDto> UpdateLiquidacionesAsync(IEnumerable<UpdateInterfaceLiquidacionDto> updateDtos, int idModificador)
    {
        var result = new InterfaceProduccionResultDto();
        var dtosList = updateDtos.ToList();

        // Fase 1: Validar todos los datos antes de actualizar (resolver codigos y parsear fechas)
        var liquidacionesParaActualizar = new List<(UpdateInterfaceLiquidacionDto Dto, int IdSede, int IdEntidadMedica, DateTime FechaLiquidacion)>();

        foreach (var updateDto in dtosList)
        {
            // Obtener IdSede a partir del CodigoSede
            var sede = await _sedeRepository.GetByCodigoAsync(updateDto.CodigoSede);
            if (sede == null)
                throw new ArgumentException($"Sede con codigo '{updateDto.CodigoSede}' no encontrada");

            // Obtener IdEntidadMedica a partir del CodigoEntidad
            var entidadMedica = await _entidadMedicaRepository.GetByCodigoAsync(updateDto.CodigoEntidad);
            if (entidadMedica == null)
                throw new ArgumentException($"Entidad medica con codigo '{updateDto.CodigoEntidad}' no encontrada");

            // Parsear FechaLiquidacion (formato: dd/MM/yyyy HH:mm:ss)
            DateTime fechaLiquidacion;
            if (!DateTime.TryParseExact(updateDto.FechaLiquidacion, "dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out fechaLiquidacion))
            {
                if (!DateTime.TryParse(updateDto.FechaLiquidacion, out fechaLiquidacion))
                {
                    throw new ArgumentException($"Formato de fecha invalido para FechaLiquidacion: '{updateDto.FechaLiquidacion}'. Use formato dd/MM/yyyy HH:mm:ss");
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
                result.CantidadObviados++;
            }
            else
            {
                liquidacionesParaActualizar.Add((updateDto, sede.IdSede, entidadMedica.IdEntidadMedica, fechaLiquidacion));
            }
        }

        // Fase 2: Actualizar las liquidaciones dentro de una transaccion
        if (liquidacionesParaActualizar.Count > 0)
        {
            using var transactionScope = new TransactionScope(
                TransactionScopeOption.Required,
                new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted },
                TransactionScopeAsyncFlowOption.Enabled);

            foreach (var (dto, idSede, idEntidadMedica, fechaLiquidacion) in liquidacionesParaActualizar)
            {
                var actualizado = await _produccionRepository.UpdateLiquidacionByKeyAsync(
                    idSede,
                    idEntidadMedica,
                    dto.CodigoProduccion,
                    dto.NumeroProduccion,
                    dto.TipoEntidadMedica,
                    dto.NumeroLiquidacion,
                    dto.CodigoLiquidacion,
                    dto.PeriodoLiquidacion,
                    dto.EstadoLiquidacion,
                    fechaLiquidacion,
                    dto.DescripcionLiquidacion,
                    idModificador);

                if (actualizado)
                {
                    result.CantidadCreados++;
                }
            }

            transactionScope.Complete();
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
            // - Nombre -> RAZON_SOCIAL
            // - Tipo_Entidad -> TIPO_ENTIDAD_MEDICA
            // - Codigo_SAP -> CODIGO_ACREEDOR
            // - Codigo_Correntista -> CODIGO_CORRIENTISTA
            _logger.LogInformation(
                "Registrando nueva entidad medica desde API San Pablo. Codigo: {Codigo}, Nombre: {Nombre}",
                entidadApi.Codigo, entidadApi.Nombre);

            var createDto = new CreateEntidadMedicaDto
            {
                CodigoEntidad = entidadApi.Codigo ?? codigoEntidad,
                RazonSocial = entidadApi.Nombre,
                Ruc = entidadApi.Ruc,
                TipoEntidadMedica = entidadApi.Tipo_Entidad ?? tipoEntidad,
                Direccion = entidadApi.Direccion,
                CodigoAcreedor = entidadApi.Codigo_SAP,
                CodigoCorrientista = entidadApi.Codigo_Correntista
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
