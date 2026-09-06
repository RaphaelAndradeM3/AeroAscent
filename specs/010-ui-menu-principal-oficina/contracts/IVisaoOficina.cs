namespace AeroAscent.Core.Aplicacao.Contratos;

using System;
using AeroAscent.Core.Aplicacao.DTOs;
using AeroAscent.Core.Dominio.Enums;

/// <summary>
/// Contrato de visão passiva para a tela da Oficina e Menu Principal.
/// Implementada na camada de Apresentação (Unity MonoBehaviour) e comandada pelo <c>ApresentadorOficina</c>.
/// </summary>
public interface IVisaoOficina
{
    /// <summary>
    /// Evento disparado pela visão quando o jogador clica no botão de compra de uma melhoria específica.
    /// </summary>
    event Action<TipoMelhoria>? AoClicarComprar;

    /// <summary>
    /// Evento disparado pela visão quando o jogador clica no botão principal "DECOLAR".
    /// </summary>
    event Action? AoClicarDecolar;

    /// <summary>
    /// Atualiza todos os elementos visuais da interface (saldo, 4 cartões, recordes) com o modelo fornecido.
    /// </summary>
    /// <param name="modelo">Dados pré-formatados prontos para exibição em tela.</param>
    void AtualizarTela(ModeloVisualOficina modelo);

    /// <summary>
    /// Habilita ou desabilita a interação com os botões de compra para prevenir reentrância e duplo clique.
    /// </summary>
    /// <param name="habilitada"><c>true</c> para permitir toques; <c>false</c> para bloquear durante transações assíncronas.</param>
    void DefinirInteracaoHabilitada(bool habilitada);

    /// <summary>
    /// Emite feedback sonoro e visual de celebração após a aquisição bem-sucedida de um upgrade.
    /// </summary>
    /// <param name="tipo">Componente evoluído.</param>
    /// <param name="novoNivel">Novo nível atingido.</param>
    void ExibirFeedbackCompra(TipoMelhoria tipo, int novoNivel);

    /// <summary>
    /// Exibe uma notificação amigável de erro na interface (ex: saldo insuficiente ou falha de leitura).
    /// </summary>
    /// <param name="mensagem">Mensagem explicativa para o usuário.</param>
    void ExibirMensagemErro(string mensagem);
}
