# Instrucciones del proyecto para agentes de IA

## Qué es este proyecto

Servidor MCP (Model Context Protocol) en C# que expone las capacidades de búsqueda de voidtools Everything a cualquier cliente IA compatible (Claude Desktop, Cursor, etc.). Permite buscar archivos en Windows de forma instantánea desde un agente IA.

## Arquitectura

Sigue Clean Architecture con separación clara en capas:

- **Nucleo**: modelos de dominio y la interoperabilidad con la DLL de Everything. Sin dependencias externas. Contiene `EverythingInterop.cs` que implementa P/Invoke y `EjecutorConsulta` thread-safe.
- **Servicios**: lógica de negocio. Solo depende del Nucleo. Testeable vía interfaces. `IServicioBusqueda` e `ServicioBusqueda` aplican validaciones y límites de seguridad.
- **Herramientas**: adaptadores MCP que exponen funcionalidad al LLM cliente. Delegan toda la lógica en los Servicios, nunca acceden directamente a `EverythingInterop`.

## Principios clave

- **SRP (Single Responsibility)**: cada clase tiene una única razón para cambiar
- **DIP (Dependency Inversion)**: Herramientas y Servicios dependen de interfaces, nunca de implementaciones concretas
- **DRY (Don't Repeat Yourself)**: construcción de sintaxis de Everything centralizada en `ServicioBusqueda`
- **Thread Safety**: `EjecutorConsulta` serializa todo acceso al SDK con `SemaphoreSlim`
- **Nombres en español**: variables, métodos, propiedades y carpetas describen qué hacen
- **Descripciones en inglés**: los `[Description("...")]` de las herramientas son lo que ve el LLM cliente

## Convenciones de nombres

- Clases y métodos públicos: `PascalCase` en español (`ServicioBusqueda`, `EjecutarAsync`)
- Variables y parámetros: `camelCase` en español (`maximoResultados`, `rutaCompleta`)
- Constantes: `MAYUSCULAS_CON_GUION_BAJO` (`SORT_NOMBRE_ASC`, `ERROR_IPC`)
- Tests: `Método_Escenario_ResultadoEsperado()` en español

## Thread Safety

`EverythingInterop` **NO es thread-safe**. Todo acceso al SDK debe pasar por `EjecutorConsulta.EjecutarAsync()`, que serializa con un `SemaphoreSlim(1, 1)`. Nunca llamar a funciones de `EverythingInterop` fuera del executor.

## Logging

- Usar `ILogger<T>` inyectado en constructores
- **Prohibido** `Console.WriteLine()` a stdout (rompe el protocolo MCP)
- Errores de Everything → nivel `Warning` o `Error`, nunca `Information`
- Rutas de archivos del usuario → máximo nivel `Debug`, nunca `Information`

## Añadir una nueva herramienta MCP

1. Decidir en qué clase de `Herramientas/` encaja (crear nueva si el dominio es distinto)
2. Si requiere nueva lógica de búsqueda, añadirla a `ServicioBusqueda`
3. Decorar con `[McpServerTool]` y `[Description("...")]` en inglés
4. Parámetros con `[Description("...")]` también en inglés
5. Retornar JSON serializado como `string`
6. Capturar `EverythingNoDisponibleException` y devolver error JSON
7. Añadir test en `tests/EverythingMCP.Tests/Herramientas/`

Ejemplo:

```csharp
[McpServerTool]
[Description("Find files by extension.")]
public async Task<string> BuscarPorExtension(
    [Description("File extension, e.g. 'pdf'.")] string extension)
{
    try
    {
        var consulta = new ConsultaEverything
        {
            TextoBusqueda = $"ext:{extension}",
            MaximoResultados = 100
        };
        var respuesta = await _servicioBusqueda.BuscarAsync(consulta);
        return SerializarRespuesta(respuesta);
    }
    catch (EverythingNoDisponibleException ex)
    {
        return SerializarError(ex.Message);
    }
}
```

## Seguridad

- Nunca loguear rutas completas de archivos del usuario en nivel `Information` o `Debug`
- Validar y sanear parámetros de entrada antes de construir consultas Everything
- Máximo 500 resultados (constante `MAXIMO_RESULTADOS_PERMITIDO`) para proteger el contexto del LLM
- Ruta de DLL configurable por variable de entorno `EVERYTHING_DLL`, nunca hardcodeada

## Testing

- Usar xUnit + Moq + FluentAssertions
- Mockear `IServicioBusqueda` en tests de Herramientas (no depender de Everything real)
- Mockear `EjecutorConsulta` en tests de `ServicioBusqueda`
- Cobertura mínima: happy path, error handling, edge cases (límites, valores nulos)

## Estructura de respuestas

Todas las herramientas devuelven JSON serializado con:
- `JsonNamingPolicy.CamelCase` (el LLM espera `totalDisponible`, no `TotalDisponible`)
- `WriteIndented = true` para legibilidad
- En caso de error: `{ "error": "mensaje descriptivo" }`

Ejemplo respuesta exitosa:

```json
{
  "totalDisponible": 42,
  "totalDevuelto": 42,
  "totalArchivos": 35,
  "totalCarpetas": 7,
  "resultados": [
    {
      "rutaCompleta": "C:\\file.txt",
      "nombreArchivo": "file.txt",
      "tamaño": 1024,
      "tamañoFormateado": "1.0 KB",
      "fechaModificacion": "2025-01-15T10:30:00",
      "esCarpeta": false
    }
  ]
}
```

## Qué NO hacer

- **No** usar `Process.Start()` ni `HttpClient` para comunicar con Everything
- **No** llamar a `EverythingInterop` directamente desde las Herramientas
- **No** escribir en `Console.Out`
- **No** capturar `Exception` genérica sin relanzar o convertir
- **No** crear implementaciones concretas donde una interfaz sea suficiente
- **No** hardcodear rutas absolutas ni configuración
- **No** ignorar `CancellationToken` en métodos async

## Build y Deploy

```bash
# Restaurar dependencias
dotnet restore

# Compilar
dotnet build --configuration Release

# Tests
dotnet test

# Publicar ejecutable independiente
dotnet publish -c Release -p:PublishSingleFile=true
```

El ejecutable resultante se configura en `claude_desktop_config.json`:

```json
{
  "mcpServers": {
    "everything-search": {
      "command": "C:\\path\\to\\EverythingMCP.exe",
      "env": {
        "EVERYTHING_DLL": "C:\\Program Files\\Everything\\Everything64.dll"
      }
    }
  }
}
```

## Recursos útiles

- [Everything SDK Documentation](https://www.voidtools.com/support/everything/)
- [Model Context Protocol](https://modelcontextprotocol.io/)
- [Clean Code Principles](https://www.amazon.com/Clean-Code-Handbook-Software-Craftsmanship/dp/0132350882)
- [SOLID Principles](https://en.wikipedia.org/wiki/SOLID)
