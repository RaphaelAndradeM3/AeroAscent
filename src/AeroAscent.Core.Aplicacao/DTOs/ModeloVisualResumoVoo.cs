namespace AeroAscent.Core.Aplicacao.DTOs;

using System;
using System.Globalization;
using AeroAscent.Core.Dominio.Excecoes;
using AeroAscent.Core.Dominio.ObjetosDeValor;

/// <summary>
/// Objeto de transferência de dados imutável alocado exclusivamente na stack (<c>readonly record struct</c>, <c>GC Alloc = 0 bytes</c>)
/// que transporta as métricas discriminadas, valores monetários formatados e sinalizadores de recorde
/// da camada de aplicação para a visão passiva da interface de término de voo.
/// </summary>
public readonly record struct ModeloVisualResumoVoo
{
    private static readonly CultureInfo CulturaBrasil = new("pt-BR");

    /// <summary>
    /// Duração nominal padrão da contagem progressiva animada de moedas em segundos.
    /// </summary>
    public const float DURACAO_ANIMACAO_PADRAO_SEGUNDOS = 1.5f;

    /// <summary>
    /// Distância horizontal total percorrida pela aeronave em metros.
    /// </summary>
    public float DistanciaMetros { get; }

    /// <summary>
    /// Distância formatada em pt-BR com uma casa decimal e sufixo (ex: "125,4 m").
    /// </summary>
    public string DistanciaFormatada { get; }

    /// <summary>
    /// Maior altitude vertical atingida pela aeronave em metros.
    /// </summary>
    public float AltitudeMaximaMetros { get; }

    /// <summary>
    /// Altitude máxima formatada em pt-BR com uma casa decimal e sufixo (ex: "45,2 m").
    /// </summary>
    public string AltitudeFormatada { get; }

    /// <summary>
    /// Moedas ganhas exclusivamente pela distância percorrida.
    /// </summary>
    public long MoedasDistancia { get; }

    /// <summary>
    /// Moedas ganhas exclusivamente pela altitude máxima atingida.
    /// </summary>
    public long MoedasAltitude { get; }

    /// <summary>
    /// Moedas físicas coletadas durante o voo.
    /// </summary>
    public int MoedasColetadas { get; }

    /// <summary>
    /// Total consolidado de moedas ganhas na sessão (soma de distância, altitude e coletáveis).
    /// </summary>
    public long TotalMoedasGanhas { get; }

    /// <summary>
    /// Total de moedas ganhas formatado com sinal de adição (ex: "+34 moedas").
    /// </summary>
    public string TotalMoedasFormatado { get; }

    /// <summary>
    /// Saldo acumulado na carteira do jogador após a creditação e persistência do voo.
    /// </summary>
    public long SaldoFinal { get; }

    /// <summary>
    /// Saldo final formatado com símbolo de moeda e separador de milhar (ex: "💰 1.250").
    /// </summary>
    public string SaldoFinalFormatado { get; }

    /// <summary>
    /// Indica se este voo superou a marca histórica de distância horizontal anterior do jogador.
    /// </summary>
    public bool EhNovoRecordeDistancia { get; }

    /// <summary>
    /// Indica se este voo superou a marca histórica de altitude máxima vertical anterior do jogador.
    /// </summary>
    public bool EhNovoRecordeAltitude { get; }

    /// <summary>
    /// Indica se qualquer novo recorde pessoal (distância ou altitude) foi quebrado nesta sessão.
    /// </summary>
    public bool EhNovoRecorde => EhNovoRecordeDistancia || EhNovoRecordeAltitude;

    /// <summary>
    /// Construtor estruturado do modelo visual de resumo de voo.
    /// </summary>
    /// <param name="distanciaMetros">Distância horizontal alcançada em metros.</param>
    /// <param name="distanciaFormatada">Distância formatada em texto.</param>
    /// <param name="altitudeMaximaMetros">Altitude máxima alcançada em metros.</param>
    /// <param name="altitudeFormatada">Altitude formatada em texto.</param>
    /// <param name="moedasDistancia">Moedas oriundas da distância.</param>
    /// <param name="moedasAltitude">Moedas oriundas da altitude.</param>
    /// <param name="moedasColetadas">Moedas coletadas no ar.</param>
    /// <param name="totalMoedasGanhas">Total de moedas ganhas no voo.</param>
    /// <param name="totalMoedasFormatado">Total de moedas ganhas formatado.</param>
    /// <param name="saldoFinal">Saldo atualizado do jogador.</param>
    /// <param name="saldoFinalFormatado">Saldo atualizado formatado.</param>
    /// <param name="ehNovoRecordeDistancia">Sinalizador de novo recorde de distância.</param>
    /// <param name="ehNovoRecordeAltitude">Sinalizador de novo recorde de altitude.</param>
    /// <exception cref="DominioInvalidoException">Lançada caso valores numéricos sejam negativos.</exception>
    public ModeloVisualResumoVoo(
        float distanciaMetros,
        string distanciaFormatada,
        float altitudeMaximaMetros,
        string altitudeFormatada,
        long moedasDistancia,
        long moedasAltitude,
        int moedasColetadas,
        long totalMoedasGanhas,
        string totalMoedasFormatado,
        long saldoFinal,
        string saldoFinalFormatado,
        bool ehNovoRecordeDistancia,
        bool ehNovoRecordeAltitude)
    {
        if (distanciaMetros < 0f)
        {
            throw new DominioInvalidoException(nameof(distanciaMetros), "A distância percorrida não pode ser negativa.");
        }

        if (altitudeMaximaMetros < 0f)
        {
            throw new DominioInvalidoException(nameof(altitudeMaximaMetros), "A altitude máxima não pode ser negativa.");
        }

        if (moedasDistancia < 0)
        {
            throw new DominioInvalidoException(nameof(moedasDistancia), "As moedas por distância não podem ser negativas.");
        }

        if (moedasAltitude < 0)
        {
            throw new DominioInvalidoException(nameof(moedasAltitude), "As moedas por altitude não podem ser negativas.");
        }

        if (moedasColetadas < 0)
        {
            throw new DominioInvalidoException(nameof(moedasColetadas), "As moedas coletadas não podem ser negativas.");
        }

        if (totalMoedasGanhas < 0)
        {
            throw new DominioInvalidoException(nameof(totalMoedasGanhas), "O total de moedas ganhas não pode ser negativo.");
        }

        if (saldoFinal < 0)
        {
            throw new DominioInvalidoException(nameof(saldoFinal), "O saldo final não pode ser negativo.");
        }

        DistanciaMetros = distanciaMetros;
        DistanciaFormatada = distanciaFormatada ?? string.Empty;
        AltitudeMaximaMetros = altitudeMaximaMetros;
        AltitudeFormatada = altitudeFormatada ?? string.Empty;
        MoedasDistancia = moedasDistancia;
        MoedasAltitude = moedasAltitude;
        MoedasColetadas = moedasColetadas;
        TotalMoedasGanhas = totalMoedasGanhas;
        TotalMoedasFormatado = totalMoedasFormatado ?? string.Empty;
        SaldoFinal = saldoFinal;
        SaldoFinalFormatado = saldoFinalFormatado ?? string.Empty;
        EhNovoRecordeDistancia = ehNovoRecordeDistancia;
        EhNovoRecordeAltitude = ehNovoRecordeAltitude;
    }

    /// <summary>
    /// Constrói o modelo visual imutável a partir do extrato consolidado de finalização de voo do domínio.
    /// Aplica a cultura pt-BR para pontuação decimal (vírgula) e milhar (ponto).
    /// </summary>
    /// <param name="resumo">Extrato imutável de finalização do voo.</param>
    /// <returns>Instância de <see cref="ModeloVisualResumoVoo"/> na stack com textos prontos para exibição.</returns>
    public static ModeloVisualResumoVoo Criar(in ResumoFinalizacaoVoo resumo)
    {
        var distanciaFormatada = string.Format(CulturaBrasil, "{0:F1} m", resumo.DistanciaMetros);
        var altitudeFormatada = string.Format(CulturaBrasil, "{0:F1} m", resumo.AltitudeMaximaMetros);
        var totalMoedasFormatado = string.Format(CulturaBrasil, "+{0:N0} moedas", resumo.MoedasTotalGanhas.Quantidade);
        var saldoFinalFormatado = string.Format(CulturaBrasil, "💰 {0:N0}", resumo.SaldoTotalAtualizado.Quantidade);

        return new ModeloVisualResumoVoo(
            resumo.DistanciaMetros,
            distanciaFormatada,
            resumo.AltitudeMaximaMetros,
            altitudeFormatada,
            resumo.MoedasPorDistancia,
            resumo.MoedasPorAltitude,
            resumo.MoedasColetadas,
            resumo.MoedasTotalGanhas.Quantidade,
            totalMoedasFormatado,
            resumo.SaldoTotalAtualizado.Quantidade,
            saldoFinalFormatado,
            resumo.EhNovoRecordeDistancia,
            resumo.EhNovoRecordeAltitude);
    }
}
