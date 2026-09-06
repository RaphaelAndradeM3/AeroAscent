namespace AeroAscent.Core.Aplicacao.Contratos;

using System;
using AeroAscent.Core.Dominio.ObjetosDeValor;

/// <summary>
/// Contrato do apresentador da tela de resumo de voo seguindo o padrão Model-View-Presenter (MVP).
/// Orquestra a formatação dos dados de finalização, o fluxo da animação progressiva de moedas,
/// o cancelamento antecipado (*skip to end*) e o despacho de eventos de navegação desacoplados.
/// </summary>
public interface IApresentadorResumoVoo
{
    /// <summary>
    /// Evento disparado quando o jogador confirma a intenção de ir para a oficina após o resumo.
    /// </summary>
    event Action? AoSolicitarIrParaOficina;

    /// <summary>
    /// Evento disparado quando o jogador confirma a intenção de iniciar um novo voo imediatamente.
    /// </summary>
    event Action? AoSolicitarVoarNovamente;

    /// <summary>
    /// Indica se a animação de contagem de recompensas está em execução ativa no momento.
    /// </summary>
    bool AnimacaoEmAndamento { get; }

    /// <summary>
    /// Prepara o modelo de apresentação a partir do extrato consolidado e comanda a exibição do resumo na visão passiva.
    /// </summary>
    /// <param name="resumo">Extrato de término de voo persistido em disco pelo caso de uso de finalização.</param>
    void Exibir(in ResumoFinalizacaoVoo resumo);

    /// <summary>
    /// Solicita a conclusão instantânea da animação de contagem de moedas (*skip to end*), liberando a navegação.
    /// </summary>
    void PularAnimacao();

    /// <summary>
    /// Notifica que a animação de contagem foi concluída pela visão passiva.
    /// </summary>
    void ConcluirAnimacao();

    /// <summary>
    /// Oculta a tela de resumo de voo.
    /// </summary>
    void Ocultar();
}
