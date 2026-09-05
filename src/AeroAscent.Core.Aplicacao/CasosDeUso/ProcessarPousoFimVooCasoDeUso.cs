namespace AeroAscent.Core.Aplicacao.CasosDeUso;

using System;
using AeroAscent.Core.Aplicacao.Contratos;
using AeroAscent.Core.Dominio.Contratos;
using AeroAscent.Core.Dominio.Entidades;
using AeroAscent.Core.Dominio.Enums;
using AeroAscent.Core.Dominio.Excecoes;
using AeroAscent.Core.Dominio.ObjetosDeValor;

/// <summary>
/// Caso de uso de aplicação responsável por processar a detecção de parada no solo,
/// comandar a transição formal para Pousado na entidade Voo e publicar o evento de voo concluído.
/// Opera com zero alocação de memória no heap (GC Alloc = 0 bytes).
/// </summary>
public class ProcessarPousoFimVooCasoDeUso : IProcessarPousoFimVooCasoDeUso
{
    private readonly IPublicadorEventosVoo _publicadorEventos;
    private bool _eventoJaPublicado;

    /// <summary>
    /// Construtor que recebe o publicador de eventos de voo desacoplado via Inversão de Dependência.
    /// </summary>
    /// <param name="publicadorEventos">Instância do publicador de eventos.</param>
    /// <exception cref="DominioInvalidoException">Lançada se o publicador informado for nulo.</exception>
    public ProcessarPousoFimVooCasoDeUso(IPublicadorEventosVoo publicadorEventos)
    {
        _publicadorEventos = publicadorEventos ?? throw new DominioInvalidoException(
            nameof(publicadorEventos),
            "O publicador de eventos de voo não pode ser nulo.");
    }

    /// <summary>
    /// Avalia o estado físico da aeronave e a sessão de voo, realizando a transição para Pousado quando a aeronave atinge repouso no solo.
    /// </summary>
    /// <param name="voo">Entidade da sessão de voo em andamento.</param>
    /// <param name="estadoAtual">Estado físico cinemático instantâneo da aeronave.</param>
    /// <returns>ResultadoFimVoo imutável na stack contendo o status de encerramento e métricas finais.</returns>
    /// <exception cref="DominioInvalidoException">Lançada se a sessão de voo for nula.</exception>
    public ResultadoFimVoo Executar(Voo voo, EstadoFisicoAeronave estadoAtual)
    {
        if (voo == null)
        {
            throw new DominioInvalidoException(nameof(voo), "A sessão de voo não pode ser nula.");
        }

        // A parada total ocorre quando a aeronave está em contato com o solo e com velocidade Z nula
        var parouNoSolo = estadoAtual.NoSolo && estadoAtual.Velocidade.Z == 0f;

        if (parouNoSolo)
        {
            // Se o voo ainda estiver em andamento, consolida métricas e realiza o pouso
            if (voo.Status == StatusVoo.EmVoo)
            {
                voo.AtualizarMetricas(
                    MathF.Max(0f, estadoAtual.Posicao.Z),
                    MathF.Max(0f, estadoAtual.Posicao.Y),
                    0);

                var resultadoVoo = voo.Pousar();
                var resultadoFim = ResultadoFimVoo.CriarPousado(
                    voo.DistanciaPercorrida,
                    voo.AltitudeMaxima,
                    voo.MoedasColetadas,
                    resultadoVoo);

                if (!_eventoJaPublicado)
                {
                    _eventoJaPublicado = true;
                    _publicadorEventos.PublicarVooConcluido(resultadoFim);
                }

                return resultadoFim;
            }

            // Se o voo já estiver pousado (ex: transição anterior ou chamada idempotente)
            if (voo.Status == StatusVoo.Pousado && voo.Resultado != null)
            {
                var resultadoFim = ResultadoFimVoo.CriarPousado(
                    voo.DistanciaPercorrida,
                    voo.AltitudeMaxima,
                    voo.MoedasColetadas,
                    voo.Resultado);

                if (!_eventoJaPublicado)
                {
                    _eventoJaPublicado = true;
                    _publicadorEventos.PublicarVooConcluido(resultadoFim);
                }

                return resultadoFim;
            }
        }

        // Se o voo ainda estiver ativo (em voo livre ou deslizando em desaceleração no solo)
        if (voo.Status == StatusVoo.EmVoo)
        {
            voo.AtualizarMetricas(
                MathF.Max(0f, estadoAtual.Posicao.Z),
                MathF.Max(0f, estadoAtual.Posicao.Y),
                0);

            return ResultadoFimVoo.CriarEmAndamento(
                voo.DistanciaPercorrida,
                voo.AltitudeMaxima,
                voo.MoedasColetadas);
        }

        // Demais estados (EmPreparacao, Cancelado)
        return new ResultadoFimVoo(
            voo.Status,
            parouNoSolo,
            voo.DistanciaPercorrida,
            voo.AltitudeMaxima,
            voo.MoedasColetadas,
            voo.Resultado);
    }
}
