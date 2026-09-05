namespace AeroAscent.Core.Dominio.Testes.ObjetosDeValor;

using AeroAscent.Core.Dominio.Enums;
using AeroAscent.Core.Dominio.Excecoes;
using AeroAscent.Core.Dominio.ObjetosDeValor;
using Xunit;

/// <summary>
/// Testes unitários para o objeto de valor ResultadoFimVoo.
/// </summary>
public class ResultadoFimVooTestes
{
    [Fact]
    public void CriarEmAndamento_DeveInicializarComStatusEmVooENaoParado()
    {
        // Act
        var resultado = ResultadoFimVoo.CriarEmAndamento(150.5f, 42.0f, 7);

        // Assert
        Assert.Equal(StatusVoo.EmVoo, resultado.Status);
        Assert.False(resultado.AeronaveParou);
        Assert.Equal(150.5f, resultado.DistanciaFinalMetros);
        Assert.Equal(42.0f, resultado.AltitudeMaximaMetros);
        Assert.Equal(7, resultado.MoedasColetadas);
        Assert.Null(resultado.Resultado);
    }

    [Fact]
    public void CriarPousado_DeveInicializarComStatusPousadoEParadoComResultado()
    {
        // Arrange
        var resultadoVoo = ResultadoVoo.Calcular(200.0f, 50.0f, 10);

        // Act
        var resultadoFim = ResultadoFimVoo.CriarPousado(200.0f, 50.0f, 10, resultadoVoo);

        // Assert
        Assert.Equal(StatusVoo.Pousado, resultadoFim.Status);
        Assert.True(resultadoFim.AeronaveParou);
        Assert.Equal(200.0f, resultadoFim.DistanciaFinalMetros);
        Assert.Equal(50.0f, resultadoFim.AltitudeMaximaMetros);
        Assert.Equal(10, resultadoFim.MoedasColetadas);
        Assert.NotNull(resultadoFim.Resultado);
        Assert.Equal(resultadoVoo, resultadoFim.Resultado);
    }

    [Fact]
    public void CriarPousado_ComResultadoNulo_DeveLancarDominioInvalidoException()
    {
        Assert.Throws<DominioInvalidoException>(() =>
            ResultadoFimVoo.CriarPousado(100f, 20f, 5, null!));
    }

    [Theory]
    [InlineData(-1f, 10f, 0)]
    [InlineData(10f, -1f, 0)]
    [InlineData(10f, 10f, -1)]
    public void Construtor_ComParametrosNegativos_DeveLancarDominioInvalidoException(
        float distancia,
        float altitude,
        int moedas)
    {
        Assert.Throws<DominioInvalidoException>(() =>
            new ResultadoFimVoo(StatusVoo.EmVoo, false, distancia, altitude, moedas, null));
    }
}
