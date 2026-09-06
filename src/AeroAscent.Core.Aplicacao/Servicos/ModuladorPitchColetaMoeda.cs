namespace AeroAscent.Core.Aplicacao.Servicos;

using System;
using AeroAscent.Core.Dominio.Excecoes;

/// <summary>
/// Serviço de modulação harmônica procedural e controle de polifonia para a coleta sequencial de moedas.
/// Opera na stack e sem alocação no heap (<c>GC Alloc = 0 bytes</c>), elevando o pitch musical em arpeggio
/// quando moedas são obtidas em rápida sucessão, respeitando a janela temporal e o teto acústico.
/// </summary>
public class ModuladorPitchColetaMoeda
{
    /// <summary>
    /// Pitch inicial neutro correspondente à afinação natural do efeito sonoro.
    /// </summary>
    public const float PITCH_BASE = 1.0f;

    /// <summary>
    /// Acréscimo incremental no pitch a cada moeda coletada em rápida sucessão (+0.05).
    /// </summary>
    public const float INCREMENTO_PITCH = 0.05f;

    /// <summary>
    /// Limite superior para evitar que o áudio atinja frequências excessivamente agudas.
    /// </summary>
    public const float PITCH_MAXIMO = 1.30f;

    /// <summary>
    /// Janela máxima de tempo em segundos entre coletas para considerar a sequência contínua (0,3s).
    /// </summary>
    public const float JANELA_TEMPO_SEGUNDOS = 0.3f;

    /// <summary>
    /// Quantidade máxima de vozes simultâneas permitidas para o efeito de moedas.
    /// </summary>
    public const int MAXIMO_VOZES_SIMULTANEAS = 4;

    private float _pitchAtual = PITCH_BASE;
    private float _ultimoTempoSegundos = -1f;

    /// <summary>
    /// Pitch atualmente calculado para a próxima reprodução.
    /// </summary>
    public float PitchAtual => _pitchAtual;

    /// <summary>
    /// Registra uma nova coleta de moeda no tempo especificado e calcula o pitch resultante.
    /// </summary>
    /// <param name="tempoAtualSegundos">Instante de tempo da coleta em segundos (não negativo).</param>
    /// <returns>Valor do pitch modulado (entre 1.0f e 1.30f).</returns>
    /// <exception cref="DominioInvalidoException">Lançada se o tempo for negativo.</exception>
    public float RegistrarColeta(float tempoAtualSegundos)
    {
        if (tempoAtualSegundos < 0f)
        {
            throw new DominioInvalidoException(nameof(tempoAtualSegundos), "O tempo atual em segundos não pode ser negativo.");
        }

        if (_ultimoTempoSegundos >= 0f && (tempoAtualSegundos - _ultimoTempoSegundos) <= JANELA_TEMPO_SEGUNDOS)
        {
            _pitchAtual = MathF.Min(_pitchAtual + INCREMENTO_PITCH, PITCH_MAXIMO);
        }
        else
        {
            _pitchAtual = PITCH_BASE;
        }

        _ultimoTempoSegundos = tempoAtualSegundos;
        return _pitchAtual;
    }

    /// <summary>
    /// Avalia se uma nova voz pode ser alocada sem violar o limite máximo de polifonia.
    /// </summary>
    /// <param name="vozesAtivas">Quantidade atual de vozes de moedas em reprodução.</param>
    /// <returns><c>true</c> se a voz pode ser adicionada; <c>false</c> se deve haver reaproveitamento (voice stealing).</returns>
    public bool PodeAlocarNovaVoz(int vozesAtivas)
    {
        return vozesAtivas < MAXIMO_VOZES_SIMULTANEAS;
    }

    /// <summary>
    /// Redefine o modulador para o estado neutro inicial.
    /// </summary>
    public void Resetar()
    {
        _pitchAtual = PITCH_BASE;
        _ultimoTempoSegundos = -1f;
    }
}
