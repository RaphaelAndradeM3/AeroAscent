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

    [Theory]
    [InlineData(true, false, true)]
    [InlineData(false, true, true)]
    [InlineData(true, true, true)]
    [InlineData(false, false, false)]
    public void Exibir_ComCombinacoesDeRecorde_DeveConfigurarFlagEhNovoRecordeAdequadamente(
        bool quebrouRecordeDistancia,
        bool quebrouRecordeAltitude,
        bool esperadoNovoRecordeGeral)
    {
        // Arrange
        var resumo = ResumoFinalizacaoVoo.Criar(
            distanciaMetros: 250f,
            altitudeMaximaMetros: 80f,
            moedasPorDistancia: 25,
            moedasPorAltitude: 4,
            moedasColetadas: 10,
            moedasTotalGanhas: new Moeda(39),
            saldoTotalAtualizado: new Moeda(2000),
            ehNovoRecordeDistancia: quebrouRecordeDistancia,
            ehNovoRecordeAltitude: quebrouRecordeAltitude);

        // Act
        _apresentador.Exibir(in resumo);

        // Assert
        Assert.Equal(quebrouRecordeDistancia, _visao.UltimoModelo.EhNovoRecordeDistancia);
        Assert.Equal(quebrouRecordeAltitude, _visao.UltimoModelo.EhNovoRecordeAltitude);
        Assert.Equal(esperadoNovoRecordeGeral, _visao.UltimoModelo.EhNovoRecorde);
    }

    [Fact]
    public void SimularCliqueOficina_AposConclusaoAnimacao_DeveDispararEventoEOcultarTela()
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
        _apresentador.ConcluirAnimacao();

        bool eventoDisparado = false;
        _apresentador.AoSolicitarIrParaOficina += () => eventoDisparado = true;

        // Act
        _visao.SimularCliqueOficina();

        // Assert
        Assert.True(eventoDisparado);
        Assert.True(_visao.TelaOcultada);
        Assert.Equal(1, _visao.ContadorOcultacoes);
    }

    [Fact]
    public void SimularCliqueVoarNovamente_AposConclusaoAnimacao_DeveDispararEventoEOcultarTela()
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
        _apresentador.ConcluirAnimacao();

        bool eventoDisparado = false;
        _apresentador.AoSolicitarVoarNovamente += () => eventoDisparado = true;

        // Act
        _visao.SimularCliqueVoarNovamente();

        // Assert
        Assert.True(eventoDisparado);
        Assert.True(_visao.TelaOcultada);
        Assert.Equal(1, _visao.ContadorOcultacoes);
    }

    [Fact]
    public void SimularCliqueOficina_DuranteAnimacao_DeveApenasPularAnimacaoESoNavegarNoSegundoClique()
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

        int contagemDisparos = 0;
        _apresentador.AoSolicitarIrParaOficina += () => contagemDisparos++;

        // Act 1: Primeiro clique durante a animação
        _visao.SimularCliqueOficina();

        // Assert 1: Não deve navegar, apenas concluir a animação
        Assert.Equal(0, contagemDisparos);
        Assert.False(_visao.TelaOcultada);
        Assert.False(_apresentador.AnimacaoEmAndamento);
        Assert.Equal(1, _visao.ContadorConclusoesAnimacao);

        // Act 2: Segundo clique agora com animação já concluída
        _visao.SimularCliqueOficina();

        // Assert 2: Agora sim deve navegar e ocultar a tela
        Assert.Equal(1, contagemDisparos);
        Assert.True(_visao.TelaOcultada);
    }

    [Fact]
    public void SimularCliqueVoarNovamente_DuranteAnimacao_DeveApenasPularAnimacaoESoNavegarNoSegundoClique()
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

        int contagemDisparos = 0;
        _apresentador.AoSolicitarVoarNovamente += () => contagemDisparos++;

        // Act 1: Primeiro clique durante a animação
        _visao.SimularCliqueVoarNovamente();

        // Assert 1: Não navega, apenas encerra a contagem
        Assert.Equal(0, contagemDisparos);
        Assert.False(_visao.TelaOcultada);
        Assert.False(_apresentador.AnimacaoEmAndamento);
        Assert.Equal(1, _visao.ContadorConclusoesAnimacao);

        // Act 2: Segundo clique com animação concluída
        _visao.SimularCliqueVoarNovamente();

        // Assert 2: Navega e oculta a tela
        Assert.Equal(1, contagemDisparos);
        Assert.True(_visao.TelaOcultada);
    }

    [Fact]
    public void Ocultar_ChamadaDireta_DeveComandarVisaoParaOcultar()
    {
        // Act
        _apresentador.Ocultar();

        // Assert
        Assert.True(_visao.TelaOcultada);
        Assert.Equal(1, _visao.ContadorOcultacoes);
    }
}
