namespace AeroAscent.Apresentacao.MAUI.Views;

using System;
using System.Threading.Tasks;
using AeroAscent.Apresentacao.MAUI.Servicos;
using AeroAscent.Core.Aplicacao.Apresentadores;
using AeroAscent.Core.Aplicacao.Contratos;
using AeroAscent.Core.Aplicacao.DTOs;
using Microsoft.Maui.Controls;

/// <summary>
/// Code-behind da página de resumo de voo e premiação, implementando a visão passiva <see cref="IVisaoResumoVoo"/>.
/// </summary>
public partial class PaginaResumoVoo : ContentPage, IVisaoResumoVoo
{
    private readonly GerenciadorSessaoJogo _gerenciadorSessao;
    private readonly ApresentadorResumoVoo _apresentador;

    /// <inheritdoc />
    public event Action? AoClicarOficina;

    /// <inheritdoc />
    public event Action? AoClicarVoarNovamente;

    /// <inheritdoc />
    public event Action? AoClicarPularAnimacao;

    /// <inheritdoc />
    public event Action? AoConcluirAnimacaoMoedas;

    public PaginaResumoVoo(GerenciadorSessaoJogo gerenciadorSessao)
    {
        InitializeComponent();

        _gerenciadorSessao = gerenciadorSessao;
        _apresentador = new ApresentadorResumoVoo(this);

        _apresentador.AoSolicitarIrParaOficina += OnNavegarParaOficina;
        _apresentador.AoSolicitarVoarNovamente += OnNavegarParaVoo;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _apresentador.Exibir(_gerenciadorSessao.UltimoResumo);
    }

    private void OnOficinaClicked(object? sender, EventArgs e)
    {
        AoClicarPularAnimacao?.Invoke();
        AoClicarOficina?.Invoke();
    }

    private void OnVoarNovamenteClicked(object? sender, EventArgs e)
    {
        AoClicarPularAnimacao?.Invoke();
        AoClicarVoarNovamente?.Invoke();
    }

    private async void OnNavegarParaOficina()
    {
        await Navigation.PopToRootAsync();
    }

    private async void OnNavegarParaVoo()
    {
        _gerenciadorSessao.PrepararNovoVoo();
        // Substitui a página atual pela página de voo
        var paginaVoo = new PaginaVoo(_gerenciadorSessao);
        Navigation.InsertPageBefore(paginaVoo, this);
        await Navigation.PopAsync();
    }

    #region Implementação de IVisaoResumoVoo

    /// <inheritdoc />
    public void ExibirResumo(in ModeloVisualResumoVoo modelo)
    {
        var dados = modelo;
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            BorderRecorde.IsVisible = dados.EhNovoRecorde;
            LabelDistancia.Text = $"{dados.DistanciaFormatada} (+{dados.MoedasDistancia} 💰)";
            LabelAltitude.Text = $"{dados.AltitudeFormatada} (+{dados.MoedasAltitude} 💰)";
            LabelMoedasColetadas.Text = $"+{dados.MoedasColetadas} 💰";
            LabelSaldoFinal.Text = dados.SaldoFinalFormatado;

            // Animação numérica da contagem de moedas ganhas
            var totalAlvo = dados.TotalMoedasGanhas;
            var passos = 20;
            var incremento = Math.Max(1L, totalAlvo / passos);
            var contador = 0L;

            for (int i = 0; i < passos && contador < totalAlvo; i++)
            {
                contador += incremento;
                if (contador > totalAlvo) contador = totalAlvo;
                LabelTotalGanho.Text = $"+{contador} 💰";
                await Task.Delay(40);
            }

            LabelTotalGanho.Text = dados.TotalMoedasFormatado;
            AoConcluirAnimacaoMoedas?.Invoke();
            HabilitarBotoesNavegacao(true);
        });
    }

    /// <inheritdoc />
    public void ConcluirAnimacaoMoedas()
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            HabilitarBotoesNavegacao(true);
        });
    }

    /// <inheritdoc />
    public void HabilitarBotoesNavegacao(bool habilitado)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            BtnOficina.IsEnabled = habilitado;
            BtnOficina.Opacity = habilitado ? 1.0 : 0.6;
            BtnVoarNovamente.IsEnabled = habilitado;
            BtnVoarNovamente.Opacity = habilitado ? 1.0 : 0.6;
        });
    }

    /// <inheritdoc />
    public void Ocultar()
    {
        // Sem-op
    }

    #endregion
}
