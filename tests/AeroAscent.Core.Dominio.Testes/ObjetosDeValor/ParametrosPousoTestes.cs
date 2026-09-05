namespace AeroAscent.Core.Dominio.Testes.ObjetosDeValor;

using AeroAscent.Core.Dominio.Excecoes;
using AeroAscent.Core.Dominio.ObjetosDeValor;
using Xunit;

/// <summary>
/// Testes unitários para o objeto de valor ParametrosPouso.
/// </summary>
public class ParametrosPousoTestes
{
    [Fact]
    public void CriarPadrao_DeveInicializarComValoresCalibrados()
    {
        // Act
        var parametros = ParametrosPouso.CriarPadrao();

        // Assert
        Assert.Equal(0.3f, parametros.CoeficienteAtritoSolo);
        Assert.Equal(0.15f, parametros.VelocidadeLimiarParada);
        Assert.Equal(15.0f, parametros.TaxaNivelamentoPitchGrausPorSegundo);
    }

    [Theory]
    [InlineData(0.2f, 0.1f, 10f)]
    [InlineData(0.5f, 0.2f, 20f)]
    public void Construtor_ComValoresValidos_DeveAtribuirPropriedades(
        float atrito,
        float limiar,
        float taxaPitch)
    {
        // Act
        var parametros = new ParametrosPouso(atrito, limiar, taxaPitch);

        // Assert
        Assert.Equal(atrito, parametros.CoeficienteAtritoSolo);
        Assert.Equal(limiar, parametros.VelocidadeLimiarParada);
        Assert.Equal(taxaPitch, parametros.TaxaNivelamentoPitchGrausPorSegundo);
    }

    [Theory]
    [InlineData(0f, 0.15f, 15f)]
    [InlineData(-0.1f, 0.15f, 15f)]
    public void Construtor_ComAtritoInvalido_DeveLancarDominioInvalidoException(float atrito, float limiar, float taxa)
    {
        Assert.Throws<DominioInvalidoException>(() => new ParametrosPouso(atrito, limiar, taxa));
    }

    [Theory]
    [InlineData(0.3f, 0f, 15f)]
    [InlineData(0.3f, -0.05f, 15f)]
    public void Construtor_ComLimiarInvalido_DeveLancarDominioInvalidoException(float atrito, float limiar, float taxa)
    {
        Assert.Throws<DominioInvalidoException>(() => new ParametrosPouso(atrito, limiar, taxa));
    }

    [Theory]
    [InlineData(0.3f, 0.15f, 0f)]
    [InlineData(0.3f, 0.15f, -10f)]
    public void Construtor_ComTaxaPitchInvalida_DeveLancarDominioInvalidoException(float atrito, float limiar, float taxa)
    {
        Assert.Throws<DominioInvalidoException>(() => new ParametrosPouso(atrito, limiar, taxa));
    }
}
