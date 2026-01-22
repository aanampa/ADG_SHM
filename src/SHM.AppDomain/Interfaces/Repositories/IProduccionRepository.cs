using SHM.AppDomain.Entities;

namespace SHM.AppDomain.Interfaces.Repositories;

/// <summary>
/// Interface del repositorio para la gestion de producciones en la capa de persistencia.
///
/// <author>ADG Antonio</author>
/// <created>2026-01-02</created>
/// </summary>
public interface IProduccionRepository
{
    /// <summary>
    /// Obtiene todas las producciones registradas en el sistema.
    /// </summary>
    Task<IEnumerable<Produccion>> GetAllAsync();

    /// <summary>
    /// Obtiene una produccion por su identificador unico.
    /// </summary>
    Task<Produccion?> GetByIdAsync(int id);

    /// <summary>
    /// Obtiene una produccion por su codigo.
    /// </summary>
    Task<Produccion?> GetByCodigoAsync(string codigo);

    /// <summary>
    /// Obtiene una produccion por su GUID de registro.
    /// </summary>
    Task<Produccion?> GetByGuidAsync(string guidRegistro);

    /// <summary>
    /// Obtiene todas las producciones asociadas a una sede especifica.
    /// </summary>
    Task<IEnumerable<Produccion>> GetBySedeAsync(int idSede);

    /// <summary>
    /// Obtiene todas las producciones asociadas a una entidad medica especifica.
    /// </summary>
    Task<IEnumerable<Produccion>> GetByEntidadMedicaAsync(int idEntidadMedica);

    /// <summary>
    /// Obtiene todas las producciones de un periodo especifico.
    /// </summary>
    Task<IEnumerable<Produccion>> GetByPeriodoAsync(string periodo);

    /// <summary>
    /// Crea una nueva produccion en la base de datos.
    /// </summary>
    Task<int> CreateAsync(Produccion produccion);

    /// <summary>
    /// Actualiza los datos de una produccion existente.
    /// </summary>
    Task<bool> UpdateAsync(int id, Produccion produccion);

    /// <summary>
    /// Elimina una produccion registrando quien realizo la eliminacion.
    /// </summary>
    Task<bool> DeleteAsync(int id, int idModificador);

    /// <summary>
    /// Verifica si existe una produccion con el identificador especificado.
    /// </summary>
    Task<bool> ExistsAsync(int id);

    /// <summary>
    /// Verifica si existe una produccion con la llave compuesta (IdSede, IdEntidadMedica, CodigoProduccion).
    ///
    /// <author>ADG Antonio</author>
    /// <created>2026-01-19</created>
    /// </summary>
    Task<bool> ExistsByKeyAsync(int idSede, int idEntidadMedica, string codigoProduccion);
}
