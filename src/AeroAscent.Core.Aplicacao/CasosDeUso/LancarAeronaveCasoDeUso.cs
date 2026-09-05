namespace AeroAscent.Core.Aplicacao.CasosDeUso;

using AeroAscent.Core.Aplicacao.Contratos;
using AeroAscent.Core.Dominio.Contratos;
using AeroAscent.Core.Dominio.Entidades;
using AeroAscent.Core.Dominio.Enums;
using AeroAscent.Core.Dominio.Excecoes;
using AeroAscent.Core.Dominio.ObjetosDeValor;

/// <summary>
/// Caso de uso que orquestra o procedimento de lançamento da aeronave pela catapulta,
/// validando o estado do voo, calculando o impulso cinemático 3D e transitando o voo para EmVoo.
/// </summary>
public class LancarAeronaveCasoDeUso : ILancarAeronaveCasoDeUso
{
    private readonly IServicoFisicaVoo _servicoFisica;

    /// <summary>
    /// Inicializa uma nova instância do caso de uso de lançamento com injeção do serviço de física.
    /// </summary>
    /// <param name="servicoFisica">Serviço de cálculo cinemático e aerodinâmico.</param>
    /// <exception cref="DominioInvalidoException">Lançada se o serviço de física for nulo.</exception>
    public LancarAeronaveCasoDeUso(IServicoFisicaVoo servicoFisica)
    {
        _servicoFisica = servicoFisica ?? throw new DominioInvalidoException(nameof(servicoFisica), "O serviço de física de voo não pode ser nulo.");
    }

    /// <summary>
    /// Executa o procedimento de lançamento da aeronave aplicando os parâmetros informados pelo jogador.
    /// </summary>
    /// <param name="voo">Instância da sessão de voo em preparação.</param>
    /// <param name="parametros">Parâmetros de precisão e ângulo de lançamento.</param>
    /// <returns>Resultado contendo o vetor de velocidade inicial ou motivo da falha.</returns>
    /// <exception cref="DominioInvalidoException">Lançada caso a sessão de voo informada seja nula.</exception>
    public ResultadoLancamento Executar(Voo voo, ParametrosLancamento parametros)
    {
        if (voo == null)
        {
            throw new DominioInvalidoException(nameof(voo), "A sessão de voo não pode ser nula.");
        }

        if (voo.Status != StatusVoo.EmPreparacao)
        {
            return ResultadoLancamento.CriarFalha(
                $"Decolagem recusada. O voo deve estar em 'EmPreparacao', mas seu status atual é '{voo.Status}'.");
        }

        var nivelCatapulta = voo.Aeronave.NivelCatapulta;
        var velocidadeInicial = _servicoFisica.CalcularImpulsoInicial(nivelCatapulta, parametros.PrecisaoEfetiva);

        voo.Decolar();

        return ResultadoLancamento.CriarSucesso(velocidadeInicial);
    }
}
