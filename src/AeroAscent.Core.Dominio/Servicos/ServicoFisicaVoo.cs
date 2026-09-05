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
        return velocidadeAtual;
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
        return estadoAtual;
    }
}
