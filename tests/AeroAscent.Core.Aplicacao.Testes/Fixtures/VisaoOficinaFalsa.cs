namespace AeroAscent.Core.Aplicacao.Testes.Fixtures;

using System;
using AeroAscent.Core.Aplicacao.Contratos;
using AeroAscent.Core.Aplicacao.DTOs;
using AeroAscent.Core.Dominio.Enums;

/// <summary>
/// Dublê de teste da visão passiva <see cref="IVisaoOficina"/> para validação das interações do apresentador.
/// </summary>
public class VisaoOficinaFalsa : IVisaoOficina
{
    public ModeloVisualOficina? UltimoModeloRecebido { get; private set; }
    public int QuantidadeAtualizacoesTela { get; private set; }
    public bool? UltimoEstadoInteracao { get; private set; }
    public TipoMelhoria? UltimoTipoFeedback { get; private set; }
    public int? UltimoNivelFeedback { get; private set; }
    public string? UltimaMensagemErro { get; private set; }

    public event Action<TipoMelhoria>? AoClicarComprar;
    public event Action? AoClicarDecolar;

    public void AtualizarTela(ModeloVisualOficina modelo)
    {
        UltimoModeloRecebido = modelo;
        QuantidadeAtualizacoesTela++;
    }

    public void DefinirInteracaoHabilitada(bool habilitada)
    {
        UltimoEstadoInteracao = habilitada;
    }

    public void ExibirFeedbackCompra(TipoMelhoria tipo, int novoNivel)
    {
        UltimoTipoFeedback = tipo;
        UltimoNivelFeedback = novoNivel;
    }

    public void ExibirMensagemErro(string mensagem)
    {
        UltimaMensagemErro = mensagem;
    }

    // Métodos utilitários para simulação de cliques do jogador
    public void SimularCliqueComprar(TipoMelhoria tipo) => AoClicarComprar?.Invoke(tipo);
    public void SimularCliqueDecolar() => AoClicarDecolar?.Invoke();
}
