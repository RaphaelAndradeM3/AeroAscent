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
    /// <summary>
    /// Gerenciador do ciclo de vida da sessão de jogo, física, partículas e áudio.
    /// </summary>
    private readonly GerenciadorSessaoJogo _gerenciadorSessao;

    /// <summary>
    /// Apresentador do HUD de voo responsável por orquestrar a telemetria e traduzir comandos de entrada.
    /// </summary>
    private readonly ApresentadorHUDVoo _apresentadorHUD;

    /// <summary>
    /// Renderizador gráfico 2D em hardware via MAUI Graphics.
    /// </summary>
    private readonly CanvasVooDrawable _drawable;

    /// <summary>
    /// Temporizador do loop de física e renderização (taxa alvo de 60 FPS / ~16ms).
    /// </summary>
    private readonly IDispatcherTimer _timer;

    /// <summary>
    /// Cronômetro de precisão para cálculo do delta time entre frames.
    /// </summary>
    private readonly Stopwatch _cronometro = new();

    /// <summary>
    /// Fase angular acumulada para oscilação sinusoidal da barra de força da catapulta.
    /// </summary>
    private float _faseCatapulta;

    /// <summary>
    /// Indica se a aeronave já foi lançada e encontra-se em voo livre ou pousando.
    /// </summary>
    private bool _emVooAtivo;

    /// <summary>
    /// Indica se o voo chegou ao fim e a transição para o resumo está em andamento.
    /// </summary>
    private bool _finalizandoVoo;

#if WINDOWS
    /// <summary>
    /// Indica que a catapulta foi disparada utilizando a barra de espaço, exigindo que o piloto
    /// solte a tecla antes de ativar o boost da turbina em voo.
    /// </summary>
    private bool _aguardandoLiberarEspacoDisparo;

    /// <summary>
    /// Indica se o comando de arfagem para cima (pitch up) via teclado está atualmente ativo.
    /// </summary>
    private bool _teclaSubidaAtiva;

    /// <summary>
    /// Indica se o comando de arfagem para baixo (pitch down) via teclado está atualmente ativo.
    /// </summary>
    private bool _teclaDescidaAtiva;

    /// <summary>
    /// Indica se o comando de propulsão a jato (boost) via teclado está atualmente ativo.
    /// </summary>
    private bool _teclaBoostAtiva;

    /// <summary>
    /// Consulta o estado físico instantâneo de uma tecla do teclado no Windows via Win32.
    /// </summary>
    /// <param name="vKey">Código da tecla virtual (Virtual-Key Code).</param>
    /// <returns>Valor com bit mais significativo setado caso a tecla esteja fisicamente pressionada.</returns>
    [System.Runtime.InteropServices.DllImport("user32.dll")]
    private static extern short GetAsyncKeyState(int vKey);

    /// <summary>
    /// Obtém o identificador da janela em primeiro plano (ativa) no sistema operacional.
    /// </summary>
    /// <returns>Handle (HWND) da janela ativa no Windows.</returns>
    [System.Runtime.InteropServices.DllImport("user32.dll")]
    private static extern IntPtr GetForegroundWindow();

    /// <summary>
    /// Obtém o identificador do processo proprietário da janela informada.
    /// </summary>
    /// <param name="hWnd">Handle da janela.</param>
    /// <param name="processId">Identificador do processo de saída.</param>
    /// <returns>Identificador da thread.</returns>
    [System.Runtime.InteropServices.DllImport("user32.dll")]
    private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint processId);

    /// <summary>Código da tecla virtual ENTER (0x0D).</summary>
    private const int VK_RETURN = 0x0D;

    /// <summary>Código da tecla virtual Barra de Espaço (0x20).</summary>
    private const int VK_SPACE = 0x20;

    /// <summary>Código da tecla virtual Seta para Cima (0x26).</summary>
    private const int VK_UP = 0x26;

    /// <summary>Código da tecla virtual Seta para Baixo (0x28).</summary>
    private const int VK_DOWN = 0x28;

    /// <summary>Código da tecla virtual W (0x57).</summary>
    private const int VK_W = 0x57;

    /// <summary>Código da tecla virtual S (0x53).</summary>
    private const int VK_S = 0x53;

    /// <summary>
    /// Verifica se a tecla virtual especificada está fisicamente pressionada no hardware.
    /// </summary>
    /// <param name="vKey">Código da tecla virtual.</param>
    /// <returns>Verdadeiro se pressionada; falso caso contrário.</returns>
    private static bool TeclaPressionada(int vKey)
    {
        return (GetAsyncKeyState(vKey) & 0x8000) != 0;
    }

    /// <summary>
    /// Verifica se a janela do jogo é a janela atualmente ativa (em primeiro plano) no Windows,
    /// prevenindo que teclas pressionadas em outros aplicativos interfiram na simulação.
    /// </summary>
    /// <returns>Verdadeiro se a janela do jogo estiver em primeiro plano.</returns>
    private static bool JanelaDoJogoEstaAtiva()
    {
        var hwndAtivo = GetForegroundWindow();
        if (hwndAtivo == IntPtr.Zero)
        {
            return true;
        }

        GetWindowThreadProcessId(hwndAtivo, out uint idProcessoAtivo);
        return idProcessoAtivo == (uint)Environment.ProcessId;
    }
