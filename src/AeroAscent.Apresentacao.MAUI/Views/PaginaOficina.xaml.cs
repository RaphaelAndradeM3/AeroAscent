namespace AeroAscent.Apresentacao.MAUI.Views;

using System;
using System.Threading.Tasks;
using AeroAscent.Apresentacao.MAUI.Servicos;
using AeroAscent.Core.Aplicacao.Apresentadores;
using AeroAscent.Core.Aplicacao.Contratos;
using AeroAscent.Core.Aplicacao.DTOs;
using AeroAscent.Core.Dominio.Enums;
using Microsoft.Maui.Controls;

/// <summary>
/// Code-behind da página da Oficina e Hangar, implementando a visão passiva <see cref="IVisaoOficina"/>.
/// </summary>
public partial class PaginaOficina : ContentPage, IVisaoOficina
{
    private readonly ApresentadorOficina _apresentador;
    private readonly GerenciadorSessaoJogo _gerenciadorSessao;

    /// <inheritdoc />
    public event Action<TipoMelhoria>? AoClicarComprar;

    /// <inheritdoc />
    public event Action? AoClicarDecolar;

    public PaginaOficina(
        IConsultarOficinaCasoDeUso consultarOficina,
        IComprarMelhoriaCasoDeUso comprarMelhoria,
        Core.Dominio.Contratos.IRepositorioProgresso repositorioProgresso,
        GerenciadorSessaoJogo gerenciadorSessao)
    {
        InitializeComponent();

        _gerenciadorSessao = gerenciadorSessao;

        // O Apresentador é instanciado vinculado a esta visão passiva
        _apresentador = new ApresentadorOficina(
            consultarOficina,
            comprarMelhoria,
            repositorioProgresso,
            this);

        _apresentador.AoSolicitarDecolagem += OnDecolagemSolicitadaPeloApresentador;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _gerenciadorSessao.InicializarAsync();
        await _apresentador.InicializarAsync();
    }

    private void OnComprarMotorClicked(object? sender, EventArgs e)
    {
        AoClicarComprar?.Invoke(TipoMelhoria.Motor);
    }

    private void OnComprarAerodinamicaClicked(object? sender, EventArgs e)
    {
        AoClicarComprar?.Invoke(TipoMelhoria.Aerodinamica);
    }

    private void OnComprarCombustivelClicked(object? sender, EventArgs e)
    {
        AoClicarComprar?.Invoke(TipoMelhoria.TanqueCombustivel);
    }

    private void OnComprarCatapultaClicked(object? sender, EventArgs e)
    {
        AoClicarComprar?.Invoke(TipoMelhoria.Catapulta);
    }

    private void OnDecolarClicked(object? sender, EventArgs e)
    {
        AoClicarDecolar?.Invoke();
    }

    private async void OnDecolagemSolicitadaPeloApresentador()
    {
        _gerenciadorSessao.PrepararNovoVoo();
        await Navigation.PushAsync(new PaginaVoo(_gerenciadorSessao));
    }

    /// <inheritdoc />
    public void AtualizarTela(ModeloVisualOficina modelo)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            LabelSaldo.Text = modelo.SaldoFormatado;
            LabelRecordeDistancia.Text = $"🏆 {modelo.RecordeDistanciaFormatado}";
            LabelRecordeAltitude.Text = $"☁️ {modelo.RecordeAltitudeFormatado}";

            // Itera e atualiza os 4 cartões de melhoria
            foreach (var card in modelo.Cartoes)
            {
                switch (card.Tipo)
                {
                    case TipoMelhoria.Motor:
                        LabelNivelMotor.Text = card.TextoNivel;
                        ProgressMotor.Progress = card.ProgressoNormalizado;
                        BtnMotor.Text = card.TextoBotao;
                        BtnMotor.IsEnabled = card.PodeComprar;
                        BtnMotor.Opacity = card.PodeComprar ? 1.0 : 0.6;
                        break;

                    case TipoMelhoria.Aerodinamica:
                        LabelNivelAerodinamica.Text = card.TextoNivel;
                        ProgressAerodinamica.Progress = card.ProgressoNormalizado;
                        BtnAerodinamica.Text = card.TextoBotao;
                        BtnAerodinamica.IsEnabled = card.PodeComprar;
                        BtnAerodinamica.Opacity = card.PodeComprar ? 1.0 : 0.6;
                        break;

                    case TipoMelhoria.TanqueCombustivel:
                        LabelNivelCombustivel.Text = card.TextoNivel;
                        ProgressCombustivel.Progress = card.ProgressoNormalizado;
                        BtnCombustivel.Text = card.TextoBotao;
                        BtnCombustivel.IsEnabled = card.PodeComprar;
                        BtnCombustivel.Opacity = card.PodeComprar ? 1.0 : 0.6;
                        break;

                    case TipoMelhoria.Catapulta:
                        LabelNivelCatapulta.Text = card.TextoNivel;
                        ProgressCatapulta.Progress = card.ProgressoNormalizado;
                        BtnCatapulta.Text = card.TextoBotao;
                        BtnCatapulta.IsEnabled = card.PodeComprar;
                        BtnCatapulta.Opacity = card.PodeComprar ? 1.0 : 0.6;
                        break;
                }
            }
        });
    }

    /// <inheritdoc />
    public void DefinirInteracaoHabilitada(bool habilitada)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            BtnMotor.IsEnabled = habilitada;
            BtnAerodinamica.IsEnabled = habilitada;
            BtnCombustivel.IsEnabled = habilitada;
            BtnCatapulta.IsEnabled = habilitada;
            BtnDecolar.IsEnabled = habilitada;
        });
    }

    /// <inheritdoc />
    public void ExibirFeedbackCompra(TipoMelhoria tipo, int novoNivel)
    {
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            BorderFeedback.BackgroundColor = Color.FromArgb("#10B981");
            LabelFeedback.Text = $"🎉 {tipo} evoluído para o Nível {novoNivel}!";
            BorderFeedback.IsVisible = true;
            await Task.Delay(2000);
            BorderFeedback.IsVisible = false;
        });
    }

    /// <inheritdoc />
    public void ExibirMensagemErro(string mensagem)
    {
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            BorderFeedback.BackgroundColor = Color.FromArgb("#EF4444");
            LabelFeedback.Text = $"⚠️ {mensagem}";
            BorderFeedback.IsVisible = true;
            await Task.Delay(2500);
            BorderFeedback.IsVisible = false;
        });
    }
}
