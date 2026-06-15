using System.Runtime.InteropServices;
using System.Text;

namespace EverythingMCP.Nucleo.Interoperabilidad;

/// <summary>
/// Contiene todas las declaraciones P/Invoke del SDK de Everything y constantes de la API.
/// Nunca usar directamente desde las Herramientas; siempre pasar por IServicioBusqueda.
/// </summary>
internal static class EverythingInterop
{
    private static readonly string NombreDll =
        Environment.GetEnvironmentVariable("EVERYTHING_DLL") ??
        @"C:\Program Files\Everything\Everything64.dll";

    #region Request Flags

    public const uint EVERYTHING_REQUEST_NOMBRE_ARCHIVO = 0x00000001;
    public const uint EVERYTHING_REQUEST_RUTA = 0x00000002;
    public const uint EVERYTHING_REQUEST_RUTA_COMPLETA = 0x00000004;
    public const uint EVERYTHING_REQUEST_EXTENSION = 0x00000008;
    public const uint EVERYTHING_REQUEST_TAMAÑO = 0x00000010;
    public const uint EVERYTHING_REQUEST_FECHA_CREACION = 0x00000020;
    public const uint EVERYTHING_REQUEST_FECHA_MODIFICACION = 0x00000040;
    public const uint EVERYTHING_REQUEST_FECHA_ACCESO = 0x00000080;
    public const uint EVERYTHING_REQUEST_ATRIBUTOS = 0x00000100;

    #endregion

    #region Sort Types

    public const uint SORT_NOMBRE_ASC = 1;
    public const uint SORT_NOMBRE_DESC = 2;
    public const uint SORT_RUTA_ASC = 3;
    public const uint SORT_RUTA_DESC = 4;
    public const uint SORT_TAMAÑO_ASC = 5;
    public const uint SORT_TAMAÑO_DESC = 6;
    public const uint SORT_FECHA_MODIFICACION_ASC = 11;
    public const uint SORT_FECHA_MODIFICACION_DESC = 12;
    public const uint SORT_FECHA_CREACION_ASC = 13;
    public const uint SORT_FECHA_CREACION_DESC = 14;
    public const uint SORT_FECHA_ACCESO_ASC = 15;
    public const uint SORT_FECHA_ACCESO_DESC = 16;

    #endregion

    #region Error Codes

    public const int ERROR_OK = 0;
    public const int ERROR_MEMORIA = 1;
    public const int ERROR_IPC = 2;
    public const int ERROR_REGISTRO_CLASE = 3;
    public const int ERROR_VENTANA = 4;
    public const int ERROR_HILO = 5;
    public const int ERROR_INDICE_INVALIDO = 6;
    public const int ERROR_LLAMADA_INVALIDA = 7;

    #endregion

    #region File Attributes

    public const uint ATRIBUTO_SOLO_LECTURA = 0x00000001;
    public const uint ATRIBUTO_OCULTO = 0x00000002;
    public const uint ATRIBUTO_SISTEMA = 0x00000004;
    public const uint ATRIBUTO_DIRECTORIO = 0x00000010;
    public const uint ATRIBUTO_ARCHIVO = 0x00000020;

    #endregion

