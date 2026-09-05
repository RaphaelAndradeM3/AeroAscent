namespace AeroAscent.Core.Dominio.Contratos;

using System.Collections.Generic;
using AeroAscent.Core.Dominio.Comum;
using AeroAscent.Core.Dominio.Entidades;

/// <summary>
/// Contrato do serviço de geração procedural de coletáveis (moedas e anéis de vento) ao longo da trajetória de voo da aeronave.
/// </summary>
public interface IServicoGeracaoProceduralColetaveis
{
    /// <summary>
    /// Semente pseudo-randômica utilizada para garantir reprodutibilidade determinística na geração da pista.
    /// </summary>
    int Semente { get; }

    /// <summary>
    /// Atualiza a janela espacial ativa de coletáveis à frente da aeronave, spawnando novos itens e reciclando os que ficaram para trás.
    /// </summary>
    /// <param name="posicaoZAeronave">Posição longitudinal atual Z da aeronave em metros.</param>
    /// <param name="poolMoedas">Pool gerenciador de moedas.</param>
    /// <param name="poolAneis">Pool gerenciador de anéis de vento.</param>
    /// <param name="coletaveisAtivos">Lista de instâncias atualmente ativas em tela.</param>
    void AtualizarJanela(
        float posicaoZAeronave,
        IPoolObjetos<Coletavel> poolMoedas,
        IPoolObjetos<Coletavel> poolAneis,
        IList<Coletavel> coletaveisAtivos);

    /// <summary>
    /// Reseta o gerador procedural para o início da pista de voo.
    /// </summary>
    void Reiniciar();
}
