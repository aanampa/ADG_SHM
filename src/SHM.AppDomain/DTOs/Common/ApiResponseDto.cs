namespace SHM.AppDomain.DTOs.Common;

/// <summary>
/// DTO generico para respuestas de API estandarizadas.
///
/// <author>ADG Antonio</author>
/// <created>2026-01-31</created>
/// </summary>
public class ApiResponseDto<T>
{
    /// <summary>
    /// Indica si la operacion fue exitosa.
    /// </summary>
    public bool IsSuccess { get; set; }

    /// <summary>
    /// Mensaje descriptivo del resultado.
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Datos de la respuesta.
    /// </summary>
    public T? Data { get; set; }

    /// <summary>
    /// Lista de errores si los hubiera.
    /// </summary>
    public List<string> Errors { get; set; } = new();

    /// <summary>
    /// Crea una respuesta exitosa.
    /// </summary>
    public static ApiResponseDto<T> Success(T data, string message = "Correcto.")
    {
        return new ApiResponseDto<T>
        {
            IsSuccess = true,
            Message = message,
            Data = data,
            Errors = new List<string>()
        };
    }

    /// <summary>
    /// Crea una respuesta de error.
    /// </summary>
    public static ApiResponseDto<T> Error(string message, List<string>? errors = null)
    {
        return new ApiResponseDto<T>
        {
            IsSuccess = false,
            Message = message,
            Data = default,
            Errors = errors ?? new List<string>()
        };
    }

    /// <summary>
    /// Crea una respuesta de error con un solo error.
    /// </summary>
    public static ApiResponseDto<T> Error(string message, string error)
    {
        return new ApiResponseDto<T>
        {
            IsSuccess = false,
            Message = message,
            Data = default,
            Errors = new List<string> { error }
        };
    }
}
