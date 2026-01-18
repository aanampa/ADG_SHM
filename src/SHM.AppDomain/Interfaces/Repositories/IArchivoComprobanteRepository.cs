using SHM.AppDomain.Entities;

namespace SHM.AppDomain.Interfaces.Repositories;

/// <summary>
/// Interface del repositorio para la gestion de archivos comprobantes en la capa de persistencia.
///
/// <author>ADG Antonio</author>
/// <created>2026-01-02</created>
/// </summary>
public interface IArchivoComprobanteRepository
{
    /// <summary>
    /// Obtiene todos los archivos comprobantes registrados en el sistema.
    /// </summary>
    Task<IEnumerable<ArchivoComprobante>> GetAllAsync();

    /// <summary>
    /// Obtiene un archivo comprobante por su identificador unico.
    /// </summary>
    Task<ArchivoComprobante?> GetByIdAsync(int id);

    /// <summary>
    /// Obtiene todos los archivos comprobantes asociados a una produccion especifica.
    /// </summary>
    Task<IEnumerable<ArchivoComprobante>> GetByProduccionAsync(int idProduccion);

    /// <summary>
    /// Obtiene todos los comprobantes asociados a un archivo especifico.
    /// </summary>
    Task<IEnumerable<ArchivoComprobante>> GetByArchivoAsync(int idArchivo);

    /// <summary>
    /// Crea un nuevo archivo comprobante en la base de datos.
    /// </summary>
    Task<int> CreateAsync(ArchivoComprobante archivoComprobante);

    /// <summary>
    /// Actualiza los datos de un archivo comprobante existente.
    /// </summary>
    Task<bool> UpdateAsync(int id, ArchivoComprobante archivoComprobante);

    /// <summary>
    /// Elimina un archivo comprobante registrando quien realizo la eliminacion.
    /// </summary>
    Task<bool> DeleteAsync(int id, int idModificador);

    /// <summary>
    /// Verifica si existe un archivo comprobante con el identificador especificado.
    /// </summary>
    Task<bool> ExistsAsync(int id);
}
