namespace AeroAscent.Core.Aplicacao.DTOs;

using System;
using System.Collections.Generic;

/// <summary>
/// Modelo de dados imutável de apresentação (<c>readonly record struct</c>) contendo o estado
/// consolidado da interface da Oficina e Hangar para renderização atômica em um único frame.
/// </summary>
public readonly record struct ModeloVisualOficina
{
    /// <summary>
    /// Saldo numérico total de moedas do jogador.
    /// </summary>
    public long SaldoMoedas { get; init; }

    /// <summary>
    /// Saldo de moedas pré-formatado com separador de milhar pt-BR (ex: "💰 1.250").
    /// </summary>
    public string SaldoFormatado { get; init; }

    /// <summary>
    /// Maior distância horizontal percorrida em metros.
    /// </summary>
    public float RecordeDistanciaMetros { get; init; }

    /// <summary>
    /// Distância recorde formatada em pt-BR (ex: "Recorde: 245,5 m").
    /// </summary>
    public string RecordeDistanciaFormatado { get; init; }

    /// <summary>
    /// Maior altitude vertical atingida em metros.
    /// </summary>
    public float RecordeAltitudeMetros { get; init; }

    /// <summary>
    /// Altitude recorde formatada em pt-BR (ex: "Altitude Máx: 82,3 m").
    /// </summary>
    public string RecordeAltitudeFormatado { get; init; }

    /// <summary>
    /// Quantidade total de lançamentos concluídos pelo jogador.
    /// </summary>
    public int TotalVoosRealizados { get; init; }

    /// <summary>
    /// Coleção dos 4 cartões de melhoria (Motor, Aerodinâmica, Tanque e Catapulta).
    /// </summary>
    public IReadOnlyList<ItemCartaoOficinaDTO> Cartoes { get; init; }
}
