# Feature Specification: Detecção de Pouso e Transição de Fim de Voo

**Feature Branch**: `006-deteccao-pouso-fim-voo`  
**Created**: 2026-09-04  
**Status**: Ready for Planning  
**Input**: User description: "006 - Detecção de Pouso no Solo, Parada de Aeronave e Transição de Fim de Voo."

---

## Clarifications

### Session 2026-09-05
- Q: Como a detecção de parada no solo e a transição formal para o status Pousado devem ser orquestradas entre as camadas de Domínio e Aplicação? → A: Opção A — Criar o caso de uso dedicado `ProcessarPousoFimVooCasoDeUso` na camada de Aplicação, responsável por inspecionar `estado.NoSolo` e $V_z == 0$, invocar `voo.Pousar()` e retornar a struct imutável `ResultadoFimVoo`.
- Q: Qual deve ser o valor canônico oficial do limiar de velocidade para congelar o movimento e declarar a parada da aeronave? → A: Opção A — Fixar o limiar canônico em 0.15 m/s, atualizando a constante `VELOCIDADE_LIMIAR_PARADA_SOLO` e alinhando todos os requisitos e testes.
- Q: No momento exato em que a aeronave toca o solo (Y <= 0), como o sistema físico deve responder em termos de velocidade vertical (Vy) e atitude angular da fuselagem (pitch)? → A: Opção A — Absorção total de impacto vertical (Vy = 0, Y = 0) com nivelamento contínuo e suave do pitch até 0° durante o deslizamento por atrito, sem penetração de terreno.
- Q: O requisito FR-005 estabelece que o sistema deve emitir uma notificação de voo concluído para consumo dos serviços de economia e apresentação visual/UI. Como essa notificação de evento deve ser modelada na Clean Architecture? → A: Opção A — Notificação via interface de contrato `IPublicadorEventosVoo` injetada no caso de uso `ProcessarPousoFimVooCasoDeUso`, além do retorno síncrono da struct `ResultadoFimVoo`.
- Q: Ao tocar o solo (NoSolo == true) e durante o deslizamento até a parada completa, como o propulsor e as tentativas de comando do piloto (pitch/boost) devem se comportar? → A: Opção A — Ao tocar o solo (NoSolo == true) ou entrar em Pousado, o propulsor é cortado imediatamente (EstadoPropulsor.CriarInativo), a queima de combustível é cessada e qualquer comando do piloto é ignorado pela simulação física.

---

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Contato com o Solo e Desaceleração por Atrito (Priority: P1)

Como jogador quando a aeronave esgota sua sustentação e toca a superfície do solo, desejo que o avião deslize suavemente perdendo velocidade gradualmente por atrito até a parada completa, proporcionando um encerramento natural e satisfatório do voo.

**Why this priority**: Evita paradas bruscas e garante a física de aterrissagem esperada em jogos casuais de lançamento.

**Independent Test**: Testável acionando a função de contato com solo e conferindo a aplicação contínua de força de atrito e restrição de altitude mínima (não atravessar o solo).

**Acceptance Scenarios**:
1. **Given** a aeronave em voo descendente, **When** sua altitude atinge zero (contato com solo), **Then** a velocidade vertical é anulada e a velocidade horizontal decresce gradualmente conforme o coeficiente de atrito do solo.
2. **Given** a aeronave deslizando no solo com velocidade horizontal menor que o limiar de parada (< 0.15 m/s), **When** a velocidade atinge zero, **Then** a simulação de movimento é congelada.

---

### User Story 2 - Transição de Estado para Fim de Voo (Priority: P2)

Como sistema de jogo, ao confirmar a parada completa da aeronave, devo transitar o estado da sessão de voo de `EmVoo` para `Pousado` e congelar as métricas finais de distância e altitude para cálculo de pontuação.

**Why this priority**: É o gatilho formal de encerramento que orquestra a abertura da tela de resultados e o processamento de recompensas.

**Independent Test**: Testável validando a transição do enum `StatusVoo` para `Pousado` no momento da parada total e a emissão do evento de voo finalizado.

**Acceptance Scenarios**:
1. **Given** um voo ativo, **When** a aeronave para completamente no solo, **Then** a entidade `Voo` atualiza seu status para `Pousado`, registra a distância final e dispara o evento de término de voo.

---

### Edge Cases

- Aterrissagem com velocidade vertical muito alta (queda em mergulho): absorção imediata com $Y=0, V_y=0$ e desaceleração pura no eixo Z, sem quiques caóticos ou valores negativos de altitude.
- Aeronave com combustível restante no momento do pouso ou deslizamento no solo: o propulsor é desligado imediatamente (`EstadoPropulsor.CriarInativo`) e o consumo de combustível é cessado.
- Tentativa de reativar controles táteis (pitch ou boost) no solo ou após o pouso: todos os comandos de voo são bloqueados e ignorados pelo motor de simulação.

---

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: O sistema DEVE detectar o evento de contato com o solo quando a altitude vertical da aeronave for menor ou igual ao nível do terreno ($Y \le 0$).
- **FR-002**: O sistema DEVE aplicar desaceleração por atrito terrestre calculada por $a_{\text{atrito}} = \mu \cdot g$ enquanto a aeronave estiver em contato com o solo e em movimento.
- **FR-003**: O sistema DEVE declarar a parada da aeronave quando a velocidade escalar resultante for inferior a $0.15\text{ m/s}$.
- **FR-004**: Ao detectar a parada total, o sistema DEVE transitar o `StatusVoo` para `Pousado` e travar a distância final percorrida via `ProcessarPousoFimVooCasoDeUso`.
- **FR-005**: O sistema DEVE emitir notificação de evento de voo concluído via `IPublicadorEventosVoo` para consumo dos serviços de economia e apresentação.

### Key Entities

- **`ParametrosPouso`**: Objeto de valor contendo coeficiente de atrito do solo ($\mu$) e limiar de parada.
- **`ResultadoFimVoo`**: Struct imutável na stack encapsulando status de parada, distância final percorrida, altitude máxima alcançada, moedas coletadas e o `ResultadoVoo`.
- **`IPublicadorEventosVoo`**: Interface de contrato para despacho desacoplado do evento de pouso e fim de voo.

---

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Transição de parada e encerramento de voo em 100% dos testes sem anomalias de colisão ou transpassamento de terreno.
- **SC-002**: Disparo do evento de conclusão em menos de 10ms após a parada total.
- **SC-003**: Zero alocação de memória no heap durante a fase de deslizamento e parada.

---

## Assumptions

- O solo é modelado como plano horizontal contínuo ($Y = 0$) no MVP.
- O atrito terrestre desacelera linearmente a aeronave até a velocidade zero.
