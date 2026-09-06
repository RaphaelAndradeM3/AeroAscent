namespace AeroAscent.Core.Aplicacao.Contratos;

using System;
using AeroAscent.Core.Dominio.Entidades;
using AeroAscent.Core.Dominio.ObjetosDeValor;

/// <summary>
/// Contrato do apresentador que comanda a lógica do HUD de voo, interpretação de comandos e telemetria.
/// </summary>
public interface IApresentadorHUDVoo
{
    /// <summary>
    /// Evento emitido quando a pausa do jogo é solicitada via interface do HUD.
    /// </summary>
    event Action? AoSolicitarPausa;

    /// <summary>
    /// Indica se a partida encontra-se atualmente em estado de pausa.
    /// </summary>
    bool EstaPausado { get; }

    /// <summary>
    /// Inicializa o apresentador com o recorde de distância histórico a ser superado.
    /// </summary>
    /// <param name="recordeInicial">Melhor marca histórica em metros.</param>
    void Inicializar(float recordeInicial);

    /// <summary>
    /// Atualiza as métricas de telemetria a partir da entidade de voo e do estado cinemático atual.
    /// </summary>
    /// <param name="voo">Entidade de sessão de voo em andamento.</param>
    /// <param name="estadoFisico">Estado físico instantâneo da aeronave.</param>
    void Atualizar(Voo voo, in EstadoFisicoAeronave estadoFisico);

    /// <summary>
    /// Obtém os parâmetros de pilotagem sintetizados na stack a partir dos comandos de toque/teclado ativos.
    /// </summary>
    /// <returns>Estrutura imutável de comandos pronta para a física.</returns>
    ParametrosControlePiloto ObterComandosControle();

    /// <summary>
    /// Inicia a sustentação do comando de arfagem para cima.
    /// </summary>
    void IniciarSubida();

    /// <summary>
    /// Interrompe o comando de subida.
    /// </summary>
    void PararSubida();

    /// <summary>
    /// Inicia a sustentação do comando de arfagem para baixo.
    /// </summary>
    void IniciarDescida();

    /// <summary>
    /// Interrompe o comando de descida.
    /// </summary>
    void PararDescida();

    /// <summary>
    /// Inicia a sustentação do comando de propulsão (boost).
    /// </summary>
    void IniciarBoost();

    /// <summary>
    /// Interrompe o comando de propulsão.
    /// </summary>
    void PararBoost();

    /// <summary>
    /// Alterna o estado de pausa e cancela comandos sustentados ativos.
    /// </summary>
    void SolicitarPausa();
}
