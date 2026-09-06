# Feature Specification: Interface do Menu Principal, Hangar 3D e Oficina

**Feature Branch**: `010-ui-menu-principal-oficina`  
**Created**: 2026-09-04  
**Status**: Ready for Planning  
**Input**: User description: "010 - Telas de Apresentação da Oficina e Hangar: Menu principal, 4 cartões de evolução, saldo e botão decolagem."

---

## Clarifications

### Session 2026-09-05

- Q: Qual padrão de apresentação deve ser adotado para a interface da Oficina e Menu Principal para assegurar testabilidade automatizada e desacoplamento do Unity? → A: Model-View-Presenter (MVP) com Presenter puro em C# (.NET Standard 2.1) e Visão passiva implementando interface C# (`IVisaoOficina`), permitindo 100% de cobertura de testes de unidade em xUnit sem acoplamento com a engine Unity.
- Q: Como o cartão de melhoria e o botão de compra devem se comportar visualmente quando um componente atinge o nível máximo (10/10)? → A: O botão de compra permanece no layout com estado desabilitado exibindo o texto "MÁXIMO" (sem valor de moedas), o nível exibe "Nível 10 (MAX)" e a barra de progresso atinge 100% preenchida com estilo comemorativo de destaque.
- Q: Como o apresentador e a visão devem gerenciar cliques rápidos consecutivos (spam click) nos botões de compra para evitar reentrância e compras duplicadas indesejadas? → A: Bloqueio de reentrância por flag no Presenter acompanhado de desabilitação temporária da interação na visão (`IVisaoOficina.DefinirInteracaoHabilitada(false)`) durante o `await` da transação assíncrona, restaurando o estado calculado imediatamente após o salvamento.
- Q: Qual formato de exibição numérica deve ser adotado para o saldo de moedas e os custos dos upgrades na interface? → A: Padrão pt-BR integral com separador de milhar por ponto (formato `N0` com `CultureInfo("pt-BR")`, ex: `1.250`, `15.000`), garantindo acessibilidade familiar e conformidade com o idioma oficial sem abreviações em inglês (k/M).
- Q: Como o apresentador (`ApresentadorOficina`) deve sinalizar e disparar o fluxo de transição para o voo ao ser acionado o botão "DECOLAR"? → A: Evento C# desacoplado no Presenter (`event Action? AoSolicitarDecolagem`), permitindo que o orquestrador de fluxo de jogo da Unity assine a solicitação, execute a interpolação suave da câmera do hangar e inicie o lançamento.

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

- **Cliques ultra-rápidos e concorrentes (*spam click*) no botão de compra**: O apresentador bloqueia reentrância via flag interna e desativa os botões na visão até a conclusão do `SalvarProgressoAsync`, garantindo que nenhuma transação duplicada seja enviada ou processada.
- Troca de idioma ou valores numéricos grandes: a interface deve suportar formatação limpa sem quebrar layout.
- Falha na leitura do progresso: a UI deve exibir mensagem amigável sem travar em tela preta.

---

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: O sistema DEVE fornecer a arquitetura Model-View-Presenter (MVP) com o apresentador `ApresentadorOficina` puro em C# (.NET Standard 2.1) desacoplado de `UnityEngine`, interagindo exclusivamente através de casos de uso (`IConsultarOficinaCasoDeUso`, `IComprarMelhoriaCasoDeUso`) e notificando a visão passiva através da interface `IVisaoOficina`.
- **FR-002**: A interface DEVE exibir o saldo total de moedas no canto superior com formatação explícita em pt-BR utilizando separador de milhar por ponto (ex: `💰 1.250`, `💰 15.000`), sem sufixos em língua inglesa.
- **FR-003**: A interface DEVE renderizar 4 cartões de melhoria contendo: ícone temático, nome da melhoria, nível numérico atual, barra visual de progresso (0% a 100%) e botão de ação com o custo da próxima evolução; quando no nível máximo (10), deve exibir "Nível 10 (MAX)" com barra a 100%.
- **FR-004**: O botão de compra DEVE ser dinamicamente ativado/desativado baseado na capacidade financeira do jogador (`Saldo >= Custo`); se o componente já estiver no nível máximo, o botão DEVE permanecer visível porém desabilitado exibindo o texto "MÁXIMO" sem custo numérico.
- **FR-005**: O botão principal "DECOLAR" DEVE disparar o evento C# desacoplado `AoSolicitarDecolagem` no `ApresentadorOficina`, permitindo ao orquestrador da Unity executar a transição de câmera e carregar a rampa de lançamento.

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
- A interface utiliza o sistema moderno de UI da Unity (Unity UI / Canvas) com âncoras responsivas adaptáveis para múltiplos aspectos de tela no Android (16:9, 18:9, 20:9) e janelas redimensionáveis no Windows.
