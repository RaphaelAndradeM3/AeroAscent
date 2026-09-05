# Feature Specification: Loja e Oficina de Upgrades da Aeronave

**Feature Branch**: `008-oficina-loja-upgrades`  
**Created**: 2026-09-04  
**Status**: Implemented  
**Input**: User description: "008 - Sistema de Compra e Evolução de Melhorias (Motor, Aerodinâmica, Tanque, Catapulta com curva de custo exponencial)."

---

## Clarifications

### Session 2026-09-05
- Q: Qual tabela de custos base deve ser a canônica da oficina? → A: Opção A — Adotar a tabela de custos base calibrada já presente na entidade Oficina.cs (Motor: 50, Aerodinâmica: 40, Tanque: 30, Catapulta: 60), aplicando rigorosamente a fórmula exponencial floor(CustoBase * 1.5^(N-1)).
- Q: Qual o teto máximo de nível para as melhorias dos componentes? → A: Opção A — Fixar o nível 10 como teto máximo de evolução para todos os 4 componentes (NIVEL_MAXIMO = 10), alinhado a Aeronave e Melhoria, disparando MelhoriaNivelMaximoException ao tentar evoluir além do nível 10.
- Q: Como as operações da oficina devem ser distribuídas na camada de Aplicação? → A: Opção A — Dois casos de uso dedicados (SRP / CQRS leve): ComprarMelhoriaCasoDeUso (IComprarMelhoriaCasoDeUso) para mutação e persistência atômica, e ConsultarOficinaCasoDeUso (IConsultarOficinaCasoDeUso) para leitura do catálogo e projeção dos DTOs.
- Q: Como o catálogo de consulta (ItemOficinaDTO) deve projetar um componente que atingiu o nível máximo 10? → A: Opção A — Projetar com CustoProximoNivel = null, PodeComprar = false e flag EstaNoNivelMaximo = true, mantendo o item visível na loja com selo de conclusão e botão de compra desabilitado.
- Q: Como os casos de uso da oficina devem se comportar caso o repositório retorne nulo (primeira execução)? → A: Opção A — Inicialização resiliente automática com ProgressoJogador.CriarNovo() (componentes no nível 1 e saldo 0), permitindo consultar a oficina e validar compras imediatamente sem exceções de inicialização.

---

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Compra de Melhorias Mecânicas com Saldo de Moedas (Priority: P1)

Como jogador no menu da oficina, desejo gastar minhas moedas acumuladas para aumentar o nível de um dos 4 componentes da aeronave (Motor, Aerodinâmica, Tanque de Combustível, Catapulta) a fim de melhorar o desempenho nos próximos voos.

**Why this priority**: É o ponto focal de progressão de curto, médio e longo prazo do jogador.

**Independent Test**: Testável acionando o caso de uso `ComprarMelhoriaCasoDeUso` com saldo suficiente e validando o débito do custo e o incremento no nível do componente solicitado.

**Acceptance Scenarios**:
1. **Given** um jogador com saldo de 200 moedas e Motor no nível 1 cujo custo para o nível 2 é de 50 moedas, **When** ele compra a melhoria de Motor, **Then** o Motor evolui para o nível 2, o saldo é debitado para 150 moedas e o novo custo para o nível 3 (75 moedas) é calculado e apresentado.
2. **Given** um jogador com saldo de 20 moedas e Tanque no nível 1 cujo custo é de 30 moedas, **When** ele tenta comprar a melhoria, **Then** a operação é rejeitada com `SaldoInsuficienteException` e o saldo permanece intacto em 20 moedas.
3. **Given** um jogador com Motor no nível 10, **When** ele consulta a oficina, **Then** o Motor exibe NivelAtual = 10, CustoProximoNivel = null, EstaNoNivelMaximo = true e PodeComprar = false; e caso tente comprar, é lançada a exceção `MelhoriaNivelMaximoException`.

---

### User Story 2 - Cálculo Escalonado Exponencial de Custos de Upgrade (Priority: P2)

Como regra de balanceamento econômico, cada nível subsequente de melhoria deve custar progressivamente mais conforme a fórmula exponencial do projeto, oferecendo desafio balanceado sem frustração.

**Why this priority**: Mantém a curva de longevidade e progressão do jogo justa e alinhada com o PRD.

**Independent Test**: Testável validando os custos gerados para os níveis 1, 2, 3, 4 e 5 de cada tipo de melhoria com base em seus respectivos custos base.

