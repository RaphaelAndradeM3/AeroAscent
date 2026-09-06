namespace AeroAscent.Core.Aplicacao.Testes.Servicos;

using AeroAscent.Core.Aplicacao.Servicos;
using AeroAscent.Core.Aplicacao.Testes.Fixtures;
using AeroAscent.Core.Dominio.Enums;
using AeroAscent.Core.Dominio.Excecoes;
using AeroAscent.Core.Dominio.ObjetosDeValor;
using Xunit;

/// <summary>
/// Suíte de testes unitários para o subsistema de áudio, cobrindo despacho de eventos,
/// loops contínuos de vento e boost, modulação harmônica de moedas e polifonia.
/// </summary>
public class ServicoAudioTestes
{
    private readonly ServicoAudioFalso _servicoAudio;
    private readonly ModuladorPitchColetaMoeda _moduladorPitch;

    public ServicoAudioTestes()
    {
        _servicoAudio = new ServicoAudioFalso();
        _moduladorPitch = new ModuladorPitchColetaMoeda();
    }

    [Fact]
    public void TocarEvento_ComEventoValido_DeveRegistrarNoHistoricoEContador()
    {
        // Act
        _servicoAudio.TocarEvento(EventoAudio.LancamentoCatapulta, 0.9f);
        _servicoAudio.TocarEvento(EventoAudio.ColetaMoeda, 1.0f);

        // Assert
        Assert.Equal(2, _servicoAudio.ContadorDisparosEventos);
        Assert.Equal(EventoAudio.ColetaMoeda, _servicoAudio.UltimoEventoTocado);
        Assert.Equal(1.0f, _servicoAudio.UltimaEscalaVolume);
        Assert.Contains(EventoAudio.LancamentoCatapulta, _servicoAudio.HistoricoEventos);
        Assert.Contains(EventoAudio.ColetaMoeda, _servicoAudio.HistoricoEventos);
    }

    [Fact]
    public void AtualizarLoopVento_ComIntensidadeNormalizada_DeveArmazenarValor()
    {
        // Act
        _servicoAudio.AtualizarLoopVento(0.75f);

        // Assert
        Assert.Equal(1, _servicoAudio.ContadorAtualizacoesVento);
        Assert.Equal(0.75f, _servicoAudio.UltimaIntensidadeVento);
    }

    [Fact]
    public void DefinirLoopPropulsao_ComEstadoEIntensidade_DeveAtualizarPropriedades()
    {
        // Act
        _servicoAudio.DefinirLoopPropulsao(ativo: true, intensidade: 0.95f);

        // Assert
        Assert.Equal(1, _servicoAudio.ContadorDefinicoesPropulsao);
        Assert.True(_servicoAudio.LoopPropulsaoAtivo);
        Assert.Equal(0.95f, _servicoAudio.UltimaIntensidadePropulsao);

        // Act - desligar
        _servicoAudio.DefinirLoopPropulsao(ativo: false, intensidade: 0f);

        // Assert - desligado
        Assert.False(_servicoAudio.LoopPropulsaoAtivo);
        Assert.Equal(0f, _servicoAudio.UltimaIntensidadePropulsao);
    }

    [Fact]
    public void TocarMusicaTema_EPararMusica_DeveAtualizarEstadoDeExecucao()
    {
        // Act - Iniciar
        _servicoAudio.TocarMusicaTema();

        // Assert
        Assert.True(_servicoAudio.MusicaTemaTocando);
        Assert.Equal(1, _servicoAudio.ContadorInicioMusica);

        // Act - Parar
        _servicoAudio.PararMusica();

        // Assert
        Assert.False(_servicoAudio.MusicaTemaTocando);
        Assert.Equal(1, _servicoAudio.ContadorParadaMusica);
    }

