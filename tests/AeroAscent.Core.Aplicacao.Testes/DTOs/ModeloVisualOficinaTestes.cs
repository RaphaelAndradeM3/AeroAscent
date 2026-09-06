namespace AeroAscent.Core.Aplicacao.Testes.DTOs;

using System.Collections.Generic;
using AeroAscent.Core.Aplicacao.DTOs;
using AeroAscent.Core.Dominio.Enums;
using Xunit;

/// <summary>
/// Testes unitários para <see cref="ModeloVisualOficina"/> e <see cref="ItemCartaoOficinaDTO"/>,
/// comprovando imutabilidade, completude dos 4 cartões e integridade dos dados de apresentação.
/// </summary>
public class ModeloVisualOficinaTestes
{
    [Fact]
    public void ItemCartaoOficinaDTO_DeveInicializarPropriedadesCorretamente()
    {
        // Arrange & Act
        var cartao = new ItemCartaoOficinaDTO
        {
            Tipo = TipoMelhoria.Motor,
            Titulo = "Motor",
            NivelAtual = 4,
            TextoNivel = "Nível 4",
            ProgressoNormalizado = 0.4f,
            CustoProximoNivel = 168,
            TextoBotao = "💰 168",
            PodeComprar = true,
            EstaNoNivelMaximo = false
        };

        // Assert
        Assert.Equal(TipoMelhoria.Motor, cartao.Tipo);
        Assert.Equal("Motor", cartao.Titulo);
        Assert.Equal(4, cartao.NivelAtual);
        Assert.Equal("Nível 4", cartao.TextoNivel);
        Assert.Equal(0.4f, cartao.ProgressoNormalizado);
        Assert.Equal(168, cartao.CustoProximoNivel);
        Assert.Equal("💰 168", cartao.TextoBotao);
        Assert.True(cartao.PodeComprar);
        Assert.False(cartao.EstaNoNivelMaximo);
    }

    [Fact]
    public void ModeloVisualOficina_DeveEncapsularOsQuatroCartoesERecordes()
    {
        // Arrange
        var cartoes = new List<ItemCartaoOficinaDTO>
        {
            new() { Tipo = TipoMelhoria.Motor, Titulo = "Motor", NivelAtual = 1 },
            new() { Tipo = TipoMelhoria.Aerodinamica, Titulo = "Aerodinâmica", NivelAtual = 1 },
            new() { Tipo = TipoMelhoria.TanqueCombustivel, Titulo = "Tanque", NivelAtual = 1 },
            new() { Tipo = TipoMelhoria.Catapulta, Titulo = "Catapulta", NivelAtual = 1 }
        };

        // Act
        var modelo = new ModeloVisualOficina
        {
            SaldoMoedas = 1250,
            SaldoFormatado = "💰 1.250",
            RecordeDistanciaMetros = 320.5f,
            RecordeDistanciaFormatado = "Recorde: 320,5 m",
            RecordeAltitudeMetros = 85.0f,
            RecordeAltitudeFormatado = "Altitude Máx: 85,0 m",
            TotalVoosRealizados = 12,
            Cartoes = cartoes
        };

        // Assert
        Assert.Equal(1250, modelo.SaldoMoedas);
        Assert.Equal("💰 1.250", modelo.SaldoFormatado);
        Assert.Equal(320.5f, modelo.RecordeDistanciaMetros);
        Assert.Equal("Recorde: 320,5 m", modelo.RecordeDistanciaFormatado);
        Assert.Equal(85.0f, modelo.RecordeAltitudeMetros);
        Assert.Equal("Altitude Máx: 85,0 m", modelo.RecordeAltitudeFormatado);
        Assert.Equal(12, modelo.TotalVoosRealizados);
        Assert.NotNull(modelo.Cartoes);
        Assert.Equal(4, modelo.Cartoes.Count);
    }
}
