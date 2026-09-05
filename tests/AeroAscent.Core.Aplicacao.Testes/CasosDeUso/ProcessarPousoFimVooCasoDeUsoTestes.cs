namespace AeroAscent.Core.Aplicacao.Testes.CasosDeUso;

using System;
using System.Diagnostics;
using AeroAscent.Core.Aplicacao.CasosDeUso;
using AeroAscent.Core.Dominio.Contratos;
using AeroAscent.Core.Dominio.Entidades;
using AeroAscent.Core.Dominio.Enums;
using AeroAscent.Core.Dominio.Excecoes;
using AeroAscent.Core.Dominio.ObjetosDeValor;
using AeroAscent.Core.Dominio.Servicos;
using Xunit;

/// <summary>
/// Testes unitários, integrados e benchmarks para o caso de uso ProcessarPousoFimVooCasoDeUso.
/// Cobre os requisitos funcionais FR-001 a FR-005 e critérios de sucesso SC-001 a SC-003.
/// </summary>
public class ProcessarPousoFimVooCasoDeUsoTestes
{
    private readonly EspiaoPublicadorEventosVoo _espiaoPublicador = new();
    private readonly ProcessarPousoFimVooCasoDeUso _casoDeUso;
    private readonly ServicoFisicaVoo _servicoFisica = new();

    public ProcessarPousoFimVooCasoDeUsoTestes()
    {
        _casoDeUso = new ProcessarPousoFimVooCasoDeUso(_espiaoPublicador);
    }

    [Fact]
    public void Construtor_ComPublicadorNulo_DeveLancarDominioInvalidoException()
    {
        // Act & Assert
        Assert.Throws<DominioInvalidoException>(() => new ProcessarPousoFimVooCasoDeUso(null!));
    }

    [Fact]
    public void Executar_ComVooNulo_DeveLancarDominioInvalidoException()
    {
        // Arrange
        var estado = EstadoFisicoAeronave.CriarInicial(VetorVoo.Zero, VetorVoo.Zero, 0f);

        // Act & Assert
        Assert.Throws<DominioInvalidoException>(() => _casoDeUso.Executar(null!, estado));
    }

    [Fact]
    public void Executar_ComVooEmAndamentoNoAr_DeveRetornarCriarEmAndamentoENaoDispararEvento()
    {
        // Arrange: aeronave voando a 50m de altitude e 100m de avanço Z
        var voo = Voo.Iniciar(Aeronave.CriarPadrao());
        voo.Decolar();

        var estado = EstadoFisicoAeronave.CriarInicial(
            new VetorVoo(0f, 50f, 100f),
            new VetorVoo(0f, 2f, 20f),
            10.0f);

        // Act
        var resultado = _casoDeUso.Executar(voo, estado);

        // Assert
        Assert.Equal(StatusVoo.EmVoo, resultado.Status);
        Assert.False(resultado.AeronaveParou);
        Assert.Null(resultado.Resultado);
        Assert.Equal(100f, resultado.DistanciaFinalMetros);
        Assert.Equal(50f, resultado.AltitudeMaximaMetros);
        Assert.Equal(0, _espiaoPublicador.ChamadasPublicarVooConcluido);
    }

    [Fact]
    public void Executar_ComAeronaveDeslizandoNoSoloEmMovimento_DeveRetornarEmAndamentoENaoDispararEvento()
    {
        // Arrange: aeronave no solo deslizando a 5 m/s
        var voo = Voo.Iniciar(Aeronave.CriarPadrao());
        voo.Decolar();

        var estadoDeslizando = EstadoFisicoAeronave.Criar(
            new VetorVoo(0f, 0f, 200f),
            new VetorVoo(0f, 0f, 5.0f),
            0.0f,
            VetorVoo.Zero,
            true);

        // Act
        var resultado = _casoDeUso.Executar(voo, estadoDeslizando);

        // Assert
        Assert.Equal(StatusVoo.EmVoo, resultado.Status);
        Assert.False(resultado.AeronaveParou);
        Assert.Null(resultado.Resultado);
        Assert.Equal(0, _espiaoPublicador.ChamadasPublicarVooConcluido);
    }

