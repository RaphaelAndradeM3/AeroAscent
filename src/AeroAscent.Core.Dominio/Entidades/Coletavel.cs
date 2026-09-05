namespace AeroAscent.Core.Dominio.Entidades;

using System;
using AeroAscent.Core.Dominio.Enums;
using AeroAscent.Core.Dominio.Excecoes;
using AeroAscent.Core.Dominio.ObjetosDeValor;

/// <summary>
/// Representa uma instância física de coletável flutuante (moeda ou anel de vento) no plano longitudinal Y-Z,
/// projetada para reciclagem contínua via Object Pooling com zero alocação de memória no heap.
/// </summary>
public class Coletavel
{
    /// <summary>
    /// Raio de detecção padrão para moedas em metros (1.5m).
    /// </summary>
    public const float RAIO_PADRAO_MOEDA_METROS = 1.5f;

    /// <summary>
    /// Raio de detecção padrão para anéis de vento em metros (3.5m).
    /// </summary>
    public const float RAIO_PADRAO_ANEL_VENTO_METROS = 3.5f;

    /// <summary>
    /// Identificador único da entidade.
    /// </summary>
    public Guid Id { get; }

    /// <summary>
    /// Tipo de coletável (Moeda ou AnelVento).
    /// </summary>
    public TipoColetavel Tipo { get; }

    /// <summary>
    /// Posição tridimensional atual no plano Y-Z (X=0, Y=altitude, Z=avanço horizontal).
    /// </summary>
    public VetorVoo Posicao { get; private set; }

    /// <summary>
    /// Raio de detecção e alcance para colisão em metros.
    /// </summary>
    public float RaioColetaMetros { get; }

    /// <summary>
    /// Indica se o coletável está ativo e visível na cena do jogo.
    /// </summary>
    public bool Ativo { get; private set; }

    /// <summary>
    /// Indica se o coletável já foi capturado pela aeronave no voo atual.
    /// </summary>
    public bool Coletado { get; private set; }

    /// <summary>
    /// Construtor estruturado da entidade Coletavel.
    /// </summary>
    /// <param name="id">Identificador único.</param>
    /// <param name="tipo">Tipo do coletável.</param>
    /// <param name="posicao">Posição inicial no espaço.</param>
    /// <param name="raioColetaMetros">Raio de colisão em metros.</param>
    /// <exception cref="DominioInvalidoException">Lançada caso os parâmetros violem invariantes físicas.</exception>
    public Coletavel(Guid id, TipoColetavel tipo, VetorVoo posicao, float raioColetaMetros)
    {
        if (id == Guid.Empty)
        {
            throw new DominioInvalidoException(nameof(id), "O identificador do coletável não pode ser vazio.");
        }

        if (raioColetaMetros <= 0f)
        {
            throw new DominioInvalidoException(nameof(raioColetaMetros), $"O raio de coleta deve ser positivo. Informado: {raioColetaMetros}.");
        }

        Id = id;
        Tipo = tipo;
        Posicao = new VetorVoo(0f, MathF.Max(0f, posicao.Y), posicao.Z);
        RaioColetaMetros = raioColetaMetros;
        Ativo = false;
        Coletado = false;
    }

    /// <summary>
    /// Cria uma nova instância de moeda flutuante com raio padrão de 1.5m.
    /// </summary>
    /// <param name="posicao">Posição inicial da moeda.</param>
    /// <returns>Nova instância de Coletavel do tipo Moeda.</returns>
    public static Coletavel CriarMoeda(VetorVoo posicao)
    {
        return new Coletavel(Guid.NewGuid(), TipoColetavel.Moeda, posicao, RAIO_PADRAO_MOEDA_METROS);
    }

    /// <summary>
    /// Cria uma nova instância de anel de vento (Air Boost Ring) com raio padrão de 3.5m.
    /// </summary>
    /// <param name="posicao">Posição inicial do anel.</param>
    /// <returns>Nova instância de Coletavel do tipo AnelVento.</returns>
    public static Coletavel CriarAnelVento(VetorVoo posicao)
    {
        return new Coletavel(Guid.NewGuid(), TipoColetavel.AnelVento, posicao, RAIO_PADRAO_ANEL_VENTO_METROS);
    }

    /// <summary>
    /// Ativa o coletável em uma nova coordenada espacial reposicionando-o via pooling.
    /// </summary>
    /// <param name="novaPosicao">Nova coordenada tridimensional (plano Y-Z).</param>
    public void Ativar(VetorVoo novaPosicao)
    {
        Posicao = new VetorVoo(0f, MathF.Max(0f, novaPosicao.Y), novaPosicao.Z);
        Ativo = true;
        Coletado = false;
    }

    /// <summary>
    /// Desativa o coletável retirando-o de tela para devolução ao pool.
    /// </summary>
    public void Desativar()
    {
        Ativo = false;
    }

    /// <summary>
    /// Registra a captura bem-sucedida do coletável pela aeronave.
    /// </summary>
    public void MarcarColetado()
    {
        Coletado = true;
        Ativo = false;
    }

    /// <summary>
    /// Verifica em O(1) sem raiz quadrada se a aeronave tocou o raio de abrangência do coletável no plano Y-Z.
    /// </summary>
    /// <param name="posicaoAeronave">Posição tridimensional atual da aeronave.</param>
    /// <param name="raioAeronaveMetros">Raio de colisão da aeronave em metros (padrão 0.5m).</param>
    /// <returns>True se houver sobreposição e o coletável estiver ativo e não coletado; caso contrário, False.</returns>
    public bool VerificarColisao(VetorVoo posicaoAeronave, float raioAeronaveMetros = 0.5f)
    {
        if (!Ativo || Coletado)
        {
            return false;
        }

        var dy = posicaoAeronave.Y - Posicao.Y;
        var dz = posicaoAeronave.Z - Posicao.Z;
        var distanciaQuadrada = dy * dy + dz * dz;

        var raioTotal = RaioColetaMetros + raioAeronaveMetros;
        var raioTotalQuadrado = raioTotal * raioTotal;

        return distanciaQuadrada <= raioTotalQuadrado;
    }
}
