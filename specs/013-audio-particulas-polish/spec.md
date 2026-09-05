# Feature Specification: Áudio, Sistema de Partículas Kenney e Polimento Geral

**Feature Branch**: `013-audio-particulas-polish`  
**Created**: 2026-09-04  
**Status**: Ready for Planning  
**Input**: User description: "013 - Sistema de Feedback Audiovisual, Efeitos Sonoros CC0 (vento, propulsão, moedas), Partículas e Polimento Geral."

---

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Efeitos Sonoros Imersivos e Suaves (Priority: P1)

Como jogador, desejo ouvir efeitos sonoros estéreo acolhedores e agradáveis ao lançar o avião, planar com o vento, acionar a turbina (*boost*), recolher moedas e comemorar recordes, tornando o jogo vivo e encantador para todas as idades.

**Why this priority**: O som confere textura, satisfação e identidade à experiência do jogo sem ser agressivo.

**Independent Test**: Testável disparando eventos sonoros através da interface `IServicoAudio` e verificando que cada evento invoca o clipe de áudio correspondente com volume balanceado e sem corte abrupto.

**Acceptance Scenarios**:
1. **Given** a aeronave em alta velocidade, **When** ela plana pelo ar, **Then** o áudio reproduz um efeito suave de vento cuja intensidade varia dinamicamente com a velocidade.
2. **Given** o acionamento do boost, **When** o propulsor é ligado, **Then** o som contínuo de propulsão é executado em loop suave e encerra sem ruídos de corte ao soltar o botão ou esgotar combustível.
3. **Given** a coleta de uma moeda, **When** o avião colide com ela, **Then** um som curto e agradável de sino/brilho (*pling*) é tocado.

---

### User Story 2 - Emissores de Partículas e Feedback Visual (Priority: P2)

Como jogadoras (Ruth, Sofia e Alice), desejamos ver fumaça colorida saindo da cauda do avião, faíscas vibrantes ao acionar o propulsor e explosão de confetes ao bater um novo recorde.

**Why this priority**: Estimula a imaginação infantil, diversão e satisfação estética imediata.

**Independent Test**: Testável ativando os emissores de partículas em resposta a eventos de voo e validando que o pool de partículas não gera alocações constantes de memória.

**Acceptance Scenarios**:
1. **Given** o avião voando, **When** o propulsor é ativado, **Then** o emissor de partículas de propulsão emite rastro de fogo/fumaça vibrante alinhado à traseira do avião.
2. **Given** a chegada na tela de novo recorde, **When** a celebração é acionada, **Then** partículas de confetes coloridos são emitidas em toda a tela.

---

### User Story 3 - Otimização Geral de Desempenho e Bateria Mobile (Priority: P3)

Como jogador em um smartphone ou tablet, desejo que o jogo execute a 60 FPS estáveis, com tempo de carregamento instantâneo e consumo de bateria eficiente, sem esquentar o dispositivo.

**Why this priority**: Garante longevidade, fluidez e estabilidade em múltiplos aparelhos.

**Independent Test**: Testável executando perfilamento de desempenho com benchmark de 60 FPS e verificação de alocações zero no loop contínuo de renderização e áudio.

**Acceptance Scenarios**:
1. **Given** o jogo em execução contínua por 10 minutos, **When** o frame rate é medido, **Then** a taxa de quadros se mantém a 60 FPS sem engasgos ou quedas perceptíveis de desempenho.

---

### Edge Cases

- Dispositivo com áudio no mudo: o sistema de som deve respeitar a configuração do sistema operacional e não gerar exceções.
- Múltiplas moedas coletadas em rápida sucessão: o áudio deve permitir sobreposição suave com leve variação de pitch (*pitch modulation*) sem estourar o volume máximo.

---

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: O sistema DEVE fornecer o serviço `IServicoAudio` desacoplado, com implementação multiplataforma para Unity (compatível com Windows e Android) para gerenciar canais de efeitos (SFX) e música ambiente.
- **FR-002**: Todos os efeitos sonoros DEVEM utilizar fontes sob licença CC0 / Domínio Público (Kenney.nl Audio Packs).
- **FR-003**: O sistema DEVE fornecer emissores de partículas para: rastro de cauda do avião (*trail*), chamas de boost, brilho de coleta de moedas e confetes comemorativos de recorde.
- **FR-004**: O áudio do propulsor DEVE variar dinamicamente de volume de acordo com a aceleração e o estado do motor.
- **FR-005**: Todo o gerenciamento de partículas e áudio DEVE reutilizar instâncias via *Object Pooling* e manter taxa de quadros a 60 FPS.

### Key Entities

- **`EventoAudio`**: Enumeração com os sons do jogo (`LancamentoCatapulta`, `VooVento`, `PropulsorBoost`, `ColetaMoeda`, `PassagemAnelVento`, `PousoSuave`, `NovoRecorde`, `CliqueBotao`).
- **`ConfiguracaoAudio`**: Objeto com volumes independentes para SFX e Música.

---

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Manutenção de 60 FPS estáveis com variação inferior a 2% em dispositivos móveis medianos.
- **SC-002**: Alocação zero de lixo de memória (`GC Alloc = 0 bytes`) nos sistemas de áudio e partículas durante o voo.
- **SC-003**: Inclusão de 100% dos assets de áudio e partículas respeitando a licença CC0 e a ética familiar do projeto.

---

## Assumptions

- Todos os clipes sonoros são pré-carregados na memória durante a inicialização para evitar engasgos em tempo de execução.
- As partículas visuais utilizam o sistema nativo de partículas Shuriken da Unity com *Object Pooling*, com alto desempenho acelerado por GPU em Windows e Android.
