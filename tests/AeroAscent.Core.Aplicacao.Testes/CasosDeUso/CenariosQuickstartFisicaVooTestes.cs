namespace AeroAscent.Core.Aplicacao.Testes.CasosDeUso;

using System;
using System.Diagnostics;
using AeroAscent.Core.Aplicacao.CasosDeUso;
using AeroAscent.Core.Dominio.Entidades;
using AeroAscent.Core.Dominio.Enums;
using AeroAscent.Core.Dominio.ObjetosDeValor;
using AeroAscent.Core.Dominio.Servicos;
using Xunit;

/// <summary>
/// Testes de aceitação automatizados que validam integralmente os 6 cenários funcionais
/// descritos no guia de validação rápida (specs/003-fisica-voo-aerodinamica/quickstart.md).
/// </summary>
public class CenariosQuickstartFisicaVooTestes
{
    private readonly ServicoFisicaVoo _servicoFisica = new();
    private readonly AtualizarFisicaVooCasoDeUso _casoDeUso;

    public CenariosQuickstartFisicaVooTestes()
    {
        _casoDeUso = new AtualizarFisicaVooCasoDeUso(_servicoFisica);
    }

    [Fact]
    public void Cenario1_GanhoDeSustentacaoAoInclinarNarizParaCima_DeveGanharAltitudeESubir()
    {
        // 1. Estado inicial: 50m de altitude, 25 m/s horizontal
        var aero = Aeronave.CriarPadrao();
        var voo = Voo.Iniciar(aero);
        voo.Decolar();

        var estado = EstadoFisicoAeronave.CriarInicial(
            new VetorVoo(0f, 50f, 0f),
            new VetorVoo(0f, 0f, 25f),
            0.0f);

        var comandoSubida = ParametrosControlePiloto.Criar(1.0f); // Pitch up

        // 2. Simular 50 passos de 0.02s (1 segundo)
        for (var i = 0; i < 50; i++)
        {
            estado = _casoDeUso.Executar(voo, estado, comandoSubida, 0.02f);
        }

        // 3. Verificações
        Assert.True(estado.InclinacaoPitchGraus > 30.0f, $"Pitch ({estado.InclinacaoPitchGraus}) deve ter subido.");
        Assert.True(estado.Velocidade.Y > 0f, $"Velocidade vertical Vy ({estado.Velocidade.Y}) deve ser positiva.");
        Assert.True(estado.Posicao.Y > 50.0f, $"Altitude final ({estado.Posicao.Y}) deve ser superior a 50m.");
    }

    [Fact]
    public void Cenario2_GanhoDeVelocidadeEmMergulho_DeveAumentarVelocidadeEscalar()
    {
        // 1. Estado inicial: 100m de altitude, 15 m/s horizontal
        var aero = Aeronave.CriarPadrao();
        var voo = Voo.Iniciar(aero);
        voo.Decolar();

        var estado = EstadoFisicoAeronave.CriarInicial(
            new VetorVoo(0f, 100f, 0f),
            new VetorVoo(0f, 0f, 15f),
            0.0f);

        var comandoMergulho = ParametrosControlePiloto.Criar(-1.0f); // Mergulho

        // 2. Simular 75 passos de 0.02s (1.5 segundos)
        for (var i = 0; i < 75; i++)
        {
            estado = _casoDeUso.Executar(voo, estado, comandoMergulho, 0.02f);
        }

        // 3. Verificações
        Assert.True(estado.VelocidadeEscalar > 15.0f,
            $"Velocidade ({estado.VelocidadeEscalar}) deve ter aumentado em mergulho.");
        Assert.True(estado.Posicao.Y < 100.0f, "Altitude deve ter descido.");
    }

    [Fact]
    public void Cenario3_ReducaoDeArrastoPorMelhoriaDeAerodinamica_Nivel5DevePlanarMaisLonge()
    {
        // 1. Duas aeronaves: Nível 1 vs Nível 5 de aerodinâmica
        var aero1 = new Aeronave(Guid.NewGuid(), 1, 1, 1, 1);
        var aero5 = new Aeronave(Guid.NewGuid(), 1, 5, 1, 1);

        var voo1 = Voo.Iniciar(aero1);
        var voo5 = Voo.Iniciar(aero5);
        voo1.Decolar();
        voo5.Decolar();

        // 2. Lançamento idêntico a 25 m/s a 35°
        var velInicial = _servicoFisica.CalcularImpulsoInicial(1, 1.0f);
        var estado1 = EstadoFisicoAeronave.CriarInicial(VetorVoo.Zero, velInicial, 35.0f);
        var estado5 = EstadoFisicoAeronave.CriarInicial(VetorVoo.Zero, velInicial, 35.0f);

        // 3. Simular até primeiro toque no solo
        var passos1 = 0;
        while (!estado1.NoSolo && passos1++ < 2000)
        {
            estado1 = _casoDeUso.Executar(voo1, estado1, ParametrosControlePiloto.Neutro, 0.02f);
        }

        var passos5 = 0;
        while (!estado5.NoSolo && passos5++ < 2000)
        {
            estado5 = _casoDeUso.Executar(voo5, estado5, ParametrosControlePiloto.Neutro, 0.02f);
        }

        // 4. Verificações: Nível 5 percorreu distância horizontal significativamente maior
        Assert.True(voo5.DistanciaPercorrida > voo1.DistanciaPercorrida,
            $"Distância nível 5 ({voo5.DistanciaPercorrida}) deve superar nível 1 ({voo1.DistanciaPercorrida}).");
    }

