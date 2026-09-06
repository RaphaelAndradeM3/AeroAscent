namespace AeroAscent.Core.Aplicacao.Testes.Apresentadores;

using System;
using AeroAscent.Core.Aplicacao.Apresentadores;
using AeroAscent.Core.Aplicacao.Testes.Fixtures;
using AeroAscent.Core.Dominio.Entidades;
using AeroAscent.Core.Dominio.Enums;
using AeroAscent.Core.Dominio.Excecoes;
using AeroAscent.Core.Dominio.ObjetosDeValor;
using Xunit;

/// <summary>
/// Suíte de testes unitários para o ApresentadorHUDVoo cobrindo telemetria, detecção de recordes e performance.
/// </summary>
public class ApresentadorHUDVooTestes
{
    private readonly VisaoHUDVooFalsa _visao;
    private readonly ApresentadorHUDVoo _apresentador;

    public ApresentadorHUDVooTestes()
    {
        _visao = new VisaoHUDVooFalsa();
        _apresentador = new ApresentadorHUDVoo(_visao);
    }

    [Fact]
    public void Construtor_ComVisaoNula_DeveLancarDominioInvalidoException()
    {
        // Assert & Act
        Assert.Throws<DominioInvalidoException>(() => new ApresentadorHUDVoo(null!));
    }

    [Fact]
    public void Inicializar_ComRecordePositivo_DeveRegistrarRecordeCorretamente()
    {
        // Act
        _apresentador.Inicializar(250f);

        // Assert
        Assert.False(_visao.NovoRecordeNotificado);
    }

    [Fact]
    public void Atualizar_ComVooNulo_DeveLancarDominioInvalidoException()
    {
        // Arrange
        var estado = EstadoFisicoAeronave.CriarInicial(VetorVoo.Zero, VetorVoo.Zero, 0f);

        // Assert & Act
        Assert.Throws<DominioInvalidoException>(() => _apresentador.Atualizar(null!, in estado));
    }

    [Fact]
    public void Atualizar_ComVooAtivo_DeveProjetarTelemetriaExataNaVisao()
    {
        // Arrange
        _apresentador.Inicializar(200f);
        var voo = Voo.Iniciar(Aeronave.CriarPadrao());
        voo.Decolar();
        voo.AtualizarMetricas(125.4f, 45.2f, 8);

        var posicao = new VetorVoo(0f, 45.2f, 125.4f);
        var velocidade = new VetorVoo(0f, 0f, 28.5f);
        var estado = EstadoFisicoAeronave.CriarInicial(posicao, velocidade, 10f);

        // Act
        _apresentador.Atualizar(voo, in estado);

        // Assert
        Assert.Equal(1, _visao.ContadorAtualizacoesTelemetria);
        var t = _visao.UltimaTelemetria;
        Assert.Equal(125.4f, t.DistanciaPercorridaMetros);
        Assert.Equal(200.0f, t.RecordeDistanciaMetros);
        Assert.Equal(45.2f, t.AltitudeAtualMetros);
        Assert.Equal(28.5f, t.VelocidadeAtualMetrosPorSegundo);
        Assert.Equal(8, t.MoedasColetadas);
        Assert.False(t.RecordeSuperado);
        Assert.True(t.BoostDisponivel);
    }

