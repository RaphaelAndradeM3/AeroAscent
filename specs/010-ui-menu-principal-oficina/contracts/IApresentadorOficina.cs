namespace AeroAscent.Core.Aplicacao.Contratos;

using System;
using System.Threading;
using System.Threading.Tasks;
using AeroAscent.Core.Dominio.Enums;

/// <summary>
/// Contrato do apresentador da Oficina e Hangar (Model-View-Presenter), responsável por orquestrar
/// as ações da interface, disparar casos de uso, formatar dados em pt-BR e prevenir concorrência de cliques.
/// </summary>
public interface IApresentadorOficina : IDisposable
{
    /// <summary>
    /// Evento disparado quando o jogador aciona o botão de decolagem, requisitando transição para o voo.
    /// </summary>
    event Action? AoSolicitarDecolagem;

    /// <summary>
    /// Inicializa a tela da oficina carregando dados via caso de uso e atualizando a visão passiva.
    /// </summary>
    /// <param name="cancelamento">Token de cancelamento cooperativo.</param>
    Task InicializarAsync(CancellationToken cancelamento = default);

    /// <summary>
    /// Processa a solicitação de compra de uma melhoria, debitando saldo e atualizando a interface.
    /// </summary>
    /// <param name="tipo">Tipo de melhoria mecânica a ser adquirida.</param>
    /// <param name="cancelamento">Token de cancelamento cooperativo.</param>
    Task ProcessarCompraAsync(TipoMelhoria tipo, CancellationToken cancelamento = default);

    /// <summary>
    /// Comanda a solicitação de transição para a partida e decolagem.
    /// </summary>
    void SolicitarDecolagem();
}
