# Tasks: Sistema de Lançamento Inicial e Catapulta

**Input**: Documentos de design em [specs/002-sistema-lancamento-catapulta/](file:///h:/tmp/RSA/Loterias/JogosMaster/GitHub/AeroAscent/specs/002-sistema-lancamento-catapulta/)  
**Prerequisites**: [plan.md](file:///h:/tmp/RSA/Loterias/JogosMaster/GitHub/AeroAscent/specs/002-sistema-lancamento-catapulta/plan.md), [spec.md](file:///h:/tmp/RSA/Loterias/JogosMaster/GitHub/AeroAscent/specs/002-sistema-lancamento-catapulta/spec.md), [research.md](file:///h:/tmp/RSA/Loterias/JogosMaster/GitHub/AeroAscent/specs/002-sistema-lancamento-catapulta/research.md), [data-model.md](file:///h:/tmp/RSA/Loterias/JogosMaster/GitHub/AeroAscent/specs/002-sistema-lancamento-catapulta/data-model.md), [contracts/ILancarAeronaveCasoDeUso.cs](file:///h:/tmp/RSA/Loterias/JogosMaster/GitHub/AeroAscent/specs/002-sistema-lancamento-catapulta/contracts/ILancarAeronaveCasoDeUso.cs), [quickstart.md](file:///h:/tmp/RSA/Loterias/JogosMaster/GitHub/AeroAscent/specs/002-sistema-lancamento-catapulta/quickstart.md)  
**Tests**: Inclui tarefas de testes unitários automatizados xUnit cobrindo 100% dos cálculos físicos, objetos de valor e casos de uso da Clean Architecture.  
**Organization**: Tarefas agrupadas por histórias de usuário para viabilizar implementação e testes independentes de cada história.

---

## Format: `- [ ] [TaskID] [P?] [Story?] Description with file path`

- **[P]**: Pode executar em paralelo (arquivos distintos, sem dependência de tarefas incompletas)
- **[Story]**: História de usuário à qual a tarefa pertence ([US1], [US2]). Omitido nas fases de Setup, Foundational e Polish.
- Caminhos de arquivo exatos e explícitos em todas as descrições.

---

## Phase 1: Setup (Infraestrutura de Projetos)

**Purpose**: Criação do projeto de Aplicação da Clean Architecture e seu respectivo projeto de testes na solução.

- [ ] T001 Criar projeto de biblioteca de classes em src/AeroAscent.Core.Aplicacao/AeroAscent.Core.Aplicacao.csproj configurado para netstandard2.1 e net8.0 com referência ao projeto src/AeroAscent.Core.Dominio/AeroAscent.Core.Dominio.csproj
- [ ] T002 [P] Criar projeto de testes unitários em tests/AeroAscent.Core.Aplicacao.Testes/AeroAscent.Core.Aplicacao.Testes.csproj em net8.0 com pacotes xUnit, xunit.runner.visualstudio, Microsoft.NET.Test.Sdk e referências a AeroAscent.Core.Aplicacao e AeroAscent.Core.Dominio
- [ ] T003 Adicionar os projetos AeroAscent.Core.Aplicacao e AeroAscent.Core.Aplicacao.Testes ao arquivo de solução AeroAscent.slnx e validar compilação

---

## Phase 2: Foundational (Pré-requisitos Bloqueantes)

**Purpose**: Objetos de valor e contratos centrais que DEVEM estar concluídos antes da implementação das histórias de usuário.

**⚠️ CRITICAL**: Nenhuma implementação de história de usuário pode começar sem a conclusão desta fase.

- [ ] T004 [P] Implementar Objeto de Valor ParametrosLancamento como readonly record struct com precisão, ângulo e piso protetivo em src/AeroAscent.Core.Dominio/ObjetosDeValor/ParametrosLancamento.cs
- [ ] T005 [P] Implementar Objeto de Valor ResultadoLancamento como record imutável com métodos de fábrica em src/AeroAscent.Core.Dominio/ObjetosDeValor/ResultadoLancamento.cs
- [ ] T006 [P] Implementar contrato de caso de uso ILancarAeronaveCasoDeUso em src/AeroAscent.Core.Aplicacao/Contratos/ILancarAeronaveCasoDeUso.cs

**Checkpoint**: Base fundamental concluída — o desenvolvimento das histórias de usuário pode prosseguir.

---

## Phase 3: User Story 1 - Lançamento da Aeronave pela Catapulta (Priority: P1) 🎯 MVP

**Goal**: Permitir o disparo da aeronave pela catapulta com cálculo trigonométrico tridimensional (decomposição em 35° nos eixos Z e Y), escalonamento linear por nível da catapulta (+25% por nível com base de 25.0 m/s), transição de estado da entidade Voo para EmVoo e retorno de ResultadoLancamento.

**Independent Test**: Instanciar Aeronave e Voo em status EmPreparacao, acionar o caso de uso LancarAeronaveCasoDeUso com precisão de 100% no nível 1 e no nível 3; verificar se a velocidade resultante corresponde aos valores teóricos (25.0 m/s e 37.5 m/s com decomposição em Z e Y), e se o status do voo mudou para EmVoo.

### Tests for User Story 1 ⚠️

> **NOTE: Escreva estes testes PRIMEIRO e confirme que FALHAM antes da implementação.**

- [ ] T007 [P] [US1] Criar testes unitários para o cálculo de impulso vetorial tridimensional e escalonamento por nível em tests/AeroAscent.Core.Dominio.Testes/Servicos/ServicoFisicaVooTestes.cs
- [ ] T008 [P] [US1] Criar testes unitários para a orquestração do caso de uso de lançamento, transição de voo, integridade de combustível inalterado (FR-005) e bloqueio de lançamento duplo em tests/AeroAscent.Core.Aplicacao.Testes/CasosDeUso/LancarAeronaveCasoDeUsoTestes.cs

### Implementation for User Story 1

- [ ] T009 [US1] Implementar serviço de domínio ServicoFisicaVoo com cálculo de impulso 3D em 35 graus e escalonamento linear em src/AeroAscent.Core.Dominio/Servicos/ServicoFisicaVoo.cs (depende de T007)
- [ ] T010 [US1] Implementar caso de uso LancarAeronaveCasoDeUso orquestrando validação, cálculo de impulso, transição para EmVoo e retorno de ResultadoLancamento em src/AeroAscent.Core.Aplicacao/CasosDeUso/LancarAeronaveCasoDeUso.cs (depende de T006, T008, T009)

**Checkpoint**: Neste estágio, a User Story 1 (MVP) estará 100% funcional e testável de forma independente.

---

## Phase 4: User Story 2 - Variação de Eficácia do Lançamento por Temporização (Priority: P2)

**Goal**: Proporcionar amostragem matemática contínua e determinística da precisão instantânea através do Objeto de Valor MedidorForcaOscilante (onda triangular a 1.0 Hz) e assegurar aplicação incondicional do piso protetivo de 10% (0.10f) contra frustração de timing.

**Independent Test**: Testar MedidorForcaOscilante em diferentes intervalos temporais (t=0.0s, 0.5s, 1.0s) verificando a curva triangular normalizada [0.0, 1.0]; testar disparos com precisão 0% e verificar aplicação de precisão efetiva mínima de 0.10f com decolagem bem-sucedida.

### Tests for User Story 2 ⚠️

> **NOTE: Escreva estes testes PRIMEIRO e confirme que FALHAM antes da implementação.**

- [ ] T011 [P] [US2] Criar testes unitários para a dinâmica periódica triangular de MedidorForcaOscilante em tests/AeroAscent.Core.Dominio.Testes/ObjetosDeValor/MedidorForcaOscilanteTestes.cs
- [ ] T012 [P] [US2] Criar testes unitários para a validação de piso protetivo de 10% e limites de ângulo em tests/AeroAscent.Core.Dominio.Testes/ObjetosDeValor/ParametrosLancamentoTestes.cs

### Implementation for User Story 2

- [ ] T013 [US2] Implementar Objeto de Valor MedidorForcaOscilante como readonly record struct com amostragem periódica triangular em src/AeroAscent.Core.Dominio/ObjetosDeValor/MedidorForcaOscilante.cs (depende de T011)
- [ ] T014 [US2] Integrar o piso protetivo e validações de temporização na orquestração de lançamento em src/AeroAscent.Core.Aplicacao/CasosDeUso/LancarAeronaveCasoDeUso.cs (depende de T010, T012, T013)

**Checkpoint**: Neste estágio, as User Stories 1 e 2 estarão totalmente integradas e testadas com zero falhas.

---

## Phase 5: Polish & Cross-Cutting Concerns

**Purpose**: Verificações transversais, documentação XML integral, conformidade com os critérios de sucesso e validação de ponta a ponta.

- [ ] T015 [P] Adicionar documentação XML completa (///) em 100% das classes, métodos, propriedades e structs públicas em src/AeroAscent.Core.Aplicacao/ e novos tipos em src/AeroAscent.Core.Dominio/
- [ ] T016 Implementar testes de aceitação automatizados para os 5 cenários funcionais do guia em tests/AeroAscent.Core.Aplicacao.Testes/CasosDeUso/CenariosQuickstartLancamentoTestes.cs baseados em specs/002-sistema-lancamento-catapulta/quickstart.md
- [ ] T017 Executar suíte completa dotnet test --configuration Release e comprovar aprovação de 100% dos testes com tempo unitário vetorial inferior a 100ms e suíte global inferior a 200ms conforme critério SC-001

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: Sem dependências prévias — inicialização imediata dos projetos C#.
- **Foundational (Phase 2)**: Depende da conclusão da Fase 1 — BLOQUEIA a implementação das histórias de usuário.
- **User Story 1 (Phase 3)**: Depende da conclusão da Fase 2 — Entrega o MVP funcional do sistema de lançamento.
- **User Story 2 (Phase 4)**: Depende da conclusão da Fase 3 — Refina a dinâmica temporal e garante o piso mínimo protetivo.
- **Polish (Phase 5)**: Depende da conclusão das histórias US1 e US2.

### User Story Dependencies

- **User Story 1 (P1)**: Depende exclusivamente da Fase Foundational. Não possui dependências de US2.
- **User Story 2 (P2)**: Depende de `LancarAeronaveCasoDeUso` (US1) e dos objetos fundamentais para enriquecer a precisão e amostragem temporal.

### Within Each User Story

- Testes unitários (TDD) escritos PRIMEIRO e falhando antes da implementação.
- Objetos de valor e serviços antes dos casos de uso de orquestração.
- Validação e testes aprovados antes do avanço para a próxima história.

### Parallel Opportunities

- Na Fase 1: `T002` pode rodar em paralelo após a criação da estrutura de pastas.
- Na Fase 2: `T004`, `T005` e `T006` podem ser implementados simultaneamente por atuarem em arquivos distintos.
- Na Fase 3: Os testes `T007` e `T008` podem ser desenvolvidos em paralelo.
- Na Fase 4: Os testes `T011` e `T012` podem ser desenvolvidos em paralelo.
- Na Fase 5: A documentação XML `T015` pode ser revisada em paralelo aos testes do quickstart `T016`.

---

## Parallel Example: User Story 1

```bash
# Executar a criação paralela dos testes unitários para a US1:
Task T007: "tests/AeroAscent.Core.Dominio.Testes/Servicos/ServicoFisicaVooTestes.cs"
Task T008: "tests/AeroAscent.Core.Aplicacao.Testes/CasosDeUso/LancarAeronaveCasoDeUsoTestes.cs"

# Após os testes falharem, implementar os componentes da US1:
Task T009: "src/AeroAscent.Core.Dominio/Servicos/ServicoFisicaVoo.cs"
Task T010: "src/AeroAscent.Core.Aplicacao/CasosDeUso/LancarAeronaveCasoDeUso.cs"
```

---

## Parallel Example: User Story 2

```bash
# Executar a criação paralela dos testes para a US2:
Task T011: "tests/AeroAscent.Core.Dominio.Testes/ObjetosDeValor/MedidorForcaOscilanteTestes.cs"
Task T012: "tests/AeroAscent.Core.Dominio.Testes/ObjetosDeValor/ParametrosLancamentoTestes.cs"

# Implementar os componentes de precisão e medidor da US2:
Task T013: "src/AeroAscent.Core.Dominio/ObjetosDeValor/MedidorForcaOscilante.cs"
Task T014: "src/AeroAscent.Core.Aplicacao/CasosDeUso/LancarAeronaveCasoDeUso.cs"
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Concluir a **Fase 1 (Setup)** com criação dos projetos `Core.Aplicacao` e `Core.Aplicacao.Testes`.
2. Concluir a **Fase 2 (Foundational)** com os objetos de valor `ParametrosLancamento`, `ResultadoLancamento` e contrato `ILancarAeronaveCasoDeUso`.
3. Implementar a **Fase 3 (User Story 1)** com testes de física 3D e caso de uso de disparo.
4. **VALIDAÇÃO INDEPENDENTE**: Executar `dotnet test` e certificar que a decolagem com catapulta funciona perfeitamente (MVP entregue!).

### Incremental Delivery

1. Fase 1 + Fase 2 $\rightarrow$ Fundação compilada e estável.
2. Fase 3 (US1) $\rightarrow$ MVP de lançamento funcional (catapulta + física 3D + transição de voo).
3. Fase 4 (US2) $\rightarrow$ Amostragem analítica da barra oscilante e proteção do piso de 10%.
4. Fase 5 (Polish) $\rightarrow$ Documentação completa, validação dos 5 cenários do quickstart e garantia de SC-001 (< 200 ms).

---

## Notes

- Todas as structs (`ParametrosLancamento`, `MedidorForcaOscilante`) são `readonly record struct` garantindo **GC Alloc = 0 bytes** na stack.
- Nomenclatura 100% em Português Brasileiro (pt-BR) com convenções .NET rigorosas (`PascalCase`, `camelCase`, `_camelCase`, `UPPER_SNAKE_CASE`).
- Clean Architecture preservada: Domínio isolado sem referências externas; Aplicação depende apenas do Domínio.
- Cada fase será compilada, testada e comitada antes de passar para a próxima fase.
