namespace AeroAscent.Core.Aplicacao.Testes.CasosDeUso;

using System;
using System.Collections.Generic;
using AeroAscent.Core.Aplicacao.CasosDeUso;
using AeroAscent.Core.Dominio.Comum;
using AeroAscent.Core.Dominio.Entidades;
using AeroAscent.Core.Dominio.Enums;
using AeroAscent.Core.Dominio.ObjetosDeValor;
using Xunit;

/// <summary>
/// Testes de integração para o caso de uso ProcessarColetaveisVooCasoDeUso.
/// </summary>
public class ProcessarColetaveisVooCasoDeUsoTestes
{
    private readonly ProcessarColetaveisVooCasoDeUso _casoDeUso;
    private readonly IPoolObjetos<Coletavel> _poolMoedas;
    private readonly IPoolObjetos<Coletavel> _poolAneis;

    public ProcessarColetaveisVooCasoDeUsoTestes()
    {
        _casoDeUso = new ProcessarColetaveisVooCasoDeUso();
        _poolMoedas = new GerenciadorPoolObjetos<Coletavel>(
            () => Coletavel.CriarMoeda(VetorVoo.Zero),
            capacidadeInicial: 20);

        _poolAneis = new GerenciadorPoolObjetos<Coletavel>(
            () => Coletavel.CriarAnelVento(VetorVoo.Zero),
            capacidadeInicial: 10);
    }

    [Fact]
    public void Executar_ComMoedaNaTrajetoria_DeveColetarEIncrementarMoedasNaEntidadeVoo()
    {
        // Arrange
        var aero = Aeronave.CriarPadrao();
        var voo = Voo.Iniciar(aero);
        voo.Decolar();
        Assert.Equal(0, voo.MoedasColetadas);

        var moeda = _poolMoedas.Obter();
        moeda.Ativar(new VetorVoo(0f, 25f, 100f));
        var coletaveisAtivos = new List<Coletavel> { moeda };

        var estadoAeronave = EstadoFisicoAeronave.CriarInicial(
            new VetorVoo(0f, 25f, 99.5f), // Distância de 0.5m da moeda
            new VetorVoo(0f, 0f, 20f),
            0f);

        // Act
        var resultado = _casoDeUso.Executar(voo, estadoAeronave, coletaveisAtivos, _poolMoedas, _poolAneis);

        // Assert
        Assert.Equal(1, resultado.MoedasColetadasNoPasso);
        Assert.Equal(1, voo.MoedasColetadas);
        Assert.Empty(coletaveisAtivos);
        Assert.False(moeda.Ativo);
        Assert.True(moeda.Coletado);
        Assert.Equal(20, _poolMoedas.DisponiveisEmEstoque); // Devolvida ao pool
    }

    [Fact]
    public void Executar_QuandoAeronaveNaoCruzaMoeda_NaoDeveIncrementarMoedas()
    {
        // Arrange
        var aero = Aeronave.CriarPadrao();
        var voo = Voo.Iniciar(aero);
        voo.Decolar();

        var moeda = _poolMoedas.Obter();
        moeda.Ativar(new VetorVoo(0f, 25f, 100f));
        var coletaveisAtivos = new List<Coletavel> { moeda };

        var estadoDistante = EstadoFisicoAeronave.CriarInicial(
            new VetorVoo(0f, 25f, 50f), // 50 metros distante da moeda
            new VetorVoo(0f, 0f, 20f),
            0f);

        // Act
        var resultado = _casoDeUso.Executar(voo, estadoDistante, coletaveisAtivos, _poolMoedas, _poolAneis);

        // Assert
        Assert.Equal(0, resultado.MoedasColetadasNoPasso);
        Assert.Equal(0, voo.MoedasColetadas);
        Assert.Single(coletaveisAtivos);
        Assert.True(moeda.Ativo);
        Assert.False(moeda.Coletado);
    }

    [Fact]
    public void Executar_ComMultiplasMoedasNoMesmoPasso_DeveAcumularCorretamente()
    {
        // Arrange
        var aero = Aeronave.CriarPadrao();
        var voo = Voo.Iniciar(aero);
        voo.Decolar();

        var moeda1 = _poolMoedas.Obter();
        moeda1.Ativar(new VetorVoo(0f, 25f, 100f));

        var moeda2 = _poolMoedas.Obter();
        moeda2.Ativar(new VetorVoo(0f, 25.5f, 100.5f));

        var coletaveisAtivos = new List<Coletavel> { moeda1, moeda2 };

        var estadoAeronave = EstadoFisicoAeronave.CriarInicial(
            new VetorVoo(0f, 25f, 100f),
            new VetorVoo(0f, 0f, 20f),
            0f);

        // Act
        var resultado = _casoDeUso.Executar(voo, estadoAeronave, coletaveisAtivos, _poolMoedas, _poolAneis);

        // Assert
        Assert.Equal(2, resultado.MoedasColetadasNoPasso);
        Assert.Equal(2, voo.MoedasColetadas);
        Assert.Empty(coletaveisAtivos);
        Assert.Equal(20, _poolMoedas.DisponiveisEmEstoque);
    }

