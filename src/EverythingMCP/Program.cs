using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using EverythingMCP.Nucleo.Interoperabilidad;
using EverythingMCP.Servicios;

var builder = Host.CreateApplicationBuilder(args);

// El logging va a stderr para no interferir con el protocolo MCP que usa stdout
builder.Logging.ClearProviders();
builder.Logging.AddConsole(opciones =>
{
    opciones.LogToStandardErrorThreshold = LogLevel.Warning;
});

// Registro de dependencias siguiendo DIP: todo por interfaz
builder.Services.AddSingleton<IEjecutorConsulta, EjecutorConsulta>();
builder.Services.AddSingleton<IServicioBusqueda, ServicioBusqueda>();

// Servidor MCP con transporte stdio; WithToolsFromAssembly descubre clases con [McpServerToolType]
builder.Services
    .AddMcpServer()
    .WithStdioServerTransport()
    .WithToolsFromAssembly(typeof(Program).Assembly);

await builder.Build().RunAsync();
