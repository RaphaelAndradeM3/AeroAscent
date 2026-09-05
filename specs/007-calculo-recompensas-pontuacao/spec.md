# Feature Specification: Cálculo de Recompensas, Conversão de Moedas e Recordes

**Feature Branch**: `007-calculo-recompensas-pontuacao`  
**Created**: 2026-09-04  
**Status**: Ready for Planning  
**Input**: User description: "007 - Caso de Uso de Finalização de Voo, Conversão de Distância/Altitude em Moedas e Registro de Recordes."

---

## Clarifications

### Session 2026-09-05
- Q: Como a orquestração do caso de uso FinalizarVooCasoDeUso e a persistência de progresso devem ser estruturadas na camada de Aplicação? → A: Opção A — Orquestração assíncrona com injeção de IRepositorioProgresso: FinalizarVooCasoDeUso.ExecutarAsync(Voo voo, CancellationToken ct) obtém o ProgressoJogador via repositório, credita saldo, atualiza recordes, persiste atomicamente via SalvarProgressoAsync e retorna ResumoFinalizacaoVoo.
- Q: Como o objeto ResumoFinalizacaoVoo deve ser modelado e quais informações de recorde deve discriminar? → A: Opção A — Modelar ResumoFinalizacaoVoo como readonly record struct na stack (GC Alloc = 0 bytes), discriminando métricas (DistanciaMetros, AltitudeMaximaMetros), fontes de moedas (MoedasPorDistancia, MoedasPorAltitude, MoedasColetadas, MoedasTotalGanhas, SaldoTotalAtualizado) e flags booleanas para ambos os recordes (EhNovoRecordeDistancia e EhNovoRecordeAltitude).
- Q: Como o sistema deve se comportar ao receber uma chamada repetida de finalização para um voo já liquidado? → A: Opção A — Retorno idempotente seguro: a entidade Voo registra PremiacaoLiquidada = true; chamadas subsequentes retornam o mesmo ResumoFinalizacaoVoo consolidado sem creditar moedas adicionais nem reincrementar o total de voos no ProgressoJogador.
- Q: Como o caso de uso FinalizarVooCasoDeUso deve se comportar em relação ao status da sessão de Voo informada? → A: Opção A — Validação rigorosa de ciclo de vida: exige StatusVoo.Pousado para creditar moedas e atualizar recordes; se Cancelado, retorna resumo com 0 moedas ganhas e sem novos recordes; se EmPreparacao ou EmVoo, lança DominioInvalidoException.
- Q: Como o caso de uso FinalizarVooCasoDeUso deve se comportar caso o repositório retorne nulo (primeira execução)? → A: Opção A — Criação resiliente automática: se CarregarProgressoAsync retornar null, instancia ProgressoJogador.CriarNovo(), credita as recompensas, registra os recordes e salva atomicamente via SalvarProgressoAsync.

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

- Finalização de voo já previamente liquidado: opera de forma estritamente idempotente via flag `PremiacaoLiquidada = true` em `Voo`, retornando o `ResumoFinalizacaoVoo` idêntico sem conceder moedas duplicadas nem reincrementar `TotalVoosRealizados`.
- Voo com status prematuro (`EmPreparacao` ou `EmVoo`): lança `DominioInvalidoException` protegendo contra encerramentos antes do pouso da aeronave; se `Cancelado`, liquida com 0 moedas ganhas sem alteração de recordes.
- Primeira execução sem dados salvos no repositório (`CarregarProgressoAsync` retorna `null`): instancia automaticamente `ProgressoJogador.CriarNovo()`, aplicando as recompensas do voo inicial e persistindo o novo perfil sem erros.
- Sessão de voo sem movimentação (distância = 0): concede 0 moedas e não corrompe os dados existentes.

---

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: O sistema DEVE fornecer o caso de uso `FinalizarVooCasoDeUso` (`IFinalizarVooCasoDeUso`) na camada de Aplicação, recebendo a entidade `Voo` e injetando `IRepositorioProgresso` para carregar o `ProgressoJogador`, creditar a recompensa, registrar novos recordes e persistir atomicamente via `SalvarProgressoAsync`.
- **FR-002**: O cálculo da recompensa em moedas DEVE seguir a fórmula matemática:
  $$\text{MoedasGanhas} = \lfloor \text{DistanciaEmMetros} \times 0.1 \rfloor + \lfloor \text{AltitudeMaximaEmMetros} \times 0.05 \rfloor + \text{MoedasColetadasEmVoo}$$
- **FR-003**: O sistema DEVE creditar `MoedasGanhas` diretamente no saldo acumulado do jogador através da entidade `ProgressoJogador` ou `Oficina`.
- **FR-004**: O sistema DEVE comparar a distância do voo com o recorde atual e atualizar o valor sempre que a nova distância for estritamente superior.
- **FR-005**: O sistema DEVE retornar a struct imutável na stack `ResumoFinalizacaoVoo` (`readonly record struct`) discriminando `DistanciaMetros`, `AltitudeMaximaMetros`, `MoedasPorDistancia`, `MoedasPorAltitude`, `MoedasColetadas`, `MoedasTotalGanhas`, `SaldoTotalAtualizado`, `EhNovoRecordeDistancia` e `EhNovoRecordeAltitude`.
- **FR-006**: O sistema DEVE garantir execução estritamente idempotente através da propriedade `PremiacaoLiquidada` na entidade `Voo`, retornando o `ResumoFinalizacaoVoo` consolidado sem creditar moedas adicionais em invocações repetidas.
- **FR-007**: O sistema DEVE validar o status da sessão de `Voo`: processando a premiação integral apenas quando `StatusVoo.Pousado`, liquidando com 0 moedas quando `StatusVoo.Cancelado` e lançando `DominioInvalidoException` caso o voo esteja em `EmPreparacao` ou `EmVoo`.
- **FR-008**: O sistema DEVE tratar a ausência de perfil salvo no repositório (`CarregarProgressoAsync` retornando `null`) instanciando automaticamente um novo perfil via `ProgressoJogador.CriarNovo()`, aplicando as recompensas e persistindo atomicamente.

### Key Entities

- **`ResumoFinalizacaoVoo`**: Struct na stack (`readonly record struct`, `GC Alloc = 0 bytes`) contendo métricas consolidadas, detalhamento das fontes de moedas, saldo final atualizado e flags booleanas para novos recordes de distância e altitude.
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
