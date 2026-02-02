using System.Globalization;
using System.Transactions;
using SHM.AppDomain.DTOs.Produccion;
using SHM.AppDomain.Entities;
using SHM.AppDomain.Interfaces.Repositories;
using SHM.AppDomain.Interfaces.Services;

namespace SHM.AppApplication.Services;

/// <summary>
/// Servicio para la creacion masiva de producciones a traves de interface.
/// Resuelve codigos a IDs y crea las producciones en el sistema.
/// Valida duplicados por llave compuesta y maneja transacciones.
///
/// <author>ADG Antonio</author>
/// <created>2026-01-19</created>
/// <modified>ADG Antonio - 2026-01-31 - Nueva llave compuesta, quitado Concepto y liquidacion, formato fecha dd/MM/yyyy HH:mm:ss</modified>
/// </summary>
public class ProduccionInterfaceService : IProduccionInterfaceService
{
    private readonly IProduccionRepository _produccionRepository;
    private readonly ISedeRepository _sedeRepository;
    private readonly IEntidadMedicaRepository _entidadMedicaRepository;

    public ProduccionInterfaceService(
        IProduccionRepository produccionRepository,
        ISedeRepository sedeRepository,
        IEntidadMedicaRepository entidadMedicaRepository)
    {
        _produccionRepository = produccionRepository;
        _sedeRepository = sedeRepository;
        _entidadMedicaRepository = entidadMedicaRepository;
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
            if (entidadMedica == null)
                throw new ArgumentException($"Entidad medica con codigo '{createDto.CodigoEntidad}' no encontrada");

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
}
