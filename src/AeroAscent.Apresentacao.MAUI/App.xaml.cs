namespace AeroAscent.Apresentacao.MAUI;

using AeroAscent.Apresentacao.MAUI.Views;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

public partial class App : Application
{
    private readonly PaginaOficina _paginaOficina;

    public App(PaginaOficina paginaOficina)
    {
        InitializeComponent();
        _paginaOficina = paginaOficina;
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        var paginaNavegacao = new NavigationPage(_paginaOficina)
        {
            BarBackgroundColor = Color.FromArgb("#0F172A"),
            BarTextColor = Colors.White
        };

        NavigationPage.SetHasNavigationBar(_paginaOficina, false);

        var janela = new Window(paginaNavegacao)
        {
            Title = "AeroAscent — Jogo de Voo Arcade",
            Width = 900,
            Height = 650
        };

        return janela;
    }
}