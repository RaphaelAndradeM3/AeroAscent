# Feature Specification: Sistema de Coletáveis em Voo e Object Pooling

**Feature Branch**: `005-coletaveis-ambiente-pooling`  
**Created**: 2026-09-04  
**Status**: Ready for Planning  
**Input**: User description: "005 - Sistema de Coletáveis em Voo (Moedas flutuantes, anéis de impulso de ar) e Object Pooling de alta performance (zero GC)."

## Clarifications

### Session 2026-09-05
- Q: Sistema de Coordenadas e Eixo de Avanço dos Coletáveis → A: Padronizado para o plano longitudinal Y-Z (Z = avanço horizontal para frente, Y = altitude vertical, X = 0), com raio de colisão circular/cilíndrico alinhado 100% às features 001 a 004.
- Q: Dinâmica e Direção do Impulso do Anel de Vento → A: Adiciona magnitude fixa (+10.0 m/s) na direção do vetor unitário da velocidade atual da aeronave, preservando o fluxo natural da trajetória sem alterar o consumo de combustível.
- Q: Calibração dos Raios de Detecção de Coleta → A: Raio padrão calibrado de 1.5 metros para Moedas e 3.5 metros para Anéis de Vento no plano Y-Z.
- Q: Padrão de Geração Procedural e Janela de Reciclagem → A: Spawn dinâmico procedural posicionado entre +30m e +150m à frente da aeronave em altitude navegável (5m a 120m) e reciclagem automática de qualquer coletável quando Z < Z_aeronave - 20m.
- Q: Arquitetura de Capacidade e Política de Esgotamento do GerenciadorPoolObjetos<T> → A: Pré-alocação padrão de 50 moedas e 15 anéis com expansão elástica de segurança em picos extremos para garantir estabilidade contínua sem crash, operando com zero alocação no heap (GC Alloc = 0 bytes) em regime regular de voo.

---

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Coleta de Moedas Flutuantes Durante o Voo (Priority: P1)

Como jogador pilotando a aeronave, desejo desviar minha rota para colidir com moedas suspensas no ar a fim de acumular recursos financeiros adicionais na sessão de voo.

**Why this priority**: Cria objetivos dinâmicos no ar e premia o controle refinado de altitude e direção.

**Independent Test**: Testável instanciando uma entidade de moeda, movendo a aeronave para a mesma posição e validando o incremento no contador de moedas da sessão e a desativação da moeda coletada.

**Acceptance Scenarios**:
1. **Given** uma moeda na coordenada (Z=100m, Y=25m, X=0), **When** a aeronave cruza o raio de colisão da moeda, **Then** o saldo temporário do voo é incrementado em 1 moeda e o objeto da moeda é reciclado.
2. **Given** uma moeda já coletada, **When** a aeronave passa pelo mesmo ponto posteriormente, **Then** a moeda não deve ser recolhida novamente.

---

### User Story 2 - Atravessar Anéis de Impulso de Vento (Priority: P2)

Como jogador, desejo passar por dentro de anéis de vento (*air boost rings*) para receber um impulso instantâneo de velocidade para frente sem gastar combustível.

**Why this priority**: Acrescenta ritmo e emoção ao voo, permitindo encadear impulsos e voar mais longe.

**Independent Test**: Testável simulando a passagem por um anel de vento e verificando o acréscimo instantâneo de +10.0 m/s alinhado ao vetor de velocidade da aeronave.

**Acceptance Scenarios**:
1. **Given** a aeronave voando com vetor de velocidade de magnitude 15.0 m/s, **When** ela atravessa um anel de vento, **Then** sua velocidade é incrementada em +10.0 m/s na direção do seu vetor unitário de velocidade (atingindo 25.0 m/s) e nenhum combustível é consumido.
2. **Given** uma aeronave atravessando o anel com velocidade muito baixa (< 0.5 m/s), **When** o anel é ativado, **Then** o impulso de +10.0 m/s é projetado na direção da arfagem/pitch do nariz.

