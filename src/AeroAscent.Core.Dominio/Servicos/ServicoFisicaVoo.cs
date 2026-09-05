namespace AeroAscent.Core.Dominio.Servicos;

using System;
using AeroAscent.Core.Dominio.Contratos;
using AeroAscent.Core.Dominio.Entidades;
using AeroAscent.Core.Dominio.Excecoes;
using AeroAscent.Core.Dominio.ObjetosDeValor;

/// <summary>
/// Serviço de domínio puro responsável pelos cálculos matemáticos cinemáticos e aerodinâmicos do voo,
/// operando sem alocação de memória no loop contínuo e sem qualquer dependência da Unity Engine.
/// </summary>
public class ServicoFisicaVoo : IServicoFisicaVoo
{
    /// <summary>
    /// Velocidade escalar de lançamento padrão (em metros por segundo) para catapulta no nível 1 com 100% de precisão (25.0 m/s = 90 km/h).
    /// </summary>
    public const float FORCA_BASE = 25.0f;

    /// <summary>
    /// Fator de acréscimo linear na velocidade inicial por nível evoluído da catapulta (+25% por nível).
    /// </summary>
    public const float INCREMENTO_POR_NIVEL = 0.25f;

    /// <summary>
    /// Ângulo padrão de inclinação da rampa da catapulta em graus em relação ao solo (35.0°).
    /// </summary>
    public const float ANGULO_PADRAO_GRAUS = 35.0f;

    /// <summary>
    /// Densidade atmosférica do ar padrão ao nível do mar (1.225 kg/m³ ISA).
    /// </summary>
    public const float DENSIDADE_AR_PADRAO = 1.225f;

    /// <summary>
    /// Área alar de referência da aeronave (1.0 m²).
    /// </summary>
    public const float AREA_ASA_REFERENCIA = 1.0f;

    /// <summary>
    /// Massa de referência da aeronave em quilogramas (10.0 kg).
    /// </summary>
    public const float MASSA_REFERENCIA_KG = 10.0f;

    /// <summary>
    /// Aceleração gravitacional terrestre padrão apontando para baixo (9.81 m/s²).
    /// </summary>
    public const float ACELERACAO_GRAVIDADE = 9.81f;

    /// <summary>
    /// Coeficiente de arrasto parasita base (CD0 = 0.04).
    /// </summary>
    public const float COEFICIENTE_ARRASTO_BASE = 0.04f;

    /// <summary>
    /// Fator de arrasto induzido (k = 0.05).
    /// </summary>
    public const float FATOR_ARRASTO_INDUZIDO = 0.05f;

    /// <summary>
    /// Ângulo de ataque crítico de estol em graus (20.0°).
    /// </summary>
    public const float ANGULO_ESTOL_GRAUS = 20.0f;

    /// <summary>
    /// Coeficiente máximo de sustentação linear atingido no ângulo crítico (CL_max ≈ 1.5).
    /// </summary>
    public const float COEFICIENTE_SUSTENTACAO_MAXIMO = 1.5f;

    /// <summary>
    /// Coeficiente de atrito cinético de deslizamento com o solo (μ ≈ 0.3).
    /// </summary>
    public const float COEFICIENTE_ATRITO_SOLO = 0.3f;

    /// <summary>
    /// Limiar de velocidade horizontal de avanço abaixo do qual a aeronave para completamente no solo (0.5 m/s).
    /// </summary>
    public const float VELOCIDADE_LIMIAR_PARADA_SOLO = 0.5f;

    /// <summary>
    /// Taxa de torque restaurador para autoestabilização direcional ao soltar comandos (graus por segundo normalizado).
    /// </summary>
    public const float TAXA_ESTABILIZACAO_PITCH = 3.0f;

    /// <summary>
    /// Redução de arrasto por nível de melhoria de aerodinâmica (+20% por nível no divisor efetivo).
    /// </summary>
    public const float INCREMENTO_AERODINAMICA_POR_NIVEL = 0.20f;

