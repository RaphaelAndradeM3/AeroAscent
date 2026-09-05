namespace AeroAscent.Core.Dominio.ObjetosDeValor;

/// <summary>
/// Encapsula os efeitos imediatos e métricas resultantes da interação com coletáveis em um passo de tempo,
/// alocado estritamente na stack como readonly record struct (GC Alloc = 0 bytes).
/// </summary>
public readonly record struct ResultadoProcessamentoColetaveis
{
    /// <summary>
    /// Quantidade de moedas coletadas neste ciclo de simulação.
    /// </summary>
    public int MoedasColetadasNoPasso { get; }

    /// <summary>
    /// Indica se a aeronave atravessou um anel de vento recebendo impulso positivo de velocidade.
    /// </summary>
    public bool RecebeuImpulsoVento { get; }

    /// <summary>
    /// Vetor de impulso instantâneo conferido à aeronave em metros por segundo.
    /// </summary>
    public VetorVoo ImpulsoAplicado { get; }

    /// <summary>
    /// Estado físico atualizado da aeronave após a aplicação de eventuais impulsos de vento.
    /// </summary>
    public EstadoFisicoAeronave EstadoFisicoAtualizado { get; }

    /// <summary>
    /// Construtor estruturado do resultado de processamento de coletáveis.
    /// </summary>
    /// <param name="moedasColetadasNoPasso">Quantidade de moedas capturadas.</param>
    /// <param name="recebeuImpulsoVento">Indica se houve impulso de vento.</param>
    /// <param name="impulsoAplicado">Vetor de impulso instantâneo adicionado.</param>
    /// <param name="estadoFisicoAtualizado">Novo estado cinemático da aeronave.</param>
    public ResultadoProcessamentoColetaveis(
        int moedasColetadasNoPasso,
        bool recebeuImpulsoVento,
        VetorVoo impulsoAplicado,
        EstadoFisicoAeronave estadoFisicoAtualizado)
    {
        MoedasColetadasNoPasso = moedasColetadasNoPasso;
        RecebeuImpulsoVento = recebeuImpulsoVento;
        ImpulsoAplicado = impulsoAplicado;
        EstadoFisicoAtualizado = estadoFisicoAtualizado;
    }

    /// <summary>
    /// Cria uma instância neutra indicando que nenhum coletável foi capturado neste passo.
    /// </summary>
    /// <param name="estadoAtual">Estado físico mantido da aeronave.</param>
    /// <returns>ResultadoProcessamentoColetaveis sem alterações na aeronave.</returns>
    public static ResultadoProcessamentoColetaveis CriarNeutro(EstadoFisicoAeronave estadoAtual)
    {
        return new ResultadoProcessamentoColetaveis(0, false, VetorVoo.Zero, estadoAtual);
    }
}
