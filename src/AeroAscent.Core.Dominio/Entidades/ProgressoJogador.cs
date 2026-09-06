namespace AeroAscent.Core.Dominio.Entidades;

using AeroAscent.Core.Dominio.Excecoes;
using AeroAscent.Core.Dominio.ObjetosDeValor;

/// <summary>
/// Raiz de agregação que consolida o estado global e persistível do jogador,
/// unificando as configurações da aeronave, saldo acumulado de moedas e recordes históricos de voo.
/// </summary>
public class ProgressoJogador
{
    /// <summary>
    /// Identificador único global do registro de progresso do jogador.
    /// </summary>
    public Guid Id { get; }

    /// <summary>
    /// Aeronave ativa do jogador com seus respectivos níveis de melhoria mecânica.
    /// </summary>
    public Aeronave Aeronave { get; private set; }

    /// <summary>
    /// Saldo total disponível de moedas acumulado pelo jogador.
    /// </summary>
    public Moeda SaldoMoedas { get; private set; }

    /// <summary>
    /// Maior distância horizontal atingida em um único voo pelo jogador em metros.
    /// </summary>
    public float RecordeDistanciaMetros { get; private set; }

    /// <summary>
    /// Maior altitude vertical atingida em um único voo pelo jogador em metros.
    /// </summary>
    public float RecordeAltitudeMetros { get; private set; }

    /// <summary>
    /// Quantidade total de voos concluídos pelo jogador ao longo do histórico.
    /// </summary>
    public int TotalVoosRealizados { get; private set; }

    /// <summary>
    /// Preferências acústicas de áudio do jogador (volumes e canais de efeitos e música).
    /// </summary>
    public ConfiguracaoAudio ConfiguracaoAudio { get; private set; }

    /// <summary>
    /// Construtor de compatibilidade do agregado ProgressoJogador, aplicando a configuração de áudio padrão.
    /// </summary>
    /// <param name="id">Identificador único do progresso.</param>
    /// <param name="aeronave">Aeronave ativa associada.</param>
    /// <param name="saldoMoedas">Saldo atual de moedas.</param>
    /// <param name="recordeDistanciaMetros">Recorde histórico de distância horizontal.</param>
    /// <param name="recordeAltitudeMetros">Recorde histórico de altitude máxima.</param>
    /// <param name="totalVoosRealizados">Total acumulado de voos executados.</param>
    public ProgressoJogador(
        Guid id,
        Aeronave aeronave,
        Moeda saldoMoedas,
        float recordeDistanciaMetros,
        float recordeAltitudeMetros,
        int totalVoosRealizados)
        : this(
            id,
            aeronave,
            saldoMoedas,
            recordeDistanciaMetros,
            recordeAltitudeMetros,
            totalVoosRealizados,
            ConfiguracaoAudio.Padrao)
    {
    }

