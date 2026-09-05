namespace AeroAscent.Core.Dominio.ObjetosDeValor;

using System;
using AeroAscent.Core.Dominio.Excecoes;

/// <summary>
/// Representa os comandos de arfagem e controle do piloto transmitidos à aeronave a cada passo de tempo,
/// alocado exclusivamente na stack como readonly record struct (GC Alloc = 0 bytes).
/// </summary>
public readonly record struct ParametrosControlePiloto
{
    /// <summary>
    /// Taxa de rotação angular padrão em graus por segundo (45°/s).
    /// </summary>
    public const float TAXA_ANGULAR_PADRAO = 45.0f;

    /// <summary>
    /// Limiar de zona morta (deadzone) abaixo do qual os comandos são considerados neutros (0.05).
    /// </summary>
    public const float ZONA_MORTA_INPUT = 0.05f;

    /// <summary>
    /// Limite inferior normalizado de intensidade de pitch (-1.0 para mergulho máximo).
    /// </summary>
    public const float INTENSIDADE_MINIMA = -1.0f;

    /// <summary>
    /// Limite superior normalizado de intensidade de pitch (+1.0 para subida máxima).
    /// </summary>
    public const float INTENSIDADE_MAXIMA = 1.0f;

    /// <summary>
    /// Instância estática constante com comandos neutros e autoestabilização ativada.
    /// </summary>
    public static readonly ParametrosControlePiloto Neutro = new(0f, TAXA_ANGULAR_PADRAO);

    /// <summary>
    /// Intensidade normalizada do comando de arfagem (-1.0 a +1.0).
    /// Valores negativos representam comando de mergulho (nariz para baixo);
    /// valores positivos representam comando de subida (nariz para cima).
    /// </summary>
    public float IntensidadePitch { get; }

    /// <summary>
    /// Taxa máxima de variação angular em graus por segundo comandada pelo piloto.
    /// </summary>
    public float TaxaVariacaoAngularGrausPorSegundo { get; }

    /// <summary>
    /// Indica se o jogador está aplicando um comando intencional fora da zona morta de estabilização.
    /// </summary>
    public bool TemComandoAtivo => MathF.Abs(IntensidadePitch) >= ZONA_MORTA_INPUT;

    /// <summary>
    /// Construtor interno para instanciação estruturada.
    /// </summary>
    /// <param name="intensidadePitch">Intensidade normalizada clamped.</param>
    /// <param name="taxaVariacaoAngular">Taxa angular em graus por segundo.</param>
    public ParametrosControlePiloto(float intensidadePitch, float taxaVariacaoAngular)
    {
        IntensidadePitch = Math.Clamp(intensidadePitch, INTENSIDADE_MINIMA, INTENSIDADE_MAXIMA);
        TaxaVariacaoAngularGrausPorSegundo = taxaVariacaoAngular;
    }

    /// <summary>
    /// Cria uma nova instância validada de parâmetros de controle do piloto.
    /// </summary>
    /// <param name="intensidadePitch">Intensidade de arfagem desejada (-1.0 a +1.0).</param>
    /// <param name="taxaVariacaoAngular">Taxa angular em graus por segundo (deve ser positiva, padrão 45°/s).</param>
    /// <returns>Nova instância imutável de ParametrosControlePiloto.</returns>
    /// <exception cref="DominioInvalidoException">Lançada caso a taxa angular seja menor ou igual a zero.</exception>
    public static ParametrosControlePiloto Criar(float intensidadePitch, float taxaVariacaoAngular = TAXA_ANGULAR_PADRAO)
    {
        if (taxaVariacaoAngular <= 0f)
        {
            throw new DominioInvalidoException(
                nameof(taxaVariacaoAngular),
                $"A taxa de variação angular deve ser estritamente positiva. Valor informado: {taxaVariacaoAngular}.");
        }

        return new ParametrosControlePiloto(intensidadePitch, taxaVariacaoAngular);
    }
}
