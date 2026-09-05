namespace AeroAscent.Core.Aplicacao.Testes.CasosDeUso;

using System;
using AeroAscent.Core.Aplicacao.CasosDeUso;
using AeroAscent.Core.Dominio.Contratos;
using AeroAscent.Core.Dominio.Entidades;
using AeroAscent.Core.Dominio.Enums;
using AeroAscent.Core.Dominio.Excecoes;
using AeroAscent.Core.Dominio.ObjetosDeValor;
using AeroAscent.Core.Dominio.Servicos;
using Xunit;

/// <summary>
/// Testes unitários para o caso de uso LancarAeronaveCasoDeUso na camada de Aplicação.
/// </summary>
public class LancarAeronaveCasoDeUsoTestes
{
    private readonly IServicoFisicaVoo _servicoFisica;
    private readonly LancarAeronaveCasoDeUso _casoDeUso;

    public LancarAeronaveCasoDeUsoTestes()
    {
        _servicoFisica = new ServicoFisicaVoo();
        _casoDeUso = new LancarAeronaveCasoDeUso(_servicoFisica);
    }

    [Fact]
    public void Executar_ComVooEmPreparacao_DeveTransitarParaEmVooERetornarSucesso()
    {
        // Arrange
        var aeronave = Aeronave.CriarPadrao();
        var voo = Voo.Iniciar(aeronave);
        var parametros = ParametrosLancamento.Criar(1.0f);

        // Act
        var resultado = _casoDeUso.Executar(voo, parametros);

        // Assert
        Assert.True(resultado.Sucesso);
        Assert.Null(resultado.MensagemErro);
        Assert.Equal(StatusVoo.EmVoo, voo.Status);
        Assert.InRange(resultado.VelocidadeInicial.Magnitude(), 24.95f, 25.05f);
        Assert.Equal(0f, resultado.VelocidadeInicial.X);
    }

    [Fact]
    public void Executar_ComVooEmPreparacao_NenhumCombustivelDeveSerConsumido_FR005()
    {
        // Arrange (FR-005: Zero consumo de combustível no lançamento)
        var aeronave = Aeronave.CriarPadrao();
        var voo = Voo.Iniciar(aeronave);
        var combustivelInicial = voo.Combustivel.QuantidadeAtual;
        var parametros = ParametrosLancamento.Criar(0.8f);

        // Act
        var resultado = _casoDeUso.Executar(voo, parametros);

        // Assert
        Assert.True(resultado.Sucesso);
        Assert.Equal(combustivelInicial, voo.Combustivel.QuantidadeAtual);
        Assert.Equal(voo.Combustivel.CapacidadeMaxima, voo.Combustivel.QuantidadeAtual);
    }

    [Fact]
    public void Executar_ComVooJaEmVoo_DeveRetornarFalhaENaoCorromperEstado()
    {
        // Arrange
        var voo = Voo.Iniciar(Aeronave.CriarPadrao());
        voo.Decolar(); // Já em voo
        var parametros = ParametrosLancamento.Criar(1.0f);

        // Act
        var resultado = _casoDeUso.Executar(voo, parametros);

        // Assert
        Assert.False(resultado.Sucesso);
        Assert.NotNull(resultado.MensagemErro);
        Assert.Contains("EmPreparacao", resultado.MensagemErro);
        Assert.Equal(StatusVoo.EmVoo, voo.Status);
    }

    [Fact]
    public void Executar_ComVooNulo_DeveLancarDominioInvalidoException()
    {
        // Arrange
        var parametros = ParametrosLancamento.Criar(1.0f);

        // Act & Assert
        Assert.Throws<DominioInvalidoException>(() => _casoDeUso.Executar(null!, parametros));
    }

    [Fact]
    public void Executar_ComCatapultaEvoluidaNivel4_DeveAplicarVelocidadeEscalonada()
    {
        // Arrange
        var aeronave = new Aeronave(Guid.NewGuid(), 1, 1, 1, 4); // Catapulta nível 4
        var voo = Voo.Iniciar(aeronave);
        var parametros = ParametrosLancamento.Criar(1.0f);
        const float velocidadeEsperada = 25.0f * (1 + (4 - 1) * 0.25f); // 43.75 m/s

        // Act
        var resultado = _casoDeUso.Executar(voo, parametros);

        // Assert
        Assert.True(resultado.Sucesso);
        Assert.InRange(resultado.VelocidadeInicial.Magnitude(), velocidadeEsperada - 0.05f, velocidadeEsperada + 0.05f);
    }

    [Fact]
    public void ExecutarComTemporizador_NoApiceDeMeioSegundo_DeveDispararComForcaMaxima()
    {
        // Arrange (1.0 Hz atinge o ápice de 100% em 0.5s)
        var voo = Voo.Iniciar(Aeronave.CriarPadrao());
        var medidor = new MedidorForcaOscilante(1.0f);

        // Act
        var resultado = _casoDeUso.ExecutarComTemporizador(voo, medidor, 0.5f);

        // Assert
        Assert.True(resultado.Sucesso);
        Assert.Equal(StatusVoo.EmVoo, voo.Status);
        Assert.InRange(resultado.VelocidadeInicial.Magnitude(), 24.95f, 25.05f);
    }

    [Fact]
    public void ExecutarComTemporizador_NoInstanteZero_DeveAplicarPisoProtetivoDe10PorCento()
    {
        // Arrange (t = 0.0s -> fator 0.0 -> piso protetivo 0.10f -> 2.5 m/s)
        var voo = Voo.Iniciar(Aeronave.CriarPadrao());
        var medidor = new MedidorForcaOscilante(1.0f);

        // Act
        var resultado = _casoDeUso.ExecutarComTemporizador(voo, medidor, 0.0f);

        // Assert
        Assert.True(resultado.Sucesso);
        Assert.Equal(StatusVoo.EmVoo, voo.Status);
        Assert.InRange(resultado.VelocidadeInicial.Magnitude(), 2.45f, 2.55f);
    }
}
