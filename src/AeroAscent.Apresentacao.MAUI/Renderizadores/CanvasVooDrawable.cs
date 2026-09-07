namespace AeroAscent.Apresentacao.MAUI.Renderizadores;

using System;
using System.Collections.Generic;
using AeroAscent.Apresentacao.MAUI.Servicos;
using AeroAscent.Core.Dominio.Entidades;
using AeroAscent.Core.Dominio.Enums;
using AeroAscent.Core.Dominio.ObjetosDeValor;
using Microsoft.Maui.Graphics;

/// <summary>
/// Renderizador gráfico 2D em hardware para a simulação de voo do AeroAscent no .NET MAUI.
/// Implementa <see cref="IDrawable"/>, desenhando céu, solo, catapulta, aeronave, partículas e coletáveis com zero GC Alloc no loop.
/// </summary>
public sealed class CanvasVooDrawable : IDrawable
{
    private const float PIXELS_POR_METRO = 10f;

    public EstadoFisicoAeronave EstadoAeronave { get; set; }
    public StatusVoo StatusAtual { get; set; } = StatusVoo.EmPreparacao;
    public float RecordeDistancia { get; set; }
    public IList<Coletavel>? ColetaveisAtivos { get; set; }
    public GerenciadorParticulasMAUI? GerenciadorParticulas { get; set; }

    // Posições suaves da câmera
    private float _cameraZ;
    private float _cameraY;

    /// <summary>
    /// Renderiza todo o frame do mundo 2D de voo no canvas MAUI.
    /// </summary>
    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        var largura = dirtyRect.Width;
        var altura = dirtyRect.Height;

        if (largura <= 0 || altura <= 0)
        {
            return;
        }

        // Suavização da câmera em direção à aeronave
        var alvoZ = EstadoAeronave.Posicao.Z;
        var alvoY = EstadoAeronave.Posicao.Y;

        _cameraZ += (alvoZ - _cameraZ) * 0.15f;
        _cameraY += (alvoY - _cameraY) * 0.15f;

        // Ponto de ancoragem da aeronave na tela (25% da esquerda, 65% da altura para ter bastante visão do céu)
        var telaAviaoX = largura * 0.25f;
        var telaChaoY = altura * 0.75f + (_cameraY * PIXELS_POR_METRO);

        // 1. Céu com gradiente dinâmico dependendo da altitude
        DesenharCeu(canvas, largura, altura, _cameraY);

        // 2. Marcadores métricos de distância e altitude
        DesenharGradeMetrica(canvas, largura, altura, telaChaoY, _cameraZ);

        // 3. Solo e pista de pouso
        DesenharSolo(canvas, largura, altura, telaChaoY, _cameraZ);

        // 4. Catapulta de lançamento no ponto Z = 0
        DesenharCatapulta(canvas, telaAviaoX, telaChaoY, _cameraZ);

        // 5. Coletáveis no ar (Moedas e Anéis de Vento)
        DesenharColetaveis(canvas, telaAviaoX, telaChaoY, _cameraZ);

        // 6. Partículas ativas do mundo
        DesenharParticulas(canvas, telaAviaoX, telaChaoY, _cameraZ);

