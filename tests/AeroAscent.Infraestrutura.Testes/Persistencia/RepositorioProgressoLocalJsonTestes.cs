namespace AeroAscent.Infraestrutura.Testes.Persistencia;

using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using AeroAscent.Core.Dominio.Entidades;
using AeroAscent.Core.Dominio.Enums;
using AeroAscent.Core.Dominio.Excecoes;
using AeroAscent.Core.Dominio.ObjetosDeValor;
using AeroAscent.Infraestrutura.Configuracao;
using AeroAscent.Infraestrutura.Persistencia;
using Xunit;

/// <summary>
/// Testes automatizados para <see cref="RepositorioProgressoLocalJson"/> cobrindo salvamento atômico,
/// carregamento com integridade, rotação de backup e resiliência a falhas.
/// </summary>
public class RepositorioProgressoLocalJsonTestes : IDisposable
{
    private readonly string _diretorioTeste;
    private readonly ConfiguracaoPersistenciaLocal _configuracao;
    private readonly RepositorioProgressoLocalJson _repositorio;

    public RepositorioProgressoLocalJsonTestes()
    {
        _diretorioTeste = Path.Combine(Path.GetTempPath(), "AeroAscent_Testes_" + Guid.NewGuid());
        Directory.CreateDirectory(_diretorioTeste);
        _configuracao = new ConfiguracaoPersistenciaLocal(_diretorioTeste);
        _repositorio = new RepositorioProgressoLocalJson(_configuracao);
    }

    public void Dispose()
    {
        try
        {
            if (Directory.Exists(_diretorioTeste))
            {
                Directory.Delete(_diretorioTeste, recursive: true);
            }
        }
        catch
        {
            // Ignorar erros de limpeza em arquivos temporários
        }
    }

    [Fact]
    public async Task SalvarProgressoAsync_ERecarregar_DeveGarantirEquivalenciaTotalDeTodasAsPropriedades_Roundtrip()
    {
        // Arrange
        var progressoOriginal = ProgressoJogador.CriarNovo();
        progressoOriginal.CreditarMoedas(new Moeda(500));
        progressoOriginal.Aeronave.AtualizarNivel(TipoMelhoria.Motor, 4);
        progressoOriginal.Aeronave.AtualizarNivel(TipoMelhoria.Aerodinamica, 3);
        progressoOriginal.Aeronave.AtualizarNivel(TipoMelhoria.TanqueCombustivel, 2);
        progressoOriginal.Aeronave.AtualizarNivel(TipoMelhoria.Catapulta, 5);

        // Act - Salvar
        await _repositorio.SalvarProgressoAsync(progressoOriginal);

        // Assert - O arquivo principal deve existir e o temporário deve ter sido limpo/movido
        Assert.True(File.Exists(_configuracao.CaminhoCompletoPrincipal));
        Assert.False(File.Exists(_configuracao.CaminhoCompletoTemporario));

        // Act - Carregar
        var progressoCarregado = await _repositorio.CarregarProgressoAsync();

        // Assert
        Assert.NotNull(progressoCarregado);
        Assert.Equal(progressoOriginal.Id, progressoCarregado.Id);
        Assert.Equal(500, progressoCarregado.SaldoMoedas.Quantidade);
        Assert.Equal(4, progressoCarregado.Aeronave.NivelMotor);
        Assert.Equal(3, progressoCarregado.Aeronave.NivelAerodinamica);
        Assert.Equal(2, progressoCarregado.Aeronave.NivelTanqueCombustivel);
        Assert.Equal(5, progressoCarregado.Aeronave.NivelCatapulta);
        Assert.Equal(progressoOriginal.RecordeDistanciaMetros, progressoCarregado.RecordeDistanciaMetros);
        Assert.Equal(progressoOriginal.RecordeAltitudeMetros, progressoCarregado.RecordeAltitudeMetros);
        Assert.Equal(progressoOriginal.TotalVoosRealizados, progressoCarregado.TotalVoosRealizados);
    }

    [Fact]
    public async Task SalvarProgressoAsync_QuandoArquivoJaExiste_DeveCriarOuAtualizarArquivoDeBackup()
    {
        // Arrange - Primeiro salvamento (Cria o principal)
        var p1 = ProgressoJogador.CriarNovo();
        p1.CreditarMoedas(new Moeda(100));
        await _repositorio.SalvarProgressoAsync(p1);

        Assert.True(File.Exists(_configuracao.CaminhoCompletoPrincipal));
        Assert.False(File.Exists(_configuracao.CaminhoCompletoBackup));

        // Act - Segundo salvamento (Deve gerar o .bak com o estado anterior)
        var p2 = ProgressoJogador.CriarNovo();
        p2.CreditarMoedas(new Moeda(200));
        await _repositorio.SalvarProgressoAsync(p2);

        // Assert
        Assert.True(File.Exists(_configuracao.CaminhoCompletoPrincipal));
        Assert.True(File.Exists(_configuracao.CaminhoCompletoBackup));

        // O arquivo principal deve conter p2 (200 moedas)
        var carregadoPrincipal = await _repositorio.CarregarProgressoAsync();
        Assert.NotNull(carregadoPrincipal);
        Assert.Equal(200, carregadoPrincipal.SaldoMoedas.Quantidade);
    }

