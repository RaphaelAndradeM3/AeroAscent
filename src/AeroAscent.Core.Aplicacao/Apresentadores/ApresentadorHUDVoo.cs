namespace AeroAscent.Core.Aplicacao.Apresentadores;

using System;
using AeroAscent.Core.Aplicacao.Contratos;
using AeroAscent.Core.Aplicacao.DTOs;
using AeroAscent.Core.Dominio.Entidades;
using AeroAscent.Core.Dominio.Enums;
using AeroAscent.Core.Dominio.Excecoes;
using AeroAscent.Core.Dominio.ObjetosDeValor;

/// <summary>
/// Apresentador do HUD de voo seguindo o padrão Model-View-Presenter (MVP).
/// Orquestra a projeção de telemetria na stack (GC Alloc = 0 bytes), a detecção de recordes,
/// a interatividade do botão de propulsão e a síntese de comandos táteis e de teclado.
/// </summary>
public class ApresentadorHUDVoo : IApresentadorHUDVoo
{
    private readonly IVisaoHUDVoo _visao;
    private float _recordeAtual;
    private bool _recordeNotificado;
    private bool _estaSubindo;
    private bool _estaDescendo;
    private bool _estaComBoost;
    private bool _estaPausado;

    /// <inheritdoc />
    public event Action? AoSolicitarPausa;

    /// <inheritdoc />
    public bool EstaPausado => _estaPausado;

    /// <summary>
    /// Inicializa uma nova instância do apresentador vinculada à visão passiva informada.
    /// </summary>
    /// <param name="visao">Interface da visão passiva implementada na camada gráfica.</param>
    /// <exception cref="DominioInvalidoException">Lançada caso a visão seja nula.</exception>
    public ApresentadorHUDVoo(IVisaoHUDVoo visao)
    {
        _visao = visao ?? throw new DominioInvalidoException(nameof(visao), "A visão do HUD não pode ser nula.");

        _visao.AoSolicitarSubida += IniciarSubida;
        _visao.AoInterromperSubida += PararSubida;
        _visao.AoSolicitarDescida += IniciarDescida;
        _visao.AoInterromperDescida += PararDescida;
        _visao.AoSolicitarBoost += IniciarBoost;
        _visao.AoInterromperBoost += PararBoost;
        _visao.AoSolicitarPausa += SolicitarPausa;
    }

    /// <inheritdoc />
    public void Inicializar(float recordeInicial)
    {
        _recordeAtual = MathF.Max(0f, recordeInicial);
        _recordeNotificado = false;
        _estaSubindo = false;
        _estaDescendo = false;
        _estaComBoost = false;
        _estaPausado = false;
    }

    /// <inheritdoc />
    public void Atualizar(Voo voo, in EstadoFisicoAeronave estadoFisico)
    {
        if (voo == null)
        {
            throw new DominioInvalidoException(nameof(voo), "A sessão de voo não pode ser nula.");
        }

        // Se o voo foi finalizado (pouso ou colisão/cancelado), oculta imediatamente os botões táteis
        if (voo.Status == StatusVoo.Pousado || voo.Status == StatusVoo.Cancelado)
        {
            _estaSubindo = false;
            _estaDescendo = false;
            _estaComBoost = false;
            _visao.DefinirVisibilidadeControles(false);
        }

        // Atualização e bloqueio de boost conforme combustível e status de voo
        var boostDisponivel = voo.Status == StatusVoo.EmVoo && !voo.Combustivel.EstaVazio && !estadoFisico.NoSolo;
        if (!boostDisponivel)
        {
            _estaComBoost = false;
        }

        _visao.DefinirInteratividadeBoost(boostDisponivel);

        // Verificação de quebra de recorde histórico
        var distanciaAtual = voo.DistanciaPercorrida;
        var recordeSuperado = _recordeAtual > 0f && distanciaAtual > _recordeAtual;
        if (recordeSuperado && !_recordeNotificado)
        {
            _recordeNotificado = true;
            _visao.NotificarNovoRecorde();
        }

        // Síntese da telemetria imutável na stack
        var velocidade = estadoFisico.VelocidadeEscalar;
        var telemetria = new TelemetriaHUDDTO(
            distanciaAtual,
            _recordeAtual,
            estadoFisico.Posicao.Y,
            velocidade,
            voo.Combustivel.PercentualRestante,
            voo.MoedasColetadas,
            recordeSuperado,
            boostDisponivel);

        _visao.AtualizarTelemetria(in telemetria);
    }

    /// <inheritdoc />
    public ParametrosControlePiloto ObterComandosControle()
    {
        if (_estaPausado)
        {
            return ParametrosControlePiloto.Neutro;
        }

        float intensidadePitch = 0f;
        if (_estaSubindo && !_estaDescendo)
        {
            intensidadePitch = 1f;
        }
        else if (_estaDescendo && !_estaSubindo)
        {
            intensidadePitch = -1f;
        }

        return new ParametrosControlePiloto(
            intensidadePitch,
            ParametrosControlePiloto.TAXA_ANGULAR_PADRAO,
            _estaComBoost);
    }

    /// <inheritdoc />
    public void IniciarSubida()
    {
        if (!_estaPausado)
        {
            _estaSubindo = true;
        }
    }

    /// <inheritdoc />
    public void PararSubida()
    {
        _estaSubindo = false;
    }

    /// <inheritdoc />
    public void IniciarDescida()
    {
        if (!_estaPausado)
        {
            _estaDescendo = true;
        }
    }

    /// <inheritdoc />
    public void PararDescida()
    {
        _estaDescendo = false;
    }

    /// <inheritdoc />
    public void IniciarBoost()
    {
        if (!_estaPausado)
        {
            _estaComBoost = true;
        }
    }

    /// <inheritdoc />
    public void PararBoost()
    {
        _estaComBoost = false;
    }

    /// <inheritdoc />
    public void SolicitarPausa()
    {
        _estaPausado = !_estaPausado;
        if (_estaPausado)
        {
            _estaSubindo = false;
            _estaDescendo = false;
            _estaComBoost = false;
        }

        AoSolicitarPausa?.Invoke();
    }
}
