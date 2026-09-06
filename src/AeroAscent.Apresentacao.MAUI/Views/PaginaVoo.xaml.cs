namespace AeroAscent.Apresentacao.MAUI.Views;

using System;
using System.Diagnostics;
using System.Threading.Tasks;
using AeroAscent.Apresentacao.MAUI.Renderizadores;
using AeroAscent.Apresentacao.MAUI.Servicos;
using AeroAscent.Core.Aplicacao.Apresentadores;
using AeroAscent.Core.Aplicacao.Contratos;
using AeroAscent.Core.Aplicacao.DTOs;
using AeroAscent.Core.Dominio.Enums;
using Microsoft.Maui.Controls;

/// <summary>
/// Code-behind da página de simulação de voo e visualização 2D em tempo real,
/// implementando a visão passiva <see cref="IVisaoHUDVoo"/>.
/// </summary>
public partial class PaginaVoo : ContentPage, IVisaoHUDVoo
{
    private readonly GerenciadorSessaoJogo _gerenciadorSessao;
    private readonly ApresentadorHUDVoo _apresentadorHUD;
    private readonly CanvasVooDrawable _drawable;
    private readonly IDispatcherTimer _timer;
    private readonly Stopwatch _cronometro = new();

    private float _faseCatapulta;
    private bool _emVooAtivo;
    private bool _finalizandoVoo;

    /// <inheritdoc />
    public event Action? AoSolicitarSubida;

    /// <inheritdoc />
    public event Action? AoInterromperSubida;

    /// <inheritdoc />
    public event Action? AoSolicitarDescida;

    /// <inheritdoc />
    public event Action? AoInterromperDescida;

    /// <inheritdoc />
    public event Action? AoSolicitarBoost;

    /// <inheritdoc />
    public event Action? AoInterromperBoost;

    /// <inheritdoc />
    public event Action? AoSolicitarPausa;

    public PaginaVoo(GerenciadorSessaoJogo gerenciadorSessao)
    {
        InitializeComponent();

        _gerenciadorSessao = gerenciadorSessao;
        _drawable = new CanvasVooDrawable
        {
            GerenciadorParticulas = _gerenciadorSessao.GerenciadorParticulas,
            ColetaveisAtivos = _gerenciadorSessao.ColetaveisAtivos,
            RecordeDistancia = _gerenciadorSessao.Progresso?.RecordeDistanciaMetros ?? 0f
        };

        VisualizadorGrafico.Drawable = _drawable;

        _apresentadorHUD = new ApresentadorHUDVoo(this);

        // Timer de simulação a ~60 FPS (16ms)
        _timer = Dispatcher.CreateTimer();
        _timer.Interval = TimeSpan.FromMilliseconds(16);
        _timer.Tick += OnGameLoopTick;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        var recorde = _gerenciadorSessao.Progresso?.RecordeDistanciaMetros ?? 0f;
        _drawable.RecordeDistancia = recorde;
        _drawable.EstadoAeronave = _gerenciadorSessao.EstadoFisico;
        _drawable.StatusAtual = StatusVoo.EmPreparacao;

        _apresentadorHUD.Inicializar(recorde);

        _emVooAtivo = false;
        _finalizandoVoo = false;
        PainelCatapulta.IsVisible = true;
        PainelHUDVoo.IsVisible = false;

        _cronometro.Restart();
        _timer.Start();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _timer.Stop();
        _cronometro.Stop();
    }

    private void OnDispararCatapultaClicked(object? sender, EventArgs e)
    {
        if (_emVooAtivo) return;

        // Precisão baseada no valor instantâneo da barra (0.0 a 1.0)
        var precisao = (float)BarraForcaCatapulta.Progress;

        var resultado = _gerenciadorSessao.LancarAeronave(precisao);
        if (resultado.Sucesso)
        {
            _emVooAtivo = true;
            PainelCatapulta.IsVisible = false;
            PainelHUDVoo.IsVisible = true;
            _drawable.StatusAtual = StatusVoo.EmVoo;
        }
    }

