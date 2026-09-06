namespace AeroAscent.Apresentacao.MAUI.Servicos;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AeroAscent.Core.Aplicacao.CasosDeUso;
using AeroAscent.Core.Aplicacao.Contratos;
using AeroAscent.Core.Dominio.Comum;
using AeroAscent.Core.Dominio.Contratos;
using AeroAscent.Core.Dominio.Entidades;
using AeroAscent.Core.Dominio.Enums;
using AeroAscent.Core.Dominio.ObjetosDeValor;
using AeroAscent.Core.Dominio.Servicos;

/// <summary>
/// Orquestrador central da sessão do jogo no cliente .NET MAUI.
/// Mantém o estado da sessão de voo ativa, o progresso persistido e os pools de objetos.
/// </summary>
public sealed class GerenciadorSessaoJogo
{
    private readonly IRepositorioProgresso _repositorioProgresso;
    private readonly IServicoFisicaVoo _servicoFisica;
    private readonly IServicoAudio _servicoAudio;
    private readonly GerenciadorParticulasMAUI _gerenciadorParticulas;
    private readonly PublicadorEventosVooMAUI _publicadorEventos;

    private readonly ILancarAeronaveCasoDeUso _lancarCasoDeUso;
    private readonly IAtualizarFisicaVooCasoDeUso _atualizarFisicaCasoDeUso;
    private readonly IProcessarColetaveisVooCasoDeUso _processarColetaveisCasoDeUso;
    private readonly IProcessarPousoFimVooCasoDeUso _processarPousoCasoDeUso;
    private readonly IFinalizarVooCasoDeUso _finalizarVooCasoDeUso;
    private readonly IServicoGeracaoProceduralColetaveis _geradorProcedural;

    private readonly IPoolObjetos<Coletavel> _poolMoedas;
    private readonly IPoolObjetos<Coletavel> _poolAneis;
    private readonly List<Coletavel> _coletaveisAtivos = new(64);

    private ProgressoJogador? _progresso;
    private Voo? _vooAtual;
    private EstadoFisicoAeronave _estadoFisico;
    private ResumoFinalizacaoVoo _ultimoResumo;

    public ProgressoJogador? Progresso => _progresso;
    public Voo? VooAtual => _vooAtual;
    public EstadoFisicoAeronave EstadoFisico => _estadoFisico;
    public ResumoFinalizacaoVoo UltimoResumo => _ultimoResumo;
    public IList<Coletavel> ColetaveisAtivos => _coletaveisAtivos;
    public GerenciadorParticulasMAUI GerenciadorParticulas => _gerenciadorParticulas;
    public IServicoAudio ServicoAudio => _servicoAudio;

    public GerenciadorSessaoJogo(
        IRepositorioProgresso repositorioProgresso,
        IServicoFisicaVoo servicoFisica,
        IServicoAudio servicoAudio,
        GerenciadorParticulasMAUI gerenciadorParticulas,
        PublicadorEventosVooMAUI publicadorEventos,
        ILancarAeronaveCasoDeUso lancarCasoDeUso,
        IAtualizarFisicaVooCasoDeUso atualizarFisicaCasoDeUso,
        IProcessarPousoFimVooCasoDeUso processarPousoCasoDeUso,
        IFinalizarVooCasoDeUso finalizarVooCasoDeUso)
    {
        _repositorioProgresso = repositorioProgresso;
        _servicoFisica = servicoFisica;
        _servicoAudio = servicoAudio;
        _gerenciadorParticulas = gerenciadorParticulas;
        _publicadorEventos = publicadorEventos;
        _lancarCasoDeUso = lancarCasoDeUso;
        _atualizarFisicaCasoDeUso = atualizarFisicaCasoDeUso;
        _processarPousoCasoDeUso = processarPousoCasoDeUso;
        _finalizarVooCasoDeUso = finalizarVooCasoDeUso;

        _geradorProcedural = new ServicoGeracaoProceduralColetaveis();
        _processarColetaveisCasoDeUso = new ProcessarColetaveisVooCasoDeUso(_geradorProcedural);

        _poolMoedas = new GerenciadorPoolObjetos<Coletavel>(
            () => Coletavel.CriarMoeda(VetorVoo.Zero),
            60);

        _poolAneis = new GerenciadorPoolObjetos<Coletavel>(
            () => Coletavel.CriarAnelVento(VetorVoo.Zero),
            30);
    }

    /// <summary>
    /// Carrega o progresso persistido do jogador ou inicializa novo se ausente.
    /// </summary>
    public async Task InicializarAsync()
    {
        _progresso = await _repositorioProgresso.CarregarProgressoAsync();
    }

