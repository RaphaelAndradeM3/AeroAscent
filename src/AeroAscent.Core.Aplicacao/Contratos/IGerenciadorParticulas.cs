namespace AeroAscent.Core.Aplicacao.Contratos;

using AeroAscent.Core.Dominio.ObjetosDeValor;

/// <summary>
/// Contrato desacoplado para gerenciamento e disparo de emissores de partículas e feedback visual no jogo,
/// operando sob Object Pooling na camada de apresentação e garantindo zero alocação de memória no heap.
/// </summary>
public interface IGerenciadorParticulas
{
    /// <summary>
    /// Define o estado e a intensidade do rastro contínuo de fumaça da cauda da aeronave.
    /// </summary>
    /// <param name="ativo">Verdadeiro se o rastro deve ser emitido; falso para pausar a emissão.</param>
    /// <param name="intensidade">Intensidade normalizada entre 0.0 e 1.0.</param>
    void DefinirRastroCauda(bool ativo, float intensidade);

    /// <summary>
    /// Define o estado e a intensidade da emissão contínua de chamas e fumaça do propulsor (boost).
    /// </summary>
    /// <param name="ativo">Verdadeiro se as chamas de propulsão devem ser emitidas; falso caso contrário.</param>
    /// <param name="intensidade">Intensidade normalizada entre 0.0 e 1.0.</param>
    void DefinirPropulsao(bool ativo, float intensidade);

    /// <summary>
    /// Dispara uma emissão pontual de brilho e partículas cintilantes na posição de coleta de uma moeda.
    /// </summary>
    /// <param name="posicao">Posição tridimensional alocada na stack onde a moeda foi coletada.</param>
    void EmitirColetaMoeda(VetorVoo posicao);

    /// <summary>
    /// Dispara uma emissão pontual de partículas de combustível na posição de coleta do galão.
    /// </summary>
    /// <param name="posicao">Posição tridimensional alocada na stack onde o combustível foi coletado.</param>
    void EmitirColetaCombustivel(VetorVoo posicao);

    /// <summary>
    /// Dispara uma explosão festiva de confetes coloridos em celebração de novo recorde ou marco alcançado.
    /// </summary>
    /// <param name="posicao">Posição tridimensional da celebração.</param>
    void EmitirCelebracaoRecorde(VetorVoo posicao);

    /// <summary>
    /// Dispara uma nuvem pontual de poeira e partículas de impacto no momento da aterrissagem ou colisão com o solo.
    /// </summary>
    /// <param name="posicao">Posição tridimensional do impacto.</param>
    void EmitirImpacto(VetorVoo posicao);

    /// <summary>
    /// Interrompe imediatamente todos os sistemas de partículas ativos e retorna as partículas aos seus respectivos pools de objetos.
    /// </summary>
    void PararTodosOsEfeitos();
}
