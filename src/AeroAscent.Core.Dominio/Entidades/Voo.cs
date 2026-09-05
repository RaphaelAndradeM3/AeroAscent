namespace AeroAscent.Core.Dominio.Entidades;

using AeroAscent.Core.Dominio.Enums;
using AeroAscent.Core.Dominio.Excecoes;
using AeroAscent.Core.Dominio.ObjetosDeValor;

/// <summary>
/// Entidade que encapsula o ciclo de vida, rastreamento de métricas e máquina de estados de uma sessão de voo ativa.
/// </summary>
public class Voo
{
    /// <summary>
    /// Identificador único global desta sessão de voo.
    /// </summary>
    public Guid Id { get; }

    /// <summary>
    /// Aeronave utilizada durante este voo.
    /// </summary>
    public Aeronave Aeronave { get; }

    /// <summary>
    /// Status operacional atual da sessão de voo.
    /// </summary>
    public StatusVoo Status { get; private set; }

    /// <summary>
    /// Distância horizontal percorrida em metros pelo avião.
    /// </summary>
    public float DistanciaPercorrida { get; private set; }

    /// <summary>
    /// Altitude máxima em metros registrada nesta sessão.
    /// </summary>
    public float AltitudeMaxima { get; private set; }

    /// <summary>
    /// Quantidade de moedas físicas coletadas no ar durante o voo.
    /// </summary>
    public int MoedasColetadas { get; private set; }

    /// <summary>
    /// Objeto de valor gerado no pouso consolidando os bônus e premiações da rodada.
    /// É nulo enquanto o voo não for pousado ou se tiver sido cancelado.
    /// </summary>
    public ResultadoVoo? Resultado { get; private set; }

    /// <summary>
    /// Construtor privado para controle estrito via método de fábrica Iniciar.
    /// </summary>
    private Voo(Guid id, Aeronave aeronave)
    {
        Id = id;
        Aeronave = aeronave;
        Status = StatusVoo.EmPreparacao;
        DistanciaPercorrida = 0f;
        AltitudeMaxima = 0f;
        MoedasColetadas = 0;
        Resultado = null;
    }

    /// <summary>
    /// Inicia uma nova sessão de voo vinculada à aeronave especificada.
    /// </summary>
    /// <param name="aeronave">Aeronave que executará o voo.</param>
    /// <returns>Nova instância da sessão de voo no status EmPreparacao.</returns>
    /// <exception cref="DominioInvalidoException">Lançada se a aeronave informada for nula.</exception>
    public static Voo Iniciar(Aeronave aeronave)
    {
        if (aeronave == null)
        {
            throw new DominioInvalidoException(nameof(aeronave), "A aeronave para iniciar o voo não pode ser nula.");
        }

        return new Voo(Guid.NewGuid(), aeronave);
    }

    /// <summary>
    /// Transita o status de voo de EmPreparacao para EmVoo no momento da decolagem pela catapulta.
    /// </summary>
    /// <exception cref="DominioInvalidoException">Lançada se o voo não estiver em preparação.</exception>
    public void Decolar()
    {
        if (Status != StatusVoo.EmPreparacao)
        {
            throw new DominioInvalidoException(nameof(Status), $"Decolagem não permitida. O voo deve estar em 'EmPreparacao'. Status atual: {Status}.");
        }

        Status = StatusVoo.EmVoo;
    }

    /// <summary>
    /// Atualiza as métricas acumuladas de voo (distância percorrida, maior altitude e moedas coletadas).
    /// </summary>
    /// <param name="distancia">Distância horizontal alcançada até o momento em metros.</param>
    /// <param name="altitudeAtual">Altitude atual do avião em metros.</param>
    /// <param name="moedasNovas">Moedas coletadas neste passo de simulação.</param>
    /// <exception cref="DominioInvalidoException">Lançada se não estiver em voo ativo ou se valores forem negativos.</exception>
    public void AtualizarMetricas(float distancia, float altitudeAtual, int moedasNovas)
    {
        if (Status != StatusVoo.EmVoo)
        {
            throw new DominioInvalidoException(nameof(Status), $"Não é possível atualizar métricas fora do estado 'EmVoo'. Status atual: {Status}.");
        }

        if (distancia < 0f)
        {
            throw new DominioInvalidoException(nameof(distancia), "A distância não pode ser negativa.");
        }

        if (altitudeAtual < 0f)
        {
            throw new DominioInvalidoException(nameof(altitudeAtual), "A altitude não pode ser negativa.");
        }

        if (moedasNovas < 0)
        {
            throw new DominioInvalidoException(nameof(moedasNovas), "Moedas novas não podem ser negativas.");
        }

        DistanciaPercorrida = MathF.Max(DistanciaPercorrida, distancia);
        AltitudeMaxima = MathF.Max(AltitudeMaxima, altitudeAtual);
        MoedasColetadas += moedasNovas;
    }

    /// <summary>
    /// Finaliza a sessão de voo com pouso regular, gerando o ResultadoVoo com a premiação calculada.
    /// </summary>
    /// <returns>O ResultadoVoo imutável gerado.</returns>
    /// <exception cref="DominioInvalidoException">Lançada se a aeronave não estiver em voo ativo.</exception>
    public ResultadoVoo Pousar()
    {
        if (Status != StatusVoo.EmVoo)
        {
            throw new DominioInvalidoException(nameof(Status), $"Apenas voos ativos em 'EmVoo' podem realizar pouso. Status atual: {Status}.");
        }

        Status = StatusVoo.Pousado;
        Resultado = ResultadoVoo.Calcular(DistanciaPercorrida, AltitudeMaxima, MoedasColetadas);
        return Resultado;
    }

    /// <summary>
    /// Cancela ou aborta a sessão de voo antes da conclusão natural.
    /// </summary>
    /// <exception cref="DominioInvalidoException">Lançada se o voo já estiver pousado ou cancelado.</exception>
    public void Cancelar()
    {
        if (Status == StatusVoo.Pousado || Status == StatusVoo.Cancelado)
        {
            throw new DominioInvalidoException(nameof(Status), $"Não é possível cancelar um voo já finalizado ({Status}).");
        }

        Status = StatusVoo.Cancelado;
        Resultado = null;
    }
}
