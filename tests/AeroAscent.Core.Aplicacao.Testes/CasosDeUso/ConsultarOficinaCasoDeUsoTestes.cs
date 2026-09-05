namespace AeroAscent.Core.Aplicacao.Testes.CasosDeUso;

using System;
using System.Linq;
using System.Threading.Tasks;
using AeroAscent.Core.Aplicacao.CasosDeUso;
using AeroAscent.Core.Aplicacao.Testes.Fixtures;
using AeroAscent.Core.Dominio.Entidades;
using AeroAscent.Core.Dominio.Enums;
using AeroAscent.Core.Dominio.Excecoes;
using AeroAscent.Core.Dominio.ObjetosDeValor;
using Xunit;

/// <summary>
/// Testes automatizados para o caso de uso <see cref="ConsultarOficinaCasoDeUso"/>.
/// </summary>
public class ConsultarOficinaCasoDeUsoTestes
{
    [Fact]
    public async Task ExecutarAsync_ComProgressoPadraoESaldoZero_DeveRetornarCatalogoCompletoComPodeComprarFalso()
    {
        // Arrange
        var progresso = ProgressoJogador.CriarNovo(); // Saldo 0, todos nível 1
        var repositorio = new ProgressoRepositorioMock(progresso);
        var casoDeUso = new ConsultarOficinaCasoDeUso(repositorio);

        // Act
        var itens = await casoDeUso.ExecutarAsync();

        // Assert
        Assert.NotNull(itens);
        Assert.Equal(4, itens.Count);

        var motor = itens.First(i => i.Tipo == TipoMelhoria.Motor);
        Assert.Equal("Motor", motor.NomeAmigavel);
        Assert.Equal(1, motor.NivelAtual);
        Assert.Equal(50, motor.CustoProximoNivel?.Quantidade);
        Assert.False(motor.PodeComprar);
        Assert.False(motor.EstaNoNivelMaximo);

        var aero = itens.First(i => i.Tipo == TipoMelhoria.Aerodinamica);
        Assert.Equal("Aerodinâmica", aero.NomeAmigavel);
        Assert.Equal(1, aero.NivelAtual);
        Assert.Equal(40, aero.CustoProximoNivel?.Quantidade);
        Assert.False(aero.PodeComprar);
        Assert.False(aero.EstaNoNivelMaximo);

        var tanque = itens.First(i => i.Tipo == TipoMelhoria.TanqueCombustivel);
        Assert.Equal("Tanque de Combustível", tanque.NomeAmigavel);
        Assert.Equal(1, tanque.NivelAtual);
        Assert.Equal(30, tanque.CustoProximoNivel?.Quantidade);
        Assert.False(tanque.PodeComprar);
        Assert.False(tanque.EstaNoNivelMaximo);

        var catapulta = itens.First(i => i.Tipo == TipoMelhoria.Catapulta);
        Assert.Equal("Catapulta", catapulta.NomeAmigavel);
        Assert.Equal(1, catapulta.NivelAtual);
        Assert.Equal(60, catapulta.CustoProximoNivel?.Quantidade);
        Assert.False(catapulta.PodeComprar);
        Assert.False(catapulta.EstaNoNivelMaximo);
    }

    [Fact]
    public async Task ExecutarAsync_ComSaldoSuficienteParaAlgunsItens_DeveSinalizarPodeComprarCorretamente()
    {
        // Arrange - Saldo de 45 moedas (pode comprar Tanque por 30 ou Aerodinâmica por 40, mas não Motor 50 nem Catapulta 60)
        var progresso = ProgressoJogador.CriarNovo();
        progresso.CreditarMoedas(new Moeda(45));
        var repositorio = new ProgressoRepositorioMock(progresso);
        var casoDeUso = new ConsultarOficinaCasoDeUso(repositorio);

        // Act
        var itens = await casoDeUso.ExecutarAsync();

        // Assert
        var motor = itens.First(i => i.Tipo == TipoMelhoria.Motor);
        var aero = itens.First(i => i.Tipo == TipoMelhoria.Aerodinamica);
        var tanque = itens.First(i => i.Tipo == TipoMelhoria.TanqueCombustivel);
        var catapulta = itens.First(i => i.Tipo == TipoMelhoria.Catapulta);

        Assert.False(motor.PodeComprar);
        Assert.True(aero.PodeComprar);
        Assert.True(tanque.PodeComprar);
        Assert.False(catapulta.PodeComprar);
    }

