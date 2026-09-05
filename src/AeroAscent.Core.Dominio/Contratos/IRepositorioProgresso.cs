namespace AeroAscent.Core.Dominio.Contratos;

using AeroAscent.Core.Dominio.Entidades;

/// <summary>
/// Contrato do repositório para persistência atômica do progresso global do jogador.
/// </summary>
public interface IRepositorioProgresso
{
    /// <summary>
    /// Persiste o agregado completo do progresso do jogador de forma atômica e assíncrona.
    /// </summary>
    /// <param name="progresso">Instância com o estado atualizado do jogador.</param>
    /// <param name="cancelamento">Token de cancelamento da operação.</param>
    /// <returns>Task assíncrona representando a conclusão do salvamento.</returns>
    Task SalvarProgressoAsync(ProgressoJogador progresso, CancellationToken cancelamento = default);

    /// <summary>
    /// Carrega o progresso persistido do jogador de forma assíncrona.
    /// </summary>
    /// <param name="cancelamento">Token de cancelamento da operação.</param>
    /// <returns>O progresso carregado ou null caso nenhum registro exista.</returns>
    Task<ProgressoJogador?> CarregarProgressoAsync(CancellationToken cancelamento = default);
}
