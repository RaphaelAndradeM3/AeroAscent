namespace AeroAscent.Core.Dominio.Entidades;

using AeroAscent.Core.Dominio.Enums;
using AeroAscent.Core.Dominio.Excecoes;
using AeroAscent.Core.Dominio.ObjetosDeValor;

/// <summary>
/// Entidade responsável por gerenciar as operações de hangar, catálogo de melhorias mecânicas e evolução da aeronave.
/// </summary>
public class Oficina
{
    /// <summary>
    /// Custo monetário base da melhoria de motor no nível 1.
    /// </summary>
    public const long CUSTO_BASE_MOTOR = 50;

    /// <summary>
    /// Custo monetário base da melhoria de aerodinâmica no nível 1.
    /// </summary>
    public const long CUSTO_BASE_AERODINAMICA = 40;

    /// <summary>
    /// Custo monetário base da melhoria de tanque de combustível no nível 1.
    /// </summary>
    public const long CUSTO_BASE_TANQUE = 30;

    /// <summary>
    /// Custo monetário base da melhoria de catapulta no nível 1.
    /// </summary>
    public const long CUSTO_BASE_CATAPULTA = 60;

    /// <summary>
    /// Identificador único da oficina.
    /// </summary>
    public Guid Id { get; }

    private readonly Dictionary<TipoMelhoria, Moeda> _custosBase;

    /// <summary>
    /// Construtor da oficina com identificador e catálogo customizável.
    /// </summary>
    /// <param name="id">Identificador único da oficina.</param>
    /// <param name="custosBase">Tabela de custos base por tipo de melhoria.</param>
    public Oficina(Guid id, Dictionary<TipoMelhoria, Moeda> custosBase)
    {
        if (id == Guid.Empty)
        {
            throw new DominioInvalidoException(nameof(Id), "O identificador da oficina não pode ser vazio.");
        }

        Id = id;
        _custosBase = new Dictionary<TipoMelhoria, Moeda>(custosBase);
    }

    /// <summary>
    /// Cria uma nova oficina configurada com a tabela de custos base canônica do PRD.
    /// </summary>
    /// <returns>Instância padrão de Oficina.</returns>
    public static Oficina CriarPadrao()
    {
        var custos = new Dictionary<TipoMelhoria, Moeda>
        {
            { TipoMelhoria.Motor, new Moeda(CUSTO_BASE_MOTOR) },
            { TipoMelhoria.Aerodinamica, new Moeda(CUSTO_BASE_AERODINAMICA) },
            { TipoMelhoria.TanqueCombustivel, new Moeda(CUSTO_BASE_TANQUE) },
            { TipoMelhoria.Catapulta, new Moeda(CUSTO_BASE_CATAPULTA) }
        };

        return new Oficina(Guid.NewGuid(), custos);
    }

    /// <summary>
    /// Obtém o catálogo de melhorias disponíveis na oficina para o nível 1.
    /// </summary>
    /// <returns>Lista com as 4 especificações de melhoria disponíveis.</returns>
    public IReadOnlyList<Melhoria> ObterCatalogo()
    {
        return new List<Melhoria>
        {
            new(TipoMelhoria.Motor, 1, ObterCustoBase(TipoMelhoria.Motor), 1.0f),
            new(TipoMelhoria.Aerodinamica, 1, ObterCustoBase(TipoMelhoria.Aerodinamica), 1.0f),
            new(TipoMelhoria.TanqueCombustivel, 1, ObterCustoBase(TipoMelhoria.TanqueCombustivel), 1.0f),
            new(TipoMelhoria.Catapulta, 1, ObterCustoBase(TipoMelhoria.Catapulta), 1.0f)
        };
    }

    /// <summary>
    /// Calcula o custo monetário em moedas para elevar um componente a partir de seu nível atual.
    /// </summary>
    /// <param name="tipo">Tipo da melhoria mecânica.</param>
    /// <param name="nivelAtual">Nível atual do componente (1 a 9).</param>
    /// <returns>Custo em Moeda calculado pela fórmula exponencial: CustoBase * 1.5^(nivelAtual - 1).</returns>
    /// <exception cref="MelhoriaNivelMaximoException">Lançada se o componente já estiver no nível 10 ou superior.</exception>
    /// <exception cref="DominioInvalidoException">Lançada se o nível for inferior a 1.</exception>
    public Moeda CalcularCustoMelhoria(TipoMelhoria tipo, int nivelAtual)
    {
        if (nivelAtual >= Aeronave.NIVEL_MAXIMO)
        {
            throw new MelhoriaNivelMaximoException(tipo, nivelAtual);
        }

        if (nivelAtual < Aeronave.NIVEL_MINIMO)
        {
            throw new DominioInvalidoException(nameof(nivelAtual), $"O nível atual não pode ser inferior a {Aeronave.NIVEL_MINIMO}.");
        }

        var custoBase = ObterCustoBase(tipo);
        var melhoriaAuxiliar = new Melhoria(tipo, nivelAtual, custoBase, 1.0f);
        return melhoriaAuxiliar.CalcularCustoProximoNivel();
    }

    /// <summary>
    /// Executa a evolução de um componente da aeronave debitando as moedas correspondentes do saldo.
    /// </summary>
    /// <param name="aeronave">Aeronave a ser modificada.</param>
    /// <param name="saldoAtual">Saldo atual de moedas do jogador.</param>
    /// <param name="tipo">Tipo da melhoria a ser comprada.</param>
    /// <returns>Novo saldo de moedas do jogador após o débito.</returns>
    /// <exception cref="MelhoriaNivelMaximoException">Lançada caso o componente já esteja no nível 10.</exception>
    /// <exception cref="SaldoInsuficienteException">Lançada caso o saldo seja menor que o custo da melhoria.</exception>
    public Moeda EvoluirComponente(Aeronave aeronave, Moeda saldoAtual, TipoMelhoria tipo)
    {
        int nivelAtual = aeronave.ObterNivel(tipo);

        if (nivelAtual >= Aeronave.NIVEL_MAXIMO)
        {
            throw new MelhoriaNivelMaximoException(tipo, nivelAtual);
        }

        var custo = CalcularCustoMelhoria(tipo, nivelAtual);

        if (saldoAtual.Quantidade < custo.Quantidade)
        {
            throw new SaldoInsuficienteException(saldoAtual.Quantidade, custo.Quantidade);
        }

        var novoSaldo = saldoAtual - custo;
        aeronave.AtualizarNivel(tipo, nivelAtual + 1);

        return novoSaldo;
    }

    private Moeda ObterCustoBase(TipoMelhoria tipo)
    {
        return _custosBase.TryGetValue(tipo, out var custo)
            ? custo
            : throw new DominioInvalidoException(nameof(tipo), $"Custo base não configurado para o tipo: {tipo}.");
    }
}
