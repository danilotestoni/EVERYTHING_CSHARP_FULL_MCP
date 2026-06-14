using EverythingMCP.Nucleo.Modelos;

namespace EverythingMCP.Servicios;

/// <summary>
/// Contrato del servicio de búsqueda. Definir la interfaz antes que la implementación
/// permite testear las herramientas MCP sin depender de Everything real (DIP).
/// </summary>
public interface IServicioBusqueda
{
    Task<RespuestaBusqueda> BuscarAsync(ConsultaEverything consulta, CancellationToken ct = default);
    bool EstaDisponible();
    string ObtenerVersion();
}