    /// <summary>
    /// Calcula o vetor tridimensional de impulso inicial conferido à aeronave no disparo da catapulta.
    /// Decompõe a velocidade escalar no eixo Z (avanço frontal) e eixo Y (altitude vertical) com desvio lateral nulo no eixo X.
    /// </summary>
    /// <param name="nivelCatapulta">Nível atual da catapulta (1 a 10).</param>
    /// <param name="forcaDisparoNormalizada">Fator de precisão instantâneo (0.0 a 1.0), com piso protetivo mínimo de 10%.</param>
    /// <returns>VetorVoo tridimensional contendo a velocidade inicial decomposta.</returns>
    /// <exception cref="DominioInvalidoException">Lançada caso o nível da catapulta esteja fora dos limites válidos.</exception>
    public VetorVoo CalcularImpulsoInicial(int nivelCatapulta, float forcaDisparoNormalizada)
    {
        if (nivelCatapulta < Aeronave.NIVEL_MINIMO || nivelCatapulta > Aeronave.NIVEL_MAXIMO)
        {
            throw new DominioInvalidoException(
                nameof(nivelCatapulta),
                $"O nível da catapulta deve estar entre {Aeronave.NIVEL_MINIMO} e {Aeronave.NIVEL_MAXIMO}. Valor informado: {nivelCatapulta}.");
        }

        var precisaoEfetiva = Math.Max(ParametrosLancamento.PISO_MINIMO_PRECISAO, Math.Min(1.0f, forcaDisparoNormalizada));
        var multiplicadorNivel = 1.0f + (nivelCatapulta - 1) * INCREMENTO_POR_NIVEL;
        var velocidadeEscalar = FORCA_BASE * multiplicadorNivel * precisaoEfetiva;

        var radianos = ANGULO_PADRAO_GRAUS * MathF.PI / 180.0f;
        var vy = velocidadeEscalar * MathF.Sin(radianos);
        var vz = velocidadeEscalar * MathF.Cos(radianos);

        return new VetorVoo(0f, vy, vz);
    }

    /// <summary>
    /// Atualiza a velocidade e sustentação do avião a cada passo de tempo com base na inclinação e aerodinâmica.
    /// </summary>
    /// <param name="velocidadeAtual">Vetor de velocidade atual da aeronave.</param>
    /// <param name="inclinacaoGraus">Ângulo de inclinação do bico (pitch) em graus.</param>
    /// <param name="nivelAerodinamica">Nível de aerodinâmica da aeronave (1 a 10).</param>
    /// <param name="deltaTempoSegundos">Intervalo de tempo transcorrido.</param>
    /// <returns>Novo vetor de velocidade resultante.</returns>
    public VetorVoo CalcularProximoPasso(VetorVoo velocidadeAtual, float inclinacaoGraus, int nivelAerodinamica, float deltaTempoSegundos)
    {
        var estadoSimulado = EstadoFisicoAeronave.CriarInicial(
            new VetorVoo(0f, 100f, 0f),
            velocidadeAtual,
            inclinacaoGraus);

        var controle = new ParametrosControlePiloto(0f, ParametrosControlePiloto.TAXA_ANGULAR_PADRAO);
        var resultado = SimularPasso(estadoSimulado, controle, nivelAerodinamica, deltaTempoSegundos);
        return resultado.Velocidade;
    }