    [Fact]
    public void Atualizar_QuandoUltrapassarRecorde_DeveNotificarNovoRecordeUmaUnicaVez()
    {
        // Arrange
        _apresentador.Inicializar(100f);
        var voo = Voo.Iniciar(Aeronave.CriarPadrao());
        voo.Decolar();

        var estado = EstadoFisicoAeronave.CriarInicial(new VetorVoo(0, 10, 99), new VetorVoo(0, 0, 20), 0f);

        // Act 1: Distância menor que recorde (99m)
        voo.AtualizarMetricas(99f, 10f, 0);
        _apresentador.Atualizar(voo, in estado);

        Assert.False(_visao.NovoRecordeNotificado);
        Assert.Equal(0, _visao.ContadorNotificacoesRecorde);

        // Act 2: Distância supera recorde (101m)
        voo.AtualizarMetricas(101f, 10f, 0);
        estado = EstadoFisicoAeronave.CriarInicial(new VetorVoo(0, 10, 101), new VetorVoo(0, 0, 20), 0f);
        _apresentador.Atualizar(voo, in estado);

        Assert.True(_visao.NovoRecordeNotificado);
        Assert.Equal(1, _visao.ContadorNotificacoesRecorde);
        Assert.True(_visao.UltimaTelemetria.RecordeSuperado);

        // Act 3: Distância continua aumentando (150m) - Não deve notificar novamente
        voo.AtualizarMetricas(150f, 10f, 0);
        estado = EstadoFisicoAeronave.CriarInicial(new VetorVoo(0, 10, 150), new VetorVoo(0, 0, 20), 0f);
        _apresentador.Atualizar(voo, in estado);

        Assert.Equal(1, _visao.ContadorNotificacoesRecorde);
    }

    [Fact]
    public void Atualizar_LoopContinuo_NaoDeveAlocarMemoriaNoHeap()
    {
        // Arrange
        _apresentador.Inicializar(100f);
        var voo = Voo.Iniciar(Aeronave.CriarPadrao());
        voo.Decolar();
        voo.AtualizarMetricas(50f, 20f, 2);
        var estado = EstadoFisicoAeronave.CriarInicial(new VetorVoo(0, 20, 50), new VetorVoo(0, 0, 15), 0f);

        // Aquecimento (Warmup JIT)
        for (int i = 0; i < 10; i++)
        {
            _apresentador.Atualizar(voo, in estado);
        }

        // Act: 100 iterações de loop contínuo medindo GC Alloc
        long memoriaAntes = GC.GetAllocatedBytesForCurrentThread();

        for (int i = 0; i < 100; i++)
        {
            _apresentador.Atualizar(voo, in estado);
        }

        long memoriaDepois = GC.GetAllocatedBytesForCurrentThread();
        long diferencaBytes = memoriaDepois - memoriaAntes;

        // Assert: SC-001 - Zero alocação de lixo no heap
        Assert.Equal(0L, diferencaBytes);
    }

    [Fact]
    public void ComandosInclinacao_SubirDescerENeutro_DeveGerarParametrosControleEsperados()
    {
        // Act & Assert 1: Inicialmente neutro
        var controle = _apresentador.ObterComandosControle();
        Assert.Equal(0.0f, controle.IntensidadePitch);
        Assert.False(controle.TemComandoAtivo);

        // Act & Assert 2: Iniciar subida
        _apresentador.IniciarSubida();
        controle = _apresentador.ObterComandosControle();
        Assert.Equal(1.0f, controle.IntensidadePitch);
        Assert.True(controle.TemComandoAtivo);

        // Act & Assert 3: Iniciar descida concorrente (multitoque oposto anula para neutro)
        _apresentador.IniciarDescida();
        controle = _apresentador.ObterComandosControle();
        Assert.Equal(0.0f, controle.IntensidadePitch);

        // Act & Assert 4: Parar subida (permanece apenas descendo)
        _apresentador.PararSubida();
        controle = _apresentador.ObterComandosControle();
        Assert.Equal(-1.0f, controle.IntensidadePitch);

        // Act & Assert 5: Parar descida (retorna a neutro)
        _apresentador.PararDescida();
        controle = _apresentador.ObterComandosControle();
        Assert.Equal(0.0f, controle.IntensidadePitch);
    }

    [Fact]
    public void EventosDaVisao_DevemAtualizarComandosDoApresentador()
    {
        // Act & Assert: Subida via visão
        _visao.SimularPressionarSubida();
        Assert.Equal(1.0f, _apresentador.ObterComandosControle().IntensidadePitch);

        _visao.SimularLiberarSubida();
        Assert.Equal(0.0f, _apresentador.ObterComandosControle().IntensidadePitch);

        // Act & Assert: Descida via visão
        _visao.SimularPressionarDescida();
        Assert.Equal(-1.0f, _apresentador.ObterComandosControle().IntensidadePitch);

        _visao.SimularLiberarDescida();
        Assert.Equal(0.0f, _apresentador.ObterComandosControle().IntensidadePitch);

        // Act & Assert: Boost via visão
        _visao.SimularPressionarBoost();
        Assert.True(_apresentador.ObterComandosControle().AcionarBoost);

        _visao.SimularLiberarBoost();
        Assert.False(_apresentador.ObterComandosControle().AcionarBoost);
    }

