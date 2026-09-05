namespace AeroAscent.Core.Aplicacao.CasosDeUso;

using System;
using System.Collections.Generic;
using AeroAscent.Core.Aplicacao.Contratos;
using AeroAscent.Core.Dominio.Comum;
using AeroAscent.Core.Dominio.Entidades;
using AeroAscent.Core.Dominio.Enums;
using AeroAscent.Core.Dominio.Excecoes;
using AeroAscent.Core.Dominio.ObjetosDeValor;

/// <summary>
/// Caso de uso de aplicação responsável por processar a detecção e interação com coletáveis em voo,
/// creditando moedas na entidade Voo, aplicando impulsos de vento e gerenciando a reciclagem no pool.
/// </summary>
public class ProcessarColetaveisVooCasoDeUso : IProcessarColetaveisVooCasoDeUso
{
    /// <summary>
    /// Magnitude escalar do impulso instantâneo conferido por um anel de vento em metros por segundo (+10.0 m/s).
    /// </summary>
    public const float IMPULSO_ANEL_VENTO_METROS_POR_SEGUNDO = 10.0f;

    /// <summary>
    /// Raio de colisão padrão da fuselagem da aeronave em metros (0.5m).
    /// </summary>
    public const float RAIO_COLISAO_AERONAVE_METROS = 0.5f;

    /// <inheritdoc />
    public ResultadoProcessamentoColetaveis Executar(
        Voo voo,
        EstadoFisicoAeronave estadoAtual,
        IList<Coletavel> coletaveisAtivos,
        IPoolObjetos<Coletavel> poolMoedas,
        IPoolObjetos<Coletavel> poolAneis)
    {
        if (voo == null)
        {
            throw new DominioInvalidoException(nameof(voo), "A sessão de voo não pode ser nula.");
        }

        if (coletaveisAtivos == null)
        {
            throw new DominioInvalidoException(nameof(coletaveisAtivos), "A lista de coletáveis ativos não pode ser nula.");
        }

        if (poolMoedas == null)
        {
            throw new DominioInvalidoException(nameof(poolMoedas), "O pool de moedas não pode ser nulo.");
        }

        if (poolAneis == null)
        {
            throw new DominioInvalidoException(nameof(poolAneis), "O pool de anéis não pode ser nulo.");
        }

        // Se o voo não está ativo ou a aeronave está no solo, não há interação aérea com coletáveis
        if (voo.Status != StatusVoo.EmVoo || estadoAtual.NoSolo)
        {
            return ResultadoProcessamentoColetaveis.CriarNeutro(estadoAtual);
        }

        var moedasColetadas = 0;
        var recebeuImpulsoVento = false;
        var impulsoTotal = VetorVoo.Zero;

        // Itera de trás para frente para permitir remoção segura da lista de ativos em O(1)
        for (var i = coletaveisAtivos.Count - 1; i >= 0; i--)
        {
            var coletavel = coletaveisAtivos[i];
            if (coletavel == null || !coletavel.Ativo || coletavel.Coletado)
            {
                continue;
            }

            if (coletavel.VerificarColisao(estadoAtual.Posicao, RAIO_COLISAO_AERONAVE_METROS))
            {
                if (coletavel.Tipo == TipoColetavel.Moeda)
                {
                    moedasColetadas++;
                    coletavel.MarcarColetado();
                    coletaveisAtivos.RemoveAt(i);
                    poolMoedas.Liberar(coletavel);
                }
                else if (coletavel.Tipo == TipoColetavel.AnelVento)
                {
                    recebeuImpulsoVento = true;

                    // Calcula vetor unitário de direção da velocidade ou alinha ao pitch se velocidade for muito baixa
                    VetorVoo direcaoImpulso;
                    var velEscalar = estadoAtual.VelocidadeEscalar;
                    if (velEscalar >= 0.5f)
                    {
                        direcaoImpulso = new VetorVoo(
                            0f,
                            estadoAtual.Velocidade.Y / velEscalar,
                            estadoAtual.Velocidade.Z / velEscalar);
                    }
                    else
                    {
                        var pitchRad = estadoAtual.InclinacaoPitchGraus * MathF.PI / 180.0f;
                        direcaoImpulso = new VetorVoo(0f, MathF.Sin(pitchRad), MathF.Cos(pitchRad));
                    }

                    var impulsoAnel = new VetorVoo(
                        0f,
                        direcaoImpulso.Y * IMPULSO_ANEL_VENTO_METROS_POR_SEGUNDO,
                        direcaoImpulso.Z * IMPULSO_ANEL_VENTO_METROS_POR_SEGUNDO);

                    impulsoTotal = new VetorVoo(
                        0f,
                        impulsoTotal.Y + impulsoAnel.Y,
                        impulsoTotal.Z + impulsoAnel.Z);

                    coletavel.MarcarColetado();
                    coletaveisAtivos.RemoveAt(i);
                    poolAneis.Liberar(coletavel);
                }
            }
        }

        // Consolida moedas novas capturadas na entidade Voo
        if (moedasColetadas > 0)
        {
            voo.AtualizarMetricas(voo.DistanciaPercorrida, voo.AltitudeMaxima, moedasColetadas);
        }

        // Aplica o impulso cinemático no estado físico da aeronave
        var estadoFinal = estadoAtual;
        if (recebeuImpulsoVento)
        {
            var novaVelocidade = new VetorVoo(
                0f,
                estadoAtual.Velocidade.Y + impulsoTotal.Y,
                estadoAtual.Velocidade.Z + impulsoTotal.Z);

            estadoFinal = estadoAtual.ComAtualizacao(
                estadoAtual.Posicao,
                novaVelocidade,
                estadoAtual.InclinacaoPitchGraus,
                estadoAtual.ForcaResultante,
                estadoAtual.NoSolo,
                estadoAtual.Propulsor);
        }

        return new ResultadoProcessamentoColetaveis(moedasColetadas, recebeuImpulsoVento, impulsoTotal, estadoFinal);
    }
}
