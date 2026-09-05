namespace AeroAscent.Core.Dominio.Excecoes;

using AeroAscent.Core.Dominio.Enums;

/// <summary>
/// Exceção lançada quando ocorre uma tentativa de evoluir um componente da aeronave além do teto máximo permitido (nível 10).
/// </summary>
public class MelhoriaNivelMaximoException : Exception
{
    /// <summary>
    /// Limite máximo fixo de nível para as melhorias mecânicas no domínio.
    /// </summary>
    public const int NIVEL_MAXIMO_PERMITIDO = 10;

    /// <summary>
    /// O tipo de melhoria mecânica envolvido na operação.
    /// </summary>
    public TipoMelhoria Tipo { get; }

    /// <summary>
    /// O nível atual que já se encontrava no limite máximo.
    /// </summary>
    public int NivelAtual { get; }

    /// <summary>
    /// Inicializa uma nova instância da exceção com o tipo de melhoria e nível atual.
    /// </summary>
    /// <param name="tipo">Tipo da melhoria mecânica.</param>
    /// <param name="nivelAtual">Nível atual do componente.</param>
    public MelhoriaNivelMaximoException(TipoMelhoria tipo, int nivelAtual)
        : base($"A melhoria do tipo '{tipo}' já atingiu o nível máximo permitido ({NIVEL_MAXIMO_PERMITIDO}). Nível atual: {nivelAtual}.")
    {
        Tipo = tipo;
        NivelAtual = nivelAtual;
    }

    /// <summary>
    /// Inicializa uma nova instância com mensagem personalizada.
    /// </summary>
    /// <param name="mensagem">Mensagem explicativa do erro.</param>
    public MelhoriaNivelMaximoException(string mensagem)
        : base(mensagem)
    {
    }

    /// <summary>
    /// Inicializa uma nova instância com mensagem e causa interna.
    /// </summary>
    /// <param name="mensagem">Mensagem explicativa do erro.</param>
    /// <param name="excecaoInterna">Exceção causadora.</param>
    public MelhoriaNivelMaximoException(string mensagem, Exception excecaoInterna)
        : base(mensagem, excecaoInterna)
    {
    }
}
