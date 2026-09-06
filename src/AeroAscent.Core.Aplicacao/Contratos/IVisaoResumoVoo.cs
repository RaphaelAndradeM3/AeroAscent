namespace AeroAscent.Core.Aplicacao.Contratos;

using System;
using AeroAscent.Core.Aplicacao.DTOs;

/// <summary>
/// Contrato de visão passiva para a tela de resumo de voo e celebração de recorde.
/// Implementado na camada de apresentação (Unity Canvas / MonoBehaviour) e acionado pelo apresentador de resumo.
/// </summary>
public interface IVisaoResumoVoo
{
    /// <summary>
    /// Evento disparado quando o jogador clica no botão para navegar até a Oficina.
    /// </summary>
    event Action? AoClicarOficina;

    /// <summary>
    /// Evento disparado quando o jogador clica no botão para iniciar um novo voo diretamente.
    /// </summary>
    event Action? AoClicarVoarNovamente;

    /// <summary>
    /// Evento disparado quando o jogador toca na tela ou clica para pular a animação de contagem de moedas.
    /// </summary>
    event Action? AoClicarPularAnimacao;

    /// <summary>
    /// Evento disparado pela visão quando a animação de contagem atinge seu término natural (1,5 segundos).
    /// </summary>
    event Action? AoConcluirAnimacaoMoedas;

    /// <summary>
    /// Apresenta a tela de resumo populada com os dados formatados do modelo visual e inicia a contagem progressiva de moedas.
    /// Caso <paramref name="modelo"/>.EhNovoRecorde seja verdadeiro, exibe o banner comemorativo e emite confetes coloridos.
    /// </summary>
    /// <param name="modelo">Estrutura de dados imutável alocada na stack com a telemetria e o extrato financeiro.</param>
    void ExibirResumo(in ModeloVisualResumoVoo modelo);

    /// <summary>
    /// Interrompe imediatamente qualquer animação numérica de moedas em curso, fixando os textos nos valores finais totais.
    /// </summary>
    void ConcluirAnimacaoMoedas();

    /// <summary>
    /// Habilita ou desabilita a interatividade visual dos botões de navegação ("Oficina" e "Voar Novamente").
    /// </summary>
    /// <param name="habilitado"><c>true</c> se os botões podem ser clicados; <c>false</c> se devem ficar inativos/esmaecidos.</param>
    void HabilitarBotoesNavegacao(bool habilitado);

    /// <summary>
    /// Oculta a tela de resumo de voo e desativa seus elementos visuais.
    /// </summary>
    void Ocultar();
}
