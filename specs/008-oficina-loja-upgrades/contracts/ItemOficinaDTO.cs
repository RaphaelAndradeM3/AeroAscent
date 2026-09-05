namespace AeroAscent.Core.Aplicacao.DTOs;

using AeroAscent.Core.Dominio.Enums;
using AeroAscent.Core.Dominio.ObjetosDeValor;

/// <summary>
/// Objeto de transferência de dados (<c>readonly record struct</c>) que projeta o estado de um componente
/// da oficina para exibição na interface de usuário, incluindo custos, níveis e permissão de compra.
/// </summary>
public readonly record struct ItemOficinaDTO
{
    /// <summary>
    /// Identificador do tipo de melhoria mecânica.
    /// </summary>
    public TipoMelhoria Tipo { get; }

    /// <summary>
    /// Nome amigável para exibição nos menus e interfaces em Português Brasileiro.
    /// </summary>
    public string NomeAmigavel { get; }

    /// <summary>
    /// Nível atual em que o componente se encontra (1 a 10).
    /// </summary>
    public int NivelAtual { get; }

    /// <summary>
    /// Custo em moedas para o próximo nível, ou <c>null</c> se o componente já atingiu o nível máximo 10.
    /// </summary>
    public Moeda? CustoProximoNivel { get; }

    /// <summary>
    /// Indica se o jogador possui saldo suficiente e se o item ainda não atingiu o nível máximo.
    /// </summary>
    public bool PodeComprar { get; }

    /// <summary>
    /// Indica se o componente já atingiu o teto máximo permitido (nível 10).
    /// </summary>
    public bool EstaNoNivelMaximo { get; }

    /// <summary>
    /// Construtor completo do DTO de item da oficina.
    /// </summary>
    public ItemOficinaDTO(
        TipoMelhoria tipo,
        string nomeAmigavel,
        int nivelAtual,
        Moeda? custoProximoNivel,
        bool podeComprar,
        bool estaNoNivelMaximo)
    {
        Tipo = tipo;
        NomeAmigavel = nomeAmigavel;
        NivelAtual = nivelAtual;
        CustoProximoNivel = custoProximoNivel;
        PodeComprar = podeComprar;
        EstaNoNivelMaximo = estaNoNivelMaximo;
    }
}
