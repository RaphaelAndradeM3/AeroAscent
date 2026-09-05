# Feature Specification: Sistema de Propulsão (Boost) e Queima de Combustível

**Feature Branch**: `004-propulsao-boost-combustivel`  
**Created**: 2026-09-04  
**Status**: Ready for Planning  
**Input**: User description: "004 - Sistema de Propulsor / Boost e Gerenciamento de Combustível durante o voo, aceleração extra e corte automático ao esgotar."

---

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Acionamento de Impulso Extra (Boost) com Consumo de Combustível (Priority: P1)

Como jogador em pleno voo, desejo pressionar e manter o botão de propulsão (*boost*) para acelerar rapidamente a aeronave na direção para onde o nariz está apontando, consumindo o combustível disponível no tanque.

**Why this priority**: É a mecânica ativa de aceleração tática do jogo, permitindo recuperar altitude ou ultrapassar barreiras de distância.

**Independent Test**: Testável ativando o comando de boost no caso de uso de voo e confirmando o aumento de empuxo vetorial e a redução gradual e precisa do combustível.

**Acceptance Scenarios**:
1. **Given** que a aeronave possui 100% de combustível, **When** o jogador aciona o botão de propulsão por 2 segundos, **Then** a aeronave recebe força de empuxo adicional e o medidor de combustível é debitado na taxa estabelecida por segundo.
2. **Given** combustível restante de 0%, **When** o jogador tenta acionar a propulsão, **Then** nenhum empuxo adicional é gerado e o estado de boost é desabilitado.

---

### User Story 2 - Impacto dos Upgrades de Motor e Tanque (Priority: P2)

Como jogador que aprimorou o motor ou o tanque de combustível na oficina, desejo que meu propulsor seja mais potente (motor) ou dure mais tempo (tanque), refletindo meu progresso.

**Why this priority**: Conecta a economia de upgrades com a durabilidade e poder de propulsão durante a partida.

**Independent Test**: Testável comparando o tempo total de queima de um tanque nível 1 vs nível 3 e a magnitude da força de empuxo de um motor nível 1 vs nível 3.

**Acceptance Scenarios**:
1. **Given** um tanque nível 3, **When** o boost é acionado continuamente até o esgotamento, **Then** a duração total da queima é proporcionalmente maior que a do tanque nível 1.
2. **Given** um motor nível 3, **When** o boost é acionado, **Then** a aceleração por segundo aplicada à aeronave é superior à do motor nível 1.

---

### Edge Cases

- Tentativa de queimar combustível quando a aeronave já estiver em solo ou status `Pousado` deve ser bloqueada.
- Acionamento intermitente rápido (*pulsos de boost*) não deve gerar vazamento ou consumo incorreto de combustível.
- O combustível nunca deve atingir valores negativos (bloqueio rígido em zero).

---

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: O sistema DEVE calcular a força de empuxo ($T$) quando o boost está ativo: $T = \text{EmpuxoBase} \times (1 + (\text{NivelMotor} - 1) \times 0.30)$.
- **FR-002**: A capacidade de combustível DEVE ser dada por $\text{Capacidade} = \text{CapacidadeBase} \times (1 + (\text{NivelTanque} - 1) \times 0.25)$.
- **FR-003**: O consumo de combustível DEVE ocorrer de forma contínua proporcional ao tempo de acionamento ($\Delta t \times \text{TaxaConsumo}$).
- **FR-004**: O sistema DEVE interromper automaticamente o empuxo extra no instante exato em que o combustível atinge zero.
- **FR-005**: O estado do propulsor (ativo/inativo) e a quantidade restante de combustível DEVEM ser expostos para vinculação com áudio e partículas na camada de apresentação.

### Key Entities

- **`EstadoPropulsor`**: Objeto de valor contendo booleano `EstaAtivo`, empuxo atual em Newtons e taxa de consumo.
- **`Combustivel`**: Objeto de valor imutável que armazena a quantidade atual e capacidade total.

---

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Esgotamento preciso de combustível com margem de erro temporal inferior a 1 milissegundo.
- **SC-002**: Atualização do estado de propulsão sem geração de alocação de lixo de memória (`GC Alloc = 0 bytes`).
- **SC-003**: 100% dos testes unitários de queima, escalonamento e corte automático validados com sucesso.

---

## Assumptions

- O empuxo é aplicado na direção do vetor frontal (nariz) da aeronave.
- A taxa de queima é constante durante o tempo em que o propulsor estiver ligado.