    /// <summary>
    /// Prepara uma nova sessão de voo a partir da oficina.
    /// </summary>
    public void PrepararNovoVoo()
    {
        if (_progresso == null) return;

        // Recicla coletáveis anteriores
        for (int i = 0; i < _coletaveisAtivos.Count; i++)
        {
            var item = _coletaveisAtivos[i];
            item.Desativar();
            if (item.Tipo == TipoColetavel.Moeda) _poolMoedas.Liberar(item);
            else if (item.Tipo == TipoColetavel.AnelVento) _poolAneis.Liberar(item);
        }
        _coletaveisAtivos.Clear();
        _geradorProcedural.Reiniciar();
        _gerenciadorParticulas.PararTodosOsEfeitos();

        var aeronave = _progresso.Aeronave;
        _vooAtual = Voo.Iniciar(aeronave);

        // Estado físico inicial repousando na catapulta em Z = 0, Y = 0
        _estadoFisico = EstadoFisicoAeronave.CriarInicial(VetorVoo.Zero, VetorVoo.Zero, 25f);
    }

    /// <summary>
    /// Lança a aeronave pela catapulta com a precisão informada pelo jogador.
    /// </summary>
    public ResultadoLancamento LancarAeronave(float precisao0a1)
    {
        if (_vooAtual == null)
        {
            return ResultadoLancamento.CriarFalha("Voo não inicializado.");
        }

        var param = new ParametrosLancamento(precisao0a1, 25f);
        var resultado = _lancarCasoDeUso.Executar(_vooAtual, param);

        if (resultado.Sucesso)
        {
            // Converte impulso inicial para o estado físico
            var propulsor = EstadoPropulsor.CriarInativo(
                _vooAtual.Combustivel.QuantidadeAtual,
                _vooAtual.Combustivel.CapacidadeMaxima,
                _vooAtual.Combustivel.TaxaQueimaPorSegundo);

            _estadoFisico = EstadoFisicoAeronave.CriarInicial(
                VetorVoo.Zero,
                resultado.VelocidadeInicial,
                25f,
                propulsor);

            _servicoAudio.TocarEvento(EventoAudio.LancamentoCatapulta);
        }

        return resultado;
    }

    /// <summary>
    /// Atualiza um frame da simulação de voo (60 FPS).
    /// </summary>
    public void AtualizarFrameVoo(ParametrosControlePiloto controle, float deltaSegundos)
    {
        if (_vooAtual == null || _vooAtual.Status != StatusVoo.EmVoo)
        {
            return;
        }

        // 1. Atualiza física e aerodinâmica
        _estadoFisico = _atualizarFisicaCasoDeUso.Executar(_vooAtual, _estadoFisico, controle, deltaSegundos);

        // 2. Processa coleta de moedas e anéis de vento
        var resultadoColetaveis = _processarColetaveisCasoDeUso.Executar(
            _vooAtual,
            _estadoFisico,
            _coletaveisAtivos,
            _poolMoedas,
            _poolAneis);

        _estadoFisico = resultadoColetaveis.EstadoFisicoAtualizado;

        // Efeitos de coleta
        if (resultadoColetaveis.MoedasColetadasNoPasso > 0)
        {
            _servicoAudio.TocarEvento(EventoAudio.ColetaMoeda);
            _gerenciadorParticulas.EmitirColetaMoeda(_estadoFisico.Posicao);
        }

        // 3. Emite rastro de fumaça / boost
        _gerenciadorParticulas.DefinirPropulsao(_estadoFisico.Propulsor.EstaAtivo, 1f);
        _gerenciadorParticulas.DefinirRastroCauda(!_estadoFisico.NoSolo, 0.5f);
        _gerenciadorParticulas.EmitirRastroAeronave(_estadoFisico.Posicao.Z, _estadoFisico.Posicao.Y, _estadoFisico.InclinacaoPitchGraus);
        _gerenciadorParticulas.Atualizar(deltaSegundos);

        // 4. Som de boost e vento
        _servicoAudio.DefinirLoopPropulsao(_estadoFisico.Propulsor.EstaAtivo, 1f);
        _servicoAudio.AtualizarLoopVento(_estadoFisico.VelocidadeEscalar / 40f);

        // 5. Avalia pouso e parada total
        if (_estadoFisico.NoSolo && _estadoFisico.Velocidade.Z <= 0.01f)
        {
            _processarPousoCasoDeUso.Executar(_vooAtual, _estadoFisico);
            _servicoAudio.DefinirLoopPropulsao(false);
            _servicoAudio.AtualizarLoopVento(0f);
        }
    }

    /// <summary>
    /// Finaliza formalmente a sessão de voo, persistindo moedas e recordes no repositório JSON.
    /// </summary>
    public async Task<ResumoFinalizacaoVoo> FinalizarVooAsync()
    {
        if (_vooAtual == null)
        {
            return default;
        }

        _ultimoResumo = await _finalizarVooCasoDeUso.ExecutarAsync(_vooAtual);
        // Atualiza a referência em memória do progresso
        _progresso = await _repositorioProgresso.CarregarProgressoAsync();

        if (_ultimoResumo.EhNovoRecordeDistancia || _ultimoResumo.EhNovoRecordeAltitude)
        {
            _servicoAudio.TocarEvento(EventoAudio.NovoRecorde);
            _gerenciadorParticulas.EmitirCelebracaoRecorde(VetorVoo.Zero);
        }

        return _ultimoResumo;
    }
}
