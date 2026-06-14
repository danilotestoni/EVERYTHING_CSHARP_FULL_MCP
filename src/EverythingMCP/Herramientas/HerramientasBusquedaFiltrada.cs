using System.ComponentModel;
using EverythingMCP.Nucleo.Excepciones;
using EverythingMCP.Nucleo.Modelos;
using EverythingMCP.Servicios;
using ModelContextProtocol.Server;

namespace EverythingMCP.Herramientas;

[McpServerToolType]
public sealed class HerramientasBusquedaFiltrada : HerramientasBase
{
    private readonly IServicioBusqueda _servicioBusqueda;

    public HerramientasBusquedaFiltrada(IServicioBusqueda servicioBusqueda)
    {
        _servicioBusqueda = servicioBusqueda ?? throw new ArgumentNullException(nameof(servicioBusqueda));
    }

    [McpServerTool]
    [Description("List all files and folders directly inside a specific folder path.")]
    public async Task<string> BuscarEnCarpeta(
        [Description("Absolute folder path to list, e.g. 'C:\\Users\\John\\Documents'.")] string carpeta,
        [Description("Optional additional search terms to filter results within the folder.")] string? consulta = null,
        [Description("If true, search recursively including subfolders. Default: true.")] bool recursivo = true,
        [Description("Maximum number of results to return (1-500). Default: 100.")] int maximoResultados = 100)
    {
        try
        {
            string sintaxis = recursivo
                ? $"path:\"{carpeta}\\\""
                : $"parent:\"{carpeta}\"";

            if (!string.IsNullOrWhiteSpace(consulta))
                sintaxis += $" {consulta}";

            var consultaObj = new ConsultaEverything
            {
                TextoBusqueda = sintaxis,
                MaximoResultados = (uint)Math.Clamp(maximoResultados, 1, 500)
            };

            var respuesta = await _servicioBusqueda.BuscarAsync(consultaObj);
            return SerializarRespuesta(respuesta);
        }
        catch (EverythingNoDisponibleException ex)
        {
            return SerializarError(ex.Message);
        }
    }

    [McpServerTool]
    [Description("Search files by their file extension. Do not include the leading dot.")]
    public async Task<string> BuscarPorExtension(
        [Description("File extension without dot, e.g. 'pdf', 'docx', 'mp4'.")] string extension,
        [Description("Optional folder path to restrict the search.")] string? carpeta = null,
        [Description("Maximum number of results to return (1-500). Default: 100.")] int maximoResultados = 100)
    {
        try
        {
            string sintaxis = $"ext:{extension}";
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
        catch (EverythingNoDisponibleException ex)
        {
            return SerializarError(ex.Message);
        }
    }

    [McpServerTool]
    [Description("Search files by type category. Supported types: imagen, video, audio, documento, codigo, ejecutable, comprimido, fuente_datos.")]
    public async Task<string> BuscarPorTipo(
        [Description("File type category: 'imagen', 'video', 'audio', 'documento', 'codigo', 'ejecutable', 'comprimido', 'fuente_datos'.")] string tipo,
        [Description("Optional folder path to restrict the search.")] string? carpeta = null,
        [Description("Maximum number of results to return (1-500). Default: 100.")] int maximoResultados = 100)
    {
        try
        {
            string extensiones = tipo?.ToLowerInvariant() switch
            {
                "imagen" => "jpg;jpeg;png;gif;bmp;webp;svg;ico;tiff;raw;heic",
                "video" => "mp4;avi;mkv;mov;wmv;flv;webm;m4v;mpg;mpeg",
                "audio" => "mp3;wav;flac;aac;ogg;wma;m4a;opus",
                "documento" => "pdf;doc;docx;xls;xlsx;ppt;pptx;odt;ods;txt;rtf",
                "codigo" => "cs;js;ts;py;java;cpp;c;h;html;css;sql;json;xml;yaml;yml;md",
                "ejecutable" => "exe;msi;bat;cmd;ps1;sh",
                "comprimido" => "zip;rar;7z;tar;gz;bz2;xz",
                "fuente_datos" => "sql;db;sqlite;mdf;bak;csv;json;xml",
                _ => throw new ArgumentException($"Tipo desconocido: {tipo}")
            };

            string sintaxis = $"ext:{extensiones}";
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
        catch (EverythingNoDisponibleException ex)
        {
            return SerializarError(ex.Message);
        }
    }

    [McpServerTool]
    [Description("Search files by size range. At least one size parameter is required.")]
    public async Task<string> BuscarPorTamaño(
        [Description("Minimum size. Supports units: b, kb, mb, gb. E.g. '10mb', '500kb'.")] string? tamañoMinimo = null,
        [Description("Maximum size. Supports units: b, kb, mb, gb. E.g. '1gb', '100mb'.")] string? tamañoMaximo = null,
        [Description("Optional folder path to restrict the search.")] string? carpeta = null,
        [Description("Maximum number of results to return (1-500). Default: 50.")] int maximoResultados = 50)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(tamañoMinimo) && string.IsNullOrWhiteSpace(tamañoMaximo))
                return SerializarError("Debe especificar al menos un tamaño (tamañoMinimo o tamañoMaximo)");

            string sintaxis = "";

            if (!string.IsNullOrWhiteSpace(tamañoMinimo))
                sintaxis += $" size:>={NormalizarTamaño(tamañoMinimo)}";

            if (!string.IsNullOrWhiteSpace(tamañoMaximo))
                sintaxis += $" size:<={NormalizarTamaño(tamañoMaximo)}";

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
        catch (EverythingNoDisponibleException ex)
        {
            return SerializarError(ex.Message);
        }
    }

