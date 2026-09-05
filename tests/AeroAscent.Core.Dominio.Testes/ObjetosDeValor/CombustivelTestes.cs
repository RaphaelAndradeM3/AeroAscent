namespace AeroAscent.Core.Dominio.Testes.ObjetosDeValor;

using AeroAscent.Core.Dominio.Excecoes;
using AeroAscent.Core.Dominio.ObjetosDeValor;
using Xunit;

/// <summary>
/// Testes unitários para o objeto de valor Combustivel cobrindo consumo normal, esgotamento fracionário e conservação.
/// </summary>
public class CombustivelTestes
{
    [Fact]
    public void CriarCheio_DeveInicializarComQuantidadeIgualCapacidade()
    {
        // Act
        var combustivel = Combustivel.CriarCheio(100f, 10f);

        // Assert
        Assert.Equal(100f, combustivel.CapacidadeMaxima);
        Assert.Equal(100f, combustivel.QuantidadeAtual);
        Assert.Equal(10f, combustivel.TaxaQueimaPorSegundo);
        Assert.Equal(1.0f, combustivel.PercentualRestante);
        Assert.False(combustivel.EstaVazio);
    }

    [Fact]
    public void Construtor_ComCapacidadeInvalida_DeveLancarDominioInvalidoException()
    {
        // Act & Assert
        Assert.Throws<DominioInvalidoException>(() => new Combustivel(10f, 0f, 5f));
        Assert.Throws<DominioInvalidoException>(() => new Combustivel(10f, -5f, 5f));
    }

    [Fact]
    public void Construtor_ComQuantidadeInvalida_DeveLancarDominioInvalidoException()
    {
        // Act & Assert
        Assert.Throws<DominioInvalidoException>(() => new Combustivel(-1f, 100f, 5f));
        Assert.Throws<DominioInvalidoException>(() => new Combustivel(120f, 100f, 5f));
    }

    [Fact]
    public void Construtor_ComTaxaQueimaNegativa_DeveLancarDominioInvalidoException()
    {
        // Act & Assert
        Assert.Throws<DominioInvalidoException>(() => new Combustivel(50f, 100f, -1f));
    }

    [Fact]
    public void Consumir_DeveReduzirQuantidadeRecalcularPercentual()
    {
        // Arrange
        var combustivel = Combustivel.CriarCheio(100f, 10f); // 10 unidades por segundo

        // Act (consome por 3 segundos = 30 unidades)
        var restante = combustivel.Consumir(3f);

        // Assert
        Assert.Equal(70f, restante.QuantidadeAtual);
        Assert.Equal(0.7f, restante.PercentualRestante, precision: 3);
        Assert.False(restante.EstaVazio);
    }

    [Fact]
    public void Consumir_AlemDaQuantidadeDisponivel_DeveZerarSemFicarNegativo()
    {
        // Arrange
        var combustivel = Combustivel.CriarCheio(50f, 20f);

        // Act (consome por 5 segundos = 100 unidades necessárias, mas só tem 50)
        var esgotado = combustivel.Consumir(5f);

        // Assert
        Assert.Equal(0f, esgotado.QuantidadeAtual);
        Assert.Equal(0f, esgotado.PercentualRestante);
        Assert.True(esgotado.EstaVazio);
    }

    [Fact]
    public void Consumir_ComDeltaTempoZeroOuNegativo_DeveRetornarMesmaInstanciaOuSemAlteracao()
    {
        // Arrange
        var combustivel = Combustivel.CriarCheio(100f, 10f);

        // Act
        var resultado = combustivel.Consumir(0f);

        // Assert
        Assert.Equal(100f, resultado.QuantidadeAtual);
    }

    [Fact]
    public void ConsumirFracionario_ComCombustivelSuficiente_DeveRetornarTempoIntegral()
    {
        // Arrange
        var combustivel = Combustivel.CriarCheio(20.0f, 5.0f);

        // Act (passo de 0.02s a 5.0 un/s consome 0.1 un)
        var resultante = combustivel.ConsumirFracionario(0.02f, out var tempoQueima);

        // Assert
        Assert.Equal(0.02f, tempoQueima, precision: 5);
        Assert.Equal(19.9f, resultante.QuantidadeAtual, precision: 3);
        Assert.False(resultante.EstaVazio);
    }

    [Fact]
    public void ConsumirFracionario_QuandoCombustivelEsgotaNoMeioDoPasso_DeveCalcularTempoResidualExato()
    {
        // Arrange: Tanque com apenas 0.025 unidades e taxa de 5.0 un/s (dura exatamente 0.005s)
        var combustivel = new Combustivel(0.025f, 20.0f, 5.0f);

        // Act: Passo de 0.02s (20ms)
        var resultante = combustivel.ConsumirFracionario(0.02f, out var tempoQueima);

        // Assert
        Assert.Equal(0.005f, tempoQueima, precision: 5);
        Assert.Equal(0.0f, resultante.QuantidadeAtual);
        Assert.True(resultante.EstaVazio);
    }

    [Fact]
    public void ConsumirFracionario_QuandoJaVazio_DeveRetornarTempoZero()
    {
        // Arrange
        var combustivel = new Combustivel(0.0f, 20.0f, 5.0f);

        // Act
        var resultante = combustivel.ConsumirFracionario(0.02f, out var tempoQueima);

        // Assert
        Assert.Equal(0.0f, tempoQueima);
        Assert.Equal(0.0f, resultante.QuantidadeAtual);
        Assert.True(resultante.EstaVazio);
    }

    [Fact]
    public void AbastecerTotal_DeveRestaurarCapacidadeMaxima()
    {
        // Arrange
        var combustivel = Combustivel.CriarCheio(100f, 10f).Consumir(8f); // 20 restantes

        // Act
        var abastecido = combustivel.AbastecerTotal();

        // Assert
        Assert.Equal(100f, abastecido.QuantidadeAtual);
        Assert.Equal(1.0f, abastecido.PercentualRestante);
    }
}
