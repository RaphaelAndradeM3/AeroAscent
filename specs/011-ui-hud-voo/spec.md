# Feature Specification: Interface HUD de Voo e Controles de Toque Mobile

**Feature Branch**: `011-ui-hud-voo`  
**Created**: 2026-09-04  
**Status**: Ready for Planning  
**Input**: User description: "011 - Interface HUD durante o Voo (Distância, altímetro, velocímetro, combustível e controles táteis de subida/descida e boost)."

---

## Clarifications

### Session 2026-09-05

- Q: Qual padrão arquitetural deve estruturar a interface do HUD de voo para garantir testabilidade unitária automatizada e desacoplamento do motor Unity? → A: Padrão MVP (Model-View-Presenter) com `ApresentadorHUDVoo` em C# puro (.NET Standard 2.1) desacoplado de `UnityEngine` e `IVisaoHUDVoo` como visão passiva implementada no Unity pelo `ControladorHUDVoo`.
- Q: Qual estratégia deve ser adotada para transferir os dados de voo ao HUD e atualizar os mostradores numéricos garantindo GC Alloc = 0 bytes no loop contínuo? → A: Modelagem via `readonly record struct TelemetriaHUDDTO` (alocação na stack) e visão com cache de valor inteiro ou buffers pré-alocados de caracteres (`char[]` / `StringBuilder`).
- Q: Qual deve ser o mecanismo de captura e despacho de comandos de controle (subida, descida e boost) entre os controles táteis/teclado e o apresentador? → A: Métodos explícitos de transição de estado no `ApresentadorHUDVoo` (`IniciarSubida`, `PararSubida`, `IniciarDescida`, `PararDescida`, `IniciarBoost`, `PararBoost`), acionados por eventos de ponteiro (`IPointerDownHandler`/`IPointerUpHandler`) na visão passiva e mapeados simultaneamente para as teclas de direção/espaço no PC.
- Q: Como o HUD deve sinalizar visualmente o esgotamento total de combustível e a quebra de recorde pessoal durante a pilotagem? → A: Botão de Boost esmaecido (50% de opacidade e desabilitado) ao zerar o combustível, e indicador de recorde com destaque cromático dourado e suave pulso de escala no momento da ultrapassagem.
- Q: Como o HUD de voo deve gerenciar o botão de pausa e a transição dos controles quando a aeronave encerra o voo (pouso ou colisão)? → A: Botão de Pausa no canto superior direito acionando `SolicitarPausa()` e liberando inputs ativos, e ocultação imediata dos botões táteis ao detectar término de voo (`StatusVoo.Pousado` / `Colidido`), mantendo a telemetria final visível até a transição para a tela de resultados (Feature 012).

---

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Exibição de Telemetria e Indicadores de Voo em Tempo Real (Priority: P1)

Como jogador durante a pilotagem da aeronave, desejo visualizar no HUD superior e lateral a distância percorrida em metros, o recorde pessoal a ser batido, a altitude atual, a velocidade e o medidor vertical de combustível para tomar decisões táticas de voo.

**Why this priority**: É a interface crítica de feedback durante a partida, essencial para a experiência do usuário.

**Independent Test**: Testável atualizando os dados do voo e validando que o HUD reflete os valores sem gerar alocação de lixo de memória na conversão de strings numéricas.

**Acceptance Scenarios**:
1. **Given** a aeronave voando a 125 metros de distância e altitude de 45 metros, **When** o HUD atualiza o frame, **Then** os rótulos de distância (`125 m`), altitude (`45 m`) e a barra de combustível refletem os valores exatos instantaneamente.
2. **Given** que o recorde anterior é de 200m, **When** a distância ultrapassa 200m em voo, **Then** o indicador de recorde altera sua cor para tom dourado brilhante e executa uma suave animação de pulso de escala.

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
- Esgotamento de combustível durante o toque: o botão de boost passa imediatamente ao estado esmaecido (opacidade 50% e desabilitado) e o comando de propulsão é cancelado mesmo que o jogador continue pressionando a tela.
- Pausa ou perda de foco do app: o HUD deve congelar o estado e evitar travamento dos comandos de toque.

---

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: O sistema DEVE estruturar o HUD no padrão MVP, com `ApresentadorHUDVoo` em C# puro (.NET Standard 2.1) sincronizado com o fluxo de voo e `IVisaoHUDVoo` como interface passiva implementada no Unity por `ControladorHUDVoo`.
- **FR-002**: O HUD DEVE exibir no topo central a distância percorrida em metros formatada (`XXX m`) e a marcação do recorde atual.
- **FR-003**: O HUD DEVE exibir altímetro e velocímetro discretos no canto superior esquerdo.
- **FR-004**: O HUD DEVE exibir um medidor vertical de combustível no canto direito, com preenchimento reativo de 0% a 100%.
- **FR-005**: A área de controle de toque DEVE conter botões ergonômicos de inclinação (subir/descer) na metade esquerda da tela e botão de Boost na metade direita, despachando transições de estado explícitas (`IniciarSubida()`, `PararSubida()`, `IniciarDescida()`, `PararDescida()`, `IniciarBoost()`, `PararBoost()`) acionadas por eventos de ponteiro táteis e mapeadas simultaneamente para teclas no PC (Setas/W-S e Espaço).
- **FR-006**: A transferência de telemetria DEVE utilizar o DTO imutável na stack `TelemetriaHUDDTO`, e a visão passiva DEVE utilizar comparação de valor inteiro anterior ou buffers pré-alocados de caracteres (`char[]` / `StringBuilder`) para assegurar zero alocação no heap (`GC Alloc = 0 bytes`) em 100% dos frames.
- **FR-007**: O HUD DEVE desabilitar e esmaecer o botão de Boost (50% de opacidade) quando o combustível for esgotado, e DEVE disparar animação de destaque cromático dourado com pulso de escala no indicador de recorde quando a marca histórica for superada.
- **FR-008**: O HUD DEVE fornecer um botão de Pausa no canto superior direito que invoca `SolicitarPausa()` no apresentador liberando comandos mantidos, e DEVE ocultar imediatamente os botões táteis de controle ao detectar a conclusão do voo (`StatusVoo.Pousado` ou `StatusVoo.Colidido`), mantendo os dados finais de telemetria visíveis até a transição para a tela de resultados (Feature 012).

### Key Entities

- **`TelemetriaHUDDTO`**: `readonly record struct` imutável alocado na stack contendo distância (float), recorde (float), altitude (float), velocidade (float), percentual de combustível (float de 0 a 1) e moedas coletadas na partida (int).

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
