namespace AeroAscent.Core.Dominio.Testes.Entidades;

using AeroAscent.Core.Dominio.Entidades;
using AeroAscent.Core.Dominio.Enums;
using AeroAscent.Core.Dominio.Excecoes;
using Xunit;

/// <summary>
/// Testes unitários para a entidade Aeronave e suas regras de negócio e invariantes.
/// </summary>
public class AeronaveTestes
{
    [Fact]
    public void CriarPadrao_DeveInicializarComIdValidoENiveisEmUm()
    {
        // Act
        var aeronave = Aeronave.CriarPadrao();

        // Assert
        Assert.NotEqual(Guid.Empty, aeronave.Id);
        Assert.Equal(1, aeronave.NivelMotor);
        Assert.Equal(1, aeronave.NivelAerodinamica);
        Assert.Equal(1, aeronave.NivelTanqueCombustivel);
        Assert.Equal(1, aeronave.NivelCatapulta);
    }

    [Fact]
    public void Construtor_ComParametrosValidos_DeveAtribuirCorretamente()
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act
        var aeronave = new Aeronave(id, nivelMotor: 3, nivelAerodinamica: 5, nivelTanqueCombustivel: 2, nivelCatapulta: 4);

        // Assert
        Assert.Equal(id, aeronave.Id);
        Assert.Equal(3, aeronave.NivelMotor);
        Assert.Equal(5, aeronave.NivelAerodinamica);
        Assert.Equal(2, aeronave.NivelTanqueCombustivel);
        Assert.Equal(4, aeronave.NivelCatapulta);
    }

    [Fact]
    public void Construtor_ComIdVazio_DeveLancarDominioInvalidoException()
    {
        // Act & Assert
        var ex = Assert.Throws<DominioInvalidoException>(() =>
            new Aeronave(Guid.Empty, 1, 1, 1, 1));

        Assert.Equal("Id", ex.NomeCampo);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(11)]
    public void Construtor_ComNivelMotorInvalido_DeveLancarDominioInvalidoException(int nivelInvalido)
    {
        // Act & Assert
        var ex = Assert.Throws<DominioInvalidoException>(() =>
            new Aeronave(Guid.NewGuid(), nivelMotor: nivelInvalido, 1, 1, 1));

        Assert.Equal("NivelMotor", ex.NomeCampo);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-2)]
    [InlineData(12)]
    public void Construtor_ComNivelAerodinamicaInvalido_DeveLancarDominioInvalidoException(int nivelInvalido)
    {
        // Act & Assert
        var ex = Assert.Throws<DominioInvalidoException>(() =>
            new Aeronave(Guid.NewGuid(), 1, nivelAerodinamica: nivelInvalido, 1, 1));

        Assert.Equal("NivelAerodinamica", ex.NomeCampo);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    [InlineData(15)]
    public void Construtor_ComNivelTanqueInvalido_DeveLancarDominioInvalidoException(int nivelInvalido)
    {
        // Act & Assert
        var ex = Assert.Throws<DominioInvalidoException>(() =>
            new Aeronave(Guid.NewGuid(), 1, 1, nivelTanqueCombustivel: nivelInvalido, 1));

        Assert.Equal("NivelTanqueCombustivel", ex.NomeCampo);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(11)]
    public void Construtor_ComNivelCatapultaInvalido_DeveLancarDominioInvalidoException(int nivelInvalido)
    {
        // Act & Assert
        var ex = Assert.Throws<DominioInvalidoException>(() =>
            new Aeronave(Guid.NewGuid(), 1, 1, 1, nivelCatapulta: nivelInvalido));

        Assert.Equal("NivelCatapulta", ex.NomeCampo);
    }

    [Fact]
    public void AtualizarNivel_ComValoresValidos_DeveModificarComponente()
    {
        // Arrange
        var aeronave = Aeronave.CriarPadrao();

        // Act
        aeronave.AtualizarNivel(TipoMelhoria.Motor, 2);
        aeronave.AtualizarNivel(TipoMelhoria.Aerodinamica, 3);
        aeronave.AtualizarNivel(TipoMelhoria.TanqueCombustivel, 4);
        aeronave.AtualizarNivel(TipoMelhoria.Catapulta, 5);

        // Assert
        Assert.Equal(2, aeronave.NivelMotor);
        Assert.Equal(3, aeronave.NivelAerodinamica);
        Assert.Equal(4, aeronave.NivelTanqueCombustivel);
        Assert.Equal(5, aeronave.NivelCatapulta);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(11)]
    public void AtualizarNivel_ComValorForaDoIntervalo_DeveLancarDominioInvalidoException(int nivelInvalido)
    {
        // Arrange
        var aeronave = Aeronave.CriarPadrao();

        // Act & Assert
        Assert.Throws<DominioInvalidoException>(() =>
            aeronave.AtualizarNivel(TipoMelhoria.Motor, nivelInvalido));
    }

    [Fact]
    public void ObterNivel_DeveRetornarNivelDoTipoCorrespondente()
    {
        // Arrange
        var aeronave = new Aeronave(Guid.NewGuid(), 2, 3, 4, 5);

        // Act & Assert
        Assert.Equal(2, aeronave.ObterNivel(TipoMelhoria.Motor));
        Assert.Equal(3, aeronave.ObterNivel(TipoMelhoria.Aerodinamica));
        Assert.Equal(4, aeronave.ObterNivel(TipoMelhoria.TanqueCombustivel));
        Assert.Equal(5, aeronave.ObterNivel(TipoMelhoria.Catapulta));
    }
}
