# Feature Specification: Persistência de Dados Local Offline First (JSON)

**Feature Branch**: `009-persistencia-local-json`  
**Created**: 2026-09-04  
**Status**: Ready for Planning  
**Input**: User description: "009 - Camada de Infraestrutura para Salvamento/Carregamento Atômico de Progresso Local em JSON (Offline-first)."

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
1. **Given** que o jogo é executado pela primeira vez (sem arquivo JSON prévio), **When** o carregamento é acionado, **Then** o sistema retorna uma instância padrão com saldo 0, níveis em 1 e cria o arquivo local inicial.
2. **Given** um arquivo JSON acidentalmente corrompido, **When** o sistema tenta ler o arquivo, **Then** ele registra o log de aviso, cria um backup e restaura o perfil padrão de segurança.

---

### Edge Cases

- Escrita interrompida no meio do processo: utilização do padrão de escrita em arquivo temporário (`.tmp`) seguido de substituição atômica (`File.Replace` ou `File.Move`).
- Tentativa de salvar simultaneamente a partir de duas tarefas assíncronas: controle de concorrência com semáforo assíncrono (`SemaphoreSlim`).
- Caracteres especiais ou caminhos de diretório com permissões restritas em plataformas mobile.

---

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: O sistema DEVE implementar a interface `IRepositorioProgresso` na camada de Infraestrutura através da classe `RepositorioProgressoLocalJson`.
- **FR-002**: O repositório DEVE implementar os métodos assíncronos `Task SalvarProgressoAsync(ProgressoJogador progresso, CancellationToken ct = default)` e `Task<ProgressoJogador> CarregarProgressoAsync(CancellationToken ct = default)`.
- **FR-003**: O salvamento DEVE ser atômico, gravando primeiramente em arquivo temporário antes de substituir o arquivo principal de progresso.
- **FR-004**: A serialização DEVE utilizar JSON formatado e estruturado sem dependências pesadas externas (`System.Text.Json` puro).
- **FR-005**: O sistema NÃO DEVE enviar nenhuma telemetria nem necessitar de conexão com a internet para persistência (100% Offline First).

### Key Entities

- **`ProgressoJogadorDTO`**: Estrutura DTO plana para serialização JSON contendo versão do schema, timestamp de salvamento, saldo de moedas, recorde de distância e níveis dos upgrades.
- **`ConfiguracaoPersistenciaLocal`**: Objeto de configuração contendo o caminho do arquivo e nome do arquivo de dados.

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
