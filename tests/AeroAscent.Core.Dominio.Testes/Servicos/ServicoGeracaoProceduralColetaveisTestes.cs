namespace AeroAscent.Core.Dominio.Testes.Servicos;

using System;
using System.Collections.Generic;
using AeroAscent.Core.Dominio.Comum;
using AeroAscent.Core.Dominio.Entidades;
using AeroAscent.Core.Dominio.Enums;
using AeroAscent.Core.Dominio.Excecoes;
using AeroAscent.Core.Dominio.ObjetosDeValor;
using AeroAscent.Core.Dominio.Servicos;
using Xunit;

/// <summary>
/// Testes unitários para o serviço de geração procedural e reciclagem de coletáveis (ServicoGeracaoProceduralColetaveis).
/// Valida limites de janela de spawn, faixas de altitude, determinismo de semente e reciclagem traseira (SC-003).
/// </summary>
public class ServicoGeracaoProceduralColetaveisTestes
{
    private readonly IPoolObjetos<Coletavel> _poolMoedas;
    private readonly IPoolObjetos<Coletavel> _poolAneis;

    /// <summary>
    /// Inicializa os pools de moedas e anéis de vento para os testes.
    /// </summary>
    public ServicoGeracaoProceduralColetaveisTestes()
    {
        _poolMoedas = new GerenciadorPoolObjetos<Coletavel>(
            capacidadeInicial: 50,
            fabrica: () => Coletavel.CriarMoeda(VetorVoo.Zero),
            aoObter: c => c.Ativar(c.Posicao),
            aoLiberar: c => c.Desativar());

        _poolAneis = new GerenciadorPoolObjetos<Coletavel>(
            capacidadeInicial: 15,
            fabrica: () => Coletavel.CriarAnelVento(VetorVoo.Zero),
            aoObter: c => c.Ativar(c.Posicao),
            aoLiberar: c => c.Desativar());
    }

    [Fact]
    public void Deve_Inicializar_Com_Semente_Padrao_Ou_Personalizada()
    {
        // Act
        var servicoPadrao = new ServicoGeracaoProceduralColetaveis();
        var servicoPersonalizado = new ServicoGeracaoProceduralColetaveis(12345);

        // Assert
        Assert.Equal(42, servicoPadrao.Semente);
        Assert.Equal(12345, servicoPersonalizado.Semente);
    }

    [Fact]
    public void Deve_Lancar_Excecao_Se_Argumentos_Forem_Nulos()
    {
        // Arrange
        var servico = new ServicoGeracaoProceduralColetaveis();
        var listaAtivos = new List<Coletavel>();

        // Act & Assert
        Assert.Throws<DominioInvalidoException>(() => servico.AtualizarJanela(0f, null!, _poolAneis, listaAtivos));
        Assert.Throws<DominioInvalidoException>(() => servico.AtualizarJanela(0f, _poolMoedas, null!, listaAtivos));
        Assert.Throws<DominioInvalidoException>(() => servico.AtualizarJanela(0f, _poolMoedas, _poolAneis, null!));
    }

    [Fact]
    public void Deve_Gerar_Coletaveis_Na_Janela_Ativa_Entre_30m_E_150m()
    {
        // Arrange
        var servico = new ServicoGeracaoProceduralColetaveis(42);
        var coletaveisAtivos = new List<Coletavel>();
        var posicaoZAeronave = 0.0f;

        // Act
        servico.AtualizarJanela(posicaoZAeronave, _poolMoedas, _poolAneis, coletaveisAtivos);

        // Assert
        Assert.NotEmpty(coletaveisAtivos);
        foreach (var coletavel in coletaveisAtivos)
        {
            Assert.True(coletavel.Posicao.Z >= posicaoZAeronave + ServicoGeracaoProceduralColetaveis.DISTANCIA_MINIMA_SPAWN_FRENTE_METROS,
                $"Z ({coletavel.Posicao.Z}) deve ser >= {posicaoZAeronave + ServicoGeracaoProceduralColetaveis.DISTANCIA_MINIMA_SPAWN_FRENTE_METROS}");
            Assert.True(coletavel.Posicao.Z <= posicaoZAeronave + ServicoGeracaoProceduralColetaveis.DISTANCIA_MAXIMA_SPAWN_FRENTE_METROS,
                $"Z ({coletavel.Posicao.Z}) deve ser <= {posicaoZAeronave + ServicoGeracaoProceduralColetaveis.DISTANCIA_MAXIMA_SPAWN_FRENTE_METROS}");
            Assert.True(coletavel.Ativo);
            Assert.False(coletavel.Coletado);
        }
    }

    [Fact]
    public void Deve_Respeitar_Faixa_De_Altitude_Navegavel_Entre_5m_E_120m()
    {
        // Arrange
        var servico = new ServicoGeracaoProceduralColetaveis(999);
        var coletaveisAtivos = new List<Coletavel>();

        // Act
        servico.AtualizarJanela(0.0f, _poolMoedas, _poolAneis, coletaveisAtivos);

        // Assert
        Assert.NotEmpty(coletaveisAtivos);
        foreach (var coletavel in coletaveisAtivos)
        {
            Assert.Equal(0.0f, coletavel.Posicao.X);
            Assert.True(coletavel.Posicao.Y >= ServicoGeracaoProceduralColetaveis.ALTITUDE_MINIMA_METROS,
                $"Y ({coletavel.Posicao.Y}) deve ser >= {ServicoGeracaoProceduralColetaveis.ALTITUDE_MINIMA_METROS}");
            Assert.True(coletavel.Posicao.Y <= ServicoGeracaoProceduralColetaveis.ALTITUDE_MAXIMA_METROS,
                $"Y ({coletavel.Posicao.Y}) deve ser <= {ServicoGeracaoProceduralColetaveis.ALTITUDE_MAXIMA_METROS}");
        }
    }

