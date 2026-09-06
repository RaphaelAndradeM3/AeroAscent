namespace AeroAscent.Core.Aplicacao.Contratos;

using System;
using AeroAscent.Core.Dominio.Entidades;
using AeroAscent.Core.Dominio.ObjetosDeValor;

/// <summary>
/// Contrato do apresentador que orquestra a lógica de telemetria, controles táteis e estado do HUD de voo.
/// </summary>
public interface IApresentadorHUDVoo
{
    /// <summary>
    /// Evento emitido para o gerenciador de fluxo de jogo quando o jogador clica no botão de pausa.
    /// </summary>
    event Action? AoSolicitarPausa;

    /// <summary>
    /// Indica se o jogo está em estado de pausa comandado pelo HUD.
    /// </summary>
    bool EstaPausado { get; }

    /// <summary>
    /// Inicializa o apresentador com o recorde atual do jogador antes da decolagem.
    /// </summary>
    /// <param name="recordeInicial">Maior distância em metros alcançada até então.</param>
    void Inicializar(float recordeInicial);

    /// <summary>
    /// Atualiza a telemetria do HUD a partir da sessão de voo e do estado cinemático da aeronave.
    /// </summary>
    /// <param name="voo">Entidade de sessão de voo ativa.</param>
    /// <param name="estadoFisico">Estado físico instantâneo da aeronave.</param>
    void Atualizar(Voo voo, in EstadoFisicoAeronave estadoFisico);

    /// <summary>
    /// Obtém os parâmetros de controle do piloto consolidados na stack a partir dos comandos de toque/teclado ativos.
    /// </summary>
    /// <returns>Estrutura imutável de comando pronta para consumo pelo caso de uso de física.</returns>
    ParametrosControlePiloto ObterComandosControle();

    /// <summary>
    /// Inicia o comando sustentado de inclinação de nariz para cima (subida).
    /// </summary>
    void IniciarSubida();

    /// <summary>
    /// Interrompe o comando sustentado de inclinação para cima.
    /// </summary>
    void PararSubida();

    /// <summary>
    /// Inicia o comando sustentado de inclinação de nariz para baixo (descida).
    /// </summary>
    void IniciarDescida();

    /// <summary>
    /// Interrompe o comando sustentado de inclinação para baixo.
    /// </summary>
    void PararDescida();

    /// <summary>
    /// Inicia o comando sustentado de acionamento do propulsor (Boost).
    /// </summary>
    void IniciarBoost();

    /// <summary>
    /// Interrompe o comando sustentado de propulsão.
    /// </summary>
    void PararBoost();

    /// <summary>
    /// Solicita a alternância de pausa na simulação de jogo, liberando comandos ativos.
    /// </summary>
    void SolicitarPausa();
}
