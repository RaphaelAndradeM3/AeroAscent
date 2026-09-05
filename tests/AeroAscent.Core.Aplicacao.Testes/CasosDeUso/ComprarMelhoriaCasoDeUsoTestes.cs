namespace AeroAscent.Core.Aplicacao.Testes.CasosDeUso;

using System;
using System.Diagnostics;
using System.Threading.Tasks;
using AeroAscent.Core.Aplicacao.CasosDeUso;
using AeroAscent.Core.Aplicacao.Testes.Fixtures;
using AeroAscent.Core.Dominio.Entidades;
using AeroAscent.Core.Dominio.Enums;
using AeroAscent.Core.Dominio.Excecoes;
using AeroAscent.Core.Dominio.ObjetosDeValor;
using Xunit;

/// <summary>
/// Testes automatizados para o caso de uso <see cref="ComprarMelhoriaCasoDeUso"/>.
/// </summary>
public class ComprarMelhoriaCasoDeUsoTestes
{
    [Fact]
    public async Task ExecutarAsync_ComSaldoSuficiente_DeveEvoluirComponenteDebitarMoedasESalvarProgresso()
    {
        // Arrange
        var progresso = ProgressoJogador.CriarNovo();
        progresso.CreditarMoedas(new Moeda(200)); // Saldo inicial: 200 moedas
        var repositorio = new ProgressoRepositorioMock(progresso);
        var casoDeUso = new ComprarMelhoriaCasoDeUso(repositorio);

        // Act - Comprar Motor do nível 1 para o nível 2 (Custo: 50 moedas)
        var resultado = await casoDeUso.ExecutarAsync(TipoMelhoria.Motor);

        // Assert
        Assert.Equal(TipoMelhoria.Motor, resultado.Tipo);
        Assert.Equal(1, resultado.NivelAnterior);
        Assert.Equal(2, resultado.NovoNivel);
        Assert.Equal(50, resultado.CustoPago.Quantidade);
        Assert.Equal(150, resultado.SaldoRestante.Quantidade);
        Assert.False(resultado.AtingiuNivelMaximo);
        Assert.NotNull(resultado.ProximoCusto);
        Assert.Equal(75, resultado.ProximoCusto.Value.Quantidade); // N2 -> N3: floor(50 * 1.5) = 75

        // Validar integridade do agregado salvo
        Assert.Equal(1, repositorio.QuantidadeChamadasSalvar);
        Assert.NotNull(repositorio.ProgressoArmazenado);
        Assert.Equal(2, repositorio.ProgressoArmazenado!.Aeronave.NivelMotor);
        Assert.Equal(150, repositorio.ProgressoArmazenado.SaldoMoedas.Quantidade);
    }

    [Theory]
    [InlineData(TipoMelhoria.Motor, 50, 75)]
    [InlineData(TipoMelhoria.Aerodinamica, 40, 60)]
    [InlineData(TipoMelhoria.TanqueCombustivel, 30, 45)]
    [InlineData(TipoMelhoria.Catapulta, 60, 90)]
    public async Task ExecutarAsync_ParaCadaComponente_DeveAplicarCustosECalcularProximoNivel(
        TipoMelhoria tipo,
        long custoEsperadoNivel1,
        long custoEsperadoNivel2)
    {
        // Arrange
        var progresso = ProgressoJogador.CriarNovo();
        progresso.CreditarMoedas(new Moeda(1000));
        var repositorio = new ProgressoRepositorioMock(progresso);
        var casoDeUso = new ComprarMelhoriaCasoDeUso(repositorio);

        // Act
        var resultado = await casoDeUso.ExecutarAsync(tipo);

        // Assert
        Assert.Equal(tipo, resultado.Tipo);
        Assert.Equal(1, resultado.NivelAnterior);
        Assert.Equal(2, resultado.NovoNivel);
        Assert.Equal(custoEsperadoNivel1, resultado.CustoPago.Quantidade);
        Assert.NotNull(resultado.ProximoCusto);
        Assert.Equal(custoEsperadoNivel2, resultado.ProximoCusto.Value.Quantidade);
    }

    [Fact]
    public async Task ExecutarAsync_ComSaldoInsuficiente_DeveLancarSaldoInsuficienteExceptionEPreservarSaldo()
    {
        // Arrange
        var progresso = ProgressoJogador.CriarNovo();
        progresso.CreditarMoedas(new Moeda(20)); // Saldo: 20 moedas, Tanque custa 30 moedas
        var repositorio = new ProgressoRepositorioMock(progresso);
        var casoDeUso = new ComprarMelhoriaCasoDeUso(repositorio);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<SaldoInsuficienteException>(() =>
            casoDeUso.ExecutarAsync(TipoMelhoria.TanqueCombustivel));

        Assert.Equal(20, ex.SaldoAtual);
        Assert.Equal(30, ex.QuantiaNecessaria);

        // Estado deve permanecer intacto e nenhuma alteração deve ter sido salva
        Assert.Equal(0, repositorio.QuantidadeChamadasSalvar);
        Assert.Equal(20, progresso.SaldoMoedas.Quantidade);
        Assert.Equal(1, progresso.Aeronave.NivelTanqueCombustivel);
    }

