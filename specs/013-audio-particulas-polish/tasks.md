# Implementation Tasks: Áudio, Sistema de Partículas Kenney e Polimento Geral (Feature 013)

**Branch**: `013-audio-particulas-polish` | **Spec**: [spec.md](file:///h:/tmp/RSA/Loterias/JogosMaster/GitHub/AeroAscent/specs/013-audio-particulas-polish/spec.md) | **Plan**: [plan.md](file:///h:/tmp/RSA/Loterias/JogosMaster/GitHub/AeroAscent/specs/013-audio-particulas-polish/plan.md)

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Estruturação de diretórios, pastas de serviços e namespaces para áudio e partículas.

- [X] T001 [P] Configurar estrutura de diretórios e namespaces para áudio e configurações em src/AeroAscent.Core.Dominio/ e src/AeroAscent.Core.Aplicacao/

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Tipos imutáveis fundamentais, enumerações, contratos de serviço e fixtures de teste essenciais para todas as histórias de usuário.

**⚠️ CRITICAL**: Nenhuma história de usuário pode ser iniciada antes da conclusão desta fase.

- [X] T002 [P] Implementar enum EventoAudio com os 10 gatilhos sonoros em src/AeroAscent.Core.Dominio/Enums/EventoAudio.cs
- [X] T003 [P] Implementar struct imutável ConfiguracaoAudio em src/AeroAscent.Core.Dominio/ObjetosDeValor/ConfiguracaoAudio.cs
- [X] T004 [P] Implementar contrato IServicoAudio em src/AeroAscent.Core.Aplicacao/Contratos/IServicoAudio.cs
- [X] T005 [P] Criar fixture ServicoAudioFalso (Spy/Mock) para testes unitários em tests/AeroAscent.Core.Aplicacao.Testes/Fixtures/ServicoAudioFalso.cs

**Checkpoint**: Infraestrutura e contratos base prontos — a implementação das histórias de usuário pode começar.

---

## Phase 3: User Story 1 - Efeitos Sonoros Imersivos e Suaves (Priority: P1) 🎯 MVP

**Goal**: Permitir o disparo de eventos sonoros de voo, modulação dinâmica na stack dos loops de vento e propulsão com zero alocação no heap e modulação de pitch melódico ascendente para coleta rápida de moedas.

**Independent Test**: Disparar eventos através de `IServicoAudio`, simular loops contínuos de vento/boost e validar a modulação de pitch e teto de polifonia via testes unitários automatizados em xUnit.

### Tests for User Story 1
- [X] T006 [P] [US1] Criar testes unitários para IServicoAudio e ServicoAudioFalso em tests/AeroAscent.Core.Aplicacao.Testes/Servicos/ServicoAudioTestes.cs

### Implementation for User Story 1
- [X] T007 [US1] Implementar lógica de registro e despacho de eventos sonoros e música tema em tests/AeroAscent.Core.Aplicacao.Testes/Fixtures/ServicoAudioFalso.cs
- [X] T008 [US1] Implementar controle na stack dos loops contínuos de vento e propulsão com atenuação suave em tests/AeroAscent.Core.Aplicacao.Testes/Fixtures/ServicoAudioFalso.cs
- [X] T009 [US1] Implementar e testar cálculo de modulação harmônica de pitch (+0.05) e limite de polifonia em tests/AeroAscent.Core.Aplicacao.Testes/Servicos/ServicoAudioTestes.cs
- [X] T010 [US1] Validar testes de User Story 1 e garantir zero alocação de memória no heap em tests/AeroAscent.Core.Aplicacao.Testes/Servicos/ServicoAudioTestes.cs

**Checkpoint**: User Story 1 concluída, testada e funcional de forma independente como MVP.

---

## Phase 4: User Story 2 - Emissores de Partículas e Feedback Visual (Priority: P2)

**Goal**: Estabelecer os contratos desacoplados e gerenciamento de partículas visuais (rastro de cauda, chamas de boost, brilho de moedas e confetes) operando sob Object Pooling.

**Independent Test**: Testável acionando os comandos de emissão de partículas e validando que o pool não gera instâncias repetitivas no heap.

### Tests for User Story 2
- [X] T011 [P] [US2] Criar testes unitários para contrato de partículas em tests/AeroAscent.Core.Aplicacao.Testes/Servicos/GerenciadorParticulasTestes.cs

### Implementation for User Story 2
- [X] T012 [P] [US2] Implementar contrato IGerenciadorParticulas em src/AeroAscent.Core.Aplicacao/Contratos/IGerenciadorParticulas.cs
- [X] T013 [US2] Criar fixture GerenciadorParticulasFalso em tests/AeroAscent.Core.Aplicacao.Testes/Fixtures/GerenciadorParticulasFalso.cs
- [X] T014 [US2] Executar e validar testes de User Story 2 assegurando conformidade de acionamento em tests/AeroAscent.Core.Aplicacao.Testes/Servicos/GerenciadorParticulasTestes.cs

**Checkpoint**: User Stories 1 e 2 funcionando de maneira integrada e independente.

---

## Phase 5: User Story 3 - Otimização Geral de Desempenho e Bateria Mobile (Priority: P3)

**Goal**: Integrar as preferências de áudio `ConfiguracaoAudio` ao agregado persistível `ProgressoJogador`, garantindo persistência atômica em JSON local, retrocompatibilidade e tolerância a falhas.

**Independent Test**: Testável alterando volumes e flags de SFX/Música em `ProgressoJogador`, persistindo e restaurando via `IRepositorioProgresso` e validando valores recuperados.

### Tests for User Story 3
- [X] T015 [P] [US3] Criar testes unitários de invariantes e imutabilidade de ConfiguracaoAudio em tests/AeroAscent.Core.Dominio.Testes/ObjetosDeValor/ConfiguracaoAudioTestes.cs

### Implementation for User Story 3
- [X] T016 [US3] Integrar ConfiguracaoAudio e método AtualizarConfiguracaoAudio no agregado ProgressoJogador em src/AeroAscent.Core.Dominio/Entidades/ProgressoJogador.cs
- [X] T017 [US3] Atualizar serialização e desserialização com retrocompatibilidade em src/AeroAscent.Infraestrutura/Repositorios/RepositorioProgressoJson.cs
- [X] T018 [US3] Executar e validar testes de persistência de áudio em tests/AeroAscent.Infraestrutura.Testes/Repositorios/RepositorioProgressoJsonTestes.cs

**Checkpoint**: Todas as histórias de usuário (US1, US2 e US3) implementadas e testadas independentemente.

---

## Phase 6: Polish & Cross-Cutting Concerns

**Purpose**: Verificações de conformidade com a Constituição, documentação XML integral em pt-BR e validação global da suíte de testes.

- [ ] T019 [P] Revisar documentação XML integral em pt-BR em todos os tipos novos de Domínio, Aplicação e Infraestrutura em src/
- [ ] T020 Executar validação completa do quickstart.md e suíte global de testes via dotnet test AeroAscent.slnx

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: Sem dependências — execução imediata.
- **Foundational (Phase 2)**: Depende do Setup — BLOQUEIA todas as histórias de usuário.
- **User Story 1 (Phase 3)**: Depende da Fase Foundational — entrega o MVP do serviço de áudio.
- **User Story 2 (Phase 4)**: Depende da Fase Foundational — adiciona a camada de partículas.
- **User Story 3 (Phase 5)**: Depende da Fase Foundational — integra as preferências de áudio ao domínio persistido.
- **Polish (Phase 6)**: Depende da conclusão de todas as histórias de usuário.

---

## Parallel Opportunities

- Tarefas marcadas com `[P]` podem ser desenvolvidas em paralelo:
  - T002, T003, T004 e T005 operam em arquivos distintos no Domínio, Aplicação e Testes.
  - T006, T011 e T015 desenvolvem suítes de teste isoladas em projetos diferentes.
  - T019 revisa documentação XML em arquivos independentes.

---

## Implementation Strategy

### MVP First (User Story 1)
1. Concluir Setup (Fase 1) e Foundational (Fase 2).
2. Concluir User Story 1 (Fase 3): Contrato e serviço de áudio, loops contínuos de vento e propulsão e modulação harmônica de moedas.
3. Validar testes de US1. O jogo passa a ter feedback auditivo completo e testável.

### Entrega Incremental
1. Integrar User Story 2 (Fase 4): Contrato e gerenciamento de partículas visuais.
2. Integrar User Story 3 (Fase 5): Preferências de áudio persistidas no agregado `ProgressoJogador`.
3. Executar Polish (Fase 6): Documentação XML em pt-BR e validação global da suíte de testes.