    /// <summary>
    /// Construtor completo do agregado ProgressoJogador com validação rigorosa de invariantes.
    /// </summary>
    /// <param name="id">Identificador único do progresso.</param>
    /// <param name="aeronave">Aeronave ativa associada.</param>
    /// <param name="saldoMoedas">Saldo atual de moedas.</param>
    /// <param name="recordeDistanciaMetros">Recorde histórico de distância horizontal.</param>
    /// <param name="recordeAltitudeMetros">Recorde histórico de altitude máxima.</param>
    /// <param name="totalVoosRealizados">Total acumulado de voos executados.</param>
    /// <param name="configuracaoAudio">Preferências acústicas de volume e canais sonoros.</param>
    /// <exception cref="DominioInvalidoException">Lançada caso algum dado viole as regras do domínio.</exception>
    public ProgressoJogador(
        Guid id,
        Aeronave aeronave,
        Moeda saldoMoedas,
        float recordeDistanciaMetros,
        float recordeAltitudeMetros,
        int totalVoosRealizados,
        ConfiguracaoAudio configuracaoAudio)
    {
        if (id == Guid.Empty)
        {
            throw new DominioInvalidoException(nameof(Id), "O identificador do progresso do jogador não pode ser vazio.");
        }

        if (aeronave == null)
        {
            throw new DominioInvalidoException(nameof(Aeronave), "A aeronave vinculada ao progresso não pode ser nula.");
        }

        if (recordeDistanciaMetros < 0f)
        {
            throw new DominioInvalidoException(nameof(RecordeDistanciaMetros), "O recorde de distância não pode ser negativo.");
        }

        if (recordeAltitudeMetros < 0f)
        {
            throw new DominioInvalidoException(nameof(RecordeAltitudeMetros), "O recorde de altitude não pode ser negativo.");
        }

        if (totalVoosRealizados < 0)
        {
            throw new DominioInvalidoException(nameof(TotalVoosRealizados), "O total de voos realizados não pode ser negativo.");
        }

        Id = id;
        Aeronave = aeronave;
        SaldoMoedas = saldoMoedas;
        RecordeDistanciaMetros = recordeDistanciaMetros;
        RecordeAltitudeMetros = recordeAltitudeMetros;
        TotalVoosRealizados = totalVoosRealizados;
        ConfiguracaoAudio = configuracaoAudio;
    }

    /// <summary>
    /// Inicializa um novo registro de progresso limpo para um novo jogador, com aeronave padrão, saldos zerados e áudio padrão.
    /// </summary>
    /// <returns>Nova instância de ProgressoJogador.</returns>
    public static ProgressoJogador CriarNovo()
    {
        return new ProgressoJogador(
            Guid.NewGuid(),
            Aeronave.CriarPadrao(),
            Moeda.Zero,
            recordeDistanciaMetros: 0f,
            recordeAltitudeMetros: 0f,
            totalVoosRealizados: 0,
            ConfiguracaoAudio.Padrao);
    }

    /// <summary>
    /// Atualiza as preferências sonoras de áudio do jogador.
    /// </summary>
    /// <param name="novaConfiguracao">Novas opções de volume e canais ativos.</param>
    public void AtualizarConfiguracaoAudio(ConfiguracaoAudio novaConfiguracao)
    {
        ConfiguracaoAudio = novaConfiguracao;
    }

    /// <summary>
    /// Adiciona moedas à carteira do jogador.
    /// </summary>
    /// <param name="ganho">Quantia monetária a ser creditada.</param>
    public void CreditarMoedas(Moeda ganho)
    {
        SaldoMoedas = SaldoMoedas.Adicionar(ganho);
    }

    /// <summary>
    /// Deduz moedas da carteira do jogador com validação de saldo suficiente.
    /// </summary>
    /// <param name="custo">Quantia monetária a ser debitada.</param>
    /// <exception cref="SaldoInsuficienteException">Lançada se o saldo for menor que o débito requerido.</exception>
    public void DebitarMoedas(Moeda custo)
    {
        SaldoMoedas = SaldoMoedas.Subtrair(custo);
    }

    /// <summary>
    /// Processa a conclusão de uma sessão de voo com base no seu ResultadoVoo imutável,
    /// creditando os ganhos obtidos e atualizando os recordes históricos de distância e altitude.
    /// </summary>
    /// <param name="resultado">Resultado consolidado do voo pousado.</param>
    /// <exception cref="DominioInvalidoException">Lançada se o resultado for nulo.</exception>
    public void ProcessarFimDeVoo(ResultadoVoo resultado)
    {
        if (resultado == null)
        {
            throw new DominioInvalidoException(nameof(resultado), "O resultado de voo a ser processado não pode ser nulo.");
        }

        CreditarMoedas(resultado.MoedasRecompensaTotal);
        RecordeDistanciaMetros = MathF.Max(RecordeDistanciaMetros, resultado.DistanciaMetros);
        RecordeAltitudeMetros = MathF.Max(RecordeAltitudeMetros, resultado.AltitudeMaximaMetros);
        TotalVoosRealizados++;
    }
}
