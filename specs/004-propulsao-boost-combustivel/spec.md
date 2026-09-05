# Feature Specification: Sistema de Propulsão (Boost) e Queima de Combustível

**Feature Branch**: `004-propulsao-boost-combustivel`  
**Created**: 2026-09-04  
**Status**: Ready for Planning  
**Input**: User description: "004 - Sistema de Propulsor / Boost e Gerenciamento de Combustível durante o voo, aceleração extra e corte automático ao esgotar."

---

## Clarifications

### Session 2026-09-05
- Q: Como calcular o empuxo e o consumo quando o combustível restante cobrir apenas uma fração do passo de simulação (dt)? → A: Aplicar impulso proporcional ao tempo residual de queima (dt_queima = CombustivelRestante / TaxaConsumo), zerando o combustível e desativando o propulsor com precisão temporal de corte inferior a 1ms (SC-001).
- Q: Como o comando de acionamento do boost e o estado do propulsor devem ser integrados à simulação de voo? → A: Incorporar 'bool AcionarBoost' em 'ParametrosControlePiloto' (struct) e expor 'EstadoPropulsor' (readonly record struct com EstaAtivo, EmpuxoNewtons, CombustivelRestante) no estado de voo com zero alocação de GC (SC-002).
- Q: Quais valores numéricos base devem ser adotados para Empuxo Base, Capacidade Base e Taxa de Consumo? → A: EmpuxoBase = 120.0 N, CapacidadeBase = 20.0 unidades, TaxaConsumo = 5.0 un/s (proporcionando 4.0s de impulso contínuo no nível 1 e 13.0s no nível 10, com aceleração líquida positiva superando o peso de 98.1 N).
- Q: Como a força vetorial de empuxo (T) deve ser decomposta nos eixos de movimento? → A: Decomposição trigonométrica no ângulo de pitch (theta) do nariz: Tx = 0, Ty = T * sin(theta) e Tz = T * cos(theta), integrando aceleração longitudinal e vertical de acordo com a atitude da aeronave.
- Q: Como o propulsor deve se comportar se o boost for acionado antes da decolagem ou ao tocar o solo? → A: Bloquear o boost estritamente tanto na preparação na catapulta quanto no solo/pousado (EstaAtivo = false, empuxo = 0, sem consumo de combustível).

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

- Bloqueio pré-decolagem: Durante o status `EmPreparacao` (aeronave na catapulta), qualquer comando de boost é bloqueado (`EstaAtivo = false`, empuxo zero e sem consumo), evitando perda acidental de combustível antes do disparo.
- Bloqueio em solo e pouso: Se a aeronave tocar o solo (`NoSolo = true`) ou transitar para `Pousado`, o propulsor é imediatamente cortado e impedido de reativar, prevenindo empuxo espúrio ou consumo no solo.
- Acionamento intermitente rápido (*pulsos de boost*) não deve gerar vazamento ou consumo incorreto de combustível.
- O combustível nunca deve atingir valores negativos (bloqueio rígido em zero).
- Queima fracionária no esgotamento: Quando o combustível restante for inferior ao consumo de um passo integral ($\text{CombustivelRestante} < \text{TaxaConsumo} \times \Delta t$), o impulso de empuxo é aplicado estritamente pelo tempo residual $\Delta t_{\text{queima}}$, zerando o combustível e desativando o propulsor sem descontinuidade física.

---

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: O sistema DEVE calcular a força escalar de empuxo ($T$) quando o boost está ativo ($T = \text{EmpuxoBase} \times (1 + (\text{NivelMotor} - 1) \times 0.30)$, adotando $\text{EmpuxoBase} = 120.0\text{ N}$) e decompô-la vetorialmente no ângulo de pitch ($\theta$): $T_x = 0$, $T_y = T \cdot \sin(\theta)$, $T_z = T \cdot \cos(\theta)$.
- **FR-002**: A capacidade de combustível DEVE ser dada por $\text{Capacidade} = \text{CapacidadeBase} \times (1 + (\text{NivelTanque} - 1) \times 0.25)$, adotando $\text{CapacidadeBase} = 20.0\text{ unidades}$.
- **FR-003**: O consumo de combustível DEVE ocorrer de forma contínua proporcional ao tempo de acionamento ($\Delta t \times \text{TaxaConsumo}$), adotando $\text{TaxaConsumo} = 5.0\text{ unidades/segundo}$.
- **FR-004**: O sistema DEVE interromper automaticamente o empuxo extra e o consumo no instante exato em que o combustível atinge zero, quando a aeronave tocar o solo ou quando o voo não estiver no status ativo `EmVoo`. Se o combustível se esgotar no meio de um passo $\Delta t$, o empuxo aplicado deve ser estritamente proporcional ao tempo de queima restante ($\Delta t_{\text{queima}} = \text{CombustivelRestante} / \text{TaxaConsumo}$), garantindo conservação de energia e transição precisa para inativo.
- **FR-005**: O estado do propulsor (`EstadoPropulsor`) e a quantidade restante de combustível DEVEM ser expostos a cada ciclo de simulação como structs imutáveis na stack, viabilizando integração sem alocação com telemetria, áudio e efeitos visuais.

### Key Entities

- **`EstadoPropulsor`**: Objeto de valor (`readonly record struct`) alocado na stack contendo `bool EstaAtivo`, `float EmpuxoNewtons`, `float CombustivelRestante`, `float PercentualRestante` e `float TaxaConsumoPorSegundo`, garantindo conformidade com SC-002 (`GC Alloc = 0 bytes`).
- **`ParametrosControlePiloto`**: Objeto de valor (`readonly record struct`) estendido com a propriedade `bool AcionarBoost`, agregando de forma unificada os comandos de pilotagem (pitch e boost) do jogador.
- **`Combustivel`**: Objeto de valor imutável que armazena a quantidade atual e capacidade total.

---

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Esgotamento preciso de combustível com margem de erro temporal inferior a 1 milissegundo.
- **SC-002**: Atualização do estado de propulsão sem geração de alocação de lixo de memória (`GC Alloc = 0 bytes`).
- **SC-003**: 100% dos testes unitários de queima, escalonamento e corte automático validados com sucesso.

---

## Assumptions

- O empuxo é decomposto estritamente no plano Y-Z alinhado à atitude do nariz: $T_y = T \cdot \sin(\theta)$ e $T_z = T \cdot \cos(\theta)$, onde $\theta$ é o ângulo de pitch em radianos.
- A taxa de queima é constante durante o tempo em que o propulsor estiver ligado.
