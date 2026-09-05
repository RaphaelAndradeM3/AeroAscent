# Feature Specification: Domínio Core, Entidades e Objetos de Valor do AeroAscent

**Feature Branch**: `001-dominio-core-aeroascent`  
**Created**: 2026-09-04  
**Status**: Ready for Planning  
**Input**: User description: "001 - Camada de Domínio C# Puro (.NET Standard), Entidades (Aeronave, Voo, Oficina), Value Objects (Combustivel, Moeda, VetorVoo) e Interfaces Base."

---

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Modelagem e Inicialização da Aeronave (Priority: P1)

Como jogador iniciando o jogo, o sistema deve instanciar uma aeronave configurada com atributos e níveis padrão de motor, aerodinâmica, tanque de combustível e catapulta para que todos os voos partam de um estado de domínio válido e consistente.

**Why this priority**: A entidade `Aeronave` é a raiz de agregação do domínio do jogo. Sem ela, nenhuma mecânica de voo, upgrade ou persistência pode operar.

**Independent Test**: Pode ser testado de forma 100% isolada via testes unitários C# verificando a criação de uma `Aeronave` com identificador único (`Guid`), níveis iniciais iguais a 1 e propriedades consistentes.

**Acceptance Scenarios**:
1. **Given** que uma nova sessão de jogo é criada sem dados anteriores, **When** a aeronave padrão for instanciada, **Then** ela deve possuir `Id` válido, nível de motor 1, aerodinâmica nível 1, tanque nível 1 e catapulta nível 1.
2. **Given** uma aeronave existente, **When** for solicitada a alteração de um nível para um valor menor que 1, **Then** o domínio deve lançar uma exceção de validação (`ArgumentOutOfRangeException` ou `DominioInvalidoException`) e impedir a alteração de estado.

---

### User Story 2 - Sessão de Voo e Atualização de Estado (Priority: P2)

Como sistema de simulação, devo poder iniciar uma sessão de `Voo` vinculada a uma `Aeronave`, rastrear distância percorrida, altitude máxima e moedas coletadas, e transitar o status de voo de forma segura (`EmPreparacao`, `EmVoo`, `Pousado`).

**Why this priority**: Permite que os casos de uso de voo e finalização trabalhem sobre uma entidade rica que encapsula regras de transição de estado e métricas.

**Independent Test**: Testável via testes unitários criando uma sessão de `Voo`, atualizando distância/altitude em tempo real e validando a máquina de estados interna.

**Acceptance Scenarios**:
1. **Given** um voo no status `EmPreparacao`, **When** a decolagem for confirmada, **Then** o status transita para `EmVoo`.
2. **Given** um voo no status `EmVoo`, **When** novas coordenadas e métricas forem registradas, **Then** a distância acumulada e a altitude máxima são atualizadas mantendo o maior valor histórico de altitude daquela sessão.
3. **Given** um voo no status `Pousado`, **When** houver tentativa de atualizar coordenadas de voo, **Then** a operação deve ser rejeitada mantendo a integridade do encerramento.

---

### User Story 3 - Operações Monetárias e Combustível com Objetos de Valor Imutáveis (Priority: P3)

Como regra de economia e física, as moedas e o combustível devem ser representados por objetos de valor imutáveis (`record` em C#), garantindo que saldos negativos e consumos inválidos sejam impossíveis por construção.

**Why this priority**: Garante integridade matemática absoluta no balanceamento econômico e na queima de combustível sem risco de estados mutáveis corrompidos.

**Independent Test**: Testável via testes unitários somando, subtraindo e comparando instâncias de `Moeda` e `Combustivel`.

**Acceptance Scenarios**:
1. **Given** um saldo de 50 moedas, **When** tentar subtrair 100 moedas, **Then** o sistema deve lançar `SaldoInsuficienteException` e não permitir saldo negativo.
2. **Given** um tanque com 20 unidades de combustível, **When** for consumido 5 unidades, **Then** uma nova instância imutável de `Combustivel` é retornada com 15 unidades restantes e percentual recalculado.

---

### Edge Cases

- Tentativa de instanciar `Aeronave` ou `Voo` com `Guid.Empty` deve falhar imediatamente.
- Registro de distância ou altitude negativa deve ser rejeitado pelas invariantes do domínio.
- Operações aritméticas com `Moeda` que possam causar overflow numérico devem ser protegidas.

---

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: O sistema DEVE definir a entidade `Aeronave` em C# puro (.NET Standard) com identificador `Guid Id`, níveis numéricos inteiros para `Motor`, `Aerodinamica`, `TanqueCombustivel` e `Catapulta`.
- **FR-002**: O sistema DEVE definir a entidade `Voo` contendo `Id`, referência à `Aeronave`, `StatusVoo` (`EmPreparacao`, `EmVoo`, `Pousado`), distância percorrida, altitude máxima e moedas coletadas na sessão.
- **FR-003**: O sistema DEVE implementar o Objeto de Valor `Moeda` como `record` imutável, com métodos de adição, subtração e validação contra valores negativos.
- **FR-004**: O sistema DEVE implementar o Objeto de Valor `Combustivel` como `record` imutável com capacidade máxima, quantidade atual, taxa de queima e cálculo de percentual restante.
- **FR-005**: O sistema DEVE implementar o Objeto de Valor `VetorVoo` como `record` imutável para coordenadas e velocidades bidimensionais/tridimensionais sem acoplamento ao `UnityEngine.Vector3`.
- **FR-006**: O sistema DEVE definir a interface `IRepositorioProgresso` e contratos de serviços do domínio (`IServicoFisicaVoo`, `IServicoEconomia`).
- **FR-007**: Todo o código desta camada DEVE estar em Português Brasileiro (pt-BR) e possuir zero referências a `UnityEngine` ou `MonoBehaviour`.

### Key Entities

- **`Aeronave`**: Entidade central representativa do avião do jogador e de suas configurações de peças mecânicas.
- **`Voo`**: Entidade que encapsula a sessão ativa de lançamento, trajetória, coleta de recursos e encerramento.
- **`Moeda`**: Objeto de valor que representa o capital financeiro do jogador.
- **`Combustivel`**: Objeto de valor que representa a energia propelente armazenada.
- **`VetorVoo`**: Objeto de valor que expressa grandezas espaciais e cinemáticas puras.

---

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Cobertura de testes unitários de 100% sobre todas as entidades, objetos de valor e regras de validação do domínio.
- **SC-002**: Tempo de execução de toda a suíte de testes do domínio inferior a 500 milissegundos.
- **SC-003**: Zero acoplamento ou dependência externa de bibliotecas gráficas ou motores de jogo no assembly de domínio.
- **SC-004**: Todas as operações de criação e cálculo executadas sem alocação residual de memória.

---

## Assumptions

- O domínio será compilado em biblioteca de classes C# (.NET Standard 2.1 / .NET 8), compatível nativamente com o ecossistema Unity.
- Todas as unidades de medida físicas adotadas no domínio seguem o Sistema Internacional (metros, metros por segundo, graus).
- A identificação única global (`Guid`) garante rastreabilidade e suporte a sincronização offline futura sem conflitos.
