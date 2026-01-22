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
    /// Valida duplicados por llave compuesta (IdSede, IdEntidadMedica, CodigoProduccion).
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
                createDto.CodigoProduccion);

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
                var produccion = new Produccion
                {
                    IdSede = idSede,
                    IdEntidadMedica = idEntidadMedica,
                    CodigoProduccion = dto.CodigoProduccion,
                    TipoProduccion = dto.TipoProduccion,
                    TipoMedico = dto.TipoMedico,
                    TipoRubro = dto.TipoRubro,
                    Descripcion = dto.Descripcion,
                    Periodo = dto.Periodo,
                    EstadoProduccion = dto.EstadoProduccion,
                    MtoConsumo = dto.MtoConsumo,
                    MtoDescuento = dto.MtoDescuento,
                    MtoSubtotal = dto.MtoSubtotal,
                    MtoRenta = dto.MtoRenta,
                    MtoIgv = dto.MtoIgv,
                    MtoTotal = dto.MtoTotal,
                    Concepto = dto.Concepto,
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
}
