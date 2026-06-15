using System.ComponentModel;
using EverythingMCP.Nucleo.Excepciones;
using EverythingMCP.Nucleo.Interoperabilidad;
using EverythingMCP.Nucleo.Modelos;
using EverythingMCP.Servicios;
using ModelContextProtocol.Server;

namespace EverythingMCP.Herramientas;

[McpServerToolType]
public sealed class HerramientasArchivos : HerramientasBase
{
    private readonly IServicioBusqueda _servicioBusqueda;

    public HerramientasArchivos(IServicioBusqueda servicioBusqueda)
    {
        _servicioBusqueda = servicioBusqueda ?? throw new ArgumentNullException(nameof(servicioBusqueda));
    }

    [McpServerTool]
    [Description("Find files modified within the last N days, sorted by most recently modified first.")]
    public async Task<string> ArchivosRecientes(
        [Description("Number of days to look back. Default: 7.")] int dias = 7,
        [Description("Optional file extension to filter by, e.g. 'pdf'.")] string? extension = null,
        [Description("Optional folder path to restrict the search.")] string? carpeta = null,
        [Description("Maximum number of results to return (1-500). Default: 50.")] int maximoResultados = 50)
    {
        try
        {
            string desde = DateTime.Today.AddDays(-Math.Abs(dias)).ToString("yyyy-MM-dd");
            string sintaxis = $"dm:>{desde}";

            if (!string.IsNullOrWhiteSpace(extension))
                sintaxis += $" ext:{extension}";

            if (!string.IsNullOrWhiteSpace(carpeta))
                sintaxis += $" path:\"{carpeta}\"";

            var consultaObj = new ConsultaEverything
            {
                TextoBusqueda = sintaxis,
                MaximoResultados = (uint)Math.Clamp(maximoResultados, 1, 500),
                TipoOrdenacion = EverythingInterop.SORT_FECHA_MODIFICACION_DESC
            };

            var respuesta = await _servicioBusqueda.BuscarAsync(consultaObj);
            return SerializarRespuesta(respuesta);
        }
        catch (Exception ex)
        {
            return SerializarExcepcion(ex);
        }
    }

    [McpServerTool]
    [Description("Find large files above a minimum size threshold, sorted by size descending.")]
    public async Task<string> ArchivosGrandes(
        [Description("Minimum file size. Supports units: b, kb, mb, gb. Default: '100mb'.")] string tamañoMinimo = "100mb",
        [Description("Optional folder path to restrict the search.")] string? carpeta = null,
        [Description("Maximum number of results to return (1-500). Default: 50.")] int maximoResultados = 50)
    {
        try
        {
            string sintaxis = $"size:>={NormalizarTamaño(tamañoMinimo)}";

            if (!string.IsNullOrWhiteSpace(carpeta))
                sintaxis += $" path:\"{carpeta}\"";

            var consultaObj = new ConsultaEverything
            {
                TextoBusqueda = sintaxis,
                MaximoResultados = (uint)Math.Clamp(maximoResultados, 1, 500),
                TipoOrdenacion = EverythingInterop.SORT_TAMAÑO_DESC
            };

            var respuesta = await _servicioBusqueda.BuscarAsync(consultaObj);
            return SerializarRespuesta(respuesta);
        }
        catch (Exception ex)
        {
            return SerializarExcepcion(ex);
        }
    }

    [McpServerTool]
    [Description("Find empty files (zero-byte files).")]
    public async Task<string> ArchivosVacios(
        [Description("Optional folder path to restrict the search.")] string? carpeta = null,
        [Description("Maximum number of results to return (1-500). Default: 100.")] int maximoResultados = 100)
    {
        try
        {
            string sintaxis = "size:0";

            if (!string.IsNullOrWhiteSpace(carpeta))
                sintaxis += $" path:\"{carpeta}\"";

            var consultaObj = new ConsultaEverything
            {
                TextoBusqueda = sintaxis,
                MaximoResultados = (uint)Math.Clamp(maximoResultados, 1, 500)
            };

            var respuesta = await _servicioBusqueda.BuscarAsync(consultaObj);
            return SerializarRespuesta(respuesta);
        }
        catch (Exception ex)
        {
            return SerializarExcepcion(ex);
        }
    }

    [McpServerTool]
    [Description("Find empty folders (folders with no files or subfolders).")]
    public async Task<string> CarpetasVacias(
        [Description("Optional folder path to restrict the search.")] string? carpeta = null,
        [Description("Maximum number of results to return (1-500). Default: 100.")] int maximoResultados = 100)
    {
        try
        {
            string sintaxis = "empty:";

            if (!string.IsNullOrWhiteSpace(carpeta))
                sintaxis += $" path:\"{carpeta}\"";

            var consultaObj = new ConsultaEverything
            {
                TextoBusqueda = sintaxis.Trim(),
                MaximoResultados = (uint)Math.Clamp(maximoResultados, 1, 500)
            };

            var respuesta = await _servicioBusqueda.BuscarAsync(consultaObj);
            return SerializarRespuesta(respuesta);
        }
        catch (Exception ex)
        {
            return SerializarExcepcion(ex);
        }
    }

