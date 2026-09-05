namespace AeroAscent.Core.Aplicacao.CasosDeUso;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AeroAscent.Core.Aplicacao.Contratos;
using AeroAscent.Core.Aplicacao.DTOs;
using AeroAscent.Core.Dominio.Contratos;
using AeroAscent.Core.Dominio.Entidades;
using AeroAscent.Core.Dominio.Enums;
using AeroAscent.Core.Dominio.Excecoes;
using AeroAscent.Core.Dominio.ObjetosDeValor;

/// <summary>
/// Caso de uso de aplicação responsável por consultar o catálogo de melhorias mecânicas da oficina,
/// projetando a lista imutável de itens com seus níveis atuais, custos para a próxima evolução,
/// permissão de compra de acordo com o saldo do jogador e sinalização de teto máximo atingido.
/// </summary>
public class ConsultarOficinaCasoDeUso : IConsultarOficinaCasoDeUso
{
    private static readonly (TipoMelhoria Tipo, string NomeAmigavel)[] ComponentesOficina =
    [
        (TipoMelhoria.Motor, "Motor"),
        (TipoMelhoria.Aerodinamica, "Aerodinâmica"),
        (TipoMelhoria.TanqueCombustivel, "Tanque de Combustível"),
        (TipoMelhoria.Catapulta, "Catapulta")
    ];

    private readonly IRepositorioProgresso _repositorioProgresso;

    /// <summary>
    /// Construtor do caso de uso com injeção obrigatória do repositório de persistência do progresso.
    /// </summary>
    /// <param name="repositorioProgresso">Instância do repositório de persistência do progresso do jogador.</param>
    /// <exception cref="DominioInvalidoException">Lançada caso o repositório informado seja nulo.</exception>
    public ConsultarOficinaCasoDeUso(IRepositorioProgresso repositorioProgresso)
    {
        _repositorioProgresso = repositorioProgresso ?? throw new DominioInvalidoException(
            nameof(repositorioProgresso),
            "O repositório de progresso não pode ser nulo.");
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<ItemOficinaDTO>> ExecutarAsync(CancellationToken cancelamento = default)
    {
        cancelamento.ThrowIfCancellationRequested();

        var progresso = await _repositorioProgresso.CarregarProgressoAsync(cancelamento).ConfigureAwait(false)
                        ?? ProgressoJogador.CriarNovo();

        var oficina = Oficina.CriarPadrao();
        var listaItens = new List<ItemOficinaDTO>(ComponentesOficina.Length);

        foreach (var (tipo, nomeAmigavel) in ComponentesOficina)
        {
            int nivelAtual = progresso.Aeronave.ObterNivel(tipo);
            bool estaNoNivelMaximo = nivelAtual >= Aeronave.NIVEL_MAXIMO;

            Moeda? custoProximoNivel = estaNoNivelMaximo
                ? null
                : oficina.CalcularCustoMelhoria(tipo, nivelAtual);

            bool podeComprar = !estaNoNivelMaximo
                               && custoProximoNivel.HasValue
                               && progresso.SaldoMoedas.Quantidade >= custoProximoNivel.Value.Quantidade;

            listaItens.Add(new ItemOficinaDTO(
                tipo,
                nomeAmigavel,
                nivelAtual,
                custoProximoNivel,
                podeComprar,
                estaNoNivelMaximo));
        }

        return listaItens.AsReadOnly();
    }
}
