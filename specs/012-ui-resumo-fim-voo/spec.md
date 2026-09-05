# Feature Specification: Interface de Resumo de Voo e Celebração de Recorde

**Feature Branch**: `012-ui-resumo-fim-voo`  
**Created**: 2026-09-04  
**Status**: Ready for Planning  
**Input**: User description: "012 - Tela de Resumo de Fim de Voo (Animação de recompensas, celebração de recorde e redirecionamento)."

---

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Animação de Recompensas e Contagem de Moedas (Priority: P1)

Como jogador ao final do pouso, desejo que a tela de resumo de voo se abra suavemente, exibindo a distância final alcançada, a altitude máxima e uma animação empolgante de contagem crescente de moedas ganhas, para celebrar o resultado do voo.

**Why this priority**: É o momento de maior satisfação emocional e recompensa do core loop.

**Independent Test**: Testável abrindo o modal de resumo com os dados da finalização de voo e validando a sequência de animação dos números e o saldo final atualizado.

**Acceptance Scenarios**:
1. **Given** um voo recém-finalizado com 34 moedas ganhas, **When** a tela de resumo é aberta, **Then** o contador de moedas ganhas sobe animadamente de 0 até 34 acompanhado de som de contagem, e o saldo do jogador é atualizado.

---

### User Story 2 - Celebração Especial de Novo Recorde Pessoal (Priority: P2)

Como jogador que bateu seu recorde de distância, desejo ver um banner comemorativo ("NOVO RECORDE!"), confetes coloridos e som comemorativo na tela de resumo para celebrar minha conquista.

**Why this priority**: Incentiva a motivação intrínseca, criando momentos memoráveis de celebração especialmente para crianças e família.

**Independent Test**: Testável exibindo a tela de resumo com a flag `EhNovoRecorde = true` e verificando a ativação dos elementos de festa visual.

**Acceptance Scenarios**:
1. **Given** que a flag `EhNovoRecorde` é verdadeira, **When** o resumo é apresentado, **Then** o selo de "NOVO RECORDE" fica em destaque animado com efeito visual de confetes.
2. **Given** um voo comum sem recorde, **When** o resumo é apresentado, **Then** a tela exibe o resumo normal sem o selo de novo recorde.

---

### User Story 3 - Navegação e Decisão Pós-Voo (Priority: P3)

Como jogador na tela de resultados, desejo escolher entre clicar em "Ir para Oficina" (para gastar minhas novas moedas) ou "Voar Novamente" (para um novo lançamento rápido).

**Why this priority**: Garante fluidez e controle ao jogador sobre o próximo passo no jogo.

**Independent Test**: Testável simulando o clique em ambos os botões e conferindo o roteamento correto de telas.

**Acceptance Scenarios**:
1. **Given** o resumo de voo aberto, **When** o jogador clica em "Ir para Oficina", **Then** a tela de resumo fecha e o menu da oficina é carregado com o saldo atualizado.
2. **Given** o resumo de voo aberto, **When** o jogador clica em "Voar Novamente", **Then** uma nova sessão de voo é iniciada diretamente na rampa de lançamento.

---

### Edge Cases

- Fechamento acidental do jogo na tela de resultados: as moedas e recordes já devem ter sido salvos previamente no caso de uso, sem perda de progresso.
- Clique durante a animação de contagem de moedas: o sistema deve permitir pular a animação (*skip to end*) para exibir o resultado final instantaneamente.

---

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: O sistema DEVE fornecer o componente `ControladorUIResumoVoo` para gerenciar a exibição dos resultados da sessão.
- **FR-002**: A tela DEVE exibir: distância final percorrida (metros), altitude máxima atingida (metros), moedas obtidas por distância/altitude e moedas coletadas em voo.
- **FR-003**: A tela DEVE conter animação de contagem numérica progressiva (*tweening*) para a quantidade de moedas ganhas.
- **FR-004**: Quando `EhNovoRecorde` for verdadeiro, a tela DEVE ativar o componente visual de comemoração de recorde.
- **FR-005**: A interface DEVE disponibilizar dois botões de navegação: "Oficina" e "Voar Novamente".
- **FR-006**: O sistema DEVE permitir toque na tela durante a contagem para concluir a animação imediatamente.

### Key Entities

- **`ModeloVisualResumoVoo`**: Objeto contendo os dados formatados para a tela de encerramento.

---

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Abertura suave da tela de resumo em menos de 100 milissegundos após a parada do avião.
- **SC-002**: Duração da animação de contagem configurada entre 1.0 e 2.0 segundos, com possibilidade de pulo instantâneo.
- **SC-003**: 100% de precisão na sincronização visual entre as moedas animadas e o saldo salvo.

---

## Assumptions

- O design visual utiliza elementos do pacote Kenney UI Pack.
- A transição entre o resumo e a oficina/lançamento ocorre sem recarregamento pesado de cenas.