    [Fact]
    public void Executar_ComAeronaveParadaNoSolo_DeveTransitarParaPousadoEDispararEvento()
    {
        // Arrange: aeronave em repouso no solo (NoSolo = true, Vz = 0)
        var voo = Voo.Iniciar(Aeronave.CriarPadrao());
        voo.Decolar();
        voo.AtualizarMetricas(250f, 80f, 12);

        var estadoParado = EstadoFisicoAeronave.Criar(
            new VetorVoo(0f, 0f, 250f),
            VetorVoo.Zero,
            0.0f,
            VetorVoo.Zero,
            true);

        // Act
        var resultado = _casoDeUso.Executar(voo, estadoParado);

        // Assert
        Assert.Equal(StatusVoo.Pousado, voo.Status);
        Assert.Equal(StatusVoo.Pousado, resultado.Status);
        Assert.True(resultado.AeronaveParou);
        Assert.NotNull(resultado.Resultado);
        Assert.Equal(250f, resultado.DistanciaFinalMetros);
        Assert.Equal(80f, resultado.AltitudeMaximaMetros);
        Assert.Equal(12, resultado.MoedasColetadas);

        // Disparo único de notificação de conclusão
        Assert.Equal(1, _espiaoPublicador.ChamadasPublicarVooConcluido);
        Assert.Equal(resultado, _espiaoPublicador.UltimoResultadoPublicado);
    }

    [Fact]
    public void Executar_IntegracaoPontaAPonta_VooDeslizeEParadaComEventoEmMenosDe10ms()
    {
        // Arrange: Simulação integrada partindo de 10m de altitude com trajetória descendente
        var voo = Voo.Iniciar(Aeronave.CriarPadrao());
        voo.Decolar();

        var estado = EstadoFisicoAeronave.CriarInicial(
            new VetorVoo(0f, 10f, 150f),
            new VetorVoo(0f, -4f, 12f),
            0.0f);

        var stopwatch = Stopwatch.StartNew();
        var passos = 0;
        var maxPassos = 1000;
        ResultadoFimVoo resultadoFinal = default;

        // Act: loop físico acionando atualização física e caso de uso de pouso
        while (voo.Status == StatusVoo.EmVoo && passos++ < maxPassos)
        {
            estado = _servicoFisica.SimularPasso(estado, ParametrosControlePiloto.Neutro, 1, 0.02f);
            resultadoFinal = _casoDeUso.Executar(voo, estado);
        }

        stopwatch.Stop();

        // Assert
        Assert.Equal(StatusVoo.Pousado, voo.Status);
        Assert.True(resultadoFinal.AeronaveParou);
        Assert.NotNull(resultadoFinal.Resultado);
        Assert.Equal(1, _espiaoPublicador.ChamadasPublicarVooConcluido);
        Assert.True(resultadoFinal.DistanciaFinalMetros > 150f, "A aeronave deve ter deslizado para frente até parar.");
        Assert.Equal(0f, estado.Posicao.Y);
        Assert.Equal(0f, estado.Velocidade.Z);
    }

    [Fact]
    public void Executar_ChamadasIdempotentesAposPouso_NaoDeveDuplicarDisparoDeEvento()
    {
        // Arrange: aeronave em repouso
        var voo = Voo.Iniciar(Aeronave.CriarPadrao());
        voo.Decolar();
        voo.AtualizarMetricas(300f, 90f, 5);

        var estadoParado = EstadoFisicoAeronave.Criar(
            new VetorVoo(0f, 0f, 300f),
            VetorVoo.Zero,
            0.0f,
            VetorVoo.Zero,
            true);

        // Act 1: primeira execução (consome pouso)
        var primeiroResultado = _casoDeUso.Executar(voo, estadoParado);

        // Act 2: execuções subsequentes na mesma instância (idempotência)
        var segundoResultado = _casoDeUso.Executar(voo, estadoParado);
        var terceiroResultado = _casoDeUso.Executar(voo, estadoParado);

        // Assert: resultados idênticos e o evento foi disparado apenas UMA vez
        Assert.Equal(1, _espiaoPublicador.ChamadasPublicarVooConcluido);
        Assert.Equal(primeiroResultado, segundoResultado);
        Assert.Equal(segundoResultado, terceiroResultado);
    }

