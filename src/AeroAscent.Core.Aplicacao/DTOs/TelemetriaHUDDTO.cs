namespace AeroAscent.Core.Aplicacao.DTOs;

using System;
using AeroAscent.Core.Dominio.Excecoes;

/// <summary>
/// Objeto de transferência de dados imutável alocado exclusivamente na stack (GC Alloc = 0 bytes)
/// que transporta a telemetria e o estado do voo da camada de aplicação para a visão passiva do HUD.
/// </summary>
public readonly record struct TelemetriaHUDDTO
{
    /// <summary>
    /// Distância horizontal percorrida em metros desde o ponto de lançamento.
    /// </summary>
    public float DistanciaPercorridaMetros { get; }

    /// <summary>
    /// Melhor distância histórica a ser superada na partida atual.
    /// </summary>
    public float RecordeDistanciaMetros { get; }

    /// <summary>
    /// Altitude atual em metros em relação ao solo.
    /// </summary>
    public float AltitudeAtualMetros { get; }

    /// <summary>
    /// Módulo da velocidade vetorial instantânea em metros por segundo.
    /// </summary>
    public float VelocidadeAtualMetrosPorSegundo { get; }

    /// <summary>
    /// Percentual de combustível restante no tanque, normalizado entre 0.0f (0%) e 1.0f (100%).
    /// </summary>
    public float PercentualCombustivel { get; }

    /// <summary>
    /// Quantidade de moedas físicas coletadas durante este voo.
    /// </summary>
    public int MoedasColetadas { get; }

    /// <summary>
    /// Indica se a distância percorrida superou o recorde estabelecido para a partida.
    /// </summary>
    public bool RecordeSuperado { get; }

    /// <summary>
    /// Indica se o propulsor de boost está disponível para uso (aeronave em voo e com combustível).
    /// </summary>
    public bool BoostDisponivel { get; }

    /// <summary>
    /// Construtor estruturado que assegura a integridade das métricas de telemetria.
    /// </summary>
    /// <param name="distanciaPercorridaMetros">Distância percorrida em metros (não negativa).</param>
    /// <param name="recordeDistanciaMetros">Recorde de distância a superar (não negativo).</param>
    /// <param name="altitudeAtualMetros">Altitude instantânea em metros (não negativa).</param>
    /// <param name="velocidadeAtualMetrosPorSegundo">Velocidade instantânea em m/s (não negativa).</param>
    /// <param name="percentualCombustivel">Fração normalizada de combustível restante (0.0f a 1.0f).</param>
    /// <param name="moedasColetadas">Moedas coletadas no voo (não negativa).</param>
    /// <param name="recordeSuperado">Flag de recorde batido.</param>
    /// <param name="boostDisponivel">Flag de propulsor utilizável.</param>
    /// <exception cref="DominioInvalidoException">Lançada caso valores numéricos sejam negativos.</exception>
    public TelemetriaHUDDTO(
        float distanciaPercorridaMetros,
        float recordeDistanciaMetros,
        float altitudeAtualMetros,
        float velocidadeAtualMetrosPorSegundo,
        float percentualCombustivel,
        int moedasColetadas,
        bool recordeSuperado,
        bool boostDisponivel)
    {
        if (distanciaPercorridaMetros < 0f)
        {
            throw new DominioInvalidoException(nameof(distanciaPercorridaMetros), "A distância percorrida não pode ser negativa.");
        }

        if (recordeDistanciaMetros < 0f)
        {
            throw new DominioInvalidoException(nameof(recordeDistanciaMetros), "O recorde de distância não pode ser negativo.");
        }

        if (altitudeAtualMetros < 0f)
        {
            throw new DominioInvalidoException(nameof(altitudeAtualMetros), "A altitude não pode ser negativa.");
        }

        if (velocidadeAtualMetrosPorSegundo < 0f)
        {
            throw new DominioInvalidoException(nameof(velocidadeAtualMetrosPorSegundo), "A velocidade não pode ser negativa.");
        }

        if (moedasColetadas < 0)
        {
            throw new DominioInvalidoException(nameof(moedasColetadas), "A quantidade de moedas coletadas não pode ser negativa.");
        }

        DistanciaPercorridaMetros = distanciaPercorridaMetros;
        RecordeDistanciaMetros = recordeDistanciaMetros;
        AltitudeAtualMetros = altitudeAtualMetros;
        VelocidadeAtualMetrosPorSegundo = velocidadeAtualMetrosPorSegundo;
        PercentualCombustivel = Math.Clamp(percentualCombustivel, 0f, 1f);
        MoedasColetadas = moedasColetadas;
        RecordeSuperado = recordeSuperado;
        BoostDisponivel = boostDisponivel;
    }
}
