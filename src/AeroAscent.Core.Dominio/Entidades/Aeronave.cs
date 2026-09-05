namespace AeroAscent.Core.Dominio.Entidades;

using AeroAscent.Core.Dominio.Enums;
using AeroAscent.Core.Dominio.Excecoes;

/// <summary>
/// Representa a aeronave do jogador, contendo suas configurações mecânicas e níveis de evolução.
/// </summary>
public class Aeronave
{
    /// <summary>
    /// Limite mínimo permitido para o nível de qualquer componente da aeronave.
    /// </summary>
    public const int NIVEL_MINIMO = 1;

    /// <summary>
    /// Limite máximo permitido para o nível de qualquer componente da aeronave.
    /// </summary>
    public const int NIVEL_MAXIMO = 10;

    /// <summary>
    /// Identificador único global da aeronave.
    /// </summary>
    public Guid Id { get; }

    /// <summary>
    /// Nível atual de potência e aceleração do motor (1 a 10).
    /// </summary>
    public int NivelMotor { get; private set; }

    /// <summary>
    /// Nível atual de coeficiente aerodinâmico e capacidade de planeio da fuselagem (1 a 10).
    /// </summary>
    public int NivelAerodinamica { get; private set; }

    /// <summary>
    /// Nível atual de capacidade e volume do tanque de combustível (1 a 10).
    /// </summary>
    public int NivelTanqueCombustivel { get; private set; }

    /// <summary>
    /// Nível atual de força de propulsão e ejeção inicial da catapulta (1 a 10).
    /// </summary>
    public int NivelCatapulta { get; private set; }

    /// <summary>
    /// Construtor completo da entidade Aeronave com validação estrita de invariantes.
    /// </summary>
    /// <param name="id">Identificador único da aeronave.</param>
    /// <param name="nivelMotor">Nível de motor (1 a 10).</param>
    /// <param name="nivelAerodinamica">Nível de aerodinâmica (1 a 10).</param>
    /// <param name="nivelTanqueCombustivel">Nível de tanque de combustível (1 a 10).</param>
    /// <param name="nivelCatapulta">Nível de catapulta (1 a 10).</param>
    /// <exception cref="DominioInvalidoException">Lançada se qualquer parâmetro violar as invariantes.</exception>
    public Aeronave(Guid id, int nivelMotor, int nivelAerodinamica, int nivelTanqueCombustivel, int nivelCatapulta)
    {
        if (id == Guid.Empty)
        {
            throw new DominioInvalidoException(nameof(Id), "O identificador da aeronave não pode ser vazio (Guid.Empty).");
        }

        ValidarNivel(nameof(NivelMotor), nivelMotor);
        ValidarNivel(nameof(NivelAerodinamica), nivelAerodinamica);
        ValidarNivel(nameof(NivelTanqueCombustivel), nivelTanqueCombustivel);
        ValidarNivel(nameof(NivelCatapulta), nivelCatapulta);

        Id = id;
        NivelMotor = nivelMotor;
        NivelAerodinamica = nivelAerodinamica;
        NivelTanqueCombustivel = nivelTanqueCombustivel;
        NivelCatapulta = nivelCatapulta;
    }

    /// <summary>
    /// Cria uma nova aeronave com níveis iniciais padrão iguais a 1 e novo Guid.
    /// </summary>
    /// <returns>Nova instância de Aeronave inicializada.</returns>
    public static Aeronave CriarPadrao()
    {
        return new Aeronave(Guid.NewGuid(), NIVEL_MINIMO, NIVEL_MINIMO, NIVEL_MINIMO, NIVEL_MINIMO);
    }

    /// <summary>
    /// Atualiza o nível de um componente específico da aeronave após validação de invariantes.
    /// </summary>
    /// <param name="tipo">Tipo de melhoria mecânica.</param>
    /// <param name="novoNivel">Novo nível inteiro (1 a 10).</param>
    /// <exception cref="DominioInvalidoException">Lançada se o novo nível estiver fora dos limites permitidos.</exception>
    public void AtualizarNivel(TipoMelhoria tipo, int novoNivel)
    {
        ValidarNivel(tipo.ToString(), novoNivel);

        switch (tipo)
        {
            case TipoMelhoria.Motor:
                NivelMotor = novoNivel;
                break;
            case TipoMelhoria.Aerodinamica:
                NivelAerodinamica = novoNivel;
                break;
            case TipoMelhoria.TanqueCombustivel:
                NivelTanqueCombustivel = novoNivel;
                break;
            case TipoMelhoria.Catapulta:
                NivelCatapulta = novoNivel;
                break;
            default:
                throw new DominioInvalidoException(nameof(tipo), $"Tipo de melhoria desconhecido: {tipo}.");
        }
    }

    /// <summary>
    /// Obtém o nível atual correspondente ao tipo de melhoria mecânica consultado.
    /// </summary>
    /// <param name="tipo">Tipo da melhoria.</param>
    /// <returns>O nível numérico inteiro do componente (1 a 10).</returns>
    public int ObterNivel(TipoMelhoria tipo)
    {
        return tipo switch
        {
            TipoMelhoria.Motor => NivelMotor,
            TipoMelhoria.Aerodinamica => NivelAerodinamica,
            TipoMelhoria.TanqueCombustivel => NivelTanqueCombustivel,
            TipoMelhoria.Catapulta => NivelCatapulta,
            _ => throw new DominioInvalidoException(nameof(tipo), $"Tipo de melhoria desconhecido: {tipo}.")
        };
    }

    private static void ValidarNivel(string nomeCampo, int nivel)
    {
        if (nivel < NIVEL_MINIMO || nivel > NIVEL_MAXIMO)
        {
            throw new DominioInvalidoException(nomeCampo, $"O nível deve estar compreendido no intervalo fixo entre {NIVEL_MINIMO} e {NIVEL_MAXIMO}. Valor informado: {nivel}.");
        }
    }
}
