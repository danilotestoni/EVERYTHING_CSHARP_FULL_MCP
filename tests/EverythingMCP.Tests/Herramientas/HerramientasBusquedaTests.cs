using Xunit;
using Moq;
using FluentAssertions;
using System.Text.Json;
using EverythingMCP.Nucleo.Excepciones;
using EverythingMCP.Nucleo.Modelos;
using EverythingMCP.Herramientas;
using EverythingMCP.Servicios;

namespace EverythingMCP.Tests.Herramientas;

public class HerramientasBusquedaTests
{
    private readonly Mock<IServicioBusqueda> _servicioBusquedaMock;

    public HerramientasBusquedaTests()
    {
        _servicioBusquedaMock = new Mock<IServicioBusqueda>();
    }

    [Fact]
    public async Task BuscarArchivos_CuandoConsultaEsValida_DevuelveJsonConResultados()
    {
        // Arrange
        var respuestaSimulada = new RespuestaBusqueda
        {
            TotalDisponible = 2,
            TotalDevuelto = 2,
            TotalArchivos = 2,
            TotalCarpetas = 0,
            Resultados = new List<ResultadoArchivo>
            {
                new() { RutaCompleta = "C:\\file1.txt", NombreArchivo = "file1.txt" },
                new() { RutaCompleta = "C:\\file2.txt", NombreArchivo = "file2.txt" }
            }
        };

        _servicioBusquedaMock
            .Setup(s => s.BuscarAsync(It.IsAny<ConsultaEverything>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(respuestaSimulada);

        var herramientas = new HerramientasBusquedaGeneral(_servicioBusquedaMock.Object);

        // Act
        var resultado = await herramientas.BuscarArchivos("*.txt");

        // Assert
        resultado.Should().NotBeNullOrEmpty();
        var jsonObj = JsonSerializer.Deserialize<JsonElement>(resultado);
        jsonObj.GetProperty("totalDisponible").GetInt32().Should().Be(2);
        jsonObj.GetProperty("resultados").GetArrayLength().Should().Be(2);
    }

    [Fact]
    public async Task BuscarPorTipo_CuandoTipoEsImagen_UsaExtensionesCorrectas()
    {
        // Arrange
        var herramientasFiltrada = new HerramientasBusquedaFiltrada(_servicioBusquedaMock.Object);

        _servicioBusquedaMock
            .Setup(s => s.BuscarAsync(It.IsAny<ConsultaEverything>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new RespuestaBusqueda { Resultados = new List<ResultadoArchivo>() });

        // Act
        var resultado = await herramientasFiltrada.BuscarPorTipo("imagen");

        // Assert
        resultado.Should().NotBeNullOrEmpty();
        _servicioBusquedaMock.Verify(
            s => s.BuscarAsync(
                It.Is<ConsultaEverything>(c =>
                    c.TextoBusqueda.Contains("jpg") &&
                    c.TextoBusqueda.Contains("png")),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task BuscarPorTamaño_CuandoSoloTamañoMinimo_GeneraSintaxisCorrecta()
    {
        // Arrange
        var herramientasFiltrada = new HerramientasBusquedaFiltrada(_servicioBusquedaMock.Object);

        _servicioBusquedaMock
            .Setup(s => s.BuscarAsync(It.IsAny<ConsultaEverything>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new RespuestaBusqueda { Resultados = new List<ResultadoArchivo>() });

        // Act
        var resultado = await herramientasFiltrada.BuscarPorTamaño("100mb");

        // Assert
        resultado.Should().NotBeNullOrEmpty();
        _servicioBusquedaMock.Verify(
            s => s.BuscarAsync(
                It.Is<ConsultaEverything>(c =>
                    c.TextoBusqueda.Contains("100mb")),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ObtenerDetalles_CuandoRutaEsValida_BuscaArchivoCorrecto()
    {
        // Arrange
        var herramientasSistema = new HerramientasSistema(_servicioBusquedaMock.Object);
        var rutaEsperada = "C:\\Windows\\System32\\notepad.exe";

        _servicioBusquedaMock
            .Setup(s => s.BuscarAsync(It.IsAny<ConsultaEverything>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new RespuestaBusqueda
            {
                TotalDisponible = 1,
                TotalDevuelto = 1,
                Resultados = new List<ResultadoArchivo>
                {
                    new() { RutaCompleta = rutaEsperada, NombreArchivo = "notepad.exe" }
                }
            });

        // Act
        var resultado = await herramientasSistema.ObtenerDetalles(rutaEsperada);

        // Assert
        resultado.Should().NotBeNullOrEmpty();
        var jsonObj = JsonSerializer.Deserialize<JsonElement>(resultado);
        jsonObj.GetProperty("totalDisponible").GetInt32().Should().Be(1);
    }

    [Fact]
    public async Task ContarResultados_CuandoHayMuchos_DevuelveNumeroSinListar()
    {
        // Arrange
        var herramientasSistema = new HerramientasSistema(_servicioBusquedaMock.Object);

        _servicioBusquedaMock
            .Setup(s => s.BuscarAsync(It.IsAny<ConsultaEverything>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new RespuestaBusqueda
            {
                TotalDisponible = 5000,
                TotalDevuelto = 1,
                TotalArchivos = 3000,
                TotalCarpetas = 2000,
                Resultados = new List<ResultadoArchivo>()
            });

        // Act
        var resultado = await herramientasSistema.ContarResultados("*.txt");

        // Assert
        resultado.Should().NotBeNullOrEmpty();
        var jsonObj = JsonSerializer.Deserialize<JsonElement>(resultado);
        jsonObj.GetProperty("total").GetInt32().Should().Be(5000);
        jsonObj.GetProperty("archivos").GetInt32().Should().Be(3000);
        jsonObj.GetProperty("carpetas").GetInt32().Should().Be(2000);
    }

    [Fact]
    public async Task BuscarArchivos_CuandoEverythingNoDisponible_DevuelveError()
    {
        // Arrange
        _servicioBusquedaMock
            .Setup(s => s.BuscarAsync(It.IsAny<ConsultaEverything>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new EverythingNoDisponibleException("Everything not available"));

        var herramientas = new HerramientasBusquedaGeneral(_servicioBusquedaMock.Object);

        // Act
        var resultado = await herramientas.BuscarArchivos("test");

        // Assert
        resultado.Should().Contain("error");
        resultado.Should().Contain("Everything not available");
    }

    [Fact]
    public void EstadoEverything_CuandoEverythingDisponible_DevuelveEstadoPositivo()
    {
        // Arrange
        _servicioBusquedaMock
            .Setup(s => s.EstaDisponible())
            .Returns(true);

        _servicioBusquedaMock
            .Setup(s => s.ObtenerVersion())
            .Returns("1.4.1.0 (rev 1, build 10001)");

        var herramientasSistema = new HerramientasSistema(_servicioBusquedaMock.Object);

        // Act
        var resultado = herramientasSistema.EstadoEverything();

        // Assert
        resultado.Should().NotBeNullOrEmpty();
        var jsonObj = JsonSerializer.Deserialize<JsonElement>(resultado);
        jsonObj.GetProperty("disponible").GetBoolean().Should().BeTrue();
        jsonObj.GetProperty("baseDatosLista").GetBoolean().Should().BeTrue();
    }
}
