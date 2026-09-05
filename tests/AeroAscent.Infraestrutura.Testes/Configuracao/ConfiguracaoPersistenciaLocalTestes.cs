namespace AeroAscent.Infraestrutura.Testes.Configuracao;

using System.IO;
using AeroAscent.Core.Dominio.Excecoes;
using AeroAscent.Infraestrutura.Configuracao;
using Xunit;

/// <summary>
/// Testes unitários para <see cref="ConfiguracaoPersistenciaLocal"/>.
/// </summary>
public class ConfiguracaoPersistenciaLocalTestes
{
    [Fact]
    public void Construtor_ComDiretorioValido_DeveDefinirCaminhosPadroes()
    {
        // Arrange
        var diretorio = @"C:\Jogos\AeroAscent";

        // Act
        var config = new ConfiguracaoPersistenciaLocal(diretorio);

        // Assert
        Assert.Equal(diretorio, config.DiretorioBase);
        Assert.Equal("progresso.json", config.NomeArquivoPrincipal);
        Assert.Equal("progresso.bak", config.NomeArquivoBackup);
        Assert.Equal("progresso.tmp", config.NomeArquivoTemporario);

        Assert.Equal(Path.Combine(diretorio, "progresso.json"), config.CaminhoCompletoPrincipal);
        Assert.Equal(Path.Combine(diretorio, "progresso.bak"), config.CaminhoCompletoBackup);
        Assert.Equal(Path.Combine(diretorio, "progresso.tmp"), config.CaminhoCompletoTemporario);
    }

    [Fact]
    public void Construtor_ComNomesCustomizados_DeveRespeitarParametros()
    {
        // Arrange
        var diretorio = @"/dados/usuario";
        var principal = "save.json";
        var backup = "save.backup";
        var temp = "save.temp";

        // Act
        var config = new ConfiguracaoPersistenciaLocal(diretorio, principal, backup, temp);

        // Assert
        Assert.Equal(principal, config.NomeArquivoPrincipal);
        Assert.Equal(backup, config.NomeArquivoBackup);
        Assert.Equal(temp, config.NomeArquivoTemporario);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Construtor_ComDiretorioInvalido_DeveLancarDominioInvalidoException(string? diretorioInvalido)
    {
        // Act & Assert
        Assert.Throws<DominioInvalidoException>(() => new ConfiguracaoPersistenciaLocal(diretorioInvalido!));
    }
}
