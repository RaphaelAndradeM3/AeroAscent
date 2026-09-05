namespace AeroAscent.Core.Dominio.ObjetosDeValor;

/// <summary>
/// Representa um vetor tridimensional imutável alocado exclusivamente na stack (readonly record struct),
/// garantindo alta performance cinemática e zero alocação de memória no heap (GC Alloc = 0 bytes).
/// </summary>
public readonly record struct VetorVoo(float X, float Y, float Z)
{
    /// <summary>
    /// Vetor com todas as componentes zeradas (0, 0, 0).
    /// </summary>
    public static readonly VetorVoo Zero = new(0f, 0f, 0f);

    /// <summary>
    /// Vetor unitário apontando para cima no eixo Y (0, 1, 0).
    /// </summary>
    public static readonly VetorVoo ParaCima = new(0f, 1f, 0f);

    /// <summary>
    /// Vetor unitário apontando para frente no eixo Z (0, 0, 1).
    /// </summary>
    public static readonly VetorVoo ParaFrente = new(0f, 0f, 1f);

    /// <summary>
    /// Vetor unitário apontando para a direita no eixo X (1, 0, 0).
    /// </summary>
    public static readonly VetorVoo ParaDireita = new(1f, 0f, 0f);

    /// <summary>
    /// Adiciona outro vetor componente a componente.
    /// </summary>
    /// <param name="outro">Vetor a somar.</param>
    /// <returns>Novo vetor resultante da soma.</returns>
    public VetorVoo Somar(VetorVoo outro) => new(X + outro.X, Y + outro.Y, Z + outro.Z);

    /// <summary>
    /// Subtrai outro vetor componente a componente.
    /// </summary>
    /// <param name="outro">Vetor a subtrair.</param>
    /// <returns>Novo vetor resultante da subtração.</returns>
    public VetorVoo Subtrair(VetorVoo outro) => new(X - outro.X, Y - outro.Y, Z - outro.Z);

    /// <summary>
    /// Multiplica todas as componentes por um fator escalar.
    /// </summary>
    /// <param name="escalar">Fator multiplicador.</param>
    /// <returns>Novo vetor com componentes multiplicadas.</returns>
    public VetorVoo Multiplicar(float escalar) => new(X * escalar, Y * escalar, Z * escalar);

    /// <summary>
    /// Divide todas as componentes por um divisor escalar. Retorna Zero caso o divisor seja 0.
    /// </summary>
    /// <param name="divisor">Divisor escalar.</param>
    /// <returns>Novo vetor resultante ou VetorVoo.Zero se divisão por zero.</returns>
    public VetorVoo Dividir(float divisor)
    {
        if (divisor == 0f)
        {
            return Zero;
        }

        return new VetorVoo(X / divisor, Y / divisor, Z / divisor);
    }

    /// <summary>
    /// Calcula a magnitude (norma Euclidiana) do vetor.
    /// </summary>
    /// <returns>O comprimento escalar do vetor.</returns>
    public float Magnitude() => MathF.Sqrt(X * X + Y * Y + Z * Z);

    /// <summary>
    /// Calcula a magnitude ao quadrado, evitando a raiz quadrada para checagens de desempenho.
    /// </summary>
    /// <returns>Magnitude elevada ao quadrado.</returns>
    public float MagnitudeAoQuadrado() => X * X + Y * Y + Z * Z;

    /// <summary>
    /// Retorna um novo vetor unitário na mesma direção (magnitude = 1.0) ou VetorVoo.Zero caso magnitude seja 0.
    /// </summary>
    /// <returns>Vetor normalizado.</returns>
    public VetorVoo Normalizar()
    {
        var mag = Magnitude();
        return mag > 0f ? Dividir(mag) : Zero;
    }

    /// <summary>
    /// Operador de soma entre dois vetores.
    /// </summary>
    public static VetorVoo operator +(VetorVoo a, VetorVoo b) => a.Somar(b);

    /// <summary>
    /// Operador de subtração entre dois vetores.
    /// </summary>
    public static VetorVoo operator -(VetorVoo a, VetorVoo b) => a.Subtrair(b);

    /// <summary>
    /// Operador de multiplicação vetor por escalar.
    /// </summary>
    public static VetorVoo operator *(VetorVoo v, float escalar) => v.Multiplicar(escalar);

    /// <summary>
    /// Operador de multiplicação escalar por vetor.
    /// </summary>
    public static VetorVoo operator *(float escalar, VetorVoo v) => v.Multiplicar(escalar);

    /// <summary>
    /// Operador de divisão vetor por escalar.
    /// </summary>
    public static VetorVoo operator /(VetorVoo v, float divisor) => v.Dividir(divisor);

    /// <inheritdoc />
    public override string ToString() => $"({X:F2}, {Y:F2}, {Z:F2})";
}
