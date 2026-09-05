namespace AeroAscent.Core.Aplicacao.CasosDeUso;

using AeroAscent.Core.Aplicacao.Contratos;
using AeroAscent.Core.Dominio.Contratos;
using AeroAscent.Core.Dominio.Entidades;
using AeroAscent.Core.Dominio.Enums;
using AeroAscent.Core.Dominio.Excecoes;
using AeroAscent.Core.Dominio.ObjetosDeValor;

/// <summary>
/// Caso de uso que orquestra a atualização cíclica contínua da física aerodinâmica de voo,
/// integrando controle de arfagem/pitch, propulsão ativa de boost com queima de combustível,
/// desaceleração por atrito de solo e transição para Pousado com zero alocação no heap.
/// </summary>
public class AtualizarFisicaVooCasoDeUso : IAtualizarFisicaVooCasoDeUso
{
    private readonly IServicoFisicaVoo _servicoFisica;

    /// <summary>
    /// Inicializa uma nova instância do caso de uso de atualização de física de voo.
    /// </summary>
    /// <param name="servicoFisica">Serviço de cálculo cinemático, aerodinâmico e de propulsão.</param>
    /// <exception cref="DominioInvalidoException">Lançada se o serviço de física for nulo.</exception>
    public AtualizarFisicaVooCasoDeUso(IServicoFisicaVoo servicoFisica)
    {
        _servicoFisica = servicoFisica ?? throw new DominioInvalidoException(nameof(servicoFisica), "O serviço de física de voo não pode ser nulo.");
    }

    /// <summary>
    /// Executa a simulação de um passo de tempo na sessão de voo ativa.
    /// </summary>
    /// <param name="voo">Sessão de voo em andamento.</param>
    /// <param name="estadoAtual">Estado físico instantâneo da aeronave.</param>
    /// <param name="controle">Comandos de controle do piloto (pitch e boost).</param>
    /// <param name="deltaTempoSegundos">Intervalo de tempo transcorrido em segundos.</param>
    /// <returns>Novo estado físico calculado da aeronave na stack.</returns>
    /// <exception cref="DominioInvalidoException">Lançada caso a sessão de voo informada seja nula.</exception>
    public EstadoFisicoAeronave Executar(
        Voo voo,
        EstadoFisicoAeronave estadoAtual,
        ParametrosControlePiloto controle,
        float deltaTempoSegundos)
    {
        if (voo == null)
        {
            throw new DominioInvalidoException(nameof(voo), "A sessão de voo não pode ser nula.");
        }

        // Se o voo não está ativo em voo, não atualiza métricas ou cinemática
        if (voo.Status != StatusVoo.EmVoo)
        {
            return estadoAtual;
        }

        var nivelAerodinamica = voo.Aeronave.NivelAerodinamica;
        var nivelMotor = voo.Aeronave.NivelMotor;

        // Propulsão e queima de combustível são bloqueadas estritamente se a aeronave estiver no solo
        float tempoEfetivoQueima = 0f;
        if (controle.AcionarBoost && !estadoAtual.NoSolo && !voo.Combustivel.EstaVazio)
        {
            voo.ConsumirCombustivel(deltaTempoSegundos, out tempoEfetivoQueima);
        }

        var novoEstado = _servicoFisica.SimularPasso(
            estadoAtual,
            controle,
            nivelAerodinamica,
            nivelMotor,
            tempoEfetivoQueima,
            deltaTempoSegundos);

        // Atualiza a telemetria do propulsor com o estado atualizado do reservatório de combustível
        var estaAtivoPropulsor = tempoEfetivoQueima > 0f && !novoEstado.NoSolo;
        var empuxoInstantaneo = estaAtivoPropulsor ? _servicoFisica.CalcularEmpuxoMotor(nivelMotor) : 0f;

        var propulsorFinal = estaAtivoPropulsor
            ? EstadoPropulsor.CriarAtivo(
                empuxoInstantaneo,
                voo.Combustivel.QuantidadeAtual,
                voo.Combustivel.CapacidadeMaxima,
                voo.Combustivel.TaxaQueimaPorSegundo)
            : EstadoPropulsor.CriarInativo(
                voo.Combustivel.QuantidadeAtual,
                voo.Combustivel.CapacidadeMaxima,
                voo.Combustivel.TaxaQueimaPorSegundo);

        novoEstado = novoEstado.ComAtualizacao(
            novoEstado.Posicao,
            novoEstado.Velocidade,
            novoEstado.InclinacaoPitchGraus,
            novoEstado.ForcaResultante,
            novoEstado.NoSolo,
            propulsorFinal);

        // Atualiza as métricas acumuladas de voo (distância percorrida em Z e altitude máxima em Y)
        voo.AtualizarMetricas(novoEstado.Posicao.Z, novoEstado.Posicao.Y, 0);

        // Se a aeronave estiver no solo e o avanço horizontal cessar (parada completa), transita para Pousado
        if (novoEstado.NoSolo && novoEstado.Velocidade.Z <= 0.001f)
        {
            voo.Pousar();
        }

        return novoEstado;
    }
}
