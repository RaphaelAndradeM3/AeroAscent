namespace AeroAscent.Core.Dominio.Entidades;

/// <summary>
/// Raiz de agregação que consolida o estado global e persistível do jogador.
/// </summary>
public class ProgressoJogador
{
    /// <summary>
    /// Identificador único global do jogador.
    /// </summary>
    public Guid Id { get; }

    /// <summary>
    /// Construtor da raiz de agregação ProgressoJogador.
    /// </summary>
    /// <param name="id">Identificador único do jogador.</param>
    public ProgressoJogador(Guid id)
    {
        Id = id;
    }
}
