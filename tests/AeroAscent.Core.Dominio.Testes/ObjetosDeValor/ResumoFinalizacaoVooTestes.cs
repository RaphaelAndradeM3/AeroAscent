namespace AeroAscent.Core.Dominio.Testes.ObjetosDeValor;

using AeroAscent.Core.Dominio.Excecoes;
using AeroAscent.Core.Dominio.ObjetosDeValor;
using Xunit;

/// <summary>
/// Testes unitários para o objeto de valor na stack <see cref="ResumoFinalizacaoVoo"/>.
/// Valida integridade, imutabilidade, cálculo, criação cancelada e regras de invariantes.
/// </summary>
public class ResumoFinalizacaoVooTestes
{
    [Fact]
    public void Criar_ComValoresValidos_DevePreencherTodasAsPropriedadesCorretamente()
    {
        // Arrange & Act
        var resumo = ResumoFinalizacaoVoo.Criar(
            distanciaMetros: 250f,
            altitudeMaximaMetros: 80f,
            moedasPorDistancia: 25,
            moedasPorAltitude: 4,
            moedasColetadas: 5,
            moedasTotalGanhas: new Moeda(34),
            saldoTotalAtualizado: new Moeda(134),
            ehNovoRecordeDistancia: true,
            ehNovoRecordeAltitude: false);

        // Assert
        Assert.Equal(250f, resumo.DistanciaMetros);
        Assert.Equal(80f, resumo.AltitudeMaximaMetros);
        Assert.Equal(25, resumo.MoedasPorDistancia);
        Assert.Equal(4, resumo.MoedasPorAltitude);
        Assert.Equal(5, resumo.MoedasColetadas);
        Assert.Equal(34, resumo.MoedasTotalGanhas.Quantidade);
        Assert.Equal(134, resumo.SaldoTotalAtualizado.Quantidade);
        Assert.True(resumo.EhNovoRecordeDistancia);
        Assert.False(resumo.EhNovoRecordeAltitude);
    }

    [Fact]
    public void CriarCancelado_ComMetricasESaldo_DeveZerarMoedasERecordes()
    {
        // Arrange
        var saldoAtual = new Moeda(100);

        // Act
        var resumo = ResumoFinalizacaoVoo.CriarCancelado(120f, 45f, saldoAtual);

        // Assert
        Assert.Equal(120f, resumo.DistanciaMetros);
        Assert.Equal(45f, resumo.AltitudeMaximaMetros);
        Assert.Equal(0, resumo.MoedasPorDistancia);
        Assert.Equal(0, resumo.MoedasPorAltitude);
        Assert.Equal(0, resumo.MoedasColetadas);
        Assert.Equal(0, resumo.MoedasTotalGanhas.Quantidade);
        Assert.Equal(100, resumo.SaldoTotalAtualizado.Quantidade);
        Assert.False(resumo.EhNovoRecordeDistancia);
        Assert.False(resumo.EhNovoRecordeAltitude);
    }

    [Theory]
    [InlineData(-1f, 10f, 0, 0, 0)]
    [InlineData(10f, -1f, 0, 0, 0)]
    [InlineData(10f, 10f, -1, 0, 0)]
    [InlineData(10f, 10f, 0, -1, 0)]
    [InlineData(10f, 10f, 0, 0, -1)]
    public void Criar_ComValoresNegativos_DeveLancarDominioInvalidoException(
        float distancia,
        float altitude,
        long moedasDistancia,
        long moedasAltitude,
        int moedasColetadas)
    {
        // Act & Assert
        Assert.Throws<DominioInvalidoException>(() => ResumoFinalizacaoVoo.Criar(
            distancia,
            altitude,
            moedasDistancia,
            moedasAltitude,
            moedasColetadas,
            Moeda.Zero,
            Moeda.Zero,
            false,
            false));
    }

    [Fact]
    public void Igualdade_DuasInstanciasComMesmosValores_DevemSerIguais()
    {
        // Arrange
        var resumo1 = ResumoFinalizacaoVoo.Criar(100f, 50f, 10, 2, 3, new Moeda(15), new Moeda(115), true, true);
        var resumo2 = ResumoFinalizacaoVoo.Criar(100f, 50f, 10, 2, 3, new Moeda(15), new Moeda(115), true, true);

        // Assert
        Assert.Equal(resumo1, resumo2);
        Assert.True(resumo1 == resumo2);
    }
}
