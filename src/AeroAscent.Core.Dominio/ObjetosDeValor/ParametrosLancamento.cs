namespace AeroAscent.Core.Dominio.ObjetosDeValor;

using System;
using AeroAscent.Core.Dominio.Excecoes;

/// <summary>
/// Representa os parâmetros de entrada fornecidos pelo jogador no disparo da catapulta,
/// alocado exclusivamente na stack como readonly record struct (GC Alloc = 0 bytes).
/// Aplica de forma incondicional o piso mínimo protetivo de 10% (0.10f) contra frustração de timing.
/// </summary>
public readonly record struct ParametrosLancamento
{
    /// <summary>
    /// Piso protetivo mínimo de precisão (10%), assegurando que erros totais de timing ainda gerem impulso.
    /// </summary>
    public const float PISO_MINIMO_PRECISAO = 0.10f;

    /// <summary>
    /// Ângulo mínimo permitido de inclinação da rampa em graus (15.0°).
    /// </summary>
    public const float ANGULO_MINIMO_GRAUS = 15.0f;

    /// <summary>
    /// Ângulo máximo permitido de inclinação da rampa em graus (60.0°).
    /// </summary>
    public const float ANGULO_MAXIMO_GRAUS = 60.0f;

    /// <summary>
    /// Ângulo padrão de lançamento da rampa de catapulta em graus (35.0°).
    /// </summary>
    public const float ANGULO_PADRAO_GRAUS = 35.0f;

    /// <summary>
    /// Valor bruto de precisão informado pelo jogador no instante do toque (0.0 a 1.0).
    /// </summary>
    public float PrecisaoOriginal { get; }

    /// <summary>
    /// Ângulo de lançamento da rampa em graus em relação ao horizonte.
    /// </summary>
    public float AnguloGraus { get; }

    /// <summary>
    /// Precisão efetiva normalizada após aplicação do piso mínimo protetivo [0.10, 1.0].
    /// </summary>
    public float PrecisaoEfetiva { get; }

    /// <summary>
    /// Inicializa uma nova instância de parâmetros de lançamento validando as invariantes físicas.
    /// </summary>
    /// <param name="precisaoOriginal">Precisão bruta fornecida pelo timing (0.0 a 1.0).</param>
    /// <param name="anguloGraus">Ângulo da rampa em graus (entre 15.0° e 60.0°, padrão 35.0°).</param>
    /// <exception cref="DominioInvalidoException">Lançada caso o ângulo esteja fora dos limites físicos.</exception>
    public ParametrosLancamento(float precisaoOriginal, float anguloGraus = ANGULO_PADRAO_GRAUS)
    {
        if (anguloGraus < ANGULO_MINIMO_GRAUS || anguloGraus > ANGULO_MAXIMO_GRAUS)
        {
            throw new DominioInvalidoException(
                $"O ângulo de lançamento ({anguloGraus}°) deve estar entre {ANGULO_MINIMO_GRAUS}° e {ANGULO_MAXIMO_GRAUS}°.",
                nameof(anguloGraus));
        }

        PrecisaoOriginal = precisaoOriginal;
        AnguloGraus = anguloGraus;
        PrecisaoEfetiva = Math.Max(PISO_MINIMO_PRECISAO, Math.Min(1.0f, precisaoOriginal));
    }

    /// <summary>
    /// Cria uma instância de parâmetros de lançamento aplicando a proteção do piso mínimo.
    /// </summary>
    /// <param name="precisao">Precisão instantânea obtida na barra oscilante.</param>
    /// <param name="anguloGraus">Ângulo de lançamento em graus (padrão 35.0°).</param>
    /// <returns>Estrutura imutável de parâmetros de lançamento.</returns>
    public static ParametrosLancamento Criar(float precisao, float anguloGraus = ANGULO_PADRAO_GRAUS) =>
        new(precisao, anguloGraus);
}
