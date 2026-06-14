using EverythingMCP.Nucleo.Excepciones;
using EverythingMCP.Nucleo.Interoperabilidad;
using EverythingMCP.Nucleo.Modelos;

namespace EverythingMCP.Servicios;

/// <summary>
/// Implementación del servicio de búsqueda. Delega la ejecución a IEjecutorConsulta
/// y aplica validaciones y límites de seguridad antes de llamar al SDK.
/// </summary>
public sealed class ServicioBusqueda : IServicioBusqueda
{
    private const uint MAXIMO_RESULTADOS_PERMITIDO = 500;

    private readonly IEjecutorConsulta _ejecutor;

    public ServicioBusqueda(IEjecutorConsulta ejecutor)
    {
        _ejecutor = ejecutor ?? throw new ArgumentNullException(nameof(ejecutor));
    }

    public async Task<RespuestaBusqueda> BuscarAsync(ConsultaEverything consulta, CancellationToken ct = default)
    {
        if (consulta == null)
            throw new ArgumentNullException(nameof(consulta));

        if (string.IsNullOrWhiteSpace(consulta.TextoBusqueda))
            throw new ArgumentException("El texto de búsqueda no puede estar vacío", nameof(consulta));

        uint maximoResultados = Math.Min(consulta.MaximoResultados, MAXIMO_RESULTADOS_PERMITIDO);
        var consultaNormalizada = consulta with { MaximoResultados = maximoResultados };

        if (!EstaDisponible())
            throw new EverythingNoDisponibleException();

        return await _ejecutor.EjecutarAsync(consultaNormalizada, ct);
    }

    public bool EstaDisponible() => _ejecutor.ServicioDisponible();

    public string ObtenerVersion() => _ejecutor.ObtenerVersionEverything();
}
