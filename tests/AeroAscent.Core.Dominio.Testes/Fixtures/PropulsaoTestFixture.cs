namespace AeroAscent.Core.Dominio.Testes.Fixtures;

using System;
using AeroAscent.Core.Dominio.Entidades;
using AeroAscent.Core.Dominio.ObjetosDeValor;

/// <summary>
/// Provedor de utilitários e instâncias de apoio para os testes de propulsão, boost e combustível.
/// </summary>
public static class PropulsaoTestFixture
{
    /// <summary>
    /// Cria uma instância válida de aeronave para testes com os níveis de melhoria especificados.
    /// </summary>
    /// <param name="nivelMotor">Nível de melhoria do motor (1 a 10).</param>
    /// <param name="nivelTanque">Nível de melhoria do tanque de combustível (1 a 10).</param>
    /// <param name="nivelAerodinamica">Nível de melhoria aerodinâmica (1 a 10).</param>
    /// <param name="nivelCatapulta">Nível de melhoria da catapulta (1 a 10).</param>
    /// <returns>Instância de Aeronave configurada.</returns>
    public static Aeronave CriarAeronave(
        int nivelMotor = 1,
        int nivelTanque = 1,
        int nivelAerodinamica = 1,
        int nivelCatapulta = 1)
    {
        return new Aeronave(
            Guid.NewGuid(),
            nivelMotor,
            nivelAerodinamica,
            nivelTanque,
            nivelCatapulta);
    }

    /// <summary>
    /// Cria uma sessão de voo no status ativo EmVoo pronta para receber comandos de propulsão.
    /// </summary>
    /// <param name="aeronave">Aeronave opcional. Se nula, utiliza configuração padrão nível 1.</param>
    /// <returns>Sessão de Voo ativa.</returns>
    public static Voo CriarVooAtivo(Aeronave? aeronave = null)
    {
        var aero = aeronave ?? CriarAeronave();
        var voo = Voo.Iniciar(aero);
        voo.Decolar();
        return voo;
    }

    /// <summary>
    /// Cria uma instância de combustível totalmente abastecida para cenários de teste.
    /// </summary>
    /// <param name="capacidade">Capacidade máxima volumétrica (padrão 20.0 un).</param>
    /// <param name="taxaConsumo">Taxa de consumo por segundo (padrão 5.0 un/s).</param>
    /// <returns>Objeto de valor Combustivel cheio.</returns>
    public static Combustivel CriarCombustivelCheio(float capacidade = 20.0f, float taxaConsumo = 5.0f)
    {
        return Combustivel.CriarCheio(capacidade, taxaConsumo);
    }
}