**Acceptance Scenarios**:
1. **Given** o custo base de 50 moedas para o Motor, **When** calculamos o custo para o nível $N$, **Then** o custo segue rigorosamente $\text{Custo}(N) = \lfloor 50 \times (1.5)^{N-1} \rfloor$ (Ex: N1 $\to$ N2: 50, N2 $\to$ N3: 75, N3 $\to$ N4: 112, N4 $\to$ N5: 168).
2. **Given** o custo base de 30 moedas para o Tanque, **When** calculamos o custo para o nível $N$, **Then** o custo segue $\text{Custo}(N) = \lfloor 30 \times (1.5)^{N-1} \rfloor$ (Ex: N1 $\to$ N2: 30, N2 $\to$ N3: 45, N3 $\to$ N4: 67).

---

### Edge Cases

- Tentativa de evoluir além do nível máximo permitido (nível 10): deve lançar estritamente `MelhoriaNivelMaximoException`, preservando o saldo e o estado da aeronave inalterados.
- Tentativa de comprar tipo de melhoria desconhecido/inválido: deve falhar com erro de validação (`DominioInvalidoException`).
- Primeira execução sem dados salvos no repositório (`CarregarProgressoAsync` retorna `null`): instancia automaticamente `ProgressoJogador.CriarNovo()`, exibindo o catálogo inicial e permitindo compras ou validações de saldo sem falhas.
- Concorrência de compras consecutivas rápidas: o saldo deve ser decrementado atomicamente sem inconsistências.

---

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: O sistema DEVE fornecer o caso de uso `ComprarMelhoriaCasoDeUso` (`IComprarMelhoriaCasoDeUso`) na camada de Aplicação, recebendo o tipo de melhoria desejada (`TipoMelhoria`: `Motor`, `Aerodinamica`, `TanqueCombustivel`, `Catapulta`), orquestrando o débito financeiro, evolução do componente na aeronave do `ProgressoJogador` e persistência atômica via `IRepositorioProgresso.SalvarProgressoAsync`.
- **FR-002**: O sistema DEVE calcular o custo de evolução para o próximo nível com a fórmula canônica:
  $$\text{Custo}(N) = \lfloor \text{CustoBase} \times 1.5^{N-1} \rfloor$$
- **FR-003**: O sistema DEVE validar se o saldo atual do jogador é maior ou igual ao custo da melhoria; caso contrário, DEVE lançar `SaldoInsuficienteException` sem alterar o saldo ou a aeronave.
- **FR-004**: Ao concluir a compra com sucesso, o sistema DEVE decrementar o saldo em `Moeda`, incrementar o nível da `Aeronave`, persistir as alterações via `IRepositorioProgresso` e retornar um extrato consolidado imutável da operação.
- **FR-005**: O sistema DEVE fornecer o caso de uso `ConsultarOficinaCasoDeUso` (`IConsultarOficinaCasoDeUso`) que retorne a lista de todas as melhorias disponíveis, seus níveis atuais, custos para a próxima evolução, flags de nível máximo e se o jogador tem saldo suficiente para comprar cada uma.
- **FR-006**: O sistema DEVE impor o teto máximo fixo de 10 níveis para cada componente (`NIVEL_MAXIMO = 10`), bloqueando compras e lançando `MelhoriaNivelMaximoException` caso o jogador tente evoluir uma peça que já atingiu o nível 10.
- **FR-007**: O sistema DEVE tratar a ausência de perfil salvo no repositório (`CarregarProgressoAsync` retornando `null`) instanciando automaticamente um perfil limpo via `ProgressoJogador.CriarNovo()`, viabilizando a consulta do catálogo e protegendo a integridade operacional.

### Key Entities

- **`TipoMelhoria`**: Enumeração contendo `Motor`, `Aerodinamica`, `TanqueCombustivel` e `Catapulta`.
- **`ItemOficinaDTO`**: Objeto de transferência de dados com tipo, nome amigável, nível atual, custo do próximo nível, flag `PodeComprar` e flag `EstaNoNivelMaximo`.
- **`ResultadoCompraMelhoria`**: Objeto de valor na stack (`readonly record struct`, `GC Alloc = 0 bytes`) detalhando o tipo evoluído, novo nível, custo debitado, saldo remanescente e se atingiu o nível máximo.

---

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: 100% de testes unitários aprovados para compras válidas, rejeições por saldo insuficiente e cálculos de custos escalonados.
- **SC-002**: Execução completa da operação de compra e persistência em menos de 5 milissegundos.
- **SC-003**: Impossibilidade matemática de saldo negativo ou duplicação de níveis.

---

## Assumptions

- Cada tipo de melhoria inicia no nível 1 para novos jogadores.
- Os custos base canônicos são definidos na entidade `Oficina`: Motor = 50, Aerodinâmica = 40, Tanque = 30 e Catapulta = 60 moedas.
