namespace AeroAscent.Core.Dominio.Testes.Entidades;

using System;
using AeroAscent.Core.Dominio.Entidades;
using AeroAscent.Core.Dominio.Enums;
using AeroAscent.Core.Dominio.Excecoes;
using AeroAscent.Core.Dominio.ObjetosDeValor;
using Xunit;

/// <summary>
/// Testes unitários para a entidade Coletavel (Moeda e Anel de Vento).
/// </summary>
public class ColetavelTestes
{
    [Fact]
    public void CriarMoeda_ComPosicaoValida_DeveInicializarComRaioPadraoEInativo()
    {
        // Arrange
        var posicao = new VetorVoo(0f, 25f, 100f);

        // Act
        var moeda = Coletavel.CriarMoeda(posicao);

        // Assert
        Assert.NotEqual(Guid.Empty, moeda.Id);
        Assert.Equal(TipoColetavel.Moeda, moeda.Tipo);
        Assert.Equal(Coletavel.RAIO_PADRAO_MOEDA_METROS, moeda.RaioColetaMetros);
        Assert.Equal(100f, moeda.Posicao.Z);
        Assert.Equal(25f, moeda.Posicao.Y);
        Assert.False(moeda.Ativo);
        Assert.False(moeda.Coletado);
    }

    [Fact]
    public void Construtor_ComIdVazioOuRaioInvalido_DeveLancarDominioInvalidoException()
    {
        // Act & Assert
        Assert.Throws<DominioInvalidoException>(() =>
            new Coletavel(Guid.Empty, TipoColetavel.Moeda, VetorVoo.Zero, 1.5f));

        Assert.Throws<DominioInvalidoException>(() =>
            new Coletavel(Guid.NewGuid(), TipoColetavel.Moeda, VetorVoo.Zero, 0f));

        Assert.Throws<DominioInvalidoException>(() =>
            new Coletavel(Guid.NewGuid(), TipoColetavel.Moeda, VetorVoo.Zero, -1.0f));
    }

    [Fact]
    public void Ativar_DeveMudarStatusParaAtivoEResetarColetado()
    {
        // Arrange
        var moeda = Coletavel.CriarMoeda(VetorVoo.Zero);
        moeda.MarcarColetado();
        Assert.True(moeda.Coletado);

        var novaPosicao = new VetorVoo(0f, 30f, 150f);

        // Act
        moeda.Ativar(novaPosicao);

        // Assert
        Assert.True(moeda.Ativo);
        Assert.False(moeda.Coletado);
        Assert.Equal(novaPosicao.Z, moeda.Posicao.Z);
        Assert.Equal(novaPosicao.Y, moeda.Posicao.Y);
    }

    [Fact]
    public void Desativar_DeveDefinirAtivoComoFalse()
    {
        // Arrange
        var moeda = Coletavel.CriarMoeda(VetorVoo.Zero);
        moeda.Ativar(new VetorVoo(0f, 10f, 20f));
        Assert.True(moeda.Ativo);

        // Act
        moeda.Desativar();

        // Assert
        Assert.False(moeda.Ativo);
    }

    [Fact]
    public void MarcarColetado_DeveDefinirColetadoTrueEAtivoFalse()
    {
        // Arrange
        var moeda = Coletavel.CriarMoeda(VetorVoo.Zero);
        moeda.Ativar(new VetorVoo(0f, 10f, 20f));

        // Act
        moeda.MarcarColetado();

        // Assert
        Assert.True(moeda.Coletado);
        Assert.False(moeda.Ativo);
    }

    [Fact]
    public void VerificarColisao_QuandoAeronaveCruzaRaioDeColeta_DeveRetornarTrue()
    {
        // Arrange
        var moeda = Coletavel.CriarMoeda(new VetorVoo(0f, 25f, 100f));
        moeda.Ativar(new VetorVoo(0f, 25f, 100f));

        // Aeronave a 1.2 metros de distância (raio moeda 1.5m + raio aero 0.5m = 2.0m de tolerância)
        var posicaoAeronave = new VetorVoo(0f, 25f, 101.2f);

        // Act
        var colidiu = moeda.VerificarColisao(posicaoAeronave, raioAeronaveMetros: 0.5f);

        // Assert
        Assert.True(colidiu);
    }

    [Fact]
    public void VerificarColisao_QuandoAeronaveForaDoAlcance_DeveRetornarFalse()
    {
        // Arrange
        var moeda = Coletavel.CriarMoeda(new VetorVoo(0f, 25f, 100f));
        moeda.Ativar(new VetorVoo(0f, 25f, 100f));

        // Aeronave a 5 metros de distância
        var posicaoAeronave = new VetorVoo(0f, 25f, 105.0f);

        // Act
        var colidiu = moeda.VerificarColisao(posicaoAeronave, raioAeronaveMetros: 0.5f);

        // Assert
        Assert.False(colidiu);
    }

    [Fact]
    public void VerificarColisao_QuandoInativoOuJaColetado_DeveRetornarFalseMesmoNaMesmaPosicao()
    {
        // Arrange
        var moeda = Coletavel.CriarMoeda(new VetorVoo(0f, 25f, 100f));
        var posicaoAeronave = new VetorVoo(0f, 25f, 100f);

        // Act & Assert 1: Inativo
        Assert.False(moeda.VerificarColisao(posicaoAeronave));

        // Act & Assert 2: Já Coletado
        moeda.Ativar(new VetorVoo(0f, 25f, 100f));
        moeda.MarcarColetado();
        Assert.False(moeda.VerificarColisao(posicaoAeronave));
    }
}
