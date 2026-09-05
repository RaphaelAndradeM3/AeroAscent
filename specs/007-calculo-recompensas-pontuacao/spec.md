# Feature Specification: Cálculo de Recompensas, Conversão de Moedas e Recordes

**Feature Branch**: `007-calculo-recompensas-pontuacao`  
**Created**: 2026-09-04  
**Status**: Ready for Planning  
**Input**: User description: "007 - Caso de Uso de Finalização de Voo, Conversão de Distância/Altitude em Moedas e Registro de Recordes."

---

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Conversão de Métricas de Voo em Moedas de Recompensa (Priority: P1)

Como jogador ao término de uma sessão de voo, desejo que minha distância percorrida, altitude máxima atingida e moedas coletadas sejam convertidas com precisão em moedas de recompensa adicionadas ao meu saldo principal, para que eu possa investir na oficina.

**Why this priority**: É a espinha dorsal da economia e da progressão do jogo (Core Loop).

**Independent Test**: Testável invocando o caso de uso `FinalizarVooCasoDeUso` com diferentes combinações de distância, altitude e coletáveis, conferindo a pontuação calculada e o saldo resultante.

**Acceptance Scenarios**:
1. **Given** um voo com distância de 250m, altitude máxima de 80m e 5 moedas coletadas no ar, **When** o caso de uso `FinalizarVooCasoDeUso` é executado, **Then** as moedas ganhas são $\lfloor 250 \times 0.1 \rfloor + \lfloor 80 \times 0.05 \rfloor + 5 = 25 + 4 + 5 = 34$ moedas, e o saldo do jogador é acrescido em exatamente 34 moedas.
2. **Given** um voo muito curto com 8m de distância e 2m de altitude e 0 coletáveis, **When** o voo é finalizado, **Then** a recompensa calculada é $\lfloor 0.8 \rfloor + \lfloor 0.1 \rfloor + 0 = 0$ moedas adicionais sem erro ou saldo negativo.

---

### User Story 2 - Verificação e Atualização de Recorde Pessoal (Priority: P2)

Como jogador que superou sua maior distância anterior, desejo que o jogo identifique o novo recorde pessoal e marque a sessão como recorde histórico.

**Why this priority**: Gera sentimento de conquista e engajamento para a família e jogadores.

**Independent Test**: Testável finalizando um voo com distância superior ao recorde salvo e validando a flag `EhNovoRecorde` e a atualização do valor de recorde no repositório.

**Acceptance Scenarios**:
1. **Given** um recorde anterior de 300 metros, **When** o jogador pousa aos 350 metros, **Then** o sistema marca `EhNovoRecorde = true`, atualiza o recorde salvo para 350 metros e persiste a alteração.
2. **Given** um recorde anterior de 300 metros, **When** o jogador pousa aos 280 metros, **Then** o sistema marca `EhNovoRecorde = false` e preserva o recorde de 300 metros intacto.

---

### Edge Cases

- Finalização de voo já previamente finalizado: deve rejeitar reprocessamento de recompensas para evitar duplicação indevida de saldo.
- Sessão de voo sem movimentação (distância = 0): concede 0 moedas e não corrompe os dados existentes.

---

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: O sistema DEVE fornecer o caso de uso `FinalizarVooCasoDeUso` orquestrando o fechamento da sessão de voo e a atualização do progresso do jogador.
- **FR-002**: O cálculo da recompensa em moedas DEVE seguir a fórmula matemática:
  $$\text{MoedasGanhas} = \lfloor \text{DistanciaEmMetros} \times 0.1 \rfloor + \lfloor \text{AltitudeMaximaEmMetros} \times 0.05 \rfloor + \text{MoedasColetadasEmVoo}$$
- **FR-003**: O sistema DEVE creditar `MoedasGanhas` diretamente no saldo acumulado do jogador através da entidade `ProgressoJogador` ou `Oficina`.
- **FR-004**: O sistema DEVE comparar a distância do voo com o recorde atual e atualizar o valor sempre que a nova distância for estritamente superior.
- **FR-005**: O sistema DEVE retornar um DTO/Record `ResumoFinalizacaoVoo` contendo distância, altitude, moedas ganhas discriminadas por fonte, saldo total atualizado e booleano indicativo de novo recorde.

### Key Entities

- **`ResumoFinalizacaoVoo`**: Objeto de valor com o detalhamento completo dos ganhos e status de recorde.
- **`ProgressoJogador`**: Entidade contendo saldo acumulado, recordes e aeronave configurada.

---

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: 100% de exatidão matemática comprovada em bateria de testes unitários para a fórmula de recompensas.
- **SC-002**: Execução completa do caso de uso de finalização em menos de 2 milissegundos.
- **SC-003**: Garantia de idempotência: chamar finalização duas vezes para o mesmo voo não duplica saldo.

---

## Assumptions

- O arredondamento é sempre para baixo (*floor / int cast*) conforme estabelecido no PRD.
- O saldo de moedas do jogador é persistido no repositório de progresso logo após o crédito.
