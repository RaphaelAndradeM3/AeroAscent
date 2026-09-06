namespace AeroAscent.Core.Aplicacao.Testes.Apresentadores;

using System;
using AeroAscent.Core.Aplicacao.Apresentadores;
using AeroAscent.Core.Aplicacao.Testes.Fixtures;
using AeroAscent.Core.Dominio.Excecoes;
using AeroAscent.Core.Dominio.ObjetosDeValor;
using Xunit;

/// <summary>
/// Suíte de testes unitários para o <see cref="ApresentadorResumoVoo"/> cobrindo formatação,
/// animação progressiva de recompensas, celebração de recordes e navegação.
/// </summary>
public class ApresentadorResumoVooTestes
{
    private readonly VisaoResumoVooFalsa _visao;
    private readonly ApresentadorResumoVoo _apresentador;

    public ApresentadorResumoVooTestes()
    {
        _visao = new VisaoResumoVooFalsa();
        _apresentador = new ApresentadorResumoVoo(_visao);
    }

    [Fact]
    public void Construtor_ComVisaoNula_DeveLancarDominioInvalidoException()
    {
        // Assert & Act
        Assert.Throws<DominioInvalidoException>(() => new ApresentadorResumoVoo(null!));
    }

    [Fact]
    public void Exibir_ComDadosValidos_DeveProjetarModeloVisualFormatadoCorretamente()
    {
        // Arrange
        var resumo = ResumoFinalizacaoVoo.Criar(
            distanciaMetros: 125.4f,
            altitudeMaximaMetros: 45.2f,
            moedasPorDistancia: 12,
            moedasPorAltitude: 2,
            moedasColetadas: 20,
            moedasTotalGanhas: new Moeda(34),
            saldoTotalAtualizado: new Moeda(1250),
            ehNovoRecordeDistancia: false,
            ehNovoRecordeAltitude: false);

        // Act
        _apresentador.Exibir(in resumo);

        // Assert
        Assert.Equal(1, _visao.ContadorExibicoes);
        Assert.Equal(125.4f, _visao.UltimoModelo.DistanciaMetros);
        Assert.Equal("125,4 m", _visao.UltimoModelo.DistanciaFormatada);
        Assert.Equal(45.2f, _visao.UltimoModelo.AltitudeMaximaMetros);
        Assert.Equal("45,2 m", _visao.UltimoModelo.AltitudeFormatada);
        Assert.Equal(12, _visao.UltimoModelo.MoedasDistancia);
        Assert.Equal(2, _visao.UltimoModelo.MoedasAltitude);
        Assert.Equal(20, _visao.UltimoModelo.MoedasColetadas);
        Assert.Equal(34, _visao.UltimoModelo.TotalMoedasGanhas);
        Assert.Equal("+34 moedas", _visao.UltimoModelo.TotalMoedasFormatado);
        Assert.Equal(1250, _visao.UltimoModelo.SaldoFinal);
        Assert.Equal("💰 1.250", _visao.UltimoModelo.SaldoFinalFormatado);
        Assert.False(_visao.UltimoModelo.EhNovoRecorde);
    }

    [Fact]
    public void Exibir_AoIniciar_DeveDesabilitarBotoesNavegacaoEDefinirAnimacaoEmAndamento()
    {
        // Arrange
        var resumo = ResumoFinalizacaoVoo.Criar(
            distanciaMetros: 50f,
            altitudeMaximaMetros: 20f,
            moedasPorDistancia: 5,
            moedasPorAltitude: 1,
            moedasColetadas: 4,
            moedasTotalGanhas: new Moeda(10),
            saldoTotalAtualizado: new Moeda(500),
            ehNovoRecordeDistancia: false,
            ehNovoRecordeAltitude: false);

        // Act
        _apresentador.Exibir(in resumo);

        // Assert
        Assert.True(_apresentador.AnimacaoEmAndamento);
        Assert.False(_visao.BotoesNavegacaoHabilitados);
        Assert.Equal(1, _visao.ContadorDefinicoesBotoes);
    }

    [Fact]
    public void ConcluirAnimacao_AoAtingirTempo_DeveLiberarBotoesNavegacaoEAtualizarStatusAnimacao()
    {
        // Arrange
        var resumo = ResumoFinalizacaoVoo.Criar(
            distanciaMetros: 50f,
            altitudeMaximaMetros: 20f,
            moedasPorDistancia: 5,
            moedasPorAltitude: 1,
            moedasColetadas: 4,
            moedasTotalGanhas: new Moeda(10),
            saldoTotalAtualizado: new Moeda(500),
            ehNovoRecordeDistancia: false,
            ehNovoRecordeAltitude: false);

        _apresentador.Exibir(in resumo);

        // Act
        _visao.SimularConclusaoAnimacaoMoedas();

        // Assert
        Assert.False(_apresentador.AnimacaoEmAndamento);
        Assert.True(_visao.BotoesNavegacaoHabilitados);
    }

    [Fact]
    public void PularAnimacao_DuranteAnimacao_DeveComandarVisaoParaConcluirMoedasELiberarBotoes()
    {
        // Arrange
        var resumo = ResumoFinalizacaoVoo.Criar(
            distanciaMetros: 100f,
            altitudeMaximaMetros: 30f,
            moedasPorDistancia: 10,
            moedasPorAltitude: 1,
            moedasColetadas: 5,
            moedasTotalGanhas: new Moeda(16),
            saldoTotalAtualizado: new Moeda(800),
            ehNovoRecordeDistancia: false,
            ehNovoRecordeAltitude: false);

        _apresentador.Exibir(in resumo);

        // Act
        _apresentador.PularAnimacao();

        // Assert
        Assert.Equal(1, _visao.ContadorConclusoesAnimacao);
        Assert.False(_apresentador.AnimacaoEmAndamento);
        Assert.True(_visao.BotoesNavegacaoHabilitados);
    }

    [Fact]
    public void SimularCliquePularAnimacao_VisaoDisparaPulo_DeveConcluirAnimacaoInstantaneamente()
    {
        // Arrange
        var resumo = ResumoFinalizacaoVoo.Criar(
            distanciaMetros: 100f,
            altitudeMaximaMetros: 30f,
            moedasPorDistancia: 10,
            moedasPorAltitude: 1,
            moedasColetadas: 5,
            moedasTotalGanhas: new Moeda(16),
            saldoTotalAtualizado: new Moeda(800),
            ehNovoRecordeDistancia: false,
            ehNovoRecordeAltitude: false);

        _apresentador.Exibir(in resumo);

        // Act
        _visao.SimularCliquePularAnimacao();

        // Assert
        Assert.Equal(1, _visao.ContadorConclusoesAnimacao);
        Assert.False(_apresentador.AnimacaoEmAndamento);
        Assert.True(_visao.BotoesNavegacaoHabilitados);
    }
}
