namespace AeroAscent.Core.Aplicacao.Contratos;

using System;
using AeroAscent.Core.Aplicacao.DTOs;

/// <summary>
/// Contrato de visão passiva para o HUD de telemetria e controles táteis durante o voo.
/// Implementado no Unity Canvas (MonoBehaviour) e comandado exclusivamente pelo apresentador de voo.
/// </summary>
public interface IVisaoHUDVoo
{
    /// <summary>
    /// Evento disparado quando o jogador pressiona o botão/comando de inclinação para cima.
    /// </summary>
    event Action? AoSolicitarSubida;

    /// <summary>
    /// Evento disparado quando o jogador libera o botão/comando de inclinação para cima.
    /// </summary>
    event Action? AoInterromperSubida;

    /// <summary>
    /// Evento disparado quando o jogador pressiona o botão/comando de inclinação para baixo.
    /// </summary>
    event Action? AoSolicitarDescida;

    /// <summary>
    /// Evento disparado quando o jogador libera o botão/comando de inclinação para baixo.
    /// </summary>
    event Action? AoInterromperDescida;

    /// <summary>
    /// Evento disparado quando o jogador pressiona o botão/comando de propulsão (Boost).
    /// </summary>
    event Action? AoSolicitarBoost;

    /// <summary>
    /// Evento disparado quando o jogador libera o botão/comando de propulsão (Boost).
    /// </summary>
    event Action? AoInterromperBoost;

    /// <summary>
    /// Evento disparado quando o jogador clica no botão de pausa da partida.
    /// </summary>
    event Action? AoSolicitarPausa;

    /// <summary>
    /// Atualiza os medidores numéricos e barras visuais do HUD com a telemetria instantânea.
    /// </summary>
    /// <param name="telemetria">Estrutura de dados imutável na stack.</param>
    void AtualizarTelemetria(in TelemetriaHUDDTO telemetria);

    /// <summary>
    /// Altera o estado visual e interativo do botão de Boost (esmaecido/desabilitado se sem combustível).
    /// </summary>
    /// <param name="disponivel"><c>true</c> se há combustível e a aeronave está em voo; caso contrário, <c>false</c>.</param>
    void DefinirInteratividadeBoost(bool disponivel);

    /// <summary>
    /// Notifica a visão de que o recorde histórico foi superado nesta rodada, acionando animação de pulso e cor dourada.
    /// </summary>
    void NotificarNovoRecorde();

    /// <summary>
    /// Define a visibilidade dos controles táteis de pilotagem (ocultados ao pousar/colidir).
    /// </summary>
    /// <param name="visivel"><c>true</c> para manter os botões na tela; <c>false</c> para ocultá-los.</param>
    void DefinirVisibilidadeControles(bool visivel);
}