    private async void OnGameLoopTick(object? sender, EventArgs e)
    {
        var deltaSegundos = (float)_cronometro.Elapsed.TotalSeconds;
        _cronometro.Restart();

        // Limita o passo de tempo a 50ms para prevenir instabilidades físicas em caso de congelamento do SO
        if (deltaSegundos > 0.05f)
        {
            deltaSegundos = 0.05f;
        }

        if (!_emVooAtivo)
        {
            // Fase de preparação: oscilação do medidor de força da catapulta
            _faseCatapulta += deltaSegundos * 3.5f;
            var valorOscilante = (MathF.Sin(_faseCatapulta) + 1f) * 0.5f;
            BarraForcaCatapulta.Progress = valorOscilante;

            if (valorOscilante >= 0.8f && valorOscilante <= 0.95f)
            {
                BarraForcaCatapulta.ProgressColor = Color.FromArgb("#10B981");
                LabelStatusForca.Text = "⚡ ZONA PERFEITA! (BÔNUS MÁXIMO)";
                LabelStatusForca.TextColor = Color.FromArgb("#34D399");
            }
            else
            {
                BarraForcaCatapulta.ProgressColor = Color.FromArgb("#F59E0B");
                LabelStatusForca.Text = "Zona Ideal: 80% - 95%";
                LabelStatusForca.TextColor = Color.FromArgb("#FCD34D");
            }

            _drawable.EstadoAeronave = _gerenciadorSessao.EstadoFisico;
            VisualizadorGrafico.Invalidate();
            return;
        }

        // Fase de voo ativo: consome comandos do piloto e atualiza a simulação física
        var comandoPiloto = _apresentadorHUD.ObterComandosControle();
        _gerenciadorSessao.AtualizarFrameVoo(comandoPiloto, deltaSegundos);

        var vooAtual = _gerenciadorSessao.VooAtual;
        var estadoAtual = _gerenciadorSessao.EstadoFisico;

        _drawable.EstadoAeronave = estadoAtual;
        _drawable.ColetaveisAtivos = _gerenciadorSessao.ColetaveisAtivos;
        VisualizadorGrafico.Invalidate();

        if (vooAtual != null)
        {
            _apresentadorHUD.Atualizar(vooAtual, estadoAtual);

            // Verificação de parada e transição para pouso
            if (vooAtual.Status == StatusVoo.Pousado && !_finalizandoVoo)
            {
                _finalizandoVoo = true;
                _timer.Stop();
                _cronometro.Stop();

                // Aguarda 1 segundo contemplativo da aeronave parada no solo
                await Task.Delay(1000);

                await _gerenciadorSessao.FinalizarVooAsync();
                await Navigation.PushAsync(new PaginaResumoVoo(_gerenciadorSessao));
            }
        }
    }

    #region Eventos de Controle do Piloto (Subir, Descer, Boost)

    private void OnSubirPressed(object? sender, EventArgs e) => AoSolicitarSubida?.Invoke();
    private void OnSubirReleased(object? sender, EventArgs e) => AoInterromperSubida?.Invoke();

    private void OnDescerPressed(object? sender, EventArgs e) => AoSolicitarDescida?.Invoke();
    private void OnDescerReleased(object? sender, EventArgs e) => AoInterromperDescida?.Invoke();

    private void OnBoostPressed(object? sender, EventArgs e) => AoSolicitarBoost?.Invoke();
    private void OnBoostReleased(object? sender, EventArgs e) => AoInterromperBoost?.Invoke();

    /// <summary>
    /// Alterna o estado de pausa da simulação.
    /// </summary>
    public void AlternarPausa() => AoSolicitarPausa?.Invoke();

    #endregion

    #region Implementação de IVisaoHUDVoo

    /// <inheritdoc />
    public void AtualizarTelemetria(in TelemetriaHUDDTO telemetria)
    {
        LabelDistancia.Text = $"{telemetria.DistanciaPercorridaMetros:F1} m";
        LabelAltitude.Text = $"{telemetria.AltitudeAtualMetros:F1} m";
        // Conversão de m/s para km/h (* 3.6)
        var kmh = telemetria.VelocidadeAtualMetrosPorSegundo * 3.6f;
        LabelVelocidade.Text = $"{kmh:F0} km/h";
        BarraCombustivel.Progress = telemetria.PercentualCombustivel;
        LabelMoedasColetadas.Text = $"💰 +{telemetria.MoedasColetadas}";
    }

    /// <inheritdoc />
    public void DefinirInteratividadeBoost(bool disponivel)
    {
        BtnBoost.IsEnabled = disponivel;
        BtnBoost.Opacity = disponivel ? 1.0 : 0.5;
    }

    /// <inheritdoc />
    public void NotificarNovoRecorde()
    {
        BadgeNovoRecorde.IsVisible = true;
    }

    /// <inheritdoc />
    public void DefinirVisibilidadeControles(bool visivel)
    {
        PainelControles.IsVisible = visivel;
    }

    #endregion
}