    [McpServerTool]
    [Description("Find all files with a specific name across the entire drive (useful for detecting duplicates by name).")]
    public async Task<string> BuscarDuplicados(
        [Description("File name to search for, e.g. 'report.pdf'.")] string nombreArchivo,
        [Description("Maximum number of results to return (1-500). Default: 100.")] int maximoResultados = 100)
    {
        try
        {
            var consultaObj = new ConsultaEverything
            {
                TextoBusqueda = $"wfn:\"{nombreArchivo}\"",
                MaximoResultados = (uint)Math.Clamp(maximoResultados, 1, 500)
            };

            var respuesta = await _servicioBusqueda.BuscarAsync(consultaObj);
            return SerializarRespuesta(respuesta);
        }
        catch (Exception ex)
        {
            return SerializarExcepcion(ex);
        }
    }

    [McpServerTool]
    [Description("Find hidden files (files with the Hidden attribute set).")]
    public async Task<string> ArchivosOcultos(
        [Description("Optional folder path to restrict the search.")] string? carpeta = null,
        [Description("Optional additional search terms to narrow results.")] string? consulta = null,
        [Description("Maximum number of results to return (1-500). Default: 100.")] int maximoResultados = 100)
    {
        try
        {
            string sintaxis = "attrib:H";

            if (!string.IsNullOrWhiteSpace(consulta))
                sintaxis += $" {consulta}";

            if (!string.IsNullOrWhiteSpace(carpeta))
                sintaxis += $" path:\"{carpeta}\"";

            var consultaObj = new ConsultaEverything
            {
                TextoBusqueda = sintaxis,
                MaximoResultados = (uint)Math.Clamp(maximoResultados, 1, 500)
            };

            var respuesta = await _servicioBusqueda.BuscarAsync(consultaObj);
            return SerializarRespuesta(respuesta);
        }
        catch (Exception ex)
        {
            return SerializarExcepcion(ex);
        }
    }

    [McpServerTool]
    [Description("Find system files (files with the System attribute set).")]
    public async Task<string> ArchivosSistema(
        [Description("Optional folder path to restrict the search.")] string? carpeta = null,
        [Description("Maximum number of results to return (1-500). Default: 50.")] int maximoResultados = 50)
    {
        try
        {
            string sintaxis = "attrib:S";

            if (!string.IsNullOrWhiteSpace(carpeta))
                sintaxis += $" path:\"{carpeta}\"";

            var consultaObj = new ConsultaEverything
            {
                TextoBusqueda = sintaxis,
                MaximoResultados = (uint)Math.Clamp(maximoResultados, 1, 500)
            };

            var respuesta = await _servicioBusqueda.BuscarAsync(consultaObj);
            return SerializarRespuesta(respuesta);
        }
        catch (Exception ex)
        {
            return SerializarExcepcion(ex);
        }
    }

    [McpServerTool]
    [Description("Find read-only files (files with the Read-Only attribute set).")]
    public async Task<string> ArchivosSoloLectura(
        [Description("Optional folder path to restrict the search.")] string? carpeta = null,
        [Description("Maximum number of results to return (1-500). Default: 100.")] int maximoResultados = 100)
    {
        try
        {
            string sintaxis = "attrib:R";

            if (!string.IsNullOrWhiteSpace(carpeta))
                sintaxis += $" path:\"{carpeta}\"";

            var consultaObj = new ConsultaEverything
            {
                TextoBusqueda = sintaxis,
                MaximoResultados = (uint)Math.Clamp(maximoResultados, 1, 500)
            };

            var respuesta = await _servicioBusqueda.BuscarAsync(consultaObj);
            return SerializarRespuesta(respuesta);
        }
        catch (Exception ex)
        {
            return SerializarExcepcion(ex);
        }
    }

    [McpServerTool]
    [Description("Find files that have no file extension.")]
    public async Task<string> ArchivosSinExtension(
        [Description("Optional folder path to restrict the search.")] string? carpeta = null,
        [Description("Maximum number of results to return (1-500). Default: 100.")] int maximoResultados = 100)
    {
        try
        {
            string sintaxis = "ext:";

            if (!string.IsNullOrWhiteSpace(carpeta))
                sintaxis += $" path:\"{carpeta}\"";

            var consultaObj = new ConsultaEverything
            {
                TextoBusqueda = sintaxis.Trim(),
                MaximoResultados = (uint)Math.Clamp(maximoResultados, 1, 500)
            };

            var respuesta = await _servicioBusqueda.BuscarAsync(consultaObj);
            return SerializarRespuesta(respuesta);
        }
        catch (Exception ex)
        {
            return SerializarExcepcion(ex);
        }
    }
}
