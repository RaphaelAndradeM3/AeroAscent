namespace AeroAscent.Core.Dominio.ObjetosDeValor;

/// <summary>
/// Representa um vetor tridimensional imutável alocado na stack para cálculos físicos e cinemáticos com zero alocação de heap (0 bytes GC).
/// </summary>
public readonly record struct VetorVoo(float X, float Y, float Z)
{
    /// <summary>
    /// Vetor com todas as componentes zeradas.
    /// </summary>
    public static readonly VetorVoo Zero = new(0f, 0f, 0f);
}
