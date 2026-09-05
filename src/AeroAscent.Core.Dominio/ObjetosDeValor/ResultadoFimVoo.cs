namespace AeroAscent.Core.Dominio.ObjetosDeValor;

using AeroAscent.Core.Dominio.Enums;
using AeroAscent.Core.Dominio.Excecoes;

/// <summary>
/// Encapsula o resultado e as métricas consolidadas ao término de um voo ou veredito de pouso,
/// alocado exclusivamente na stack como readonly record struct (GC Alloc = 0 bytes).
/// </summary>
public readonly record struct ResultadoFimVoo
{
    /// <summary>
    /// Status atual da sessão de voo (EmVoo, Pousado, Cancelado).
    /// </summary>
    public StatusVoo Status { get; }

    /// <summary>
    /// Indica se a aeronave atingiu a parada total no solo (Vz = 0 e NoSolo = true).
    /// </summary>
    public bool AeronaveParou { get; }

    /// <summary>
    /// Distância final percorrida no eixo horizontal Z em metros.
    /// </summary>
    public float DistanciaFinalMetros { get; }

    /// <summary>
    /// Altitude máxima vertical atingida durante a sessão de voo em metros.
    /// </summary>
    public float AltitudeMaximaMetros { get; }

    /// <summary>
    /// Quantidade total de moedas coletadas na sessão.
    /// </summary>
    public int MoedasColetadas { get; }

    /// <summary>
    /// Resultado consolidado da sessão de voo com pontuação e premiação calculadas (nulo se o voo ainda estiver em andamento).
    /// </summary>
    public ResultadoVoo? Resultado { get; }

    /// <summary>
    /// Construtor estruturado do resultado de fim de voo.
    /// </summary>
    /// <param name="status">Status da sessão de voo.</param>
    /// <param name="aeronaveParou">Indica se a aeronave está em repouso no solo.</param>
    /// <param name="distanciaFinalMetros">Distância percorrida em metros.</param>
    /// <param name="altitudeMaximaMetros">Altitude máxima em metros.</param>
    /// <param name="moedasColetadas">Moedas coletadas no percurso.</param>
    /// <param name="resultado">Resultado formal do voo.</param>
    public ResultadoFimVoo(
        StatusVoo status,
        bool aeronaveParou,
        float distanciaFinalMetros,
        float altitudeMaximaMetros,
        int moedasColetadas,
        ResultadoVoo? resultado)
    {
        if (distanciaFinalMetros < 0f)
        {
            throw new DominioInvalidoException(
                nameof(distanciaFinalMetros),
                $"A distância final não pode ser negativa. Informado: {distanciaFinalMetros}.");
        }

        if (altitudeMaximaMetros < 0f)
        {
            throw new DominioInvalidoException(
                nameof(altitudeMaximaMetros),
                $"A altitude máxima não pode ser negativa. Informado: {altitudeMaximaMetros}.");
        }

        if (moedasColetadas < 0)
        {
            throw new DominioInvalidoException(
                nameof(moedasColetadas),
                $"A quantidade de moedas coletadas não pode ser negativa. Informado: {moedasColetadas}.");
        }

        Status = status;
        AeronaveParou = aeronaveParou;
        DistanciaFinalMetros = distanciaFinalMetros;
        AltitudeMaximaMetros = altitudeMaximaMetros;
        MoedasColetadas = moedasColetadas;
        Resultado = resultado;
    }

    /// <summary>
    /// Cria uma instância representando um voo ainda em andamento ou em deslizamento no solo.
    /// </summary>
    /// <param name="distancia">Distância percorrida até o momento.</param>
    /// <param name="altitudeMaxima">Altitude máxima atingida.</param>
    /// <param name="moedasColetadas">Moedas coletadas até o momento.</param>
    /// <returns>ResultadoFimVoo com Status EmVoo e AeronaveParou falso.</returns>
    public static ResultadoFimVoo CriarEmAndamento(float distancia, float altitudeMaxima, int moedasColetadas)
    {
        return new ResultadoFimVoo(StatusVoo.EmVoo, false, distancia, altitudeMaxima, moedasColetadas, null);
    }

    /// <summary>
    /// Cria uma instância representando um voo finalizado por pouso e parada total no solo.
    /// </summary>
    /// <param name="distancia">Distância final travada.</param>
    /// <param name="altitudeMaxima">Altitude máxima travada.</param>
    /// <param name="moedasColetadas">Moedas coletadas consolidadas.</param>
    /// <param name="resultado">ResultadoVoo calculado.</param>
    /// <returns>ResultadoFimVoo com Status Pousado e AeronaveParou verdadeiro.</returns>
    public static ResultadoFimVoo CriarPousado(
        float distancia,
        float altitudeMaxima,
        int moedasColetadas,
        ResultadoVoo resultado)
    {
        if (resultado == null)
        {
            throw new DominioInvalidoException(
                nameof(resultado),
                "O resultado de voo não pode ser nulo para um pouso finalizado.");
        }

        return new ResultadoFimVoo(StatusVoo.Pousado, true, distancia, altitudeMaxima, moedasColetadas, resultado);
    }
}