    [McpServerTool]
    [Description("Search files by their last modification date range. At least one date parameter is required.")]
    public async Task<string> BuscarPorFechaModificacion(
        [Description("Earliest modification date. Accepts: 'YYYY-MM-DD', '7d' (last 7 days), '1m' (last month), '1y' (last year).")] string? desde = null,
        [Description("Latest modification date. Accepts same formats as 'desde'.")] string? hasta = null,
        [Description("Optional folder path to restrict the search.")] string? carpeta = null,
        [Description("Optional file extension to filter by, e.g. 'pdf'.")] string? extension = null,
        [Description("Maximum number of results to return (1-500). Default: 100.")] int maximoResultados = 100)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(desde) && string.IsNullOrWhiteSpace(hasta))
                return SerializarError("Debe especificar al menos una fecha (desde o hasta)");

            string sintaxis = "";

            if (!string.IsNullOrWhiteSpace(desde))
                sintaxis += $" dm:>{ConvertirFechaRelativa(desde)}";

            if (!string.IsNullOrWhiteSpace(hasta))
                sintaxis += $" dm:<{ConvertirFechaRelativa(hasta)}";

            if (!string.IsNullOrWhiteSpace(extension))
                sintaxis += $" ext:{extension}";

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
        catch (EverythingNoDisponibleException ex)
        {
            return SerializarError(ex.Message);
        }
    }

    [McpServerTool]
    [Description("Search files by their creation date range. At least one date parameter is required.")]
    public async Task<string> BuscarPorFechaCreacion(
        [Description("Earliest creation date. Accepts: 'YYYY-MM-DD', '7d' (last 7 days), '1m' (last month), '1y' (last year).")] string? desde = null,
        [Description("Latest creation date. Accepts same formats as 'desde'.")] string? hasta = null,
        [Description("Optional folder path to restrict the search.")] string? carpeta = null,
        [Description("Optional file extension to filter by, e.g. 'pdf'.")] string? extension = null,
        [Description("Maximum number of results to return (1-500). Default: 100.")] int maximoResultados = 100)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(desde) && string.IsNullOrWhiteSpace(hasta))
                return SerializarError("Debe especificar al menos una fecha (desde o hasta)");

            string sintaxis = "";

            if (!string.IsNullOrWhiteSpace(desde))
                sintaxis += $" dc:>{ConvertirFechaRelativa(desde)}";

            if (!string.IsNullOrWhiteSpace(hasta))
                sintaxis += $" dc:<{ConvertirFechaRelativa(hasta)}";

            if (!string.IsNullOrWhiteSpace(extension))
                sintaxis += $" ext:{extension}";

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
        catch (EverythingNoDisponibleException ex)
        {
            return SerializarError(ex.Message);
        }
    }

    private static string ConvertirFechaRelativa(string fecha)
    {
        if (string.IsNullOrWhiteSpace(fecha))
            return DateTime.Today.ToString("yyyy-MM-dd");

        fecha = fecha.ToLowerInvariant().Trim();

        if (fecha.EndsWith("d") && int.TryParse(fecha[..^1], out int dias))
            return DateTime.Today.AddDays(-dias).ToString("yyyy-MM-dd");

        if (fecha.EndsWith("m") && int.TryParse(fecha[..^1], out int meses))
            return DateTime.Today.AddMonths(-meses).ToString("yyyy-MM-dd");

        if (fecha.EndsWith("y") && int.TryParse(fecha[..^1], out int años))
            return DateTime.Today.AddYears(-años).ToString("yyyy-MM-dd");

        if (DateTime.TryParse(fecha, out DateTime fechaParsed))
            return fechaParsed.ToString("yyyy-MM-dd");

        return DateTime.Today.ToString("yyyy-MM-dd");
    }
}
