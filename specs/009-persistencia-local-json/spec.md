# Feature Specification: Persistência de Dados Local Offline First (JSON)

**Feature Branch**: `009-persistencia-local-json`  
**Created**: 2026-09-04  
**Status**: Ready for Planning  
**Input**: User description: "009 - Camada de Infraestrutura para Salvamento/Carregamento Atômico de Progresso Local em JSON (Offline-first)."

---

## Clarifications

### Session 2026-09-05
- Q: Como o método CarregarProgressoAsync deve se comportar quando o arquivo de salvamento JSON ainda não existir no disco (como na primeira execução do jogo)? → A: Opção A — Retornar null se o arquivo não existir, respeitando a assinatura nullable da interface de domínio Task<ProgressoJogador?> e permitindo que as camadas superiores instanciem ProgressoJogador.CriarNovo() de forma resiliente e idempotente.
- Q: Qual mecanismo de I/O de arquivo deve ser adotado para garantir gravação atômica e prevenção de corrupção em plataformas Windows e Android? → A: Opção A — Gravar primeiramente no arquivo temporário (.tmp), rotacionar o backup (.bak) e aplicar substituição atômica no arquivo principal (.json) via File.Move(caminhoTmp, caminhoPrincipal, overwrite: true), prevenindo perda de dados mesmo em interrupções abruptas.
- Q: Como o repositório deve proceder se o arquivo JSON principal estiver corrompido ou contiver dados inválidos durante o carregamento? → A: Opção A — Tentar restauração transparente a partir do arquivo de backup (.bak); se o backup for válido, restaurá-lo como principal; se o backup também estiver corrompido ou inexistente, isolar o arquivo danificado renomeando-o com sufixo .corrompido_[timestamp] e retornar null para permitir inicialização resiliente sem crash.
- Q: Como o repositório deve gerenciar chamadas concorrentes assíncronas de salvamento e carregamento para evitar bloqueios de arquivo e exceções de acesso compartilhado (IOException)? → A: Opção A — Utilizar um semáforo assíncrono interno SemaphoreSlim(1, 1) para sincronizar e enfileirar as operações de I/O em disco com liberação garantida no bloco finally, prevenindo colisões e travamentos de thread.
- Q: Como o DTO de persistência (ProgressoJogadorDTO) deve ser estruturado para suportar versionamento de schema e migrações futuras sem corromper perfis existentes? → A: Opção A — Definir ProgressoJogadorDTO como estrutura plana contendo VersaoSchema = 1 (inteiro), DataHoraSalvamentoUtc (ISO 8601) e todas as propriedades desnormalizadas do progresso do jogador, validando a versão no carregamento e desacoplando a representação física das regras de negócio.

---

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Salvamento Automático e Seguro do Progresso Localmente (Priority: P1)

Como jogador, desejo que todas as minhas conquistas, moedas ganhas, níveis de aeronave e recordes de voo sejam salvos automaticamente na memória do dispositivo de forma atômica e segura, para que meu progresso nunca seja perdido ao fechar o jogo ou descarregar a bateria.

**Why this priority**: Garante a permanência e integridade da experiência do jogador no modelo 100% offline.

**Independent Test**: Testável salvando o estado do progresso via `RepositorioProgressoLocalJson.SalvarProgressoAsync()`, reinicializando a instância e chamando `CarregarProgressoAsync()` para verificar a equivalência exata dos dados.

**Acceptance Scenarios**:
1. **Given** um progresso com 500 moedas e Motor nível 4, **When** o método `SalvarProgressoAsync` é executado, **Then** o arquivo JSON local é escrito atomicamente e validado contra corrupção.
2. **Given** um arquivo salvo existente, **When** o jogo é iniciado e `CarregarProgressoAsync` é chamado, **Then** a entidade de progresso é completamente desserializada com todos os valores íntegros.

---

### User Story 2 - Recuperação Graciosa de Falhas e Arquivo Inexistente (Priority: P2)

Como novo jogador ou em caso de arquivo corrompido, o sistema deve inicializar automaticamente um perfil de progresso padrão limpo sem gerar exceções não tratadas ou travamento na inicialização.

**Why this priority**: Garante robustez de inicialização em qualquer dispositivo.

**Independent Test**: Testável chamando o carregamento em diretório limpo ou com arquivo contendo JSON corrompido e verificando a geração de um novo perfil padrão válido.

