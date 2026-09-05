namespace AeroAscent.Core.Dominio.Contratos;

using AeroAscent.Core.Dominio.ObjetosDeValor;

/// <summary>
/// Contrato de serviço para os cálculos das forças aerodinâmicas, propulsão e cinemáticas do voo.
/// </summary>
public interface IServicoFisicaVoo
{
    /// <summary>
    /// Calcula o vetor de velocidade e impulso inicial gerado pelo lançamento da catapulta.
    /// </summary>
    /// <param name="nivelCatapulta">Nível atual da catapulta (1 a 10).</param>
    /// <param name="forcaDisparoNormalizada">Fator de precisão do toque na barra de força (0.0 a 1.0).</param>
    /// <returns>Vetor tridimensional com a velocidade inicial.</returns>
    VetorVoo CalcularImpulsoInicial(int nivelCatapulta, float forcaDisparoNormalizada);

    /// <summary>
    /// Atualiza a velocidade e sustentação do avião a cada passo de tempo com base na inclinação e aerodinâmica.
    /// </summary>
    /// <param name="velocidadeAtual">Vetor de velocidade atual da aeronave.</param>
    /// <param name="inclinacaoGraus">Ângulo de inclinação do bico (pitch) em graus.</param>
    /// <param name="nivelAerodinamica">Nível de aerodinâmica da aeronave (1 a 10).</param>
    /// <param name="deltaTempoSegundos">Intervalo de tempo transcorrido.</param>
    /// <returns>Novo vetor de velocidade resultante.</returns>
    VetorVoo CalcularProximoPasso(VetorVoo velocidadeAtual, float inclinacaoGraus, int nivelAerodinamica, float deltaTempoSegundos);

    /// <summary>
    /// Simula um passo cinemático completo da aeronave integrando sustentação, arrasto, gravidade,
    /// controle de arfagem/pitch e dinâmica de solo (sem propulsão ativa).
    /// </summary>
    /// <param name="estadoAtual">Estado físico anterior da aeronave.</param>
    /// <param name="controle">Comandos de controle do piloto.</param>
    /// <param name="nivelAerodinamica">Nível da melhoria de aerodinâmica (1 a 10).</param>
    /// <param name="deltaTempoSegundos">Intervalo de tempo transcorrido (dt).</param>
    /// <returns>Novo EstadoFisicoAeronave atualizado na stack com zero alocação no heap.</returns>
    EstadoFisicoAeronave SimularPasso(
        EstadoFisicoAeronave estadoAtual,
        ParametrosControlePiloto controle,
        int nivelAerodinamica,
        float deltaTempoSegundos);

    /// <summary>
    /// Simula um passo cinemático completo da aeronave integrando sustentação, arrasto, gravidade,
    /// controle de arfagem/pitch, dinâmica de solo e propulsão vetorial de boost por queima de combustível.
    /// </summary>
    /// <param name="estadoAtual">Estado físico anterior da aeronave.</param>
    /// <param name="controle">Comandos de controle do piloto (pitch e boost).</param>
    /// <param name="nivelAerodinamica">Nível da melhoria de aerodinâmica (1 a 10).</param>
    /// <param name="nivelMotor">Nível da melhoria de motor para cálculo de empuxo (1 a 10).</param>
    /// <param name="tempoEfetivoQueimaSegundos">Duração efetiva com queima autorizada no passo (fração dt).</param>
    /// <param name="deltaTempoSegundos">Intervalo de tempo transcorrido total (dt).</param>
    /// <returns>Novo EstadoFisicoAeronave atualizado na stack com telemetria do propulsor.</returns>
    EstadoFisicoAeronave SimularPasso(
        EstadoFisicoAeronave estadoAtual,
        ParametrosControlePiloto controle,
        int nivelAerodinamica,
        int nivelMotor,
        float tempoEfetivoQueimaSegundos,
        float deltaTempoSegundos);

    /// <summary>
    /// Aplica empuxo frontal gerado pelo consumo de combustível do propulsor (boost).
    /// </summary>
    /// <param name="velocidadeAtual">Vetor de velocidade antes da propulsão.</param>
    /// <param name="nivelMotor">Nível do motor da aeronave (1 a 10).</param>
    /// <param name="deltaTempoSegundos">Intervalo de tempo de acionamento.</param>
    /// <returns>Vetor de velocidade incrementado pelo empuxo do motor.</returns>
    VetorVoo AplicarPropulsaoMotor(VetorVoo velocidadeAtual, int nivelMotor, float deltaTempoSegundos);

    /// <summary>
    /// Calcula a magnitude escalar de empuxo (T) gerada pelo motor em Newtons com base no nível da melhoria.
    /// </summary>
    /// <param name="nivelMotor">Nível do motor da aeronave (1 a 10).</param>
    /// <returns>Força escalar de empuxo em Newtons (N).</returns>
    float CalcularEmpuxoMotor(int nivelMotor);
}
