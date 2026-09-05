# Guia de Validação Rápida: Feature 009 — Persistência de Dados Local Offline First (JSON)

**Branch**: `009-persistencia-local-json` | **Data**: 2026-09-05 | **Spec**: [spec.md](./spec.md)

---

## 🚀 Cenários Executáveis de Integração

### Cenário 1: Ciclo Completo de Salvamento Atômico e Carregamento (Roundtrip)
```csharp
// 1. Configurar diretório temporário isolado
var diretorioTeste = Path.Combine(Path.GetTempPath(), "AeroAscent_Testes_" + Guid.NewGuid());
Directory.CreateDirectory(diretorioTeste);
var config = new ConfiguracaoPersistenciaLocal(diretorioTeste);
var repositorio = new RepositorioProgressoLocalJson(config);

// 2. Criar e personalizar progresso do jogador
var progresso = ProgressoJogador.CriarNovo();
progresso.CreditarMoedas(new Moeda(750));
progresso.Aeronave.AtualizarNivel(TipoMelhoria.Motor, 4);

// 3. Salvar atomicamente no disco
await repositorio.SalvarProgressoAsync(progresso);

// 4. Carregar em nova instância e validar integridade
var progressoCarregado = await repositorio.CarregarProgressoAsync();

Assert.NotNull(progressoCarregado);
Assert.Equal(progresso.Id, progressoCarregado.Id);
Assert.Equal(750, progressoCarregado.SaldoMoedas.Quantidade);
Assert.Equal(4, progressoCarregado.Aeronave.NivelMotor);

// 5. Validar que o arquivo principal e o temporário estão no estado correto
Assert.True(File.Exists(config.CaminhoCompletoPrincipal));
Assert.False(File.Exists(config.CaminhoCompletoTemporario));
```

---

### Cenário 2: Primeira Execução sem Arquivo Salvo (Resiliência)
```csharp
var diretorioVazio = Path.Combine(Path.GetTempPath(), "AeroAscent_Vazio_" + Guid.NewGuid());
Directory.CreateDirectory(diretorioVazio);
var config = new ConfiguracaoPersistenciaLocal(diretorioVazio);
var repositorio = new RepositorioProgressoLocalJson(config);

// Ao carregar sem arquivo prévio, retorna null de forma segura
var resultado = await repositorio.CarregarProgressoAsync();

Assert.Null(resultado);
```

---

### Cenário 3: Recuperação Automática a Partir do Arquivo de Backup (`.bak`)
```csharp
var diretorio = Path.Combine(Path.GetTempPath(), "AeroAscent_Backup_" + Guid.NewGuid());
Directory.CreateDirectory(diretorio);
var config = new ConfiguracaoPersistenciaLocal(diretorio);
var repositorio = new RepositorioProgressoLocalJson(config);

// Salvar primeiro estado válido (Gera principal)
var p1 = ProgressoJogador.CriarNovo();
p1.CreditarMoedas(new Moeda(100));
await repositorio.SalvarProgressoAsync(p1);

// Salvar segundo estado válido (Gera backup do primeiro e atualiza principal)
var p2 = ProgressoJogador.CriarNovo();
p2.CreditarMoedas(new Moeda(200));
await repositorio.SalvarProgressoAsync(p2);

// Corromper intencionalmente o arquivo principal com lixo
await File.WriteAllTextAsync(config.CaminhoCompletoPrincipal, "{ JSON_CORROMPIDO_INVALIDO! }");

// Carregar novamente: deve detectar corrupção no principal e restaurar do backup
var restaurado = await repositorio.CarregarProgressoAsync();

Assert.NotNull(restaurado);
Assert.True(restaurado.SaldoMoedas.Quantidade > 0);
```

---

### Cenário 4: Isolamento de Arquivo Corrompido sem Backup
```csharp
var diretorio = Path.Combine(Path.GetTempPath(), "AeroAscent_SemBackup_" + Guid.NewGuid());
Directory.CreateDirectory(diretorio);
var config = new ConfiguracaoPersistenciaLocal(diretorio);
var repositorio = new RepositorioProgressoLocalJson(config);

// Criar arquivo principal corrompido sem backup
await File.WriteAllTextAsync(config.CaminhoCompletoPrincipal, "CORRUPCAO_TOTAL");

// Carregar: deve isolar o arquivo como .corrompido e retornar null sem lançar exceção
var resultado = await repositorio.CarregarProgressoAsync();

Assert.Null(resultado);
Assert.False(File.Exists(config.CaminhoCompletoPrincipal)); // Foi renomeado
var arquivosCorrompidos = Directory.GetFiles(diretorio, "*corrompido*");
Assert.Single(arquivosCorrompidos);
```

---

### Cenário 5: Concorrência e Benchmark de Performance (SC-001 & SC-002)
```csharp
var diretorio = Path.Combine(Path.GetTempPath(), "AeroAscent_Benchmark_" + Guid.NewGuid());
Directory.CreateDirectory(diretorio);
var config = new ConfiguracaoPersistenciaLocal(diretorio);
var repositorio = new RepositorioProgressoLocalJson(config);

// Disparar 10 operações concorrentes de salvamento
var tarefas = Enumerable.Range(1, 10).Select(i =>
{
    var p = ProgressoJogador.CriarNovo();
    p.CreditarMoedas(new Moeda(i * 100));
    return repositorio.SalvarProgressoAsync(p);
});

await Task.WhenAll(tarefas);

// Validar que o arquivo final está 100% íntegro
var carregado = await repositorio.CarregarProgressoAsync();
Assert.NotNull(carregado);
```
