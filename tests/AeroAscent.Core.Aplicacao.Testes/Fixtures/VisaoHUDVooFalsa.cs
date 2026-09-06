namespace AeroAscent.Core.Aplicacao.Testes.Fixtures;

using System;
using AeroAscent.Core.Aplicacao.Contratos;
using AeroAscent.Core.Aplicacao.DTOs;

/// <summary>
/// Implementação falsa (Spy/Mock) da visão passiva do HUD de voo para testes unitários em xUnit.
/// </summary>
public class VisaoHUDVooFalsa : IVisaoHUDVoo
{
    public event Action? AoSolicitarSubida;
    public event Action? AoInterromperSubida;
    public event Action? AoSolicitarDescida;
    public event Action? AoInterromperDescida;
    public event Action? AoSolicitarBoost;
    public event Action? AoInterromperBoost;
    public event Action? AoSolicitarPausa;

    /// <summary>
    /// Última telemetria recebida pela visão.
    /// </summary>
    public TelemetriaHUDDTO UltimaTelemetria { get; private set; }

    /// <summary>
    /// Quantidade de vezes que AtualizarTelemetria foi invocado.
    /// </summary>
    public int ContadorAtualizacoesTelemetria { get; private set; }

    /// <summary>
    /// Estado atual de interatividade do botão de boost.
    /// </summary>
    public bool BoostHabilitado { get; private set; } = true;

    /// <summary>
    /// Quantidade de vezes que DefinirInteratividadeBoost foi invocado.
    /// </summary>
    public int ContadorDefinicoesBoost { get; private set; }

    /// <summary>
    /// Indica se NotificarNovoRecorde foi chamado.
    /// </summary>
    public bool NovoRecordeNotificado { get; private set; }

    /// <summary>
    /// Quantidade de vezes que NotificarNovoRecorde foi invocado.
    /// </summary>
    public int ContadorNotificacoesRecorde { get; private set; }

    /// <summary>
    /// Indica se os controles táteis estão visíveis.
    /// </summary>
    public bool ControlesVisiveis { get; private set; } = true;

    /// <summary>
    /// Quantidade de vezes que DefinirVisibilidadeControles foi invocado.
    /// </summary>
    public int ContadorDefinicoesVisibilidade { get; private set; }

    public void AtualizarTelemetria(in TelemetriaHUDDTO telemetria)
    {
        UltimaTelemetria = telemetria;
        ContadorAtualizacoesTelemetria++;
    }

    public void DefinirInteratividadeBoost(bool disponivel)
    {
        BoostHabilitado = disponivel;
        ContadorDefinicoesBoost++;
    }

    public void NotificarNovoRecorde()
    {
        NovoRecordeNotificado = true;
        ContadorNotificacoesRecorde++;
    }

    public void DefinirVisibilidadeControles(bool visivel)
    {
        ControlesVisiveis = visivel;
        ContadorDefinicoesVisibilidade++;
    }

    // Métodos utilitários para simulação de eventos acionados pelo usuário:

    public void SimularPressionarSubida() => AoSolicitarSubida?.Invoke();
    public void SimularLiberarSubida() => AoInterromperSubida?.Invoke();
    public void SimularPressionarDescida() => AoSolicitarDescida?.Invoke();
    public void SimularLiberarDescida() => AoInterromperDescida?.Invoke();
    public void SimularPressionarBoost() => AoSolicitarBoost?.Invoke();
    public void SimularLiberarBoost() => AoInterromperBoost?.Invoke();
    public void SimularCliquePausa() => AoSolicitarPausa?.Invoke();
}
