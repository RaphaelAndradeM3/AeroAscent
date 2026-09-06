namespace AeroAscent.Core.Aplicacao.Testes.DTOs;

using AeroAscent.Core.Aplicacao.DTOs;
using AeroAscent.Core.Dominio.Excecoes;
using Xunit;

/// <summary>
/// Suíte de testes unitários para validação de integridade e regras de negócio do DTO TelemetriaHUDDTO.
/// </summary>
public class TelemetriaHUDDTOTestes
{
    [Fact]
    public void Criar_ComParametrosValidos_DeveAtribuirPropriedadesCorretamente()
    {
        // Act
        var dto = new TelemetriaHUDDTO(
            distanciaPercorridaMetros: 150.5f,
            recordeDistanciaMetros: 200.0f,
            altitudeAtualMetros: 42.0f,
            velocidadeAtualMetrosPorSegundo: 25.0f,
            percentualCombustivel: 0.8f,
            moedasColetadas: 5,
            recordeSuperado: false,
            boostDisponivel: true);

        // Assert
        Assert.Equal(150.5f, dto.DistanciaPercorridaMetros);
        Assert.Equal(200.0f, dto.RecordeDistanciaMetros);
        Assert.Equal(42.0f, dto.AltitudeAtualMetros);
        Assert.Equal(25.0f, dto.VelocidadeAtualMetrosPorSegundo);
        Assert.Equal(0.8f, dto.PercentualCombustivel);
        Assert.Equal(5, dto.MoedasColetadas);
        Assert.False(dto.RecordeSuperado);
        Assert.True(dto.BoostDisponivel);
    }

    [Theory]
    [InlineData(-0.1f, 100f, 10f, 10f, 0.5f, 0)]
    [InlineData(100f, -0.1f, 10f, 10f, 0.5f, 0)]
    [InlineData(100f, 100f, -0.1f, 10f, 0.5f, 0)]
    [InlineData(100f, 100f, 10f, -0.1f, 0.5f, 0)]
    [InlineData(100f, 100f, 10f, 10f, 0.5f, -1)]
    public void Criar_ComValoresNegativosInvalidos_DeveLancarDominioInvalidoException(
        float distancia,
        float recorde,
        float altitude,
        float velocidade,
        float combustivel,
        int moedas)
    {
        // Assert & Act
        Assert.Throws<DominioInvalidoException>(() => new TelemetriaHUDDTO(
            distancia,
            recorde,
            altitude,
            velocidade,
            combustivel,
            moedas,
            false,
            true));
    }

    [Fact]
    public void Criar_ComCombustivelForaDoIntervalo_DeveAplicarClampEntreZeroEUm()
    {
        // Act
        var dtoMenorQueZero = new TelemetriaHUDDTO(10f, 10f, 10f, 10f, -0.5f, 0, false, false);
        var dtoMaiorQueUm = new TelemetriaHUDDTO(10f, 10f, 10f, 10f, 1.5f, 0, false, false);

        // Assert
        Assert.Equal(0.0f, dtoMenorQueZero.PercentualCombustivel);
        Assert.Equal(1.0f, dtoMaiorQueUm.PercentualCombustivel);
    }

    [Fact]
    public void IgualdadePorValor_ComMesmosValores_DeveSerIgual()
    {
        // Arrange
        var dto1 = new TelemetriaHUDDTO(100f, 200f, 30f, 15f, 0.5f, 3, false, true);
        var dto2 = new TelemetriaHUDDTO(100f, 200f, 30f, 15f, 0.5f, 3, false, true);

        // Assert
        Assert.Equal(dto1, dto2);
        Assert.True(dto1 == dto2);
    }
}
