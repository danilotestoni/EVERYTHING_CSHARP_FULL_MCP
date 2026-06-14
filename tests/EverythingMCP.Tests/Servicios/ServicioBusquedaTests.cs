using Xunit;
using Moq;
using FluentAssertions;
using EverythingMCP.Nucleo.Excepciones;
using EverythingMCP.Nucleo.Interoperabilidad;
using EverythingMCP.Nucleo.Modelos;
using EverythingMCP.Servicios;

namespace EverythingMCP.Tests.Servicios;

public class ServicioBusquedaTests
{
    private readonly Mock<IEjecutorConsulta> _ejecutorMock;
    private readonly ServicioBusqueda _servicio;

    public ServicioBusquedaTests()
    {
        _ejecutorMock = new Mock<IEjecutorConsulta>();
        _servicio = new ServicioBusqueda(_ejecutorMock.Object);
    }

    [Fact]
    public async Task BuscarAsync_CuandoConsultaEsValida_DevuelveResultados()
    {
        // Arrange
        var consulta = new ConsultaEverything { TextoBusqueda = "*.pdf" };
        var respuestaEsperada = new RespuestaBusqueda
        {
            TotalDisponible = 5,
            TotalDevuelto = 5,
            TotalArchivos = 5,
            TotalCarpetas = 0,
            Resultados = new List<ResultadoArchivo>
            {
                new() { RutaCompleta = "C:\\archivo1.pdf", NombreArchivo = "archivo1.pdf", Extension = "pdf" }
            }
        };

        _ejecutorMock.Setup(e => e.ServicioDisponible()).Returns(true);
        _ejecutorMock
            .Setup(e => e.EjecutarAsync(It.IsAny<ConsultaEverything>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(respuestaEsperada);

        // Act
        var resultado = await _servicio.BuscarAsync(consulta);

        // Assert
        resultado.Should().NotBeNull();
        resultado.TotalDisponible.Should().Be(5);
        resultado.Resultados.Should().HaveCount(1);
    }

    [Fact]
    public async Task BuscarAsync_CuandoServicioNoDisponible_LanzaEverythingNoDisponibleException()
    {
        // Arrange
        var consulta = new ConsultaEverything { TextoBusqueda = "test" };
        _ejecutorMock.Setup(e => e.ServicioDisponible()).Returns(false);

        // Act & Assert
        await _servicio.Invoking(s => s.BuscarAsync(consulta))
            .Should()
            .ThrowAsync<EverythingNoDisponibleException>();
    }

    [Fact]
    public async Task BuscarAsync_CuandoMaximoResultadosSuperaLimite_LoAjustaA500()
    {
        // Arrange
        var consulta = new ConsultaEverything { TextoBusqueda = "test", MaximoResultados = 1000 };
        var respuestaCapturada = new RespuestaBusqueda
        {
            TotalDisponible = 1000,
            TotalDevuelto = 500,
            Resultados = new List<ResultadoArchivo>()
        };

        _ejecutorMock.Setup(e => e.ServicioDisponible()).Returns(true);
        _ejecutorMock
            .Setup(e => e.EjecutarAsync(It.IsAny<ConsultaEverything>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ConsultaEverything c, CancellationToken ct) =>
            {
                c.MaximoResultados.Should().Be(500);
                return respuestaCapturada;
            });

        // Act
        var resultado = await _servicio.BuscarAsync(consulta);

        // Assert
        resultado.Should().NotBeNull();
    }

    [Fact]
    public async Task BuscarAsync_CuandoTextoBusquedaEsVacio_LanzaArgumentException()
    {
        // Arrange
        var consulta = new ConsultaEverything { TextoBusqueda = "" };

        // Act & Assert
        await _servicio.Invoking(s => s.BuscarAsync(consulta))
            .Should()
            .ThrowAsync<ArgumentException>();
    }

    [Fact]
    public void EstaDisponible_CuandoEverythingCorre_DevuelveTrue()
    {
        // Arrange
        _ejecutorMock.Setup(e => e.ServicioDisponible()).Returns(true);

        // Act
        var resultado = _servicio.EstaDisponible();

        // Assert
        resultado.Should().BeTrue();
    }

    [Fact]
    public void EstaDisponible_CuandoEverythingNoCorre_DevuelveFalse()
    {
        // Arrange
        _ejecutorMock.Setup(e => e.ServicioDisponible()).Returns(false);

        // Act
        var resultado = _servicio.EstaDisponible();

        // Assert
        resultado.Should().BeFalse();
    }

    [Fact]
    public void ObtenerVersion_DevuelveVersionValida()
    {
        // Arrange
        _ejecutorMock.Setup(e => e.ObtenerVersionEverything()).Returns("1.4.1.0 (rev 1, build 10001)");

        // Act
        var resultado = _servicio.ObtenerVersion();

        // Assert
        resultado.Should().Contain("1.4.1.0");
    }

    [Fact]
    public async Task BuscarAsync_CuandoConsultaEsNula_LanzaArgumentNullException()
    {
        // Act & Assert
        await _servicio.Invoking(s => s.BuscarAsync(null!))
            .Should()
            .ThrowAsync<ArgumentNullException>();
    }
}
