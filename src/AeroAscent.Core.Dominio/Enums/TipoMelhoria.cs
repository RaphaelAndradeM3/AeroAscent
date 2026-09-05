namespace AeroAscent.Core.Dominio.Enums;

/// <summary>
/// Especifica as quatro categorias mecânicas de componentes evolutivos da aeronave na oficina.
/// </summary>
public enum TipoMelhoria
{
    /// <summary>
    /// Potência do motor e aceleração do impulso (boost).
    /// </summary>
    Motor = 1,

    /// <summary>
    /// Eficiência de sustentação e redução do coeficiente de arrasto da fuselagem.
    /// </summary>
    Aerodinamica = 2,

    /// <summary>
    /// Volume e capacidade máxima do tanque de combustível para propulsão.
    /// </summary>
    TanqueCombustivel = 3,

    /// <summary>
    /// Impulso e velocidade de ejeção da catapulta no momento do lançamento inicial.
    /// </summary>
    Catapulta = 4
}
