namespace AeroAscent.Core.Dominio.Testes.Entidades;

using AeroAscent.Core.Dominio.Entidades;
using AeroAscent.Core.Dominio.Enums;
using AeroAscent.Core.Dominio.Excecoes;
using Xunit;

/// <summary>
/// Testes unitários para a entidade Voo e sua máquina de estados.
/// </summary>
public class VooTestes
{
    [Fact]
    public void CriarSessaoVoo_DeveInicializarEmPreparacaoComMetricasZeradas()
    {
        // Arrange
        var aeronave = Aeronave.CriarPadrao();

        // Act
        var voo = Voo.Iniciar(aeronave);

        // Assert
        Assert.NotEqual(Guid.Empty, voo.Id);
        Assert.Same(aeronave, voo.Aeronave);
        Assert.Equal(StatusVoo.EmPreparacao, voo.Status);
        Assert.Equal(0f, voo.DistanciaPercorrida);
        Assert.Equal(0f, voo.AltitudeMaxima);
        Assert.Equal(0, voo.MoedasColetadas);
        Assert.Null(voo.Resultado);
    }

    [Fact]
    public void Iniciar_ComAeronaveNula_DeveLancarDominioInvalidoException()
    {
        // Act & Assert
        Assert.Throws<DominioInvalidoException>(() => Voo.Iniciar(null!));
    }

    [Fact]
    public void Decolar_EmPreparacao_DeveTransitarParaEmVoo()
    {
        // Arrange
        var voo = Voo.Iniciar(Aeronave.CriarPadrao());

        // Act
        voo.Decolar();

        // Assert
        Assert.Equal(StatusVoo.EmVoo, voo.Status);
    }

    [Fact]
    public void Decolar_QuandoJaEmVoo_DeveLancarDominioInvalidoException()
    {
        // Arrange
        var voo = Voo.Iniciar(Aeronave.CriarPadrao());
        voo.Decolar();

        // Act & Assert
        Assert.Throws<DominioInvalidoException>(() => voo.Decolar());
    }

    [Fact]
    public void AtualizarMetricas_EmVoo_DeveAcumularDistanciaEAltitudeHistorica()
    {
        // Arrange
        var voo = Voo.Iniciar(Aeronave.CriarPadrao());
        voo.Decolar();

        // Act 1: Voo sobe até 50m e percorre 100m com 5 moedas
        voo.AtualizarMetricas(100f, 50f, 5);

        // Assert 1
        Assert.Equal(100f, voo.DistanciaPercorrida);
        Assert.Equal(50f, voo.AltitudeMaxima);
        Assert.Equal(5, voo.MoedasColetadas);

        // Act 2: Voo desce para 30m, atinge 200m e pega mais 3 moedas
        voo.AtualizarMetricas(200f, 30f, 3);

        // Assert 2 (Altitude máxima deve permanecer 50m)
        Assert.Equal(200f, voo.DistanciaPercorrida);
        Assert.Equal(50f, voo.AltitudeMaxima);
        Assert.Equal(8, voo.MoedasColetadas);
    }

    [Fact]
    public void AtualizarMetricas_ForaDeEmVoo_DeveLancarDominioInvalidoException()
    {
        // Arrange (ainda EmPreparacao)
        var voo = Voo.Iniciar(Aeronave.CriarPadrao());

        // Act & Assert
        Assert.Throws<DominioInvalidoException>(() => voo.AtualizarMetricas(10f, 5f, 1));
    }

    [Fact]
    public void Pousar_EmVoo_DeveTransitarParaPousadoEGearResultadoVoo()
    {
        // Arrange
        var voo = Voo.Iniciar(Aeronave.CriarPadrao());
        voo.Decolar();
        voo.AtualizarMetricas(200f, 60f, 10);

        // Act
        var resultado = voo.Pousar();

        // Assert
        Assert.Equal(StatusVoo.Pousado, voo.Status);
        Assert.NotNull(voo.Resultado);
        Assert.Same(resultado, voo.Resultado);
        Assert.Equal(200f, resultado.DistanciaMetros);
        Assert.Equal(60f, resultado.AltitudeMaximaMetros);
        Assert.Equal(10, resultado.MoedasColetadas);
        // Formula: 200 * 0.1 = 20 + 60 * 0.05 = 3 + 10 = 33 moedas
        Assert.Equal(33, resultado.MoedasRecompensaTotal.Quantidade);
    }

    [Fact]
    public void Cancelar_EmPreparacaoOuEmVoo_DeveTransitarParaCanceladoSemResultado()
    {
        // Arrange
        var voo = Voo.Iniciar(Aeronave.CriarPadrao());

        // Act
        voo.Cancelar();

        // Assert
        Assert.Equal(StatusVoo.Cancelado, voo.Status);
        Assert.Null(voo.Resultado);
    }

    [Fact]
    public void AcoesAposPousoOuCancelamento_DevemSerBloqueadas()
    {
        // Arrange
        var voo = Voo.Iniciar(Aeronave.CriarPadrao());
        voo.Decolar();
        voo.Pousar();

        // Act & Assert
        Assert.Throws<DominioInvalidoException>(() => voo.AtualizarMetricas(300f, 10f, 1));
        Assert.Throws<DominioInvalidoException>(() => voo.Pousar());
        Assert.Throws<DominioInvalidoException>(() => voo.Cancelar());
    }

    [Theory]
    [InlineData(1, 20.0f)]
    [InlineData(2, 25.0f)]
    [InlineData(3, 30.0f)]
    [InlineData(5, 40.0f)]
    [InlineData(10, 65.0f)]
    public void Iniciar_ComNiveisDiferentesDeTanque_DeveInicializarCombustivelComCapacidadeEscalonada(int nivelTanque, float capacidadeEsperada)
    {
        // Arrange
        var aeronave = new Aeronave(Guid.NewGuid(), nivelMotor: 1, nivelAerodinamica: 1, nivelTanqueCombustivel: nivelTanque, nivelCatapulta: 1);

        // Act
        var voo = Voo.Iniciar(aeronave);

        // Assert
        Assert.Equal(capacidadeEsperada, voo.Combustivel.CapacidadeMaxima, precision: 2);
        Assert.Equal(capacidadeEsperada, voo.Combustivel.QuantidadeAtual, precision: 2);
        Assert.Equal(5.0f, voo.Combustivel.TaxaQueimaPorSegundo, precision: 2);
        Assert.False(voo.Combustivel.EstaVazio);
    }
}
