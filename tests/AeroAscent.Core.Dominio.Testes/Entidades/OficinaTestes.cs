namespace AeroAscent.Core.Dominio.Testes.Entidades;

using AeroAscent.Core.Dominio.Entidades;
using AeroAscent.Core.Dominio.Enums;
using AeroAscent.Core.Dominio.Excecoes;
using AeroAscent.Core.Dominio.ObjetosDeValor;
using Xunit;

/// <summary>
/// Testes unitários para a entidade Oficina, regras de evolução e compra de melhorias.
/// </summary>
public class OficinaTestes
{
    [Fact]
    public void CriarPadrao_DeveInicializarComIdValidoECatalogoCompleto()
    {
        // Act
        var oficina = Oficina.CriarPadrao();

        // Assert
        Assert.NotEqual(Guid.Empty, oficina.Id);
        var catalogo = oficina.ObterCatalogo();
        Assert.Equal(4, catalogo.Count);
        Assert.Contains(catalogo, m => m.Tipo == TipoMelhoria.Motor);
        Assert.Contains(catalogo, m => m.Tipo == TipoMelhoria.Aerodinamica);
        Assert.Contains(catalogo, m => m.Tipo == TipoMelhoria.TanqueCombustivel);
        Assert.Contains(catalogo, m => m.Tipo == TipoMelhoria.Catapulta);
    }

    [Fact]
    public void CalcularCustoMelhoria_DeveRetornarCustoCorretoPeloTipoENivel()
    {
        // Arrange
        var oficina = Oficina.CriarPadrao();

        // Act
        var custoMotorNivel1 = oficina.CalcularCustoMelhoria(TipoMelhoria.Motor, 1);
        var custoMotorNivel2 = oficina.CalcularCustoMelhoria(TipoMelhoria.Motor, 2);

        // Assert
        // Motor CustoBase = 50 => Nível 1: 50 * 1.5^0 = 50; Nível 2: 50 * 1.5^1 = 75
        Assert.Equal(new Moeda(50), custoMotorNivel1);
        Assert.Equal(new Moeda(75), custoMotorNivel2);
    }

    [Fact]
    public void CalcularCustoMelhoria_NoNivel10OuSuperior_DeveLancarMelhoriaNivelMaximoException()
    {
        // Arrange
        var oficina = Oficina.CriarPadrao();

        // Act & Assert
        Assert.Throws<MelhoriaNivelMaximoException>(() =>
            oficina.CalcularCustoMelhoria(TipoMelhoria.Motor, 10));
    }

    [Fact]
    public void EvoluirComponente_ComSaldoSuficiente_DeveEvoluirEDeduzirMoedas()
    {
        // Arrange
        var oficina = Oficina.CriarPadrao();
        var aeronave = Aeronave.CriarPadrao(); // Nível Motor = 1
        var saldo = new Moeda(200);

        // Act (Motor nível 1 -> custo = 50 moedas)
        var novoSaldo = oficina.EvoluirComponente(aeronave, saldo, TipoMelhoria.Motor);

        // Assert
        Assert.Equal(2, aeronave.NivelMotor);
        Assert.Equal(new Moeda(150), novoSaldo);
    }

    [Fact]
    public void EvoluirComponente_ComSaldoInsuficiente_DeveLancarSaldoInsuficienteExceptionSemModificar()
    {
        // Arrange
        var oficina = Oficina.CriarPadrao();
        var aeronave = Aeronave.CriarPadrao(); // Nível Motor = 1 (custo = 50)
        var saldoInsuficiente = new Moeda(30);

        // Act & Assert
        Assert.Throws<SaldoInsuficienteException>(() =>
            oficina.EvoluirComponente(aeronave, saldoInsuficiente, TipoMelhoria.Motor));

        Assert.Equal(1, aeronave.NivelMotor);
    }

    [Fact]
    public void EvoluirComponente_QuandoJaNoNivelMaximo10_DeveLancarMelhoriaNivelMaximoException()
    {
        // Arrange
        var oficina = Oficina.CriarPadrao();
        var aeronave = new Aeronave(Guid.NewGuid(), nivelMotor: 10, 1, 1, 1);
        var saldo = new Moeda(10000);

        // Act & Assert
        Assert.Throws<MelhoriaNivelMaximoException>(() =>
            oficina.EvoluirComponente(aeronave, saldo, TipoMelhoria.Motor));

        Assert.Equal(10, aeronave.NivelMotor);
    }
}
