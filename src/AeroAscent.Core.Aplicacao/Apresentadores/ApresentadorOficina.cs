namespace AeroAscent.Core.Aplicacao.Apresentadores;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using AeroAscent.Core.Aplicacao.Contratos;
using AeroAscent.Core.Aplicacao.DTOs;
using AeroAscent.Core.Dominio.Contratos;
using AeroAscent.Core.Dominio.Entidades;
using AeroAscent.Core.Dominio.Enums;
using AeroAscent.Core.Dominio.Excecoes;

/// <summary>
/// Apresentador da Oficina mecânica e Hangar (Model-View-Presenter), em C# puro (.NET Standard 2.1),
/// desacoplado da Unity Engine. Responsável por orquestrar os casos de uso, formatar valores em pt-BR,
/// calcular estados visuais dos cartões e prevenir duplo clique por reentrância.
/// </summary>
public sealed class ApresentadorOficina : IApresentadorOficina
{
    private static readonly CultureInfo CulturaPtBr = new("pt-BR");

    private readonly IConsultarOficinaCasoDeUso _consultarOficinaCasoDeUso;
    private readonly IComprarMelhoriaCasoDeUso _comprarMelhoriaCasoDeUso;
    private readonly IRepositorioProgresso _repositorioProgresso;
    private readonly IVisaoOficina _visao;

    private bool _estaProcessandoCompra;
    private bool _descartado;

    /// <inheritdoc />
    public event Action? AoSolicitarDecolagem;

    /// <summary>
    /// Construtor com injeção obrigatória dos casos de uso, do repositório de progresso e da visão passiva.
    /// </summary>
    public ApresentadorOficina(
        IConsultarOficinaCasoDeUso consultarOficinaCasoDeUso,
        IComprarMelhoriaCasoDeUso comprarMelhoriaCasoDeUso,
        IRepositorioProgresso repositorioProgresso,
        IVisaoOficina visao)
    {
        _consultarOficinaCasoDeUso = consultarOficinaCasoDeUso ?? throw new DominioInvalidoException(
            nameof(consultarOficinaCasoDeUso),
            "O caso de uso de consulta da oficina não pode ser nulo.");

        _comprarMelhoriaCasoDeUso = comprarMelhoriaCasoDeUso ?? throw new DominioInvalidoException(
            nameof(comprarMelhoriaCasoDeUso),
            "O caso de uso de compra de melhoria não pode ser nulo.");

        _repositorioProgresso = repositorioProgresso ?? throw new DominioInvalidoException(
            nameof(repositorioProgresso),
            "O repositório de progresso não pode ser nulo.");

        _visao = visao ?? throw new DominioInvalidoException(
            nameof(visao),
            "A visão passiva da oficina não pode ser nula.");

        _visao.AoClicarComprar += TratarCliqueComprar;
        _visao.AoClicarDecolar += TratarCliqueDecolar;
    }

    /// <inheritdoc />
    public async Task InicializarAsync(CancellationToken cancelamento = default)
    {
        cancelamento.ThrowIfCancellationRequested();
        await AtualizarVisaoAsync(cancelamento).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task ProcessarCompraAsync(TipoMelhoria tipo, CancellationToken cancelamento = default)
    {
        cancelamento.ThrowIfCancellationRequested();

        if (_estaProcessandoCompra)
        {
            return; // Bloqueio atômico de spam click e reentrância
        }

        _estaProcessandoCompra = true;
        _visao.DefinirInteracaoHabilitada(false);

        try
        {
            var resultado = await _comprarMelhoriaCasoDeUso.ExecutarAsync(tipo, cancelamento).ConfigureAwait(false);
            _visao.ExibirFeedbackCompra(resultado.Tipo, resultado.NovoNivel);
            await AtualizarVisaoAsync(cancelamento).ConfigureAwait(false);
        }
        catch (SaldoInsuficienteException ex)
        {
            _visao.ExibirMensagemErro(ex.Message);
        }
        catch (MelhoriaNivelMaximoException ex)
        {
            _visao.ExibirMensagemErro(ex.Message);
        }
        finally
        {
            _estaProcessandoCompra = false;
            _visao.DefinirInteracaoHabilitada(true);
        }
    }

    /// <inheritdoc />
    public void SolicitarDecolagem()
    {
        AoSolicitarDecolagem?.Invoke();
    }

    private async Task AtualizarVisaoAsync(CancellationToken cancelamento)
    {
        var progresso = await _repositorioProgresso.CarregarProgressoAsync(cancelamento).ConfigureAwait(false)
                        ?? ProgressoJogador.CriarNovo();

        var itensOficina = await _consultarOficinaCasoDeUso.ExecutarAsync(cancelamento).ConfigureAwait(false);

        var cartoes = new List<ItemCartaoOficinaDTO>(itensOficina.Count);

        foreach (var item in itensOficina)
        {
            bool estaNoNivelMaximo = item.EstaNoNivelMaximo;
            string textoNivel = estaNoNivelMaximo ? "Nível 10 (MAX)" : $"Nível {item.NivelAtual}";
            float progressoNormalizado = item.NivelAtual / 10.0f;
            long? custoMoedas = item.CustoProximoNivel?.Quantidade;

            string textoBotao;
            if (estaNoNivelMaximo)
            {
                textoBotao = "MÁXIMO";
            }
            else if (custoMoedas.HasValue)
            {
                textoBotao = string.Format(CulturaPtBr, "💰 {0:N0}", custoMoedas.Value);
            }
            else
            {
                textoBotao = "MÁXIMO";
            }

            cartoes.Add(new ItemCartaoOficinaDTO
            {
                Tipo = item.Tipo,
                Titulo = item.NomeAmigavel,
                NivelAtual = item.NivelAtual,
                TextoNivel = textoNivel,
                ProgressoNormalizado = progressoNormalizado,
                CustoProximoNivel = custoMoedas,
                TextoBotao = textoBotao,
                PodeComprar = item.PodeComprar,
                EstaNoNivelMaximo = estaNoNivelMaximo
            });
        }

        long saldoMoedas = progresso.SaldoMoedas.Quantidade;

        var modelo = new ModeloVisualOficina
        {
            SaldoMoedas = saldoMoedas,
            SaldoFormatado = string.Format(CulturaPtBr, "💰 {0:N0}", saldoMoedas),
            RecordeDistanciaMetros = progresso.RecordeDistanciaMetros,
            RecordeDistanciaFormatado = string.Format(CulturaPtBr, "Recorde: {0:N1} m", progresso.RecordeDistanciaMetros),
            RecordeAltitudeMetros = progresso.RecordeAltitudeMetros,
            RecordeAltitudeFormatado = string.Format(CulturaPtBr, "Altitude Máx: {0:N1} m", progresso.RecordeAltitudeMetros),
            TotalVoosRealizados = progresso.TotalVoosRealizados,
            Cartoes = cartoes
        };

        _visao.AtualizarTela(modelo);
    }

    private void TratarCliqueComprar(TipoMelhoria tipo)
    {
        _ = ProcessarCompraAsync(tipo);
    }

    private void TratarCliqueDecolar()
    {
        SolicitarDecolagem();
    }

    /// <summary>
    /// Libera os manipuladores de eventos da visão passiva.
    /// </summary>
    public void Dispose()
    {
        if (!_descartado)
        {
            _visao.AoClicarComprar -= TratarCliqueComprar;
            _visao.AoClicarDecolar -= TratarCliqueDecolar;
            _descartado = true;
        }
    }
}
