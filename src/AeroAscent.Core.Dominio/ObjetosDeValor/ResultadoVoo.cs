namespace AeroAscent.Core.Dominio.ObjetosDeValor;

/// <summary>
/// Objeto de valor imutável consolidando as métricas e premiações ao final de um voo.
/// </summary>
public record ResultadoVoo(float DistanciaMetros, float AltitudeMaximaMetros, int MoedasColetadas, Moeda MoedasRecompensaTotal);
