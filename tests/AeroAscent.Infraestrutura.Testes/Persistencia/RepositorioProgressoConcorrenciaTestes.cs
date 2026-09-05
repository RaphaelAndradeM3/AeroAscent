namespace AeroAscent.Infraestrutura.Testes.Persistencia;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AeroAscent.Core.Dominio.Entidades;
using AeroAscent.Core.Dominio.ObjetosDeValor;
using AeroAscent.Infraestrutura.Configuracao;
using AeroAscent.Infraestrutura.Persistencia;
using Xunit;

/// <summary>
/// Testes de estresse e concorrência assíncrona para <see cref="RepositorioProgressoLocalJson"/>,
/// comprovando exclusão mútua segura via <see cref="System.Threading.SemaphoreSlim"/> sem colisão de I/O
/// e integridade total dos dados sob múltiplas chamadas simultâneas (SC-002).
/// </summary>
public class RepositorioProgressoConcorrenciaTestes : IDisposable
{
    private readonly string _diretorioTeste;
    private readonly ConfiguracaoPersistenciaLocal _configuracao;
    private readonly RepositorioProgressoLocalJson _repositorio;

    public RepositorioProgressoConcorrenciaTestes()
    {
        _diretorioTeste = Path.Combine(Path.GetTempPath(), "AeroAscent_Concorrencia_" + Guid.NewGuid());
        Directory.CreateDirectory(_diretorioTeste);
        _configuracao = new ConfiguracaoPersistenciaLocal(_diretorioTeste);
        _repositorio = new RepositorioProgressoLocalJson(_configuracao);
    }

    public void Dispose()
    {
        _repositorio.Dispose();
        try
        {
            if (Directory.Exists(_diretorioTeste))
            {
                Directory.Delete(_diretorioTeste, recursive: true);
            }
        }
        catch
        {
            // Ignora falhas de limpeza de arquivos temporários do SO
        }
    }

    [Fact]
    public async Task SalvarProgressoAsync_ComDezChamadasConcorrentes_DeveExecutarSemLancarIOExceptionENemCorromperArquivo_SC002()
    {
        // Arrange - Criação de 10 entidades de progresso com quantias distintas
        const int quantidadeTarefas = 10;
        var tarefas = new List<Task>();

        for (int i = 1; i <= quantidadeTarefas; i++)
        {
            var progresso = ProgressoJogador.CriarNovo();
            progresso.CreditarMoedas(new Moeda(i * 100));

            // Dispara tarefas concorrentes em paralelo sem await individual
            tarefas.Add(Task.Run(async () =>
            {
                await _repositorio.SalvarProgressoAsync(progresso);
            }));
        }

        // Act - Aguarda todas as 10 operações simultâneas finalizarem
        await Task.WhenAll(tarefas);

        // Assert - O arquivo deve estar íntegro e legível, sem corrupção
        var progressoFinal = await _repositorio.CarregarProgressoAsync();
        Assert.NotNull(progressoFinal);
        Assert.True(progressoFinal.SaldoMoedas.Quantidade >= 100, "O saldo final deve refletir um dos valores gravados com sucesso.");

        // Nenhhum arquivo com padrão .corrompido deve ter sido gerado
        var arquivosCorrompidos = Directory.GetFiles(_diretorioTeste, "*corrompido*");
        Assert.Empty(arquivosCorrompidos);

        // Arquivo temporário não deve restar órfão
        Assert.False(File.Exists(_configuracao.CaminhoCompletoTemporario));
    }

    [Fact]
    public async Task SalvarECarregarConcorrentemente_NaoDeveDispararColisaoDeAcessoDeArquivo()
    {
        // Arrange
        var progressoInicial = ProgressoJogador.CriarNovo();
        progressoInicial.CreditarMoedas(new Moeda(50));
        await _repositorio.SalvarProgressoAsync(progressoInicial);

        var tarefas = new List<Task>();

        // Act - Misturar 10 operações de salvamento e 10 de leitura concorrentes
        for (int i = 1; i <= 10; i++)
        {
            int index = i;
            tarefas.Add(Task.Run(async () =>
            {
                var p = ProgressoJogador.CriarNovo();
                p.CreditarMoedas(new Moeda(index * 10));
                await _repositorio.SalvarProgressoAsync(p);
            }));

            tarefas.Add(Task.Run(async () =>
            {
                var lido = await _repositorio.CarregarProgressoAsync();
                Assert.NotNull(lido);
            }));
        }

        await Task.WhenAll(tarefas);

        // Assert
        var final = await _repositorio.CarregarProgressoAsync();
        Assert.NotNull(final);
    }
}
