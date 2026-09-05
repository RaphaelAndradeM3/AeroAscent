# Feature Specification: Simulação de Física Aerodinâmica e Controle de Pitch

**Feature Branch**: `003-fisica-voo-aerodinamica`  
**Created**: 2026-09-04  
**Status**: Ready for Planning  
**Input**: User description: "003 - Simulação Física de Sustentação, Arrasto, Gravidade e Controle de Pitch (inclinação do nariz) para cálculo desacoplado de trajetória."

---

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Controle de Inclinação do Nariz (Pitch) e Balanço de Forças (Priority: P1)

Como jogador durante o voo livre, desejo inclinar o nariz do avião para cima a fim de ganhar sustentação e altitude, ou para baixo para mergulhar e ganhar velocidade horizontal, permitindo que eu planeje e controle minha trajetória.

**Why this priority**: É a mecânica central do core loop de voo do AeroAscent, onde a habilidade de pilotagem do jogador se manifesta.

**Independent Test**: Testável alimentando o `ServicoFisicaVoo` com diferentes ângulos de pitch e velocidades atuais, e verificando a aplicação correta dos vetores de sustentação (*lift*) e arrasto (*drag*).

**Acceptance Scenarios**:
1. **Given** a aeronave voando a 20 m/s horizontalmente, **When** o jogador inclina o nariz 15 graus para cima, **Then** o serviço de física calcula uma força de sustentação vertical positiva e aumenta ligeiramente o coeficiente de arrasto frontal.
2. **Given** a aeronave em altitude elevada, **When** o jogador inclina o nariz para baixo (mergulho), **Then** a sustentação diminui, a gravidade acelera a aeronave e a velocidade horizontal aumenta progressivamente.

---

### User Story 2 - Influência da Melhoria de Aerodinâmica no Arrasto (Priority: P2)

Como jogador que investiu em melhorias de aerodinâmica na oficina, desejo que meu avião sofra menos arrasto do ar para que possa manter velocidade e planar por distâncias significativamente maiores.

**Why this priority**: Conecta a progressão do jogador diretamente com a física do jogo, tornando as melhorias imediatamente perceptíveis no voo.

**Independent Test**: Testável comparando a taxa de desaceleração de uma aeronave com aerodinâmica nível 1 versus nível 5 sob as mesmas condições iniciais.

**Acceptance Scenarios**:
1. **Given** duas aeronaves lançadas à mesma velocidade inicial, **When** a aeronave com maior nível de aerodinâmica plana livremente, **Then** seu coeficiente de arrasto é menor e ela percorre uma distância horizontal maior antes de perder sustentação.

---

### Edge Cases

- Velocidade horizontal próxima de zero: a sustentação cai a zero e a aeronave entra em estol (*stall*), caindo sob ação direta da gravidade.
- Ângulos extremos de inclinação (ex: > 80° para cima): o arrasto atinge o pico e a velocidade decai rapidamente.
- Altitude negativa: deve ser travada no solo (altitude = 0).

---

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: O sistema DEVE fornecer o serviço `IServicoFisicaVoo` com método puro para cálculo das forças que atuam no avião a cada delta de tempo ($dt$).
- **FR-002**: A força de sustentação ($L$) DEVE ser proporcional ao quadrado da velocidade e ao coeficiente de sustentação baseado no ângulo de ataque: $L = \frac{1}{2} \cdot \rho \cdot v^2 \cdot S \cdot C_L(\alpha)$.
- **FR-003**: A força de arrasto ($D$) DEVE ser calculada considerando a redução proporcionada pelo nível de aerodinâmica: $D = \frac{1}{2} \cdot \rho \cdot v^2 \cdot S \cdot \frac{C_D(\alpha)}{1 + (\text{NivelAerodinamica} - 1) \times 0.20}$.
- **FR-004**: O sistema DEVE aplicar aceleração da gravidade padrão ($9.81\text{ m/s}^2$ para baixo).
- **FR-005**: O caso de uso `AtualizarFisicaVooCasoDeUso` DEVE atualizar o vetor de posição e velocidade da aeronave sem alocar novos objetos na memória durante a execução contínua.

### Key Entities

- **`EstadoFisicoAeronave`**: Objeto de valor contendo posição, velocidade vetorial, ângulo atual e aceleração resultante.
- **`ParametrosControlePiloto`**: Comando de entrada do jogador contendo a intensidade de inclinação de pitch (-1.0 a +1.0).

---

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Execução do cálculo físico de um passo de simulação em menos de 0.05ms (permitindo folga massiva para 60 FPS / 16ms por frame).
- **SC-002**: Zero alocação de memória no heap (`GC Alloc = 0 bytes`) durante a chamada contínua de atualização física.
- **SC-003**: Conservação consistente de energia mecânica em simulações sem propulsão externa.

---

## Assumptions

- O ar possui densidade atmosférica padrão constante ao nível do mar nas fases iniciais do jogo.
- O modelo aerodinâmico é bidimensional projetado no plano de voo (X horizontal, Y vertical, Z alinhado).
