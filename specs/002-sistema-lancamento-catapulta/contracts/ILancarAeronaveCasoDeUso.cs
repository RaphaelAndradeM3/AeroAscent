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
}
