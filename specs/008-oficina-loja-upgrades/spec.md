# Feature Specification: Loja e Oficina de Upgrades da Aeronave

**Feature Branch**: `008-oficina-loja-upgrades`  
**Created**: 2026-09-04  
**Status**: Ready for Planning  
**Input**: User description: "008 - Sistema de Compra e Evolução de Melhorias (Motor, Aerodinâmica, Tanque, Catapulta com curva de custo exponencial)."

---

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Compra de Melhorias Mecânicas com Saldo de Moedas (Priority: P1)

Como jogador no menu da oficina, desejo gastar minhas moedas acumuladas para aumentar o nível de um dos 4 componentes da aeronave (Motor, Aerodinâmica, Tanque de Combustível, Catapulta) a fim de melhorar o desempenho nos próximos voos.

**Why this priority**: É o ponto focal de progressão de curto, médio e longo prazo do jogador.

**Independent Test**: Testável acionando o caso de uso `ComprarMelhoriaCasoDeUso` com saldo suficiente e validando o débito do custo e o incremento no nível do componente solicitado.

**Acceptance Scenarios**:
1. **Given** um jogador com saldo de 200 moedas e Motor no nível 1 cujo custo para o nível 2 é de 100 moedas, **When** ele compra a melhoria de Motor, **Then** o Motor evolui para o nível 2, o saldo é debitado para 100 moedas e o novo custo para o nível 3 é calculado e apresentado.
2. **Given** um jogador com saldo de 30 moedas e Tanque no nível 1 cujo custo é de 100 moedas, **When** ele tenta comprar a melhoria, **Then** a operação é rejeitada com `SaldoInsuficienteException` e o saldo permanece intacto em 30 moedas.

---

### User Story 2 - Cálculo Escalonado Exponencial de Custos de Upgrade (Priority: P2)

Como regra de balanceamento econômico, cada nível subsequente de melhoria deve custar progressivamente mais conforme a fórmula exponencial do projeto, oferecendo desafio balanceado sem frustração.

**Why this priority**: Mantém a curva de longevidade e progressão do jogo justa e alinhada com o PRD.

**Independent Test**: Testável validando os custos gerados para os níveis 1, 2, 3, 4 e 5 de cada tipo de melhoria.

**Acceptance Scenarios**:
1. **Given** um custo base de 100 moedas para um componente, **When** calculamos o custo para o nível $N$, **Then** o custo segue rigorosamente $\text{Custo}(N) = \lfloor \text{CustoBase} \times (1.5)^{N-1} \rfloor$ (Ex: N1 $\to$ N2: 100, N2 $\to$ N3: 150, N3 $\to$ N4: 225, N4 $\to$ N5: 337).

---

### Edge Cases

- Tentativa de evoluir além do nível máximo permitido (se houver teto, ex: nível 20): deve lançar `MelhoriaNivelMaximoException`.
- Tentativa de comprar tipo de melhoria desconhecido/inválido: deve falhar com erro de validação.
- Concorrência de compras consecutivas rápidas: o saldo deve ser decrementado atomicamente sem inconsistências.

---

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: O sistema DEVE fornecer o caso de uso `ComprarMelhoriaCasoDeUso` recebendo o identificador da melhoria desejada (`TipoMelhoria`: `Motor`, `Aerodinamica`, `TanqueCombustivel`, `Catapulta`).
- **FR-002**: O sistema DEVE calcular o custo de evolução para o próximo nível com a fórmula:
  $$\text{Custo}(N) = \lfloor \text{CustoBase} \times 1.5^{N-1} \rfloor$$
- **FR-003**: O sistema DEVE validar se o saldo atual do jogador é maior ou igual ao custo da melhoria; caso contrário, DEVE lançar `SaldoInsuficienteException`.
- **FR-004**: Ao concluir a compra com sucesso, o sistema DEVE decrementar o saldo em `Moeda`, incrementar o nível da `Aeronave` e persistir as alterações via `IRepositorioProgresso`.
- **FR-005**: O sistema DEVE fornecer método de consulta que retorne a lista de todas as melhorias disponíveis, seus níveis atuais, custos para a próxima evolução e se o jogador tem saldo suficiente para comprar cada uma.

### Key Entities

- **`TipoMelhoria`**: Enumeração contendo `Motor`, `Aerodinamica`, `TanqueCombustivel` e `Catapulta`.
- **`ItemOficinaDTO`**: Objeto de transferência de dados com nome, descrição, nível atual, custo do próximo nível e flag `PodeComprar`.

---

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: 100% de testes unitários aprovados para compras válidas, rejeições por saldo insuficiente e cálculos de custos escalonados.
- **SC-002**: Execução completa da operação de compra e persistência em menos de 5 milissegundos.
- **SC-003**: Impossibilidade matemática de saldo negativo ou duplicação de níveis.

---

## Assumptions

- Cada tipo de melhoria inicia no nível 1 para novos jogadores.
- Os custos base são definidos por configuração do domínio (padrão: 100 moedas para cada categoria).
