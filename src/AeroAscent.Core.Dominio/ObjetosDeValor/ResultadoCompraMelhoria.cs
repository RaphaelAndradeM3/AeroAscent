namespace AeroAscent.Core.Dominio.ObjetosDeValor;

using AeroAscent.Core.Dominio.Enums;
using AeroAscent.Core.Dominio.Excecoes;

/// <summary>
/// Objeto de valor alocado na stack (<c>readonly record struct</c>, <c>GC Alloc = 0 bytes</c>)
/// que detalha o resultado consolidado da transação de compra de uma melhoria mecânica da aeronave.
/// </summary>
public readonly record struct ResultadoCompraMelhoria
{
    /// <summary>
    /// Componente mecânico da aeronave que foi evoluído.
    /// </summary>
    public TipoMelhoria Tipo { get; }

    /// <summary>
    /// Nível em que o componente se encontrava antes da realização da compra.
    /// </summary>
    public int NivelAnterior { get; }

    /// <summary>
    /// Novo nível alcançado pelo componente após a evolução (NivelAnterior + 1).
    /// </summary>
    public int NovoNivel { get; }

    /// <summary>
    /// Quantidade de moedas debitada da carteira do jogador pelo upgrade.
    /// </summary>
    public Moeda CustoPago { get; }

    /// <summary>
    /// Saldo restante atualizado de moedas do jogador após o pagamento.
    /// </summary>
    public Moeda SaldoRestante { get; }

    /// <summary>
    /// Indica se o componente atingiu o teto máximo permitido (nível 10).
    /// </summary>
    public bool AtingiuNivelMaximo { get; }

    /// <summary>
    /// Custo monetário para a evolução seguinte, ou <c>null</c> se o componente atingiu o teto máximo.
    /// </summary>
    public Moeda? ProximoCusto { get; }

    /// <summary>
    /// Construtor completo do resultado de compra de melhoria.
    /// </summary>
    /// <param name="tipo">Tipo do componente mecânico evoluído.</param>
    /// <param name="nivelAnterior">Nível anterior à compra.</param>
    /// <param name="novoNivel">Novo nível resultante da compra.</param>
    /// <param name="custoPago">Custo debitado pela transação.</param>
    /// <param name="saldoRestante">Saldo restante na carteira do jogador.</param>
    /// <param name="atingiuNivelMaximo">Verdadeiro se alcançou o nível 10.</param>
    /// <param name="proximoCusto">Custo para a próxima melhoria ou null se no teto.</param>
    /// <exception cref="DominioInvalidoException">Lançada caso os níveis sejam inválidos.</exception>
    public ResultadoCompraMelhoria(
        TipoMelhoria tipo,
        int nivelAnterior,
        int novoNivel,
        Moeda custoPago,
        Moeda saldoRestante,
        bool atingiuNivelMaximo,
        Moeda? proximoCusto)
    {
        if (nivelAnterior < 1)
        {
            throw new DominioInvalidoException(nameof(nivelAnterior), "O nível anterior não pode ser menor que 1.");
        }

        if (novoNivel <= nivelAnterior)
        {
            throw new DominioInvalidoException(nameof(novoNivel), "O novo nível deve ser estritamente superior ao nível anterior.");
        }

        Tipo = tipo;
        NivelAnterior = nivelAnterior;
        NovoNivel = novoNivel;
        CustoPago = custoPago;
        SaldoRestante = saldoRestante;
        AtingiuNivelMaximo = atingiuNivelMaximo;
        ProximoCusto = proximoCusto;
    }
}