    [Fact]
    public void Cenario4_ComportamentoDeEstolAcolhedor_NaoDeveZerarSustentacaoBruscamente()
    {
        // 1. Estado inicial em estol: velocidade baixa e pitch elevado (50°)
        var aero = Aeronave.CriarPadrao();
        var voo = Voo.Iniciar(aero);
        voo.Decolar();

        var estado = EstadoFisicoAeronave.CriarInicial(
            new VetorVoo(0f, 50f, 0f),
            new VetorVoo(0f, 1f, 4f),
            50.0f);

        // 2. Simular 50 passos (1 segundo) com comando neutro
        for (var i = 0; i < 50; i++)
        {
            estado = _casoDeUso.Executar(voo, estado, ParametrosControlePiloto.Neutro, 0.02f);
        }

        // 3. Verificações: sustentação não zerou abruptamente, aeronave desce suavemente e autoestabiliza
        Assert.False(float.IsNaN(estado.Velocidade.Y));
        Assert.False(float.IsInfinity(estado.Velocidade.Y));
        Assert.True(estado.InclinacaoPitchGraus < 50.0f, "Autoestabilização deve atuar reduzindo o pitch excessivo.");
    }

    [Fact]
    public void Cenario5_DeslizamentoNoSoloComAtritoEFinalizacaoComPouso_DevePararEAcionarPousar()
    {
        // 1. Aeronave tocando solo a 200m de avanço com velocidade vertical descendente
        var aero = Aeronave.CriarPadrao();
        var voo = Voo.Iniciar(aero);
        voo.Decolar();

        var estado = EstadoFisicoAeronave.CriarInicial(
            new VetorVoo(0f, 0.5f, 200f),
            new VetorVoo(0f, -3f, 10f),
            0.0f);

        // 2. Executar passos contínuos até repouso
        var passos = 0;
        while (voo.Status == StatusVoo.EmVoo && passos < 500)
        {
            estado = _casoDeUso.Executar(voo, estado, ParametrosControlePiloto.Neutro, 0.02f);
            passos++;
        }

        // 3. Verificações: sessão transitou para Pousado com Resultado consolidado
        Assert.Equal(StatusVoo.Pousado, voo.Status);
        Assert.NotNull(voo.Resultado);
        Assert.True(voo.DistanciaPercorrida > 200f, "Aeronave deve ter deslizado alguns metros após tocar o solo.");
        Assert.Equal(0f, estado.Posicao.Y);
        Assert.Equal(0f, estado.Velocidade.Z);
    }

    [Fact]
    public void Cenario6_BenchmarkDePerformanceEZeroAlocacao_DeveExecutarAbaixoDe005msEZeroAlocacao()
    {
        // 1. Preparar simulação em altitude para voo contínuo sem pouso prematuro
        var aero = Aeronave.CriarPadrao();
        var voo = Voo.Iniciar(aero);
        voo.Decolar();

        var estado = EstadoFisicoAeronave.CriarInicial(
            new VetorVoo(0f, 50000f, 0f),
            new VetorVoo(0f, 0f, 25f),
            0.0f);

        // Warm-up JIT
        for (var i = 0; i < 100; i++)
        {
            estado = _casoDeUso.Executar(voo, estado, ParametrosControlePiloto.Neutro, 0.02f);
        }

        // 2. Medição estrita de 10.000 passos
        var sw = Stopwatch.StartNew();
        var memoriaAntes = GC.GetAllocatedBytesForCurrentThread();

        for (var i = 0; i < 10000; i++)
        {
            estado = _casoDeUso.Executar(voo, estado, ParametrosControlePiloto.Neutro, 0.02f);
        }

        var memoriaDepois = GC.GetAllocatedBytesForCurrentThread();
        sw.Stop();
        var bytesAlocados = memoriaDepois - memoriaAntes;

        // SC-001: < 0.05ms por passo (10.000 passos < 500ms)
        Assert.True(sw.ElapsedMilliseconds < 500,
            $"10.000 passos executados em {sw.ElapsedMilliseconds}ms (esperado < 500ms). Média por passo: {sw.Elapsed.TotalMilliseconds / 10000:F4}ms.");

        // SC-002: Zero alocação de memória no heap
        Assert.Equal(0, bytesAlocados);
    }
}
