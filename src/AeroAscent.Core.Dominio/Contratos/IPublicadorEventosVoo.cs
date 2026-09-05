namespace AeroAscent.Core.Dominio.Contratos;

using AeroAscent.Core.Dominio.ObjetosDeValor;

/// <summary>
/// Contrato do publicador de eventos relacionados ao ciclo de vida da sessão de voo da aeronave.
/// Permite que camadas externas (UI, Áudio, Economia, Persistência) sejam notificadas sem acoplamento.
/// </summary>
public interface IPublicadorEventosVoo
{
    /// <summary>
    /// Notifica os observadores que o voo foi concluído com pouso e parada total da aeronave no solo.
    /// </summary>
    /// <param name="resultado">Métricas finais e premiação calculada da sessão de voo.</param>
    void PublicarVooConcluido(ResultadoFimVoo resultado);
}
