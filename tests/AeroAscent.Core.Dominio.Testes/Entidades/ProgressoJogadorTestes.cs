namespace AeroAscent.Core.Dominio.Testes.Entidades;

using AeroAscent.Core.Dominio.Entidades;
using AeroAscent.Core.Dominio.Excecoes;
using AeroAscent.Core.Dominio.ObjetosDeValor;
using Xunit;

/// <summary>
/// Testes unitários para a raiz de agregação ProgressoJogador.
/// </summary>
public class ProgressoJogadorTestes
{
    [Fact]
    public void CriarNovo_DeveInicializarComValoresPadraoERecordesZerados()
    {
        // Act
        var progresso = ProgressoJogador.CriarNovo();

        // Assert
        Assert.NotEqual(Guid.Empty, progresso.Id);
        Assert.NotNull(progresso.Aeronave);
        Assert.Equal(Moeda.Zero, progresso.SaldoMoedas);
        Assert.Equal(0f, progresso.RecordeDistanciaMetros);
        Assert.Equal(0f, progresso.RecordeAltitudeMetros);
        Assert.Equal(0, progresso.TotalVoosRealizados);
    }

    [Fact]
    public void Construtor_ComParametrosValidos_DeveAtribuirCorretamente()
    {
        // Arrange
        var id = Guid.NewGuid();
        var aeronave = new Aeronave(Guid.NewGuid(), 2, 2, 2, 2);
        var saldo = new Moeda(500);

        // Act
        var progresso = new ProgressoJogador(id, aeronave, saldo, 150f, 45f, 3);

        // Assert
        Assert.Equal(id, progresso.Id);
        Assert.Same(aeronave, progresso.Aeronave);
        Assert.Equal(saldo, progresso.SaldoMoedas);
        Assert.Equal(150f, progresso.RecordeDistanciaMetros);
        Assert.Equal(45f, progresso.RecordeAltitudeMetros);
        Assert.Equal(3, progresso.TotalVoosRealizados);
    }

    [Fact]
    public void Construtor_ComIdVazio_DeveLancarDominioInvalidoException()
    {
        // Act & Assert
        Assert.Throws<DominioInvalidoException>(() =>
            new ProgressoJogador(Guid.Empty, Aeronave.CriarPadrao(), Moeda.Zero, 0f, 0f, 0));
    }

    [Fact]
    public void Construtor_ComAeronaveNula_DeveLancarDominioInvalidoException()
    {
        // Act & Assert
        Assert.Throws<DominioInvalidoException>(() =>
            new ProgressoJogador(Guid.NewGuid(), null!, Moeda.Zero, 0f, 0f, 0));
    }

    [Fact]
    public void CreditarEDebitarMoedas_DevemAtualizarSaldoCorretamente()
    {
        // Arrange
        var progresso = ProgressoJogador.CriarNovo();

        // Act 1: Credita 100
        progresso.CreditarMoedas(new Moeda(100));
        Assert.Equal(new Moeda(100), progresso.SaldoMoedas);

        // Act 2: Debita 40
        progresso.DebitarMoedas(new Moeda(40));
        Assert.Equal(new Moeda(60), progresso.SaldoMoedas);
    }

    [Fact]
    public void DebitarMoedas_ComSaldoInsuficiente_DeveLancarSaldoInsuficienteException()
    {
        // Arrange
        var progresso = ProgressoJogador.CriarNovo(); // saldo 0

        // Act & Assert
        Assert.Throws<SaldoInsuficienteException>(() =>
            progresso.DebitarMoedas(new Moeda(50)));
    }

    [Fact]
    public void ProcessarFimDeVoo_DeveCreditarRecompensaEAtualizarRecordesETotalVoos()
    {
        // Arrange
        var progresso = ProgressoJogador.CriarNovo();
        var resultado1 = ResultadoVoo.Calcular(200f, 50f, 10); // 20 + 2 + 10 = 32 moedas

        // Act 1: Primeiro voo
        progresso.ProcessarFimDeVoo(resultado1);

        // Assert 1
        Assert.Equal(new Moeda(32), progresso.SaldoMoedas);
        Assert.Equal(200f, progresso.RecordeDistanciaMetros);
        Assert.Equal(50f, progresso.RecordeAltitudeMetros);
        Assert.Equal(1, progresso.TotalVoosRealizados);

        // Act 2: Segundo voo com maior distância mas menor altitude
        var resultado2 = ResultadoVoo.Calcular(350f, 30f, 5); // 35 + 1 + 5 = 41 moedas
        progresso.ProcessarFimDeVoo(resultado2);

        // Assert 2 (Distância subiu para 350m, altitude permaneceu 50m, saldo acumulou 73)
        Assert.Equal(new Moeda(73), progresso.SaldoMoedas);
        Assert.Equal(350f, progresso.RecordeDistanciaMetros);
        Assert.Equal(50f, progresso.RecordeAltitudeMetros);
        Assert.Equal(2, progresso.TotalVoosRealizados);
    }

    [Fact]
    public void ProcessarFimDeVoo_ComResultadoNulo_DeveLancarDominioInvalidoException()
    {
        // Arrange
        var progresso = ProgressoJogador.CriarNovo();

        // Act & Assert
        Assert.Throws<DominioInvalidoException>(() => progresso.ProcessarFimDeVoo(null!));
    }
}
