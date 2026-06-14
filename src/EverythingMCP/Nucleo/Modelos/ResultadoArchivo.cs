namespace EverythingMCP.Nucleo.Modelos;

/// <summary>
/// Representa un único archivo o carpeta devuelto por Everything.
/// </summary>
public sealed class ResultadoArchivo
{
    public string RutaCompleta { get; init; } = string.Empty;
    public string NombreArchivo { get; init; } = string.Empty;
    public string Directorio { get; init; } = string.Empty;
    public string Extension { get; init; } = string.Empty;
    public bool EsCarpeta { get; init; }
    public long TamañoBytes { get; init; }
    public string TamañoFormateado { get; init; } = string.Empty;
    public DateTime? FechaModificacion { get; init; }
    public DateTime? FechaCreacion { get; init; }
    public DateTime? FechaAcceso { get; init; }
    public bool EsOculto { get; init; }
    public bool EsSistema { get; init; }
    public bool EsSoloLectura { get; init; }
}
