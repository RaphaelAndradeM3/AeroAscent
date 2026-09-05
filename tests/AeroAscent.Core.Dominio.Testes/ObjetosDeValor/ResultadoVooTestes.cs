namespace AeroAscent.Core.Dominio.Testes.ObjetosDeValor;

using AeroAscent.Core.Dominio.Excecoes;
using AeroAscent.Core.Dominio.ObjetosDeValor;
using Xunit;

/// <summary>
/// Testes unitários para o objeto de valor ResultadoVoo e a fórmula canônica de premiação.
/// </summary>
public class ResultadoVooTestes
{
    [Fact]
    public void Calcular_ComValoresTipicos_DeveAplicarFormulaCanonicasDoPRD()
    {
        // Fórmula: floor(distancia * 0.1) + floor(altitude * 0.05) + moedasColetadas
        // Exemplo: 250m de distância (25) + 80m de altitude (4) + 12 moedas coletadas = 41 moedas
        float distancia = 250f;
        float altitude = 80f;
        int moedasColetadas = 12;

        // Act
        var resultado = ResultadoVoo.Calcular(distancia, altitude, moedasColetadas);

        // Assert
        Assert.Equal(250f, resultado.DistanciaMetros);
        Assert.Equal(80f, resultado.AltitudeMaximaMetros);
        Assert.Equal(12, resultado.MoedasColetadas);
        Assert.Equal(new Moeda(41), resultado.MoedasRecompensaTotal);
    }

    [Fact]
    public void Calcular_ComValoresQueGeramFracao_DeveTruncarCorretamente()
    {
        // Distância: 15.8m * 0.1 = 1.58 => floor = 1
        // Altitude: 19.9m * 0.05 = 0.995 => floor = 0
        // Moedas: 5
        // Total esperado = 1 + 0 + 5 = 6
        var resultado = ResultadoVoo.Calcular(15.8f, 19.9f, 5);

        // Assert
        Assert.Equal(new Moeda(6), resultado.MoedasRecompensaTotal);
    }

    [Fact]
    public void Calcular_ComValoresZerados_DeveRetornarMoedaZero()
    {
        // Act
        var resultado = ResultadoVoo.Calcular(0f, 0f, 0);

        // Assert
        Assert.Equal(Moeda.Zero, resultado.MoedasRecompensaTotal);
    }

    [Fact]
    public void Calcular_ComDistanciaNegativa_DeveLancarDominioInvalidoException()
    {
        // Act & Assert
        Assert.Throws<DominioInvalidoException>(() => ResultadoVoo.Calcular(-1f, 10f, 0));
    }

    [Fact]
    public void Calcular_ComAltitudeNegativa_DeveLancarDominioInvalidoException()
    {
        // Act & Assert
        Assert.Throws<DominioInvalidoException>(() => ResultadoVoo.Calcular(10f, -1f, 0));
    }

    [Fact]
    public void Calcular_ComMoedasColetadasNegativas_DeveLancarDominioInvalidoException()
    {
        // Act & Assert
        Assert.Throws<DominioInvalidoException>(() => ResultadoVoo.Calcular(10f, 10f, -1));
    }
}