    [Fact]
    public void Benchmark_ZeroAlocacaoDeMemoriaDuranteDeslizamentoEParada_SC003()
    {
        // Arrange: aquecimento para evitar JIT e alocações estáticas iniciais
        var vooAquecimento = Voo.Iniciar(Aeronave.CriarPadrao());
        vooAquecimento.Decolar();
        var estadoAquecimento = EstadoFisicoAeronave.Criar(
            new VetorVoo(0f, 0f, 100f),
            new VetorVoo(0f, 0f, 10f),
            0.0f,
            VetorVoo.Zero,
            true);

        for (var i = 0; i < 50; i++)
        {
            estadoAquecimento = _servicoFisica.SimularPasso(estadoAquecimento, ParametrosControlePiloto.Neutro, 1, 0.02f);
        }

        // Teste de 10.000 iterações puras de deslizamento no solo (SC-003)
        var estadoDeslizando = EstadoFisicoAeronave.Criar(
            new VetorVoo(0f, 0f, 100f),
            new VetorVoo(0f, 0f, 15f),
            0.0f,
            VetorVoo.Zero,
            true);

        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        var memoriaAntes = GC.GetAllocatedBytesForCurrentThread();

        for (var i = 0; i < 10000; i++)
        {
            estadoDeslizando = _servicoFisica.SimularPasso(estadoDeslizando, ParametrosControlePiloto.Neutro, 1, 0.001f);
        }

        var memoriaDepois = GC.GetAllocatedBytesForCurrentThread();
        var bytesAlocados = memoriaDepois - memoriaAntes;

        // Assert: zero bytes de alocação no heap no loop físico (SC-003)
        Assert.Equal(0L, bytesAlocados);
    }

    [Fact]
    public void Benchmark_LatenciaDisparoEventoAbaixoDe10ms_SC002()
    {
        // Arrange: aeronave preparada para pousar
        var voo = Voo.Iniciar(Aeronave.CriarPadrao());
        voo.Decolar();
        voo.AtualizarMetricas(200f, 50f, 8);

        var estadoParado = EstadoFisicoAeronave.Criar(
            new VetorVoo(0f, 0f, 200f),
            VetorVoo.Zero,
            0.0f,
            VetorVoo.Zero,
            true);

        // Medição de alta precisão
        var sw = Stopwatch.StartNew();
        var resultado = _casoDeUso.Executar(voo, estadoParado);
        sw.Stop();

        // Assert: latência estritamente inferior a 10 milissegundos (SC-002)
        Assert.True(sw.ElapsedMilliseconds < 10,
            $"A latência de encerramento e disparo de evento foi de {sw.ElapsedMilliseconds}ms, devendo ser < 10ms.");
        Assert.Equal(StatusVoo.Pousado, resultado.Status);
        Assert.Equal(1, _espiaoPublicador.ChamadasPublicarVooConcluido);
    }

    [Fact]
    public void Executar_AposStatusVooPousado_TentativaDeAcionarBoostOuPitchDeveSerBloqueada()
    {
        // Arrange: voo já concluído e aeronave parada
        var voo = Voo.Iniciar(Aeronave.CriarPadrao());
        voo.Decolar();
        voo.AtualizarMetricas(200f, 50f, 10);

        var estadoParado = EstadoFisicoAeronave.Criar(
            new VetorVoo(0f, 0f, 200f),
            VetorVoo.Zero,
            0.0f,
            VetorVoo.Zero,
            true);

        _casoDeUso.Executar(voo, estadoParado);
        Assert.Equal(StatusVoo.Pousado, voo.Status);

        // Act 1: Tentar simular passo físico com comandos táteis agressivos de boost e arfagem
        var comandoAgressivo = new ParametrosControlePiloto(1.0f, 45f, true);
        var proximoEstadoFisico = _servicoFisica.SimularPasso(estadoParado, comandoAgressivo, 10, 10, 0.05f, 0.05f);

        // Act 2: Tentar consumir combustível diretamente no voo pós-pouso
        var tempoQueima = voo.ConsumirCombustivel(0.1f, out var efetivo);

        // Act 3: Reavaliar caso de uso com o estado resultante
        var resultadoSubsequente = _casoDeUso.Executar(voo, proximoEstadoFisico);

        // Assert: Propulsor inativo, sem empuxo, sem queima de combustível, pitch travado e métricas inalteradas
        Assert.False(proximoEstadoFisico.Propulsor.EstaAtivo);
        Assert.Equal(0f, proximoEstadoFisico.Propulsor.EmpuxoNewtons);
        Assert.Equal(0f, tempoQueima);
        Assert.Equal(0f, efetivo);
        Assert.Equal(0f, proximoEstadoFisico.InclinacaoPitchGraus);
        Assert.Equal(200f, resultadoSubsequente.DistanciaFinalMetros);
        Assert.Equal(50f, resultadoSubsequente.AltitudeMaximaMetros);
        Assert.Equal(StatusVoo.Pousado, resultadoSubsequente.Status);
    }

    private class EspiaoPublicadorEventosVoo : IPublicadorEventosVoo
    {
        public int ChamadasPublicarVooConcluido { get; private set; }
        public ResultadoFimVoo UltimoResultadoPublicado { get; private set; }

        public void PublicarVooConcluido(ResultadoFimVoo resultado)
        {
            ChamadasPublicarVooConcluido++;
            UltimoResultadoPublicado = resultado;
        }
    }
}
