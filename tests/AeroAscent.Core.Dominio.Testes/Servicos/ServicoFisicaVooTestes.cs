namespace AeroAscent.Core.Dominio.Testes.Servicos;

using System;
using System.Diagnostics;
using AeroAscent.Core.Dominio.Excecoes;
using AeroAscent.Core.Dominio.ObjetosDeValor;
using AeroAscent.Core.Dominio.Servicos;
using Xunit;

/// <summary>
/// Testes unitários para o cálculo de física cinemática de lançamento da catapulta (ServicoFisicaVoo).
/// </summary>
public class ServicoFisicaVooTestes
{
    private readonly ServicoFisicaVoo _servicoFisica = new();

    [Fact]
    public void CalcularImpulsoInicial_Nivel1Precisao100_DeveRetornarVetor3DCom25MetrosPorSegundoEAngulo35Graus()
    {
        // Arrange
        const int nivelCatapulta = 1;
        const float precisao = 1.0f;
        const float velocidadeEsperada = 25.0f;
        var radianos35 = 35.0f * MathF.PI / 180.0f;
        var yEsperado = velocidadeEsperada * MathF.Sin(radianos35);
        var zEsperado = velocidadeEsperada * MathF.Cos(radianos35);

        // Act
        var impulso = _servicoFisica.CalcularImpulsoInicial(nivelCatapulta, precisao);

        // Assert
        Assert.Equal(0f, impulso.X);
        Assert.InRange(impulso.Y, yEsperado - 0.05f, yEsperado + 0.05f);
        Assert.InRange(impulso.Z, zEsperado - 0.05f, zEsperado + 0.05f);
        Assert.InRange(impulso.Magnitude(), velocidadeEsperada - 0.05f, velocidadeEsperada + 0.05f);
    }

    [Fact]
    public void CalcularImpulsoInicial_Nivel3Precisao100_DeveEscalonarVelocidadeLinearmente()
    {
        // Arrange
        const int nivelCatapulta = 3; // 1 + (3 - 1) * 0.25 = 1.5x
        const float precisao = 1.0f;
        const float velocidadeEsperada = 25.0f * 1.5f; // 37.5 m/s

        // Act
        var impulso = _servicoFisica.CalcularImpulsoInicial(nivelCatapulta, precisao);

        // Assert
        Assert.Equal(0f, impulso.X);
        Assert.InRange(impulso.Magnitude(), velocidadeEsperada - 0.05f, velocidadeEsperada + 0.05f);
    }

    [Fact]
    public void CalcularImpulsoInicial_ComPrecisao50PorCento_DeveReduzirImpulsoProporcionalmente()
    {
        // Arrange
        const int nivelCatapulta = 1;
        const float precisao = 0.5f;
        const float velocidadeEsperada = 25.0f * 0.5f; // 12.5 m/s

        // Act
        var impulso = _servicoFisica.CalcularImpulsoInicial(nivelCatapulta, precisao);

        // Assert
        Assert.InRange(impulso.Magnitude(), velocidadeEsperada - 0.05f, velocidadeEsperada + 0.05f);
    }

    [Fact]
    public void CalcularImpulsoInicial_ComPrecisaoAbaixoDoPiso_DeveAplicarPisoMinimoDe10PorCento()
    {
        // Arrange
        const int nivelCatapulta = 1;
        const float precisaoNula = 0.0f;
        const float velocidadeEsperada = 25.0f * 0.10f; // 2.5 m/s

        // Act
        var impulso = _servicoFisica.CalcularImpulsoInicial(nivelCatapulta, precisaoNula);

        // Assert
        Assert.InRange(impulso.Magnitude(), velocidadeEsperada - 0.05f, velocidadeEsperada + 0.05f);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(11)]
    public void CalcularImpulsoInicial_ComNivelInvalido_DeveLancarDominioInvalidoException(int nivelInvalido)
    {
        // Act & Assert
        Assert.Throws<DominioInvalidoException>(() =>
            _servicoFisica.CalcularImpulsoInicial(nivelInvalido, 1.0f));
    }

