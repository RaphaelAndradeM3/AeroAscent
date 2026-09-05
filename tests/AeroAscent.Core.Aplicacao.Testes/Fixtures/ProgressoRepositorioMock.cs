namespace AeroAscent.Core.Aplicacao.Testes.Fixtures;

using System.Threading;
using System.Threading.Tasks;
using AeroAscent.Core.Dominio.Contratos;
using AeroAscent.Core.Dominio.Entidades;

/// <summary>
/// Implementação em memória e espião de testes para a interface <see cref="IRepositorioProgresso"/>.
/// Permite simular ausência de perfil salvo, rastrear contagem de chamadas e inspecionar o progresso gravado.
/// </summary>
public class ProgressoRepositorioMock : IRepositorioProgresso
{
    /// <summary>
    /// Progresso atualmente armazenado em memória.
    /// </summary>
    public ProgressoJogador? ProgressoArmazenado { get; set; }

    /// <summary>
    /// Quantidade de vezes que <see cref="SalvarProgressoAsync"/> foi invocado.
    /// </summary>
    public int QuantidadeChamadasSalvar { get; private set; }

    /// <summary>
    /// Quantidade de vezes que <see cref="CarregarProgressoAsync"/> foi invocado.
    /// </summary>
    public int QuantidadeChamadasCarregar { get; private set; }

    /// <summary>
    /// Inicializa o mock com um progresso inicial opcional.
    /// </summary>
    /// <param name="progressoInicial">Instância inicial ou null para simular primeira execução.</param>
    public ProgressoRepositorioMock(ProgressoJogador? progressoInicial = null)
    {
        ProgressoArmazenado = progressoInicial;
    }

    /// <inheritdoc />
    public Task SalvarProgressoAsync(ProgressoJogador progresso, CancellationToken cancelamento = default)
    {
        cancelamento.ThrowIfCancellationRequested();
        QuantidadeChamadasSalvar++;
        ProgressoArmazenado = progresso;
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task<ProgressoJogador?> CarregarProgressoAsync(CancellationToken cancelamento = default)
    {
        cancelamento.ThrowIfCancellationRequested();
        QuantidadeChamadasCarregar++;
        return Task.FromResult(ProgressoArmazenado);
    }
}
