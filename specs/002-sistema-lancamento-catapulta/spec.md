# Feature Specification: Sistema de Lançamento Inicial e Catapulta

**Feature Branch**: `002-sistema-lancamento-catapulta`  
**Created**: 2026-09-04  
**Status**: Ready for Planning  
**Input**: User description: "002 - Sistema de Lançamento Inicial e Catapulta: Mecânica de Lançamento com precisão de força, ângulo e impulso vetorial inicial baseado no nível da catapulta."

---

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Lançamento da Aeronave pela Catapulta (Priority: P1)

Como jogador no início de um novo voo, desejo ajustar o momento de disparo na rampa de lançamento para imprimir a maior força inicial possível na aeronave e começar a trajetória com velocidade máxima.

**Why this priority**: O lançamento é o primeiro passo interativo de qualquer rodada do jogo e define a velocidade e energia cinética inicial da aeronave.

**Independent Test**: Pode ser testado acionando o caso de uso `LancarAeronaveCasoDeUso` com diferentes coeficientes de precisão e validando a velocidade resultante aplicada à aeronave.

**Acceptance Scenarios**:
1. **Given** que a aeronave está posicionada na catapulta no nível 1, **When** o jogador realiza o disparo no ápice da barra de força (100% de precisão), **Then** a aeronave é impulsionada com o vetor de velocidade inicial máxima para aquele nível e o status do voo muda para `EmVoo`.
2. **Given** uma catapulta no nível 3 (evoluída na oficina), **When** o disparo é realizado com 100% de precisão, **Then** a velocidade inicial aplicada é proporcionalmente maior do que a do nível 1 conforme o multiplicador da catapulta.

---

### User Story 2 - Variação de Eficácia do Lançamento por Temporização (Priority: P2)

Como jogador, desejo que o timing do meu toque influencie a força do disparo para que a habilidade e precisão sejam recompensadas com um início de voo superior.

**Why this priority**: Proporciona o elemento de mecânica de habilidade logo nos primeiros segundos de jogo.

**Independent Test**: Testável fornecendo valores de precisão variados (ex: 30%, 75%, 100%) e conferindo que o impulso gerado é escalonado correspondentemente.

**Acceptance Scenarios**:
1. **Given** um medidor oscilante de força, **When** o jogador aciona o botão em um ponto de precisão de 50%, **Then** a força inicial resultante é exatamente 50% da força máxima disponível para o nível atual da catapulta.

---

### Edge Cases

- Tentativa de lançar aeronave já em voo ou finalizada deve ser ignorada ou rejeitada.
- Parâmetro de precisão menor que 0 ou maior que 1 deve ser normalizado ou rejeitado.
- Ângulo de lançamento fixo ou configurável deve permanecer dentro dos limites físicos válidos (ex: entre 15° e 60°).

---

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: O sistema DEVE fornecer o caso de uso `LancarAeronaveCasoDeUso` para converter a precisão do disparo e os atributos da catapulta em um vetor de lançamento.
- **FR-002**: A força do lançamento DEVE ser calculada pela fórmula base $\text{ForcaInicial} = \text{ForcaBase} \times (1 + (\text{NivelCatapulta} - 1) \times 0.25) \times \text{Precisao}$.
- **FR-003**: O ângulo padrão de lançamento DEVE ser configurado para 35 graus em relação ao horizonte, projetando o vetor de velocidade inicial nos eixos X e Y.
- **FR-004**: Ao disparar com sucesso, a sessão de `Voo` DEVE transitar seu estado para `EmVoo` e registrar a hora/tempo de início.
- **FR-005**: O sistema DEVE garantir que nenhum consumo de combustível ocorra durante a fase de disparo da catapulta.

### Key Entities

- **`ParametrosLancamento`**: Objeto de valor contendo a precisão (0.0 a 1.0) e o ângulo de saída.
- **`ResultadoLancamento`**: Objeto de valor contendo o vetor de velocidade inicial e o status de sucesso.

---

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: 100% dos testes unitários de cálculo vetorial de lançamento executados com precisão matemática em menos de 100ms.
- **SC-002**: Resposta instantânea da transição de estado da catapulta para voo em menos de 16ms (1 frame a 60 FPS).
- **SC-003**: Aumento linear e consistente de velocidade inicial comprovado a cada nível de catapulta.

---

## Assumptions

- A física da Unity ou simulador desacoplado receberá o vetor inicial e assumirá a continuidade da trajetória balística.
- O ângulo da catapulta permanece constante na primeira versão, permitindo foco exclusivo na precisão de disparo do jogador.
