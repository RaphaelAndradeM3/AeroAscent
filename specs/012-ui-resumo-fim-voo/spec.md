# Feature Specification: Interface de Resumo de Voo e Celebração de Recorde

**Feature Branch**: `012-ui-resumo-fim-voo`  
**Created**: 2026-09-04  
**Status**: Ready for Planning  
**Input**: User description: "012 - Tela de Resumo de Fim de Voo (Animação de recompensas, celebração de recorde e redirecionamento)."

---

## Clarifications

### Session 2026-09-06

- Q: Qual padrão arquitetural deve estruturar a tela de resumo de voo para assegurar separação estrita de camadas e testabilidade automatizada sem acoplamento à Unity Engine? → A: Padrão MVP (Model-View-Presenter) com `ApresentadorResumoVoo` em C# puro (.NET Standard 2.1) desacoplado de `UnityEngine` e `IVisaoResumoVoo` como visão passiva implementada no Unity por `ControladorUIResumoVoo`.
- Q: Em que momento a liquidação das recompensas e a persistência em disco do progresso do jogador devem ser executadas em relação à abertura da tela de resumo? → A: Persistência imediata pré-resumo via `IFinalizarVooCasoDeUso`, assegurando que o progresso (moedas e recordes) já esteja gravado em disco antes do início da exibição e animação.
- Q: Como a animação de contagem progressiva de moedas deve se comportar e qual é o efeito imediato do toque do jogador na tela? → A: Contagem progressiva com duração padrão de 1,5 segundos; qualquer toque na tela conclui instantaneamente a animação (*skip to end*), fixando os totais finais e liberando a interação dos botões de ação.
- Q: Como o apresentador da tela de resumo deve comunicar a escolha de navegação do jogador ("Ir para Oficina" ou "Voar Novamente") para o restante do jogo? → A: Disparo de eventos C# desacoplados no apresentador (`AoSolicitarIrParaOficina` e `AoSolicitarVoarNovamente`) acionados pelos botões da visão passiva e ouvidos pelo coordenador de fluxo da Unity.
- Q: Como o modelo visual da tela e os efeitos comemorativos de novo recorde pessoal devem ser estruturados e acionados na interface gráfica? → A: `ModeloVisualResumoVoo` imutável com dados pré-formatados em pt-BR (distância, altitude, decomposição detalhada de moedas, saldo e flag `EhNovoRecorde`); a visão passiva aciona partículas de confete e destaque comemorativo estritamente quando `EhNovoRecorde == true`.

---

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Animação de Recompensas e Contagem de Moedas (Priority: P1)

Como jogador ao final do pouso, desejo que a tela de resumo de voo se abra suavemente, exibindo a distância final alcançada, a altitude máxima e uma animação empolgante de contagem crescente de moedas ganhas, para celebrar o resultado do voo.

**Why this priority**: É o momento de maior satisfação emocional e recompensa do core loop.

**Independent Test**: Testável abrindo o modal de resumo com os dados da finalização de voo e validando a sequência de animação dos números e o saldo final atualizado.

**Acceptance Scenarios**:
1. **Given** um voo recém-finalizado com 34 moedas ganhas, **When** a tela de resumo é aberta, **Then** o contador de moedas ganhas sobe animadamente de 0 até 34 em 1,5 segundos acompanhado de som de contagem, e o saldo do jogador é atualizado.
2. **Given** a contagem de moedas em andamento na tela, **When** o jogador toca em qualquer ponto da tela, **Then** a animação é concluída instantaneamente exibindo o valor total de 34 moedas e liberando a interação dos botões.

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

- Fechamento acidental do jogo na tela de resultados: as moedas e recordes já estão liquidados e persistidos em disco antes da renderização da tela via `IFinalizarVooCasoDeUso`, garantindo zero perda de progresso.
- Clique durante a animação de contagem de moedas: o sistema deve permitir pular a animação (*skip to end*) para exibir o resultado final instantaneamente.

---

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: O sistema DEVE estruturar a tela no padrão MVP, com `ApresentadorResumoVoo` em C# puro (.NET Standard 2.1) orquestrando a lógica de apresentação e `IVisaoResumoVoo` como contrato de visão passiva implementada no Unity por `ControladorUIResumoVoo`.
- **FR-002**: A tela DEVE exibir: distância final percorrida (metros), altitude máxima atingida (metros), moedas obtidas por distância/altitude e moedas coletadas em voo.
- **FR-003**: A tela DEVE conter animação de contagem numérica progressiva (*tweening*) para a quantidade de moedas ganhas.
- **FR-004**: Quando `EhNovoRecorde` for verdadeiro no `ModeloVisualResumoVoo`, a visão passiva DEVE ativar o selo comemorativo "NOVO RECORDE!" acompanhado de partículas de confete e fanfarra sonora.
- **FR-005**: A interface DEVE disponibilizar dois botões de navegação: "Oficina" e "Voar Novamente", que acionam no apresentador os eventos C# puros `AoSolicitarIrParaOficina` e `AoSolicitarVoarNovamente`, delegando ao orquestrador de fluxo da Unity a transição suave de cena ou reinício imediato.
- **FR-006**: A contagem animada de moedas DEVE ter duração de 1,5 segundos e o sistema DEVE permitir toque em qualquer ponto da tela durante a animação para concluir a contagem instantaneamente (*skip to end*), liberando a interação dos botões de ação.
- **FR-007**: A liquidação e persistência das recompensas e recordes DEVE ser concluída via `IFinalizarVooCasoDeUso` antes da transição e abertura da tela de resumo, assegurando total consistência e segurança dos dados gravados.

### Key Entities

- **`ModeloVisualResumoVoo`**: Registro imutável de apresentação contendo distância percorrida formatada (ex: `"125,4 m"`), altitude máxima formatada (ex: `"45,2 m"`), decomposição das moedas ganhas (por distância, altitude e coletadas no ar), total da rodada formatado, saldo final acumulado do jogador formatado (ex: `"💰 1.250"`) e a flag booleana `EhNovoRecorde`.

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
