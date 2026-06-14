namespace EverythingMCP.Nucleo.Excepciones;

/// <summary>
/// Se lanza cuando Everything no está ejecutándose o su base de datos no está cargada.
/// </summary>
public sealed class EverythingNoDisponibleException : Exception
{
    public EverythingNoDisponibleException()
        : base(
            "El servicio Everything no está disponible. " +
            "Asegúrate de que Everything está instalado en Windows y se ejecuta en segundo plano. " +
            "Descárgalo desde: https://www.voidtools.com/")
    {
    }

    public EverythingNoDisponibleException(string message) : base(message)
    {
    }

    public EverythingNoDisponibleException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
