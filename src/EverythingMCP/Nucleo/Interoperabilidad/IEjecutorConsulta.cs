using EverythingMCP.Nucleo.Modelos;

namespace EverythingMCP.Nucleo.Interoperabilidad;

public interface IEjecutorConsulta
{
    Task<RespuestaBusqueda> EjecutarAsync(ConsultaEverything consulta, CancellationToken ct = default);
    bool ServicioDisponible();
    string ObtenerVersionEverything();
}
