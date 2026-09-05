namespace AeroAscent.Core.Dominio.ObjetosDeValor;

/// <summary>
/// Representa o resultado da operação de disparo da aeronave pela catapulta,
/// informando se a decolagem ocorreu com sucesso e fornecendo o vetor de velocidade resultante.
/// </summary>
public record ResultadoLancamento
{
    /// <summary>
    /// Indica se o lançamento foi executado com êxito e o voo decolou.
    /// </summary>
    public bool Sucesso { get; init; }

    /// <summary>
    /// Vetor tridimensional da velocidade inicial aplicada à aeronave (em metros por segundo).
    /// </summary>
    public VetorVoo VelocidadeInicial { get; init; }

    /// <summary>
    /// Mensagem explicativa em caso de recusa ou falha no lançamento (ex: voo já em andamento).
    /// </summary>
    public string? MensagemErro { get; init; }

    /// <summary>
    /// Construtor privado para garantir instanciação exclusivamente através dos métodos de fábrica semânticos.
    /// </summary>
    private ResultadoLancamento()
    {
    }

    /// <summary>
    /// Cria um resultado indicando lançamento bem-sucedido com o vetor de velocidade inicial.
    /// </summary>
    /// <param name="velocidadeInicial">Vetor tridimensional com o impulso gerado pela catapulta.</param>
    /// <returns>Instância de ResultadoLancamento representando sucesso.</returns>
    public static ResultadoLancamento CriarSucesso(VetorVoo velocidadeInicial) =>
        new()
        {
            Sucesso = true,
            VelocidadeInicial = velocidadeInicial,
            MensagemErro = null
        };

    /// <summary>
    /// Cria um resultado indicando falha ou recusa do lançamento com a respectiva mensagem de erro.
    /// </summary>
    /// <param name="mensagemErro">Motivo pelo qual o lançamento não pôde ser concluído.</param>
    /// <returns>Instância de ResultadoLancamento representando falha.</returns>
    public static ResultadoLancamento CriarFalha(string mensagemErro) =>
        new()
        {
            Sucesso = false,
            VelocidadeInicial = VetorVoo.Zero,
            MensagemErro = mensagemErro
        };
}
