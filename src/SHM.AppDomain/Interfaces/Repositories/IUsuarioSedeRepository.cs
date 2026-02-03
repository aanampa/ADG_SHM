using SHM.AppDomain.Entities;

namespace SHM.AppDomain.Interfaces.Repositories;

/// <summary>
/// Interface del repositorio para la gestion de la relacion usuario-sede.
///
/// <author>Vladimir</author>
/// <created>2026-02-02</created>
/// </summary>
public interface IUsuarioSedeRepository
{
    /// <summary>
    /// Obtiene todas las sedes asignadas a un usuario.
    /// </summary>
    /// <param name="idUsuario">ID del usuario</param>
    /// <returns>Lista de relaciones usuario-sede</returns>
    Task<IEnumerable<UsuarioSede>> GetByUsuarioIdAsync(int idUsuario);

    /// <summary>
    /// Obtiene los IDs de las sedes asignadas a un usuario.
    /// </summary>
    /// <param name="idUsuario">ID del usuario</param>
    /// <returns>Lista de IDs de sedes</returns>
    Task<IEnumerable<int>> GetSedeIdsByUsuarioIdAsync(int idUsuario);

    /// <summary>
    /// Actualiza las sedes de un usuario usando la estrategia DELETE ALL + INSERT.
    /// </summary>
    /// <param name="idUsuario">ID del usuario</param>
    /// <param name="idsSedesSeleccionadas">Lista de IDs de sedes a asignar</param>
    /// <param name="idCreador">ID del usuario que realiza la operacion</param>
    /// <returns>True si la operacion fue exitosa</returns>
    Task<bool> UpdateSedesUsuarioAsync(int idUsuario, IEnumerable<int> idsSedesSeleccionadas, int idCreador);

    /// <summary>
    /// Obtiene la sede seleccionada para el usuario al iniciar sesion.
    /// Prioridad: ES_ULTIMA_SEDE = 1, si no existe toma el primer registro.
    /// </summary>
    /// <param name="idUsuario">ID del usuario</param>
    /// <returns>Tupla con IdSede y NombreSede, o null si no tiene sedes asignadas</returns>
    Task<(int IdSede, string NombreSede)?> GetSedeSeleccionadaLoginAsync(int idUsuario);
}
