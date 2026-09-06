namespace AeroAscent.Core.Dominio.ObjetosDeValor;

using System;
using AeroAscent.Core.Dominio.Excecoes;

/// <summary>
/// Objeto de valor imutável alocado exclusivamente na stack (<c>readonly record struct</c>, <c>GC Alloc = 0 bytes</c>)
/// que encapsula as preferências sonoras de volume e ativação de canais de efeitos (SFX) e música de fundo.
/// </summary>
public readonly record struct ConfiguracaoAudio
{
    /// <summary>
    /// Volume normalizado dos efeitos sonoros (SFX) entre 0.0f (silencioso) e 1.0f (máximo).
    /// </summary>
    public float VolumeEfeitos { get; }

    /// <summary>
    /// Volume normalizado da música temática entre 0.0f (silencioso) e 1.0f (máximo).
    /// </summary>
    public float VolumeMusica { get; }

    /// <summary>
    /// Sinaliza se a reprodução dos efeitos sonoros está habilitada.
    /// </summary>
    public bool EfeitosAtivos { get; }

    /// <summary>
    /// Sinaliza se a reprodução da música de fundo está habilitada.
    /// </summary>
    public bool MusicaAtiva { get; }

    /// <summary>
    /// Configuração acústica padrão recomendada: SFX 80%, Música 70%, ambos canais habilitados.
    /// </summary>
    public static readonly ConfiguracaoAudio Padrao = new(0.8f, 0.7f, true, true);

    /// <summary>
    /// Inicializa uma nova instância imutável de <see cref="ConfiguracaoAudio"/> com validação estrita de limites.
    /// </summary>
    /// <param name="volumeEfeitos">Volume dos efeitos sonoros (0.0f a 1.0f).</param>
    /// <param name="volumeMusica">Volume da música ambiente (0.0f a 1.0f).</param>
    /// <param name="efeitosAtivos">Indicador de ativação do canal de efeitos.</param>
    /// <param name="musicaAtiva">Indicador de ativação do canal musical.</param>
    /// <exception cref="DominioInvalidoException">Lançada caso qualquer volume seja menor que 0.0f ou maior que 1.0f.</exception>
    public ConfiguracaoAudio(float volumeEfeitos, float volumeMusica, bool efeitosAtivos, bool musicaAtiva)
    {
        if (volumeEfeitos < 0f || volumeEfeitos > 1f)
        {
            throw new DominioInvalidoException(nameof(volumeEfeitos), $"O volume de efeitos deve estar entre 0.0 e 1.0. Valor recebido: {volumeEfeitos}.");
        }

        if (volumeMusica < 0f || volumeMusica > 1f)
        {
            throw new DominioInvalidoException(nameof(volumeMusica), $"O volume de música deve estar entre 0.0 e 1.0. Valor recebido: {volumeMusica}.");
        }

        VolumeEfeitos = volumeEfeitos;
        VolumeMusica = volumeMusica;
        EfeitosAtivos = efeitosAtivos;
        MusicaAtiva = musicaAtiva;
    }

    /// <summary>
    /// Retorna uma nova instância com o volume de efeitos ajustado.
    /// </summary>
    /// <param name="novoVolume">Novo valor de volume para efeitos (0.0f a 1.0f).</param>
    /// <returns>Nova configuração imutável.</returns>
    public ConfiguracaoAudio ComVolumeEfeitos(float novoVolume)
    {
        return new ConfiguracaoAudio(novoVolume, VolumeMusica, EfeitosAtivos, MusicaAtiva);
    }

    /// <summary>
    /// Retorna uma nova instância com o volume de música ajustado.
    /// </summary>
    /// <param name="novoVolume">Novo valor de volume para música (0.0f a 1.0f).</param>
    /// <returns>Nova configuração imutável.</returns>
    public ConfiguracaoAudio ComVolumeMusica(float novoVolume)
    {
        return new ConfiguracaoAudio(VolumeEfeitos, novoVolume, EfeitosAtivos, MusicaAtiva);
    }

    /// <summary>
    /// Retorna uma nova instância invertendo o estado de ativação dos efeitos sonoros.
    /// </summary>
    /// <returns>Nova configuração com flag de efeitos alternada.</returns>
    public ConfiguracaoAudio AlternarEfeitos()
    {
        return new ConfiguracaoAudio(VolumeEfeitos, VolumeMusica, !EfeitosAtivos, MusicaAtiva);
    }

    /// <summary>
    /// Retorna uma nova instância invertendo o estado de ativação da música ambiente.
    /// </summary>
    /// <returns>Nova configuração com flag musical alternada.</returns>
    public ConfiguracaoAudio AlternarMusica()
    {
        return new ConfiguracaoAudio(VolumeEfeitos, VolumeMusica, EfeitosAtivos, !MusicaAtiva);
    }
}
