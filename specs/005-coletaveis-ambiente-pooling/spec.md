# Feature Specification: Sistema de Coletáveis em Voo e Object Pooling

**Feature Branch**: `005-coletaveis-ambiente-pooling`  
**Created**: 2026-09-04  
**Status**: Ready for Planning  
**Input**: User description: "005 - Sistema de Coletáveis em Voo (Moedas flutuantes, anéis de impulso de ar) e Object Pooling de alta performance (zero GC)."

---

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Coleta de Moedas Flutuantes Durante o Voo (Priority: P1)

Como jogador pilotando a aeronave, desejo desviar minha rota para colidir com moedas suspensas no ar a fim de acumular recursos financeiros adicionais na sessão de voo.

**Why this priority**: Cria objetivos dinâmicos no ar e premia o controle refinado de altitude e direção.

**Independent Test**: Testável instanciando uma entidade de moeda, movendo a aeronave para a mesma posição e validando o incremento no contador de moedas da sessão e a desativação da moeda coletada.

**Acceptance Scenarios**:
1. **Given** uma moeda na coordenada (X=100, Y=25), **When** a aeronave cruza o raio de colisão da moeda, **Then** o saldo temporário do voo é incrementado em 1 moeda e o objeto da moeda é reciclado.
2. **Given** uma moeda já coletada, **When** a aeronave passa pelo mesmo ponto posteriormente, **Then** a moeda não deve ser recolhida novamente.

---

### User Story 2 - Atravessar Anéis de Impulso de Vento (Priority: P2)

Como jogador, desejo passar por dentro de anéis de vento (*air boost rings*) para receber um impulso instantâneo de velocidade para frente sem gastar combustível.

**Why this priority**: Acrescenta ritmo e emoção ao voo, permitindo encadear impulsos e voar mais longe.

**Independent Test**: Testável simulando a passagem por um anel de vento e verificando o acréscimo instantâneo no vetor de velocidade da aeronave.

**Acceptance Scenarios**:
1. **Given** a aeronave voando a 15 m/s, **When** ela atravessa um anel de vento, **Then** sua velocidade horizontal é impulsionada para um valor superior (ex: +10 m/s instantâneo) e nenhum combustível é consumido.

---

### User Story 3 - Reutilização de Objetos via Pooling com Zero GC (Priority: P3)

Como sistema de performance, os elementos do cenário (moedas, anéis de vento, nuvens) devem ser gerenciados por um pool de objetos (`ObjectPool<T>`), evitando instanciações e destruições dinâmicas de memória.

**Why this priority**: Garante que o jogo mantenha 60 FPS estáveis no mobile sem pausas de coleta de lixo (*Garbage Collection stutter*).

**Independent Test**: Testável executando spawn e recycle contínuos de 1000 coletáveis e validando que o GC Alloc permanece em 0 bytes após a inicialização.

**Acceptance Scenarios**:
1. **Given** um pool de moedas inicializado, **When** uma moeda sai do campo de visão ou é coletada, **Then** ela é desativada e devolvida ao pool sem invocar `Destroy` ou `new`.

---

### Edge Cases

- Múltiplas moedas coletadas no mesmo frame não devem causar race condition na contagem.
- Coletáveis fora do campo de visão da câmera (atrás da aeronave) devem ser reciclados automaticamente para liberar o pool.
- Pool vazio durante pico de demanda deve expandir de forma segura sem crash.

---

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: O sistema DEVE fornecer o modelo de dados de coletáveis (`ColetavelMoeda`, `ColetavelAnelVento`) com coordenadas e status ativo.
- **FR-002**: O sistema DEVE gerar coletáveis proceduralmente à frente da trajetória da aeronave respeitando faixas de altitude navegáveis.
- **FR-003**: Ao colidir com uma moeda, o sistema DEVE adicionar o valor ao total de moedas coletadas na entidade `Voo`.
- **FR-004**: Ao colidir com um anel de vento, o sistema DEVE aplicar impulso instantâneo de velocidade vetorial.
- **FR-005**: O sistema DEVE fornecer a estrutura genérica `GerenciadorPoolObjetos<T>` para reuso de instâncias sem alocação no heap.

### Key Entities

- **`Coletavel`**: Objeto que define posição no espaço, tipo (`Moeda`, `AnelVento`), raio de coleta e estado de ativação.
- **`PoolObjetos`**: Estrutura gerenciadora de alocação prévia e reciclagem.

---

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Zero alocação de memória no heap (`0 bytes GC Alloc`) durante todo o ciclo de spawn, coleta e reciclagem.
- **SC-002**: Processamento de detecção de proximidade de todos os coletáveis em tela em menos de 0.1ms por frame.
- **SC-003**: Reciclagem de 100% dos coletáveis que ultrapassam o limite traseiro da câmera.

---

## Assumptions

- O cenário é percorrido primordialmente da esquerda para a direita (sentido positivo do eixo X).
- As colisões são verificadas por raio de distância esférico/circular simples para máxima performance.