        // 7. Aeronave
        DesenharAeronave(canvas, telaAviaoX, telaChaoY - (EstadoAeronave.Posicao.Y * PIXELS_POR_METRO), EstadoAeronave.InclinacaoPitchGraus);
    }

    private void DesenharCeu(ICanvas canvas, float largura, float altura, float cameraY)
    {
        // Conforme a altitude aumenta, o céu transita para um azul mais escuro/estratosférico
        var fatorAltitude = Math.Clamp(cameraY / 300f, 0f, 1f);
        var corTopo = Color.FromRgba(0.1f * (1f - fatorAltitude), 0.3f * (1f - fatorAltitude) + 0.1f, 0.7f * (1f - fatorAltitude) + 0.3f, 1f);
        var corBase = Color.FromRgba(0.6f - (0.3f * fatorAltitude), 0.85f - (0.3f * fatorAltitude), 1f, 1f);

        var paint = new LinearGradientPaint
        {
            StartPoint = new Point(0, 0),
            EndPoint = new Point(0, 1),
            GradientStops =
            [
                new PaintGradientStop(0.0f, corTopo),
                new PaintGradientStop(1.0f, corBase)
            ]
        };

        canvas.SetFillPaint(paint, new RectF(0, 0, largura, altura));
        canvas.FillRectangle(0, 0, largura, altura);

        // Nuvens estilizadas flutuantes com efeito paralaxe
        canvas.FillColor = Colors.White.WithAlpha(0.45f);
        for (int i = 0; i < 6; i++)
        {
            var nuvemX = ((i * 350f) - (_cameraZ * 2f)) % (largura + 400f) - 100f;
            var nuvemY = 40f + (i * 35f) + (_cameraY * 1.5f);
            if (nuvemY > 0 && nuvemY < altura)
            {
                canvas.FillRoundedRectangle(nuvemX, nuvemY, 120f, 35f, 18f);
                canvas.FillRoundedRectangle(nuvemX + 30f, nuvemY - 15f, 60f, 30f, 15f);
            }
        }
    }

    private void DesenharSolo(ICanvas canvas, float largura, float altura, float telaChaoY, float cameraZ)
    {
        if (telaChaoY < altura)
        {
            // Grama superficial
            canvas.FillColor = Color.FromArgb("#4CAF50");
            canvas.FillRectangle(0, telaChaoY, largura, 16f);

            // Terra abaixo da grama
            canvas.FillColor = Color.FromArgb("#5D4037");
            canvas.FillRectangle(0, telaChaoY + 16f, largura, Math.Max(0, altura - (telaChaoY + 16f)));

            // Faixa de asfalto da pista de decolagem próxima a Z = 0
            var pistaInicioTela = (0f - cameraZ) * PIXELS_POR_METRO + (largura * 0.25f);
            var pistaComprimento = 60f * PIXELS_POR_METRO;
            if (pistaInicioTela + pistaComprimento > 0 && pistaInicioTela < largura)
            {
                canvas.FillColor = Color.FromArgb("#37474F");
                canvas.FillRectangle(pistaInicioTela, telaChaoY - 2f, pistaComprimento, 8f);

                // Faixas brancas da pista
                canvas.FillColor = Colors.White;
                for (float fx = pistaInicioTela + 20f; fx < pistaInicioTela + pistaComprimento; fx += 50f)
                {
                    if (fx > 0 && fx < largura)
                    {
                        canvas.FillRectangle(fx, telaChaoY + 1f, 25f, 3f);
                    }
                }
            }
        }
    }

    private void DesenharGradeMetrica(ICanvas canvas, float largura, float altura, float telaChaoY, float cameraZ)
    {
        canvas.StrokeColor = Colors.White.WithAlpha(0.25f);
        canvas.StrokeSize = 1f;
        canvas.FontColor = Colors.White.WithAlpha(0.7f);
        canvas.FontSize = 12f;

        // Marcadores verticais de distância a cada 50 metros
        var primeiroMarcoZ = MathF.Floor((cameraZ - 50f) / 50f) * 50f;
        for (float z = primeiroMarcoZ; z < cameraZ + (largura / PIXELS_POR_METRO) + 50f; z += 50f)
        {
            if (z < 0) continue;

            var telaX = (z - cameraZ) * PIXELS_POR_METRO + (largura * 0.25f);
            if (telaX >= -50f && telaX <= largura + 50f)
            {
                canvas.DrawLine(telaX, telaChaoY - 300f, telaX, telaChaoY);
                canvas.DrawString($"{z:F0}m", telaX + 4f, telaChaoY - 8f, HorizontalAlignment.Left);
            }
        }

        // Marcador do recorde histórico
        if (RecordeDistancia > 0)
        {
            var telaRecordeX = (RecordeDistancia - cameraZ) * PIXELS_POR_METRO + (largura * 0.25f);
            if (telaRecordeX >= 0 && telaRecordeX <= largura)
            {
                canvas.StrokeColor = Colors.Gold;
                canvas.StrokeSize = 2f;
                canvas.DrawLine(telaRecordeX, 0, telaRecordeX, telaChaoY);
                canvas.FontColor = Colors.Gold;
                canvas.DrawString($"🏆 RECORDE: {RecordeDistancia:F0}m", telaRecordeX + 6f, 30f, HorizontalAlignment.Left);
            }
        }
    }

    private void DesenharCatapulta(ICanvas canvas, float telaAviaoX, float telaChaoY, float cameraZ)
    {
        var catapultaTelaX = (0f - cameraZ) * PIXELS_POR_METRO + telaAviaoX;

        // Estrutura de madeira/metal da catapulta
        canvas.StrokeColor = Color.FromArgb("#8D6E63");
        canvas.StrokeSize = 6f;
        canvas.DrawLine(catapultaTelaX - 25f, telaChaoY, catapultaTelaX + 15f, telaChaoY - 20f);
        canvas.DrawLine(catapultaTelaX, telaChaoY, catapultaTelaX + 15f, telaChaoY - 20f);

        // Suporte de impulsão
        canvas.FillColor = Color.FromArgb("#D32F2F");
        canvas.FillCircle(catapultaTelaX + 15f, telaChaoY - 20f, 6f);
    }

    private void DesenharColetaveis(ICanvas canvas, float telaAviaoX, float telaChaoY, float cameraZ)
    {
        if (ColetaveisAtivos == null) return;

        for (int i = 0; i < ColetaveisAtivos.Count; i++)
        {
            var item = ColetaveisAtivos[i];
            if (!item.Ativo || item.Coletado) continue;

            var telaX = (item.Posicao.Z - cameraZ) * PIXELS_POR_METRO + telaAviaoX;
            var telaY = telaChaoY - (item.Posicao.Y * PIXELS_POR_METRO);

            if (item.Tipo == TipoColetavel.Moeda)
            {
                // Moeda dourada brilhante
                canvas.FillColor = Colors.Gold;
                canvas.FillCircle(telaX, telaY, 10f);
                canvas.StrokeColor = Colors.Orange;
                canvas.StrokeSize = 2f;
                canvas.DrawCircle(telaX, telaY, 10f);

                // Cifrão estilizado
                canvas.FontColor = Colors.DarkGoldenrod;
                canvas.FontSize = 10f;
                canvas.DrawString("$", telaX, telaY + 4f, HorizontalAlignment.Center);
            }
            else if (item.Tipo == TipoColetavel.AnelVento)
            {
                // Anel de aceleração aerodinâmica (Wind Ring)
                canvas.StrokeColor = Colors.Cyan;
                canvas.StrokeSize = 4f;
                canvas.DrawEllipse(telaX, telaY, 12f, 28f);

                canvas.StrokeColor = Colors.White.WithAlpha(0.8f);
                canvas.StrokeSize = 2f;
                canvas.DrawLine(telaX - 6f, telaY, telaX + 6f, telaY);
                canvas.DrawLine(telaX + 2f, telaY - 4f, telaX + 6f, telaY);
                canvas.DrawLine(telaX + 2f, telaY + 4f, telaX + 6f, telaY);
            }
        }
    }

    private void DesenharParticulas(ICanvas canvas, float telaAviaoX, float telaChaoY, float cameraZ)
    {
        if (GerenciadorParticulas == null) return;

        var particulas = GerenciadorParticulas.ObterParticulas();
        for (int i = 0; i < particulas.Length; i++)
        {
            ref readonly var p = ref particulas[i];
            if (!p.Ativa) continue;

            var telaX = (p.PosicaoX - cameraZ) * PIXELS_POR_METRO + telaAviaoX;
            var telaY = telaChaoY - (p.PosicaoY * PIXELS_POR_METRO);

            var alfa = Math.Clamp(p.VidaRestante / p.VidaTotal, 0f, 1f);
            canvas.FillColor = p.Cor.WithAlpha(alfa);
            canvas.FillCircle(telaX, telaY, p.Tamanho * alfa);
        }
    }

    private void DesenharAeronave(ICanvas canvas, float telaX, float telaY, float pitchGraus)
    {
        canvas.SaveState();

        // Translação e rotação em torno do centro da aeronave
        canvas.Translate(telaX, telaY);
        // Em gráficos de tela Y cresce para baixo, então pitch positivo (nariz para cima) inclina no sentido anti-horário (-pitch)
        canvas.Rotate(-pitchGraus);

        // 1. Chamas do motor traseiro se estiver ativo
        if (EstadoAeronave.Propulsor.EstaAtivo)
        {
            var flamePath = new PathF();
            flamePath.MoveTo(-18f, -4f);
            flamePath.LineTo(-35f, 0f);
            flamePath.LineTo(-18f, 4f);
            flamePath.Close();

            canvas.FillColor = Colors.OrangeRed;
            canvas.FillPath(flamePath);

            var innerFlame = new PathF();
            innerFlame.MoveTo(-18f, -2f);
            innerFlame.LineTo(-26f, 0f);
            innerFlame.LineTo(-18f, 2f);
            innerFlame.Close();

            canvas.FillColor = Colors.Yellow;
            canvas.FillPath(innerFlame);
        }

        // 2. Fuselagem do avião (estilo planador supersônico ágil)
        var fuselagem = new PathF();
        fuselagem.MoveTo(24f, 0f);       // Nariz
        fuselagem.LineTo(4f, -6f);      // Topo frontal
        fuselagem.LineTo(-18f, -4f);    // Traseira superior
        fuselagem.LineTo(-18f, 4f);     // Bico do motor
        fuselagem.LineTo(4f, 6f);       // Barriga
        fuselagem.Close();

        canvas.FillColor = Color.FromArgb("#E53935"); // Vermelho aeronáutico vibrante
        canvas.FillPath(fuselagem);

        // 3. Asa principal
        var asa = new PathF();
        asa.MoveTo(2f, 0f);
        asa.LineTo(-12f, -18f); // Ponta da asa superior
        asa.LineTo(-16f, -16f);
        asa.LineTo(-8f, 0f);
        asa.Close();

        canvas.FillColor = Color.FromArgb("#D32F2F"); // Asa com tom contrastante
        canvas.FillPath(asa);

        // 4. Leme / Cauda vertical
        var leme = new PathF();
        leme.MoveTo(-12f, -4f);
        leme.LineTo(-20f, -14f);
        leme.LineTo(-23f, -12f);
        leme.LineTo(-18f, -4f);
        leme.Close();

        canvas.FillColor = Color.FromArgb("#B71C1C");
        canvas.FillPath(leme);

        // 5. Vidro do Cockpit (Canopy azul com brilho)
        var cockpit = new PathF();
        cockpit.MoveTo(16f, -1f);
        cockpit.LineTo(6f, -5f);
        cockpit.LineTo(-2f, -4f);
        cockpit.LineTo(6f, -1f);
        cockpit.Close();

        canvas.FillColor = Colors.LightCyan.WithAlpha(0.9f);
        canvas.FillPath(cockpit);

        canvas.RestoreState();
    }
}
