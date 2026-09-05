namespace AeroAscent.Infraestrutura.DTOs;

using System;
using System.Text.Json.Serialization;
using AeroAscent.Core.Dominio.Entidades;
using AeroAscent.Core.Dominio.Enums;
using AeroAscent.Core.Dominio.Excecoes;
using AeroAscent.Core.Dominio.ObjetosDeValor;

/// <summary>
/// Objeto de transferência de dados imutável (<c>readonly record struct</c>) para serialização JSON
/// do progresso do jogador, contendo versionamento explícito de schema para garantir retrocompatibilidade.
/// </summary>
public readonly record struct ProgressoJogadorDTO
{
    /// <summary>
    /// Versão atual do schema de persistência local em JSON (versão canônica: 1).
    /// </summary>
    [JsonPropertyName("versaoSchema")]
    public int VersaoSchema { get; init; }

    /// <summary>
    /// Data e hora da gravação em formato UTC (ISO 8601).
    /// </summary>
    [JsonPropertyName("dataHoraSalvamentoUtc")]
    public DateTime DataHoraSalvamentoUtc { get; init; }

    /// <summary>
    /// Identificador único global do registro de progresso.
    /// </summary>
    [JsonPropertyName("id")]
    public Guid Id { get; init; }

    /// <summary>
    /// Quantidade total de moedas acumuladas pelo jogador.
    /// </summary>
    [JsonPropertyName("saldoMoedas")]
    public long SaldoMoedas { get; init; }

    /// <summary>
    /// Nível atual do motor da aeronave (1 a 10).
    /// </summary>
    [JsonPropertyName("nivelMotor")]
    public int NivelMotor { get; init; }

    /// <summary>
    /// Nível atual de aerodinâmica da aeronave (1 a 10).
    /// </summary>
    [JsonPropertyName("nivelAerodinamica")]
    public int NivelAerodinamica { get; init; }

    /// <summary>
    /// Nível atual do tanque de combustível da aeronave (1 a 10).
    /// </summary>
    [JsonPropertyName("nivelTanqueCombustivel")]
    public int NivelTanqueCombustivel { get; init; }

    /// <summary>
    /// Nível atual da catapulta de propulsão inicial (1 a 10).
    /// </summary>
    [JsonPropertyName("nivelCatapulta")]
    public int NivelCatapulta { get; init; }

    /// <summary>
    /// Maior distância horizontal atingida em metros em um único voo.
    /// </summary>
    [JsonPropertyName("recordeDistanciaMetros")]
    public float RecordeDistanciaMetros { get; init; }

    /// <summary>
    /// Maior altitude vertical atingida em metros em um único voo.
    /// </summary>
    [JsonPropertyName("recordeAltitudeMetros")]
    public float RecordeAltitudeMetros { get; init; }

    /// <summary>
    /// Quantidade total de voos concluídos pelo jogador.
    /// </summary>
    [JsonPropertyName("totalVoosRealizados")]
    public int TotalVoosRealizados { get; init; }

    /// <summary>
    /// Cria uma instância de DTO a partir de uma entidade de domínio <see cref="ProgressoJogador"/>.
    /// </summary>
    /// <param name="progresso">Entidade de domínio a ser mapeada.</param>
    /// <returns>DTO preenchido com dados e versão de schema.</returns>
    /// <exception cref="DominioInvalidoException">Lançada caso a entidade seja nula.</exception>
    public static ProgressoJogadorDTO DoDominio(ProgressoJogador progresso)
    {
        if (progresso == null)
        {
            throw new DominioInvalidoException(nameof(progresso), "A entidade de progresso não pode ser nula.");
        }

        return new ProgressoJogadorDTO
        {
            VersaoSchema = 1,
            DataHoraSalvamentoUtc = DateTime.UtcNow,
            Id = progresso.Id,
            SaldoMoedas = progresso.SaldoMoedas.Quantidade,
            NivelMotor = progresso.Aeronave.ObterNivel(TipoMelhoria.Motor),
            NivelAerodinamica = progresso.Aeronave.ObterNivel(TipoMelhoria.Aerodinamica),
            NivelTanqueCombustivel = progresso.Aeronave.ObterNivel(TipoMelhoria.TanqueCombustivel),
            NivelCatapulta = progresso.Aeronave.ObterNivel(TipoMelhoria.Catapulta),
            RecordeDistanciaMetros = progresso.RecordeDistanciaMetros,
            RecordeAltitudeMetros = progresso.RecordeAltitudeMetros,
            TotalVoosRealizados = progresso.TotalVoosRealizados
        };
    }

    /// <summary>
    /// Converte o DTO desserializado para a entidade de domínio <see cref="ProgressoJogador"/>.
    /// </summary>
    /// <returns>Entidade de domínio com invariantes válidas.</returns>
    public ProgressoJogador ParaDominio()
    {
        var aeronave = new Aeronave(
            Guid.NewGuid(),
            NivelMotor,
            NivelAerodinamica,
            NivelTanqueCombustivel,
            NivelCatapulta);

        return new ProgressoJogador(
            Id,
            aeronave,
            new Moeda(SaldoMoedas),
            RecordeDistanciaMetros,
            RecordeAltitudeMetros,
            TotalVoosRealizados);
    }
}
