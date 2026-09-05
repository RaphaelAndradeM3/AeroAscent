namespace AeroAscent.Core.Dominio.ObjetosDeValor;

using System;
using AeroAscent.Core.Dominio.Excecoes;

/// <summary>
/// Representa a energia propelente e o reservatório de combustível da aeronave como objeto de valor imutável.
/// </summary>
public record Combustivel
{
    /// <summary>
    /// Quantidade atual de combustível em unidades físicas (litros/pontos).
    /// </summary>
    public float QuantidadeAtual { get; }

    /// <summary>
    /// Capacidade volumétrica máxima do tanque da aeronave.
    /// </summary>
    public float CapacidadeMaxima { get; }

    /// <summary>
    /// Taxa de queima de combustível por segundo durante a ativação da propulsão (boost).
    /// </summary>
    public float TaxaQueimaPorSegundo { get; }

    /// <summary>
    /// Construtor completo do objeto de valor Combustivel com validação de limites físicos.
    /// </summary>
    /// <param name="quantidadeAtual">Quantidade atual de combustível.</param>
    /// <param name="capacidadeMaxima">Capacidade total do tanque.</param>
    /// <param name="taxaQueimaPorSegundo">Taxa de consumo por segundo.</param>
    /// <exception cref="DominioInvalidoException">Lançada se os limites forem violados.</exception>
    public Combustivel(float quantidadeAtual, float capacidadeMaxima, float taxaQueimaPorSegundo)
    {
        if (capacidadeMaxima <= 0f)
        {
            throw new DominioInvalidoException(nameof(CapacidadeMaxima), $"A capacidade máxima do tanque deve ser maior que zero. Valor informado: {capacidadeMaxima}.");
        }

        if (quantidadeAtual < 0f || quantidadeAtual > capacidadeMaxima)
        {
            throw new DominioInvalidoException(nameof(QuantidadeAtual), $"A quantidade atual ({quantidadeAtual}) deve ser não negativa e não exceder a capacidade máxima ({capacidadeMaxima}).");
        }

        if (taxaQueimaPorSegundo < 0f)
        {
            throw new DominioInvalidoException(nameof(TaxaQueimaPorSegundo), $"A taxa de queima não pode ser negativa. Valor informado: {taxaQueimaPorSegundo}.");
        }

        QuantidadeAtual = quantidadeAtual;
        CapacidadeMaxima = capacidadeMaxima;
        TaxaQueimaPorSegundo = taxaQueimaPorSegundo;
    }

    /// <summary>
    /// Cria uma nova instância de combustível completamente abastecida.
    /// </summary>
    /// <param name="capacidadeMaxima">Capacidade total do tanque.</param>
    /// <param name="taxaQueimaPorSegundo">Taxa de consumo por segundo.</param>
    /// <returns>Combustível com QuantidadeAtual igual à CapacidadeMaxima.</returns>
    public static Combustivel CriarCheio(float capacidadeMaxima, float taxaQueimaPorSegundo)
    {
        return new Combustivel(capacidadeMaxima, capacidadeMaxima, taxaQueimaPorSegundo);
    }

    /// <summary>
    /// Indica se o reservatório de combustível está completamente esgotado.
    /// </summary>
    public bool EstaVazio => QuantidadeAtual <= 0f;

    /// <summary>
    /// Fração percentual de combustível restante entre 0.0 (vazio) e 1.0 (cheio).
    /// </summary>
    public float PercentualRestante => CapacidadeMaxima > 0f ? QuantidadeAtual / CapacidadeMaxima : 0f;

    /// <summary>
    /// Executa o consumo de combustível por um intervalo de tempo transcorrido.
    /// </summary>
    /// <param name="deltaTempoSegundos">Duração da queima em segundos.</param>
    /// <returns>Nova instância imutável com a quantidade restante calculada.</returns>
    public Combustivel Consumir(float deltaTempoSegundos)
    {
        return ConsumirFracionario(deltaTempoSegundos, out _);
    }

    /// <summary>
    /// Executa o consumo de combustível calculando precisamente o tempo efetivo de queima no intervalo informado.
    /// Se o combustível restante for insuficiente para o passo completo, queima apenas a fração residual disponível,
    /// zerando o tanque e retornando o tempo real em que o motor permaneceu ligado (SC-001).
    /// </summary>
    /// <param name="deltaTempoSegundos">Duração total do passo de simulação em segundos.</param>
    /// <param name="tempoEfetivoQueima">Tempo efetivo durante o qual houve queima de combustível no passo.</param>
    /// <returns>Nova instância imutável com a quantidade restante calculada.</returns>
    public Combustivel ConsumirFracionario(float deltaTempoSegundos, out float tempoEfetivoQueima)
    {
        if (deltaTempoSegundos <= 0f || EstaVazio || TaxaQueimaPorSegundo <= 0f)
        {
            tempoEfetivoQueima = 0f;
            return this;
        }

        var consumoDesejado = TaxaQueimaPorSegundo * deltaTempoSegundos;

        if (QuantidadeAtual >= consumoDesejado)
        {
            tempoEfetivoQueima = deltaTempoSegundos;
            var novaQuantidade = QuantidadeAtual - consumoDesejado;
            return new Combustivel(novaQuantidade, CapacidadeMaxima, TaxaQueimaPorSegundo);
        }
        else
        {
            tempoEfetivoQueima = QuantidadeAtual / TaxaQueimaPorSegundo;
            return new Combustivel(0f, CapacidadeMaxima, TaxaQueimaPorSegundo);
        }
    }

    /// <summary>
    /// Reabastece o tanque ao volume máximo.
    /// </summary>
    /// <returns>Nova instância com o tanque completamente cheio.</returns>
    public Combustivel AbastecerTotal()
    {
        return new Combustivel(CapacidadeMaxima, CapacidadeMaxima, TaxaQueimaPorSegundo);
    }
}