    #region Kernel32 P/Invoke

    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    private static extern IntPtr LoadLibrary(string lpLibFileName);

    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Ansi)]
    private static extern IntPtr GetProcAddress(IntPtr hModule, string procName);

    #endregion

    #region Cached Module Handle

    private static readonly IntPtr _hModule = LoadLibrary(NombreDll);

    #endregion

    #region Delegates para P/Invoke
    // Marshal.GetDelegateForFunctionPointer<T> no admite tipos genéricos (Action<T>, Func<T>),
    // aunque estén completamente construidos. Todos los delegates deben ser tipos concretos con nombre.

    [UnmanagedFunctionPointer(CallingConvention.Winapi, CharSet = CharSet.Unicode)]
    private delegate void SetSearchProc([MarshalAs(UnmanagedType.LPWStr)] string lpSearchString);

    [UnmanagedFunctionPointer(CallingConvention.Winapi)]
    private delegate void SetBoolProc([MarshalAs(UnmanagedType.Bool)] bool bEnable);

    [UnmanagedFunctionPointer(CallingConvention.Winapi)]
    private delegate void SetUintProc(uint dwValue);

    [UnmanagedFunctionPointer(CallingConvention.Winapi)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private delegate bool QueryProc([MarshalAs(UnmanagedType.Bool)] bool bWait);

    [UnmanagedFunctionPointer(CallingConvention.Winapi)]
    private delegate int GetIntProc();

    [UnmanagedFunctionPointer(CallingConvention.Winapi)]
    private delegate uint GetUintProc();

    [UnmanagedFunctionPointer(CallingConvention.Winapi)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private delegate bool IndexBoolProc(int nIndex);

    // Retorna IntPtr en lugar de string para evitar que el marshaler llame a CoTaskMemFree
    // sobre punteros que pertenecen al buffer interno de Everything (no son CoTaskMem).
    [UnmanagedFunctionPointer(CallingConvention.Winapi)]
    private delegate IntPtr IndexRawStringProc(int nIndex);

    [UnmanagedFunctionPointer(CallingConvention.Winapi, CharSet = CharSet.Unicode)]
    private delegate uint GetFullPathNameProc(int nIndex, StringBuilder lpString, int nMaxCount);

    [UnmanagedFunctionPointer(CallingConvention.Winapi)]
    private delegate uint IndexUintProc(int nIndex);

    [UnmanagedFunctionPointer(CallingConvention.Winapi)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private delegate bool NoArgBoolProc();

    [UnmanagedFunctionPointer(CallingConvention.Winapi)]
    private delegate void NoArgVoidProc();

    [UnmanagedFunctionPointer(CallingConvention.Winapi)]
    private delegate bool Everything_GetResultSizeProc(int nIndex, out long lpFileSize);

    [UnmanagedFunctionPointer(CallingConvention.Winapi)]
    private delegate bool Everything_GetResultDateModifiedProc(int nIndex, out long lpDateModified);

    [UnmanagedFunctionPointer(CallingConvention.Winapi)]
    private delegate bool Everything_GetResultDateCreatedProc(int nIndex, out long lpDateCreated);

    [UnmanagedFunctionPointer(CallingConvention.Winapi)]
    private delegate bool Everything_GetResultDateAccessedProc(int nIndex, out long lpDateAccessed);

    #endregion

    #region SDK Wrappers

    internal static void SetSearch(string lpSearchString) =>
        InvokeFunction<SetSearchProc>("Everything_SetSearchW")(lpSearchString);

    internal static void SetMatchCase(bool bEnable) =>
        InvokeFunction<SetBoolProc>("Everything_SetMatchCase")(bEnable);

    internal static void SetMatchWholeWord(bool bEnable) =>
        InvokeFunction<SetBoolProc>("Everything_SetMatchWholeWord")(bEnable);

    internal static void SetRegex(bool bEnable) =>
        InvokeFunction<SetBoolProc>("Everything_SetRegex")(bEnable);

    internal static void SetMax(uint dwMax) =>
        InvokeFunction<SetUintProc>("Everything_SetMax")(dwMax);

    internal static void SetOffset(uint dwOffset) =>
        InvokeFunction<SetUintProc>("Everything_SetOffset")(dwOffset);

    internal static void SetRequestFlags(uint dwRequestFlags) =>
        InvokeFunction<SetUintProc>("Everything_SetRequestFlags")(dwRequestFlags);

    internal static void SetSort(uint dwSortType) =>
        InvokeFunction<SetUintProc>("Everything_SetSort")(dwSortType);

    internal static bool Query(bool bWait) =>
        InvokeFunction<QueryProc>("Everything_QueryW")(bWait);

    internal static int GetNumResults() =>
        InvokeFunction<GetIntProc>("Everything_GetNumResults")();

    internal static int GetNumFileResults() =>
        InvokeFunction<GetIntProc>("Everything_GetNumFileResults")();

    internal static int GetNumFolderResults() =>
        InvokeFunction<GetIntProc>("Everything_GetNumFolderResults")();

    internal static uint GetTotFileResults() =>
        InvokeFunction<GetUintProc>("Everything_GetTotFileResults")();

    internal static uint GetTotFolderResults() =>
        InvokeFunction<GetUintProc>("Everything_GetTotFolderResults")();

    internal static bool IsFileResult(int nIndex) =>
        InvokeFunction<IndexBoolProc>("Everything_IsFileResult")(nIndex);

    internal static bool IsFolderResult(int nIndex) =>
        InvokeFunction<IndexBoolProc>("Everything_IsFolderResult")(nIndex);

    internal static string GetResultFileName(int nIndex) =>
        Marshal.PtrToStringUni(InvokeFunction<IndexRawStringProc>("Everything_GetResultFileNameW")(nIndex)) ?? "";

    internal static string GetResultPath(int nIndex) =>
        Marshal.PtrToStringUni(InvokeFunction<IndexRawStringProc>("Everything_GetResultPathW")(nIndex)) ?? "";

    internal static string GetResultFullPathName(int nIndex)
    {
        // MAX_PATH = 260; para mayor robustez usamos un buffer más grande
        var sb = new StringBuilder(32767);
        InvokeFunction<GetFullPathNameProc>("Everything_GetResultFullPathNameW")(nIndex, sb, sb.Capacity);
        return sb.ToString();
    }

    internal static string GetResultExtension(int nIndex) =>
        Marshal.PtrToStringUni(InvokeFunction<IndexRawStringProc>("Everything_GetResultExtensionW")(nIndex)) ?? "";

    internal static bool GetResultSize(int nIndex, out long lpFileSize) =>
        InvokeFunction<Everything_GetResultSizeProc>("Everything_GetResultSize")(nIndex, out lpFileSize);

    internal static bool GetResultDateModified(int nIndex, out long lpDateModified) =>
        InvokeFunction<Everything_GetResultDateModifiedProc>("Everything_GetResultDateModified")(nIndex, out lpDateModified);

    internal static bool GetResultDateCreated(int nIndex, out long lpDateCreated) =>
        InvokeFunction<Everything_GetResultDateCreatedProc>("Everything_GetResultDateCreated")(nIndex, out lpDateCreated);

    internal static bool GetResultDateAccessed(int nIndex, out long lpDateAccessed) =>
        InvokeFunction<Everything_GetResultDateAccessedProc>("Everything_GetResultDateAccessed")(nIndex, out lpDateAccessed);

    internal static uint GetResultAttributes(int nIndex) =>
        InvokeFunction<IndexUintProc>("Everything_GetResultAttributes")(nIndex);

    internal static int GetLastError() =>
        InvokeFunction<GetIntProc>("Everything_GetLastError")();

    internal static void CleanUp() =>
        InvokeFunction<NoArgVoidProc>("Everything_CleanUp")();

    internal static bool IsDBLoaded() =>
        InvokeFunction<NoArgBoolProc>("Everything_IsDBLoaded")();

    internal static uint GetMajorVersion() =>
        InvokeFunction<GetUintProc>("Everything_GetMajorVersion")();

    internal static uint GetMinorVersion() =>
        InvokeFunction<GetUintProc>("Everything_GetMinorVersion")();

    internal static uint GetRevision() =>
        InvokeFunction<GetUintProc>("Everything_GetRevision")();

    internal static uint GetBuildNumber() =>
        InvokeFunction<GetUintProc>("Everything_GetBuildNumber")();

    #endregion

    #region Dynamic P/Invoke Helper

    private static T InvokeFunction<T>(string functionName) where T : Delegate
    {
        if (_hModule == IntPtr.Zero)
            throw new DllNotFoundException($"No se pudo cargar {NombreDll}");

        IntPtr procAddress = GetProcAddress(_hModule, functionName);
        if (procAddress == IntPtr.Zero)
            throw new EntryPointNotFoundException($"Función {functionName} no encontrada en {NombreDll}");

        return Marshal.GetDelegateForFunctionPointer<T>(procAddress);
    }

    #endregion

    #region Helper Methods (internal para uso de EjecutorConsulta)

    internal static string TraducirError(int codigoError) => codigoError switch
    {
        ERROR_OK => "Operación exitosa",
        ERROR_MEMORIA => "No hay memoria suficiente",
        ERROR_IPC => "Everything no está ejecutándose",
        ERROR_REGISTRO_CLASE => "Clase no registrada",
        ERROR_VENTANA => "Error en la ventana",
        ERROR_HILO => "Error en el hilo",
        ERROR_INDICE_INVALIDO => "Índice inválido",
        ERROR_LLAMADA_INVALIDA => "Llamada inválida",
        _ => $"Error desconocido (código: {codigoError})"
    };

    internal static DateTime? ConvertirFileTime(long fileTime)
    {
        if (fileTime == 0)
            return null;

        try { return DateTime.FromFileTime(fileTime); }
        catch { return null; }
    }

    internal static string FormatearTamaño(long bytes)
    {
        const long kb = 1024;
        const long mb = kb * 1024;
        const long gb = mb * 1024;

        return bytes switch
        {
            >= gb => $"{bytes / (double)gb:F2} GB",
            >= mb => $"{bytes / (double)mb:F2} MB",
            >= kb => $"{bytes / (double)kb:F2} KB",
            _ => $"{bytes} B"
        };
    }

    #endregion
}
