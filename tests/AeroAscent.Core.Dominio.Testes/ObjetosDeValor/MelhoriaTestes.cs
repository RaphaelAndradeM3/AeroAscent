namespace AeroAscent.Core.Dominio.Testes.ObjetosDeValor;

using AeroAscent.Core.Dominio.Enums;
using AeroAscent.Core.Dominio.Excecoes;
using AeroAscent.Core.Dominio.ObjetosDeValor;
using Xunit;

/// <summary>
/// Testes unitários para o objeto de valor Melhoria e cálculo de custos e eficácia.
/// </summary>
public class MelhoriaTestes
{
    [Fact]
    public void Construtor_ComParametrosValidos_DeveAtribuirPropriedades()
    {
        // Act
        var melhoria = new Melhoria(TipoMelhoria.Motor, nivel: 2, custoBase: new Moeda(50), multiplicadorEficacia: 1.25f);

        // Assert
        Assert.Equal(TipoMelhoria.Motor, melhoria.Tipo);
        Assert.Equal(2, melhoria.Nivel);
        Assert.Equal(new Moeda(50), melhoria.CustoBase);
        Assert.Equal(1.25f, melhoria.MultiplicadorEficacia);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(11)]
    public void Construtor_ComNivelForaDoIntervalo_DeveLancarDominioInvalidoException(int nivelInvalido)
    {
        // Act & Assert
        Assert.Throws<DominioInvalidoException>(() =>
            new Melhoria(TipoMelhoria.Aerodinamica, nivelInvalido, new Moeda(30), 1.0f));
    }

    [Fact]
    public void CalcularCustoProximoNivel_DeveAplicarFormulaExponencial()
    {
        // Nível 1: CustoBase * 1.5^0 = 50 * 1 = 50 moedas
        var mNivel1 = new Melhoria(TipoMelhoria.Motor, 1, new Moeda(50), 1.0f);
        Assert.Equal(new Moeda(50), mNivel1.CalcularCustoProximoNivel());

        // Nível 2: CustoBase * 1.5^1 = 50 * 1.5 = 75 moedas
        var mNivel2 = new Melhoria(TipoMelhoria.Motor, 2, new Moeda(50), 1.2f);
        Assert.Equal(new Moeda(75), mNivel2.CalcularCustoProximoNivel());

        // Nível 3: CustoBase * 1.5^2 = 50 * 2.25 = 112 moedas
        var mNivel3 = new Melhoria(TipoMelhoria.Motor, 3, new Moeda(50), 1.4f);
        Assert.Equal(new Moeda(112), mNivel3.CalcularCustoProximoNivel());
    }

    [Fact]
    public void CalcularCustoProximoNivel_NoNivelMaximo_DeveLancarMelhoriaNivelMaximoException()
    {
        // Arrange (nível 10 é o máximo)
        var mNivelMax = new Melhoria(TipoMelhoria.Motor, 10, new Moeda(50), 2.5f);

        // Act & Assert
        Assert.Throws<MelhoriaNivelMaximoException>(() => mNivelMax.CalcularCustoProximoNivel());
    }
}
