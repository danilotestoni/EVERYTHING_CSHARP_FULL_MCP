namespace EverythingMCP.Nucleo.Modelos;

/// <summary>
/// Agrupa los resultados de una búsqueda junto con información sobre el total disponible.
/// Permite al cliente MCP saber si hay más resultados sin haberlos listado todos.
/// </summary>
public sealed class RespuestaBusqueda
{
    public int TotalDisponible { get; init; }
    public int TotalDevuelto { get; init; }
    public int TotalArchivos { get; init; }
    public int TotalCarpetas { get; init; }
    public IReadOnlyList<ResultadoArchivo> Resultados { get; init; } = [];
}
