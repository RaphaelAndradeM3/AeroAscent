namespace AeroAscent.Core.Dominio.Comum;

using System;
using System.Collections.Generic;
using AeroAscent.Core.Dominio.Excecoes;

/// <summary>
/// Gerenciador genérico de pool de objetos com alta performance O(1), alocação zero no loop contínuo e expansão elástica de segurança.
/// </summary>
/// <typeparam name="T">Tipo da classe gerenciada no pool.</typeparam>
public class GerenciadorPoolObjetos<T> : IPoolObjetos<T> where T : class
{
    private readonly Stack<T> _itensDisponiveis;
    private readonly Func<T> _fabrica;
    private readonly Action<T>? _aoObter;
    private readonly Action<T>? _aoLiberar;
    private int _capacidadeTotal;

    /// <inheritdoc />
    public int CapacidadeTotal => _capacidadeTotal;

    /// <inheritdoc />
    public int DisponiveisEmEstoque => _itensDisponiveis.Count;

    /// <inheritdoc />
    public int EmUso => Math.Max(0, _capacidadeTotal - _itensDisponiveis.Count);

    /// <summary>
    /// Inicializa uma nova instância de GerenciadorPoolObjetos pré-alocando a capacidade solicitada.
    /// </summary>
    /// <param name="fabrica">Função geradora de novas instâncias de T.</param>
    /// <param name="capacidadeInicial">Quantidade de instâncias a serem pré-alocadas de imediato.</param>
    /// <param name="aoObter">Ação opcional executada ao retirar um objeto do pool (ex: reset de estado ou ativação).</param>
    /// <param name="aoLiberar">Ação opcional executada ao devolver um objeto ao pool (ex: desativação).</param>
    /// <exception cref="DominioInvalidoException">Lançada caso a fábrica seja nula ou a capacidade inicial seja negativa.</exception>
    public GerenciadorPoolObjetos(
        Func<T> fabrica,
        int capacidadeInicial,
        Action<T>? aoObter = null,
        Action<T>? aoLiberar = null)
    {
        if (fabrica == null)
        {
            throw new DominioInvalidoException(nameof(fabrica), "A fábrica de instâncias do pool não pode ser nula.");
        }

        if (capacidadeInicial < 0)
        {
            throw new DominioInvalidoException(nameof(capacidadeInicial), $"A capacidade inicial não pode ser negativa. Informado: {capacidadeInicial}.");
        }

        _fabrica = fabrica;
        _aoObter = aoObter;
        _aoLiberar = aoLiberar;
        _itensDisponiveis = new Stack<T>(capacidadeInicial);
        _capacidadeTotal = capacidadeInicial;

        for (var i = 0; i < capacidadeInicial; i++)
        {
            var item = _fabrica();
            _aoLiberar?.Invoke(item);
            _itensDisponiveis.Push(item);
        }
    }

    /// <inheritdoc />
    public T Obter()
    {
        T item;
        if (_itensDisponiveis.Count > 0)
        {
            item = _itensDisponiveis.Pop();
        }
        else
        {
            // Expansão elástica de segurança para suportar picos extremos sem crash
            item = _fabrica();
            _capacidadeTotal++;
        }

        _aoObter?.Invoke(item);
        return item;
    }

    /// <inheritdoc />
    public void Liberar(T item)
    {
        if (item == null)
        {
            throw new DominioInvalidoException(nameof(item), "Não é permitido devolver uma referência nula ao pool.");
        }

        _aoLiberar?.Invoke(item);
        _itensDisponiveis.Push(item);
    }

    /// <inheritdoc />
    public void Limpar()
    {
        _itensDisponiveis.Clear();
        _capacidadeTotal = 0;
    }
}
