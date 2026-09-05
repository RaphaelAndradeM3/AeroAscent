namespace AeroAscent.Core.Dominio.Contratos;

using AeroAscent.Core.Dominio.Enums;
using AeroAscent.Core.Dominio.ObjetosDeValor;

/// <summary>
/// Contrato de serviço para cálculos de balanceamento econômico, recompensas de voo e custos de melhorias.
/// </summary>
public interface IServicoEconomia
{
    /// <summary>
    /// Calcula a premiação final de um voo com base na distância, altitude e moedas coletadas.
    /// </summary>
    /// <param name="distanciaMetros">Distância horizontal alcançada em metros.</param>
    /// <param name="altitudeMaximaMetros">Altitude máxima atingida na rodada.</param>
    /// <param name="moedasColetadas">Total de moedas físicas coletadas durante o voo.</param>
    /// <returns>Instância imutável de ResultadoVoo consolidando os valores calculados.</returns>
    ResultadoVoo CalcularRecompensaVoo(float distanciaMetros, float altitudeMaximaMetros, int moedasColetadas);

    /// <summary>
    /// Calcula o custo de moedas necessário para evoluir uma melhoria para o nível subsequente.
    /// </summary>
    /// <param name="tipo">Tipo da melhoria (Motor, Aerodinâmica, Tanque ou Catapulta).</param>
    /// <param name="nivelAtual">Nível atual do componente (1 a 9).</param>
    /// <returns>Objeto de valor Moeda com o custo calculado.</returns>
    Moeda CalcularCustoMelhoria(TipoMelhoria tipo, int nivelAtual);
}
