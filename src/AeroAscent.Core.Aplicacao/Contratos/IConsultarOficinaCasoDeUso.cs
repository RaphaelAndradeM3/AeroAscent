namespace AeroAscent.Core.Aplicacao.Contratos;

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AeroAscent.Core.Aplicacao.DTOs;

/// <summary>
/// Contrato do caso de uso de aplicação responsável por consultar o catálogo consolidado da oficina,
/// projetando a lista de itens com seus níveis atuais, custos para a próxima evolução e status de compra.
/// </summary>
public interface IConsultarOficinaCasoDeUso
{
    /// <summary>
    /// Consulta o estado atual da oficina baseado no progresso salvo do jogador.
    /// </summary>
    /// <param name="cancelamento">Token de cancelamento da operação assíncrona.</param>
    /// <returns>Lista imutável com a projeção das 4 melhorias mecânicas.</returns>
    Task<IReadOnlyList<ItemOficinaDTO>> ExecutarAsync(CancellationToken cancelamento = default);
}
