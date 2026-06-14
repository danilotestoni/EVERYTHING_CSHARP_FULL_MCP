# Everything MCP Server

A production-ready Model Context Protocol (MCP) server in C# that integrates [voidtools Everything](https://www.voidtools.com/) with AI assistants like Claude, Cursor, and other MCP-compatible clients. Search your entire Windows filesystem instantly from any AI agent.

## Features

Everything MCP Server exposes 24 powerful search tools organized by functionality:

### General Search (3 tools)
- **buscar_archivos** — Full system search with Everything syntax support (wildcards, operators, filters)
- **buscar_con_regex** — Advanced pattern matching using regular expressions
- **buscar_por_nombre_exacto** — Find files by exact filename match

### Filtered Search (7 tools)
- **buscar_en_carpeta** — Search within specific folder paths (recursive or non-recursive)
- **buscar_por_extension** — Find all files with a specific extension
- **buscar_por_tipo** — Search by file type (image, video, audio, document, code, executable, archive, database)
- **buscar_por_tamaño** — Find files within a size range (e.g., 100mb–2gb)
- **buscar_por_fecha_modificacion** — Find files modified within date ranges
- **buscar_por_fecha_creacion** — Find files created within date ranges

### File Operations (8 tools)
- **archivos_recientes** — Find recently modified files
- **archivos_grandes** — Locate the largest files
- **archivos_vacios** — Find zero-byte files
- **carpetas_vacias** — Find empty folders
- **buscar_duplicados** — Find files with the same name in different locations
- **archivos_ocultos** — Find hidden files and folders
- **archivos_sistema** — Find system files
- **archivos_solo_lectura** — Find read-only files
- **archivos_sin_extension** — Find files without extensions
- **listar_carpeta** — List immediate folder contents (non-recursive)

### System Tools (6 tools)
- **obtener_detalles** — Get full metadata for a specific file (size, dates, attributes)
- **contar_resultados** — Count matching files without retrieving them
- **estadisticas_carpeta** — Get folder statistics (file count, size, etc.)
- **buscar_ejecutables** — Find executable files
- **estado_everything** — Check Everything service status and version

## Requirements

- **Windows 10 or 11** — Everything only runs on Windows
- **Everything installed and running** — Download from https://www.voidtools.com/
- **.NET 8 SDK** — For building from source
- **MCP-compatible AI client** — Claude Desktop, Cursor, or other MCP clients

## Installation

### Option 1: Use Pre-built Binary

1. Download the latest release from GitHub
2. Extract `EverythingMCP.exe` to a folder (e.g., `C:\Tools\EverythingMCP\`)
3. Configure your AI client (see Configuration section)

### Option 2: Build from Source

```bash
git clone https://github.com/yourusername/EverythingMCP.git
cd EverythingMCP

dotnet restore
dotnet build --configuration Release
dotnet publish -c Release -p:PublishSingleFile=true

# Binary will be at: bin/Release/net8.0-windows/publish/EverythingMCP.exe
```

## Configuration

### Claude Desktop

Edit `~/.claude_desktop/claude_desktop_config.json`:

```json
{
  "mcpServers": {
    "everything-search": {
      "command": "C:\\Tools\\EverythingMCP\\EverythingMCP.exe",
      "env": {
        "EVERYTHING_DLL": "C:\\Program Files\\Everything\\Everything64.dll"
      }
    }
  }
}
```

### Cursor

Similar configuration in Cursor's MCP settings:

```json
{
  "mcpServers": {
    "everything-search": {
      "command": "C:\\Tools\\EverythingMCP\\EverythingMCP.exe"
    }
  }
}
```

### Custom Everything Installation Path

If Everything is installed in a non-standard location, set the `EVERYTHING_DLL` environment variable:

```json
{
  "mcpServers": {
    "everything-search": {
      "command": "C:\\Tools\\EverythingMCP\\EverythingMCP.exe",
      "env": {
        "EVERYTHING_DLL": "D:\\CustomFolder\\Everything64.dll"
      }
    }
  }
}
```

## Usage Examples

### Finding Recent Files
```
"Find all PDFs modified in the last 7 days"
→ Uses: buscar_por_extension + buscar_por_fecha_modificacion
```

### Locating Large Files
```
"Find all files larger than 500MB in my Documents folder"
→ Uses: buscar_por_tamaño
```

### Searching Code Files
```
"Find all C# files containing 'async' in their name"
→ Uses: buscar_por_tipo (código)
```

### Finding Duplicates
```
"Show me all files named 'document.docx' on my system"
→ Uses: buscar_duplicados
```

## Troubleshooting

### "Everything service not available"
- Ensure Everything is installed: https://www.voidtools.com/download/
- Verify Everything is running (check system tray)
- Restart Everything if it seems unresponsive

### "Everything64.dll not found"
- Check that Everything is installed in `C:\Program Files\Everything\`
- If installed elsewhere, set `EVERYTHING_DLL` environment variable
- Verify the file exists: `dir "C:\Program Files\Everything\Everything64.dll"`

### "No results found"
- Check your search syntax (Everything uses special operators)
- Try simpler queries first: `*.pdf` instead of complex patterns
- Verify Everything's database is loaded (check File menu in Everything app)

### Performance Issues
- Limit results: use `maximoResultados` parameter (default 50, max 500)
- Use specific filters (extension, folder path) to narrow searches
- Avoid regex on very large result sets

## Architecture

The project follows Clean Architecture principles:

- **Nucleo** (Core): Domain models, P/Invoke declarations, thread-safe executor
- **Servicios** (Services): Business logic, validation, error handling
- **Herramientas** (Tools): MCP adapters that expose functionality to AI clients

All access to the Everything SDK is serialized through `EjecutorConsulta` to ensure thread safety.

## Testing

```bash
# Run all tests
dotnet test

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"
```

Tests use xUnit, Moq, and FluentAssertions. Core components (service, executor) are well-tested; tool tests verify MCP contract and error handling.

## Development

See [CLAUDE.md](CLAUDE.md) for detailed development guidelines, architecture decisions, and coding conventions.

### Key Principles
- **DIP (Dependency Inversion)**: Everything depends on interfaces
- **SRP (Single Responsibility)**: Each class has one reason to change
- **Thread Safety**: All SDK access is serialized
- **Security**: Input validation, result limiting, sensitive data handling

## Contributing

Contributions are welcome! Please:

1. Fork the repository
2. Create a feature branch
3. Add tests for new functionality
4. Submit a pull request

## License

MIT License — See LICENSE file for details

## Resources

- [Everything Documentation](https://www.voidtools.com/support/everything/)
- [Model Context Protocol](https://modelcontextprotocol.io/)
- [voidtools Website](https://www.voidtools.com/)

---

**Note**: Everything MCP Server is an unofficial third-party tool. It is not affiliated with or endorsed by voidtools.
