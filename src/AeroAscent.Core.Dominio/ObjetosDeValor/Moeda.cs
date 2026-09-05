namespace AeroAscent.Core.Dominio.ObjetosDeValor;

using AeroAscent.Core.Dominio.Excecoes;

/// <summary>
/// Representa uma quantia monetária na economia do jogo como objeto de valor imutável protegido contra saldos negativos e overflows.
/// </summary>
public readonly record struct Moeda : IComparable<Moeda>
{
    /// <summary>
    /// Saldo zerado de moedas.
    /// </summary>
    public static readonly Moeda Zero = new(0);

    /// <summary>
    /// Quantidade numérica de moedas representadas (sempre maior ou igual a zero).
    /// </summary>
    public long Quantidade { get; }

    /// <summary>
    /// Construtor do objeto de valor Moeda com validação de saldo não negativo.
    /// </summary>
    /// <param name="quantidade">Quantidade de moedas.</param>
    /// <exception cref="DominioInvalidoException">Lançada se a quantidade for negativa.</exception>
    public Moeda(long quantidade)
    {
        if (quantidade < 0)
        {
            throw new DominioInvalidoException(nameof(Quantidade), $"A quantidade de moedas não pode ser negativa. Valor informado: {quantidade}.");
        }

        Quantidade = quantidade;
    }

    /// <summary>
    /// Soma uma quantia de moedas com verificação segura de overflow aritmético.
    /// </summary>
    /// <param name="outra">Moeda a ser adicionada.</param>
    /// <returns>Nova instância com a soma resultante.</returns>
    public Moeda Adicionar(Moeda outra)
    {
        checked
        {
            return new Moeda(Quantidade + outra.Quantidade);
        }
    }

    /// <summary>
    /// Subtrai uma quantia de moedas com validação estrita de saldo suficiente.
    /// </summary>
    /// <param name="outra">Moeda a ser deduzida.</param>
    /// <returns>Nova instância com o saldo restante.</returns>
    /// <exception cref="SaldoInsuficienteException">Lançada quando a quantia a subtrair for maior que o saldo atual.</exception>
    public Moeda Subtrair(Moeda outra)
    {
        if (outra.Quantidade > Quantidade)
        {
            throw new SaldoInsuficienteException(Quantidade, outra.Quantidade);
        }

        return new Moeda(Quantidade - outra.Quantidade);
    }

    /// <summary>
    /// Operador de soma entre duas instâncias de Moeda.
    /// </summary>
    public static Moeda operator +(Moeda a, Moeda b) => a.Adicionar(b);

    /// <summary>
    /// Operador de subtração entre duas instâncias de Moeda.
    /// </summary>
    public static Moeda operator -(Moeda a, Moeda b) => a.Subtrair(b);

    /// <summary>
    /// Operador de comparação menor que.
    /// </summary>
    public static bool operator <(Moeda a, Moeda b) => a.Quantidade < b.Quantidade;

    /// <summary>
    /// Operador de comparação maior que.
    /// </summary>
    public static bool operator >(Moeda a, Moeda b) => a.Quantidade > b.Quantidade;

    /// <summary>
    /// Operador de comparação menor ou igual a.
    /// </summary>
    public static bool operator <=(Moeda a, Moeda b) => a.Quantidade <= b.Quantidade;

    /// <summary>
    /// Operador de comparação maior ou igual a.
    /// </summary>
    public static bool operator >=(Moeda a, Moeda b) => a.Quantidade >= b.Quantidade;

    /// <inheritdoc />
    public int CompareTo(Moeda other) => Quantidade.CompareTo(other.Quantidade);

    /// <inheritdoc />
    public override string ToString() => $"{Quantidade} Moedas";
}
