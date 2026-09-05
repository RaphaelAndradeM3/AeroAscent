namespace AeroAscent.Core.Aplicacao.Contratos;

using AeroAscent.Core.Dominio.Entidades;
using AeroAscent.Core.Dominio.ObjetosDeValor;

/// <summary>
/// Contrato do caso de uso para orquestrar o disparo e lançamento da aeronave pela catapulta.
/// </summary>
public interface ILancarAeronaveCasoDeUso
{
    /// <summary>
    /// Executa o procedimento de lançamento da aeronave aplicando os parâmetros de precisão do jogador.
    /// </summary>
    /// <param name="voo">Instância da sessão de voo em preparação.</param>
    /// <param name="parametros">Parâmetros de precisão e ângulo de lançamento.</param>
    /// <returns>Resultado contendo o vetor de velocidade inicial ou motivo da falha.</returns>
    ResultadoLancamento Executar(Voo voo, ParametrosLancamento parametros);

    /// <summary>
    /// Executa o procedimento de lançamento com base na amostragem instantânea de um medidor oscilante de força.
    /// </summary>
    /// <param name="voo">Instância da sessão de voo em preparação.</param>
    /// <param name="medidor">Medidor de força oscilante.</param>
    /// <param name="tempoSegundos">Tempo transcorrido no momento do toque do jogador.</param>
    /// <param name="anguloGraus">Ângulo de lançamento da rampa em graus.</param>
    /// <returns>Resultado contendo o vetor de velocidade inicial ou motivo da falha.</returns>
    ResultadoLancamento ExecutarComTemporizador(
        Voo voo,
        MedidorForcaOscilante medidor,
        float tempoSegundos,
        float anguloGraus = ParametrosLancamento.ANGULO_PADRAO_GRAUS);
}
