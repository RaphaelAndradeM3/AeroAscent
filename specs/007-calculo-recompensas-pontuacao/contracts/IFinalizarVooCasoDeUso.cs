namespace AeroAscent.Core.Aplicacao.Contratos;

using System.Threading;
using System.Threading.Tasks;
using AeroAscent.Core.Dominio.Entidades;
using AeroAscent.Core.Dominio.ObjetosDeValor;

/// <summary>
/// Contrato do caso de uso de aplicação responsável por finalizar a sessão de voo,
/// calcular as recompensas financeiras por distância, altitude e coletáveis,
/// creditar o saldo do jogador, registrar recordes históricos e persistir o progresso.
/// </summary>
public interface IFinalizarVooCasoDeUso
{
    /// <summary>
    /// Executa a finalização da sessão de voo, calculando premiações e persistindo o progresso do jogador.
    /// </summary>
    /// <param name="voo">Entidade da sessão de voo a ser finalizada.</param>
    /// <param name="cancelamento">Token de cancelamento da operação assíncrona.</param>
    /// <returns>Estrutura ResumoFinalizacaoVoo na stack com as métricas discriminadas e novos recordes.</returns>
    Task<ResumoFinalizacaoVoo> ExecutarAsync(Voo voo, CancellationToken cancelamento = default);
}
