namespace AeroAscent.Core.Dominio.Testes.ObjetosDeValor;

using System;
using AeroAscent.Core.Dominio.Excecoes;
using AeroAscent.Core.Dominio.ObjetosDeValor;
using Xunit;

/// <summary>
/// Testes unitários para o objeto de valor imutável <see cref="ConfiguracaoAudio"/>,
/// validando invariantes de limites, operações de cópia imutável e zero alocação na stack.
/// </summary>
public sealed class ConfiguracaoAudioTestes
{
    [Fact]
    public void Construtor_ComValoresValidos_DeveInstanciarComSucesso()
    {
        // Act
        var config = new ConfiguracaoAudio(0.5f, 0.6f, true, false);

        // Assert
        Assert.Equal(0.5f, config.VolumeEfeitos);
        Assert.Equal(0.6f, config.VolumeMusica);
        Assert.True(config.EfeitosAtivos);
        Assert.False(config.MusicaAtiva);
    }

    [Theory]
    [InlineData(-0.1f)]
    [InlineData(1.1f)]
    [InlineData(-100f)]
    [InlineData(50f)]
    public void Construtor_ComVolumeEfeitosInvalido_DeveLancarDominioInvalidoException(float volumeInvalido)
    {
        // Act & Assert
        var ex = Assert.Throws<DominioInvalidoException>(() =>
            new ConfiguracaoAudio(volumeInvalido, 0.5f, true, true));

        Assert.Equal("volumeEfeitos", ex.NomeCampo);
    }

    [Theory]
    [InlineData(-0.01f)]
    [InlineData(1.05f)]
    [InlineData(-5f)]
    [InlineData(2f)]
    public void Construtor_ComVolumeMusicaInvalido_DeveLancarDominioInvalidoException(float volumeInvalido)
    {
        // Act & Assert
        var ex = Assert.Throws<DominioInvalidoException>(() =>
            new ConfiguracaoAudio(0.5f, volumeInvalido, true, true));

        Assert.Equal("volumeMusica", ex.NomeCampo);
    }

    [Fact]
    public void Padrao_DevePossuirValoresPredefinidosBalanceadosEAmbosCanaisAtivos()
    {
        // Arrange & Act
        var padrao = ConfiguracaoAudio.Padrao;

        // Assert
        Assert.Equal(0.8f, padrao.VolumeEfeitos);
        Assert.Equal(0.7f, padrao.VolumeMusica);
        Assert.True(padrao.EfeitosAtivos);
        Assert.True(padrao.MusicaAtiva);
    }

    [Fact]
    public void ComVolumeEfeitos_DeveRetornarNovaInstanciaSemModificarOutrosCampos()
    {
        // Arrange
        var original = new ConfiguracaoAudio(0.5f, 0.4f, true, true);

        // Act
        var modificada = original.ComVolumeEfeitos(0.9f);

        // Assert
        Assert.Equal(0.9f, modificada.VolumeEfeitos);
        Assert.Equal(0.4f, modificada.VolumeMusica);
        Assert.True(modificada.EfeitosAtivos);
        Assert.True(modificada.MusicaAtiva);
        Assert.Equal(0.5f, original.VolumeEfeitos); // Imutabilidade preservada
    }

    [Fact]
    public void ComVolumeMusica_DeveRetornarNovaInstanciaSemModificarOutrosCampos()
    {
        // Arrange
        var original = new ConfiguracaoAudio(0.5f, 0.4f, true, true);

        // Act
        var modificada = original.ComVolumeMusica(0.1f);

        // Assert
        Assert.Equal(0.5f, modificada.VolumeEfeitos);
        Assert.Equal(0.1f, modificada.VolumeMusica);
        Assert.True(modificada.EfeitosAtivos);
        Assert.True(modificada.MusicaAtiva);
        Assert.Equal(0.4f, original.VolumeMusica); // Imutabilidade preservada
    }

    [Fact]
    public void AlternarEfeitos_DeveInverterFlagDeEfeitosSemModificarOutrosCampos()
    {
        // Arrange
        var original = new ConfiguracaoAudio(0.8f, 0.7f, true, true);

        // Act
        var alternada = original.AlternarEfeitos();

        // Assert
        Assert.False(alternada.EfeitosAtivos);
        Assert.True(alternada.MusicaAtiva);
        Assert.Equal(0.8f, alternada.VolumeEfeitos);
        Assert.Equal(0.7f, alternada.VolumeMusica);
    }

    [Fact]
    public void AlternarMusica_DeveInverterFlagDeMusicaSemModificarOutrosCampos()
    {
        // Arrange
        var original = new ConfiguracaoAudio(0.8f, 0.7f, true, true);

        // Act
        var alternada = original.AlternarMusica();

        // Assert
        Assert.True(alternada.EfeitosAtivos);
        Assert.False(alternada.MusicaAtiva);
        Assert.Equal(0.8f, alternada.VolumeEfeitos);
        Assert.Equal(0.7f, alternada.VolumeMusica);
    }

    [Fact]
    public void OperacoesEmConfiguracaoAudio_NaoDevemGerarAlocacaoNoHeap()
    {
        // Aquecimento (JIT)
        var config = ConfiguracaoAudio.Padrao;
        _ = config.ComVolumeEfeitos(0.5f);
        _ = config.AlternarMusica();

        // Medição de alocação de memória no heap
        long bytesAntes = GC.GetAllocatedBytesForCurrentThread();

        for (int i = 0; i < 1000; i++)
        {
            var c = ConfiguracaoAudio.Padrao;
            c = c.ComVolumeEfeitos(0.6f);
            c = c.ComVolumeMusica(0.3f);
            c = c.AlternarEfeitos();
            c = c.AlternarMusica();
        }

        long bytesDepois = GC.GetAllocatedBytesForCurrentThread();
        long diferencaAlocacao = bytesDepois - bytesAntes;

        // Struct imutável na stack garante zero alocação no heap (GC Alloc = 0 bytes)
        Assert.Equal(0, diferencaAlocacao);
    }
}