    [Fact]
    public void CalcularImpulsoInicial_Benchmark10000Calculos_DeveExecutarEmMenosDe100Milissegundos()
    {
        // Arrange
        var sw = Stopwatch.StartNew();

        // Act
        for (var i = 0; i < 10000; i++)
        {
            var _ = _servicoFisica.CalcularImpulsoInicial(1 + (i % 10), 0.75f);
        }

        sw.Stop();

        // Assert (Critério SC-001)
        Assert.True(sw.ElapsedMilliseconds < 100, $"Tempo de 10.000 cálculos físicos foi de {sw.ElapsedMilliseconds}ms, esperado < 100ms.");
    }

    [Theory]
    [InlineData(0f, 0f)]
    [InlineData(10f, 0.75f)]
    [InlineData(20f, 1.50f)]
    [InlineData(-10f, -0.75f)]
    public void CalcularCoeficienteSustentacao_FaixaLinearAbaixoDe20Graus_DeveSerLinear(float alfa, float esperado)
    {
        // Act
        var cl = ServicoFisicaVoo.CalcularCoeficienteSustentacao(alfa);

        // Assert
        Assert.InRange(cl, esperado - 0.01f, esperado + 0.01f);
    }

    [Fact]
    public void CalcularCoeficienteSustentacao_AcimaDe20GrausEstol_DeveDecairSuavementeSemZerar()
    {
        // Act
        var clEstol25 = ServicoFisicaVoo.CalcularCoeficienteSustentacao(25f);
        var clEstol45 = ServicoFisicaVoo.CalcularCoeficienteSustentacao(45f);
        var clEstol90 = ServicoFisicaVoo.CalcularCoeficienteSustentacao(90f);

        // Assert
        Assert.True(clEstol25 < 1.50f && clEstol25 > 1.30f, $"CL a 25° ({clEstol25}) deve decair suavemente.");
        Assert.True(clEstol45 < clEstol25 && clEstol45 > 0.8f, $"CL a 45° ({clEstol45}) deve continuar suave.");
        Assert.InRange(clEstol90, 0.29f, 0.31f); // Piso mínimo acolhedor de 0.3
    }

    [Fact]
    public void SimularPasso_ComPitchPositivo15Graus_DeveGerarSustentacaoVerticalPositiva()
    {
        // Arrange: aeronave voando horizontalmente a 20 m/s
        var estadoInicial = EstadoFisicoAeronave.CriarInicial(
            new VetorVoo(0f, 50f, 0f),
            new VetorVoo(0f, 0f, 20f),
            15.0f); // Nariz apontado 15° para cima

        var controle = ParametrosControlePiloto.Neutro;

        // Act: 1 passo de 0.02s
        var proximoEstado = _servicoFisica.SimularPasso(estadoInicial, controle, 1, 0.02f);

        // Assert
        Assert.True(proximoEstado.ForcaResultante.Y > 0f, "A força resultante vertical deve ser positiva com pitch de 15°.");
        Assert.True(proximoEstado.Velocidade.Y > 0f, "A velocidade vertical Vy deve se tornar positiva.");
        Assert.True(proximoEstado.Posicao.Y > estadoInicial.Posicao.Y, "A altitude deve aumentar.");
    }

