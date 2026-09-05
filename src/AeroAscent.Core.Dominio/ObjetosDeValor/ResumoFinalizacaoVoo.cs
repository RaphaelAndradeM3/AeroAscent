namespace AeroAscent.Core.Dominio.ObjetosDeValor;

using AeroAscent.Core.Dominio.Excecoes;

/// <summary>
/// Objeto de valor alocado na stack (<c>readonly record struct</c>, <c>GC Alloc = 0 bytes</c>)
/// que consolida o extrato financeiro discriminado e o registro de novos recordes obtidos ao término de uma sessão de voo.
/// </summary>
public readonly record struct ResumoFinalizacaoVoo
{
    /// <summary>
    /// Distância horizontal total percorrida pela aeronave em metros.
    /// </summary>
    public float DistanciaMetros { get; }

    /// <summary>
    /// Maior altitude vertical atingida pela aeronave em metros.
    /// </summary>
    public float AltitudeMaximaMetros { get; }

    /// <summary>
    /// Quantidade de moedas ganhas exclusivamente pela distância percorrida (floor(Distancia * 0.1)).
    /// </summary>
    public long MoedasPorDistancia { get; }

    /// <summary>
    /// Quantidade de moedas ganhas exclusivamente pela altitude máxima atingida (floor(Altitude * 0.05)).
    /// </summary>
    public long MoedasPorAltitude { get; }

    /// <summary>
    /// Quantidade de moedas físicas coletadas no ar durante o voo.
    /// </summary>
    public int MoedasColetadas { get; }

    /// <summary>
    /// Total de moedas ganhas na sessão (soma de distância, altitude e coletáveis).
    /// </summary>
    public Moeda MoedasTotalGanhas { get; }

    /// <summary>
    /// Saldo acumulado atualizado na carteira do jogador após a finalização do voo.
    /// </summary>
    public Moeda SaldoTotalAtualizado { get; }

    /// <summary>
    /// Indica se este voo superou a marca histórica de distância horizontal anterior do jogador.
    /// </summary>
    public bool EhNovoRecordeDistancia { get; }

    /// <summary>
    /// Indica se este voo superou a marca histórica de altitude máxima vertical anterior do jogador.
    /// </summary>
    public bool EhNovoRecordeAltitude { get; }

    /// <summary>
    /// Construtor privado da struct de extrato.
    /// </summary>
    private ResumoFinalizacaoVoo(
        float distanciaMetros,
        float altitudeMaximaMetros,
        long moedasPorDistancia,
        long moedasPorAltitude,
        int moedasColetadas,
        Moeda moedasTotalGanhas,
        Moeda saldoTotalAtualizado,
        bool ehNovoRecordeDistancia,
        bool ehNovoRecordeAltitude)
    {
        DistanciaMetros = distanciaMetros;
        AltitudeMaximaMetros = altitudeMaximaMetros;
        MoedasPorDistancia = moedasPorDistancia;
        MoedasPorAltitude = moedasPorAltitude;
        MoedasColetadas = moedasColetadas;
        MoedasTotalGanhas = moedasTotalGanhas;
        SaldoTotalAtualizado = saldoTotalAtualizado;
        EhNovoRecordeDistancia = ehNovoRecordeDistancia;
        EhNovoRecordeAltitude = ehNovoRecordeAltitude;
    }

    /// <summary>
    /// Constrói uma nova instância de <see cref="ResumoFinalizacaoVoo"/> validando todas as métricas e regras de negócio.
    /// </summary>
    /// <param name="distanciaMetros">Distância horizontal alcançada em metros.</param>
    /// <param name="altitudeMaximaMetros">Altitude máxima alcançada em metros.</param>
    /// <param name="moedasPorDistancia">Moedas oriundas da distância.</param>
    /// <param name="moedasPorAltitude">Moedas oriundas da altitude.</param>
    /// <param name="moedasColetadas">Moedas coletadas no ar.</param>
    /// <param name="moedasTotalGanhas">Total de moedas ganhas nesta sessão.</param>
    /// <param name="saldoTotalAtualizado">Saldo do jogador após creditar os ganhos.</param>
    /// <param name="ehNovoRecordeDistancia">Sinalizador de novo recorde de distância.</param>
    /// <param name="ehNovoRecordeAltitude">Sinalizador de novo recorde de altitude.</param>
    /// <returns>Extrato discriminado imutável alocado na stack.</returns>
    /// <exception cref="DominioInvalidoException">Lançada caso algum valor seja negativo.</exception>
    public static ResumoFinalizacaoVoo Criar(
        float distanciaMetros,
        float altitudeMaximaMetros,
        long moedasPorDistancia,
        long moedasPorAltitude,
        int moedasColetadas,
        Moeda moedasTotalGanhas,
        Moeda saldoTotalAtualizado,
        bool ehNovoRecordeDistancia,
        bool ehNovoRecordeAltitude)
    {
        if (distanciaMetros < 0f)
        {
            throw new DominioInvalidoException(nameof(distanciaMetros), "A distância percorrida não pode ser negativa.");
        }

        if (altitudeMaximaMetros < 0f)
        {
            throw new DominioInvalidoException(nameof(altitudeMaximaMetros), "A altitude máxima não pode ser negativa.");
        }

        if (moedasPorDistancia < 0)
        {
            throw new DominioInvalidoException(nameof(moedasPorDistancia), "As moedas por distância não podem ser negativas.");
        }

        if (moedasPorAltitude < 0)
        {
            throw new DominioInvalidoException(nameof(moedasPorAltitude), "As moedas por altitude não podem ser negativas.");
        }

        if (moedasColetadas < 0)
        {
            throw new DominioInvalidoException(nameof(moedasColetadas), "As moedas coletadas não podem ser negativas.");
        }

        return new ResumoFinalizacaoVoo(
            distanciaMetros,
            altitudeMaximaMetros,
            moedasPorDistancia,
            moedasPorAltitude,
            moedasColetadas,
            moedasTotalGanhas,
            saldoTotalAtualizado,
            ehNovoRecordeDistancia,
            ehNovoRecordeAltitude);
    }

    /// <summary>
    /// Cria uma instância de extrato para um voo cancelado ou abortado, com 0 moedas ganhas e sem quebra de recordes.
    /// </summary>
    /// <param name="distanciaMetros">Distância alcançada até o cancelamento.</param>
    /// <param name="altitudeMaximaMetros">Altitude máxima alcançada até o cancelamento.</param>
    /// <param name="saldoAtual">Saldo corrente inalterado do jogador.</param>
    /// <returns>Extrato com recompensas zeradas.</returns>
    public static ResumoFinalizacaoVoo CriarCancelado(
        float distanciaMetros,
        float altitudeMaximaMetros,
        Moeda saldoAtual)
    {
        return Criar(
            distanciaMetros,
            altitudeMaximaMetros,
            moedasPorDistancia: 0,
            moedasPorAltitude: 0,
            moedasColetadas: 0,
            moedasTotalGanhas: Moeda.Zero,
            saldoTotalAtualizado: saldoAtual,
            ehNovoRecordeDistancia: false,
            ehNovoRecordeAltitude: false);
    }
}
