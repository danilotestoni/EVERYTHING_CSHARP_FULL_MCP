using Xunit;
using FluentAssertions;
using EverythingMCP.Nucleo.Interoperabilidad;

namespace EverythingMCP.Tests.Nucleo;

/// <summary>
/// Verifica que nuestras constantes del SDK de Everything coinciden con las del header oficial
/// (Everything.h). Si voidtools libera una nueva versión del SDK con valores distintos, estos
/// tests fallarán y forzarán una revisión manual.
/// </summary>
public class ConstantesSdkTests
{
    [Theory]
    [InlineData(EverythingInterop.EVERYTHING_REQUEST_NOMBRE_ARCHIVO, 0x00000001u, "FILE_NAME")]
    [InlineData(EverythingInterop.EVERYTHING_REQUEST_RUTA, 0x00000002u, "PATH")]
    [InlineData(EverythingInterop.EVERYTHING_REQUEST_RUTA_COMPLETA, 0x00000004u, "FULL_PATH_AND_FILE_NAME")]
    [InlineData(EverythingInterop.EVERYTHING_REQUEST_EXTENSION, 0x00000008u, "EXTENSION")]
    [InlineData(EverythingInterop.EVERYTHING_REQUEST_TAMAÑO, 0x00000010u, "SIZE")]
    [InlineData(EverythingInterop.EVERYTHING_REQUEST_FECHA_CREACION, 0x00000020u, "DATE_CREATED")]
    [InlineData(EverythingInterop.EVERYTHING_REQUEST_FECHA_MODIFICACION, 0x00000040u, "DATE_MODIFIED")]
    [InlineData(EverythingInterop.EVERYTHING_REQUEST_FECHA_ACCESO, 0x00000080u, "DATE_ACCESSED")]
    [InlineData(EverythingInterop.EVERYTHING_REQUEST_ATRIBUTOS, 0x00000100u, "ATTRIBUTES")]
    public void RequestFlag_CoincideConHeaderSdk(uint valorActual, uint valorEsperado, string nombre)
    {
        valorActual.Should().Be(valorEsperado, $"el flag {nombre} del header oficial es 0x{valorEsperado:X8}");
    }

    [Theory]
    [InlineData(EverythingInterop.SORT_NOMBRE_ASC, 1u, "NAME_ASCENDING")]
    [InlineData(EverythingInterop.SORT_NOMBRE_DESC, 2u, "NAME_DESCENDING")]
    [InlineData(EverythingInterop.SORT_RUTA_ASC, 3u, "PATH_ASCENDING")]
    [InlineData(EverythingInterop.SORT_RUTA_DESC, 4u, "PATH_DESCENDING")]
    [InlineData(EverythingInterop.SORT_TAMAÑO_ASC, 5u, "SIZE_ASCENDING")]
    [InlineData(EverythingInterop.SORT_TAMAÑO_DESC, 6u, "SIZE_DESCENDING")]
    [InlineData(EverythingInterop.SORT_EXTENSION_ASC, 7u, "EXTENSION_ASCENDING")]
    [InlineData(EverythingInterop.SORT_EXTENSION_DESC, 8u, "EXTENSION_DESCENDING")]
    [InlineData(EverythingInterop.SORT_FECHA_CREACION_ASC, 11u, "DATE_CREATED_ASCENDING")]
    [InlineData(EverythingInterop.SORT_FECHA_CREACION_DESC, 12u, "DATE_CREATED_DESCENDING")]
    [InlineData(EverythingInterop.SORT_FECHA_MODIFICACION_ASC, 13u, "DATE_MODIFIED_ASCENDING")]
    [InlineData(EverythingInterop.SORT_FECHA_MODIFICACION_DESC, 14u, "DATE_MODIFIED_DESCENDING")]
    [InlineData(EverythingInterop.SORT_ATRIBUTOS_ASC, 15u, "ATTRIBUTES_ASCENDING")]
    [InlineData(EverythingInterop.SORT_ATRIBUTOS_DESC, 16u, "ATTRIBUTES_DESCENDING")]
    [InlineData(EverythingInterop.SORT_FECHA_ACCESO_ASC, 23u, "DATE_ACCESSED_ASCENDING")]
    [InlineData(EverythingInterop.SORT_FECHA_ACCESO_DESC, 24u, "DATE_ACCESSED_DESCENDING")]
    public void SortConstante_CoincideConHeaderSdk(uint valorActual, uint valorEsperado, string nombre)
    {
        valorActual.Should().Be(valorEsperado, $"EVERYTHING_SORT_{nombre} en el header oficial es {valorEsperado}");
    }

    [Theory]
    [InlineData(EverythingInterop.ERROR_OK, 0, "OK")]
    [InlineData(EverythingInterop.ERROR_MEMORIA, 1, "MEMORY")]
    [InlineData(EverythingInterop.ERROR_IPC, 2, "IPC")]
    [InlineData(EverythingInterop.ERROR_REGISTRO_CLASE, 3, "REGISTERCLASSEX")]
    [InlineData(EverythingInterop.ERROR_VENTANA, 4, "CREATEWINDOW")]
    [InlineData(EverythingInterop.ERROR_HILO, 5, "CREATETHREAD")]
    [InlineData(EverythingInterop.ERROR_INDICE_INVALIDO, 6, "INVALIDINDEX")]
    [InlineData(EverythingInterop.ERROR_LLAMADA_INVALIDA, 7, "INVALIDCALL")]
    [InlineData(EverythingInterop.ERROR_PETICION_INVALIDA, 8, "INVALIDREQUEST")]
    [InlineData(EverythingInterop.ERROR_PARAMETRO_INVALIDO, 9, "INVALIDPARAMETER")]
    public void CodigoError_CoincideConHeaderSdk(int valorActual, int valorEsperado, string nombre)
    {
        valorActual.Should().Be(valorEsperado, $"EVERYTHING_ERROR_{nombre} en el header oficial es {valorEsperado}");
    }

    [Theory]
    [InlineData(EverythingInterop.ATRIBUTO_SOLO_LECTURA, 0x00000001u, "READONLY")]
    [InlineData(EverythingInterop.ATRIBUTO_OCULTO, 0x00000002u, "HIDDEN")]
    [InlineData(EverythingInterop.ATRIBUTO_SISTEMA, 0x00000004u, "SYSTEM")]
    [InlineData(EverythingInterop.ATRIBUTO_DIRECTORIO, 0x00000010u, "DIRECTORY")]
    [InlineData(EverythingInterop.ATRIBUTO_ARCHIVO, 0x00000020u, "ARCHIVE")]
    public void AtributoArchivo_CoincideConWin32(uint valorActual, uint valorEsperado, string nombre)
    {
        valorActual.Should().Be(valorEsperado, $"FILE_ATTRIBUTE_{nombre} de Win32 es 0x{valorEsperado:X8}");
    }
}
