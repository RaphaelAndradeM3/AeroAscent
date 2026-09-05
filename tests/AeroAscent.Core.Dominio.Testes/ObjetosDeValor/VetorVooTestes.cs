namespace AeroAscent.Core.Dominio.Testes.ObjetosDeValor;

using AeroAscent.Core.Dominio.ObjetosDeValor;
using Xunit;

/// <summary>
/// Testes unitários para o objeto de valor tridimensional VetorVoo.
/// </summary>
public class VetorVooTestes
{
    [Fact]
    public void Construtor_DeveAtribuirComponentesCorretamente()
    {
        // Act
        var v = new VetorVoo(1f, 2f, 3f);

        // Assert
        Assert.Equal(1f, v.X);
        Assert.Equal(2f, v.Y);
        Assert.Equal(3f, v.Z);
    }

    [Fact]
    public void ConstantesEstaticas_DevemPossuirValoresEsperados()
    {
        // Assert
        Assert.Equal(new VetorVoo(0f, 0f, 0f), VetorVoo.Zero);
        Assert.Equal(new VetorVoo(0f, 1f, 0f), VetorVoo.ParaCima);
        Assert.Equal(new VetorVoo(0f, 0f, 1f), VetorVoo.ParaFrente);
        Assert.Equal(new VetorVoo(1f, 0f, 0f), VetorVoo.ParaDireita);
    }

    [Fact]
    public void Somar_DeveRetornarSomaComponenteAComponente()
    {
        // Arrange
        var v1 = new VetorVoo(10f, 20f, 30f);
        var v2 = new VetorVoo(5f, 15f, 25f);

        // Act
        var resultado = v1.Somar(v2);
        var resultadoOperador = v1 + v2;

        // Assert
        Assert.Equal(new VetorVoo(15f, 35f, 55f), resultado);
        Assert.Equal(resultado, resultadoOperador);
    }

    [Fact]
    public void Subtrair_DeveRetornarDiferencaComponenteAComponente()
    {
        // Arrange
        var v1 = new VetorVoo(10f, 20f, 30f);
        var v2 = new VetorVoo(3f, 5f, 10f);

        // Act
        var resultado = v1.Subtrair(v2);
        var resultadoOperador = v1 - v2;

        // Assert
        Assert.Equal(new VetorVoo(7f, 15f, 20f), resultado);
        Assert.Equal(resultado, resultadoOperador);
    }

    [Fact]
    public void MultiplicarPorEscalar_DeveMultiplicarTodasComponentes()
    {
        // Arrange
        var v = new VetorVoo(2f, -3f, 4f);

        // Act
        var resultado = v.Multiplicar(2.5f);
        var resultadoOperador1 = v * 2.5f;
        var resultadoOperador2 = 2.5f * v;

        // Assert
        Assert.Equal(new VetorVoo(5f, -7.5f, 10f), resultado);
        Assert.Equal(resultado, resultadoOperador1);
        Assert.Equal(resultado, resultadoOperador2);
    }

    [Fact]
    public void DividirPorEscalar_DeveDividirTodasComponentes()
    {
        // Arrange
        var v = new VetorVoo(10f, 20f, 30f);

        // Act
        var resultado = v.Dividir(2f);
        var resultadoOperador = v / 2f;

        // Assert
        Assert.Equal(new VetorVoo(5f, 10f, 15f), resultado);
        Assert.Equal(resultado, resultadoOperador);
    }

    [Fact]
    public void DividirPorZero_DeveRetornarVetorComInfinitosOuZeroSeguro()
    {
        // Arrange
        var v = new VetorVoo(10f, 20f, 30f);

        // Act
        var resultado = v.Dividir(0f);

        // Assert
        Assert.Equal(VetorVoo.Zero, resultado);
    }

    [Fact]
    public void Magnitude_DeveCalcularNormaEuclidianaCorreta()
    {
        // Arrange (Triângulo 3-4 no plano XY com Z=0 => magnitude 5)
        var v = new VetorVoo(3f, 4f, 0f);

        // Act
        var magnitude = v.Magnitude();
        var magnitudeAoQuadrado = v.MagnitudeAoQuadrado();

        // Assert
        Assert.Equal(5f, magnitude);
        Assert.Equal(25f, magnitudeAoQuadrado);
    }

    [Fact]
    public void Normalizar_DeveRetornarVetorUnitarioComMesmaDirecao()
    {
        // Arrange
        var v = new VetorVoo(0f, 10f, 0f);

        // Act
        var normalizado = v.Normalizar();

        // Assert
        Assert.Equal(new VetorVoo(0f, 1f, 0f), normalizado);
        Assert.Equal(1f, normalizado.Magnitude());
    }

    [Fact]
    public void Normalizar_VetorZero_DeveRetornarVetorZero()
    {
        // Arrange
        var v = VetorVoo.Zero;

        // Act
        var normalizado = v.Normalizar();

        // Assert
        Assert.Equal(VetorVoo.Zero, normalizado);
    }
}
