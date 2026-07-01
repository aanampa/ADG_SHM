namespace SHM.AppApplication.Helpers;

/// <summary>
/// Helper estatico para operaciones de almacenamiento de archivos.
/// Proporciona metodos utilitarios para el manejo de archivos.
///
/// <author>ADG Vladimir</author>
/// <created>2026-01-29</created>
/// </summary>
public static class FileStorageHelper
{
    /// <summary>
    /// Obtiene el tipo MIME segun la extension del archivo.
    /// </summary>
    /// <param name="extension">Extension del archivo (ej: .pdf)</param>
    /// <returns>Tipo MIME correspondiente</returns>
    public static string GetContentType(string? extension)
    {
        return extension?.ToLower() switch
        {
            ".pdf" => "application/pdf",
            ".xml" => "application/xml",
            ".zip" => "application/zip",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".doc" => "application/msword",
            ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            ".xls" => "application/vnd.ms-excel",
            ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            ".txt" => "text/plain",
            ".json" => "application/json",
            _ => "application/octet-stream"
        };
    }

    /// <summary>
    /// Valida si la extension del archivo es permitida.
    /// </summary>
    /// <param name="extension">Extension a validar</param>
    /// <param name="allowedExtensions">Lista de extensiones permitidas</param>
    /// <returns>True si la extension es permitida</returns>
    public static bool IsAllowedExtension(string? extension, params string[] allowedExtensions)
    {
        if (string.IsNullOrEmpty(extension))
            return false;

        var ext = extension.ToLower();
        return allowedExtensions.Any(allowed => allowed.ToLower() == ext);
    }

    /// <summary>
    /// Formatea el tamaño de un archivo en formato legible.
    /// </summary>
    /// <param name="bytes">Tamaño en bytes</param>
    /// <returns>Tamaño formateado (KB, MB, GB)</returns>
    public static string FormatFileSize(long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB", "TB" };
        int order = 0;
        double size = bytes;

        while (size >= 1024 && order < sizes.Length - 1)
        {
            order++;
            size /= 1024;
        }

        return $"{size:0.##} {sizes[order]}";
    }
}
