namespace AeroAscent.Core.Aplicacao.Contratos;

using System.Collections.Generic;
using AeroAscent.Core.Dominio.Comum;
using AeroAscent.Core.Dominio.Entidades;
using AeroAscent.Core.Dominio.ObjetosDeValor;

/// <summary>
/// Contrato do caso de uso de aplicação responsável por processar a detecção de colisões da aeronave com coletáveis,
/// creditar moedas na entidade Voo, aplicar impulsos cinemáticos e reciclar objetos no pool.
/// </summary>
public interface IProcessarColetaveisVooCasoDeUso
{
    /// <summary>
    /// Processa um ciclo de detecção e interação da aeronave com os coletáveis ativos na janela de voo.
    /// </summary>
    /// <param name="voo">Entidade de sessão de voo em andamento.</param>
    /// <param name="estadoAtual">Estado físico instantâneo da aeronave.</param>
    /// <param name="coletaveisAtivos">Coleção de coletáveis atualmente ativos na janela espacial.</param>
    /// <param name="poolMoedas">Pool de reciclagem de moedas.</param>
    /// <param name="poolAneis">Pool de reciclagem de anéis de vento.</param>
    /// <returns>Resultado com contagem de moedas coletadas, novo estado físico atualizado e impulso aplicado.</returns>
    ResultadoProcessamentoColetaveis Executar(
        Voo voo,
        EstadoFisicoAeronave estadoAtual,
        IList<Coletavel> coletaveisAtivos,
        IPoolObjetos<Coletavel> poolMoedas,
        IPoolObjetos<Coletavel> poolAneis);
}
