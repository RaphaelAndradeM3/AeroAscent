namespace AeroAscent.Core.Aplicacao.DTOs;

using AeroAscent.Core.Dominio.Enums;

/// <summary>
/// DTO imutável de apresentação (<c>readonly record struct</c>) contendo todos os dados visuais
/// pré-formatados para a renderização de um cartão de melhoria mecânica na interface da Oficina.
/// </summary>
public readonly record struct ItemCartaoOficinaDTO
{
    /// <summary>
    /// Tipo da melhoria mecânica correspondente ao cartão.
    /// </summary>
    public TipoMelhoria Tipo { get; init; }

    /// <summary>
    /// Nome amigável e localizado da melhoria mecânica (ex: "Motor", "Aerodinâmica").
    /// </summary>
    public string Titulo { get; init; }

    /// <summary>
    /// Nível atual do componente mecânico (1 a 10).
    /// </summary>
    public int NivelAtual { get; init; }

    /// <summary>
    /// Texto de exibição do nível (ex: "Nível 3" ou "Nível 10 (MAX)").
    /// </summary>
    public string TextoNivel { get; init; }

    /// <summary>
    /// Valor normalizado entre 0.0f e 1.0f para preenchimento da barra de progresso visual.
    /// </summary>
    public float ProgressoNormalizado { get; init; }

    /// <summary>
    /// Custo monetário em moedas para a próxima evolução, ou <c>null</c> caso já esteja no nível máximo.
    /// </summary>
    public long? CustoProximoNivel { get; init; }

    /// <summary>
    /// Texto exibido no interior do botão de compra (ex: "💰 150" ou "MÁXIMO").
    /// </summary>
    public string TextoBotao { get; init; }

    /// <summary>
    /// Indica se o botão de compra deve estar habilitado para interação.
    /// </summary>
    public bool PodeComprar { get; init; }

    /// <summary>
    /// Indica se o componente já atingiu o nível máximo permitido (nível 10).
    /// </summary>
    public bool EstaNoNivelMaximo { get; init; }
}
