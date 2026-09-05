namespace AeroAscent.Core.Dominio.ObjetosDeValor;

/// <summary>
/// Representa a moeda da economia do jogo como objeto de valor imutável.
/// </summary>
public readonly record struct Moeda(long Quantidade)
{
    /// <summary>
    /// Saldo zerado de moedas.
    /// </summary>
    public static readonly Moeda Zero = new(0);
}
