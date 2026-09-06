namespace AeroAscent.Core.Aplicacao.Testes.Fixtures;

using System.Collections.Generic;
using AeroAscent.Core.Aplicacao.Contratos;
using AeroAscent.Core.Dominio.Enums;
using AeroAscent.Core.Dominio.ObjetosDeValor;

/// <summary>
/// Implementação falsa (Spy/Mock) do <see cref="IServicoAudio"/> para validação em testes unitários xUnit.
/// </summary>
public class ServicoAudioFalso : IServicoAudio
{
    private readonly List<EventoAudio> _historicoEventos = new();

    /// <summary>
    /// Histórico de todos os eventos sonoros disparados na sessão.
    /// </summary>
    public IReadOnlyList<EventoAudio> HistoricoEventos => _historicoEventos;

    /// <summary>
    /// Último evento sonoro reproduzido.
    /// </summary>
    public EventoAudio? UltimoEventoTocado { get; private set; }

    /// <summary>
    /// Última escala de volume informada ao tocar evento.
    /// </summary>
    public float UltimaEscalaVolume { get; private set; } = 1f;

    /// <summary>
    /// Quantidade de vezes que TocarEvento foi invocado.
    /// </summary>
    public int ContadorDisparosEventos { get; private set; }

    /// <summary>
    /// Última intensidade normalizada de vento registrada.
    /// </summary>
    public float UltimaIntensidadeVento { get; private set; }

    /// <summary>
    /// Quantidade de vezes que AtualizarLoopVento foi invocado.
    /// </summary>
    public int ContadorAtualizacoesVento { get; private set; }

    /// <summary>
    /// Estado ativo/inativo do loop de propulsão.
    /// </summary>
    public bool LoopPropulsaoAtivo { get; private set; }

    /// <summary>
    /// Intensidade da propulsão informada no loop.
    /// </summary>
    public float UltimaIntensidadePropulsao { get; private set; }

    /// <summary>
    /// Quantidade de vezes que DefinirLoopPropulsao foi invocado.
    /// </summary>
    public int ContadorDefinicoesPropulsao { get; private set; }

    /// <summary>
    /// Indica se a música tema está em execução.
    /// </summary>
    public bool MusicaTemaTocando { get; private set; }

    /// <summary>
    /// Quantidade de vezes que TocarMusicaTema foi invocado.
    /// </summary>
    public int ContadorInicioMusica { get; private set; }

    /// <summary>
    /// Quantidade de vezes que PararMusica foi invocado.
    /// </summary>
    public int ContadorParadaMusica { get; private set; }

    /// <summary>
    /// Configuração atual aplicada.
    /// </summary>
    public ConfiguracaoAudio ConfiguracaoAtual { get; private set; } = ConfiguracaoAudio.Padrao;

    /// <summary>
    /// Quantidade de vezes que AplicarConfiguracao foi invocado.
    /// </summary>
    public int ContadorAplicacoesConfiguracao { get; private set; }

    /// <inheritdoc />
    public void TocarEvento(EventoAudio evento, float escalaVolume = 1f)
    {
        UltimoEventoTocado = evento;
        UltimaEscalaVolume = escalaVolume;
        _historicoEventos.Add(evento);
        ContadorDisparosEventos++;
    }

    /// <inheritdoc />
    public void AtualizarLoopVento(float intensidadeNormalizada)
    {
        UltimaIntensidadeVento = intensidadeNormalizada;
        ContadorAtualizacoesVento++;
    }

    /// <inheritdoc />
    public void DefinirLoopPropulsao(bool ativo, float intensidade = 1f)
    {
        LoopPropulsaoAtivo = ativo;
        UltimaIntensidadePropulsao = intensidade;
        ContadorDefinicoesPropulsao++;
    }

    /// <inheritdoc />
    public void TocarMusicaTema()
    {
        MusicaTemaTocando = true;
        ContadorInicioMusica++;
    }

    /// <inheritdoc />
    public void PararMusica()
    {
        MusicaTemaTocando = false;
        ContadorParadaMusica++;
    }

    /// <inheritdoc />
    public void AplicarConfiguracao(in ConfiguracaoAudio configuracao)
    {
        ConfiguracaoAtual = configuracao;
        ContadorAplicacoesConfiguracao++;
    }

    /// <inheritdoc />
    public ConfiguracaoAudio ObterConfiguracao()
    {
        return ConfiguracaoAtual;
    }

    /// <summary>
    /// Limpa o histórico de eventos e redefine contadores para novos testes.
    /// </summary>
    public void Limpar()
    {
        _historicoEventos.Clear();
        UltimoEventoTocado = null;
        ContadorDisparosEventos = 0;
        ContadorAtualizacoesVento = 0;
        ContadorDefinicoesPropulsao = 0;
        ContadorInicioMusica = 0;
        ContadorParadaMusica = 0;
    }
}
