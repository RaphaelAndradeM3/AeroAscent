namespace AeroAscent.Core.Aplicacao.Testes.Fixtures;

using System;
using AeroAscent.Core.Aplicacao.Contratos;
using AeroAscent.Core.Aplicacao.DTOs;

/// <summary>
/// Implementação falsa (Spy/Mock) da visão passiva do resumo de voo para testes unitários em xUnit.
/// </summary>
public class VisaoResumoVooFalsa : IVisaoResumoVoo
{
    /// <inheritdoc />
    public event Action? AoClicarOficina;

    /// <inheritdoc />
    public event Action? AoClicarVoarNovamente;

    /// <inheritdoc />
    public event Action? AoClicarPularAnimacao;

    /// <inheritdoc />
    public event Action? AoConcluirAnimacaoMoedas;

    /// <summary>
    /// Último modelo visual recebido pela visão.
    /// </summary>
    public ModeloVisualResumoVoo UltimoModelo { get; private set; }

    /// <summary>
    /// Quantidade de vezes que ExibirResumo foi invocado.
    /// </summary>
    public int ContadorExibicoes { get; private set; }

    /// <summary>
    /// Quantidade de vezes que ConcluirAnimacaoMoedas foi invocado.
    /// </summary>
    public int ContadorConclusoesAnimacao { get; private set; }

    /// <summary>
    /// Estado atual de interatividade dos botões de navegação.
    /// </summary>
    public bool BotoesNavegacaoHabilitados { get; private set; }

    /// <summary>
    /// Quantidade de vezes que HabilitarBotoesNavegacao foi invocado.
    /// </summary>
    public int ContadorDefinicoesBotoes { get; private set; }

    /// <summary>
    /// Indica se a tela de resumo foi ocultada.
    /// </summary>
    public bool TelaOcultada { get; private set; }

    /// <summary>
    /// Quantidade de vezes que Ocultar foi invocado.
    /// </summary>
    public int ContadorOcultacoes { get; private set; }

    /// <inheritdoc />
    public void ExibirResumo(in ModeloVisualResumoVoo modelo)
    {
        UltimoModelo = modelo;
        ContadorExibicoes++;
        TelaOcultada = false;
    }

    /// <inheritdoc />
    public void ConcluirAnimacaoMoedas()
    {
        ContadorConclusoesAnimacao++;
    }

    /// <inheritdoc />
    public void HabilitarBotoesNavegacao(bool habilitado)
    {
        BotoesNavegacaoHabilitados = habilitado;
        ContadorDefinicoesBotoes++;
    }

    /// <inheritdoc />
    public void Ocultar()
    {
        TelaOcultada = true;
        ContadorOcultacoes++;
    }

    // Métodos utilitários para simulação de eventos acionados pelo jogador ou pela visão:

    /// <summary>
    /// Simula o clique do jogador no botão "Ir para Oficina".
    /// </summary>
    public void SimularCliqueOficina() => AoClicarOficina?.Invoke();

    /// <summary>
    /// Simula o clique do jogador no botão "Voar Novamente".
    /// </summary>
    public void SimularCliqueVoarNovamente() => AoClicarVoarNovamente?.Invoke();

    /// <summary>
    /// Simula o toque do jogador na tela para pular a contagem animada.
    /// </summary>
    public void SimularCliquePularAnimacao() => AoClicarPularAnimacao?.Invoke();

    /// <summary>
    /// Simula o término natural do tempo de animação (1,5 segundos).
    /// </summary>
    public void SimularConclusaoAnimacaoMoedas() => AoConcluirAnimacaoMoedas?.Invoke();
}
