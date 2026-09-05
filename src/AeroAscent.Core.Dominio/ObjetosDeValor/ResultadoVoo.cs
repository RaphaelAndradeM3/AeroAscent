namespace AeroAscent.Core.Dominio.ObjetosDeValor;

using AeroAscent.Core.Dominio.Excecoes;

/// <summary>
/// Objeto de valor imutável que consolida as métricas registradas ao final de um voo e calcula a recompensa financeira do jogador.
/// </summary>
public record ResultadoVoo
{
    /// <summary>
    /// Distância horizontal total percorrida pelo avião durante o voo em metros.
    /// </summary>
    public float DistanciaMetros { get; }

    /// <summary>
    /// Altitude máxima atingida pela aeronave em metros durante a sessão.
    /// </summary>
    public float AltitudeMaximaMetros { get; }

    /// <summary>
    /// Quantidade de moedas físicas coletadas pelo avião no ar.
    /// </summary>
    public int MoedasColetadas { get; }

    /// <summary>
    /// Total de moedas concedidas ao jogador, incluindo o bônus de voo e as moedas coletadas.
    /// </summary>
    public Moeda MoedasRecompensaTotal { get; }

    /// <summary>
    /// Construtor do objeto de valor ResultadoVoo com validação de invariantes.
    /// </summary>
    /// <param name="distanciaMetros">Distância horizontal alcançada (não negativa).</param>
    /// <param name="altitudeMaximaMetros">Altitude máxima alcançada (não negativa).</param>
    /// <param name="moedasColetadas">Moedas coletadas no ar (não negativa).</param>
    /// <param name="moedasRecompensaTotal">Total final de moedas calculado.</param>
    /// <exception cref="DominioInvalidoException">Lançada caso alguma métrica seja negativa.</exception>
    public ResultadoVoo(float distanciaMetros, float altitudeMaximaMetros, int moedasColetadas, Moeda moedasRecompensaTotal)
    {
        if (distanciaMetros < 0f)
        {
            throw new DominioInvalidoException(nameof(DistanciaMetros), $"A distância percorrida não pode ser negativa. Valor: {distanciaMetros}.");
        }

        if (altitudeMaximaMetros < 0f)
        {
            throw new DominioInvalidoException(nameof(AltitudeMaximaMetros), $"A altitude máxima não pode ser negativa. Valor: {altitudeMaximaMetros}.");
        }

        if (moedasColetadas < 0)
        {
            throw new DominioInvalidoException(nameof(MoedasColetadas), $"As moedas coletadas não podem ser negativas. Valor: {moedasColetadas}.");
        }

        DistanciaMetros = distanciaMetros;
        AltitudeMaximaMetros = altitudeMaximaMetros;
        MoedasColetadas = moedasColetadas;
        MoedasRecompensaTotal = moedasRecompensaTotal;
    }

    /// <summary>
    /// Calcula a premiação de um voo aplicando a fórmula canônica do PRD:
    /// floor(Distancia * 0.1) + floor(AltitudeMaxima * 0.05) + MoedasColetadas.
    /// </summary>
    /// <param name="distanciaMetros">Distância percorrida em metros.</param>
    /// <param name="altitudeMaximaMetros">Altitude máxima atingida em metros.</param>
    /// <param name="moedasColetadas">Moedas coletadas em voo.</param>
    /// <returns>Nova instância de ResultadoVoo com a recompensa calculada.</returns>
    public static ResultadoVoo Calcular(float distanciaMetros, float altitudeMaximaMetros, int moedasColetadas)
    {
        if (distanciaMetros < 0f)
        {
            throw new DominioInvalidoException(nameof(distanciaMetros), $"A distância não pode ser negativa: {distanciaMetros}.");
        }

        if (altitudeMaximaMetros < 0f)
        {
            throw new DominioInvalidoException(nameof(altitudeMaximaMetros), $"A altitude não pode ser negativa: {altitudeMaximaMetros}.");
        }

        if (moedasColetadas < 0)
        {
            throw new DominioInvalidoException(nameof(moedasColetadas), $"As moedas coletadas não podem ser negativas: {moedasColetadas}.");
        }

        long bonusDistancia = (long)MathF.Floor(distanciaMetros * 0.1f);
        long bonusAltitude = (long)MathF.Floor(altitudeMaximaMetros * 0.05f);
        long totalMoedas = bonusDistancia + bonusAltitude + moedasColetadas;

        return new ResultadoVoo(distanciaMetros, altitudeMaximaMetros, moedasColetadas, new Moeda(totalMoedas));
    }
}