    [Fact]
    public void Boost_QuandoCombustivelEsgotar_DeveDesativarComandoEEsmaecerBotao()
    {
        // Arrange
        _apresentador.Inicializar(100f);
        var voo = Voo.Iniciar(Aeronave.CriarPadrao());
        voo.Decolar();

        // Aeronave no ar com boost ativado
        var estadoAr = EstadoFisicoAeronave.CriarInicial(new VetorVoo(0, 50, 100), new VetorVoo(0, 0, 30), 0f);
        _apresentador.IniciarBoost();
        _apresentador.Atualizar(voo, in estadoAr);

        Assert.True(_apresentador.ObterComandosControle().AcionarBoost);
        Assert.True(_visao.BoostHabilitado);

        // Consome todo o combustível da aeronave
        voo.ConsumirCombustivel(100f, out _);
        Assert.True(voo.Combustivel.EstaVazio);

        // Act: Atualização do HUD com combustível esgotado
        _apresentador.Atualizar(voo, in estadoAr);

        // Assert: Boost desativado automaticamente e botão desabilitado na visão
        Assert.False(_apresentador.ObterComandosControle().AcionarBoost);
        Assert.False(_visao.BoostHabilitado);
        Assert.False(_visao.UltimaTelemetria.BoostDisponivel);
    }

    [Fact]
    public void SolicitarPausa_DeveAlternarEstadoEmitirEventoEResetarComandosAtivos()
    {
        // Arrange
        var eventoDisparado = false;
        _apresentador.AoSolicitarPausa += () => eventoDisparado = true;

        _apresentador.IniciarSubida();
        _apresentador.IniciarBoost();
        Assert.True(_apresentador.ObterComandosControle().AcionarBoost);
        Assert.Equal(1.0f, _apresentador.ObterComandosControle().IntensidadePitch);

        // Act 1: Pausar
        _apresentador.SolicitarPausa();

        // Assert 1: Jogo pausado, inputs resetados e neutros
        Assert.True(_apresentador.EstaPausado);
        Assert.True(eventoDisparado);
        var controlePausado = _apresentador.ObterComandosControle();
        Assert.Equal(0.0f, controlePausado.IntensidadePitch);
        Assert.False(controlePausado.AcionarBoost);

        // Tentar enviar comando durante a pausa deve ser ignorado
        _apresentador.IniciarDescida();
        Assert.Equal(0.0f, _apresentador.ObterComandosControle().IntensidadePitch);

        // Act 2: Despausar
        _apresentador.SolicitarPausa();
        Assert.False(_apresentador.EstaPausado);
    }

    [Fact]
    public void Atualizar_QuandoVooPousarOuCancelar_DeveOcultarControlesVisuais()
    {
        // Arrange
        _apresentador.Inicializar(100f);
        var voo = Voo.Iniciar(Aeronave.CriarPadrao());
        voo.Decolar();
        var estadoAr = EstadoFisicoAeronave.CriarInicial(new VetorVoo(0, 20, 100), new VetorVoo(0, 0, 10), 0f);

        _apresentador.Atualizar(voo, in estadoAr);
        Assert.True(_visao.ControlesVisiveis);

        // Act: Pouso da aeronave
        voo.Pousar();
        var estadoSolo = EstadoFisicoAeronave.CriarInicial(new VetorVoo(0, 0, 150), VetorVoo.Zero, 0f);
        _apresentador.Atualizar(voo, in estadoSolo);

        // Assert: Botões táteis ocultados da interface
        Assert.False(_visao.ControlesVisiveis);
        Assert.False(_apresentador.ObterComandosControle().AcionarBoost);
        Assert.Equal(0.0f, _apresentador.ObterComandosControle().IntensidadePitch);
    }
}
