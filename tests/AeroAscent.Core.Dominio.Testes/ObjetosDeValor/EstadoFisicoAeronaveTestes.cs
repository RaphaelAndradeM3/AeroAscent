namespace AeroAscent.Core.Dominio.Testes.ObjetosDeValor;

using System;
using AeroAscent.Core.Dominio.ObjetosDeValor;
using Xunit;

/// <summary>
/// Testes unitários para o Objeto de Valor EstadoFisicoAeronave.
/// </summary>
public class EstadoFisicoAeronaveTestes
{
    [Fact]
    public void CriarInicial_ComValoresValidos_DeveInstanciarCorretamente()
    {
        // Arrange
        var posicao = new VetorVoo(0f, 10f, 20f);
        var velocidade = new VetorVoo(0f, 5f, 25f);
        const float pitch = 15.0f;

        // Act
        var estado = EstadoFisicoAeronave.CriarInicial(posicao, velocidade, pitch);

        // Assert
        Assert.Equal(posicao, estado.Posicao);
        Assert.Equal(velocidade, estado.Velocidade);
        Assert.Equal(pitch, estado.InclinacaoPitchGraus);
        Assert.Equal(VetorVoo.Zero, estado.ForcaResultante);
        Assert.False(estado.NoSolo);
        Assert.InRange(estado.VelocidadeEscalar, MathF.Sqrt(5 * 5 + 25 * 25) - 0.01f, MathF.Sqrt(5 * 5 + 25 * 25) + 0.01f);
    }

    [Fact]
    public void Criar_ComAltitudeNegativa_DeveClamparParaZeroEAtivarNoSolo()
    {
        // Arrange
        var posicao = new VetorVoo(0f, -5f, 100f);
        var velocidade = new VetorVoo(0f, -2f, 10f);

        // Act
        var estado = EstadoFisicoAeronave.Criar(posicao, velocidade, 0f, VetorVoo.Zero, false);

        // Assert
        Assert.Equal(0f, estado.Posicao.Y);
        Assert.True(estado.NoSolo);
        Assert.Equal(0f, estado.Velocidade.Y); // Vy deve ser zerada ao colidir com o solo
    }

    [Fact]
    public void Criar_NoSoloComVelocidadeVerticalDescendente_DeveZerarVy()
    {
        // Arrange
        var posicao = new VetorVoo(0f, 0f, 50f);
        var velocidadeDescendente = new VetorVoo(0f, -8f, 15f);

        // Act
        var estado = EstadoFisicoAeronave.Criar(posicao, velocidadeDescendente, 5f, VetorVoo.Zero, true);

        // Assert
        Assert.Equal(0f, estado.Velocidade.Y);
        Assert.Equal(15f, estado.Velocidade.Z);
        Assert.True(estado.NoSolo);
    }

    [Fact]
    public void Criar_ComPitchAcimaDoMaximo_DeveClamparPara60Graus()
    {
        // Arrange & Act
        var estado = EstadoFisicoAeronave.Criar(
            new VetorVoo(0f, 10f, 0f),
            new VetorVoo(0f, 0f, 20f),
            85.0f,
            VetorVoo.Zero,
            false);

        // Assert
        Assert.Equal(EstadoFisicoAeronave.PITCH_MAXIMO_GRAUS, estado.InclinacaoPitchGraus);
    }

    [Fact]
    public void Criar_ComPitchAbaixoDoMinimo_DeveClamparParaMenos45Graus()
    {
        // Arrange & Act
        var estado = EstadoFisicoAeronave.Criar(
            new VetorVoo(0f, 10f, 0f),
            new VetorVoo(0f, 0f, 20f),
            -70.0f,
            VetorVoo.Zero,
            false);

        // Assert
        Assert.Equal(EstadoFisicoAeronave.PITCH_MINIMO_GRAUS, estado.InclinacaoPitchGraus);
    }

    [Fact]
    public void ComAtualizacao_DeveRetornarNovoEstadoComDadosAtualizados()
    {
        // Arrange
        var inicial = EstadoFisicoAeronave.CriarInicial(
            new VetorVoo(0f, 20f, 10f),
            new VetorVoo(0f, 0f, 20f),
            10.0f);

        var novaPos = new VetorVoo(0f, 25f, 30f);
        var novaVel = new VetorVoo(0f, 2f, 19f);
        var novaForca = new VetorVoo(0f, 50f, -10f);

        // Act
        var atualizado = inicial.ComAtualizacao(novaPos, novaVel, 12.0f, novaForca, false);

        // Assert
        Assert.Equal(novaPos, atualizado.Posicao);
        Assert.Equal(novaVel, atualizado.Velocidade);
        Assert.Equal(12.0f, atualizado.InclinacaoPitchGraus);
        Assert.Equal(novaForca, atualizado.ForcaResultante);
        Assert.False(atualizado.NoSolo);
    }
}
