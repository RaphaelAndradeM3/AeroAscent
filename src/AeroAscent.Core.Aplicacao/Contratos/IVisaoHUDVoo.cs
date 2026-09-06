namespace AeroAscent.Core.Aplicacao.Contratos;

using System;
using AeroAscent.Core.Aplicacao.DTOs;

/// <summary>
/// Contrato de visão passiva para a interface do HUD de voo e controles de toque.
/// Implementado na camada de apresentação (Unity Canvas / MonoBehaviour) e acionado pelo apresentador de voo.
/// </summary>
public interface IVisaoHUDVoo
{
    /// <summary>
    /// Evento disparado quando o jogador inicia a sustentação do comando de subida (pitch para cima).
    /// </summary>
    event Action? AoSolicitarSubida;

    /// <summary>
    /// Evento disparado quando o jogador libera o comando de subida.
    /// </summary>
    event Action? AoInterromperSubida;

    /// <summary>
    /// Evento disparado quando o jogador inicia a sustentação do comando de descida (pitch para baixo).
    /// </summary>
    event Action? AoSolicitarDescida;

    /// <summary>
    /// Evento disparado quando o jogador libera o comando de descida.
    /// </summary>
    event Action? AoInterromperDescida;

    /// <summary>
    /// Evento disparado quando o jogador inicia a sustentação do acionamento de propulsão (boost).
    /// </summary>
    event Action? AoSolicitarBoost;

    /// <summary>
    /// Evento disparado quando o jogador libera o acionamento de propulsão.
    /// </summary>
    event Action? AoInterromperBoost;

    /// <summary>
    /// Evento disparado quando o jogador clica no botão de pausa na interface.
    /// </summary>
    event Action? AoSolicitarPausa;

    /// <summary>
    /// Atualiza os marcadores e textos de telemetria do HUD a partir dos dados passados na stack.
    /// </summary>
    /// <param name="telemetria">Métricas instantâneas de voo.</param>
    void AtualizarTelemetria(in TelemetriaHUDDTO telemetria);

    /// <summary>
    /// Define o estado de interatividade e opacidade do botão de Boost (esmaecido quando desabilitado).
    /// </summary>
    /// <param name="disponivel"><c>true</c> se há combustível e a aeronave está em voo ativo; caso contrário, <c>false</c>.</param>
    void DefinirInteratividadeBoost(bool disponivel);

    /// <summary>
    /// Emite feedback visual comemorativo (pulso de escala e cor dourada) indicando a quebra de recorde pessoal.
    /// </summary>
    void NotificarNovoRecorde();

    /// <summary>
    /// Define a visibilidade dos controles táteis de pilotagem (subir, descer e boost).
    /// </summary>
    /// <param name="visivel"><c>true</c> para exibir os botões na tela; <c>false</c> para ocultá-los ao finalizar o voo.</param>
    void DefinirVisibilidadeControles(bool visivel);
}