    [Fact]
    public void SimularPasso_ComPitchNegativoMergulho_DeveAumentarVelocidadeEscalar()
    {
        // Arrange: aeronave em alta altitude mergulhando
        var estadoInicial = EstadoFisicoAeronave.CriarInicial(
            new VetorVoo(0f, 100f, 0f),
            new VetorVoo(0f, -5f, 10f),
            -30.0f); // Nariz apontando para baixo (-30°)

        var controle = ParametrosControlePiloto.Criar(-1.0f); // Mergulho ativo

        // Act: simular 0.5s de mergulho (25 passos de 0.02s)
        var estado = estadoInicial;
        for (var i = 0; i < 25; i++)
        {
            estado = _servicoFisica.SimularPasso(estado, controle, 1, 0.02f);
        }

        // Assert
        Assert.True(estado.VelocidadeEscalar > estadoInicial.VelocidadeEscalar,
            $"Velocidade final ({estado.VelocidadeEscalar}) deve ser maior que inicial ({estadoInicial.VelocidadeEscalar}).");
        Assert.True(estado.Posicao.Y < estadoInicial.Posicao.Y, "Altitude deve ter descido.");
    }

    [Fact]
    public void SimularPasso_ComComandoNeutro_DeveAutoestabilizarNarizComVetorVelocidade()
    {
        // Arrange: nariz em 40° mas velocidade quase puramente horizontal (trajetória ~0°)
        var estadoInicial = EstadoFisicoAeronave.CriarInicial(
            new VetorVoo(0f, 50f, 0f),
            new VetorVoo(0f, 0f, 25f),
            40.0f);

        var controle = ParametrosControlePiloto.Neutro;

        // Act: 1 segundo de voo sem comandos do piloto (50 passos)
        var estado = estadoInicial;
        for (var i = 0; i < 50; i++)
        {
            estado = _servicoFisica.SimularPasso(estado, controle, 1, 0.02f);
        }

        // Assert: o pitch deve ter convergido em direção à trajetória
        Assert.True(estado.InclinacaoPitchGraus < 40.0f,
            $"O pitch ({estado.InclinacaoPitchGraus}) deve ter decaído em direção à trajetória.");
    }

    [Fact]
    public void SimularPasso_NoSolo_DeveDesacelerarPorAtrito()
    {
        // Arrange: aeronave deslizando no solo a 10 m/s
        var estadoNoSolo = EstadoFisicoAeronave.Criar(
            new VetorVoo(0f, 0f, 100f),
            new VetorVoo(0f, 0f, 10f),
            5.0f,
            VetorVoo.Zero,
            true);

        var controle = ParametrosControlePiloto.Neutro;

        // Act: 1 passo de 0.1s (atrito ~2.943 m/s² deve reduzir vz em ~0.294 m/s)
        var proximo = _servicoFisica.SimularPasso(estadoNoSolo, controle, 1, 0.1f);

        // Assert
        Assert.True(proximo.NoSolo);
        Assert.Equal(0f, proximo.Posicao.Y);
        Assert.Equal(0f, proximo.Velocidade.Y);
        Assert.InRange(proximo.Velocidade.Z, 9.6f, 9.8f);
    }

    [Fact]
    public void SimularPasso_NoSoloAbaixoDoLimiarCanonico015_DevePararCompletamente()
    {
        // Arrange: aeronave no solo com velocidade horizontal abaixo de 0.15 m/s
        var estadoNoSolo = EstadoFisicoAeronave.Criar(
            new VetorVoo(0f, 0f, 150f),
            new VetorVoo(0f, 0f, 0.14f),
            0.0f,
            VetorVoo.Zero,
            true);

        // Act
        var proximo = _servicoFisica.SimularPasso(estadoNoSolo, ParametrosControlePiloto.Neutro, 1, 0.02f);

        // Assert: parada cinemática imediata abaixo de 0.15 m/s
        Assert.True(proximo.NoSolo);
        Assert.Equal(0f, proximo.Velocidade.Z);
    }

