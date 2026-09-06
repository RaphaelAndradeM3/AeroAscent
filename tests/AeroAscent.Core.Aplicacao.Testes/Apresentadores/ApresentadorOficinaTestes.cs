namespace AeroAscent.Core.Aplicacao.Testes.Apresentadores;

using System;
using System.Linq;
using System.Threading.Tasks;
using AeroAscent.Core.Aplicacao.Apresentadores;
using AeroAscent.Core.Aplicacao.CasosDeUso;
using AeroAscent.Core.Aplicacao.Testes.Fixtures;
using AeroAscent.Core.Dominio.Entidades;
using AeroAscent.Core.Dominio.Enums;
using AeroAscent.Core.Dominio.ObjetosDeValor;
using Xunit;

/// <summary>
/// Testes automatizados para <see cref="ApresentadorOficina"/> cobrindo inicialização, formatação pt-BR,
/// habilitação de botões, compra de upgrades, tratamento de nível máximo e proteção contra spam click.
/// </summary>
public class ApresentadorOficinaTestes
{
    private readonly ProgressoRepositorioMock _repositorioMock;
    private readonly ConsultarOficinaCasoDeUso _consultarCasoDeUso;
    private readonly ComprarMelhoriaCasoDeUso _comprarCasoDeUso;
    private readonly VisaoOficinaFalsa _visaoMock;

    public ApresentadorOficinaTestes()
    {
        var progresso = ProgressoJogador.CriarNovo();
        _repositorioMock = new ProgressoRepositorioMock(progresso);
        _consultarCasoDeUso = new ConsultarOficinaCasoDeUso(_repositorioMock);
        _comprarCasoDeUso = new ComprarMelhoriaCasoDeUso(_repositorioMock);
        _visaoMock = new VisaoOficinaFalsa();
    }

    private ApresentadorOficina CriarApresentador()
    {
        return new ApresentadorOficina(
            _consultarCasoDeUso,
            _comprarCasoDeUso,
            _repositorioMock,
            _visaoMock);
    }

    [Fact]
    public async Task InicializarAsync_DeveConsultarCatalogoEAtualizarVisaoComOsQuatroCartoesMecanicos()
    {
        // Arrange
        using var apresentador = CriarApresentador();

        // Act
        await apresentador.InicializarAsync();

        // Assert
        Assert.Equal(1, _visaoMock.QuantidadeAtualizacoesTela);
        Assert.NotNull(_visaoMock.UltimoModeloRecebido);

        var modelo = _visaoMock.UltimoModeloRecebido.Value;
        Assert.Equal(4, modelo.Cartoes.Count);

        var motor = modelo.Cartoes.First(c => c.Tipo == TipoMelhoria.Motor);
        Assert.Equal("Motor", motor.Titulo);
        Assert.Equal(1, motor.NivelAtual);
        Assert.Equal("Nível 1", motor.TextoNivel);
        Assert.Equal(0.1f, motor.ProgressoNormalizado);

        var aero = modelo.Cartoes.First(c => c.Tipo == TipoMelhoria.Aerodinamica);
        Assert.Equal("Aerodinâmica", aero.Titulo);
        Assert.Equal(1, aero.NivelAtual);

        var tanque = modelo.Cartoes.First(c => c.Tipo == TipoMelhoria.TanqueCombustivel);
        Assert.Equal("Tanque de Combustível", tanque.Titulo);
        Assert.Equal(1, tanque.NivelAtual);

        var catapulta = modelo.Cartoes.First(c => c.Tipo == TipoMelhoria.Catapulta);
        Assert.Equal("Catapulta", catapulta.Titulo);
        Assert.Equal(1, catapulta.NivelAtual);
    }

