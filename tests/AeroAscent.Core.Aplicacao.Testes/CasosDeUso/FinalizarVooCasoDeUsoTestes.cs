namespace AeroAscent.Core.Aplicacao.Testes.CasosDeUso;

using System;
using System.Threading.Tasks;
using AeroAscent.Core.Aplicacao.CasosDeUso;
using AeroAscent.Core.Aplicacao.Testes.Fixtures;
using AeroAscent.Core.Dominio.Entidades;
using AeroAscent.Core.Dominio.Enums;
using AeroAscent.Core.Dominio.Excecoes;
using AeroAscent.Core.Dominio.ObjetosDeValor;
using Xunit;

/// <summary>
/// Testes automatizados para o caso de uso <see cref="FinalizarVooCasoDeUso"/>.
/// Cobre o cálculo exato de moedas (US1 / SC-001), persistência do saldo,
/// resiliência na 1ª execução e casos de borda.
/// </summary>
public class FinalizarVooCasoDeUsoTestes
{
    private readonly ProgressoRepositorioMock _repositorioMock;
    private readonly FinalizarVooCasoDeUso _casoDeUso;

    public FinalizarVooCasoDeUsoTestes()
    {
        _repositorioMock = new ProgressoRepositorioMock();
        _casoDeUso = new FinalizarVooCasoDeUso(_repositorioMock);
    }

    [Fact]
    public void Construtor_ComRepositorioNulo_DeveLancarDominioInvalidoException()
    {
        // Act & Assert
        Assert.Throws<DominioInvalidoException>(() => new FinalizarVooCasoDeUso(null!));
    }

