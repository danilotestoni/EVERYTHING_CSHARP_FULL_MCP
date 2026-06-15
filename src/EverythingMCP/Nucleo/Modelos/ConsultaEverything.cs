using EverythingMCP.Nucleo.Interoperabilidad;

namespace EverythingMCP.Nucleo.Modelos;

/// <summary>
/// Encapsula todos los parámetros de una consulta al índice de Everything.
/// Se construye en las herramientas MCP y se pasa al servicio de búsqueda.
/// </summary>
public record ConsultaEverything
{
    public string TextoBusqueda { get; init; } = string.Empty;
    public bool DistinguirMayusculas { get; init; } = false;
    public bool PalabraCompleta { get; init; } = false;
    public bool UsarExpresionRegular { get; init; } = false;
    public uint MaximoResultados { get; init; } = 50;
    public uint Desplazamiento { get; init; } = 0;
    public uint TipoOrdenacion { get; init; } = EverythingInterop.SORT_NOMBRE_ASC;
    public uint FlagsMetadatos { get; init; } =
        EverythingInterop.EVERYTHING_REQUEST_NOMBRE_ARCHIVO |
        EverythingInterop.EVERYTHING_REQUEST_RUTA |
        EverythingInterop.EVERYTHING_REQUEST_RUTA_COMPLETA |
        EverythingInterop.EVERYTHING_REQUEST_EXTENSION |
        EverythingInterop.EVERYTHING_REQUEST_TAMAÑO |
        EverythingInterop.EVERYTHING_REQUEST_FECHA_CREACION |
        EverythingInterop.EVERYTHING_REQUEST_FECHA_MODIFICACION |
        EverythingInterop.EVERYTHING_REQUEST_FECHA_ACCESO |
        EverythingInterop.EVERYTHING_REQUEST_ATRIBUTOS;
}