    [Fact]
    public async Task ExecutarAsync_QuandoComponenteJaEstaNoNivelMaximo10_DeveLancarMelhoriaNivelMaximoException()
    {
        // Arrange
        var aeronave = new Aeronave(Guid.NewGuid(), nivelMotor: 10, nivelAerodinamica: 1, nivelTanqueCombustivel: 1, nivelCatapulta: 1);
        var progresso = new ProgressoJogador(Guid.NewGuid(), aeronave, new Moeda(50000), 0f, 0f, 0);
        var repositorio = new ProgressoRepositorioMock(progresso);
        var casoDeUso = new ComprarMelhoriaCasoDeUso(repositorio);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<MelhoriaNivelMaximoException>(() =>
            casoDeUso.ExecutarAsync(TipoMelhoria.Motor));

        Assert.Equal(TipoMelhoria.Motor, ex.Tipo);
        Assert.Equal(10, ex.NivelAtual);
        Assert.Equal(0, repositorio.QuantidadeChamadasSalvar);
    }

    [Fact]
    public async Task ExecutarAsync_QuandoEvolucaoAlcancaoNivel10_DeveMarcarAtingiuNivelMaximoEProximoCustoNulo()
    {
        // Arrange
        var aeronave = new Aeronave(Guid.NewGuid(), nivelMotor: 9, nivelAerodinamica: 1, nivelTanqueCombustivel: 1, nivelCatapulta: 1);
        var progresso = new ProgressoJogador(Guid.NewGuid(), aeronave, new Moeda(50000), 0f, 0f, 0);
        var repositorio = new ProgressoRepositorioMock(progresso);
        var casoDeUso = new ComprarMelhoriaCasoDeUso(repositorio);

        // Act - Evoluir Motor de 9 para 10
        var resultado = await casoDeUso.ExecutarAsync(TipoMelhoria.Motor);

        // Assert
        Assert.Equal(9, resultado.NivelAnterior);
        Assert.Equal(10, resultado.NovoNivel);
        Assert.True(resultado.AtingiuNivelMaximo);
        Assert.Null(resultado.ProximoCusto);
        Assert.Equal(1, repositorio.QuantidadeChamadasSalvar);
    }

    [Fact]
    public async Task ExecutarAsync_QuandoRepositorioRetornaNulo_DeveInstanciarProgressoResiliente()
    {
        // Arrange - Repositório vazio (primeira execução)
        var repositorio = new ProgressoRepositorioMock(null);
        var casoDeUso = new ComprarMelhoriaCasoDeUso(repositorio);

        // Act & Assert - Não tem saldo (0 moedas), deve lançar SaldoInsuficienteException e não NullReferenceException
        var ex = await Assert.ThrowsAsync<SaldoInsuficienteException>(() =>
            casoDeUso.ExecutarAsync(TipoMelhoria.Motor));

        Assert.Equal(0, ex.SaldoAtual);
        Assert.Equal(50, ex.QuantiaNecessaria);
    }

    [Fact]
    public async Task ExecutarAsync_ComTipoInvalido_DeveLancarDominioInvalidoException()
    {
        // Arrange
        var progresso = ProgressoJogador.CriarNovo();
        progresso.CreditarMoedas(new Moeda(1000));
        var repositorio = new ProgressoRepositorioMock(progresso);
        var casoDeUso = new ComprarMelhoriaCasoDeUso(repositorio);

        // Act & Assert
        await Assert.ThrowsAsync<DominioInvalidoException>(() =>
            casoDeUso.ExecutarAsync((TipoMelhoria)999));
    }

    [Fact]
    public void Construtor_ComRepositorioNulo_DeveLancarDominioInvalidoException()
    {
        // Act & Assert
        Assert.Throws<DominioInvalidoException>(() => new ComprarMelhoriaCasoDeUso(null!));
    }

    [Fact]
    public async Task ExecutarAsync_DeveExecutarEmMenosDe5Milissegundos_SC002()
    {
        // Arrange
        var progresso = ProgressoJogador.CriarNovo();
        progresso.CreditarMoedas(new Moeda(1000));
        var repositorio = new ProgressoRepositorioMock(progresso);
        var casoDeUso = new ComprarMelhoriaCasoDeUso(repositorio);

        // Warmup
        await casoDeUso.ExecutarAsync(TipoMelhoria.Aerodinamica);

        // Act & Measure
        var cronometro = Stopwatch.StartNew();
        var resultado = await casoDeUso.ExecutarAsync(TipoMelhoria.Catapulta);
        cronometro.Stop();

        // Assert - SC-002: tempo < 5ms
        Assert.True(cronometro.ElapsedMilliseconds < 5, $"Tempo de execução ({cronometro.ElapsedMilliseconds}ms) excedeu o teto de 5ms.");
        Assert.Equal(2, resultado.NovoNivel);
    }
}