**Acceptance Scenarios**:
1. **Given** que o jogo é executado pela primeira vez (sem arquivo JSON prévio), **When** o carregamento `CarregarProgressoAsync` é acionado, **Then** o método retorna `null` de forma segura e sem lançar exceções, permitindo à camada de aplicação instanciar `ProgressoJogador.CriarNovo()`.
2. **Given** um arquivo JSON acidentalmente corrompido, **When** o sistema tenta ler o arquivo, **Then** ele tenta restaurar a partir do arquivo `.bak`; se o backup for válido, restaura-o; se o backup também estiver corrompido ou ausente, isola o arquivo original como `.corrompido_[timestamp]` e retorna `null` com segurança.

---

### Edge Cases

- Escrita interrompida no meio do processo: utilização do padrão de escrita em arquivo temporário (`.tmp`), criação/atualização de backup (`.bak`) e substituição atômica via `File.Move(..., overwrite: true)`.
- Tentativa de salvar simultaneamente a partir de duas tarefas assíncronas: controle de concorrência com semáforo assíncrono (`SemaphoreSlim`).
- Arquivo corrompido com sintaxe JSON mutilada ou dados truncados: tentativa de recuperação de `.bak`, isolamento e log sem travamento.
- Caracteres especiais ou caminhos de diretório com permissões restritas em plataformas mobile.

---

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: O sistema DEVE implementar a interface `IRepositorioProgresso` na camada de Infraestrutura através da classe `RepositorioProgressoLocalJson`.
- **FR-002**: O repositório DEVE implementar os métodos assíncronos `Task SalvarProgressoAsync(ProgressoJogador progresso, CancellationToken ct = default)` e `Task<ProgressoJogador?> CarregarProgressoAsync(CancellationToken ct = default)`, retornando `null` quando o arquivo ainda não existir no disco.
- **FR-003**: O salvamento DEVE ser atômico, gravando primeiramente em arquivo temporário com extensão `.tmp`, gerando cópia de backup `.bak` do arquivo prévio e promovendo o arquivo temporário a arquivo principal via `File.Move(..., overwrite: true)`.
- **FR-004**: A serialização DEVE utilizar JSON formatado e estruturado sem dependências pesadas externas (`System.Text.Json` puro).
- **FR-005**: O sistema NÃO DEVE enviar nenhuma telemetria nem necessitar de conexão com a internet para persistência (100% Offline First).
- **FR-006**: Em caso de arquivo JSON corrompido ou inválido, o repositório DEVE tentar carregar o arquivo de backup `.bak`; caso inexistente ou inválido, DEVE renomear o arquivo danificado para preservação de diagnóstico e retornar `null`.
- **FR-007**: O repositório DEVE sincronizar o acesso concorrente a arquivos utilizando internamente `SemaphoreSlim(1, 1)` assíncrono, protegendo contra leituras e escritas simultâneas conflitantes.
- **FR-008**: O repositório DEVE serializar e desserializar o progresso através de um DTO plano dedicado (`ProgressoJogadorDTO`) contendo campo de versionamento explícito `VersaoSchema = 1` e carimbo de data/hora UTC (`DataHoraSalvamentoUtc`), validando a compatibilidade no carregamento e mapeando bidirecionalmente com a entidade de domínio `ProgressoJogador`.

### Key Entities

- **`ProgressoJogadorDTO`**: Estrutura DTO plana para serialização JSON contendo versão do schema (`VersaoSchema = 1`), data/hora do salvamento em UTC (`DataHoraSalvamentoUtc`), identificador único (`Id`), saldo de moedas (`SaldoMoedas`), níveis de upgrades mecânicos da aeronave (`NivelMotor`, `NivelAerodinamica`, `NivelTanqueCombustivel`, `NivelCatapulta`) e recordes (`RecordeDistanciaMetros`, `RecordeAltitudeMetros`, `TotalVoosRealizados`).
- **`ConfiguracaoPersistenciaLocal`**: Objeto de configuração contendo o diretório base (`DiretorioBase`), nome do arquivo principal (`progresso.json`), nome do arquivo de backup (`progresso.bak`) e extensão temporária (`.tmp`).

---

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Tempo de salvamento assíncrono em disco inferior a 15 milissegundos em operações normais.
- **SC-002**: Zero perda ou corrupção de dados comprovada em testes de estresse de leitura/escrita concorrente.
- **SC-003**: 100% de cobertura de testes de unidade e integração nos fluxos de save, load, fallback e recuperação de arquivo corrompido.

---

## Assumptions

- O caminho base de persistência será configurável (`Application.persistentDataPath` na Unity para Windows/Android ou diretório customizado em testes .NET).
- O formato do arquivo é `.json` com codificação UTF-8.