    [Fact]
    public void SimularPasso_NoSoloAcimaDoLimiarCanonico015_NaoDevePararImediatamente()
    {
        // Arrange: aeronave no solo com velocidade horizontal acima de 0.15 m/s (0.40 m/s)
        var estadoNoSolo = EstadoFisicoAeronave.Criar(
            new VetorVoo(0f, 0f, 150f),
            new VetorVoo(0f, 0f, 0.40f),
            0.0f,
            VetorVoo.Zero,
            true);

        // Act: em 0.02s o atrito desacelera ~0.059 m/s, resultando em ~0.341 m/s > 0.15 m/s
        var proximo = _servicoFisica.SimularPasso(estadoNoSolo, ParametrosControlePiloto.Neutro, 1, 0.02f);

        // Assert: não deve congelar ainda pois continua acima de 0.15 m/s
        Assert.True(proximo.NoSolo);
        Assert.True(proximo.Velocidade.Z > 0.15f);
        Assert.True(proximo.Velocidade.Z < 0.40f);
    }

    [Fact]
    public void SimularPasso_TransitandoParaSolo_DeveTravarAltitudeEmZeroEZerarVy()
    {
        // Arrange: aeronave caindo a 1 metro do solo com Vy = -10 m/s
        var estadoDescendo = EstadoFisicoAeronave.CriarInicial(
            new VetorVoo(0f, 0.1f, 50f),
            new VetorVoo(0f, -10f, 15f),
            0.0f);

        // Act: passo de 0.05s levaria Y para negativo
        var proximo = _servicoFisica.SimularPasso(estadoDescendo, ParametrosControlePiloto.Neutro, 1, 0.05f);

        // Assert
        Assert.True(proximo.NoSolo);
        Assert.Equal(0f, proximo.Posicao.Y);
        Assert.Equal(0f, proximo.Velocidade.Y);
    }

    [Fact]
    public void SimularPasso_DeslizandoNoSoloComPitchPositivo_DeveNivelarSuavementePitchA15GrausPorSegundo()
    {
        // Arrange: aeronave deslizando no solo com pitch positivo de 30°
        var estadoNoSolo = EstadoFisicoAeronave.Criar(
            new VetorVoo(0f, 0f, 100f),
            new VetorVoo(0f, 0f, 10f),
            30.0f,
            VetorVoo.Zero,
            true);

        // Act: dt = 0.5s -> redução de 15°/s * 0.5s = 7.5° -> novo pitch = 22.5°
        var proximo = _servicoFisica.SimularPasso(estadoNoSolo, ParametrosControlePiloto.Neutro, 1, 0.5f);

        // Assert
        Assert.True(proximo.NoSolo);
        Assert.Equal(22.5f, proximo.InclinacaoPitchGraus, 2);
    }

    [Fact]
    public void SimularPasso_DeslizandoNoSoloComPitchNegativo_DeveNivelarSuavementePitchParaZero()
    {
        // Arrange: aeronave deslizando no solo com pitch negativo de -10°
        var estadoNoSolo = EstadoFisicoAeronave.Criar(
            new VetorVoo(0f, 0f, 100f),
            new VetorVoo(0f, 0f, 10f),
            -10.0f,
            VetorVoo.Zero,
            true);

        // Act: dt = 0.5s -> incremento de 15°/s * 0.5s = 7.5° -> novo pitch = -2.5°
        var proximo = _servicoFisica.SimularPasso(estadoNoSolo, ParametrosControlePiloto.Neutro, 1, 0.5f);

        // Assert
        Assert.True(proximo.NoSolo);
        Assert.Equal(-2.5f, proximo.InclinacaoPitchGraus, 2);
    }

    [Fact]
    public void SimularPasso_DeslizandoNoSoloPitchPequeno_DeveCravarEmZeroGraus()
    {
        // Arrange: pitch pequeno de 5°, passo de 0.5s (redução teórica de 7.5° superaria 0°)
        var estadoNoSolo = EstadoFisicoAeronave.Criar(
            new VetorVoo(0f, 0f, 100f),
            new VetorVoo(0f, 0f, 10f),
            5.0f,
            VetorVoo.Zero,
            true);

        // Act
        var proximo = _servicoFisica.SimularPasso(estadoNoSolo, ParametrosControlePiloto.Neutro, 1, 0.5f);

        // Assert: crava exatamente em 0°
        Assert.True(proximo.NoSolo);
        Assert.Equal(0f, proximo.InclinacaoPitchGraus);
    }

