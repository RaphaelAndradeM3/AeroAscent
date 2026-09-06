namespace AeroAscent.Core.Aplicacao.Apresentadores;

using System;
using AeroAscent.Core.Aplicacao.Contratos;
using AeroAscent.Core.Aplicacao.DTOs;
using AeroAscent.Core.Dominio.Excecoes;
using AeroAscent.Core.Dominio.ObjetosDeValor;

/// <summary>
/// Apresentador da tela de resumo de término de voo seguindo o padrão Model-View-Presenter (MVP).
/// Orquestra a exibição das métricas consolidadas, a animação progressiva de moedas,
/// o suporte a pulo instantâneo (*skip to end*) e o despacho desacoplado de eventos de navegação pós-voo.
/// </summary>
public class ApresentadorResumoVoo : IApresentadorResumoVoo
{
    private readonly IVisaoResumoVoo _visao;

    /// <inheritdoc />
    public event Action? AoSolicitarIrParaOficina;

    /// <inheritdoc />
    public event Action? AoSolicitarVoarNovamente;

    /// <inheritdoc />
    public bool AnimacaoEmAndamento { get; private set; }

    /// <summary>
    /// Inicializa uma nova instância do apresentador de resumo vinculado à visão passiva informada.
    /// </summary>
    /// <param name="visao">Interface da visão passiva implementada na camada gráfica.</param>
    /// <exception cref="DominioInvalidoException">Lançada caso a visão seja nula.</exception>
    public ApresentadorResumoVoo(IVisaoResumoVoo visao)
    {
        _visao = visao ?? throw new DominioInvalidoException(nameof(visao), "A visão de resumo de voo não pode ser nula.");

        _visao.AoClicarPularAnimacao += PularAnimacao;
        _visao.AoConcluirAnimacaoMoedas += ConcluirAnimacao;
        _visao.AoClicarOficina += TratarCliqueOficina;
        _visao.AoClicarVoarNovamente += TratarCliqueVoarNovamente;
    }

    /// <inheritdoc />
    public void Exibir(in ResumoFinalizacaoVoo resumo)
    {
        AnimacaoEmAndamento = true;

        var modelo = ModeloVisualResumoVoo.Criar(in resumo);

        _visao.ExibirResumo(in modelo);
        _visao.HabilitarBotoesNavegacao(false);
    }

    /// <inheritdoc />
    public void PularAnimacao()
    {
        if (AnimacaoEmAndamento)
        {
            _visao.ConcluirAnimacaoMoedas();
            ConcluirAnimacao();
        }
    }

    /// <inheritdoc />
    public void ConcluirAnimacao()
    {
        AnimacaoEmAndamento = false;
        _visao.HabilitarBotoesNavegacao(true);
    }

    /// <inheritdoc />
    public void Ocultar()
    {
        _visao.Ocultar();
    }

    private void TratarCliqueOficina()
    {
        if (AnimacaoEmAndamento)
        {
            PularAnimacao();
            return;
        }

        _visao.Ocultar();
        AoSolicitarIrParaOficina?.Invoke();
    }

    private void TratarCliqueVoarNovamente()
    {
        if (AnimacaoEmAndamento)
        {
            PularAnimacao();
            return;
        }

        _visao.Ocultar();
        AoSolicitarVoarNovamente?.Invoke();
    }
}
