# Feature Specification: Detecção de Pouso e Transição de Fim de Voo

**Feature Branch**: `006-deteccao-pouso-fim-voo`  
**Created**: 2026-09-04  
**Status**: Ready for Planning  
**Input**: User description: "006 - Detecção de Pouso no Solo, Parada de Aeronave e Transição de Fim de Voo."

---

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Contato com o Solo e Desaceleração por Atrito (Priority: P1)

Como jogador quando a aeronave esgota sua sustentação e toca a superfície do solo, desejo que o avião deslize suavemente perdendo velocidade gradualmente por atrito até a parada completa, proporcionando um encerramento natural e satisfatório do voo.

**Why this priority**: Evita paradas bruscas e garante a física de aterrissagem esperada em jogos casuais de lançamento.

**Independent Test**: Testável acionando a função de contato com solo e conferindo a aplicação contínua de força de atrito e restrição de altitude mínima (não atravessar o solo).

**Acceptance Scenarios**:
1. **Given** a aeronave em voo descendente, **When** sua altitude atinge zero (contato com solo), **Then** a velocidade vertical é anulada e a velocidade horizontal decresce gradualmente conforme o coeficiente de atrito do solo.
2. **Given** a aeronave deslizando no solo com velocidade horizontal menor que o limiar de parada (ex: < 0.2 m/s), **When** a velocidade atinge zero, **Then** a simulação de movimento é congelada.

---

### User Story 2 - Transição de Estado para Fim de Voo (Priority: P2)

Como sistema de jogo, ao confirmar a parada completa da aeronave, devo transitar o estado da sessão de voo de `EmVoo` para `Pousado` e congelar as métricas finais de distância e altitude para cálculo de pontuação.

**Why this priority**: É o gatilho formal de encerramento que orquestra a abertura da tela de resultados e o processamento de recompensas.

**Independent Test**: Testável validando a transição do enum `StatusVoo` para `Pousado` no momento da parada total e a emissão do evento de voo finalizado.

**Acceptance Scenarios**:
1. **Given** um voo ativo, **When** a aeronave para completamente no solo, **Then** a entidade `Voo` atualiza seu status para `Pousado`, registra a distância final e dispara o evento de término de voo.

---

### Edge Cases

- Aterrissagem com velocidade vertical muito alta (queda em mergulho): deve quicar suavemente ou frear com atrito sem gerar valores de altitude negativos.
- Aeronave com combustível restante no momento do pouso: o motor deve ser desligado automaticamente e o combustível remanescente não deve ser mais consumido.
- Tentativa de reativar controles táteis após o pouso: todos os comandos de voo devem ser desabilitados.

---

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: O sistema DEVE detectar o evento de contato com o solo quando a altitude vertical da aeronave for menor ou igual ao nível do terreno ($Y \le 0$).
- **FR-002**: O sistema DEVE aplicar desaceleração por atrito terrestre calculada por $a_{\text{atrito}} = \mu \cdot g$ enquanto a aeronave estiver em contato com o solo e em movimento.
- **FR-003**: O sistema DEVE declarar a parada da aeronave quando a velocidade escalar resultante for inferior a $0.15\text{ m/s}$.
- **FR-004**: Ao detectar a parada total, o sistema DEVE transitar o `StatusVoo` para `Pousado` e travar a distância final percorrida.
- **FR-005**: O sistema DEVE emitir notificação de evento de voo concluído para consumo dos serviços de economia e apresentação.

### Key Entities

- **`ParametrosPouso`**: Objeto de valor contendo coeficiente de atrito do solo ($\mu$) e limiar de parada.
- **`ResultadoFimVoo`**: Objeto de valor contendo a distância final percorrida, altitude máxima alcançada e moedas coletadas.

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
