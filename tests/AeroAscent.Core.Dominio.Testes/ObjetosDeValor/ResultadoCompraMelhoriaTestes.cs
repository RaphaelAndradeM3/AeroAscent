namespace AeroAscent.Core.Dominio.Testes.ObjetosDeValor;

using AeroAscent.Core.Dominio.Enums;
using AeroAscent.Core.Dominio.Excecoes;
using AeroAscent.Core.Dominio.ObjetosDeValor;
using Xunit;

/// <summary>
/// Testes unitários para o objeto de valor na stack <see cref="ResultadoCompraMelhoria"/>.
/// </summary>
public class ResultadoCompraMelhoriaTestes
{
    [Fact]
    public void Construtor_ComParametrosValidos_DeveInicializarPropriedades()
    {
        // Arrange
        var tipo = TipoMelhoria.Motor;
        var nivelAnterior = 1;
        var novoNivel = 2;
        var custoPago = new Moeda(50);
        var saldoRestante = new Moeda(150);
        var atingiuNivelMaximo = false;
        var proximoCusto = new Moeda(75);

        // Act
        var resultado = new ResultadoCompraMelhoria(
            tipo,
            nivelAnterior,
            novoNivel,
            custoPago,
            saldoRestante,
            atingiuNivelMaximo,
            proximoCusto);

        // Assert
        Assert.Equal(tipo, resultado.Tipo);
        Assert.Equal(nivelAnterior, resultado.NivelAnterior);
        Assert.Equal(novoNivel, resultado.NovoNivel);
        Assert.Equal(custoPago, resultado.CustoPago);
        Assert.Equal(saldoRestante, resultado.SaldoRestante);
        Assert.False(resultado.AtingiuNivelMaximo);
        Assert.Equal(proximoCusto, resultado.ProximoCusto);
    }

    [Fact]
    public void Construtor_QuandoAtingeNivelMaximo_DeveAceitarProximoCustoNulo()
    {
        // Arrange & Act
        var resultado = new ResultadoCompraMelhoria(
            TipoMelhoria.Catapulta,
            9,
            10,
            new Moeda(2306),
            new Moeda(500),
            atingiuNivelMaximo: true,
            proximoCusto: null);

        // Assert
        Assert.True(resultado.AtingiuNivelMaximo);
        Assert.Null(resultado.ProximoCusto);
        Assert.Equal(10, resultado.NovoNivel);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Construtor_ComNivelAnteriorInvalido_DeveLancarDominioInvalidoException(int nivelInvalido)
    {
        // Act & Assert
        Assert.Throws<DominioInvalidoException>(() => new ResultadoCompraMelhoria(
            TipoMelhoria.Motor,
            nivelInvalido,
            2,
            new Moeda(50),
            new Moeda(100),
            false,
            new Moeda(75)));
    }

    [Theory]
    [InlineData(2, 2)]
    [InlineData(3, 2)]
    public void Construtor_ComNovoNivelMenorOuIgualAoAnterior_DeveLancarDominioInvalidoException(int nivelAnterior, int novoNivel)
    {
        // Act & Assert
        Assert.Throws<DominioInvalidoException>(() => new ResultadoCompraMelhoria(
            TipoMelhoria.Aerodinamica,
            nivelAnterior,
            novoNivel,
            new Moeda(40),
            new Moeda(100),
            false,
            new Moeda(60)));
    }

    [Fact]
    public void IgualdadePorValor_DeveGarantirImutabilidadeEValorEstrutural()
    {
        // Arrange
        var r1 = new ResultadoCompraMelhoria(
            TipoMelhoria.TanqueCombustivel,
            1,
            2,
            new Moeda(30),
            new Moeda(70),
            false,
            new Moeda(45));

        var r2 = new ResultadoCompraMelhoria(
            TipoMelhoria.TanqueCombustivel,
            1,
            2,
            new Moeda(30),
            new Moeda(70),
            false,
            new Moeda(45));

        // Assert
        Assert.Equal(r1, r2);
    }
}
