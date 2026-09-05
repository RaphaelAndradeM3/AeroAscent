namespace AeroAscent.Core.Dominio.Servicos;

using System;
using System.Collections.Generic;
using AeroAscent.Core.Dominio.Comum;
using AeroAscent.Core.Dominio.Contratos;
using AeroAscent.Core.Dominio.Entidades;
using AeroAscent.Core.Dominio.Enums;
using AeroAscent.Core.Dominio.Excecoes;
using AeroAscent.Core.Dominio.ObjetosDeValor;

/// <summary>
/// Serviço de domínio encarregado da geração procedural determinística de coletáveis em janela dinâmica
/// e reciclagem automática de itens que ficarem para trás da aeronave (SC-003).
/// </summary>
public class ServicoGeracaoProceduralColetaveis : IServicoGeracaoProceduralColetaveis
{
    /// <summary>
    /// Distância mínima à frente da aeronave onde novos coletáveis começam a ser gerados (+30.0m).
    /// </summary>
    public const float DISTANCIA_MINIMA_SPAWN_FRENTE_METROS = 30.0f;

    /// <summary>
    /// Distância máxima à frente da aeronave que define o limite da janela de geração ativa (+150.0m).
    /// </summary>
    public const float DISTANCIA_MAXIMA_SPAWN_FRENTE_METROS = 150.0f;

    /// <summary>
    /// Distância atrás da aeronave a partir da qual qualquer coletável deve ser reciclado (-20.0m).
    /// </summary>
    public const float DISTANCIA_RECICLAGEM_TRASEIRA_METROS = 20.0f;

    /// <summary>
    /// Altitude mínima permitida para o spawn de coletáveis acima do solo (5.0m).
    /// </summary>
    public const float ALTITUDE_MINIMA_METROS = 5.0f;

    /// <summary>
    /// Altitude máxima permitida para o spawn de coletáveis acima do solo (120.0m).
    /// </summary>
    public const float ALTITUDE_MAXIMA_METROS = 120.0f;

    /// <summary>
    /// Espaçamento longitudinal padrão entre sucessivos coletáveis na pista (15.0m).
    /// </summary>
    public const float ESPACAMENTO_ENTRE_SPAWNS_METROS = 15.0f;

    /// <summary>
    /// Probabilidade percentual de um coletável gerado ser um anel de vento (25%). O restante (75%) são moedas.
    /// </summary>
    public const int PROBABILIDADE_ANEL_VENTO_PERCENTUAL = 25;

    private Random _random;
    private float _proximoZParaGerar;

    /// <summary>
    /// Inicializa uma nova instância do serviço com a semente pseudo-randômica especificada.
    /// </summary>
    /// <param name="semente">Semente para reprodução determinística (padrão 42).</param>
    public ServicoGeracaoProceduralColetaveis(int semente = 42)
    {
        Semente = semente;
        _random = new Random(semente);
        _proximoZParaGerar = DISTANCIA_MINIMA_SPAWN_FRENTE_METROS;
    }

    /// <inheritdoc />
    public int Semente { get; }

    /// <inheritdoc />
    public void AtualizarJanela(
        float posicaoZAeronave,
        IPoolObjetos<Coletavel> poolMoedas,
        IPoolObjetos<Coletavel> poolAneis,
        IList<Coletavel> coletaveisAtivos)
    {
        if (poolMoedas == null)
        {
            throw new DominioInvalidoException(nameof(poolMoedas), "O pool de moedas não pode ser nulo.");
        }

        if (poolAneis == null)
        {
            throw new DominioInvalidoException(nameof(poolAneis), "O pool de anéis não pode ser nulo.");
        }

        if (coletaveisAtivos == null)
        {
            throw new DominioInvalidoException(nameof(coletaveisAtivos), "A lista de coletáveis ativos não pode ser nula.");
        }

        // 1. Reciclagem de coletáveis que ficaram para trás da aeronave (SC-003: Z < Z_aeronave - 20m)
        var limiteTraseiro = posicaoZAeronave - DISTANCIA_RECICLAGEM_TRASEIRA_METROS;
        for (var i = coletaveisAtivos.Count - 1; i >= 0; i--)
        {
            var coletavel = coletaveisAtivos[i];
            if (coletavel == null)
            {
                continue;
            }

            if (coletavel.Posicao.Z < limiteTraseiro)
            {
                coletavel.Desativar();
                coletaveisAtivos.RemoveAt(i);

                if (coletavel.Tipo == TipoColetavel.Moeda)
                {
                    poolMoedas.Liberar(coletavel);
                }
                else if (coletavel.Tipo == TipoColetavel.AnelVento)
                {
                    poolAneis.Liberar(coletavel);
                }
            }
        }

        // 2. Geração procedural de novos coletáveis na janela ativa [+30m, +150m] à frente
        var limiteFrontal = posicaoZAeronave + DISTANCIA_MAXIMA_SPAWN_FRENTE_METROS;
        var inicioJanela = posicaoZAeronave + DISTANCIA_MINIMA_SPAWN_FRENTE_METROS;

        if (_proximoZParaGerar < inicioJanela)
        {
            _proximoZParaGerar = inicioJanela;
        }

        while (_proximoZParaGerar <= limiteFrontal)
        {
            var altitude = ALTITUDE_MINIMA_METROS + (float)_random.NextDouble() * (ALTITUDE_MAXIMA_METROS - ALTITUDE_MINIMA_METROS);
            var posicao = new VetorVoo(0f, altitude, _proximoZParaGerar);

            var sorteio = _random.Next(100);
            if (sorteio < PROBABILIDADE_ANEL_VENTO_PERCENTUAL)
            {
                var anel = poolAneis.Obter();
                anel.Ativar(posicao);
                coletaveisAtivos.Add(anel);
            }
            else
            {
                var moeda = poolMoedas.Obter();
                moeda.Ativar(posicao);
                coletaveisAtivos.Add(moeda);
            }

            _proximoZParaGerar += ESPACAMENTO_ENTRE_SPAWNS_METROS;
        }
    }

    /// <inheritdoc />
    public void Reiniciar()
    {
        _random = new Random(Semente);
        _proximoZParaGerar = DISTANCIA_MINIMA_SPAWN_FRENTE_METROS;
    }
}
