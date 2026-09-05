namespace AeroAscent.Core.Dominio.ObjetosDeValor;

using System;
using AeroAscent.Core.Dominio.Excecoes;

/// <summary>
/// Representa o medidor oscilante de força da catapulta em C# puro, modelado como readonly record struct na stack.
/// Calcula de forma determinística e analítica a precisão instantânea (0.0 a 1.0) através de uma onda periódica triangular,
/// permitindo testabilidade isolada e integração desacoplada com a interface da Unity Engine.
/// </summary>
public readonly record struct MedidorForcaOscilante
{
    /// <summary>
    /// Frequência de oscilação padrão do medidor de força em Hertz (1.0 Hz = 1 ciclo de ida e volta por segundo).
    /// </summary>
    public const float FREQUENCIA_PADRAO_HZ = 1.0f;

    /// <summary>
    /// Frequência de oscilação da barra de força em ciclos por segundo (Hz).
    /// </summary>
    public float FrequenciaHz { get; }

    /// <summary>
    /// Inicializa uma nova instância do medidor com a frequência especificada.
    /// </summary>
    /// <param name="frequenciaHz">Frequência em Hertz (deve ser estritamente positiva, padrão 1.0 Hz).</param>
    /// <exception cref="DominioInvalidoException">Lançada caso a frequência seja menor ou igual a zero.</exception>
    public MedidorForcaOscilante(float frequenciaHz = FREQUENCIA_PADRAO_HZ)
    {
        if (frequenciaHz <= 0f)
        {
            throw new DominioInvalidoException(
                nameof(frequenciaHz),
                $"A frequência de oscilação do medidor ({frequenciaHz} Hz) deve ser maior que zero.");
        }

        FrequenciaHz = frequenciaHz;
    }

    /// <summary>
    /// Calcula a precisão instantânea contínua (0.0 a 1.0) em função do tempo decorrido.
    /// Utiliza função de onda triangular analítica com pico em meio período (t = 0.5s para 1 Hz).
    /// </summary>
    /// <param name="tempoSegundos">Tempo transcorrido desde o início da oscilação em segundos.</param>
    /// <returns>Fator de precisão normalizado no intervalo fechado [0.0, 1.0].</returns>
    /// <exception cref="DominioInvalidoException">Lançada se o tempo for negativo.</exception>
    public float ObterFatorPrecisao(float tempoSegundos)
    {
        if (tempoSegundos < 0f)
        {
            throw new DominioInvalidoException(
                nameof(tempoSegundos),
                $"O tempo decorrido para cálculo de precisão ({tempoSegundos}s) não pode ser negativo.");
        }

        var ciclo = (2.0f * tempoSegundos * FrequenciaHz) % 2.0f;
        var fator = 1.0f - MathF.Abs(ciclo - 1.0f);
        return Math.Max(0.0f, Math.Min(1.0f, fator));
    }
}
