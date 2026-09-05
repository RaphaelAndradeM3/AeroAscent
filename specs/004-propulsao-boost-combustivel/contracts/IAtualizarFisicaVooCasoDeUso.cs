namespace AeroAscent.Core.Aplicacao.Contratos;

using AeroAscent.Core.Dominio.Entidades;
using AeroAscent.Core.Dominio.ObjetosDeValor;

/// <summary>
/// Contrato do caso de uso responsável pela atualização cíclica contínua da física de voo da aeronave,
/// integrando forças aerodinâmicas, arfagem/pitch, propulsão ativa de boost com queima fracionária de combustível,
/// desaceleração por atrito no solo e consolidação das métricas da sessão de voo.
/// </summary>
public interface IAtualizarFisicaVooCasoDeUso
{
    /// <summary>
    /// Executa a simulação física de um passo de tempo (dt), calculando o novo estado cinemático e de propulsão,
    /// atualizando o tanque de combustível da entidade Voo e gerenciando a transição para Pousado ao parar no solo.
    /// </summary>
    /// <param name="voo">Entidade da sessão de voo ativa.</param>
    /// <param name="estadoAtual">Estado físico instantâneo anterior da aeronave.</param>
    /// <param name="controle">Comandos de controle do piloto (pitch e boost).</param>
    /// <param name="deltaTempoSegundos">Intervalo de tempo transcorrido (dt em segundos).</param>
    /// <returns>Novo estado físico resultante contendo a telemetria do propulsor (GC Alloc = 0 bytes).</returns>
    EstadoFisicoAeronave Executar(
        Voo voo,
        EstadoFisicoAeronave estadoAtual,
        ParametrosControlePiloto controle,
        float deltaTempoSegundos);
}