    [Fact]
    public async Task OperacoesAssincronas_ComCancellationTokenCancelado_DevemRespeitarCancelamento()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        cts.Cancel();
        var progresso = ProgressoJogador.CriarNovo();

        // Act & Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            _repositorio.SalvarProgressoAsync(progresso, cts.Token));

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            _repositorio.CarregarProgressoAsync(cts.Token));
    }

    [Fact]
    public async Task SalvarProgressoAsync_ComProgressoNulo_DeveLancarDominioInvalidoException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<DominioInvalidoException>(() =>
            _repositorio.SalvarProgressoAsync(null!));
    }

    [Fact]
    public void Construtor_ComConfiguracaoNula_DeveLancarDominioInvalidoException()
    {
        // Act & Assert
        Assert.Throws<DominioInvalidoException>(() => new RepositorioProgressoLocalJson(null!));
    }

    [Fact]
    public async Task CarregarProgressoAsync_QuandoArquivoInexiste_DeveRetornarNuloSemLancarExcecao()
    {
        // Act - Nenhum arquivo salvo no diretório
        var resultado = await _repositorio.CarregarProgressoAsync();

        // Assert
        Assert.Null(resultado);
    }

    [Fact]
    public async Task CarregarProgressoAsync_QuandoArquivoPrincipalCorrompidoComBackupValido_DeveRestaurarDoBackup()
    {
        // Arrange - Criar dois estados para gerar o backup válido
        var p1 = ProgressoJogador.CriarNovo();
        p1.CreditarMoedas(new Moeda(300));
        await _repositorio.SalvarProgressoAsync(p1);

        var p2 = ProgressoJogador.CriarNovo();
        p2.CreditarMoedas(new Moeda(400));
        await _repositorio.SalvarProgressoAsync(p2); // p1 foi para o backup

        // Corromper intencionalmente o arquivo principal com dados mutilados
        await File.WriteAllTextAsync(_configuracao.CaminhoCompletoPrincipal, "JSON_MUTILADO_INVALIDO!@@@");

        // Act - Carregar
        var restaurado = await _repositorio.CarregarProgressoAsync();

        // Assert - Deve ter recuperado do backup (300 moedas)
        Assert.NotNull(restaurado);
        Assert.Equal(300, restaurado.SaldoMoedas.Quantidade);
    }

    [Fact]
    public async Task CarregarProgressoAsync_QuandoArquivoCorrompidoESemBackupValido_DeveIsolarCorrompidoERetornarNulo()
    {
        // Arrange - Criar arquivo principal corrompido sem backup
        await File.WriteAllTextAsync(_configuracao.CaminhoCompletoPrincipal, "{ JSON_CORROMPIDO_SEM_BACKUP }");

        // Act
        var resultado = await _repositorio.CarregarProgressoAsync();

        // Assert
        Assert.Null(resultado);
        Assert.False(File.Exists(_configuracao.CaminhoCompletoPrincipal)); // Foi renomeado
        var arquivosCorrompidos = Directory.GetFiles(_diretorioTeste, "*corrompido*");
        Assert.NotEmpty(arquivosCorrompidos);
    }

    [Fact]
    public async Task CarregarProgressoAsync_QuandoPrincipalEBackupCorrompidos_DeveIsolarPrincipalERetornarNulo()
    {
        // Arrange - Criar principal e backup danificados
        await File.WriteAllTextAsync(_configuracao.CaminhoCompletoPrincipal, "{ JSON_PRINCIPAL_CORROMPIDO }");
        await File.WriteAllTextAsync(_configuracao.CaminhoCompletoBackup, "{ JSON_BACKUP_CORROMPIDO }");

        // Act
        var resultado = await _repositorio.CarregarProgressoAsync();

        // Assert - Não deve travar nem lançar exceção, deve isolar o arquivo e retornar nulo
        Assert.Null(resultado);
        Assert.False(File.Exists(_configuracao.CaminhoCompletoPrincipal));
        var arquivosCorrompidos = Directory.GetFiles(_diretorioTeste, "*corrompido*");
        Assert.NotEmpty(arquivosCorrompidos);
    }

    [Fact]
    public async Task SalvarProgressoAsync_DeveExecutarEmMenosDe15Milissegundos_SC001()
    {
        // Arrange
        var progresso = ProgressoJogador.CriarNovo();
        progresso.CreditarMoedas(new Moeda(100));

        // Warmup
        await _repositorio.SalvarProgressoAsync(progresso);

        // Act & Measure
        var cronometro = Stopwatch.StartNew();
        progresso.CreditarMoedas(new Moeda(50));
        await _repositorio.SalvarProgressoAsync(progresso);
        cronometro.Stop();

        // Assert - SC-001: Tempo < 15ms
        Assert.True(cronometro.ElapsedMilliseconds < 15, $"Tempo de salvamento ({cronometro.ElapsedMilliseconds}ms) excedeu o teto de 15ms.");
    }
}
