namespace AeroAscent.Infraestrutura.Persistencia;

using System;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using AeroAscent.Core.Dominio.Contratos;
using AeroAscent.Core.Dominio.Entidades;
using AeroAscent.Core.Dominio.Excecoes;
using AeroAscent.Infraestrutura.Configuracao;
using AeroAscent.Infraestrutura.DTOs;

/// <summary>
/// Implementação concreta da interface <see cref="IRepositorioProgresso"/> na camada de Infraestrutura,
/// responsável pelo salvamento e carregamento atômico local do progresso do jogador em formato JSON (Offline First),
/// com tolerância a falhas, rotação de backup e sincronização assíncrona contra concorrência.
/// </summary>
public sealed class RepositorioProgressoLocalJson : IRepositorioProgresso, IDisposable
{
    private static readonly JsonSerializerOptions OpcoesSerializacao = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true
    };

    private readonly ConfiguracaoPersistenciaLocal _configuracao;
    private readonly SemaphoreSlim _semaforo = new(1, 1);
    private bool _descartado;

    /// <summary>
    /// Construtor com injeção obrigatória das configurações de persistência local.
    /// </summary>
    /// <param name="configuracao">Configurações de diretórios e arquivos.</param>
    /// <exception cref="DominioInvalidoException">Lançada se a configuração for nula.</exception>
    public RepositorioProgressoLocalJson(ConfiguracaoPersistenciaLocal configuracao)
    {
        _configuracao = configuracao ?? throw new DominioInvalidoException(
            nameof(configuracao),
            "A configuração de persistência local não pode ser nula.");
    }

    /// <inheritdoc />
    public async Task SalvarProgressoAsync(ProgressoJogador progresso, CancellationToken cancelamento = default)
    {
        cancelamento.ThrowIfCancellationRequested();

        if (progresso == null)
        {
            throw new DominioInvalidoException(nameof(progresso), "A entidade de progresso a ser salva não pode ser nula.");
        }

        await _semaforo.WaitAsync(cancelamento).ConfigureAwait(false);
        try
        {
            cancelamento.ThrowIfCancellationRequested();

            if (!Directory.Exists(_configuracao.DiretorioBase))
            {
                Directory.CreateDirectory(_configuracao.DiretorioBase);
            }

            var dto = ProgressoJogadorDTO.DoDominio(progresso);

            // 1. Gravação primária no arquivo temporário (.tmp)
            await using (var streamTemporario = new FileStream(
                _configuracao.CaminhoCompletoTemporario,
                FileMode.Create,
                FileAccess.Write,
                FileShare.None,
                bufferSize: 4096,
                useAsync: true))
            {
                await JsonSerializer.SerializeAsync(streamTemporario, dto, OpcoesSerializacao, cancelamento).ConfigureAwait(false);
                await streamTemporario.FlushAsync(cancelamento).ConfigureAwait(false);
            }

            // 2. Se o arquivo principal já existe, cria/atualiza o backup (.bak)
            if (File.Exists(_configuracao.CaminhoCompletoPrincipal))
            {
                File.Copy(_configuracao.CaminhoCompletoPrincipal, _configuracao.CaminhoCompletoBackup, overwrite: true);
            }

            // 3. Substituição atômica: promove o arquivo temporário para principal
            MoverArquivoComSobrescrita(_configuracao.CaminhoCompletoTemporario, _configuracao.CaminhoCompletoPrincipal);
        }
        finally
        {
            _semaforo.Release();
        }
    }

    /// <inheritdoc />
    public async Task<ProgressoJogador?> CarregarProgressoAsync(CancellationToken cancelamento = default)
    {
        cancelamento.ThrowIfCancellationRequested();

        await _semaforo.WaitAsync(cancelamento).ConfigureAwait(false);
        try
        {
            cancelamento.ThrowIfCancellationRequested();

            // 1. Se o arquivo principal não existir, tenta o backup ou retorna null (1ª execução)
            if (!File.Exists(_configuracao.CaminhoCompletoPrincipal))
            {
                if (File.Exists(_configuracao.CaminhoCompletoBackup))
                {
                    return await TentarCarregarArquivoAsync(_configuracao.CaminhoCompletoBackup, cancelamento).ConfigureAwait(false);
                }

                return null;
            }

            // 2. Tenta carregar o arquivo principal
            try
            {
                var progresso = await TentarCarregarArquivoAsync(_configuracao.CaminhoCompletoPrincipal, cancelamento).ConfigureAwait(false);
                if (progresso != null)
                {
                    return progresso;
                }
            }
            catch (Exception)
            {
                // Falha de leitura ou JSON mutilado no principal -> aciona plano de recuperação
            }

            // 3. Plano de contingência: tentar recuperar a partir do backup (.bak)
            if (File.Exists(_configuracao.CaminhoCompletoBackup))
            {
                try
                {
                    var progressoDoBackup = await TentarCarregarArquivoAsync(_configuracao.CaminhoCompletoBackup, cancelamento).ConfigureAwait(false);
                    if (progressoDoBackup != null)
                    {
                        // Restaura o backup como arquivo principal para as próximas leituras
                        File.Copy(_configuracao.CaminhoCompletoBackup, _configuracao.CaminhoCompletoPrincipal, overwrite: true);
                        return progressoDoBackup;
                    }
                }
                catch (Exception)
                {
                    // Backup também danificado
                }
            }

            // 4. Se o arquivo principal está corrompido e não há backup recuperável, isola o corrompido
            IsolarArquivoCorrompido();

            return null;
        }
        finally
        {
            _semaforo.Release();
        }
    }

    private async Task<ProgressoJogador?> TentarCarregarArquivoAsync(string caminhoArquivo, CancellationToken cancelamento)
    {
        await using var streamLeitura = new FileStream(
            caminhoArquivo,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            bufferSize: 4096,
            useAsync: true);

        var dto = await JsonSerializer.DeserializeAsync<ProgressoJogadorDTO>(streamLeitura, OpcoesSerializacao, cancelamento).ConfigureAwait(false);

        if (dto.VersaoSchema <= 0 || dto.Id == Guid.Empty)
        {
            throw new DominioInvalidoException(nameof(dto), "Os dados desserializados contêm schema ou identificador inválido.");
        }

        return dto.ParaDominio();
    }

    private void IsolarArquivoCorrompido()
    {
        try
        {
            if (File.Exists(_configuracao.CaminhoCompletoPrincipal))
            {
                string sufixo = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
                string caminhoCorrompido = Path.Combine(_configuracao.DiretorioBase, $"progresso.corrompido_{sufixo}");
                MoverArquivoComSobrescrita(_configuracao.CaminhoCompletoPrincipal, caminhoCorrompido);
            }
        }
        catch
        {
            // Proteção contra falhas secundárias de isolamento
        }
    }

    private static void MoverArquivoComSobrescrita(string origem, string destino)
    {
#if NETSTANDARD2_1
        if (File.Exists(destino))
        {
            File.Delete(destino);
        }
        File.Move(origem, destino);
#else
        File.Move(origem, destino, overwrite: true);
#endif
    }

    /// <summary>
    /// Libera os recursos gerenciados do semáforo assíncrono.
    /// </summary>
    public void Dispose()
    {
        if (!_descartado)
        {
            _semaforo.Dispose();
            _descartado = true;
        }
    }
}
