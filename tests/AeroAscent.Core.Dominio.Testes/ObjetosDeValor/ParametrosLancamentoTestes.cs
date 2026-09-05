namespace AeroAscent.Core.Dominio.Testes.ObjetosDeValor;

using AeroAscent.Core.Dominio.Excecoes;
using AeroAscent.Core.Dominio.ObjetosDeValor;
using Xunit;

/// <summary>
/// Testes unitários para o Objeto de Valor ParametrosLancamento (validação de piso protetivo e limites de rampa).
/// </summary>
public class ParametrosLancamentoTestes
{
    [Theory]
    [InlineData(0.0f, 0.10f)]
    [InlineData(0.05f, 0.10f)]
    [InlineData(-0.5f, 0.10f)]
    public void Criar_ComPrecisaoAbaixoDoPiso_DeveElevarParaPisoDe10PorCento(float precisaoBruta, float precisaoEsperada)
    {
        // Act
        var parametros = ParametrosLancamento.Criar(precisaoBruta);

        // Assert
        Assert.Equal(precisaoBruta, parametros.PrecisaoOriginal);
        Assert.Equal(precisaoEsperada, parametros.PrecisaoEfetiva);
    }

    [Theory]
    [InlineData(1.0f, 1.0f)]
    [InlineData(1.5f, 1.0f)]
    [InlineData(2.0f, 1.0f)]
    public void Criar_ComPrecisaoAcimaDe1_DeveLimitarEm100PorCento(float precisaoBruta, float precisaoEsperada)
    {
        // Act
        var parametros = ParametrosLancamento.Criar(precisaoBruta);

        // Assert
        Assert.Equal(precisaoBruta, parametros.PrecisaoOriginal);
        Assert.Equal(precisaoEsperada, parametros.PrecisaoEfetiva);
    }

    [Fact]
    public void Criar_ComPrecisaoValida_DevePreservarValorExato()
    {
        // Act
        var parametros = ParametrosLancamento.Criar(0.72f);

        // Assert
        Assert.Equal(0.72f, parametros.PrecisaoOriginal);
        Assert.Equal(0.72f, parametros.PrecisaoEfetiva);
    }

    [Fact]
    public void Criar_ComAnguloPadrao_DeveAdotar35Graus()
    {
        // Act
        var parametros = ParametrosLancamento.Criar(0.85f);

        // Assert
        Assert.Equal(35.0f, parametros.AnguloGraus);
    }

    [Theory]
    [InlineData(15.0f)]
    [InlineData(35.0f)]
    [InlineData(45.0f)]
    [InlineData(60.0f)]
    public void Criar_ComAnguloNoLimitePermitido_DeveInstanciarComSucesso(float anguloValido)
    {
        // Act
        var parametros = ParametrosLancamento.Criar(0.5f, anguloValido);

        // Assert
        Assert.Equal(anguloValido, parametros.AnguloGraus);
    }

    [Theory]
    [InlineData(14.9f)]
    [InlineData(0.0f)]
    [InlineData(-10.0f)]
    [InlineData(60.1f)]
    [InlineData(90.0f)]
    public void Criar_ComAnguloForaDosLimites_DeveLancarDominioInvalidoException(float anguloInvalido)
    {
        // Act & Assert
        Assert.Throws<DominioInvalidoException>(() => ParametrosLancamento.Criar(0.5f, anguloInvalido));
    }
}
