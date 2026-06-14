using EverythingMCP.Nucleo.Excepciones;
using EverythingMCP.Nucleo.Modelos;

namespace EverythingMCP.Nucleo.Interoperabilidad;

/// <summary>
/// Wrapper thread-safe para el SDK de Everything. Serializa el acceso a las funciones P/Invoke
/// con un SemaphoreSlim para garantizar que solo un hilo ejecuta consultas a la vez.
/// </summary>
public sealed class EjecutorConsulta : IEjecutorConsulta
{
    private readonly SemaphoreSlim _semaforo = new(1, 1);

    public async Task<RespuestaBusqueda> EjecutarAsync(
        ConsultaEverything consulta,
        CancellationToken ct = default)
    {
        await _semaforo.WaitAsync(ct);
        try
        {
            if (!EverythingInterop.IsDBLoaded())
                throw new EverythingNoDisponibleException();

            EverythingInterop.SetSearch(consulta.TextoBusqueda);
            EverythingInterop.SetMatchCase(consulta.DistinguirMayusculas);
            EverythingInterop.SetMatchWholeWord(consulta.PalabraCompleta);
            EverythingInterop.SetRegex(consulta.UsarExpresionRegular);
            EverythingInterop.SetMax(consulta.MaximoResultados);
            EverythingInterop.SetOffset(consulta.Desplazamiento);
            EverythingInterop.SetRequestFlags(consulta.FlagsMetadatos);
            EverythingInterop.SetSort(consulta.TipoOrdenacion);

            if (!EverythingInterop.Query(true))
            {
                int codigoError = EverythingInterop.GetLastError();
                if (codigoError == EverythingInterop.ERROR_IPC)
                    throw new EverythingNoDisponibleException();
                throw new InvalidOperationException($"Error en la consulta: {EverythingInterop.TraducirError(codigoError)}");
            }

            int totalDisponible = EverythingInterop.GetNumResults();
            int totalArchivos = EverythingInterop.GetNumFileResults();
            int totalCarpetas = EverythingInterop.GetNumFolderResults();
            int totalDevuelto = (int)Math.Min(consulta.MaximoResultados, (uint)totalDisponible);

            var resultados = new List<ResultadoArchivo>(totalDevuelto);

            for (int i = 0; i < totalDevuelto; i++)
            {
                long tamaño = ObtenerTamaño(i);
                var resultado = new ResultadoArchivo
                {
                    RutaCompleta = EverythingInterop.GetResultFullPathName(i),
                    NombreArchivo = EverythingInterop.GetResultFileName(i),
                    Directorio = EverythingInterop.GetResultPath(i) ?? "",
                    Extension = EverythingInterop.GetResultExtension(i),
                    EsCarpeta = EverythingInterop.IsFolderResult(i),
                    TamañoBytes = tamaño,
                    TamañoFormateado = EverythingInterop.FormatearTamaño(tamaño),
                    FechaModificacion = ObtenerFechaModificacion(i),
                    FechaCreacion = ObtenerFechaCreacion(i),
                    FechaAcceso = ObtenerFechaAcceso(i),
                    EsOculto = TieneAtributo(i, EverythingInterop.ATRIBUTO_OCULTO),
                    EsSistema = TieneAtributo(i, EverythingInterop.ATRIBUTO_SISTEMA),
                    EsSoloLectura = TieneAtributo(i, EverythingInterop.ATRIBUTO_SOLO_LECTURA)
                };

                resultados.Add(resultado);
            }

            return new RespuestaBusqueda
            {
                TotalDisponible = totalDisponible,
                TotalDevuelto = totalDevuelto,
                TotalArchivos = totalArchivos,
                TotalCarpetas = totalCarpetas,
                Resultados = resultados.AsReadOnly()
            };
        }
        finally
        {
            _semaforo.Release();
        }
    }

    public bool ServicioDisponible() => EverythingInterop.IsDBLoaded();

    public string ObtenerVersionEverything() =>
        $"{EverythingInterop.GetMajorVersion()}.{EverythingInterop.GetMinorVersion()} " +
        $"(rev {EverythingInterop.GetRevision()}, build {EverythingInterop.GetBuildNumber()})";

    private static long ObtenerTamaño(int indice)
    {
        if (EverythingInterop.GetResultSize(indice, out long tamaño))
            return tamaño;
        return 0;
    }

    private static DateTime? ObtenerFechaModificacion(int indice)
    {
        if (EverythingInterop.GetResultDateModified(indice, out long fecha))
            return EverythingInterop.ConvertirFileTime(fecha);
        return null;
    }

    private static DateTime? ObtenerFechaCreacion(int indice)
    {
        if (EverythingInterop.GetResultDateCreated(indice, out long fecha))
            return EverythingInterop.ConvertirFileTime(fecha);
        return null;
    }

    private static DateTime? ObtenerFechaAcceso(int indice)
    {
        if (EverythingInterop.GetResultDateAccessed(indice, out long fecha))
            return EverythingInterop.ConvertirFileTime(fecha);
        return null;
    }

    private static bool TieneAtributo(int indice, uint atributo)
    {
        uint atributos = EverythingInterop.GetResultAttributes(indice);
        return (atributos & atributo) != 0;
    }
}
