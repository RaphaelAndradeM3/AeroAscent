namespace AeroAscent.Core.Dominio.ObjetosDeValor;

using AeroAscent.Core.Dominio.Enums;
using AeroAscent.Core.Dominio.Excecoes;

/// <summary>
/// Objeto de valor imutável que especifica uma melhoria mecânica, seu nível e parâmetros de custo e eficácia.
/// </summary>
public record Melhoria
{
    /// <summary>
    /// Limite máximo fixo de nível para qualquer melhoria mecânica no jogo.
    /// </summary>
    public const int NIVEL_MAXIMO = 10;

    /// <summary>
    /// Categoria do componente mecânico correspondente.
    /// </summary>
    public TipoMelhoria Tipo { get; }

    /// <summary>
    /// Nível atual da melhoria (1 a 10).
    /// </summary>
    public int Nivel { get; }

    /// <summary>
    /// Custo monetário base de compra no nível 1.
    /// </summary>
    public Moeda CustoBase { get; }

    /// <summary>
    /// Fator multiplicador de eficácia aplicado sobre o componente no voo.
    /// </summary>
    public float MultiplicadorEficacia { get; }

    /// <summary>
    /// Construtor completo do objeto de valor Melhoria com validação de invariantes.
    /// </summary>
    /// <param name="tipo">Tipo da melhoria mecânica.</param>
    /// <param name="nivel">Nível da melhoria (1 a 10).</param>
    /// <param name="custoBase">Custo base da peça.</param>
    /// <param name="multiplicadorEficacia">Multiplicador de eficácia mecânica.</param>
    /// <exception cref="DominioInvalidoException">Lançada caso os parâmetros violem as regras de domínio.</exception>
    public Melhoria(TipoMelhoria tipo, int nivel, Moeda custoBase, float multiplicadorEficacia)
    {
        if (nivel < 1 || nivel > NIVEL_MAXIMO)
        {
            throw new DominioInvalidoException(nameof(Nivel), $"O nível da melhoria deve estar entre 1 e {NIVEL_MAXIMO}. Nível informado: {nivel}.");
        }

        Tipo = tipo;
        Nivel = nivel;
        CustoBase = custoBase;
        MultiplicadorEficacia = multiplicadorEficacia;
    }

    /// <summary>
    /// Calcula o custo em moedas necessário para evoluir o componente do nível atual para o próximo nível
    /// utilizando a fórmula exponencial canônica do PRD: CustoBase * 1.5^(Nivel - 1).
    /// </summary>
    /// <returns>Objeto de valor Moeda com o custo calculado.</returns>
    /// <exception cref="MelhoriaNivelMaximoException">Lançada se a melhoria já estiver no nível 10.</exception>
    public Moeda CalcularCustoProximoNivel()
    {
        if (Nivel >= NIVEL_MAXIMO)
        {
            throw new MelhoriaNivelMaximoException(Tipo, Nivel);
        }

        double expoente = Nivel - 1;
        double fatorExponencial = Math.Pow(1.5, expoente);
        long custoFinal = (long)Math.Floor(CustoBase.Quantidade * fatorExponencial);

        return new Moeda(custoFinal);
    }
}
