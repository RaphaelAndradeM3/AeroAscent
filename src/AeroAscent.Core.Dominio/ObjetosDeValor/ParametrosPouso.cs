namespace AeroAscent.Core.Dominio.ObjetosDeValor;

using AeroAscent.Core.Dominio.Excecoes;

/// <summary>
/// Encapsula os parâmetros físicos e limiares de desaceleração durante o contato com o solo e pouso,
/// alocado exclusivamente na stack como readonly record struct (GC Alloc = 0 bytes).
/// </summary>
public readonly record struct ParametrosPouso
{
    /// <summary>
    /// Coeficiente de atrito cinético de deslizamento padrão com o solo (μ = 0.3).
    /// </summary>
    public const float COEFICIENTE_ATRITO_PADRAO = 0.3f;

    /// <summary>
    /// Limiar canônico de velocidade longitudinal abaixo do qual a aeronave para totalmente (0.15 m/s).
    /// </summary>
    public const float VELOCIDADE_LIMIAR_PARADA_PADRAO = 0.15f;

    /// <summary>
    /// Taxa de nivelamento da atitude de arfagem (pitch) em graus por segundo ao deslizar no solo (15.0°/s).
    /// </summary>
    public const float TAXA_NIVELAMENTO_PITCH_PADRAO = 15.0f;

    /// <summary>
    /// Coeficiente de atrito cinético de deslizamento do solo (μ).
    /// </summary>
    public float CoeficienteAtritoSolo { get; }

    /// <summary>
    /// Limiar inferior de velocidade horizontal para congelamento no repouso absoluto.
    /// </summary>
    public float VelocidadeLimiarParada { get; }

    /// <summary>
    /// Velocidade angular de restauração horizontal do nariz da aeronave durante o deslize.
    /// </summary>
    public float TaxaNivelamentoPitchGrausPorSegundo { get; }

    /// <summary>
    /// Construtor estruturado dos parâmetros de pouso com validação de limites físicos.
    /// </summary>
    /// <param name="coeficienteAtritoSolo">Coeficiente de atrito cinético (deve ser positivo).</param>
    /// <param name="velocidadeLimiarParada">Limiar de parada em metros por segundo (deve ser positivo).</param>
    /// <param name="taxaNivelamentoPitchGrausPorSegundo">Taxa de nivelamento em graus por segundo (deve ser positiva).</param>
    /// <exception cref="DominioInvalidoException">Lançada caso algum parâmetro viole invariantes físicas.</exception>
    public ParametrosPouso(
        float coeficienteAtritoSolo,
        float velocidadeLimiarParada,
        float taxaNivelamentoPitchGrausPorSegundo)
    {
        if (coeficienteAtritoSolo <= 0f)
        {
            throw new DominioInvalidoException(
                nameof(coeficienteAtritoSolo),
                $"O coeficiente de atrito do solo deve ser positivo. Informado: {coeficienteAtritoSolo}.");
        }

        if (velocidadeLimiarParada <= 0f)
        {
            throw new DominioInvalidoException(
                nameof(velocidadeLimiarParada),
                $"O limiar de velocidade de parada deve ser positivo. Informado: {velocidadeLimiarParada}.");
        }

        if (taxaNivelamentoPitchGrausPorSegundo <= 0f)
        {
            throw new DominioInvalidoException(
                nameof(taxaNivelamentoPitchGrausPorSegundo),
                $"A taxa de nivelamento de pitch deve ser positiva. Informado: {taxaNivelamentoPitchGrausPorSegundo}.");
        }

        CoeficienteAtritoSolo = coeficienteAtritoSolo;
        VelocidadeLimiarParada = velocidadeLimiarParada;
        TaxaNivelamentoPitchGrausPorSegundo = taxaNivelamentoPitchGrausPorSegundo;
    }

    /// <summary>
    /// Cria uma instância com os valores físicos calibrados padrão para o solo.
    /// </summary>
    /// <returns>Nova instância de ParametrosPouso.</returns>
    public static ParametrosPouso CriarPadrao()
    {
        return new ParametrosPouso(
            COEFICIENTE_ATRITO_PADRAO,
            VELOCIDADE_LIMIAR_PARADA_PADRAO,
            TAXA_NIVELAMENTO_PITCH_PADRAO);
    }
}