    [Fact]
    public async Task ExecutarAsync_ComVooNulo_DeveLancarDominioInvalidoException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<DominioInvalidoException>(() => _casoDeUso.ExecutarAsync(null!));
    }

    [Fact]
    public async Task ExecutarAsync_CenarioCanônicoUS1_DeveCalcularMoedasExatasECreditarNoSaldo()
    {
        // Arrange: voo pousado com 250m de distância, 80m de altitude e 5 moedas coletadas
        // Cálculo esperado: floor(250 * 0.1) + floor(80 * 0.05) + 5 = 25 + 4 + 5 = 34 moedas
        var progressoInicial = ProgressoJogador.CriarNovo();
        progressoInicial.CreditarMoedas(new Moeda(100)); // Saldo inicial: 100
        _repositorioMock.ProgressoArmazenado = progressoInicial;

        var voo = Voo.Iniciar(progressoInicial.Aeronave);
        voo.Decolar();
        voo.AtualizarMetricas(250f, 80f, 5);
        voo.Pousar();

        // Act
        var resumo = await _casoDeUso.ExecutarAsync(voo);

        // Assert
        Assert.Equal(250f, resumo.DistanciaMetros);
        Assert.Equal(80f, resumo.AltitudeMaximaMetros);
        Assert.Equal(25, resumo.MoedasPorDistancia);
        Assert.Equal(4, resumo.MoedasPorAltitude);
        Assert.Equal(5, resumo.MoedasColetadas);
        Assert.Equal(34, resumo.MoedasTotalGanhas.Quantidade);
        Assert.Equal(134, resumo.SaldoTotalAtualizado.Quantidade);
        Assert.Equal(134, _repositorioMock.ProgressoArmazenado.SaldoMoedas.Quantidade);
        Assert.True(voo.PremiacaoLiquidada);
    }

    [Fact]
    public async Task ExecutarAsync_PersistenciaAtomica_DeveSalvarProgressoAtualizadoNoRepositorio()
    {
        // Arrange
        var progresso = ProgressoJogador.CriarNovo();
        _repositorioMock.ProgressoArmazenado = progresso;

        var voo = Voo.Iniciar(progresso.Aeronave);
        voo.Decolar();
        voo.AtualizarMetricas(100f, 20f, 2);
        voo.Pousar();

        // Act
        var resumo = await _casoDeUso.ExecutarAsync(voo);

        // Assert
        Assert.Equal(1, _repositorioMock.QuantidadeChamadasSalvar);
        Assert.NotNull(_repositorioMock.ProgressoArmazenado);
        Assert.Equal(resumo.SaldoTotalAtualizado.Quantidade, _repositorioMock.ProgressoArmazenado.SaldoMoedas.Quantidade);
        Assert.Equal(1, _repositorioMock.ProgressoArmazenado.TotalVoosRealizados);
    }

    [Fact]
    public async Task ExecutarAsync_VooMuitoCurto_DeveConcederZeroMoedasAdicionaisSemErros()
    {
        // Arrange: 8m de distância e 2m de altitude -> floor(0.8) + floor(0.1) + 0 = 0
        var progresso = ProgressoJogador.CriarNovo();
        progresso.CreditarMoedas(new Moeda(50));
        _repositorioMock.ProgressoArmazenado = progresso;

        var voo = Voo.Iniciar(progresso.Aeronave);
        voo.Decolar();
        voo.AtualizarMetricas(8f, 2f, 0);
        voo.Pousar();

        // Act
        var resumo = await _casoDeUso.ExecutarAsync(voo);

        // Assert
        Assert.Equal(0, resumo.MoedasPorDistancia);
        Assert.Equal(0, resumo.MoedasPorAltitude);
        Assert.Equal(0, resumo.MoedasColetadas);
        Assert.Equal(0, resumo.MoedasTotalGanhas.Quantidade);
        Assert.Equal(50, resumo.SaldoTotalAtualizado.Quantidade);
        Assert.Equal(50, _repositorioMock.ProgressoArmazenado.SaldoMoedas.Quantidade);
    }

    [Fact]
    public async Task ExecutarAsync_PrimeiraExecucaoQuandoRepositorioRetornaNulo_DeveCriarNovoPerfilComSucesso()
    {
        // Arrange: _repositorioMock não possui progresso salvo (retorna null)
        _repositorioMock.ProgressoArmazenado = null;

        var aeronave = Aeronave.CriarPadrao();
        var voo = Voo.Iniciar(aeronave);
        voo.Decolar();
        voo.AtualizarMetricas(150f, 40f, 3);
        voo.Pousar();

        // Act
        var resumo = await _casoDeUso.ExecutarAsync(voo);

        // Assert
        // 150*0.1 = 15, 40*0.05 = 2, coletadas = 3 => 20 moedas
        Assert.NotNull(_repositorioMock.ProgressoArmazenado);
        Assert.Equal(20, resumo.MoedasTotalGanhas.Quantidade);
        Assert.Equal(20, resumo.SaldoTotalAtualizado.Quantidade);
        Assert.Equal(20, _repositorioMock.ProgressoArmazenado.SaldoMoedas.Quantidade);
        Assert.Equal(1, _repositorioMock.QuantidadeChamadasSalvar);
        Assert.Equal(1, _repositorioMock.ProgressoArmazenado.TotalVoosRealizados);
    }

    [Fact]
    public async Task ExecutarAsync_NovoRecordeDistancia_DeveSinalizarEhNovoRecordeDistanciaEAtualizarRepositorio()
    {
        // Arrange: recorde anterior de 300m e altitude de 100m
        var progresso = new ProgressoJogador(
            Guid.NewGuid(),
            Aeronave.CriarPadrao(),
            Moeda.Zero,
            recordeDistanciaMetros: 300f,
            recordeAltitudeMetros: 100f,
            totalVoosRealizados: 5);
        _repositorioMock.ProgressoArmazenado = progresso;

        var voo = Voo.Iniciar(progresso.Aeronave);
        voo.Decolar();
        voo.AtualizarMetricas(350f, 80f, 0); // Nova distância maior (350m > 300m), altitude menor (80m < 100m)
        voo.Pousar();

        // Act
        var resumo = await _casoDeUso.ExecutarAsync(voo);

        // Assert
        Assert.True(resumo.EhNovoRecordeDistancia);
        Assert.False(resumo.EhNovoRecordeAltitude);
        Assert.Equal(350f, _repositorioMock.ProgressoArmazenado.RecordeDistanciaMetros);
        Assert.Equal(100f, _repositorioMock.ProgressoArmazenado.RecordeAltitudeMetros);
    }

    [Fact]
    public async Task ExecutarAsync_DistanciaInferiorAoRecorde_DeveManterEhNovoRecordeDistanciaComoFalseEPreservarRecordeAnterior()
    {
        // Arrange: recorde anterior de 300m
        var progresso = new ProgressoJogador(
            Guid.NewGuid(),
            Aeronave.CriarPadrao(),
            Moeda.Zero,
            recordeDistanciaMetros: 300f,
            recordeAltitudeMetros: 100f,
            totalVoosRealizados: 5);
        _repositorioMock.ProgressoArmazenado = progresso;

        var voo = Voo.Iniciar(progresso.Aeronave);
        voo.Decolar();
        voo.AtualizarMetricas(280f, 80f, 0); // Distância menor que o recorde (280m < 300m)
        voo.Pousar();

        // Act
        var resumo = await _casoDeUso.ExecutarAsync(voo);

        // Assert
        Assert.False(resumo.EhNovoRecordeDistancia);
        Assert.Equal(300f, _repositorioMock.ProgressoArmazenado.RecordeDistanciaMetros);
    }

    [Fact]
    public async Task ExecutarAsync_NovoRecordeAltitude_DeveSinalizarEhNovoRecordeAltitudeEAtualizarRepositorio()
    {
        // Arrange: recorde de altitude anterior de 50m
        var progresso = new ProgressoJogador(
            Guid.NewGuid(),
            Aeronave.CriarPadrao(),
            Moeda.Zero,
            recordeDistanciaMetros: 400f,
            recordeAltitudeMetros: 50f,
            totalVoosRealizados: 2);
        _repositorioMock.ProgressoArmazenado = progresso;

        var voo = Voo.Iniciar(progresso.Aeronave);
        voo.Decolar();
        voo.AtualizarMetricas(200f, 75f, 0); // Altitude maior (75m > 50m)
        voo.Pousar();

        // Act
        var resumo = await _casoDeUso.ExecutarAsync(voo);

        // Assert
        Assert.True(resumo.EhNovoRecordeAltitude);
        Assert.False(resumo.EhNovoRecordeDistancia);
        Assert.Equal(75f, _repositorioMock.ProgressoArmazenado.RecordeAltitudeMetros);
        Assert.Equal(400f, _repositorioMock.ProgressoArmazenado.RecordeDistanciaMetros);
    }

    [Fact]
    public async Task ExecutarAsync_AltitudeInferiorAoRecorde_DeveManterEhNovoRecordeAltitudeComoFalseEPreservarRecordeAnterior()
    {
        // Arrange: recorde anterior de altitude de 80m
        var progresso = new ProgressoJogador(
            Guid.NewGuid(),
            Aeronave.CriarPadrao(),
            Moeda.Zero,
            recordeDistanciaMetros: 200f,
            recordeAltitudeMetros: 80f,
            totalVoosRealizados: 1);
        _repositorioMock.ProgressoArmazenado = progresso;

        var voo = Voo.Iniciar(progresso.Aeronave);
        voo.Decolar();
        voo.AtualizarMetricas(100f, 60f, 0); // Altitude menor (60m < 80m)
        voo.Pousar();

        // Act
        var resumo = await _casoDeUso.ExecutarAsync(voo);

        // Assert
        Assert.False(resumo.EhNovoRecordeAltitude);
        Assert.Equal(80f, _repositorioMock.ProgressoArmazenado.RecordeAltitudeMetros);
    }

    [Fact]
    public async Task ExecutarAsync_IntegracaoE2E_ComQuebraDeAmbosOsRecordes_DeveAtualizarProgressoERetornarExtratoConsistente()
    {
        // Arrange: histórico com 200m distância, 60m altitude e 50 moedas
        var progresso = new ProgressoJogador(
            Guid.NewGuid(),
            Aeronave.CriarPadrao(),
            new Moeda(50),
            recordeDistanciaMetros: 200f,
            recordeAltitudeMetros: 60f,
            totalVoosRealizados: 3);
        _repositorioMock.ProgressoArmazenado = progresso;

        var voo = Voo.Iniciar(progresso.Aeronave);
        voo.Decolar();
        voo.AtualizarMetricas(320f, 90f, 10);
        voo.Pousar();

        // Act
        var resumo = await _casoDeUso.ExecutarAsync(voo);

        // Assert
        // Recompensas: floor(320*0.1) + floor(90*0.05) + 10 = 32 + 4 + 10 = 46 moedas
        // Saldo total: 50 + 46 = 96 moedas
        Assert.True(resumo.EhNovoRecordeDistancia);
        Assert.True(resumo.EhNovoRecordeAltitude);
        Assert.Equal(46, resumo.MoedasTotalGanhas.Quantidade);
        Assert.Equal(96, resumo.SaldoTotalAtualizado.Quantidade);
        Assert.Equal(320f, _repositorioMock.ProgressoArmazenado.RecordeDistanciaMetros);
        Assert.Equal(90f, _repositorioMock.ProgressoArmazenado.RecordeAltitudeMetros);
        Assert.Equal(96, _repositorioMock.ProgressoArmazenado.SaldoMoedas.Quantidade);
        Assert.Equal(4, _repositorioMock.ProgressoArmazenado.TotalVoosRealizados);
        Assert.True(voo.PremiacaoLiquidada);
    }
}
