namespace AeroAscent.Core.Dominio.Testes.ObjetosDeValor;

using AeroAscent.Core.Dominio.Excecoes;
using AeroAscent.Core.Dominio.ObjetosDeValor;
using Xunit;

/// <summary>
/// Testes unitários para o objeto de valor Moeda.
/// </summary>
public class MoedaTestes
{
    [Fact]
    public void CriarMoeda_ComValorPositivo_DeveCriarComSucesso()
    {
        // Act
        var moeda = new Moeda(150);

        // Assert
        Assert.Equal(150, moeda.Quantidade);
    }

    [Fact]
    public void CriarMoeda_ComZero_DeveCriarComSucesso()
    {
        // Act
        var moeda = Moeda.Zero;

        // Assert
        Assert.Equal(0, moeda.Quantidade);
    }

    [Fact]
    public void CriarMoeda_ComValorNegativo_DeveLancarDominioInvalidoException()
    {
        // Act & Assert
        Assert.Throws<DominioInvalidoException>(() => new Moeda(-10));
    }

    [Fact]
    public void Adicionar_DeveSomarValoresCorretamente()
    {
        // Arrange
        var m1 = new Moeda(100);
        var m2 = new Moeda(50);

        // Act
        var resultado = m1.Adicionar(m2);

        // Assert
        Assert.Equal(150, resultado.Quantidade);
        Assert.Equal(150, (m1 + m2).Quantidade);
    }

    [Fact]
    public void Subtrair_ComSaldoSuficiente_DeveRetornarDiferenca()
    {
        // Arrange
        var m1 = new Moeda(100);
        var m2 = new Moeda(40);

        // Act
        var resultado = m1.Subtrair(m2);

        // Assert
        Assert.Equal(60, resultado.Quantidade);
        Assert.Equal(60, (m1 - m2).Quantidade);
    }

    [Fact]
    public void Subtrair_ComSaldoInsuficiente_DeveLancarSaldoInsuficienteException()
    {
        // Arrange
        var saldo = new Moeda(50);
        var custo = new Moeda(100);

        // Act & Assert
        var ex = Assert.Throws<SaldoInsuficienteException>(() => saldo.Subtrair(custo));
        Assert.Equal(50, ex.SaldoAtual);
        Assert.Equal(100, ex.QuantiaNecessaria);
    }

    [Fact]
    public void Comparadores_DevemFuncionarCorretamente()
    {
        // Arrange
        var menor = new Moeda(10);
        var maior = new Moeda(20);
        var igual = new Moeda(10);

        // Assert
        Assert.True(menor < maior);
        Assert.True(maior > menor);
        Assert.True(menor <= igual);
        Assert.True(menor >= igual);
        Assert.Equal(menor, igual);
    }

    [Fact]
    public void Adicionar_ComOverflow_DeveLancarOverflowException()
    {
        // Arrange
        var max = new Moeda(long.MaxValue);
        var extra = new Moeda(1);

        // Act & Assert
        Assert.Throws<OverflowException>(() => max.Adicionar(extra));
    }
}
