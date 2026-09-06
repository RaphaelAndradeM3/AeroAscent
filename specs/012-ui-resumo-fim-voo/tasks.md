# Implementation Tasks: Interface de Resumo de Voo e Celebração de Recorde (Feature 012)

**Branch**: `012-ui-resumo-fim-voo` | **Spec**: [spec.md](file:///h:/tmp/RSA/Loterias/JogosMaster/GitHub/AeroAscent/specs/012-ui-resumo-fim-voo/spec.md) | **Plan**: [plan.md](file:///h:/tmp/RSA/Loterias/JogosMaster/GitHub/AeroAscent/specs/012-ui-resumo-fim-voo/plan.md)

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Estruturação inicial do namespace e organização de diretórios do projeto para a interface de resumo de voo.

- [X] T001 [P] Configurar estrutura de pastas e namespaces para resumo de voo em src/AeroAscent.Core.Aplicacao/Apresentadores/ e tests/AeroAscent.Core.Aplicacao.Testes/Apresentadores/

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Contratos de interface, DTO imutável e fixtures de teste fundamentais para todas as histórias de usuário.

**⚠️ CRITICAL**: Nenhuma história de usuário pode ser iniciada antes da conclusão desta fase.

- [X] T002 [P] Implementar DTO imutável na stack ModeloVisualResumoVoo em src/AeroAscent.Core.Aplicacao/DTOs/ModeloVisualResumoVoo.cs
- [X] T003 [P] Implementar contrato de visão passiva IVisaoResumoVoo em src/AeroAscent.Core.Aplicacao/Contratos/IVisaoResumoVoo.cs
- [X] T004 [P] Implementar contrato do apresentador IApresentadorResumoVoo em src/AeroAscent.Core.Aplicacao/Contratos/IApresentadorResumoVoo.cs
- [X] T005 [P] Criar fixture VisaoResumoVooFalsa (Spy/Mock) para testes unitários em tests/AeroAscent.Core.Aplicacao.Testes/Fixtures/VisaoResumoVooFalsa.cs

**Checkpoint**: Infraestrutura e contratos base prontos — a implementação das histórias de usuário pode começar.

---

## Phase 3: User Story 1 - Animação de Recompensas e Contagem de Moedas (Priority: P1) 🎯 MVP

**Goal**: Exibir a distância percorrida, altitude máxima, decomposição analítica das moedas ganhas, contagem animada de 1,5 segundos e suporte a pulo imediato (*skip to end*) por toque na tela com atualização do saldo final.

**Independent Test**: Simular a finalização de um voo com 34 moedas ganhas e verificar que a tela exibe valores formatados em pt-BR, executa a animação bloqueando navegação e permite concluir instantaneamente via toque.

### Tests for User Story 1
- [X] T006 [P] [US1] Criar testes unitários para formatação do modelo visual, contagem de moedas e pulo da animação em tests/AeroAscent.Core.Aplicacao.Testes/Apresentadores/ApresentadorResumoVooTestes.cs

### Implementation for User Story 1
- [X] T007 [US1] Implementar esqueleto de ApresentadorResumoVoo e projeção com formatação pt-BR em src/AeroAscent.Core.Aplicacao/Apresentadores/ApresentadorResumoVoo.cs
- [X] T008 [US1] Implementar controle da animação de contagem de moedas, bloqueio inicial de navegação e método PularAnimacao em src/AeroAscent.Core.Aplicacao/Apresentadores/ApresentadorResumoVoo.cs
- [X] T009 [US1] Executar e validar testes de User Story 1 assegurando zero alocação no heap em tests/AeroAscent.Core.Aplicacao.Testes/Apresentadores/ApresentadorResumoVooTestes.cs

**Checkpoint**: User Story 1 concluída, testada e funcional de forma independente como MVP.

---

## Phase 4: User Story 2 - Celebração Especial de Novo Recorde Pessoal (Priority: P2)

**Goal**: Ativar celebração festiva com banner "NOVO RECORDE!" e confetes quando o voo quebrar o recorde de distância ou altitude.

**Independent Test**: Apresentar um voo com `EhNovoRecordeDistancia = true` ou `EhNovoRecordeAltitude = true` e validar a ativação do banner de recorde e confetes no modelo visual; validar ausência em voos comuns.

### Tests for User Story 2
- [X] T010 [P] [US2] Adicionar testes unitários para detecção e ativação de novo recorde pessoal no resumo em tests/AeroAscent.Core.Aplicacao.Testes/Apresentadores/ApresentadorResumoVooTestes.cs

### Implementation for User Story 2
- [X] T011 [US2] Implementar lógica de agregação de recordes e acionamento de celebração festiva em src/AeroAscent.Core.Aplicacao/Apresentadores/ApresentadorResumoVoo.cs
- [X] T012 [US2] Executar e validar testes de User Story 2 garantindo separação correta entre voos com e sem recorde em tests/AeroAscent.Core.Aplicacao.Testes/Apresentadores/ApresentadorResumoVooTestes.cs

**Checkpoint**: User Stories 1 e 2 funcionando de maneira integrada e independente.

---

## Phase 5: User Story 3 - Navegação e Decisão Pós-Voo (Priority: P3)

**Goal**: Permitir ao jogador escolher entre "Ir para Oficina" ou "Voar Novamente", despachando eventos C# puros para o orquestrador da Unity e gerenciando cliques acidentais durante a animação.

**Independent Test**: Simular cliques nos botões de navegação, verificando o disparo dos eventos C# correspondentes, a proteção contra avanço prematuro durante a animação e o fechamento da tela via `Ocultar()`.

### Tests for User Story 3
- [X] T013 [P] [US3] Adicionar testes unitários para eventos de navegação pós-voo e interceptação de clique durante animação em tests/AeroAscent.Core.Aplicacao.Testes/Apresentadores/ApresentadorResumoVooTestes.cs

### Implementation for User Story 3
- [X] T014 [US3] Implementar inscrição de eventos da visão e disparo de AoSolicitarIrParaOficina e AoSolicitarVoarNovamente em src/AeroAscent.Core.Aplicacao/Apresentadores/ApresentadorResumoVoo.cs
- [X] T015 [US3] Implementar método Ocultar e proteção contra múltiplos disparos em src/AeroAscent.Core.Aplicacao/Apresentadores/ApresentadorResumoVoo.cs
- [X] T016 [US3] Executar e validar testes de User Story 3 garantindo cobertura completa do ciclo de navegação em tests/AeroAscent.Core.Aplicacao.Testes/Apresentadores/ApresentadorResumoVooTestes.cs

**Checkpoint**: Todas as histórias de usuário (US1, US2 e US3) implementadas e testadas independentemente.

---

## Phase 6: Polish & Cross-Cutting Concerns

**Purpose**: Verificações de conformidade com a Constituição, documentação XML integral em pt-BR e validação global da suíte de testes.

- [ ] T017 [P] Revisar documentação XML integral em pt-BR em todos os contratos, DTOs e classes do resumo em src/AeroAscent.Core.Aplicacao/
- [ ] T018 Executar validação completa do quickstart.md e suíte global de testes via dotnet test AeroAscent.slnx

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: Sem dependências — execução imediata.
- **Foundational (Phase 2)**: Depende do Setup — BLOQUEIA todas as histórias de usuário.
- **User Story 1 (Phase 3)**: Depende da Fase Foundational — entrega o MVP da tela de resumo.
- **User Story 2 (Phase 4)**: Depende da Fase Foundational e integra-se ao modelo visual de US1.
- **User Story 3 (Phase 5)**: Depende da Fase Foundational e das regras de animação de US1.
- **Polish (Phase 6)**: Depende da conclusão de todas as histórias de usuário.

### User Story Dependencies

- **User Story 1 (P1)**: Autônoma após Foundational.
- **User Story 2 (P2)**: Extensão do modelo visual já exibido em US1.
- **User Story 3 (P3)**: Utiliza o estado de animação de US1 para travar/destravar botões.

---

## Parallel Opportunities

- Todas as tarefas marcadas com `[P]` podem ser desenvolvidas em paralelo quando não houver conflito de arquivo:
  - T002, T003, T004 e T005 operam em arquivos distintos.
  - T006, T010 e T013 desenvolvem cenários de teste específicos.
  - T017 opera em documentação de tipos distintos.

---

## Implementation Strategy

### MVP First (User Story 1)
1. Concluir Setup (Fase 1) e Foundational (Fase 2).
2. Concluir User Story 1 (Fase 3): Exibição formatada das moedas, contagem animada e pulo instantâneo.
3. Validar testes de US1. O jogo já passa a ter um extrato funcional pós-voo.

### Entrega Incremental
1. Integrar User Story 2 (Fase 4): Banner de comemoração de recordes e confetes.
2. Integrar User Story 3 (Fase 5): Roteamento de fluxo para oficina e reinício de voo.
3. Executar Polish (Fase 6): Garantir documentação XML, testes globais e conformidade constitucional.
