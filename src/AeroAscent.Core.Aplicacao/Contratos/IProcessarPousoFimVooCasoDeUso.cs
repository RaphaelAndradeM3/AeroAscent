namespace AeroAscent.Core.Aplicacao.Contratos;

using AeroAscent.Core.Dominio.Entidades;
using AeroAscent.Core.Dominio.ObjetosDeValor;

/// <summary>
/// Contrato do caso de uso de aplicação responsável por processar a detecção de pouso,
/// comandar o encerramento do voo na entidade de domínio e disparar as notificações de conclusão.
/// </summary>
public interface IProcessarPousoFimVooCasoDeUso
{
    /// <summary>
    /// Avalia o estado físico da aeronave e a sessão de voo, realizando a transição para Pousado quando a aeronave atinge repouso no solo.
    /// </summary>
    /// <param name="voo">Entidade da sessão de voo em andamento.</param>
    /// <param name="estadoAtual">Estado físico cinemático instantâneo da aeronave.</param>
    /// <returns>ResultadoFimVoo imutável na stack contendo o status de encerramento e métricas finais.</returns>
    ResultadoFimVoo Executar(Voo voo, EstadoFisicoAeronave estadoAtual);
}
