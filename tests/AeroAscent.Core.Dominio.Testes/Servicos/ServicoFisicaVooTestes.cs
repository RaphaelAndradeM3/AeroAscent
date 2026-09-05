namespace AeroAscent.Core.Dominio.Testes.Servicos;

using System;
using System.Diagnostics;
using AeroAscent.Core.Dominio.Excecoes;
using AeroAscent.Core.Dominio.ObjetosDeValor;
using AeroAscent.Core.Dominio.Servicos;
using Xunit;

/// <summary>
/// Testes unitários para o cálculo de física cinemática de lançamento da catapulta (ServicoFisicaVoo).
/// </summary>
public class ServicoFisicaVooTestes
{
    private readonly ServicoFisicaVoo _servicoFisica = new();

    [Fact]
    public void CalcularImpulsoInicial_Nivel1Precisao100_DeveRetornarVetor3DCom25MetrosPorSegundoEAngulo35Graus()
    {
        // Arrange
        const int nivelCatapulta = 1;
        const float precisao = 1.0f;
        const float velocidadeEsperada = 25.0f;
        var radianos35 = 35.0f * MathF.PI / 180.0f;
        var yEsperado = velocidadeEsperada * MathF.Sin(radianos35);
        var zEsperado = velocidadeEsperada * MathF.Cos(radianos35);

        // Act
        var impulso = _servicoFisica.CalcularImpulsoInicial(nivelCatapulta, precisao);

        // Assert
        Assert.Equal(0f, impulso.X);
        Assert.InRange(impulso.Y, yEsperado - 0.05f, yEsperado + 0.05f);
        Assert.InRange(impulso.Z, zEsperado - 0.05f, zEsperado + 0.05f);
        Assert.InRange(impulso.Magnitude(), velocidadeEsperada - 0.05f, velocidadeEsperada + 0.05f);
    }

    [Fact]
    public void CalcularImpulsoInicial_Nivel3Precisao100_DeveEscalonarVelocidadeLinearmente()
    {
        // Arrange
        const int nivelCatapulta = 3; // 1 + (3 - 1) * 0.25 = 1.5x
        const float precisao = 1.0f;
        const float velocidadeEsperada = 25.0f * 1.5f; // 37.5 m/s

        // Act
        var impulso = _servicoFisica.CalcularImpulsoInicial(nivelCatapulta, precisao);

        // Assert
        Assert.Equal(0f, impulso.X);
        Assert.InRange(impulso.Magnitude(), velocidadeEsperada - 0.05f, velocidadeEsperada + 0.05f);
    }

    [Fact]
    public void CalcularImpulsoInicial_ComPrecisao50PorCento_DeveReduzirImpulsoProporcionalmente()
    {
        // Arrange
        const int nivelCatapulta = 1;
        const float precisao = 0.5f;
        const float velocidadeEsperada = 25.0f * 0.5f; // 12.5 m/s

        // Act
        var impulso = _servicoFisica.CalcularImpulsoInicial(nivelCatapulta, precisao);

        // Assert
        Assert.InRange(impulso.Magnitude(), velocidadeEsperada - 0.05f, velocidadeEsperada + 0.05f);
    }

    [Fact]
    public void CalcularImpulsoInicial_ComPrecisaoAbaixoDoPiso_DeveAplicarPisoMinimoDe10PorCento()
    {
        // Arrange
        const int nivelCatapulta = 1;
        const float precisaoNula = 0.0f;
        const float velocidadeEsperada = 25.0f * 0.10f; // 2.5 m/s

        // Act
        var impulso = _servicoFisica.CalcularImpulsoInicial(nivelCatapulta, precisaoNula);

        // Assert
        Assert.InRange(impulso.Magnitude(), velocidadeEsperada - 0.05f, velocidadeEsperada + 0.05f);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(11)]
    public void CalcularImpulsoInicial_ComNivelInvalido_DeveLancarDominioInvalidoException(int nivelInvalido)
    {
        // Act & Assert
        Assert.Throws<DominioInvalidoException>(() =>
            _servicoFisica.CalcularImpulsoInicial(nivelInvalido, 1.0f));
    }

    [Fact]
    public void CalcularImpulsoInicial_Benchmark10000Calculos_DeveExecutarEmMenosDe100Milissegundos()
    {
        // Arrange
        var sw = Stopwatch.StartNew();

        // Act
        for (var i = 0; i < 10000; i++)
        {
            var _ = _servicoFisica.CalcularImpulsoInicial(1 + (i % 10), 0.75f);
        }

        sw.Stop();

        // Assert (Critério SC-001)
        Assert.True(sw.ElapsedMilliseconds < 100, $"Tempo de 10.000 cálculos físicos foi de {sw.ElapsedMilliseconds}ms, esperado < 100ms.");
    }
}