    [Theory]
    [InlineData(0, "💰 0")]
    [InlineData(500, "💰 500")]
    [InlineData(1250, "💰 1.250")]
    [InlineData(15000, "💰 15.000")]
    [InlineData(1000000, "💰 1.000.000")]
    public async Task InicializarAsync_DeveFormatarSaldoDeMoedasEmPadraoPtBrComSeparadorDeMilharPorPonto(
        long quantidadeMoedas,
        string saldoFormatadoEsperado)
    {
        // Arrange
        var progresso = ProgressoJogador.CriarNovo();
        progresso.CreditarMoedas(new Moeda(quantidadeMoedas));
        _repositorioMock.ProgressoArmazenado = progresso;

        using var apresentador = CriarApresentador();

        // Act
        await apresentador.InicializarAsync();

        // Assert
        var modelo = _visaoMock.UltimoModeloRecebido!.Value;
        Assert.Equal(quantidadeMoedas, modelo.SaldoMoedas);
        Assert.Equal(saldoFormatadoEsperado, modelo.SaldoFormatado);
    }

    [Fact]
    public async Task InicializarAsync_ComSaldoEspecifico_DeveCalcularEstadoPodeComprarDosBotoesCorretamente()
    {
        // Arrange - Saldo de 45 moedas.
        // Custos nível 1: Motor=50, Aerodinâmica=40, Tanque=30, Catapulta=60.
        // Esperado: Tanque (30) e Aerodinâmica (40) habilitados; Motor (50) e Catapulta (60) desabilitados.
        var progresso = ProgressoJogador.CriarNovo();
        progresso.CreditarMoedas(new Moeda(45));
        _repositorioMock.ProgressoArmazenado = progresso;

        using var apresentador = CriarApresentador();

        // Act
        await apresentador.InicializarAsync();

        // Assert
        var modelo = _visaoMock.UltimoModeloRecebido!.Value;
        var motor = modelo.Cartoes.First(c => c.Tipo == TipoMelhoria.Motor);
        var aero = modelo.Cartoes.First(c => c.Tipo == TipoMelhoria.Aerodinamica);
        var tanque = modelo.Cartoes.First(c => c.Tipo == TipoMelhoria.TanqueCombustivel);
        var catapulta = modelo.Cartoes.First(c => c.Tipo == TipoMelhoria.Catapulta);

        Assert.False(motor.PodeComprar);
        Assert.True(aero.PodeComprar);
        Assert.True(tanque.PodeComprar);
        Assert.False(catapulta.PodeComprar);
    }

    [Fact]
    public async Task ProcessarCompraAsync_ComSaldoSuficiente_DeveEvoluirComponenteDeduzirSaldoEEmitirFeedback()
    {
        // Arrange - Saldo de 100 moedas, Motor nível 1 (custo 50)
        var progresso = ProgressoJogador.CriarNovo();
        progresso.CreditarMoedas(new Moeda(100));
        _repositorioMock.ProgressoArmazenado = progresso;

        using var apresentador = CriarApresentador();
        await apresentador.InicializarAsync();

        // Act - Compra o motor
        await apresentador.ProcessarCompraAsync(TipoMelhoria.Motor);

        // Assert
        Assert.Equal(TipoMelhoria.Motor, _visaoMock.UltimoTipoFeedback);
        Assert.Equal(2, _visaoMock.UltimoNivelFeedback);

        var modelo = _visaoMock.UltimoModeloRecebido!.Value;
        Assert.Equal(50, modelo.SaldoMoedas);
        Assert.Equal("💰 50", modelo.SaldoFormatado);

        var motor = modelo.Cartoes.First(c => c.Tipo == TipoMelhoria.Motor);
        Assert.Equal(2, motor.NivelAtual);
        Assert.Equal("Nível 2", motor.TextoNivel);
        Assert.Equal(0.2f, motor.ProgressoNormalizado);
    }

    [Fact]
    public async Task ProcessarCompraAsync_ComSaldoInsuficiente_DeveExibirMensagemErroNaVisaoSemTravar()
    {
        // Arrange - Saldo de 10 moedas, Motor custa 50
        var progresso = ProgressoJogador.CriarNovo();
        progresso.CreditarMoedas(new Moeda(10));
        _repositorioMock.ProgressoArmazenado = progresso;

        using var apresentador = CriarApresentador();
        await apresentador.InicializarAsync();

        // Act - Tenta comprar com saldo insuficiente
        await apresentador.ProcessarCompraAsync(TipoMelhoria.Motor);

        // Assert
        Assert.NotNull(_visaoMock.UltimaMensagemErro);
        Assert.Null(_visaoMock.UltimoTipoFeedback); // Nenhum feedback de sucesso
    }

