namespace AeroAscent.Core.Dominio.Testes.ObjetosDeValor;

using AeroAscent.Core.Dominio.Excecoes;
using AeroAscent.Core.Dominio.ObjetosDeValor;
using Xunit;

/// <summary>
/// Testes unitários para o Objeto de Valor MedidorForcaOscilante (dinâmica de temporização triangular).
/// </summary>
public class MedidorForcaOscilanteTestes
{
    [Fact]
    public void ObterFatorPrecisao_TempoInicial_DeveRetornarZero()
    {
        // Arrange
        var medidor = new MedidorForcaOscilante(1.0f);

        // Act
        var fator = medidor.ObterFatorPrecisao(0.0f);

        // Assert
        Assert.Equal(0.0f, fator);
    }

    [Fact]
    public void ObterFatorPrecisao_MeioCiclo_DeveAtingirApiceMaximo()
    {
        // Arrange (1.0 Hz -> meio ciclo em 0.5s)
        var medidor = new MedidorForcaOscilante(1.0f);

        // Act
        var fator = medidor.ObterFatorPrecisao(0.5f);

        // Assert
        Assert.InRange(fator, 0.99f, 1.0f);
    }

    [Fact]
    public void ObterFatorPrecisao_CicloCompleto_DeveRetornarAZero()
    {
        // Arrange (1.0 Hz -> ciclo completo em 1.0s)
        var medidor = new MedidorForcaOscilante(1.0f);

        // Act
        var fator = medidor.ObterFatorPrecisao(1.0f);

        // Assert
        Assert.InRange(fator, 0.0f, 0.01f);
    }

    [Theory]
    [InlineData(0.25f, 0.50f)]
    [InlineData(0.75f, 0.50f)]
    public void ObterFatorPrecisao_PontosIntermediarios_DeveRespeitarSimetriaTriangular(float tempo, float fatorEsperado)
    {
        // Arrange
        var medidor = new MedidorForcaOscilante(1.0f);

        // Act
        var fator = medidor.ObterFatorPrecisao(tempo);

        // Assert
        Assert.InRange(fator, fatorEsperado - 0.02f, fatorEsperado + 0.02f);
    }

    [Fact]
    public void Construtor_ComFrequenciaZeroOuNegativa_DeveLancarDominioInvalidoException()
    {
        // Act & Assert
        Assert.Throws<DominioInvalidoException>(() => new MedidorForcaOscilante(0f));
        Assert.Throws<DominioInvalidoException>(() => new MedidorForcaOscilante(-1.5f));
    }

    [Fact]
    public void ObterFatorPrecisao_ComTempoNegativo_DeveLancarDominioInvalidoException()
    {
        // Arrange
        var medidor = new MedidorForcaOscilante(1.0f);

        // Act & Assert
        Assert.Throws<DominioInvalidoException>(() => medidor.ObterFatorPrecisao(-0.1f));
    }
}
