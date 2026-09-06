namespace AeroAscent.Core.Aplicacao.Testes.Fixtures;

using AeroAscent.Core.Aplicacao.Contratos;
using AeroAscent.Core.Dominio.ObjetosDeValor;

/// <summary>
/// Fixture de teste que simula e espiona as operações de <see cref="IGerenciadorParticulas"/>,
/// permitindo validação isolada de feedback visual sem acoplamento com a engine de partículas da Unity.
/// </summary>
public sealed class GerenciadorParticulasFalso : IGerenciadorParticulas
{
    /// <summary>
    /// Indica se o rastro de cauda está atualmente ativo.
    /// </summary>
    public bool RastroCaudaAtivo { get; private set; }

    /// <summary>
    /// Última intensidade configurada para o rastro de cauda (0.0 a 1.0).
    /// </summary>
    public float IntensidadeRastroCauda { get; private set; }

    /// <summary>
    /// Indica se o efeito de propulsão (boost) está atualmente ativo.
    /// </summary>
    public bool PropulsaoAtiva { get; private set; }

    /// <summary>
    /// Última intensidade configurada para as chamas de propulsão (0.0 a 1.0).
    /// </summary>
    public float IntensidadePropulsao { get; private set; }

    /// <summary>
    /// Quantidade total de emissões de partículas de coleta de moeda registradas.
    /// </summary>
    public int ContagemColetaMoeda { get; private set; }

    /// <summary>
    /// Última posição registrada da emissão de coleta de moeda.
    /// </summary>
    public VetorVoo UltimaPosicaoColetaMoeda { get; private set; }

    /// <summary>
    /// Quantidade total de emissões de partículas de coleta de combustível registradas.
    /// </summary>
    public int ContagemColetaCombustivel { get; private set; }

    /// <summary>
    /// Última posição registrada da emissão de coleta de combustível.
    /// </summary>
    public VetorVoo UltimaPosicaoColetaCombustivel { get; private set; }

    /// <summary>
    /// Quantidade total de celebrações de recorde com confetes disparadas.
    /// </summary>
    public int ContagemCelebracaoRecorde { get; private set; }

    /// <summary>
    /// Última posição registrada da celebração de recorde.
    /// </summary>
    public VetorVoo UltimaPosicaoCelebracaoRecorde { get; private set; }

    /// <summary>
    /// Quantidade total de impactos com o solo registrados.
    /// </summary>
    public int ContagemImpacto { get; private set; }

    /// <summary>
    /// Última posição registrada do impacto com o solo.
    /// </summary>
    public VetorVoo UltimaPosicaoImpacto { get; private set; }

    /// <summary>
    /// Quantidade de vezes que a interrupção geral de efeitos foi acionada.
    /// </summary>
    public int ContagemPararTodosOsEfeitos { get; private set; }

    /// <inheritdoc />
    public void DefinirRastroCauda(bool ativo, float intensidade)
    {
        RastroCaudaAtivo = ativo;
        IntensidadeRastroCauda = Math.Clamp(intensidade, 0f, 1f);
    }

    /// <inheritdoc />
    public void DefinirPropulsao(bool ativo, float intensidade)
    {
        PropulsaoAtiva = ativo;
        IntensidadePropulsao = Math.Clamp(intensidade, 0f, 1f);
    }

    /// <inheritdoc />
    public void EmitirColetaMoeda(VetorVoo posicao)
    {
        ContagemColetaMoeda++;
        UltimaPosicaoColetaMoeda = posicao;
    }

    /// <inheritdoc />
    public void EmitirColetaCombustivel(VetorVoo posicao)
    {
        ContagemColetaCombustivel++;
        UltimaPosicaoColetaCombustivel = posicao;
    }

    /// <inheritdoc />
    public void EmitirCelebracaoRecorde(VetorVoo posicao)
    {
        ContagemCelebracaoRecorde++;
        UltimaPosicaoCelebracaoRecorde = posicao;
    }

    /// <inheritdoc />
    public void EmitirImpacto(VetorVoo posicao)
    {
        ContagemImpacto++;
        UltimaPosicaoImpacto = posicao;
    }

    /// <inheritdoc />
    public void PararTodosOsEfeitos()
    {
        RastroCaudaAtivo = false;
        IntensidadeRastroCauda = 0f;
        PropulsaoAtiva = false;
        IntensidadePropulsao = 0f;
        ContagemPararTodosOsEfeitos++;
    }

    /// <summary>
    /// Redefine todos os contadores e estados para uma execução limpa de teste.
    /// </summary>
    public void Limpar()
    {
        RastroCaudaAtivo = false;
        IntensidadeRastroCauda = 0f;
        PropulsaoAtiva = false;
        IntensidadePropulsao = 0f;
        ContagemColetaMoeda = 0;
        UltimaPosicaoColetaMoeda = VetorVoo.Zero;
        ContagemColetaCombustivel = 0;
        UltimaPosicaoColetaCombustivel = VetorVoo.Zero;
        ContagemCelebracaoRecorde = 0;
        UltimaPosicaoCelebracaoRecorde = VetorVoo.Zero;
        ContagemImpacto = 0;
        UltimaPosicaoImpacto = VetorVoo.Zero;
        ContagemPararTodosOsEfeitos = 0;
    }
}
