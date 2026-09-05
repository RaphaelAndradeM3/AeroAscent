# Feature Specification: Interface do Menu Principal, Hangar 3D e Oficina

**Feature Branch**: `010-ui-menu-principal-oficina`  
**Created**: 2026-09-04  
**Status**: Ready for Planning  
**Input**: User description: "010 - Telas de Apresentação da Oficina e Hangar: Menu principal, 4 cartões de evolução, saldo e botão decolagem."

---

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Visualização e Navegação na Oficina / Hangar (Priority: P1)

Como jogador ao abrir o jogo, desejo ver o hangar 3D com meu avião, meu saldo atual de moedas no topo da tela, os 4 cartões de melhoria (Motor, Aerodinâmica, Tanque, Catapulta) e um botão chamativo "DECOLAR" para iniciar uma partida.

**Why this priority**: É a porta de entrada visual do jogo onde o jogador toma decisões de evolução antes de cada voo.

**Independent Test**: Testável montando a interface da Oficina e verificando a vinculação correta do saldo de moedas, níveis exibidos em cada cartão e estado dos botões de compra.

**Acceptance Scenarios**:
1. **Given** um saldo de 300 moedas, **When** a tela da oficina é aberta, **Then** o cabeçalho exibe exatamente 300 moedas e os cartões que custam $\le 300$ exibem o botão de compra habilitado em verde.
2. **Given** um cartão de melhoria cujo custo é de 500 moedas (saldo atual 300), **When** a tela é exibida, **Then** o botão de compra para esse cartão específico é exibido desabilitado/apagado, impedindo toques inválidos.

---

### User Story 2 - Compra Reativa de Melhoria na Interface (Priority: P2)

Como jogadora (ex: Ruth, Sofia ou Alice), ao clicar no botão de compra de um upgrade, desejo ver feedback visual e sonoro imediato, o nível subindo na barra de progresso e o saldo de moedas decrescendo instantaneamente.

**Why this priority**: Proporciona a satisfação imediata de progressão e clareza visual para todas as faixas etárias.

**Independent Test**: Testável disparando o evento de clique no botão de compra e validando a atualização da UI via ViewModel/Presenter sem recarregar a cena.

**Acceptance Scenarios**:
1. **Given** saldo de 150 moedas e Motor no nível 1 (custo 100), **When** a jogadora clica em "Melhorar Motor", **Then** o saldo na tela atualiza imediatamente para 50, a barra do motor avança para o nível 2 e os botões recalculam seus estados habilitados/desabilitados.

---

### User Story 3 - Transição para a Decolagem (Priority: P3)

Como jogador, ao pressionar o botão "DECOLAR", desejo que o jogo transite suavemente da câmera da oficina para a rampa de lançamento.

**Why this priority**: Conecta o menu ao loop ativo de gameplay.

**Independent Test**: Testável simulando o clique em "DECOLAR" e conferindo a abertura da tela de lançamento/voo e desativação do menu principal.

**Acceptance Scenarios**:
1. **Given** o jogador no menu principal, **When** ele toca no botão "DECOLAR", **Then** a interface da oficina fecha e o fluxo de lançamento da aeronave é iniciado.

---

### Edge Cases

- Toques múltiplos ultra-rápidos (*spam click*) no botão de compra: a UI deve debouncear e processar cada compra sequencialmente sem duplicar débitos.
- Troca de idioma ou valores numéricos grandes: a interface deve suportar formatação limpa sem quebrar layout.
- Falha na leitura do progresso: a UI deve exibir mensagem amigável sem travar em tela preta.

---

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: O sistema DEVE fornecer o controlador de apresentação `ControladorUIOficina` (ou `MenuPrincipalPresenter`) desacoplado de lógica de negócio direta, interagindo exclusivamente através de Casos de Uso.
- **FR-002**: A interface DEVE exibir o saldo total de moedas no canto superior com formatação clara (ex: `💰 1.250`).
- **FR-003**: A interface DEVE renderizar 4 cartões de melhoria contendo: ícone temático, nome da melhoria, nível numérico atual, barra visual de progresso e botão de ação com o custo da próxima evolução.
- **FR-004**: O botão de compra DEVE ser dinamicamente ativado/desativado baseado na capacidade financeira do jogador (`Saldo >= Custo`).
- **FR-005**: O botão principal "DECOLAR" DEVE disparar a transição para a cena/fase de voo.

### Key Entities

- **`ModeloVisualOficina`**: Estrutura reativa de dados para bind de interface contendo saldo, lista de 4 cartões com status de compra e recorde do jogador.

---

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Tempo de inicialização e renderização completa da tela de menu inferior a 200 milissegundos.
- **SC-002**: Taxa de quadros estável em 60 FPS durante todas as transições e interações de menu.
- **SC-003**: Feedback visual e auditivo de compra em menos de 1 frame após o toque.

---

## Assumptions

- Os assets visuais de interface seguem o pacote UI Pack da Kenney.nl (CC0).
- A interface utiliza o sistema moderno de UI da Unity (Unity UI / UI Toolkit) com âncoras responsivas para múltiplos aspectos de tela (16:9, 18:9, 20:9).