    [Fact]
    public void AplicarConfiguracao_ComNovaConfiguracao_DeveAtualizarConfiguracaoAtiva()
    {
        // Arrange
        var novaConfig = new ConfiguracaoAudio(0.5f, 0.4f, true, false);

        // Act
        _servicoAudio.AplicarConfiguracao(in novaConfig);

        // Assert
        Assert.Equal(1, _servicoAudio.ContadorAplicacoesConfiguracao);
        Assert.Equal(0.5f, _servicoAudio.ObterConfiguracao().VolumeEfeitos);
        Assert.Equal(0.4f, _servicoAudio.ObterConfiguracao().VolumeMusica);
        Assert.True(_servicoAudio.ObterConfiguracao().EfeitosAtivos);
        Assert.False(_servicoAudio.ObterConfiguracao().MusicaAtiva);
    }

    [Fact]
    public void ModuladorPitch_ComColetasEmRapidaSucessao_DeveElevarPitchEmArpeggioAteTeto()
    {
        // Act & Assert
        var pitch1 = _moduladorPitch.RegistrarColeta(0.0f);
        Assert.Equal(ModuladorPitchColetaMoeda.PITCH_BASE, pitch1);

        var pitch2 = _moduladorPitch.RegistrarColeta(0.1f);
        Assert.Equal(1.05f, MathF.Round(pitch2, 2));

        var pitch3 = _moduladorPitch.RegistrarColeta(0.2f);
        Assert.Equal(1.10f, MathF.Round(pitch3, 2));

        var pitch4 = _moduladorPitch.RegistrarColeta(0.28f);
        Assert.Equal(1.15f, MathF.Round(pitch4, 2));

        var pitch5 = _moduladorPitch.RegistrarColeta(0.35f);
        Assert.Equal(1.20f, MathF.Round(pitch5, 2));

        var pitch6 = _moduladorPitch.RegistrarColeta(0.45f);
        Assert.Equal(1.25f, MathF.Round(pitch6, 2));

        var pitch7 = _moduladorPitch.RegistrarColeta(0.55f);
        Assert.Equal(ModuladorPitchColetaMoeda.PITCH_MAXIMO, MathF.Round(pitch7, 2));

        // Mais uma coleta ainda rápida não deve ultrapassar o teto
        var pitch8 = _moduladorPitch.RegistrarColeta(0.65f);
        Assert.Equal(ModuladorPitchColetaMoeda.PITCH_MAXIMO, MathF.Round(pitch8, 2));
    }

    [Fact]
    public void ModuladorPitch_AposIntervaloSuperiorAJanela_DeveResetarParaPitchBase()
    {
        // Arrange
        _moduladorPitch.RegistrarColeta(0.0f);
        _moduladorPitch.RegistrarColeta(0.1f); // pitch 1.05f

        // Act - Coleta após 0.5s (janela de 0.3s expirada)
        var pitchAposPausa = _moduladorPitch.RegistrarColeta(0.6f);

        // Assert
        Assert.Equal(ModuladorPitchColetaMoeda.PITCH_BASE, pitchAposPausa);
    }

    [Fact]
    public void ModuladorPitch_PodeAlocarNovaVoz_DeveRespeitarLimiteMaximoDe4Vozes()
    {
        // Assert
        Assert.True(_moduladorPitch.PodeAlocarNovaVoz(0));
        Assert.True(_moduladorPitch.PodeAlocarNovaVoz(1));
        Assert.True(_moduladorPitch.PodeAlocarNovaVoz(2));
        Assert.True(_moduladorPitch.PodeAlocarNovaVoz(3));
        Assert.False(_moduladorPitch.PodeAlocarNovaVoz(4));
        Assert.False(_moduladorPitch.PodeAlocarNovaVoz(5));
    }

    [Fact]
    public void ModuladorPitch_ComTempoNegativo_DeveLancarDominioInvalidoException()
    {
        // Assert & Act
        Assert.Throws<DominioInvalidoException>(() => _moduladorPitch.RegistrarColeta(-0.5f));
    }

    [Fact]
    public void ModuladorPitch_Resetar_DeveRestaurarEstadoNeutro()
    {
        // Arrange
        _moduladorPitch.RegistrarColeta(0.0f);
        _moduladorPitch.RegistrarColeta(0.1f);

        // Act
        _moduladorPitch.Resetar();

        // Assert
        Assert.Equal(ModuladorPitchColetaMoeda.PITCH_BASE, _moduladorPitch.PitchAtual);
    }
}
