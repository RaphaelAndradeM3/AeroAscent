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

    [Fact]
    public void Executar_ComColetaveisDeixadosParaTras_DeveReciclarAutomaticamenteSC003()
    {
        // Arrange
        var aero = Aeronave.CriarPadrao();
        var voo = Voo.Iniciar(aero);
        voo.Decolar();

        var moedaLongeAtras = _poolMoedas.Obter();
        moedaLongeAtras.Ativar(new VetorVoo(0f, 15f, 100f)); // Z = 100m

        var anelAtras = _poolAneis.Obter();
        anelAtras.Ativar(new VetorVoo(0f, 25f, 120f)); // Z = 120m

        var moedaValida = _poolMoedas.Obter();
        moedaValida.Ativar(new VetorVoo(0f, 20f, 140f)); // Z = 140m

        var coletaveisAtivos = new List<Coletavel> { moedaLongeAtras, anelAtras, moedaValida };

        // Aeronave em Z = 150m. Limite de reciclagem: Z < 150 - 20 = 130m.
        // Z = 100m e Z = 120m devem ser reciclados. Z = 140m deve continuar ativo.
        var estadoAeronave = EstadoFisicoAeronave.CriarInicial(
            new VetorVoo(0f, 20f, 150f),
            new VetorVoo(0f, 0f, 20f),
            0f);

        var estoqueMoedasAntes = _poolMoedas.DisponiveisEmEstoque;
        var estoqueAneisAntes = _poolAneis.DisponiveisEmEstoque;

        // Act
        var resultado = _casoDeUso.Executar(voo, estadoAeronave, coletaveisAtivos, _poolMoedas, _poolAneis);

        // Assert
        Assert.False(moedaLongeAtras.Ativo);
        Assert.False(anelAtras.Ativo);
        Assert.DoesNotContain(moedaLongeAtras, coletaveisAtivos);
        Assert.DoesNotContain(anelAtras, coletaveisAtivos);
        Assert.Contains(moedaValida, coletaveisAtivos);
        Assert.True(moedaValida.Ativo);

        // Verifica devolução ao estoque dos pools
        Assert.True(_poolMoedas.DisponiveisEmEstoque > estoqueMoedasAntes);
        Assert.True(_poolAneis.DisponiveisEmEstoque > estoqueAneisAntes);
    }

    [Fact]
    public void Executar_ComServicoGeracaoProceduralInjetado_DeveAtualizarJanelaESpawnarNovosItens()
    {
        // Arrange
        var servicoGeracao = new AeroAscent.Core.Dominio.Servicos.ServicoGeracaoProceduralColetaveis(42);
        var casoDeUsoComProcedural = new ProcessarColetaveisVooCasoDeUso(servicoGeracao);
        var aero = Aeronave.CriarPadrao();
        var voo = Voo.Iniciar(aero);
        voo.Decolar();
        var coletaveisAtivos = new List<Coletavel>();

        // Aeronave em Z = 50m
        var estadoAeronave = EstadoFisicoAeronave.CriarInicial(
            new VetorVoo(0f, 20f, 50f),
            new VetorVoo(0f, 0f, 25f),
            0f);

        // Act
        var resultado = casoDeUsoComProcedural.Executar(voo, estadoAeronave, coletaveisAtivos, _poolMoedas, _poolAneis);

        // Assert
        Assert.NotEmpty(coletaveisAtivos);
        foreach (var item in coletaveisAtivos)
        {
            Assert.True(item.Posicao.Z >= 50f + 30f, $"Item em Z={item.Posicao.Z} deve estar à frente da janela mínima (+30m)");
            Assert.True(item.Posicao.Z <= 50f + 150f, $"Item em Z={item.Posicao.Z} deve estar dentro da janela máxima (+150m)");
            Assert.True(item.Ativo);
        }
    }

    [Fact]
    public void Executar_DuranteSimulacaoContinuaDeVoo_DeveReciclarEGerarProceduralmenteSemFalhas()
    {
        // Arrange
        var servicoGeracao = new AeroAscent.Core.Dominio.Servicos.ServicoGeracaoProceduralColetaveis(100);
        var casoDeUso = new ProcessarColetaveisVooCasoDeUso(servicoGeracao);
        var aero = Aeronave.CriarPadrao();
        var voo = Voo.Iniciar(aero);
        voo.Decolar();
        var coletaveisAtivos = new List<Coletavel>();

        var estado = EstadoFisicoAeronave.CriarInicial(
            new VetorVoo(0f, 20f, 0f),
            new VetorVoo(0f, 0f, 20f),
            0f);

        // Act: Simula avanço contínuo de 0 a 300 metros
        for (var z = 0f; z <= 300f; z += 20f)
        {
            estado = estado.ComAtualizacao(
                new VetorVoo(0f, 20f, z),
                estado.Velocidade,
                estado.InclinacaoPitchGraus,
                estado.ForcaResultante,
                novoNoSolo: false,
                novoPropulsor: estado.Propulsor);

            casoDeUso.Executar(voo, estado, coletaveisAtivos, _poolMoedas, _poolAneis);
        }

        // Assert: Todos os itens remanescentes devem estar após Z = 300 - 20 = 280m
        Assert.NotEmpty(coletaveisAtivos);
        foreach (var item in coletaveisAtivos)
        {
            Assert.True(item.Posicao.Z >= 280f, $"Item remanescente em Z={item.Posicao.Z} deve ser >= 280m (SC-003)");
        }
    }

    [Fact]
    public void Benchmark_DeteccaoDeProximidadeEmTela_DeveExecutarEmMenosDePontoUmMilissegundoPorFrame_SC002()
    {
        // Arrange: Popula 50 coletáveis ativos simulando carga cheia de tela
        var voo = Voo.Iniciar(Aeronave.CriarPadrao());
        voo.Decolar();

        var coletaveisAtivos = new List<Coletavel>(50);
        for (var i = 0; i < 40; i++)
        {
            var moeda = Coletavel.CriarMoeda(new VetorVoo(0f, 20f + i, 100f + i * 2));
            moeda.Ativar(moeda.Posicao);
            coletaveisAtivos.Add(moeda);
        }
        for (var i = 0; i < 10; i++)
        {
            var anel = Coletavel.CriarAnelVento(new VetorVoo(0f, 30f + i, 150f + i * 5));
            anel.Ativar(anel.Posicao);
            coletaveisAtivos.Add(anel);
        }

        var estado = EstadoFisicoAeronave.CriarInicial(
            new VetorVoo(0f, 20f, 50f),
            new VetorVoo(0f, 0f, 20f),
            0f);

        // Warm-up do JIT
        for (var i = 0; i < 100; i++)
        {
            _casoDeUso.Executar(voo, estado, coletaveisAtivos, _poolMoedas, _poolAneis);
        }

        // Act: Medição de 2.000 iterações de frame
        const int iteracoes = 2_000;
        var sw = System.Diagnostics.Stopwatch.StartNew();
        for (var i = 0; i < iteracoes; i++)
        {
            _casoDeUso.Executar(voo, estado, coletaveisAtivos, _poolMoedas, _poolAneis);
        }
        sw.Stop();

        var tempoMedioMilissegundos = (double)sw.ElapsedMilliseconds / iteracoes;

        // Assert: SC-002 processamento em menos de 0.1ms por frame
        Assert.True(tempoMedioMilissegundos < 0.1,
            $"Tempo médio por frame ({tempoMedioMilissegundos:F4}ms) excedeu o limite de 0.1ms (SC-002).");
    }

    [Fact]
    public void Executar_ComMultiplasMoedasNoMesmoPasso_DeveColetarTodasSemPerda()
    {
        // Arrange: 3 moedas na mesma vizinhança da aeronave
        var voo = Voo.Iniciar(Aeronave.CriarPadrao());
        voo.Decolar();

        var m1 = _poolMoedas.Obter();
        m1.Ativar(new VetorVoo(0f, 20f, 50f));
        var m2 = _poolMoedas.Obter();
        m2.Ativar(new VetorVoo(0f, 20.5f, 50.2f));
        var m3 = _poolMoedas.Obter();
        m3.Ativar(new VetorVoo(0f, 19.8f, 49.9f));

        var coletaveisAtivos = new List<Coletavel> { m1, m2, m3 };
        var estado = EstadoFisicoAeronave.CriarInicial(
            new VetorVoo(0f, 20f, 50f),
            new VetorVoo(0f, 0f, 15f),
            0f);

        // Act
        var resultado = _casoDeUso.Executar(voo, estado, coletaveisAtivos, _poolMoedas, _poolAneis);

        // Assert
        Assert.Equal(3, resultado.MoedasColetadasNoPasso);
        Assert.Equal(3, voo.MoedasColetadas);
        Assert.Empty(coletaveisAtivos);
        Assert.True(m1.Coletado && m2.Coletado && m3.Coletado);
    }

    [Fact]
    public void Executar_QuandoAeronaveNoSolo_NaoDeveColetarNemAplicarImpulso()
    {
        // Arrange: Moeda colocada exatamente na posição da aeronave
        var voo = Voo.Iniciar(Aeronave.CriarPadrao());
        voo.Decolar();

        var moeda = _poolMoedas.Obter();
        moeda.Ativar(new VetorVoo(0f, 0f, 0f));
        var coletaveisAtivos = new List<Coletavel> { moeda };

        // Aeronave no solo (altitude = 0 e noSolo = true)
        var estadoNoSolo = EstadoFisicoAeronave.CriarInicial(
            new VetorVoo(0f, 0f, 0f),
            new VetorVoo(0f, 0f, 0f),
            0f);

        // Act
        var resultado = _casoDeUso.Executar(voo, estadoNoSolo, coletaveisAtivos, _poolMoedas, _poolAneis);

        // Assert
        Assert.Equal(0, resultado.MoedasColetadasNoPasso);
        Assert.False(resultado.RecebeuImpulsoVento);
        Assert.Equal(0, voo.MoedasColetadas);
        Assert.True(moeda.Ativo);
        Assert.False(moeda.Coletado);
    }

    [Fact]
    public void Executar_QuandoVooNaoEstiverEmAndamento_DeveRetornarNeutro()
    {
        // Arrange: Voo ainda em AguardandoLancamento
        var voo = Voo.Iniciar(Aeronave.CriarPadrao());
        var moeda = _poolMoedas.Obter();
        moeda.Ativar(new VetorVoo(0f, 20f, 50f));
        var coletaveisAtivos = new List<Coletavel> { moeda };

        var estado = EstadoFisicoAeronave.Criar(
            new VetorVoo(0f, 20f, 50f),
            new VetorVoo(0f, 0f, 15f),
            0f,
            VetorVoo.Zero,
            noSolo: false);

        // Act
        var resultado = _casoDeUso.Executar(voo, estado, coletaveisAtivos, _poolMoedas, _poolAneis);

        // Assert
        Assert.Equal(0, resultado.MoedasColetadasNoPasso);
        Assert.False(resultado.RecebeuImpulsoVento);
        Assert.True(moeda.Ativo);
    }

    [Fact]
    public void Executar_ComPoolInicialMenorQueDemanda_DeveExpandirElasticamente()
    {
        // Arrange: Pool com apenas 1 moeda inicialmente
        var poolReduzido = new GerenciadorPoolObjetos<Coletavel>(
            () => Coletavel.CriarMoeda(VetorVoo.Zero),
            capacidadeInicial: 1);

        var servicoProcedural = new AeroAscent.Core.Dominio.Servicos.ServicoGeracaoProceduralColetaveis(42);
        var casoDeUso = new ProcessarColetaveisVooCasoDeUso(servicoProcedural);
        var voo = Voo.Iniciar(Aeronave.CriarPadrao());
        voo.Decolar();
        var coletaveisAtivos = new List<Coletavel>();

        var estado = EstadoFisicoAeronave.CriarInicial(
            new VetorVoo(0f, 20f, 0f),
            new VetorVoo(0f, 0f, 20f),
            0f);

        // Act: Demanda mais que 1 item na janela
        var resultado = casoDeUso.Executar(voo, estado, coletaveisAtivos, poolReduzido, _poolAneis);

        // Assert: Expandiu a capacidade total sem lançar erro
        Assert.True(poolReduzido.CapacidadeTotal > 1);
        Assert.NotEmpty(coletaveisAtivos);
    }
}

