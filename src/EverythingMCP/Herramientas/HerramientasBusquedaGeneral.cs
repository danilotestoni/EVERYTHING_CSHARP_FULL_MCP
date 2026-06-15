using System.ComponentModel;
using EverythingMCP.Nucleo.Excepciones;
using EverythingMCP.Nucleo.Interoperabilidad;
using EverythingMCP.Nucleo.Modelos;
using EverythingMCP.Servicios;
using ModelContextProtocol.Server;

namespace EverythingMCP.Herramientas;

[McpServerToolType]
public sealed class HerramientasBusquedaGeneral : HerramientasBase
{
    private readonly IServicioBusqueda _servicioBusqueda;

    public HerramientasBusquedaGeneral(IServicioBusqueda servicioBusqueda)
    {
        _servicioBusqueda = servicioBusqueda ?? throw new ArgumentNullException(nameof(servicioBusqueda));
    }

    [McpServerTool]
    [Description("Search for files and folders using Everything search syntax. Supports wildcards (* ?), operators (ext:, path:, size:, dm:, attrib:) and boolean operators (AND, OR, NOT).")]
    public async Task<string> BuscarArchivos(
        [Description("Search query. Examples: '*.pdf', 'informe 2024', 'ext:jpg path:C:\\Users'.")] string consulta,
        [Description("Maximum number of results to return (1-500). Default: 50.")] int maximoResultados = 50,
        [Description("Number of results to skip for pagination. Default: 0.")] int desplazamiento = 0,
        [Description("Whether the search is case-sensitive. Default: false.")] bool distinguirMayusculas = false,
        [Description("Whether to match whole words only. Default: false.")] bool palabraCompleta = false,
        [Description("Sort field: 'nombre', 'tamaño', 'fecha_modificacion', 'fecha_creacion', 'ruta'. Default: 'nombre'.")] string ordenarPor = "nombre",
        [Description("Sort direction: true for ascending, false for descending. Default: true.")] bool ascendente = true)
    {
        try
        {
            uint tipoOrdenacion = ObtenerTipoOrdenacion(ordenarPor, ascendente);

            var consultaObj = new ConsultaEverything
            {
                TextoBusqueda = consulta,
                MaximoResultados = (uint)Math.Clamp(maximoResultados, 1, 500),
                Desplazamiento = (uint)desplazamiento,
                DistinguirMayusculas = distinguirMayusculas,
                PalabraCompleta = palabraCompleta,
                TipoOrdenacion = tipoOrdenacion
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
    [Description("Search for files and folders using a regular expression pattern.")]
    public async Task<string> BuscarConRegex(
        [Description("Regular expression pattern to match against file names.")] string patron,
        [Description("Maximum number of results to return (1-500). Default: 50.")] int maximoResultados = 50,
        [Description("Whether the regex is case-sensitive. Default: false.")] bool distinguirMayusculas = false)
    {
        try
        {
            var consultaObj = new ConsultaEverything
            {
                TextoBusqueda = patron,
                MaximoResultados = (uint)Math.Clamp(maximoResultados, 1, 500),
                UsarExpresionRegular = true,
                DistinguirMayusculas = distinguirMayusculas
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
    [Description("Search for files or folders with an exact name match (case-insensitive whole-word match).")]
    public async Task<string> BuscarPorNombreExacto(
        [Description("Exact file or folder name to search for, e.g. 'report.pdf' or 'Documents'.")] string nombre,
        [Description("Maximum number of results to return (1-500). Default: 100.")] int maximoResultados = 100)
    {
        try
        {
            var consultaObj = new ConsultaEverything
            {
                TextoBusqueda = $"wfn:\"{nombre}\"",
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

    private static uint ObtenerTipoOrdenacion(string ordenarPor, bool ascendente) =>
        (ordenarPor?.ToLowerInvariant(), ascendente) switch
        {
            ("nombre", true) => EverythingInterop.SORT_NOMBRE_ASC,
            ("nombre", false) => EverythingInterop.SORT_NOMBRE_DESC,
            ("tamaño", true) => EverythingInterop.SORT_TAMAÑO_ASC,
            ("tamaño", false) => EverythingInterop.SORT_TAMAÑO_DESC,
            ("fecha_modificacion", true) => EverythingInterop.SORT_FECHA_MODIFICACION_ASC,
            ("fecha_modificacion", false) => EverythingInterop.SORT_FECHA_MODIFICACION_DESC,
            ("fecha_creacion", true) => EverythingInterop.SORT_FECHA_CREACION_ASC,
            ("fecha_creacion", false) => EverythingInterop.SORT_FECHA_CREACION_DESC,
            ("ruta", true) => EverythingInterop.SORT_RUTA_ASC,
            ("ruta", false) => EverythingInterop.SORT_RUTA_DESC,
            _ => EverythingInterop.SORT_NOMBRE_ASC
        };
}