    [Fact]
    public void SimularPasso_AposParadaTotal_DeveManterCongelamentoCinematicoAbsoluto()
    {
        // Arrange: aeronave parada no solo
        var estadoParado = EstadoFisicoAeronave.Criar(
            new VetorVoo(0f, 0f, 250f),
            VetorVoo.Zero,
            0.0f,
            VetorVoo.Zero,
            true);

        // Act: simular múltiplos passos com comandos ativos de subida e boost
        var controleTentativa = new ParametrosControlePiloto(1.0f, 30f, true);
        var estado = estadoParado;
        for (var i = 0; i < 20; i++)
        {
            estado = _servicoFisica.SimularPasso(estado, controleTentativa, 5, 5, 0.05f, 0.05f);
        }

        // Assert: imutabilidade cinemática e bloqueio total de controles
        Assert.True(estado.NoSolo);
        Assert.Equal(250f, estado.Posicao.Z);
        Assert.Equal(0f, estado.Posicao.Y);
        Assert.Equal(0f, estado.Velocidade.Z);
        Assert.Equal(0f, estado.Velocidade.Y);
        Assert.Equal(0f, estado.InclinacaoPitchGraus);
        Assert.False(estado.Propulsor.EstaAtivo);
        Assert.Equal(0f, estado.Propulsor.EmpuxoNewtons);
    }

    [Fact]
    public void SimularVooAteParada_CenarioIndependenteUS1_DevePararSuavementeSemTranspassarSolo()
    {
        // Arrange: Cenario de teste independente da US1:
        // Y = 0.5m, Vy = -3.0 m/s, Vz = 10.0 m/s, pitch = 5.0°
        var estado = EstadoFisicoAeronave.CriarInicial(
            new VetorVoo(0f, 0.5f, 50f),
            new VetorVoo(0f, -3.0f, 10.0f),
            5.0f);

        var controle = ParametrosControlePiloto.Neutro;
        var dt = 0.02f;
        var iteracoesMaximas = 300; // 6 segundos de simulação

        var tocouSolo = false;

        // Act
        for (var i = 0; i < iteracoesMaximas; i++)
        {
            estado = _servicoFisica.SimularPasso(estado, controle, 1, dt);

            // Invariante: Y nunca pode ser negativo (SC-001)
            Assert.True(estado.Posicao.Y >= 0f, $"Altitude Y ({estado.Posicao.Y}) não pode ser negativa!");

            if (estado.NoSolo)
            {
                tocouSolo = true;
                Assert.Equal(0f, estado.Posicao.Y);
                Assert.Equal(0f, estado.Velocidade.Y);
            }

            if (estado.NoSolo && estado.Velocidade.Z == 0f)
            {
                break;
            }
        }

        // Assert
        Assert.True(tocouSolo, "A aeronave deveria ter tocado o solo.");
        Assert.True(estado.NoSolo, "O estado final deve ser no solo.");
        Assert.Equal(0f, estado.Velocidade.Z);
        Assert.Equal(0f, estado.InclinacaoPitchGraus);
        Assert.True(estado.Posicao.Z > 50f, "A aeronave deve ter deslizado para frente antes de parar.");
    }