    [Fact]
    public void Deve_Garantir_Determinismo_Com_Mesma_Semente()
    {
        // Arrange
        var servico1 = new ServicoGeracaoProceduralColetaveis(777);
        var servico2 = new ServicoGeracaoProceduralColetaveis(777);
        var ativos1 = new List<Coletavel>();
        var ativos2 = new List<Coletavel>();

        // Act
        servico1.AtualizarJanela(100.0f, _poolMoedas, _poolAneis, ativos1);

        var poolMoedas2 = new GerenciadorPoolObjetos<Coletavel>(
            capacidadeInicial: 50,
            fabrica: () => Coletavel.CriarMoeda(VetorVoo.Zero),
            aoObter: c => c.Ativar(c.Posicao),
            aoLiberar: c => c.Desativar());

        var poolAneis2 = new GerenciadorPoolObjetos<Coletavel>(
            capacidadeInicial: 15,
            fabrica: () => Coletavel.CriarAnelVento(VetorVoo.Zero),
            aoObter: c => c.Ativar(c.Posicao),
            aoLiberar: c => c.Desativar());

        servico2.AtualizarJanela(100.0f, poolMoedas2, poolAneis2, ativos2);

        // Assert
        Assert.Equal(ativos1.Count, ativos2.Count);
        for (var i = 0; i < ativos1.Count; i++)
        {
            Assert.Equal(ativos1[i].Tipo, ativos2[i].Tipo);
            Assert.Equal(ativos1[i].Posicao.Y, ativos2[i].Posicao.Y, 4);
            Assert.Equal(ativos1[i].Posicao.Z, ativos2[i].Posicao.Z, 4);
        }
    }

    [Fact]
    public void Deve_Reciclar_Automaticamente_Coletaveis_Atras_Da_Aeronave_SC003()
    {
        // Arrange: Coletáveis espalhados antes do avanço
        var servico = new ServicoGeracaoProceduralColetaveis(42);
        var coletaveisAtivos = new List<Coletavel>();

        var moedaAntiga1 = _poolMoedas.Obter();
        moedaAntiga1.Ativar(new VetorVoo(0f, 10f, 20f)); // Z = 20m
        coletaveisAtivos.Add(moedaAntiga1);

        var anelAntigo = _poolAneis.Obter();
        anelAntigo.Ativar(new VetorVoo(0f, 25f, 75f)); // Z = 75m
        coletaveisAtivos.Add(anelAntigo);

        var moedaProxima = _poolMoedas.Obter();
        moedaProxima.Ativar(new VetorVoo(0f, 15f, 85f)); // Z = 85m
        coletaveisAtivos.Add(moedaProxima);

        // Aeronave em Z = 100m. Limite de reciclagem traseira: Z < 100 - 20 = 80m.
        // As posições Z = 20m e Z = 75m devem ser recicladas. Z = 85m deve permanecer!
        var posicaoZAeronave = 100.0f;

        // Act
        servico.AtualizarJanela(posicaoZAeronave, _poolMoedas, _poolAneis, coletaveisAtivos);

        // Assert
        // Coletáveis nas posições antigas (< 80m) não devem mais existir em coletaveisAtivos
        Assert.DoesNotContain(coletaveisAtivos, c => c.Posicao.Z == 20.0f);
        Assert.DoesNotContain(coletaveisAtivos, c => c.Posicao.Z == 75.0f);
        Assert.Contains(coletaveisAtivos, c => c.Posicao.Z == 85.0f && c.Ativo);
    }

    [Fact]
    public void Deve_Reiniciar_Cursor_Ao_Chamar_Reiniciar()
    {
        // Arrange
        var servico = new ServicoGeracaoProceduralColetaveis(42);
        var ativos1 = new List<Coletavel>();
        servico.AtualizarJanela(0f, _poolMoedas, _poolAneis, ativos1);
        var quantidadeOriginal = ativos1.Count;

        // Act
        servico.Reiniciar();
        foreach (var item in ativos1)
        {
            if (item.Tipo == TipoColetavel.Moeda)
            {
                _poolMoedas.Liberar(item);
            }
            else
            {
                _poolAneis.Liberar(item);
            }
        }
        ativos1.Clear();

        var ativos2 = new List<Coletavel>();
        servico.AtualizarJanela(0f, _poolMoedas, _poolAneis, ativos2);

        // Assert
        Assert.Equal(quantidadeOriginal, ativos2.Count);
        for (var i = 0; i < ativos2.Count; i++)
        {
            Assert.True(ativos2[i].Posicao.Z >= ServicoGeracaoProceduralColetaveis.DISTANCIA_MINIMA_SPAWN_FRENTE_METROS);
        }
    }
}
