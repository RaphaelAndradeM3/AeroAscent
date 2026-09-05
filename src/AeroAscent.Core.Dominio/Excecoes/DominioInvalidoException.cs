namespace AeroAscent.Core.Dominio.Excecoes;

/// <summary>
/// Exceção lançada quando uma invariante de domínio é violada ou dados de entrada são inconsistentes com as regras do jogo.
/// </summary>
public class DominioInvalidoException : Exception
{
    /// <summary>
    /// Nome da propriedade ou regra que violou a integridade do domínio.
    /// </summary>
    public string? NomeCampo { get; }

    /// <summary>
    /// Inicializa uma nova instância da exceção informando a mensagem descritiva da violação.
    /// </summary>
    /// <param name="mensagem">Mensagem explicativa da invariante violada.</param>
    public DominioInvalidoException(string mensagem)
        : base(mensagem)
    {
    }

    /// <summary>
    /// Inicializa uma nova instância da exceção informando o campo e a mensagem descritiva da violação.
    /// </summary>
    /// <param name="nomeCampo">Nome do parâmetro ou entidade inválida.</param>
    /// <param name="mensagem">Mensagem explicativa da invariante violada.</param>
    public DominioInvalidoException(string nomeCampo, string mensagem)
        : base($"Violação de domínio no campo '{nomeCampo}': {mensagem}")
    {
        NomeCampo = nomeCampo;
    }

    /// <summary>
    /// Inicializa uma nova instância com mensagem e exceção causadora interna.
    /// </summary>
    /// <param name="mensagem">Mensagem explicativa da invariante violada.</param>
    /// <param name="excecaoInterna">Exceção interna causadora.</param>
    public DominioInvalidoException(string mensagem, Exception excecaoInterna)
        : base(mensagem, excecaoInterna)
    {
    }
}
