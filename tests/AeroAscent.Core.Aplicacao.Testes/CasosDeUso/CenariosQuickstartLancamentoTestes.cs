namespace AeroAscent.Core.Aplicacao.Testes.CasosDeUso;

using System;
using AeroAscent.Core.Aplicacao.CasosDeUso;
using AeroAscent.Core.Dominio.Entidades;
using AeroAscent.Core.Dominio.Enums;
using AeroAscent.Core.Dominio.ObjetosDeValor;
using AeroAscent.Core.Dominio.Servicos;
using Xunit;

/// <summary>
/// Testes de aceitação automatizados que validam integralmente os 5 cenários funcionais
/// descritos no guia de validação rápida (specs/002-sistema-lancamento-catapulta/quickstart.md).
/// </summary>
public class CenariosQuickstartLancamentoTestes
{
    private readonly ServicoFisicaVoo _servicoFisica = new();
    private readonly LancarAeronaveCasoDeUso _casoDeUso;

    public CenariosQuickstartLancamentoTestes()
    {
        _casoDeUso = new LancarAeronaveCasoDeUso(_servicoFisica);
    }

    [Fact]
    public void Cenario1_LancamentoComForcaMaximaNoNivel1_DeveDecolarComVelocidade25MetrosPorSegundo()
    {
        // 1. Instanciar Aeronave padrão (Catapulta nível 1)
        var aeronave = Aeronave.CriarPadrao();

        // 2. Criar sessão de Voo no status EmPreparacao
        var voo = Voo.Iniciar(aeronave);

        // 3. Executar LancarAeronaveCasoDeUso com precisão de 1.0 (100%) e ângulo de 35°
        var parametros = ParametrosLancamento.Criar(1.0f, 35.0f);
        var resultado = _casoDeUso.Executar(voo, parametros);

        // 4. Verificações
        Assert.True(resultado.Sucesso);
        Assert.Equal(StatusVoo.EmVoo, voo.Status);
        Assert.InRange(resultado.VelocidadeInicial.Magnitude(), 24.95f, 25.05f);

        // Z (horizontal) ~= 25 * cos(35°) ~= 20.479 m/s
        const float zEsperado = 25.0f * 0.819152044f;
        Assert.InRange(resultado.VelocidadeInicial.Z, zEsperado - 0.05f, zEsperado + 0.05f);

        // Y (vertical) ~= 25 * sin(35°) ~= 14.339 m/s
        const float yEsperado = 25.0f * 0.573576436f;
        Assert.InRange(resultado.VelocidadeInicial.Y, yEsperado - 0.05f, yEsperado + 0.05f);

        // X (lateral) = 0.0 m/s
        Assert.Equal(0.0f, resultado.VelocidadeInicial.X);
    }

    [Fact]
    public void Cenario2_EscalonamentoDeForcaComCatapultaEvoluidaNivel3_DeveAumentarVelocidadeEm50PorCento()
    {
        // 1. Instanciar Aeronave com Catapulta no nível 3 (1 + (3 - 1) * 0.25 = 1.5x)
        var aeronave = new Aeronave(Guid.NewGuid(), 1, 1, 1, 3);
        var voo = Voo.Iniciar(aeronave);

        // 2. Executar lançamento com 100% de precisão
        var parametros = ParametrosLancamento.Criar(1.0f);
        var resultado = _casoDeUso.Executar(voo, parametros);

        // 3. Verificações
        Assert.True(resultado.Sucesso);
        const float velocidadeEsperada = 37.5f; // 25.0 * 1.5
        Assert.InRange(resultado.VelocidadeInicial.Magnitude(), velocidadeEsperada - 0.05f, velocidadeEsperada + 0.05f);

        // Z ~= 37.5 * cos(35°) ~= 30.718 m/s
        const float zEsperado = 37.5f * 0.819152044f;
        Assert.InRange(resultado.VelocidadeInicial.Z, zEsperado - 0.05f, zEsperado + 0.05f);

        // Y ~= 37.5 * sin(35°) ~= 21.509 m/s
        const float yEsperado = 37.5f * 0.573576436f;
        Assert.InRange(resultado.VelocidadeInicial.Y, yEsperado - 0.05f, yEsperado + 0.05f);
    }

    [Fact]
    public void Cenario3_ProtecaoDePisoMinimoEmFalhaDeTiming_DeveGarantirNoMinimo10PorCentoDeImpulso()
    {
        // 1. Executar lançamento com precisão de 0.0 (0%)
        var voo = Voo.Iniciar(Aeronave.CriarPadrao());
        var parametros = ParametrosLancamento.Criar(0.0f);
        var resultado = _casoDeUso.Executar(voo, parametros);

        // 2. Verificações
        Assert.True(resultado.Sucesso);
        Assert.Equal(0.10f, parametros.PrecisaoEfetiva);
        const float velocidadeMinimaEsperada = 2.5f; // 25.0 * 1.0 * 0.10
        Assert.InRange(resultado.VelocidadeInicial.Magnitude(), velocidadeMinimaEsperada - 0.05f, velocidadeMinimaEsperada + 0.05f);
        Assert.Equal(StatusVoo.EmVoo, voo.Status);
    }

    [Fact]
    public void Cenario4_BloqueioDeLancamentoDuplo_DeveRetornarFalhaPreservandoEstadoAtivo()
    {
        // 1. Executar lançamento com sucesso no voo
        var voo = Voo.Iniciar(Aeronave.CriarPadrao());
        var resultado1 = _casoDeUso.Executar(voo, ParametrosLancamento.Criar(1.0f));
        Assert.True(resultado1.Sucesso);
        Assert.Equal(StatusVoo.EmVoo, voo.Status);

        // 2. Tentar executar um segundo lançamento no mesmo voo ativo
        var resultado2 = _casoDeUso.Executar(voo, ParametrosLancamento.Criar(1.0f));

        // 3. Verificações
        Assert.False(resultado2.Sucesso);
        Assert.NotNull(resultado2.MensagemErro);
        Assert.Contains("EmPreparacao", resultado2.MensagemErro);
        Assert.Equal(StatusVoo.EmVoo, voo.Status);
    }

    [Fact]
    public void Cenario5_DinamicaDoMedidorDeForcaOscilante_DeveComprovarCurvaTriangularDeterministica()
    {
        // 1. Criar MedidorForcaOscilante com frequência padrão de 1.0 Hz
        var medidor = new MedidorForcaOscilante(1.0f);

        // 2. Validar que em t = 0.0s o fator é 0.0, em t = 0.5s é 1.0, e em t = 1.0s é 0.0
        var fatorInicial = medidor.ObterFatorPrecisao(0.0f);
        var fatorApice = medidor.ObterFatorPrecisao(0.5f);
        var fatorFinal = medidor.ObterFatorPrecisao(1.0f);

        Assert.Equal(0.0f, fatorInicial);
        Assert.InRange(fatorApice, 0.99f, 1.0f);
        Assert.InRange(fatorFinal, 0.0f, 0.01f);
    }
}
