namespace AeroAscent.Infraestrutura.Testes.DTOs;

using System;
using System.Text.Json;
using AeroAscent.Core.Dominio.Entidades;
using AeroAscent.Core.Dominio.Enums;
using AeroAscent.Core.Dominio.Excecoes;
using AeroAscent.Core.Dominio.ObjetosDeValor;
using AeroAscent.Infraestrutura.DTOs;
using Xunit;

/// <summary>
/// Testes unitários para <see cref="ProgressoJogadorDTO"/> cobrindo mapeamento bidirecional e serialização JSON.
/// </summary>
public class ProgressoJogadorDTOTestes
{
    [Fact]
    public void DoDominio_ComEntidadeValida_DeveMapearTodasAsPropriedadesEVersaoSchema1()
    {
        // Arrange
        var progresso = ProgressoJogador.CriarNovo();
        progresso.CreditarMoedas(new Moeda(1500));
        progresso.Aeronave.AtualizarNivel(TipoMelhoria.Motor, 3);
        progresso.Aeronave.AtualizarNivel(TipoMelhoria.Aerodinamica, 2);
        progresso.Aeronave.AtualizarNivel(TipoMelhoria.TanqueCombustivel, 4);
        progresso.Aeronave.AtualizarNivel(TipoMelhoria.Catapulta, 5);

        // Act
        var dto = ProgressoJogadorDTO.DoDominio(progresso);

        // Assert
        Assert.Equal(1, dto.VersaoSchema);
        Assert.True(dto.DataHoraSalvamentoUtc <= DateTime.UtcNow);
        Assert.Equal(progresso.Id, dto.Id);
        Assert.Equal(1500, dto.SaldoMoedas);
        Assert.Equal(3, dto.NivelMotor);
        Assert.Equal(2, dto.NivelAerodinamica);
        Assert.Equal(4, dto.NivelTanqueCombustivel);
        Assert.Equal(5, dto.NivelCatapulta);
        Assert.Equal(progresso.RecordeDistanciaMetros, dto.RecordeDistanciaMetros);
        Assert.Equal(progresso.RecordeAltitudeMetros, dto.RecordeAltitudeMetros);
        Assert.Equal(progresso.TotalVoosRealizados, dto.TotalVoosRealizados);
    }

    [Fact]
    public void ParaDominio_AposMapeamento_DeveRecriarEntidadeComValoresExatos()
    {
        // Arrange
        var id = Guid.NewGuid();
        var dto = new ProgressoJogadorDTO
        {
            VersaoSchema = 1,
            DataHoraSalvamentoUtc = DateTime.UtcNow,
            Id = id,
            SaldoMoedas = 800,
            NivelMotor = 2,
            NivelAerodinamica = 3,
            NivelTanqueCombustivel = 1,
            NivelCatapulta = 4,
            RecordeDistanciaMetros = 120.5f,
            RecordeAltitudeMetros = 45.2f,
            TotalVoosRealizados = 7
        };

        // Act
        var progresso = dto.ParaDominio();

        // Assert
        Assert.Equal(id, progresso.Id);
        Assert.Equal(800, progresso.SaldoMoedas.Quantidade);
        Assert.Equal(2, progresso.Aeronave.NivelMotor);
        Assert.Equal(3, progresso.Aeronave.NivelAerodinamica);
        Assert.Equal(1, progresso.Aeronave.NivelTanqueCombustivel);
        Assert.Equal(4, progresso.Aeronave.NivelCatapulta);
        Assert.Equal(120.5f, progresso.RecordeDistanciaMetros);
        Assert.Equal(45.2f, progresso.RecordeAltitudeMetros);
        Assert.Equal(7, progresso.TotalVoosRealizados);
    }