#endif

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

    /// <summary>
    /// Construtor da página de simulação de voo 2D, vinculando os subsistemas e apresentador.
    /// </summary>
    /// <param name="gerenciadorSessao">Instância do orquestrador de sessão injetada via DI.</param>
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

    /// <summary>
    /// Executado quando a página torna-se visível na navegação, inicializando o estado de voo.
    /// </summary>
    protected override void OnAppearing()
    {
        base.OnAppearing();

        if (_gerenciadorSessao.VooAtual == null)
        {
            _gerenciadorSessao.PrepararNovoVoo();
        }

        var recorde = _gerenciadorSessao.Progresso?.RecordeDistanciaMetros ?? 0f;
        _drawable.RecordeDistancia = recorde;
        _drawable.EstadoAeronave = _gerenciadorSessao.EstadoFisico;
        _drawable.StatusAtual = StatusVoo.EmPreparacao;

        _apresentadorHUD.Inicializar(recorde);

        _emVooAtivo = false;
        _finalizandoVoo = false;

#if WINDOWS
        _aguardandoLiberarEspacoDisparo = false;
        _teclaSubidaAtiva = false;
        _teclaDescidaAtiva = false;
        _teclaBoostAtiva = false;
#endif

        BtnDisparar.IsEnabled = true;

        OverlayCatapulta.IsVisible = true;
        OverlayCatapulta.Opacity = 1;
        OverlayCatapulta.InputTransparent = false;

        PainelCatapulta.IsVisible = true;
        PainelCatapulta.Opacity = 1;
        PainelCatapulta.InputTransparent = false;

        PainelHUDVoo.IsVisible = false;
        PainelHUDVoo.Opacity = 0;
        PainelHUDVoo.InputTransparent = true;
        BadgeNovoRecorde.IsVisible = false;

        _cronometro.Restart();
        _timer.Start();
    }

    /// <summary>
    /// Executado quando a página é ocultada ou descarregada, pausando o loop de simulação e liberando teclas ativas.
    /// </summary>
    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _timer.Stop();
        _cronometro.Stop();

#if WINDOWS
        if (_teclaBoostAtiva) AoInterromperBoost?.Invoke();
        if (_teclaSubidaAtiva) AoInterromperSubida?.Invoke();
        if (_teclaDescidaAtiva) AoInterromperDescida?.Invoke();
        _teclaBoostAtiva = false;
        _teclaSubidaAtiva = false;
        _teclaDescidaAtiva = false;
#endif
    }

    /// <summary>
    /// Manipulador do evento de clique no botão de disparo da catapulta.
    /// </summary>
    private void OnDispararCatapultaClicked(object? sender, EventArgs e)
    {
        DispararCatapulta();
    }

    /// <summary>
    /// Efetua o lançamento da aeronave com base na precisão instantânea do medidor de força,
    /// ocultando o painel de preparação e ativando o HUD de voo.
    /// </summary>
    private void DispararCatapulta()
    {
        if (_emVooAtivo) return;

        // Precisão baseada no valor instantâneo da barra (0.0 a 1.0)
        var precisao = (float)BarraForcaCatapulta.Progress;

        var resultado = _gerenciadorSessao.LancarAeronave(precisao);
        if (resultado.Sucesso)
        {
            _emVooAtivo = true;

#if WINDOWS
            // Se o lançamento foi com a tecla espaço, aguarda soltá-la antes de ligar o boost
            _aguardandoLiberarEspacoDisparo = TeclaPressionada(VK_SPACE);
#endif

            BtnDisparar.IsEnabled = false;
            BtnDisparar.Unfocus();

            OverlayCatapulta.IsVisible = false;
            OverlayCatapulta.Opacity = 0;
            OverlayCatapulta.InputTransparent = true;

            PainelCatapulta.IsVisible = false;
            PainelCatapulta.Opacity = 0;
            PainelCatapulta.InputTransparent = true;

            PainelHUDVoo.IsVisible = true;
            PainelHUDVoo.Opacity = 1;
            PainelHUDVoo.InputTransparent = false;

            _drawable.StatusAtual = StatusVoo.EmVoo;
        }
        else
        {
            LabelStatusForca.Text = $"⚠️ {resultado.MensagemErro}";
            LabelStatusForca.TextColor = Color.FromArgb("#EF4444");
        }
    }

    /// <summary>
    /// Loop principal de jogo disparado a ~60 FPS pelo dispatcher da interface gráfica.
    /// Realiza a oscilação da catapulta na preparação ou processa a física, partículas, áudio e telemetria em voo ativo.
    /// </summary>
    /// <param name="sender">Origem do evento do timer.</param>
    /// <param name="e">Argumentos do evento de tick.</param>
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
#if WINDOWS
            // Suporte imediato via teclado para disparar a catapulta no Windows com Enter ou Espaço
            if (JanelaDoJogoEstaAtiva())
            {
                if (TeclaPressionada(VK_RETURN) || TeclaPressionada(VK_SPACE))
                {
                    DispararCatapulta();
                    return;
                }
            }