    [Fact]
    public async Task InicializarAsync_QuandoComponenteEstaNoNivelMaximo_DeveExibirSeloMaximoEBotaoDesabilitado()
    {
        // Arrange - Motor no nível 10
        var progresso = ProgressoJogador.CriarNovo();
        progresso.CreditarMoedas(new Moeda(5000));
        progresso.Aeronave.AtualizarNivel(TipoMelhoria.Motor, 10);
        _repositorioMock.ProgressoArmazenado = progresso;

        using var apresentador = CriarApresentador();

        // Act
        await apresentador.InicializarAsync();

        // Assert
        var modelo = _visaoMock.UltimoModeloRecebido!.Value;
        var motor = modelo.Cartoes.First(c => c.Tipo == TipoMelhoria.Motor);

        Assert.True(motor.EstaNoNivelMaximo);
        Assert.Equal("Nível 10 (MAX)", motor.TextoNivel);
        Assert.Equal(1.0f, motor.ProgressoNormalizado);
        Assert.Equal("MÁXIMO", motor.TextoBotao);
        Assert.Null(motor.CustoProximoNivel);
        Assert.False(motor.PodeComprar);
    }

    [Fact]
    public async Task ProcessarCompraAsync_QuandoOcorreToqueDuploConcorrente_DeveBloquearReentranciaPorSpamClick()
    {
        // Arrange - Saldo de 100 moedas, Motor custa 50
        var progresso = ProgressoJogador.CriarNovo();
        progresso.CreditarMoedas(new Moeda(100));
        _repositorioMock.ProgressoArmazenado = progresso;

        using var apresentador = CriarApresentador();
        await apresentador.InicializarAsync();

        // Act - Dispara duas compras simultaneamente simulando spam click
        var tarefa1 = apresentador.ProcessarCompraAsync(TipoMelhoria.Motor);
        var tarefa2 = apresentador.ProcessarCompraAsync(TipoMelhoria.Motor);

        await Task.WhenAll(tarefa1, tarefa2);

        // Assert - Apenas uma compra deve ter sido executada com sucesso
        var modelo = _visaoMock.UltimoModeloRecebido!.Value;
        Assert.Equal(50, modelo.SaldoMoedas);
        Assert.Equal(2, modelo.Cartoes.First(c => c.Tipo == TipoMelhoria.Motor).NivelAtual);
        Assert.True(_visaoMock.UltimoEstadoInteracao, "A visão deve ter sido reabilitada ao final da transação.");
    }

    [Fact]
    public void SolicitarDecolagem_DeveInvocarOuvinteDoEventoAoSolicitarDecolagem()
    {
        // Arrange
        using var apresentador = CriarApresentador();
        bool eventoDisparado = false;
        apresentador.AoSolicitarDecolagem += () => eventoDisparado = true;

        // Act
        apresentador.SolicitarDecolagem();

        // Assert
        Assert.True(eventoDisparado, "O evento AoSolicitarDecolagem deve ter sido disparado.");
    }

    [Fact]
    public void Visao_QuandoDisparaAoClicarDecolar_DeveRepassarParaAoSolicitarDecolagemNoApresentador()
    {
        // Arrange
        using var apresentador = CriarApresentador();
        int vezesDisparado = 0;
        apresentador.AoSolicitarDecolagem += () => vezesDisparado++;

        // Act - Simula o clique do jogador no botão DECOLAR na visão passiva
        _visaoMock.SimularCliqueDecolar();

        // Assert
        Assert.Equal(1, vezesDisparado);
    }

    [Fact]
    public async Task InicializarAsync_DeveExecutarEmMenosDe5Milissegundos_SC001()
    {
        // Arrange
        using var apresentador = CriarApresentador();

        // Warmup
        await apresentador.InicializarAsync();

        // Act & Measure
        var cronometro = System.Diagnostics.Stopwatch.StartNew();
        await apresentador.InicializarAsync();
        cronometro.Stop();

        // Assert - SC-001: Tempo de processamento e projeção visual < 5ms
        Assert.True(
            cronometro.ElapsedMilliseconds < 5,
            $"Tempo de processamento ({cronometro.ElapsedMilliseconds}ms) excedeu o teto estipulado de 5ms.");
    }
}
