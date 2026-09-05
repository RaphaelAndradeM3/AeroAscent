namespace AeroAscent.Core.Aplicacao.CasosDeUso;

using System;
using System.Threading;
using System.Threading.Tasks;
using AeroAscent.Core.Aplicacao.Contratos;
using AeroAscent.Core.Dominio.Contratos;
using AeroAscent.Core.Dominio.Entidades;
using AeroAscent.Core.Dominio.Enums;
using AeroAscent.Core.Dominio.Excecoes;
using AeroAscent.Core.Dominio.ObjetosDeValor;

/// <summary>
/// Caso de uso de aplicação responsável por orquestrar a compra e evolução de melhorias mecânicas da aeronave,
/// aplicando a validação de saldo, evolução do componente na aeronave, salvamento atômico no repositório
/// e devolução de um extrato consolidado imutável alocado na stack.
/// </summary>
public class ComprarMelhoriaCasoDeUso : IComprarMelhoriaCasoDeUso
{
    private readonly IRepositorioProgresso _repositorioProgresso;

    /// <summary>
    /// Construtor do caso de uso com injeção obrigatória do repositório de persistência do progresso.
    /// </summary>
    /// <param name="repositorioProgresso">Instância do repositório de persistência do progresso do jogador.</param>
    /// <exception cref="DominioInvalidoException">Lançada caso o repositório informado seja nulo.</exception>
    public ComprarMelhoriaCasoDeUso(IRepositorioProgresso repositorioProgresso)
    {
        _repositorioProgresso = repositorioProgresso ?? throw new DominioInvalidoException(
            nameof(repositorioProgresso),
            "O repositório de progresso não pode ser nulo.");
    }

    /// <inheritdoc />
    public async Task<ResultadoCompraMelhoria> ExecutarAsync(TipoMelhoria tipo, CancellationToken cancelamento = default)
    {
        cancelamento.ThrowIfCancellationRequested();

        if (!Enum.IsDefined(typeof(TipoMelhoria), tipo))
        {
            throw new DominioInvalidoException(nameof(tipo), $"Tipo de melhoria mecânica inválido ou desconhecido: {tipo}.");
        }

        var progresso = await _repositorioProgresso.CarregarProgressoAsync(cancelamento).ConfigureAwait(false)
                        ?? ProgressoJogador.CriarNovo();

        int nivelAnterior = progresso.Aeronave.ObterNivel(tipo);

        if (nivelAnterior >= Aeronave.NIVEL_MAXIMO)
        {
            throw new MelhoriaNivelMaximoException(tipo, nivelAnterior);
        }

        var oficina = Oficina.CriarPadrao();
        var custo = oficina.CalcularCustoMelhoria(tipo, nivelAnterior);

        if (progresso.SaldoMoedas.Quantidade < custo.Quantidade)
        {
            throw new SaldoInsuficienteException(progresso.SaldoMoedas.Quantidade, custo.Quantidade);
        }

        progresso.DebitarMoedas(custo);

        int novoNivel = nivelAnterior + 1;
        progresso.Aeronave.AtualizarNivel(tipo, novoNivel);

        await _repositorioProgresso.SalvarProgressoAsync(progresso, cancelamento).ConfigureAwait(false);

        bool atingiuNivelMaximo = novoNivel >= Aeronave.NIVEL_MAXIMO;
        Moeda? proximoCusto = atingiuNivelMaximo ? null : oficina.CalcularCustoMelhoria(tipo, novoNivel);

        return new ResultadoCompraMelhoria(
            tipo,
            nivelAnterior,
            novoNivel,
            custo,
            progresso.SaldoMoedas,
            atingiuNivelMaximo,
            proximoCusto);
    }
}