#endif

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

#if WINDOWS
        // Polling contínuo de hardware com zero alocação (GC Alloc = 0 bytes) para controles fluidos no Windows
        if (JanelaDoJogoEstaAtiva())
        {
            // 1. Propulsão / Boost (Barra de Espaço)
            var espacoPressionado = TeclaPressionada(VK_SPACE);
            if (_aguardandoLiberarEspacoDisparo)
            {
                if (!espacoPressionado)
                {
                    _aguardandoLiberarEspacoDisparo = false;
                }
            }
            else
            {
                if (espacoPressionado && !_teclaBoostAtiva)
                {
                    _teclaBoostAtiva = true;
                    AoSolicitarBoost?.Invoke();
                }
                else if (!espacoPressionado && _teclaBoostAtiva)
                {
                    _teclaBoostAtiva = false;
                    AoInterromperBoost?.Invoke();
                }
            }

            // 2. Inclinação para Subida (W ou Seta para Cima)
            var subidaPressionada = TeclaPressionada(VK_W) || TeclaPressionada(VK_UP);
            if (subidaPressionada && !_teclaSubidaAtiva)
            {
                _teclaSubidaAtiva = true;
                AoSolicitarSubida?.Invoke();
            }
            else if (!subidaPressionada && _teclaSubidaAtiva)
            {
                _teclaSubidaAtiva = false;
                AoInterromperSubida?.Invoke();
            }

            // 3. Inclinação para Descida (S ou Seta para Baixo)
            var descidaPressionada = TeclaPressionada(VK_S) || TeclaPressionada(VK_DOWN);
            if (descidaPressionada && !_teclaDescidaAtiva)
            {
                _teclaDescidaAtiva = true;
                AoSolicitarDescida?.Invoke();
            }
            else if (!descidaPressionada && _teclaDescidaAtiva)
            {
                _teclaDescidaAtiva = false;
                AoInterromperDescida?.Invoke();
            }
        }
        else
        {
            // Se a janela perdeu foco, desativa entradas ativas para segurança
            if (_teclaBoostAtiva) { _teclaBoostAtiva = false; AoInterromperBoost?.Invoke(); }
            if (_teclaSubidaAtiva) { _teclaSubidaAtiva = false; AoInterromperSubida?.Invoke(); }
            if (_teclaDescidaAtiva) { _teclaDescidaAtiva = false; AoInterromperDescida?.Invoke(); }
        }
#endif

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

#if WINDOWS
                if (_teclaBoostAtiva) AoInterromperBoost?.Invoke();
                if (_teclaSubidaAtiva) AoInterromperSubida?.Invoke();
                if (_teclaDescidaAtiva) AoInterromperDescida?.Invoke();
                _teclaBoostAtiva = false;
                _teclaSubidaAtiva = false;
                _teclaDescidaAtiva = false;
#endif

                // Aguarda 1 segundo contemplativo da aeronave parada no solo
                await Task.Delay(1000);

                await _gerenciadorSessao.FinalizarVooAsync();
                await Navigation.PushAsync(new PaginaResumoVoo(_gerenciadorSessao));
            }
        }
    }

    #region Eventos de Controle do Piloto (Subir, Descer, Boost)

    /// <summary>Inicia comando de subida ao pressionar o botão tátil.</summary>
    private void OnSubirPressed(object? sender, EventArgs e) => AoSolicitarSubida?.Invoke();

    /// <summary>Interrompe comando de subida ao soltar o botão tátil.</summary>
    private void OnSubirReleased(object? sender, EventArgs e) => AoInterromperSubida?.Invoke();

    /// <summary>Inicia comando de descida ao pressionar o botão tátil.</summary>
    private void OnDescerPressed(object? sender, EventArgs e) => AoSolicitarDescida?.Invoke();

    /// <summary>Interrompe comando de descida ao soltar o botão tátil.</summary>
    private void OnDescerReleased(object? sender, EventArgs e) => AoInterromperDescida?.Invoke();

    /// <summary>Inicia acionamento da propulsão (boost) ao pressionar o botão tátil.</summary>
    private void OnBoostPressed(object? sender, EventArgs e) => AoSolicitarBoost?.Invoke();

    /// <summary>Interrompe acionamento da propulsão (boost) ao soltar o botão tátil.</summary>
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
