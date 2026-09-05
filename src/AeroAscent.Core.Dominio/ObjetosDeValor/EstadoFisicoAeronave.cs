namespace AeroAscent.Core.Dominio.ObjetosDeValor;

using System;

/// <summary>
/// Representa o estado cinemático e dinâmico instantâneo da aeronave no espaço tridimensional (plano longitudinal Y-Z),
/// alocado exclusivamente na stack como readonly record struct garantindo GC Alloc = 0 bytes.
/// </summary>
public readonly record struct EstadoFisicoAeronave
{
    /// <summary>
    /// Limite inferior de inclinação do nariz em mergulho (-45.0°).
    /// </summary>
    public const float PITCH_MINIMO_GRAUS = -45.0f;

    /// <summary>
    /// Limite superior de inclinação do nariz em subida (+60.0°).
    /// </summary>
    public const float PITCH_MAXIMO_GRAUS = 60.0f;

    /// <summary>
    /// Posição tridimensional atual da aeronave em metros (X, Y, Z), onde X=0, Y=altitude e Z=avanço horizontal.
    /// </summary>
    public VetorVoo Posicao { get; }

    /// <summary>
    /// Vetor velocidade tridimensional instantâneo da aeronave em metros por segundo.
    /// </summary>
    public VetorVoo Velocidade { get; }

    /// <summary>
    /// Ângulo de arfagem (pitch) do nariz em graus em relação ao horizonte horizontal (-45° a +60°).
    /// </summary>
    public float InclinacaoPitchGraus { get; }

    /// <summary>
    /// Força tridimensional resultante que atuou sobre a aeronave neste passo de tempo em Newtons.
    /// </summary>
    public VetorVoo ForcaResultante { get; }

    /// <summary>
    /// Indica se a aeronave está em contato com o solo (altitude Y &lt;= 0).
    /// </summary>
    public bool NoSolo { get; }

    /// <summary>
    /// Magnitude escalar da velocidade atual da aeronave em metros por segundo.
    /// </summary>
    public float VelocidadeEscalar => Velocidade.Magnitude();

    /// <summary>
    /// Construtor estruturado garantindo a aplicação das invariantes do domínio físico.
    /// </summary>
    /// <param name="posicao">Posição tridimensional.</param>
    /// <param name="velocidade">Vetor de velocidade.</param>
    /// <param name="inclinacaoPitchGraus">Ângulo de pitch em graus.</param>
    /// <param name="forcaResultante">Força resultante atuante.</param>
    /// <param name="noSolo">Indicador de contato com o solo.</param>
    public EstadoFisicoAeronave(
        VetorVoo posicao,
        VetorVoo velocidade,
        float inclinacaoPitchGraus,
        VetorVoo forcaResultante,
        bool noSolo)
    {
        // Invariante: Altitude física não pode ser negativa
        var yEfetivo = MathF.Max(0f, posicao.Y);
        Posicao = new VetorVoo(posicao.X, yEfetivo, posicao.Z);

        // Invariante: Se no solo com velocidade vertical descendente, trava em 0
        var vyEfetivo = (yEfetivo <= 0f && velocidade.Y < 0f) ? 0f : velocidade.Y;
        Velocidade = new VetorVoo(velocidade.X, vyEfetivo, velocidade.Z);

        // Invariante: Pitch clamped nos limites operacionais do jogo arcade
        InclinacaoPitchGraus = Math.Clamp(inclinacaoPitchGraus, PITCH_MINIMO_GRAUS, PITCH_MAXIMO_GRAUS);
        ForcaResultante = forcaResultante;
        NoSolo = (noSolo || yEfetivo <= 0f) && (velocidade.Y <= 0f);
    }

    /// <summary>
    /// Instancia um estado físico inicial para decolagem ou início de trajetória.
    /// </summary>
    /// <param name="posicaoInicial">Posição inicial de voo.</param>
    /// <param name="velocidadeInicial">Velocidade inicial conferida.</param>
    /// <param name="inclinacaoPitchGraus">Ângulo inicial de arfagem em graus.</param>
    /// <returns>Novo EstadoFisicoAeronave alocado na stack.</returns>
    public static EstadoFisicoAeronave CriarInicial(
        VetorVoo posicaoInicial,
        VetorVoo velocidadeInicial,
        float inclinacaoPitchGraus)
    {
        var noSolo = posicaoInicial.Y <= 0f && velocidadeInicial.Y <= 0f;
        return new EstadoFisicoAeronave(
            posicaoInicial,
            velocidadeInicial,
            inclinacaoPitchGraus,
            VetorVoo.Zero,
            noSolo);
    }

    /// <summary>
    /// Instancia um estado físico com todos os parâmetros calculados explicitamente.
    /// </summary>
    public static EstadoFisicoAeronave Criar(
        VetorVoo posicao,
        VetorVoo velocidade,
        float inclinacaoPitchGraus,
        VetorVoo forcaResultante,
        bool noSolo)
    {
        return new EstadoFisicoAeronave(posicao, velocidade, inclinacaoPitchGraus, forcaResultante, noSolo);
    }

    /// <summary>
    /// Retorna uma cópia atualizada com novas posições, velocidades e forças resultantes.
    /// </summary>
    public EstadoFisicoAeronave ComAtualizacao(
        VetorVoo novaPosicao,
        VetorVoo novaVelocidade,
        float novoPitch,
        VetorVoo novaForca,
        bool novoNoSolo)
    {
        return new EstadoFisicoAeronave(novaPosicao, novaVelocidade, novoPitch, novaForca, novoNoSolo);
    }
}
