namespace AeroAscent.Core.Dominio.Testes.Comum;

using System;
using AeroAscent.Core.Dominio.Comum;
using AeroAscent.Core.Dominio.Entidades;
using AeroAscent.Core.Dominio.Excecoes;
using AeroAscent.Core.Dominio.ObjetosDeValor;
using Xunit;

/// <summary>
/// Testes unitários para o GerenciadorPoolObjetos genérico.
/// </summary>
public class GerenciadorPoolObjetosTestes
{
    [Fact]
    public void Construtor_ComParametrosValidos_DeveInicializarComCapacidadeInformada()
    {
        // Arrange & Act
        var pool = new GerenciadorPoolObjetos<Coletavel>(
            () => Coletavel.CriarMoeda(VetorVoo.Zero),
            capacidadeInicial: 10);

        // Assert
        Assert.Equal(10, pool.CapacidadeTotal);
        Assert.Equal(10, pool.DisponiveisEmEstoque);
        Assert.Equal(0, pool.EmUso);
    }

    [Fact]
    public void Construtor_ComFabricaNulaOuCapacidadeNegativa_DeveLancarDominioInvalidoException()
    {
        // Act & Assert
        Assert.Throws<DominioInvalidoException>(() =>
            new GerenciadorPoolObjetos<Coletavel>(null!, 10));

        Assert.Throws<DominioInvalidoException>(() =>
            new GerenciadorPoolObjetos<Coletavel>(() => Coletavel.CriarMoeda(VetorVoo.Zero), -1));
    }

    [Fact]
    public void Obter_DeveRetirarItemDoEstoqueEIncrementarContadorEmUso()
    {
        // Arrange
        var pool = new GerenciadorPoolObjetos<Coletavel>(
            () => Coletavel.CriarMoeda(VetorVoo.Zero),
            capacidadeInicial: 5);

        // Act
        var item = pool.Obter();

        // Assert
        Assert.NotNull(item);
        Assert.Equal(5, pool.CapacidadeTotal);
        Assert.Equal(4, pool.DisponiveisEmEstoque);
        Assert.Equal(1, pool.EmUso);
    }

    [Fact]
    public void Liberar_DeveDevolverItemAoEstoqueEDecrementarContadorEmUso()
    {
        // Arrange
        var pool = new GerenciadorPoolObjetos<Coletavel>(
            () => Coletavel.CriarMoeda(VetorVoo.Zero),
            capacidadeInicial: 5);

        var item = pool.Obter();
        Assert.Equal(1, pool.EmUso);

        // Act
        pool.Liberar(item);

        // Assert
        Assert.Equal(5, pool.CapacidadeTotal);
        Assert.Equal(5, pool.DisponiveisEmEstoque);
        Assert.Equal(0, pool.EmUso);
    }

    [Fact]
    public void Obter_QuandoEstoqueEsgotado_DeveExpandirElasticamenteSemFalhar()
    {
        // Arrange: Pool com capacidade para apenas 2 itens
        var pool = new GerenciadorPoolObjetos<Coletavel>(
            () => Coletavel.CriarMoeda(VetorVoo.Zero),
            capacidadeInicial: 2);

        var item1 = pool.Obter();
        var item2 = pool.Obter();
        Assert.Equal(0, pool.DisponiveisEmEstoque);

        // Act: Requisita terceiro item em pico
        var itemExtra = pool.Obter();

        // Assert: Não lança exceção e expande capacidade total
        Assert.NotNull(itemExtra);
        Assert.Equal(3, pool.CapacidadeTotal);
        Assert.Equal(0, pool.DisponiveisEmEstoque);
        Assert.Equal(3, pool.EmUso);
    }

    [Fact]
    public void Liberar_ComItemNulo_DeveLancarDominioInvalidoException()
    {
        // Arrange
        var pool = new GerenciadorPoolObjetos<Coletavel>(
            () => Coletavel.CriarMoeda(VetorVoo.Zero),
            capacidadeInicial: 2);

        // Act & Assert
        Assert.Throws<DominioInvalidoException>(() => pool.Liberar(null!));
    }

    [Fact]
    public void Limpar_DeveZerarEstoqueECapacidade()
    {
        // Arrange
        var pool = new GerenciadorPoolObjetos<Coletavel>(
            () => Coletavel.CriarMoeda(VetorVoo.Zero),
            capacidadeInicial: 5);

        // Act
        pool.Limpar();

        // Assert
        Assert.Equal(0, pool.CapacidadeTotal);
        Assert.Equal(0, pool.DisponiveisEmEstoque);
        Assert.Equal(0, pool.EmUso);
    }
}
