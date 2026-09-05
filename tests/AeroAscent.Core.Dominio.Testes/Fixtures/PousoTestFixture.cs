namespace AeroAscent.Core.Dominio.Testes.Fixtures;

using System;
using AeroAscent.Core.Dominio.Entidades;
using AeroAscent.Core.Dominio.ObjetosDeValor;

/// <summary>
/// Provedor de dados e utilitários de teste para o sistema de pouso, física de solo e encerramento de voo.
/// </summary>
public static class PousoTestFixture
{
    /// <summary>
    /// Limiar canônico de velocidade para congelamento no solo (0.15 m/s).
    /// </summary>
    public const float LIMIAR_PARADA_CANONICO_METROS_POR_SEGUNDO = 0.15f;

    /// <summary>
    /// Coeficiente de atrito cinético padrão do solo (0.3).
    /// </summary>
    public const float COEFICIENTE_ATRITO_PADRAO = 0.3f;

    /// <summary>
    /// Cria uma sessão de voo ativa com valores pré-configurados para testes de pouso.
    /// </summary>
    /// <param name="distanciaInicial">Distância inicial percorrida em metros.</param>
    /// <param name="altitudeMaxima">Altitude máxima atingida em metros.</param>
    /// <param name="moedasColetadas">Quantidade inicial de moedas coletadas.</param>
    /// <returns>Instância de Voo no status EmVoo.</returns>
    public static Voo CriarVooAtivo(float distanciaInicial = 100f, float altitudeMaxima = 30f, int moedasColetadas = 5)
    {
        var aeronave = new Aeronave(Guid.NewGuid(), 1, 1, 1, 1);
        var voo = Voo.Iniciar(aeronave);
        voo.Decolar();
        if (distanciaInicial > 0f || altitudeMaxima > 0f || moedasColetadas > 0)
        {
            voo.AtualizarMetricas(distanciaInicial, altitudeMaxima, moedasColetadas);
        }
        return voo;
    }

    /// <summary>
    /// Cria um estado físico descendente se aproximando do solo.
    /// </summary>
    /// <param name="posicaoZ">Posição longitudinal atual em metros.</param>
    /// <param name="altitudeY">Altitude vertical em metros.</param>
    /// <param name="velocidadeZ">Velocidade horizontal em metros por segundo.</param>
    /// <param name="velocidadeY">Velocidade vertical descendente em metros por segundo.</param>
    /// <param name="inclinacaoPitchGraus">Ângulo de pitch em graus.</param>
    /// <returns>EstadoFisicoAeronave alocado na stack.</returns>
    public static EstadoFisicoAeronave CriarEstadoDescendente(
        float posicaoZ = 50f,
        float altitudeY = 2f,
        float velocidadeZ = 15f,
        float velocidadeY = -3f,
        float inclinacaoPitchGraus = -5f)
    {
        return EstadoFisicoAeronave.CriarInicial(
            new VetorVoo(0f, altitudeY, posicaoZ),
            new VetorVoo(0f, velocidadeY, velocidadeZ),
            inclinacaoPitchGraus);
    }

    /// <summary>
    /// Cria um estado físico da aeronave já deslizando no solo com velocidade horizontal positiva.
    /// </summary>
    /// <param name="posicaoZ">Posição longitudinal atual em metros.</param>
    /// <param name="velocidadeZ">Velocidade horizontal residual em metros por segundo.</param>
    /// <param name="inclinacaoPitchGraus">Ângulo de arfagem em graus.</param>
    /// <returns>EstadoFisicoAeronave no solo.</returns>
    public static EstadoFisicoAeronave CriarEstadoNoSolo(
        float posicaoZ = 120f,
        float velocidadeZ = 5f,
        float inclinacaoPitchGraus = 2f)
    {
        return EstadoFisicoAeronave.Criar(
            new VetorVoo(0f, 0f, posicaoZ),
            new VetorVoo(0f, 0f, velocidadeZ),
            inclinacaoPitchGraus,
            VetorVoo.Zero,
            noSolo: true);
    }

    /// <summary>
    /// Cria um estado físico de aeronave em repouso absoluto no solo.
    /// </summary>
    /// <param name="posicaoZ">Posição final em metros.</param>
    /// <returns>EstadoFisicoAeronave completamente parado no solo.</returns>
    public static EstadoFisicoAeronave CriarEstadoParadoNoSolo(float posicaoZ = 150f)
    {
        return EstadoFisicoAeronave.Criar(
            new VetorVoo(0f, 0f, posicaoZ),
            VetorVoo.Zero,
            0f,
            VetorVoo.Zero,
            noSolo: true);
    }
}