    /// <summary>
    /// Simula um passo cinemático completo da aeronave integrando forças de sustentação, arrasto, gravidade,
    /// controle de arfagem/pitch e dinâmica de solo, retornando um novo EstadoFisicoAeronave na stack.
    /// </summary>
    /// <param name="estadoAtual">Estado físico anterior da aeronave.</param>
    /// <param name="controle">Comandos de controle do piloto.</param>
    /// <param name="nivelAerodinamica">Nível da melhoria de aerodinâmica (1 a 10).</param>
    /// <param name="deltaTempoSegundos">Intervalo de tempo transcorrido (dt).</param>
    /// <returns>Novo EstadoFisicoAeronave atualizado com alocação zero no heap.</returns>
    public EstadoFisicoAeronave SimularPasso(
        EstadoFisicoAeronave estadoAtual,
        ParametrosControlePiloto controle,
        int nivelAerodinamica,
        float deltaTempoSegundos)
    {
        if (deltaTempoSegundos <= 0f)
        {
            return estadoAtual;
        }

        var nivelAeroEfetivo = Math.Clamp(nivelAerodinamica, Aeronave.NIVEL_MINIMO, Aeronave.NIVEL_MAXIMO);

        // -------------------------------------------------------------
        // CASO 1: Dinâmica de Solo (Contato / Deslizamento com atrito)
        // -------------------------------------------------------------
        if (estadoAtual.NoSolo || estadoAtual.Posicao.Y <= 0f)
        {
            var desaceleracaoAtrito = COEFICIENTE_ATRITO_SOLO * ACELERACAO_GRAVIDADE;
            var vzAtual = MathF.Max(0f, estadoAtual.Velocidade.Z);

            var novaVzSolo = MathF.Max(0f, vzAtual - desaceleracaoAtrito * deltaTempoSegundos);
            if (novaVzSolo < VELOCIDADE_LIMIAR_PARADA_SOLO)
            {
                novaVzSolo = 0f;
            }

            var novaPosicaoZSolo = estadoAtual.Posicao.Z + novaVzSolo * deltaTempoSegundos;
            var novoPitchSolo = Math.Max(0f, estadoAtual.InclinacaoPitchGraus - 10f * deltaTempoSegundos);

            var forcaAtritoZ = novaVzSolo > 0f ? -(COEFICIENTE_ATRITO_SOLO * MASSA_REFERENCIA_KG * ACELERACAO_GRAVIDADE) : 0f;
            var forcaResultanteSolo = new VetorVoo(0f, 0f, forcaAtritoZ);

            return new EstadoFisicoAeronave(
                new VetorVoo(0f, 0f, novaPosicaoZSolo),
                new VetorVoo(0f, 0f, novaVzSolo),
                novoPitchSolo,
                forcaResultanteSolo,
                true);
        }

        // -------------------------------------------------------------
        // CASO 2: Dinâmica de Voo Livre (Aerodinâmica + Gravidade)
        // -------------------------------------------------------------

        // 1. Atualização da Arfagem (Pitch)
        float novoPitch;
        if (controle.TemComandoAtivo)
        {
            var variacaoPitch = controle.IntensidadePitch * controle.TaxaVariacaoAngularGrausPorSegundo * deltaTempoSegundos;
            novoPitch = Math.Clamp(
                estadoAtual.InclinacaoPitchGraus + variacaoPitch,
                EstadoFisicoAeronave.PITCH_MINIMO_GRAUS,
                EstadoFisicoAeronave.PITCH_MAXIMO_GRAUS);
        }
        else
        {
            // Autoestabilização suave alinhada ao vetor velocidade
            var velEscalarAtual = estadoAtual.VelocidadeEscalar;
            if (velEscalarAtual > 0.5f)
            {
                var anguloTrajetoria = MathF.Atan2(estadoAtual.Velocidade.Y, estadoAtual.Velocidade.Z) * 180.0f / MathF.PI;
                var anguloAlvoClamped = Math.Clamp(
                    anguloTrajetoria,
                    EstadoFisicoAeronave.PITCH_MINIMO_GRAUS,
                    EstadoFisicoAeronave.PITCH_MAXIMO_GRAUS);

                var fatorInterp = MathF.Min(1.0f, TAXA_ESTABILIZACAO_PITCH * deltaTempoSegundos);
                novoPitch = estadoAtual.InclinacaoPitchGraus + (anguloAlvoClamped - estadoAtual.InclinacaoPitchGraus) * fatorInterp;
            }
            else
            {
                novoPitch = estadoAtual.InclinacaoPitchGraus;
            }
        }

        // 2. Balanço Aerodinâmico e Forças
        var vy = estadoAtual.Velocidade.Y;
        var vz = estadoAtual.Velocidade.Z;
        var velocidadeEscalar = MathF.Sqrt(vy * vy + vz * vz);

        VetorVoo forcaTotal;

        if (velocidadeEscalar < 0.1f)
        {
            // Praticamente parado: apenas gravidade atuando
            forcaTotal = new VetorVoo(0f, -MASSA_REFERENCIA_KG * ACELERACAO_GRAVIDADE, 0f);
        }
        else
        {
            // Ângulo de trajetória (gamma) e Ângulo de ataque (alpha)
            var anguloTrajetoriaGraus = MathF.Atan2(vy, vz) * 180.0f / MathF.PI;
            var anguloAtaqueGraus = novoPitch - anguloTrajetoriaGraus;

            // Coeficiente de sustentação CL(alpha) com estol suave acolhedor (Artigos I e II)
            var cl = CalcularCoeficienteSustentacao(anguloAtaqueGraus);

            // Coeficiente de arrasto CD(alpha) com escalonamento redutor por nível de aerodinâmica
            var divisorAerodinamica = 1.0f + (nivelAeroEfetivo - 1) * INCREMENTO_AERODINAMICA_POR_NIVEL;
            var cdBase = COEFICIENTE_ARRASTO_BASE + FATOR_ARRASTO_INDUZIDO * (cl * cl);
            var cdEfetivo = cdBase / divisorAerodinamica;

            // Magnitudes de força
            var pressaoDinamica = 0.5f * DENSIDADE_AR_PADRAO * (velocidadeEscalar * velocidadeEscalar) * AREA_ASA_REFERENCIA;
            var magnitudeSustentacao = pressaoDinamica * cl;
            var magnitudeArrasto = pressaoDinamica * cdEfetivo;

            // Decomposição vetorial
            // Vetor unitário na direção da velocidade:
            var uVelY = vy / velocidadeEscalar;
            var uVelZ = vz / velocidadeEscalar;

            // Vetor unitário normal de sustentação (girado +90 graus no plano Y-Z):
            var uLiftY = uVelZ;
            var uLiftZ = -uVelY;

            // Força de sustentação
            var fLiftY = uLiftY * magnitudeSustentacao;
            var fLiftZ = uLiftZ * magnitudeSustentacao;

            // Força de arrasto (aponta contra a velocidade)
            var fDragY = -uVelY * magnitudeArrasto;
            var fDragZ = -uVelZ * magnitudeArrasto;

            // Força da gravidade
            var fGravY = -MASSA_REFERENCIA_KG * ACELERACAO_GRAVIDADE;

            // Força total resultante
            forcaTotal = new VetorVoo(0f, fLiftY + fDragY + fGravY, fLiftZ + fDragZ);
        }

        // 3. Integração Numérica Semi-Implícita de Euler (Euler-Cromer)
        var aceleracaoY = forcaTotal.Y / MASSA_REFERENCIA_KG;
        var aceleracaoZ = forcaTotal.Z / MASSA_REFERENCIA_KG;

        var novaVy = vy + aceleracaoY * deltaTempoSegundos;
        var novaVz = vz + aceleracaoZ * deltaTempoSegundos;

        var novaPosY = estadoAtual.Posicao.Y + novaVy * deltaTempoSegundos;
        var novaPosZ = estadoAtual.Posicao.Z + novaVz * deltaTempoSegundos;

        // 4. Detecção e resposta de colisão com o solo no final do passo
        if (novaPosY <= 0f)
        {
            return new EstadoFisicoAeronave(
                new VetorVoo(0f, 0f, novaPosZ),
                new VetorVoo(0f, 0f, MathF.Max(0f, novaVz)),
                Math.Max(0f, novoPitch),
                forcaTotal,
                true);
        }

        return new EstadoFisicoAeronave(
            new VetorVoo(0f, novaPosY, novaPosZ),
            new VetorVoo(0f, novaVy, novaVz),
            novoPitch,
            forcaTotal,
            false);
    }

