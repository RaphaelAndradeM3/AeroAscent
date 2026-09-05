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
/// Implementação do caso de uso de aplicação responsável por finalizar formalmente sessões de voo,
/// converter métricas de voo em moedas de recompensa, atualizar o saldo e os recordes históricos do jogador,
/// persistir o progresso no repositório de forma atômica e garantir execução estritamente idempotente.
/// </summary>
public class FinalizarVooCasoDeUso : IFinalizarVooCasoDeUso
{
    private readonly IRepositorioProgresso _repositorioProgresso;

    /// <summary>
    /// Construtor com injeção de dependência do repositório de progresso.
    /// </summary>
    /// <param name="repositorioProgresso">Repositório de persistência do progresso do jogador.</param>
    /// <exception cref="DominioInvalidoException">Lançada caso a referência do repositório seja nula.</exception>
    public FinalizarVooCasoDeUso(IRepositorioProgresso repositorioProgresso)
    {
        _repositorioProgresso = repositorioProgresso ?? throw new DominioInvalidoException(
            nameof(repositorioProgresso),
            "O repositório de progresso não pode ser nulo.");
    }

    /// <inheritdoc />
    public async Task<ResumoFinalizacaoVoo> ExecutarAsync(Voo voo, CancellationToken cancelamento = default)
    {
        if (voo == null)
        {
            throw new DominioInvalidoException(nameof(voo), "A sessão de voo a ser finalizada não pode ser nula.");
        }

        cancelamento.ThrowIfCancellationRequested();

        if (voo.Status == StatusVoo.EmPreparacao || voo.Status == StatusVoo.EmVoo)
        {
            throw new DominioInvalidoException(
                nameof(voo.Status),
                $"Não é possível finalizar um voo que ainda não pousou ou foi cancelado. Status atual: {voo.Status}.");
        }

        var progresso = await _repositorioProgresso.CarregarProgressoAsync(cancelamento);
        progresso ??= ProgressoJogador.CriarNovo();

        var moedasDistancia = (long)MathF.Floor(voo.DistanciaPercorrida * 0.1f);
        var moedasAltitude = (long)MathF.Floor(voo.AltitudeMaxima * 0.05f);
        var totalGanhas = new Moeda(moedasDistancia + moedasAltitude + voo.MoedasColetadas);

        // Caso o voo já tenha tido sua premiação liquidada (garantia de idempotência - SC-003)
        if (voo.PremiacaoLiquidada)
        {
            if (voo.Status == StatusVoo.Cancelado)
            {
                return ResumoFinalizacaoVoo.CriarCancelado(voo.DistanciaPercorrida, voo.AltitudeMaxima, progresso.SaldoMoedas);
            }

            return ResumoFinalizacaoVoo.Criar(
                voo.DistanciaPercorrida,
                voo.AltitudeMaxima,
                moedasDistancia,
                moedasAltitude,
                voo.MoedasColetadas,
                totalGanhas,
                progresso.SaldoMoedas,
                ehNovoRecordeDistancia: false,
                ehNovoRecordeAltitude: false);
        }

        // Tratamento de voo cancelado ou abortado
        if (voo.Status == StatusVoo.Cancelado)
        {
            voo.MarcarPremiacaoLiquidada();
            return ResumoFinalizacaoVoo.CriarCancelado(voo.DistanciaPercorrida, voo.AltitudeMaxima, progresso.SaldoMoedas);
        }

        // Voo pousado com sucesso: avaliar recordes históricos antes da atualização
        var ehNovoRecordeDistancia = voo.DistanciaPercorrida > progresso.RecordeDistanciaMetros;
        var ehNovoRecordeAltitude = voo.AltitudeMaxima > progresso.RecordeAltitudeMetros;

        var resultadoVoo = voo.Resultado ?? ResultadoVoo.Calcular(voo.DistanciaPercorrida, voo.AltitudeMaxima, voo.MoedasColetadas);
        progresso.ProcessarFimDeVoo(resultadoVoo);
        voo.MarcarPremiacaoLiquidada();

        await _repositorioProgresso.SalvarProgressoAsync(progresso, cancelamento);

        return ResumoFinalizacaoVoo.Criar(
            voo.DistanciaPercorrida,
            voo.AltitudeMaxima,
            moedasDistancia,
            moedasAltitude,
            voo.MoedasColetadas,
            totalGanhas,
            progresso.SaldoMoedas,
            ehNovoRecordeDistancia,
            ehNovoRecordeAltitude);
    }
}