    [Fact]
    public void Executar_ComAeronaveNoSolo_NaoDeveColetarMoedasAereas()
    {
        // Arrange
        var aero = Aeronave.CriarPadrao();
        var voo = Voo.Iniciar(aero);
        voo.Decolar();

        var moeda = _poolMoedas.Obter();
        moeda.Ativar(new VetorVoo(0f, 0f, 100f));
        var coletaveisAtivos = new List<Coletavel> { moeda };

        var estadoSolo = EstadoFisicoAeronave.Criar(
            new VetorVoo(0f, 0f, 100f),
            VetorVoo.Zero,
            0f,
            VetorVoo.Zero,
            noSolo: true);

        // Act
        var resultado = _casoDeUso.Executar(voo, estadoSolo, coletaveisAtivos, _poolMoedas, _poolAneis);

        // Assert
        Assert.Equal(0, resultado.MoedasColetadasNoPasso);
        Assert.Equal(0, voo.MoedasColetadas);
        Assert.Single(coletaveisAtivos);
    }

    [Fact]
    public void Executar_AtravessandoAnelVento_DeveAplicarImpulsoDe10MetrosPorSegundoSemConsumirCombustivel()
    {
        // Arrange
        var aero = Aeronave.CriarPadrao();
        var voo = Voo.Iniciar(aero);
        voo.Decolar();
        var combustivelInicial = voo.Combustivel.QuantidadeAtual;

        var anel = _poolAneis.Obter();
        anel.Ativar(new VetorVoo(0f, 30f, 80f));
        var coletaveisAtivos = new List<Coletavel> { anel };

        // Aeronave voando a 15 m/s horizontal no mesmo ponto do anel
        var estadoAeronave = EstadoFisicoAeronave.CriarInicial(
            new VetorVoo(0f, 30f, 80f),
            new VetorVoo(0f, 0f, 15.0f),
            0f);

        // Act
        var resultado = _casoDeUso.Executar(voo, estadoAeronave, coletaveisAtivos, _poolMoedas, _poolAneis);

        // Assert
        Assert.True(resultado.RecebeuImpulsoVento);
        Assert.Equal(0, resultado.MoedasColetadasNoPasso);
        Assert.Equal(10.0f, resultado.ImpulsoAplicado.Z, precision: 2);
        Assert.Equal(0f, resultado.ImpulsoAplicado.Y, precision: 2);
        Assert.Equal(25.0f, resultado.EstadoFisicoAtualizado.Velocidade.Z, precision: 2); // 15 + 10 = 25 m/s
        Assert.Equal(combustivelInicial, voo.Combustivel.QuantidadeAtual); // Combustível intacto
        Assert.Empty(coletaveisAtivos);
        Assert.Equal(10, _poolAneis.DisponiveisEmEstoque); // Devolvido ao pool
    }

    [Fact]
    public void Executar_AtravessandoAnelComVelocidadeBaixa_DeveProjetarImpulsoNoPitchDoNariz()
    {
        // Arrange
        var aero = Aeronave.CriarPadrao();
        var voo = Voo.Iniciar(aero);
        voo.Decolar();

        var anel = _poolAneis.Obter();
        anel.Ativar(new VetorVoo(0f, 30f, 80f));
        var coletaveisAtivos = new List<Coletavel> { anel };

        // Aeronave quase parada (0.1 m/s) com pitch apontado 30 graus para cima
        var estadoAeronave = EstadoFisicoAeronave.CriarInicial(
            new VetorVoo(0f, 30f, 80f),
            new VetorVoo(0f, 0f, 0.1f),
            inclinacaoPitchGraus: 30.0f);

        // Act
        var resultado = _casoDeUso.Executar(voo, estadoAeronave, coletaveisAtivos, _poolMoedas, _poolAneis);

        // Assert
        Assert.True(resultado.RecebeuImpulsoVento);
        // sin(30°) = 0.5 -> 10 * 0.5 = 5.0 m/s em Y
        Assert.Equal(5.0f, resultado.ImpulsoAplicado.Y, precision: 1);
        // cos(30°) = 0.866 -> 10 * 0.866 = 8.66 m/s em Z
        Assert.Equal(8.66f, resultado.ImpulsoAplicado.Z, precision: 1);
        Assert.True(resultado.EstadoFisicoAtualizado.Velocidade.Y > 4.5f);
        Assert.True(resultado.EstadoFisicoAtualizado.Velocidade.Z > 8.0f);
    }
}
