namespace AeroAscent.Core.Dominio.Comum;

using System;

/// <summary>
/// Contrato genérico para gerenciamento de pools de objetos reutilizáveis em memória com zero alocação no loop de execução.
/// </summary>
/// <typeparam name="T">Tipo do objeto gerenciado no pool.</typeparam>
public interface IPoolObjetos<T> where T : class
{
    /// <summary>
    /// Capacidade total de instâncias criadas e alocadas sob gestão do pool.
    /// </summary>
    int CapacidadeTotal { get; }

    /// <summary>
    /// Quantidade de instâncias atualmente em repouso prontas para reutilização.
    /// </summary>
    int DisponiveisEmEstoque { get; }

    /// <summary>
    /// Quantidade de instâncias atualmente ativas e em uso na simulação.
    /// </summary>
    int EmUso { get; }

    /// <summary>
    /// Obtém uma instância do pool, ativando-a e disponibilizando-a para o jogo em O(1).
    /// </summary>
    /// <returns>Instância reutilizada de T.</returns>
    T Obter();

    /// <summary>
    /// Devolve uma instância utilizada de volta ao pool, desativando-a para reaproveitamento em O(1).
    /// </summary>
    /// <param name="item">Item a ser devolvido ao pool.</param>
    void Liberar(T item);

    /// <summary>
    /// Esvazia o estoque e limpa referências do pool.
    /// </summary>
    void Limpar();
}
