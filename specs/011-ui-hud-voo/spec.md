# Feature Specification: Interface HUD de Voo e Controles de Toque Mobile

**Feature Branch**: `011-ui-hud-voo`  
**Created**: 2026-09-04  
**Status**: Ready for Planning  
**Input**: User description: "011 - Interface HUD durante o Voo (Distância, altímetro, velocímetro, combustível e controles táteis de subida/descida e boost)."

---

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Exibição de Telemetria e Indicadores de Voo em Tempo Real (Priority: P1)

Como jogador durante a pilotagem da aeronave, desejo visualizar no HUD superior e lateral a distância percorrida em metros, o recorde pessoal a ser batido, a altitude atual, a velocidade e o medidor vertical de combustível para tomar decisões táticas de voo.

**Why this priority**: É a interface crítica de feedback durante a partida, essencial para a experiência do usuário.

**Independent Test**: Testável atualizando os dados do voo e validando que o HUD reflete os valores sem gerar alocação de lixo de memória na conversão de strings numéricas.

**Acceptance Scenarios**:
1. **Given** a aeronave voando a 125 metros de distância e altitude de 45 metros, **When** o HUD atualiza o frame, **Then** os rótulos de distância (`125 m`), altitude (`45 m`) e a barra de combustível refletem os valores exatos instantaneamente.
2. **Given** que o recorde anterior é de 200m, **When** a distância ultrapassa 200m em voo, **Then** o indicador de recorde altera sutilmente sua cor ou exibe animação de superação de marca.

---

### User Story 2 - Controles Táteis Mobile Responsivos e Ergonômicos (Priority: P2)

Como jogadora em um dispositivo móvel (ou tablet), desejo botões táteis confortáveis no lado esquerdo da tela para inclinar o nariz do avião para cima/baixo e um botão grande no lado direito para acionar o Boost.

**Why this priority**: Garante ergonomia confortável com as duas mãos para crianças e adultos jogando em modo paisagem (*landscape*).

**Independent Test**: Testável acionando eventos de toque nos botões de controle e conferindo os eventos de comando transmitidos ao controlador da aeronave.

**Acceptance Scenarios**:
1. **Given** o jogador segurando o botão de subir no canto esquerdo, **When** o toque é mantido, **Then** o comando de inclinação positiva de pitch é enviado continuamente ao serviço de física.
2. **Given** o jogador pressionando o botão de Boost no canto direito, **When** o botão é mantido pressionado e há combustível, **Then** o sinal de ativação do propulsor é transmitido e a barra visual de combustível desce em tempo real.

---

### Edge Cases

- Toques fora dos botões designados ou multitoque: o sistema deve ignorar toques acidentais e gerenciar múltiplos dedos sem conflito de entrada.
- Esgotamento de combustível durante o toque: o botão de boost deve ficar visualmente inativo mesmo que o jogador continue pressionando a tela.
- Pausa ou perda de foco do app: o HUD deve congelar o estado e evitar travamento dos comandos de toque.

---

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: O sistema DEVE fornecer o componente de visualização `ControladorHUDVoo` sincronizado com os eventos da entidade `Voo`.
- **FR-002**: O HUD DEVE exibir no topo central a distância percorrida em metros formatada (`XXX m`) e a marcação do recorde atual.
- **FR-003**: O HUD DEVE exibir altímetro e velocímetro discretos no canto superior esquerdo.
- **FR-004**: O HUD DEVE exibir um medidor vertical de combustível no canto direito, com preenchimento reativo de 0% a 100%.
- **FR-005**: A área de controle de toque DEVE conter botões ergonômicos de inclinação (subir/descer) na metade esquerda da tela e botão de Boost na metade direita.
- **FR-006**: A atualização dos textos e medidores no HUD DEVE utilizar buffers de formatação eficientes para evitar alocação de lixo na memória (`GC Alloc = 0 bytes`).

### Key Entities

- **`DadosHUDVoo`**: Estrutura contendo distância em metros, altitude, velocidade horizontal, percentual de combustível e moedas coletadas.

---

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Zero alocação de lixo no heap (`GC Alloc = 0 bytes`) em 100% dos frames durante a atualização do HUD.
- **SC-002**: Latência de resposta ao toque inferior a 16 milissegundos.
- **SC-003**: Legibilidade comprovada em telas com proporções de 16:9 até 21:9 em orientação paisagem.

---

## Assumptions

- O jogo roda exclusivamente em orientação paisagem (*landscape*).
- O sistema de controle suporta tanto entrada por toque móvel quanto teclado (setas/espaço) para testes no editor.