    [Fact]
    public void SimularPasso_QuedaEmMergulhoSevero_DeveAbsorverVerticalmenteSemPenetracaoDeSolo()
    {
        // Arrange: mergulho vertical severo (-80 m/s) com pitch negativo (-60°)
        var estadoMergulho = EstadoFisicoAeronave.CriarInicial(
            new VetorVoo(0f, 0.5f, 100f),
            new VetorVoo(0f, -80.0f, 5.0f),
            -60.0f);

        // Act
        var proximo = _servicoFisica.SimularPasso(estadoMergulho, ParametrosControlePiloto.Neutro, 1, 0.02f);

        // Assert: absorção total vertical sem penetração
        Assert.True(proximo.NoSolo);
        Assert.Equal(0f, proximo.Posicao.Y);
        Assert.Equal(0f, proximo.Velocidade.Y);
        Assert.True(proximo.InclinacaoPitchGraus > -60.0f, "Pitch deve iniciar nivelamento em direção a 0°.");
    }

    [Fact]
    public void SimularPasso_NoSoloComCombustivelRestanteETentativaDeBoost_DeveManterPropulsorInativo()
    {
        // Arrange: aeronave deslizando no solo com combustível (15 de 20 litros)
        var propulsorComCombustivel = EstadoPropulsor.CriarInativo(15f, 20f, 5f);
        var estadoNoSolo = new EstadoFisicoAeronave(
            new VetorVoo(0f, 0f, 100f),
            new VetorVoo(0f, 0f, 8.0f),
            0.0f,
            VetorVoo.Zero,
            true,
            propulsorComCombustivel);

        var controleComBoost = new ParametrosControlePiloto(0f, 30f, true);

        // Act: tenta acionar boost por 0.05s
        var proximo = _servicoFisica.SimularPasso(estadoNoSolo, controleComBoost, 1, 5, 0.05f, 0.05f);

        // Assert: propulsor deve permanecer inativo e combustível não deve ser consumido
        Assert.True(proximo.NoSolo);
        Assert.False(proximo.Propulsor.EstaAtivo);
        Assert.Equal(0f, proximo.Propulsor.EmpuxoNewtons);
        Assert.Equal(15f, proximo.Propulsor.CombustivelRestante);
    }

    [Fact]
    public void SimularPasso_NoSoloComComandoDePitch_DeveIgnorarComandoENivelarParaZero()
    {
        // Arrange: aeronave no solo com pitch = 20°, comando tentando puxar nariz para cima (+1.0)
        var estadoNoSolo = EstadoFisicoAeronave.Criar(
            new VetorVoo(0f, 0f, 100f),
            new VetorVoo(0f, 0f, 5.0f),
            20.0f,
            VetorVoo.Zero,
            true);

        var comandoPuxarParaCima = new ParametrosControlePiloto(1.0f, 30f, false);

        // Act: 0.5s de simulação no solo
        var proximo = _servicoFisica.SimularPasso(estadoNoSolo, comandoPuxarParaCima, 1, 0.5f);

        // Assert: comando ignorado, pitch nivelou 15°/s * 0.5s = 7.5° -> 12.5°
        Assert.True(proximo.NoSolo);
        Assert.Equal(12.5f, proximo.InclinacaoPitchGraus, 2);
    }

    [Fact]
    public void SimularPasso_Nivel5Aerodinamica_DeveSofrerMenosArrastoQueNivel1()
    {
        // Arrange: Duas aeronaves idênticas em velocidade horizontal alta (30 m/s)
        var estadoInicial = EstadoFisicoAeronave.CriarInicial(
            new VetorVoo(0f, 100f, 0f),
            new VetorVoo(0f, 0f, 30f),
            0.0f);

        var controle = ParametrosControlePiloto.Neutro;

        // Act: Simular 50 passos (1 segundo) para nível 1 e nível 5
        var estadoNivel1 = estadoInicial;
        var estadoNivel5 = estadoInicial;

        for (var i = 0; i < 50; i++)
        {
            estadoNivel1 = _servicoFisica.SimularPasso(estadoNivel1, controle, 1, 0.02f);
            estadoNivel5 = _servicoFisica.SimularPasso(estadoNivel5, controle, 5, 0.02f);
        }

        // Assert: Nível 5 deve ter sofrido menor arrasto frontal, mantendo mais velocidade Z e avançando mais
        Assert.True(estadoNivel5.Velocidade.Z > estadoNivel1.Velocidade.Z,
            $"Vz nível 5 ({estadoNivel5.Velocidade.Z}) deve ser maior que nível 1 ({estadoNivel1.Velocidade.Z}).");
        Assert.True(estadoNivel5.Posicao.Z > estadoNivel1.Posicao.Z,
            $"Distância Z nível 5 ({estadoNivel5.Posicao.Z}) deve ser maior que nível 1 ({estadoNivel1.Posicao.Z}).");
    }