    [Fact]
    public void SerializacaoJson_DeveManterIntegridadeEmRoundtripTexto()
    {
        // Arrange
        var progressoOriginal = ProgressoJogador.CriarNovo();
        progressoOriginal.CreditarMoedas(new Moeda(250));
        progressoOriginal.Aeronave.AtualizarNivel(TipoMelhoria.Motor, 6);
        progressoOriginal.AtualizarConfiguracaoAudio(new ConfiguracaoAudio(0.4f, 0.3f, false, true));
        var dtoOriginal = ProgressoJogadorDTO.DoDominio(progressoOriginal);

        // Act
        string json = JsonSerializer.Serialize(dtoOriginal);
        var dtoDesserializado = JsonSerializer.Deserialize<ProgressoJogadorDTO>(json);

        // Assert
        Assert.Equal(dtoOriginal.VersaoSchema, dtoDesserializado.VersaoSchema);
        Assert.Equal(dtoOriginal.Id, dtoDesserializado.Id);
        Assert.Equal(dtoOriginal.SaldoMoedas, dtoDesserializado.SaldoMoedas);
        Assert.Equal(dtoOriginal.NivelMotor, dtoDesserializado.NivelMotor);
        Assert.Equal(0.4f, dtoDesserializado.VolumeEfeitos);
        Assert.Equal(0.3f, dtoDesserializado.VolumeMusica);
        Assert.False(dtoDesserializado.EfeitosAtivos);
        Assert.True(dtoDesserializado.MusicaAtiva);

        var progressoReconstituido = dtoDesserializado.ParaDominio();
        Assert.Equal(progressoOriginal.Id, progressoReconstituido.Id);
        Assert.Equal(250, progressoReconstituido.SaldoMoedas.Quantidade);
        Assert.Equal(6, progressoReconstituido.Aeronave.NivelMotor);
        Assert.Equal(0.4f, progressoReconstituido.ConfiguracaoAudio.VolumeEfeitos);
        Assert.Equal(0.3f, progressoReconstituido.ConfiguracaoAudio.VolumeMusica);
        Assert.False(progressoReconstituido.ConfiguracaoAudio.EfeitosAtivos);
        Assert.True(progressoReconstituido.ConfiguracaoAudio.MusicaAtiva);
    }

    [Fact]
    public void ParaDominio_QuandoCamposAudioForemNulos_DeveAplicarValoresPadraoDeConfiguracaoAudio()
    {
        // Arrange - Simulando JSON legado sem as chaves de áudio
        var dtoLegado = new ProgressoJogadorDTO
        {
            VersaoSchema = 1,
            DataHoraSalvamentoUtc = DateTime.UtcNow,
            Id = Guid.NewGuid(),
            SaldoMoedas = 100,
            NivelMotor = 1,
            NivelAerodinamica = 1,
            NivelTanqueCombustivel = 1,
            NivelCatapulta = 1,
            RecordeDistanciaMetros = 50f,
            RecordeAltitudeMetros = 20f,
            TotalVoosRealizados = 2,
            VolumeEfeitos = null,
            VolumeMusica = null,
            EfeitosAtivos = null,
            MusicaAtiva = null
        };

        // Act
        var progresso = dtoLegado.ParaDominio();

        // Assert - Deve adotar com segurança os padrões de ConfiguracaoAudio.Padrao
        Assert.Equal(ConfiguracaoAudio.Padrao.VolumeEfeitos, progresso.ConfiguracaoAudio.VolumeEfeitos);
        Assert.Equal(ConfiguracaoAudio.Padrao.VolumeMusica, progresso.ConfiguracaoAudio.VolumeMusica);
        Assert.Equal(ConfiguracaoAudio.Padrao.EfeitosAtivos, progresso.ConfiguracaoAudio.EfeitosAtivos);
        Assert.Equal(ConfiguracaoAudio.Padrao.MusicaAtiva, progresso.ConfiguracaoAudio.MusicaAtiva);
    }

    [Fact]
    public void DoDominio_ComEntidadeNula_DeveLancarDominioInvalidoException()
    {
        // Act & Assert
        Assert.Throws<DominioInvalidoException>(() => ProgressoJogadorDTO.DoDominio(null!));
    }
}
