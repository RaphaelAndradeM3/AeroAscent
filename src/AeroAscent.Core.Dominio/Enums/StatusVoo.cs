namespace AeroAscent.Core.Dominio.Enums;

/// <summary>
/// Representa o ciclo de vida e estado operacional de uma sessão de voo da aeronave.
/// </summary>
public enum StatusVoo
{
    /// <summary>
    /// Voo em fase de preparação na catapulta aguardando o disparo do jogador.
    /// </summary>
    EmPreparacao = 0,

    /// <summary>
    /// Aeronave em voo ativo na atmosfera, sujeita às forças da física e comandos de inclinação/boost.
    /// </summary>
    EmVoo = 1,

    /// <summary>
    /// Voo finalizado por pouso e parada completa da aeronave no solo.
    /// </summary>
    Pousado = 2,

    /// <summary>
    /// Voo cancelado ou abortado pelo jogador antes do pouso normal.
    /// </summary>
    Cancelado = 3
}
