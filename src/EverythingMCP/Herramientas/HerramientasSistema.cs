using System.ComponentModel;
using System.Text.Json;
using EverythingMCP.Nucleo.Excepciones;
using EverythingMCP.Nucleo.Modelos;
using EverythingMCP.Servicios;
using ModelContextProtocol.Server;

namespace EverythingMCP.Herramientas;

[McpServerToolType]
public sealed class HerramientasSistema : HerramientasBase
{
    private readonly IServicioBusqueda _servicioBusqueda;

    public HerramientasSistema(IServicioBusqueda servicioBusqueda)
    {
        _servicioBusqueda = servicioBusqueda ?? throw new ArgumentNullException(nameof(servicioBusqueda));
    }

    [McpServerTool]
    [Description("List the immediate contents of a folder (files and subfolders).")]
    public async Task<string> ListarCarpeta(
        [Description("Absolute folder path to list, e.g. 'C:\\Users\\John'.")] string carpeta,
        [Description("Maximum number of results to return (1-500). Default: 200.")] int maximoResultados = 200)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(carpeta))
                throw new ArgumentException("La ruta de la carpeta no puede estar vacía");

            var consultaObj = new ConsultaEverything
            {
                TextoBusqueda = $"parent:\"{carpeta}\"",
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
    [Description("Get details of a specific file or folder by its exact full path.")]
    public async Task<string> ObtenerDetalles(
        [Description("Full absolute path of the file or folder, e.g. 'C:\\Windows\\System32\\notepad.exe'.")] string ruta)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(ruta))
                throw new ArgumentException("La ruta no puede estar vacía");

            string directorio = Path.GetDirectoryName(ruta) ?? "";
            string nombre = Path.GetFileName(ruta) ?? "";

            // wfn: (whole filename) + parent: = localiza el archivo exacto en ese directorio
            string sintaxis = string.IsNullOrEmpty(directorio)
                ? $"wfn:\"{nombre}\""
                : $"wfn:\"{nombre}\" parent:\"{directorio}\"";

            var consultaObj = new ConsultaEverything
            {
                TextoBusqueda = sintaxis,
                MaximoResultados = 1
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
    [Description("Count the total number of matching files and folders without listing them. Useful for large result sets.")]
    public async Task<string> ContarResultados(
        [Description("Search query in Everything syntax.")] string consulta)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(consulta))
                throw new ArgumentException("La consulta no puede estar vacía");

            var consultaObj = new ConsultaEverything
            {
                TextoBusqueda = consulta,
                MaximoResultados = 1
            };

            var respuesta = await _servicioBusqueda.BuscarAsync(consultaObj);

            var resultado = new
            {
                archivos = respuesta.TotalArchivos,
                carpetas = respuesta.TotalCarpetas,
                total = respuesta.TotalDisponible
            };

            return JsonSerializer.Serialize(resultado, OpcionesJson);
        }
        catch (Exception ex)
        {
            return SerializarExcepcion(ex);
        }
    }

    [McpServerTool]
    [Description("Get statistics for a folder: file count, folder count, and approximate total size (based on up to 500 files).")]
    public async Task<string> EstadisticasCarpeta(
        [Description("Absolute folder path to analyze, e.g. 'C:\\Users\\John\\Documents'.")] string carpeta,
        [Description("If true, include all subfolders recursively. Default: true.")] bool recursivo = true)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(carpeta))
                throw new ArgumentException("La ruta de la carpeta no puede estar vacía");

            string sintaxis = recursivo
                ? $"path:\"{carpeta}\\\""
                : $"parent:\"{carpeta}\"";

            var consultaObj = new ConsultaEverything
            {
                TextoBusqueda = sintaxis,
                MaximoResultados = 500
            };

            var respuesta = await _servicioBusqueda.BuscarAsync(consultaObj);
            long tamañoMuestra = respuesta.Resultados.Sum(r => r.TamañoBytes);

            var resultado = new
            {
                carpeta,
                totalArchivos = respuesta.TotalArchivos,
                totalCarpetas = respuesta.TotalCarpetas,
                tamañoMuestraBytes = tamañoMuestra,
                tamañoMuestraFormateado = FormatearTamaño(tamañoMuestra),
                recursivo,
                nota = respuesta.TotalDisponible > 500
                    ? $"Tamaño calculado sobre los primeros 500 de {respuesta.TotalDisponible} elementos"
                    : (string?)null
            };

            return JsonSerializer.Serialize(resultado, OpcionesJson);
        }
        catch (Exception ex)
        {
            return SerializarExcepcion(ex);
        }
    }

    [McpServerTool]
    [Description("Search for executable files (exe, msi, bat, cmd, ps1, sh) optionally filtered by name or folder.")]
    public async Task<string> BuscarEjecutables(
        [Description("Optional name filter for the executable, e.g. 'setup'.")] string? nombre = null,
        [Description("Optional folder path to restrict the search.")] string? carpeta = null,
        [Description("Maximum number of results to return (1-500). Default: 50.")] int maximoResultados = 50)
    {
        try
        {
            string sintaxis = "ext:exe;msi;bat;cmd;ps1;sh";

            if (!string.IsNullOrWhiteSpace(nombre))
                sintaxis += $" wfn:*{nombre}*";

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
    [Description("Get the current status of the Everything search engine: availability, version, and database readiness.")]
    public string EstadoEverything()
    {
        try
        {
            bool disponible = _servicioBusqueda.EstaDisponible();
            string version = _servicioBusqueda.ObtenerVersion();

            var resultado = new
            {
                disponible,
                version,
                baseDatosLista = disponible,
                mensaje = disponible
                    ? "Everything está disponible y listo"
                    : "Everything no está disponible. Asegúrate de que está instalado y en ejecución."
            };

            return JsonSerializer.Serialize(resultado, OpcionesJson);
        }
        catch (Exception ex)
        {
            return SerializarExcepcion(ex);
        }
    }
}
