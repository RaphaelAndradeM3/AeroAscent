namespace AeroAscent.Core.Aplicacao.Contratos;

using AeroAscent.Core.Dominio.Entidades;
using AeroAscent.Core.Dominio.ObjetosDeValor;

/// <summary>
/// Contrato do caso de uso responsável pela atualização cíclica contínua da física de voo da aeronave,
/// integrando forças aerodinâmicas, arfagem/pitch, desaceleração por atrito no solo e atualização das métricas do voo.
/// </summary>
public interface IAtualizarFisicaVooCasoDeUso
{
    /// <summary>
    /// Executa a simulação física de um passo de tempo (dt), calculando o novo estado cinemático,
    /// atualizando a entidade Voo ativa e gerenciando a transição para Pousado ao cessar o movimento no solo.
    /// </summary>
    /// <param name="voo">Entidade da sessão de voo em andamento.</param>
    /// <param name="estadoAtual">Estado físico anterior da aeronave (posição, velocidade, pitch, etc.).</param>
    /// <param name="controle">Comandos de controle do piloto (input de arfagem).</param>
    /// <param name="deltaTempoSegundos">Intervalo de tempo transcorrido (dt em segundos, ex: 0.016s ou 0.02s).</param>
    /// <returns>Novo estado físico resultante da aeronave, alocado exclusivamente na stack (GC Alloc = 0 bytes).</returns>
    EstadoFisicoAeronave Executar(
        Voo voo,
        EstadoFisicoAeronave estadoAtual,
        ParametrosControlePiloto controle,
        float deltaTempoSegundos);
}
