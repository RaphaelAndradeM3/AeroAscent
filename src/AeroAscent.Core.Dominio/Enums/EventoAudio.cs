namespace AeroAscent.Core.Dominio.Enums;

/// <summary>
/// Catálogo tipado dos eventos sonoros discretos e gatilhos acústicos de AeroAscent.
/// </summary>
public enum EventoAudio
{
    /// <summary>
    /// Disparo da aeronave na rampa de lançamento inicial.
    /// </summary>
    LancamentoCatapulta = 1,

    /// <summary>
    /// Efeito sonoro de vento aerodinâmico durante o voo livre.
    /// </summary>
    VooVento = 2,

    /// <summary>
    /// Acionamento do propulsor de aceleração (boost) da aeronave.
    /// </summary>
    PropulsorBoost = 3,

    /// <summary>
    /// Coleta de moeda dourada no ar.
    /// </summary>
    ColetaMoeda = 4,

    /// <summary>
    /// Transposição bem-sucedida de um anel acelerador de vento.
    /// </summary>
    PassagemAnelVento = 5,

    /// <summary>
    /// Toque suave da aeronave no solo durante o pouso seguro.
    /// </summary>
    PousoSuave = 6,

    /// <summary>
    /// Celebração comemorativa de superação de novo recorde pessoal.
    /// </summary>
    NovoRecorde = 7,

    /// <summary>
    /// Feedback tátil/sonoro de interação com botões de interface.
    /// </summary>
    CliqueBotao = 8,

    /// <summary>
    /// Confirmação de aquisição de melhoria ou item na oficina mecânica.
    /// </summary>
    CompraOficina = 9,

    /// <summary>
    /// Impacto ou perda abrupta de sustentação ao colidir no solo.
    /// </summary>
    ColisaoSolo = 10
}
