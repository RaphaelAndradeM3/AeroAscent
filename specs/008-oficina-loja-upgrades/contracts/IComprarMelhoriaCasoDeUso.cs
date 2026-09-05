namespace AeroAscent.Core.Aplicacao.Contratos;

using System.Threading;
using System.Threading.Tasks;
using AeroAscent.Core.Dominio.Enums;
using AeroAscent.Core.Dominio.ObjetosDeValor;

/// <summary>
/// Contrato do caso de uso de aplicação responsável por orquestrar a compra de melhorias mecânicas da aeronave,
/// validando o saldo de moedas do jogador, decrementando o custo, incrementando o nível do componente
/// e persistindo o progresso atualizado no repositório.
/// </summary>
public interface IComprarMelhoriaCasoDeUso
{
    /// <summary>
    /// Executa a transação de compra de uma melhoria para a aeronave do jogador.
    /// </summary>
    /// <param name="tipo">Tipo da melhoria mecânica a ser adquirida.</param>
    /// <param name="cancelamento">Token de cancelamento da operação assíncrona.</param>
    /// <returns>Extrato discriminado da compra alocado na stack.</returns>
    Task<ResultadoCompraMelhoria> ExecutarAsync(TipoMelhoria tipo, CancellationToken cancelamento = default);
}
