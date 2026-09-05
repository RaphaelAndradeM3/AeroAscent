namespace AeroAscent.Core.Dominio.Testes.ObjetosDeValor;

using AeroAscent.Core.Dominio.Excecoes;
using AeroAscent.Core.Dominio.ObjetosDeValor;
using Xunit;

/// <summary>
/// Testes unitários para o Objeto de Valor ParametrosControlePiloto cobrindo pitch, zona morta e boost.
/// </summary>
public class ParametrosControlePilotoTestes
{
    [Fact]
    public void Criar_ComParametrosValidos_DeveInstanciarCorretamente()
    {
        // Arrange & Act
        var controle = ParametrosControlePiloto.Criar(0.5f, 50.0f, true);

        // Assert
        Assert.Equal(0.5f, controle.IntensidadePitch);
        Assert.Equal(50.0f, controle.TaxaVariacaoAngularGrausPorSegundo);
        Assert.True(controle.AcionarBoost);
        Assert.True(controle.TemComandoAtivo);
    }

    [Fact]
    public void Criar_SemEspecificarBoost_DeveSerInativoPorPadrao()
    {
        // Arrange & Act
        var controle = ParametrosControlePiloto.Criar(0.5f);

        // Assert
        Assert.False(controle.AcionarBoost);
    }

    [Fact]
    public void Criar_ComIntensidadeAcimaDoMaximo_DeveClamparPara1()
    {
        // Arrange & Act
        var controle = ParametrosControlePiloto.Criar(2.5f);

        // Assert
        Assert.Equal(ParametrosControlePiloto.INTENSIDADE_MAXIMA, controle.IntensidadePitch);
    }

    [Fact]
    public void Criar_ComIntensidadeAbaixoDoMinimo_DeveClamparParaMenos1()
    {
        // Arrange & Act
        var controle = ParametrosControlePiloto.Criar(-3.0f);

        // Assert
        Assert.Equal(ParametrosControlePiloto.INTENSIDADE_MINIMA, controle.IntensidadePitch);
    }

    [Theory]
    [InlineData(0f)]
    [InlineData(-10f)]
    public void Criar_ComTaxaAngularZeroOuNegativa_DeveLancarDominioInvalidoException(float taxaInvalida)
    {
        // Act & Assert
        Assert.Throws<DominioInvalidoException>(() =>
            ParametrosControlePiloto.Criar(0.5f, taxaInvalida));
    }

    [Theory]
    [InlineData(0.0f, false)]
    [InlineData(0.04f, false)]
    [InlineData(-0.04f, false)]
    [InlineData(0.05f, true)]
    [InlineData(-0.05f, true)]
    [InlineData(0.8f, true)]
    public void TemComandoAtivo_DeveRespeitarZonaMorta(float intensidade, bool esperadoAtivo)
    {
        // Arrange & Act
        var controle = ParametrosControlePiloto.Criar(intensidade);

        // Assert
        Assert.Equal(esperadoAtivo, controle.TemComandoAtivo);
    }

    [Fact]
    public void Neutro_DeveConterIntensidadeZeroETaxaPadraoEBoostFalso()
    {
        // Arrange & Act
        var neutro = ParametrosControlePiloto.Neutro;

        // Assert
        Assert.Equal(0f, neutro.IntensidadePitch);
        Assert.Equal(ParametrosControlePiloto.TAXA_ANGULAR_PADRAO, neutro.TaxaVariacaoAngularGrausPorSegundo);
        Assert.False(neutro.AcionarBoost);
        Assert.False(neutro.TemComandoAtivo);
    }
}
