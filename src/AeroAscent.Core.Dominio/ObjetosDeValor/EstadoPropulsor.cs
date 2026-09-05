namespace AeroAscent.Core.Dominio.ObjetosDeValor;

using System;
using AeroAscent.Core.Dominio.Excecoes;

/// <summary>
/// Representa a telemetria e o estado instantâneo do sistema de propulsão a cada ciclo de simulação física.
/// Modelado como readonly record struct para garantir alocação zero no heap (GC Alloc = 0 bytes).
/// </summary>
public readonly record struct EstadoPropulsor
{
    /// <summary>
    /// Indica se o propulsor está ativamente gerando empuxo positivo neste instante.
    /// </summary>
    public bool EstaAtivo { get; }

    /// <summary>
    /// Magnitude instantânea da força de empuxo aplicada pelo motor em Newtons (N).
    /// </summary>
    public float EmpuxoNewtons { get; }

    /// <summary>
    /// Quantidade de combustível restante no reservatório em unidades físicas.
    /// </summary>
    public float CombustivelRestante { get; }

    /// <summary>
    /// Fração normalizada de combustível restante no tanque (entre 0.0 e 1.0).
    /// </summary>
    public float PercentualRestante { get; }

    /// <summary>
    /// Taxa de consumo de combustível por segundo de queima ativa (unidades/s).
    /// </summary>
    public float TaxaConsumoPorSegundo { get; }

    /// <summary>
    /// Construtor completo com validação de invariantes e limites físicos.
    /// </summary>
    /// <param name="estaAtivo">Indica se o motor está ligado.</param>
    /// <param name="empuxoNewtons">Força de empuxo em Newtons.</param>
    /// <param name="combustivelRestante">Quantidade de combustível restante.</param>
    /// <param name="percentualRestante">Percentual restante entre 0.0 e 1.0.</param>
    /// <param name="taxaConsumoPorSegundo">Taxa de consumo por segundo.</param>
    /// <exception cref="DominioInvalidoException">Lançada caso os valores violem limites físicos.</exception>
    public EstadoPropulsor(
        bool estaAtivo,
        float empuxoNewtons,
        float combustivelRestante,
        float percentualRestante,
        float taxaConsumoPorSegundo)
    {
        if (empuxoNewtons < 0f)
        {
            throw new DominioInvalidoException(nameof(empuxoNewtons), $"O empuxo não pode ser negativo. Valor informado: {empuxoNewtons}.");
        }

        if (combustivelRestante < 0f)
        {
            throw new DominioInvalidoException(nameof(combustivelRestante), $"O combustível restante não pode ser negativo. Valor informado: {combustivelRestante}.");
        }

        if (taxaConsumoPorSegundo < 0f)
        {
            throw new DominioInvalidoException(nameof(taxaConsumoPorSegundo), $"A taxa de consumo não pode ser negativa. Valor informado: {taxaConsumoPorSegundo}.");
        }

        var percentualClamped = Math.Max(0f, Math.Min(1f, percentualRestante));

        // Se o combustível estiver zerado, o propulsor é rigidamente inativo e o empuxo é nulo
        if (combustivelRestante <= 0f)
        {
            EstaAtivo = false;
            EmpuxoNewtons = 0f;
        }
        else
        {
            EstaAtivo = estaAtivo;
            EmpuxoNewtons = estaAtivo ? empuxoNewtons : 0f;
        }

        CombustivelRestante = combustivelRestante;
        PercentualRestante = percentualClamped;
        TaxaConsumoPorSegundo = taxaConsumoPorSegundo;
    }

    /// <summary>
    /// Cria uma instância inativa do propulsor (sem empuxo gerado).
    /// </summary>
    /// <param name="combustivelRestante">Quantidade de combustível restante.</param>
    /// <param name="capacidadeMaxima">Capacidade máxima do reservatório.</param>
    /// <param name="taxaConsumo">Taxa de consumo por segundo.</param>
    /// <returns>Instância de EstadoPropulsor inativa.</returns>
    public static EstadoPropulsor CriarInativo(float combustivelRestante, float capacidadeMaxima, float taxaConsumo)
    {
        var percentual = capacidadeMaxima > 0f ? Math.Max(0f, Math.Min(1f, combustivelRestante / capacidadeMaxima)) : 0f;
        return new EstadoPropulsor(false, 0f, combustivelRestante, percentual, taxaConsumo);
    }

    /// <summary>
    /// Cria uma instância ativa do propulsor com empuxo aplicado.
    /// </summary>
    /// <param name="empuxoNewtons">Força de empuxo em Newtons.</param>
    /// <param name="combustivelRestante">Quantidade de combustível restante.</param>
    /// <param name="capacidadeMaxima">Capacidade máxima do reservatório.</param>
    /// <param name="taxaConsumo">Taxa de consumo por segundo.</param>
    /// <returns>Instância de EstadoPropulsor ativa.</returns>
    public static EstadoPropulsor CriarAtivo(float empuxoNewtons, float combustivelRestante, float capacidadeMaxima, float taxaConsumo)
    {
        var percentual = capacidadeMaxima > 0f ? Math.Max(0f, Math.Min(1f, combustivelRestante / capacidadeMaxima)) : 0f;
        return new EstadoPropulsor(true, empuxoNewtons, combustivelRestante, percentual, taxaConsumo);
    }
}
