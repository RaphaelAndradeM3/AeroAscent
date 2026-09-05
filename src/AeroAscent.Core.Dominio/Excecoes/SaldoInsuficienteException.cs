namespace AeroAscent.Core.Dominio.Excecoes;

/// <summary>
/// Exceção lançada quando uma operação de compra ou débito requer uma quantia de moedas superior ao saldo disponível.
/// </summary>
public class SaldoInsuficienteException : Exception
{
    /// <summary>
    /// Saldo atual disponível no momento da tentativa de débito.
    /// </summary>
    public long SaldoAtual { get; }

    /// <summary>
    /// Quantidade de moedas que tentou ser debitada.
    /// </summary>
    public long QuantiaNecessaria { get; }

    /// <summary>
    /// Inicializa uma nova instância da exceção com os dados do saldo insuficiente.
    /// </summary>
    /// <param name="saldoAtual">Saldo atual de moedas do jogador.</param>
    /// <param name="quantiaNecessaria">Quantia necessária que excedeu o saldo.</param>
    public SaldoInsuficienteException(long saldoAtual, long quantiaNecessaria)
        : base($"Saldo insuficiente de moedas. Saldo atual: {saldoAtual}, Quantia necessária: {quantiaNecessaria}.")
    {
        SaldoAtual = saldoAtual;
        QuantiaNecessaria = quantiaNecessaria;
    }

    /// <summary>
    /// Inicializa uma nova instância com mensagem personalizada.
    /// </summary>
    /// <param name="mensagem">Mensagem explicativa do erro.</param>
    public SaldoInsuficienteException(string mensagem)
        : base(mensagem)
    {
    }

    /// <summary>
    /// Inicializa uma nova instância com mensagem e causa interna.
    /// </summary>
    /// <param name="mensagem">Mensagem explicativa do erro.</param>
    /// <param name="excecaoInterna">Exceção causadora.</param>
    public SaldoInsuficienteException(string mensagem, Exception excecaoInterna)
        : base(mensagem, excecaoInterna)
    {
    }
}