---

### User Story 3 - Reutilização de Objetos via Pooling com Zero GC (Priority: P3)

Como sistema de performance, os elementos do cenário (moedas e anéis de vento) devem ser gerenciados por uma estrutura de pool de objetos (`GerenciadorPoolObjetos<T>`) pré-alocada (50 moedas e 15 anéis) com política de expansão elástica de segurança para picos, evitando instanciações e destruições dinâmicas de memória durante o loop de voo.

**Why this priority**: Garante que o jogo mantenha 60 FPS estáveis no mobile sem pausas de coleta de lixo (*Garbage Collection stutter*) e cumpra a meta constitucional de GC Alloc = 0 bytes no voo ativo.

**Independent Test**: Testável executando spawn e recycle contínuos de 1000 coletáveis dentro da capacidade pré-alocada e validando que o GC Alloc permanece estritamente em 0 bytes.

**Acceptance Scenarios**:
1. **Given** um pool de moedas inicializado com 50 instâncias, **When** moedas são requisitadas e posteriormente recicladas (após coleta ou ao ficarem para trás da aeronave), **Then** as instâncias são desativadas e devolvidas à pilha do pool sem alocar novos objetos no heap (`GC Alloc = 0 bytes`).
2. **Given** um pico atípico onde as 50 instâncias estejam em uso simultâneo, **When** uma nova moeda for requisitada, **Then** o pool instancia elasticamente um novo item sem travar ou interromper a execução do jogo.

---

### Edge Cases

- Múltiplas moedas coletadas no mesmo frame não devem causar race condition na contagem.
- Coletáveis que ficarem para trás da aeronave (distância longitudinal Z < Z_aeronave - 20m) DEVEM ser reciclados automaticamente para o pool.
- Pool vazio durante pico de demanda deve expandir de forma segura sem crash.

---

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: O sistema DEVE fornecer o modelo de dados de coletáveis (`ColetavelMoeda` com raio de 1.5m, `ColetavelAnelVento` com raio de 3.5m) com coordenadas no plano Y-Z e status ativo/reciclado.
- **FR-002**: O sistema DEVE gerar coletáveis proceduralmente em janela ativa de +30m a +150m à frente do avanço Z da aeronave respeitando faixas de altitude navegáveis entre 5m e 120m.
- **FR-003**: Ao colidir com uma moeda, o sistema DEVE adicionar o valor ao total de moedas coletadas na entidade `Voo`.
- **FR-004**: Ao colidir com um anel de vento, o sistema DEVE aplicar impulso instantâneo vetorial de +10.0 m/s projetado na direção do vetor velocidade da aeronave sem consumir combustível.
- **FR-005**: O sistema DEVE fornecer a estrutura genérica `GerenciadorPoolObjetos<T>` pré-dimensionada (50 moedas, 15 anéis) com capacidade de expansão segura e alocação zero no loop contínuo de simulação.

### Key Entities

- **`Coletavel`**: Objeto/struct com posição tridimensional (plano Y-Z), tipo enumerado (`Moeda`, `AnelVento`), raio de coleta parametrizado (1.5m para moeda, 3.5m para anel) e estado de ativação.
- **`PoolObjetos`**: Estrutura gerenciadora de alocação prévia e reciclagem sem alocação contínua no heap.

---

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Zero alocação de memória no heap (`0 bytes GC Alloc`) durante todo o ciclo de spawn, coleta e reciclagem.
- **SC-002**: Processamento de detecção de proximidade de todos os coletáveis em tela em menos de 0.1ms por frame.
- **SC-003**: Reciclagem de 100% dos coletáveis que ficarem para trás da aeronave (Z < Z_aeronave - 20m).

---

## Assumptions

- O cenário é percorrido primordialmente no sentido de avanço longitudinal positivo do eixo Z (altitude vertical em Y).
- As colisões são verificadas por raio de distância circular no plano Y-Z para máxima performance.
