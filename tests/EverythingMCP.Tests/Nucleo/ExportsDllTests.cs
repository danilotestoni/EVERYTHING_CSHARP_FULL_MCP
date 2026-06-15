using System.Runtime.InteropServices;
using Xunit;
using FluentAssertions;

namespace EverythingMCP.Tests.Nucleo;

/// <summary>
/// Verifica que cada función P/Invoke que llamamos existe realmente como export en
/// Everything64.dll. Estos tests se saltan si la DLL no está disponible (CI sin Everything).
///
/// Si voidtools renombrara un export, este test fallaría con un mensaje claro indicando
/// qué función no existe — y evitaría las "An error occurred" silenciosas en producción.
/// </summary>
public class ExportsDllTests : IDisposable
{
    private readonly IntPtr _hModule;
    private readonly string? _dllPath;

    public ExportsDllTests()
    {
        _dllPath = LocalizarDll();
        _hModule = _dllPath is not null ? LoadLibrary(_dllPath) : IntPtr.Zero;
    }

    public void Dispose()
    {
        if (_hModule != IntPtr.Zero) FreeLibrary(_hModule);
    }

    [SkippableTheory]
    [InlineData("Everything_SetSearchW")]
    [InlineData("Everything_SetMatchCase")]
    [InlineData("Everything_SetMatchWholeWord")]
    [InlineData("Everything_SetRegex")]
    [InlineData("Everything_SetMax")]
    [InlineData("Everything_SetOffset")]
    [InlineData("Everything_SetRequestFlags")]
    [InlineData("Everything_SetSort")]
    [InlineData("Everything_QueryW")]
    [InlineData("Everything_GetNumResults")]
    [InlineData("Everything_GetNumFileResults")]
    [InlineData("Everything_GetNumFolderResults")]
    [InlineData("Everything_GetTotFileResults")]
    [InlineData("Everything_GetTotFolderResults")]
    [InlineData("Everything_IsFileResult")]
    [InlineData("Everything_IsFolderResult")]
    [InlineData("Everything_GetResultFileNameW")]
    [InlineData("Everything_GetResultPathW")]
    [InlineData("Everything_GetResultFullPathNameW")]
    [InlineData("Everything_GetResultExtensionW")]
    [InlineData("Everything_GetResultSize")]
    [InlineData("Everything_GetResultDateModified")]
    [InlineData("Everything_GetResultDateCreated")]
    [InlineData("Everything_GetResultDateAccessed")]
    [InlineData("Everything_GetResultAttributes")]
    [InlineData("Everything_GetLastError")]
    [InlineData("Everything_CleanUp")]
    [InlineData("Everything_IsDBLoaded")]
    [InlineData("Everything_GetMajorVersion")]
    [InlineData("Everything_GetMinorVersion")]
    [InlineData("Everything_GetRevision")]
    [InlineData("Everything_GetBuildNumber")]
    public void ExportExiste_EnDllReal(string nombreFuncion)
    {
        Skip.If(_hModule == IntPtr.Zero, $"Everything64.dll no encontrada (probado: {_dllPath ?? "rutas estándar"})");

        IntPtr address = GetProcAddress(_hModule, nombreFuncion);
        address.Should().NotBe(IntPtr.Zero, $"el export {nombreFuncion} debería existir en {_dllPath}");
    }

    private static string? LocalizarDll()
    {
        // Prioridad: variable de entorno > junto al ejecutable > carpeta Lib del repo > instalación estándar.
        // Subir varios niveles desde bin/Release/netX/ hasta encontrar src/EverythingMCP/Lib/Everything64.dll.
        var candidatos = new List<string?>
        {
            Environment.GetEnvironmentVariable("EVERYTHING_DLL"),
            Path.Combine(AppContext.BaseDirectory, "Everything64.dll"),
            @"C:\Program Files\Everything\Everything64.dll"
        };

        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        for (int i = 0; i < 8 && dir is not null; i++, dir = dir.Parent)
        {
            candidatos.Add(Path.Combine(dir.FullName, "src", "EverythingMCP", "Lib", "Everything64.dll"));
        }

        return candidatos.FirstOrDefault(p => !string.IsNullOrEmpty(p) && File.Exists(p));
    }

    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    private static extern IntPtr LoadLibrary(string lpLibFileName);

    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Ansi)]
    private static extern IntPtr GetProcAddress(IntPtr hModule, string procName);

    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool FreeLibrary(IntPtr hModule);
}
