namespace AeroAscent.Infraestrutura.Persistencia.Configuracao;

using System;
using System.IO;
using AeroAscent.Core.Dominio.Excecoes;

/// <summary>
/// Configuração que encapsula os caminhos e nomes de arquivos utilizados pelo repositório de persistência local em JSON.
/// Desacopla o sistema de arquivos da plataforma (Unity Windows / Android ou ambiente de testes).
/// </summary>
public sealed class ConfiguracaoPersistenciaLocal
{
    /// <summary>
    /// Nome canônico padrão do arquivo principal de progresso do jogador.
    /// </summary>
    public const string NOME_PADRAO_ARQUIVO_PRINCIPAL = "progresso.json";

    /// <summary>
    /// Nome canônico padrão do arquivo de backup de redundância.
    /// </summary>
    public const string NOME_PADRAO_ARQUIVO_BACKUP = "progresso.bak";

    /// <summary>
    /// Nome canônico padrão do arquivo temporário para escrita atômica.
    /// </summary>
    public const string NOME_PADRAO_ARQUIVO_TEMPORARIO = "progresso.tmp";

    /// <summary>
    /// Diretório base onde os arquivos de persistência serão lidos e gravados.
    /// </summary>
    public string DiretorioBase { get; }

    /// <summary>
    /// Nome do arquivo principal de dados.
    /// </summary>
    public string NomeArquivoPrincipal { get; }

    /// <summary>
    /// Nome do arquivo de backup redundante.
    /// </summary>
    public string NomeArquivoBackup { get; }

    /// <summary>
    /// Nome do arquivo temporário utilizado para gravação atômica.
    /// </summary>
    public string NomeArquivoTemporario { get; }

    /// <summary>
    /// Caminho físico absoluto completo do arquivo principal.
    /// </summary>
    public string CaminhoCompletoPrincipal => Path.Combine(DiretorioBase, NomeArquivoPrincipal);

    /// <summary>
    /// Caminho físico absoluto completo do arquivo de backup.
    /// </summary>
    public string CaminhoCompletoBackup => Path.Combine(DiretorioBase, NomeArquivoBackup);

    /// <summary>
    /// Caminho físico absoluto completo do arquivo temporário.
    /// </summary>
    public string CaminhoCompletoTemporario => Path.Combine(DiretorioBase, NomeArquivoTemporario);

    /// <summary>
    /// Construtor completo com validação de diretório.
    /// </summary>
    /// <param name="diretorioBase">Caminho da pasta base (ex: <c>Application.persistentDataPath</c>).</param>
    /// <param name="nomeArquivoPrincipal">Nome do arquivo principal (opcional).</param>
    /// <param name="nomeArquivoBackup">Nome do arquivo de backup (opcional).</param>
    /// <param name="nomeArquivoTemporario">Nome do arquivo temporário (opcional).</param>
    /// <exception cref="DominioInvalidoException">Lançada caso o diretório base seja nulo ou vazio.</exception>
    public ConfiguracaoPersistenciaLocal(
        string diretorioBase,
        string nomeArquivoPrincipal = NOME_PADRAO_ARQUIVO_PRINCIPAL,
        string nomeArquivoBackup = NOME_PADRAO_ARQUIVO_BACKUP,
        string nomeArquivoTemporario = NOME_PADRAO_ARQUIVO_TEMPORARIO)
    {
        if (string.IsNullOrWhiteSpace(diretorioBase))
        {
            throw new DominioInvalidoException(nameof(diretorioBase), "O diretório base de persistência não pode ser nulo ou vazio.");
        }

        DiretorioBase = diretorioBase;
        NomeArquivoPrincipal = string.IsNullOrWhiteSpace(nomeArquivoPrincipal) ? NOME_PADRAO_ARQUIVO_PRINCIPAL : nomeArquivoPrincipal;
        NomeArquivoBackup = string.IsNullOrWhiteSpace(nomeArquivoBackup) ? NOME_PADRAO_ARQUIVO_BACKUP : nomeArquivoBackup;
        NomeArquivoTemporario = string.IsNullOrWhiteSpace(nomeArquivoTemporario) ? NOME_PADRAO_ARQUIVO_TEMPORARIO : nomeArquivoTemporario;
    }
}
