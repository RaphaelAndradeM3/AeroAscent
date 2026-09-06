namespace AeroAscent.Apresentacao.MAUI.Servicos;

using System;
using AeroAscent.Core.Aplicacao.Contratos;
using AeroAscent.Core.Dominio.Enums;
using AeroAscent.Core.Dominio.ObjetosDeValor;

/// <summary>
/// Implementação do serviço de áudio da apresentação em .NET MAUI.
/// Fornece feedback auditivo com tolerância a falhas e controle de preferências.
/// </summary>
public sealed class ServicoAudioMAUI : IServicoAudio
{
    private ConfiguracaoAudio _configuracao = ConfiguracaoAudio.Padrao;
    private bool _propulsaoAtiva;
    private float _intensidadePropulsao;
    private float _intensidadeVento;

    /// <summary>
    /// Propriedade que indica se a propulsão está soando.
    /// </summary>
    public bool PropulsaoAtiva => _propulsaoAtiva;

    /// <summary>
    /// Intensidade atual da propulsão.
    /// </summary>
    public float IntensidadePropulsao => _intensidadePropulsao;

    /// <summary>
    /// Intensidade atual do vento.
    /// </summary>
    public float IntensidadeVento => _intensidadeVento;

    /// <inheritdoc />
    public void TocarEvento(EventoAudio evento, float escalaVolume = 1f)
    {
        if (!_configuracao.EfeitosAtivos)
        {
            return;
        }

        try
        {
#if WINDOWS
            if (evento == EventoAudio.ColetaMoeda)
            {
                Console.Beep(988, 50); // B5
            }
            else if (evento == EventoAudio.LancamentoCatapulta)
            {
                Console.Beep(523, 100); // C5
            }
            else if (evento == EventoAudio.NovoRecorde)
            {
                Console.Beep(1318, 150); // E6
            }
#endif
        }
        catch
        {
            // O subsistema de áudio tolera plataformas sem suporte a beep
        }
    }

    /// <inheritdoc />
    public void AtualizarLoopVento(float intensidadeNormalizada)
    {
        _intensidadeVento = Math.Clamp(intensidadeNormalizada, 0f, 1f);
    }

    /// <inheritdoc />
    public void DefinirLoopPropulsao(bool ativo, float intensidade = 1f)
    {
        _propulsaoAtiva = ativo;
        _intensidadePropulsao = Math.Clamp(intensidade, 0f, 1f);
    }

    /// <inheritdoc />
    public void TocarMusicaTema()
    {
        // Trilha sonora ambiente
    }

    /// <inheritdoc />
    public void PararMusica()
    {
        // Silenciar trilha ambiente
    }

    /// <inheritdoc />
    public void AplicarConfiguracao(in ConfiguracaoAudio configuracao)
    {
        _configuracao = configuracao;
    }

    /// <inheritdoc />
    public ConfiguracaoAudio ObterConfiguracao()
    {
        return _configuracao;
    }
}
