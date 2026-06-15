using System.Text.Json;
using EverythingMCP.Nucleo.Excepciones;
using EverythingMCP.Nucleo.Modelos;

namespace EverythingMCP.Herramientas;

/// <summary>
/// Utilidades compartidas por todas las clases de herramientas MCP.
/// Centraliza serialización JSON y normalización de tamaño (DRY).
/// </summary>
public abstract class HerramientasBase
{
    // Reutilizar la misma instancia: JsonSerializerOptions tiene alto costo de inicialización
    protected static readonly JsonSerializerOptions OpcionesJson = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    protected static string SerializarRespuesta(RespuestaBusqueda respuesta) =>
        JsonSerializer.Serialize(respuesta, OpcionesJson);

    protected static string SerializarError(string mensaje) =>
        JsonSerializer.Serialize(new { error = mensaje }, OpcionesJson);

    /// <summary>
    /// Convierte cualquier excepción en un JSON de error con el tipo y mensaje.
    /// Las excepciones específicas se traducen a mensajes más amigables.
    /// </summary>
    protected static string SerializarExcepcion(Exception ex) => ex switch
    {
        EverythingNoDisponibleException => SerializarError(ex.Message),
        ArgumentException => SerializarError($"Parámetro inválido: {ex.Message}"),
        _ => SerializarError($"{ex.GetType().Name}: {ex.Message}")
    };

    protected static string NormalizarTamaño(string tamaño)
    {
        if (string.IsNullOrWhiteSpace(tamaño))
            throw new ArgumentException("El tamaño no puede estar vacío");

        string normalizado = tamaño.ToLowerInvariant().Trim();
        if (normalizado.EndsWith("gb") || normalizado.EndsWith("mb") ||
            normalizado.EndsWith("kb") || normalizado.EndsWith("b"))
            return normalizado;

        return $"{normalizado}mb";
    }

    protected static string FormatearTamaño(long bytes)
    {
        const long kb = 1024;
        const long mb = kb * 1024;
        const long gb = mb * 1024;

        return bytes switch
        {
            >= gb => $"{bytes / (double)gb:F2} GB",
            >= mb => $"{bytes / (double)mb:F2} MB",
            >= kb => $"{bytes / (double)kb:F2} KB",
            _ => $"{bytes} B"
        };
    }
}
