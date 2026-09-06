namespace AeroAscent.Apresentacao.MAUI.Servicos;

using System;
using AeroAscent.Core.Dominio.Contratos;
using AeroAscent.Core.Dominio.ObjetosDeValor;

/// <summary>
/// Publicador de eventos de ciclo de vida de voo da apresentação em .NET MAUI.
/// Permite que a interface e os apresentadores reajam ao pouso sem acoplamento.
/// </summary>
public sealed class PublicadorEventosVooMAUI : IPublicadorEventosVoo
{
    /// <summary>
    /// Evento acionado quando um voo é finalizado formalmente.
    /// </summary>
    public event Action<ResultadoFimVoo>? AoConcluirVoo;

    /// <inheritdoc />
    public void PublicarVooConcluido(ResultadoFimVoo resultado)
    {
        AoConcluirVoo?.Invoke(resultado);
    }
}
