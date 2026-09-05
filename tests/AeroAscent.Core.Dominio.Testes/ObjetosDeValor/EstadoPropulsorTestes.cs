namespace AeroAscent.Core.Dominio.Testes.ObjetosDeValor;

using System;
using AeroAscent.Core.Dominio.Excecoes;
using AeroAscent.Core.Dominio.ObjetosDeValor;
using Xunit;

/// <summary>
/// Testes unitários para o objeto de valor EstadoPropulsor validando invariantes, criação ativo/inativo e limites físicos.
/// </summary>
public class EstadoPropulsorTestes
{
    [Fact]
    public void CriarInativo_ComCombustivelValido_DeveInstanciarPropulsorDesligado()
    {
        // Act
        var propulsor = EstadoPropulsor.CriarInativo(15.0f, 20.0f, 5.0f);

        // Assert
        Assert.False(propulsor.EstaAtivo);
        Assert.Equal(0.0f, propulsor.EmpuxoNewtons);
        Assert.Equal(15.0f, propulsor.CombustivelRestante);
        Assert.Equal(0.75f, propulsor.PercentualRestante);
        Assert.Equal(5.0f, propulsor.TaxaConsumoPorSegundo);
    }

    [Fact]
    public void CriarAtivo_ComCombustivelDisponivel_DeveInstanciarPropulsorLigado()
    {
        // Act
        var propulsor = EstadoPropulsor.CriarAtivo(120.0f, 10.0f, 20.0f, 5.0f);

        // Assert
        Assert.True(propulsor.EstaAtivo);
        Assert.Equal(120.0f, propulsor.EmpuxoNewtons);
        Assert.Equal(10.0f, propulsor.CombustivelRestante);
        Assert.Equal(0.5f, propulsor.PercentualRestante);
        Assert.Equal(5.0f, propulsor.TaxaConsumoPorSegundo);
    }

    [Fact]
    public void CriarAtivo_ComCombustivelZero_DeveForcarInativoEEmpuxoZero()
    {
        // Act
        var propulsor = EstadoPropulsor.CriarAtivo(120.0f, 0.0f, 20.0f, 5.0f);

        // Assert
        Assert.False(propulsor.EstaAtivo);
        Assert.Equal(0.0f, propulsor.EmpuxoNewtons);
        Assert.Equal(0.0f, propulsor.CombustivelRestante);
        Assert.Equal(0.0f, propulsor.PercentualRestante);
    }

    [Fact]
    public void Construtor_ComEmpuxoNegativo_DeveLancarExcecao()
    {
        // Act & Assert
        Assert.Throws<DominioInvalidoException>(() =>
            new EstadoPropulsor(true, -10.0f, 10.0f, 0.5f, 5.0f));
    }

    [Fact]
    public void Construtor_ComCombustivelNegativo_DeveLancarExcecao()
    {
        // Act & Assert
        Assert.Throws<DominioInvalidoException>(() =>
            new EstadoPropulsor(true, 120.0f, -5.0f, 0.5f, 5.0f));
    }

    [Fact]
    public void Construtor_ComTaxaConsumoNegativa_DeveLancarExcecao()
    {
        // Act & Assert
        Assert.Throws<DominioInvalidoException>(() =>
            new EstadoPropulsor(true, 120.0f, 10.0f, 0.5f, -1.0f));
    }

    [Fact]
    public void PercentualRestante_DeveSerClampedEntreZeroEUm()
    {
        // Act
        var propulsorA = new EstadoPropulsor(true, 100f, 10f, 1.5f, 5f);
        var propulsorB = new EstadoPropulsor(true, 100f, 10f, -0.2f, 5f);

        // Assert
        Assert.Equal(1.0f, propulsorA.PercentualRestante);
        Assert.Equal(0.0f, propulsorB.PercentualRestante);
    }
}
