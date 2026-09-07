namespace AeroAscent.Apresentacao.MAUI.Servicos;

using System;
using AeroAscent.Core.Aplicacao.Contratos;
using AeroAscent.Core.Dominio.ObjetosDeValor;
using Microsoft.Maui.Graphics;

/// <summary>
/// Partícula visual 2D alocada estaticamente para renderização sem GC Alloc.
/// </summary>
public struct ParticulaVisual2D
{
    public bool Ativa;
    public float PosicaoX;
    public float PosicaoY;
    public float VelocidadeX;
    public float VelocidadeY;
    public float VidaRestante;
    public float VidaTotal;
    public Color Cor;
    public float Tamanho;
}

/// <summary>
/// Gerenciador de partículas 2D para .NET MAUI com Object Pooling estrito (Zero GC Alloc).
/// </summary>
public sealed class GerenciadorParticulasMAUI : IGerenciadorParticulas
{
    private const int MAX_PARTICULAS = 200;
    private readonly ParticulaVisual2D[] _pool = new ParticulaVisual2D[MAX_PARTICULAS];
    private readonly Random _random = new();

    private bool _rastroCaudaAtivo;
    private float _intensidadeRastro;
    private bool _propulsaoAtiva;
    private float _intensidadePropulsao;

    /// <summary>
    /// Pool de partículas ativas acessível pelo renderizador 2D.
    /// </summary>
    public ReadOnlySpan<ParticulaVisual2D> ObterParticulas() => _pool;

    /// <inheritdoc />
    public void DefinirRastroCauda(bool ativo, float intensidade)
    {
        _rastroCaudaAtivo = ativo;
        _intensidadeRastro = Math.Clamp(intensidade, 0f, 1f);
    }

    /// <inheritdoc />
    public void DefinirPropulsao(bool ativo, float intensidade)
    {
        _propulsaoAtiva = ativo;
        _intensidadePropulsao = Math.Clamp(intensidade, 0f, 1f);
    }

    /// <summary>
    /// Emite partícula contínua de rastro ou propulsão na posição atual da aeronave.
    /// </summary>
    public void EmitirRastroAeronave(float x, float y, float anguloGraus)
    {
        if (_propulsaoAtiva)
        {
            var rad = anguloGraus * (MathF.PI / 180f);
            var offsetTraseiroX = -MathF.Cos(rad) * 1.5f;
            var offsetTraseiroY = -MathF.Sin(rad) * 1.5f;

            AlocarParticula(
                x + offsetTraseiroX,
                y + offsetTraseiroY,
                -MathF.Cos(rad) * 8f + ((float)_random.NextDouble() - 0.5f) * 2f,
                -MathF.Sin(rad) * 8f + ((float)_random.NextDouble() - 0.5f) * 2f,
                0.4f,
                Colors.OrangeRed,
                6f);
        }
        else if (_rastroCaudaAtivo)
        {
            var rad = anguloGraus * (MathF.PI / 180f);
            var offsetTraseiroX = -MathF.Cos(rad) * 1.2f;
            var offsetTraseiroY = -MathF.Sin(rad) * 1.2f;

            AlocarParticula(
                x + offsetTraseiroX,
                y + offsetTraseiroY,
                ((float)_random.NextDouble() - 0.5f) * 1f,
                ((float)_random.NextDouble() - 0.5f) * 1f,
                0.3f,
                Colors.WhiteSmoke.WithAlpha(0.6f),
                4f);
        }
    }

    /// <inheritdoc />
    public void EmitirColetaMoeda(VetorVoo posicao)
    {
        for (int i = 0; i < 10; i++)
        {
            var vx = ((float)_random.NextDouble() - 0.5f) * 8f;
            var vy = ((float)_random.NextDouble() - 0.5f) * 8f;
            AlocarParticula(posicao.X, posicao.Y, vx, vy, 0.5f, Colors.Gold, 5f);
        }
    }

    /// <inheritdoc />
    public void EmitirColetaCombustivel(VetorVoo posicao)
    {
        for (int i = 0; i < 8; i++)
        {
            var vx = ((float)_random.NextDouble() - 0.5f) * 6f;
            var vy = ((float)_random.NextDouble() - 0.5f) * 6f;
            AlocarParticula(posicao.X, posicao.Y, vx, vy, 0.5f, Colors.DeepSkyBlue, 5f);
        }
    }

    /// <inheritdoc />
    public void EmitirCelebracaoRecorde(VetorVoo posicao)
    {
        Color[] coresConfete = [Colors.Gold, Colors.Magenta, Colors.Cyan, Colors.LimeGreen, Colors.Orange];
        for (int i = 0; i < 40; i++)
        {
            var vx = ((float)_random.NextDouble() - 0.5f) * 15f;
            var vy = (float)_random.NextDouble() * 15f;
            var cor = coresConfete[_random.Next(coresConfete.Length)];
            AlocarParticula(posicao.X, posicao.Y, vx, vy, 1.2f, cor, 6f);
        }
    }

    /// <inheritdoc />
    public void EmitirImpacto(VetorVoo posicao)
    {
        for (int i = 0; i < 15; i++)
        {
            var vx = ((float)_random.NextDouble() - 0.5f) * 10f;
            var vy = (float)_random.NextDouble() * 6f;
            AlocarParticula(posicao.X, posicao.Y, vx, vy, 0.6f, Colors.SaddleBrown.WithAlpha(0.7f), 7f);
        }
    }

    /// <summary>
    /// Atualiza as posições e o ciclo de vida das partículas a cada passo de simulação.
    /// </summary>
    public void Atualizar(float deltaSegundos)
    {
        for (int i = 0; i < _pool.Length; i++)
        {
            if (!_pool[i].Ativa) continue;

            _pool[i].VidaRestante -= deltaSegundos;
            if (_pool[i].VidaRestante <= 0f)
            {
                _pool[i].Ativa = false;
                continue;
            }

            _pool[i].PosicaoX += _pool[i].VelocidadeX * deltaSegundos;
            _pool[i].PosicaoY += _pool[i].VelocidadeY * deltaSegundos;
        }
    }

    /// <inheritdoc />
    public void PararTodosOsEfeitos()
    {
        for (int i = 0; i < _pool.Length; i++)
        {
            _pool[i].Ativa = false;
        }
        _rastroCaudaAtivo = false;
        _propulsaoAtiva = false;
    }

    private void AlocarParticula(float x, float y, float vx, float vy, float vida, Color cor, float tamanho)
    {
        for (int i = 0; i < _pool.Length; i++)
        {
            if (!_pool[i].Ativa)
            {
                _pool[i].Ativa = true;
                _pool[i].PosicaoX = x;
                _pool[i].PosicaoY = y;
                _pool[i].VelocidadeX = vx;
                _pool[i].VelocidadeY = vy;
                _pool[i].VidaRestante = vida;
                _pool[i].VidaTotal = vida;
                _pool[i].Cor = cor;
                _pool[i].Tamanho = tamanho;
                return;
            }
        }
    }
}