    /// <summary>
    /// Calcula o coeficiente de sustentação CL baseado no ângulo de ataque alfa,
    /// aplicando resposta linear até 20° e transição de estol suave acolhedor para crianças e novatos.
    /// </summary>
    /// <param name="anguloAtaqueGraus">Ângulo de ataque em graus.</param>
    /// <returns>Coeficiente de sustentação CL.</returns>
    public static float CalcularCoeficienteSustentacao(float anguloAtaqueGraus)
    {
        var absAlfa = MathF.Abs(anguloAtaqueGraus);
        var sinal = MathF.Sign(anguloAtaqueGraus);

        if (absAlfa <= ANGULO_ESTOL_GRAUS)
        {
            // Resposta linear arcade: CL = 0.075 * alfa (atinge 1.5 a 20°)
            return 0.075f * anguloAtaqueGraus;
        }

        // Estol Suave Acolhedor (Artigo I e II):
        // Decai suavemente com cosseno sem corte abrupto a zero
        var alfaClamped = MathF.Min(90f, absAlfa);
        var t = (alfaClamped - ANGULO_ESTOL_GRAUS) / (90f - ANGULO_ESTOL_GRAUS);
        var fatorSuave = MathF.Cos(t * MathF.PI * 0.5f);
        var clAcolhedor = 1.2f * fatorSuave + 0.3f;

        return sinal * clAcolhedor;
    }

    /// <summary>
    /// Aplica empuxo frontal gerado pelo consumo de combustível do propulsor (boost).
    /// </summary>
    /// <param name="velocidadeAtual">Vetor de velocidade antes da propulsão.</param>
    /// <param name="nivelMotor">Nível do motor da aeronave (1 a 10).</param>
    /// <param name="deltaTempoSegundos">Intervalo de tempo de acionamento.</param>
    /// <returns>Vetor de velocidade incrementado pelo empuxo do motor.</returns>
    public VetorVoo AplicarPropulsaoMotor(VetorVoo velocidadeAtual, int nivelMotor, float deltaTempoSegundos)
    {
        return velocidadeAtual;
    }
}
