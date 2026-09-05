namespace AeroAscent.Core.Aplicacao.Testes.CasosDeUso;

using System;
using AeroAscent.Core.Aplicacao.CasosDeUso;
using AeroAscent.Core.Dominio.Entidades;
using AeroAscent.Core.Dominio.Enums;
using AeroAscent.Core.Dominio.Excecoes;
using AeroAscent.Core.Dominio.ObjetosDeValor;
using AeroAscent.Core.Dominio.Servicos;
using Xunit;

/// <summary>
/// Testes unitários para o caso de uso AtualizarFisicaVooCasoDeUso.
/// </summary>
public class AtualizarFisicaVooCasoDeUsoTestes
{
    private readonly ServicoFisicaVoo _servicoFisica = new();
    private readonly AtualizarFisicaVooCasoDeUso _casoDeUso;

    public AtualizarFisicaVooCasoDeUsoTestes()
    {
        _casoDeUso = new AtualizarFisicaVooCasoDeUso(_servicoFisica);
    }

    [Fact]
    public void Construtor_ComServicoNulo_DeveLancarDominioInvalidoException()
    {
        // Act & Assert
        Assert.Throws<DominioInvalidoException>(() => new AtualizarFisicaVooCasoDeUso(null!));
    }

    [Fact]
    public void Executar_ComVooNulo_DeveLancarDominioInvalidoException()
    {
        // Arrange
        var estado = EstadoFisicoAeronave.CriarInicial(VetorVoo.Zero, VetorVoo.Zero, 0f);

        // Act & Assert
        Assert.Throws<DominioInvalidoException>(() =>
            _casoDeUso.Executar(null!, estado, ParametrosControlePiloto.Neutro, 0.02f));
    }

    [Fact]
    public void Executar_ComVooEmPreparacao_NaoDeveAlterarEstado()
    {
        // Arrange: Voo criado sem decolar (Status = EmPreparacao)
        var aeronave = Aeronave.CriarPadrao();
        var voo = Voo.Iniciar(aeronave);
        var estado = EstadoFisicoAeronave.CriarInicial(new VetorVoo(0f, 10f, 0f), new VetorVoo(0f, 0f, 20f), 10f);

        // Act
        var resultado = _casoDeUso.Executar(voo, estado, ParametrosControlePiloto.Neutro, 0.02f);

        // Assert
        Assert.Equal(estado, resultado);
        Assert.Equal(0f, voo.DistanciaPercorrida);
        Assert.Equal(0f, voo.AltitudeMaxima);
    }

    [Fact]
    public void Executar_ComVooEmVoo_DeveAtualizarMetricasDeDistanciaEAltitude()
    {
        // Arrange: Voo ativo após decolagem
        var aeronave = Aeronave.CriarPadrao();
        var voo = Voo.Iniciar(aeronave);
        voo.Decolar();

        var estadoInicial = EstadoFisicoAeronave.CriarInicial(
            new VetorVoo(0f, 20f, 50f),
            new VetorVoo(0f, 5f, 20f),
            10.0f);

        // Act: 1 passo de simulação
        var novoEstado = _casoDeUso.Executar(voo, estadoInicial, ParametrosControlePiloto.Neutro, 0.02f);

        // Assert
        Assert.Equal(StatusVoo.EmVoo, voo.Status);
        Assert.True(voo.DistanciaPercorrida > 50f, $"Distância ({voo.DistanciaPercorrida}) deve ter aumentado além de 50.");
        Assert.True(voo.AltitudeMaxima >= 20f, $"Altitude ({voo.AltitudeMaxima}) deve ser registrada.");
        Assert.False(novoEstado.NoSolo);
    }

    [Fact]
    public void Executar_ComAeronaveParadaNoSolo_DevePousarVooEAtualizarResultado()
    {
        // Arrange: Voo ativo, aeronave no solo com velocidade abaixo de 0.5 m/s
        var aeronave = Aeronave.CriarPadrao();
        var voo = Voo.Iniciar(aeronave);
        voo.Decolar();

        var estadoNoSoloParando = EstadoFisicoAeronave.Criar(
            new VetorVoo(0f, 0f, 250f),
            new VetorVoo(0f, 0f, 0.2f),
            0.0f,
            VetorVoo.Zero,
            true);

        // Act: Executar passo que reduzirá a velocidade para zero
        var estadoFinal = _casoDeUso.Executar(voo, estadoNoSoloParando, ParametrosControlePiloto.Neutro, 0.02f);

        // Assert
        Assert.True(estadoFinal.NoSolo);
        Assert.Equal(0f, estadoFinal.Velocidade.Z);
        Assert.Equal(StatusVoo.Pousado, voo.Status);
        Assert.NotNull(voo.Resultado);
        Assert.True(voo.DistanciaPercorrida >= 250f);
    }
}