    [Fact]
    public async Task ExecutarAsync_QuandoComponenteAtingeNivelMaximo10_DeveExibirStatusDeclarativoCorreto()
    {
        // Arrange - Motor e Catapulta no nível 10
        var aeronave = new Aeronave(Guid.NewGuid(), nivelMotor: 10, nivelAerodinamica: 3, nivelTanqueCombustivel: 2, nivelCatapulta: 10);
        var progresso = new ProgressoJogador(Guid.NewGuid(), aeronave, new Moeda(100000), 0f, 0f, 0);
        var repositorio = new ProgressoRepositorioMock(progresso);
        var casoDeUso = new ConsultarOficinaCasoDeUso(repositorio);

        // Act
        var itens = await casoDeUso.ExecutarAsync();

        // Assert
        var motor = itens.First(i => i.Tipo == TipoMelhoria.Motor);
        Assert.Equal(10, motor.NivelAtual);
        Assert.Null(motor.CustoProximoNivel);
        Assert.True(motor.EstaNoNivelMaximo);
        Assert.False(motor.PodeComprar); // Mesmo com saldo infinito, no nível 10 não pode comprar

        var catapulta = itens.First(i => i.Tipo == TipoMelhoria.Catapulta);
        Assert.Equal(10, catapulta.NivelAtual);
        Assert.Null(catapulta.CustoProximoNivel);
        Assert.True(catapulta.EstaNoNivelMaximo);
        Assert.False(catapulta.PodeComprar);

        var aero = itens.First(i => i.Tipo == TipoMelhoria.Aerodinamica);
        Assert.Equal(3, aero.NivelAtual);
        Assert.NotNull(aero.CustoProximoNivel);
        Assert.False(aero.EstaNoNivelMaximo);
        Assert.True(aero.PodeComprar);
    }

    [Fact]
    public async Task ExecutarAsync_QuandoRepositorioRetornaNulo_DeveInstanciarCatalogoInicialResiliente()
    {
        // Arrange - Primeira execução sem arquivo salvo
        var repositorio = new ProgressoRepositorioMock(null);
        var casoDeUso = new ConsultarOficinaCasoDeUso(repositorio);

        // Act
        var itens = await casoDeUso.ExecutarAsync();

        // Assert
        Assert.NotNull(itens);
        Assert.Equal(4, itens.Count);
        Assert.All(itens, item =>
        {
            Assert.Equal(1, item.NivelAtual);
            Assert.False(item.PodeComprar);
            Assert.False(item.EstaNoNivelMaximo);
            Assert.NotNull(item.CustoProximoNivel);
        });
    }

    [Fact]
    public void Construtor_ComRepositorioNulo_DeveLancarDominioInvalidoException()
    {
        // Act & Assert
        Assert.Throws<DominioInvalidoException>(() => new ConsultarOficinaCasoDeUso(null!));
    }

    [Fact]
    public async Task FluxoIntegrado_ConsultaInicial_CompraMelhoria_NovaConsultaComNiveisECustosAtualizados()
    {
        // Arrange - Jogador com 200 moedas
        var progresso = ProgressoJogador.CriarNovo();
        progresso.CreditarMoedas(new Moeda(200));
        var repositorio = new ProgressoRepositorioMock(progresso);

        var casoDeUsoConsulta = new ConsultarOficinaCasoDeUso(repositorio);
        var casoDeUsoCompra = new ComprarMelhoriaCasoDeUso(repositorio);

        // Act 1: Consulta inicial
        var itensIniciais = await casoDeUsoConsulta.ExecutarAsync();
        var motorInicial = itensIniciais.First(i => i.Tipo == TipoMelhoria.Motor);
        Assert.Equal(1, motorInicial.NivelAtual);
        Assert.Equal(50, motorInicial.CustoProximoNivel?.Quantidade);
        Assert.True(motorInicial.PodeComprar);

        // Act 2: Comprar Motor (50 moedas)
        var resultadoCompra = await casoDeUsoCompra.ExecutarAsync(TipoMelhoria.Motor);
        Assert.Equal(2, resultadoCompra.NovoNivel);
        Assert.Equal(150, resultadoCompra.SaldoRestante.Quantidade);

        // Act 3: Nova consulta após a compra
        var itensAposCompra = await casoDeUsoConsulta.ExecutarAsync();
        var motorApos = itensAposCompra.First(i => i.Tipo == TipoMelhoria.Motor);

        // Assert
        Assert.Equal(2, motorApos.NivelAtual);
        Assert.Equal(75, motorApos.CustoProximoNivel?.Quantidade); // N2 -> N3: floor(50 * 1.5) = 75
        Assert.True(motorApos.PodeComprar); // 150 moedas >= 75
        Assert.False(motorApos.EstaNoNivelMaximo);

        // Comprar motor mais uma vez (75 moedas -> saldo restante 75)
        await casoDeUsoCompra.ExecutarAsync(TipoMelhoria.Motor);

        // Nova consulta
        var itensAposSegundaCompra = await casoDeUsoConsulta.ExecutarAsync();
        var motorSegunda = itensAposSegundaCompra.First(i => i.Tipo == TipoMelhoria.Motor);
        Assert.Equal(3, motorSegunda.NivelAtual);
        Assert.Equal(112, motorSegunda.CustoProximoNivel?.Quantidade); // N3 -> N4: floor(50 * 1.5^2) = 112
        Assert.False(motorSegunda.PodeComprar); // 75 moedas < 112 moedas
    }
}
