namespace AeroAscent.Core.Dominio.Testes.Fixtures;

using System;
using AeroAscent.Core.Dominio.Entidades;
using AeroAscent.Core.Dominio.ObjetosDeValor;

/// <summary>
/// Provedor de dados e utilitários de teste para o sistema de coletáveis em voo e object pooling.
/// </summary>
public static class ColetaveisTestFixture
{
    /// <summary>
    /// Raio de coleta padrão para moedas flutuantes (1.5 metros).
    /// </summary>
    public const float RAIO_PADRAO_MOEDA_METROS = 1.5f;

    /// <summary>
    /// Raio de coleta padrão para anéis de impulso de vento (3.5 metros).
    /// </summary>
    public const float RAIO_PADRAO_ANEL_VENTO_METROS = 3.5f;

    /// <summary>
    /// Cria uma posição padrão de teste no plano longitudinal Y-Z.
    /// </summary>
    /// <param name="posicaoZ">Avanço longitudinal em metros (Z).</param>
    /// <param name="altitudeY">Altitude em metros (Y).</param>
    /// <returns>Instância de VetorVoo no plano Y-Z com X=0.</returns>
    public static VetorVoo CriarPosicao(float posicaoZ, float altitudeY)
    {
        return new VetorVoo(0f, altitudeY, posicaoZ);
    }

    /// <summary>
    /// Cria uma sessão de voo no status EmVoo com métricas iniciais zeradas.
    /// </summary>
    /// <returns>Instância de Voo ativa.</returns>
    public static Voo CriarVooEmAndamento()
    {
        var aero = new Aeronave(Guid.NewGuid(), 1, 1, 1, 1);
        var voo = Voo.Iniciar(aero);
        voo.Decolar();
        return voo;
    }

    /// <summary>
    /// Cria um estado físico de aeronave para testes de colisão e aproximação.
    /// </summary>
    /// <param name="posicaoZ">Avanço horizontal em metros.</param>
    /// <param name="altitudeY">Altitude vertical em metros.</param>
    /// <param name="velocidadeZ">Velocidade horizontal em metros por segundo.</param>
    /// <param name="velocidadeY">Velocidade vertical em metros por segundo.</param>
    /// <returns>Novo EstadoFisicoAeronave na stack.</returns>
    public static EstadoFisicoAeronave CriarEstadoAeronave(
        float posicaoZ,
        float altitudeY,
        float velocidadeZ = 20.0f,
        float velocidadeY = 0f)
    {
        return EstadoFisicoAeronave.CriarInicial(
            new VetorVoo(0f, altitudeY, posicaoZ),
            new VetorVoo(0f, velocidadeY, velocidadeZ),
            0f);
    }
}