    [Fact]
    public void CalcularProximoPasso_Nivel10Aerodinamica_DeveReterMaisVelocidadeQueNivel1()
    {
        // Arrange
        var velInicial = new VetorVoo(0f, 5f, 25f);

        // Act
        var velNivel1 = _servicoFisica.CalcularProximoPasso(velInicial, 5.0f, 1, 0.1f);
        var velNivel10 = _servicoFisica.CalcularProximoPasso(velInicial, 5.0f, 10, 0.1f);

        // Assert
        Assert.True(velNivel10.Magnitude() > velNivel1.Magnitude(),
            $"Velocidade com nível 10 ({velNivel10.Magnitude()}) deve ser maior que nível 1 ({velNivel1.Magnitude()}).");
    }

    [Fact]
    public void SimularPasso_Benchmark10000Passos_DeveExecutarEmMenosDe500MilissegundosEComZeroAlocacaoNoHeap()
    {
        // Arrange
        var estado = EstadoFisicoAeronave.CriarInicial(
            new VetorVoo(0f, 50f, 0f),
            new VetorVoo(0f, 2f, 20f),
            10.0f);
        var controle = ParametrosControlePiloto.Neutro;

        // Warm-up JIT
        for (var i = 0; i < 100; i++)
        {
            estado = _servicoFisica.SimularPasso(estado, controle, 1, 0.02f);
        }

        // Medição estrita de memória alocada no heap
        var sw = Stopwatch.StartNew();
        var memoriaAntes = GC.GetAllocatedBytesForCurrentThread();

        for (var i = 0; i < 10000; i++)
        {
            estado = _servicoFisica.SimularPasso(estado, controle, 1 + (i % 10), 0.02f);
        }

        var memoriaDepois = GC.GetAllocatedBytesForCurrentThread();
        sw.Stop();
        var bytesAlocados = memoriaDepois - memoriaAntes;

        // Assert (SC-001: < 0.05ms por passo => 10.000 passos < 500ms; média arcade esperada < 0.01ms)
        Assert.True(sw.ElapsedMilliseconds < 500,
            $"Tempo de 10.000 passos foi {sw.ElapsedMilliseconds}ms (esperado < 500ms). Média: {sw.Elapsed.TotalMilliseconds / 10000:F4}ms por passo.");

        // Assert (SC-002: GC Alloc = 0 bytes no loop contínuo de física)
        Assert.Equal(0, bytesAlocados);
    }

    [Theory]
    [InlineData(1, 120.0f)]
    [InlineData(2, 156.0f)]
    [InlineData(3, 192.0f)]
    [InlineData(5, 264.0f)]
    [InlineData(10, 444.0f)]
    public void CalcularEmpuxoMotor_NiveisValidos_DeveRetornarEmpuxoEscalonadoCorretamente(int nivelMotor, float empuxoEsperado)
    {
        // Act
        var empuxo = _servicoFisica.CalcularEmpuxoMotor(nivelMotor);

        // Assert
        Assert.Equal(empuxoEsperado, empuxo, precision: 2);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(11)]
    public void CalcularEmpuxoMotor_NivelInvalido_DeveLancarDominioInvalidoException(int nivelInvalido)
    {
        // Act & Assert
        Assert.Throws<DominioInvalidoException>(() => _servicoFisica.CalcularEmpuxoMotor(nivelInvalido));
    }

    [Fact]
    public void SimularPasso_ComBoostAtivo_NivelMotor3GeraAceleracaoSuperiorAoNivel1()
    {
        // Arrange
        var estadoInicial = EstadoFisicoAeronave.CriarInicial(
            new VetorVoo(0f, 100f, 0f),
            new VetorVoo(0f, 0f, 20f),
            0.0f);

        var controle = new ParametrosControlePiloto(0f, ParametrosControlePiloto.TAXA_ANGULAR_PADRAO, acionarBoost: true);

        // Act: Simula 1 passo com motor 1 vs motor 3
        var resultadoMotor1 = _servicoFisica.SimularPasso(estadoInicial, controle, 1, 1, 0.1f, 0.1f);
        var resultadoMotor3 = _servicoFisica.SimularPasso(estadoInicial, controle, 1, 3, 0.1f, 0.1f);

        // Assert
        Assert.True(resultadoMotor3.Velocidade.Z > resultadoMotor1.Velocidade.Z,
            $"A velocidade com motor 3 ({resultadoMotor3.Velocidade.Z}) deve superar motor 1 ({resultadoMotor1.Velocidade.Z}).");
    }

    [Fact]
    public void SimularPasso_ComPitch30GrausEBoost_DeveDecomporEmpuxoNosEixosY_E_Z()
    {
        // Arrange: Bico inclinado para cima em 30 graus
        var estadoInicial = EstadoFisicoAeronave.CriarInicial(
            new VetorVoo(0f, 100f, 0f),
            new VetorVoo(0f, 0f, 20f),
            30.0f);

        var controle = new ParametrosControlePiloto(0f, ParametrosControlePiloto.TAXA_ANGULAR_PADRAO, acionarBoost: true);

        // Act
        var estadoComBoost = _servicoFisica.SimularPasso(estadoInicial, controle, 1, 1, 0.1f, 0.1f);
        var estadoSemBoost = _servicoFisica.SimularPasso(estadoInicial, controle, 1, 1, 0.0f, 0.1f);

        // Assert: Componente Y e Z receberam empuxo positivo comparado a sem boost
        Assert.True(estadoComBoost.Velocidade.Y > estadoSemBoost.Velocidade.Y,
            "Empuxo com pitch 30° deve adicionar velocidade vertical (Y).");
        Assert.True(estadoComBoost.Velocidade.Z > estadoSemBoost.Velocidade.Z,
            "Empuxo com pitch 30° deve adicionar velocidade horizontal (Z).");
    }

    [Fact]
    public void SimularPasso_ComBoostAtivoContinuo_NaoDeveAlocarMemoriaNoHeap()
    {
        // Arrange
        var estado = EstadoFisicoAeronave.CriarInicial(
            new VetorVoo(0f, 1000f, 0f),
            new VetorVoo(0f, 0f, 30f),
            15.0f);

        var controle = new ParametrosControlePiloto(0f, ParametrosControlePiloto.TAXA_ANGULAR_PADRAO, acionarBoost: true);

        // Warm-up JIT
        for (var i = 0; i < 100; i++)
        {
            estado = _servicoFisica.SimularPasso(estado, controle, 3, 5, 0.02f, 0.02f);
        }

        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        // Medição estrita de memória alocada no heap
        var memoriaAntes = GC.GetAllocatedBytesForCurrentThread();

        // Act: 10.000 iterações com boost ativo e cálculos trigonométricos de empuxo
        for (var i = 0; i < 10000; i++)
        {
            estado = _servicoFisica.SimularPasso(estado, controle, 3, 5, 0.02f, 0.02f);
        }

        var memoriaDepois = GC.GetAllocatedBytesForCurrentThread();
        var bytesAlocados = memoriaDepois - memoriaAntes;

        // Assert (SC-002: GC Alloc = 0 bytes no loop contínuo de simulação física)
        Assert.Equal(0, bytesAlocados);
    }
}



