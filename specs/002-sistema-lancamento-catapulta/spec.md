# Feature Specification: Sistema de Lançamento Inicial e Catapulta

**Feature Branch**: `002-sistema-lancamento-catapulta`  
**Created**: 2026-09-04  
**Status**: Ready for Planning  
**Input**: User description: "002 - Sistema de Lançamento Inicial e Catapulta: Mecânica de Lançamento com precisão de força, ângulo e impulso vetorial inicial baseado no nível da catapulta."

---

## Clarifications

### Session 2026-09-04
- Q: Qual convenção de eixos espaciais o vetor de lançamento inicial (VetorVoo) deve adotar para decompor a velocidade no ângulo de 35°? → A: Eixo Z para frente (avanço horizontal: V0 * cos(35°)), Eixo Y para cima (altitude: V0 * sin(35°)) e Eixo X = 0 (desvio lateral nulo), alinhado ao padrão canônico 3D da Unity Engine.
- Q: Qual deve ser o valor numérico padrão da ForcaBase (velocidade escalar em m/s) da catapulta no nível 1 com 100% de precisão? → A: 25.0 m/s (90 km/h), garantindo arco balístico equilibrado (~50m a 70m no nível 1) e progressão balanceada a cada nível (+25% por nível).
- Q: Como o sistema deve reagir caso o jogador dispare no ponto mínimo da barra oscilante (precisão nula ou próxima de zero)? → A: Aplicar piso mínimo de 10% (0.10f), garantindo que mesmo em falhas graves de timing a aeronave receba impulso suficiente para iniciar o voo e divertir o jogador, sem frustração excessiva (conforme Artigo I da Constituição).
- Q: Como deve ser a divisão de responsabilidades arquiteturais entre o cálculo do impulso vetorial e o fluxo de lançamento? → A: Clean Architecture estrita: 'ServicoFisicaVoo' na camada de Domínio (implementando o contrato IServicoFisicaVoo para o cálculo trigonométrico puro 3D) e 'LancarAeronaveCasoDeUso' na camada de Aplicação ('Core/Aplicacao'), orquestrando a obtenção do nível da catapulta, chamada ao serviço de física, transição de estado da entidade Voo para 'EmVoo' e geração do 'ResultadoLancamento'.
- Q: Como a dinâmica temporal da barra oscilante de força deve ser modelada para garantir testabilidade matemática desacoplada da interface gráfica? → A: Modelar o Objeto de Valor puro 'MedidorForcaOscilante' em C# no Domínio/Aplicação, calculando deterministicamente a precisão instantânea (0.0 a 1.0) com base no tempo decorrido e na frequência de oscilação em Hz (padrão 1.0 Hz = 1 ciclo completo por segundo), permitindo 100% de testabilidade unitária e consumo direto pelo script Unity.

---

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Lançamento da Aeronave pela Catapulta (Priority: P1)

Como jogador no início de um novo voo, desejo ajustar o momento de disparo na rampa de lançamento para imprimir a maior força inicial possível na aeronave e começar a trajetória com velocidade máxima.

**Why this priority**: O lançamento é o primeiro passo interativo de qualquer rodada do jogo e define a velocidade e energia cinética inicial da aeronave.

**Independent Test**: Pode ser testado acionando o caso de uso `LancarAeronaveCasoDeUso` com diferentes coeficientes de precisão e validando a velocidade resultante aplicada à aeronave.

**Acceptance Scenarios**:
1. **Given** que a aeronave está posicionada na catapulta no nível 1, **When** o jogador realiza o disparo no ápice da barra de força (100% de precisão), **Then** a aeronave é impulsionada com o vetor de velocidade inicial máxima para aquele nível e o status do voo muda para `EmVoo`.
2. **Given** uma catapulta no nível 3 (evoluída na oficina), **When** o disparo é realizado com 100% de precisão, **Then** a velocidade inicial aplicada é proporcionalmente maior do que a do nível 1 conforme o multiplicador da catapulta.

---

### User Story 2 - Variação de Eficácia do Lançamento por Temporização (Priority: P2)

Como jogador, desejo que o timing do meu toque influencie a força do disparo para que a habilidade e precisão sejam recompensadas com um início de voo superior.

**Why this priority**: Proporciona o elemento de mecânica de habilidade logo nos primeiros segundos de jogo.

**Independent Test**: Testável fornecendo valores de precisão variados (ex: 30%, 75%, 100%) e conferindo que o impulso gerado é escalonado correspondentemente.

**Acceptance Scenarios**:
1. **Given** um medidor oscilante de força, **When** o jogador aciona o botão em um ponto de precisão de 50%, **Then** a força inicial resultante é exatamente 50% da força máxima disponível para o nível atual da catapulta.

---

### Edge Cases

- Tentativa de lançar aeronave já em voo ou finalizada: no nível de Domínio, `Voo.Decolar()` lança `DominioInvalidoException`; no nível de Aplicação, o caso de uso `LancarAeronaveCasoDeUso` encapsula a operação retornando `ResultadoLancamento.CriarFalha(...)` de forma segura para a interface gráfica.
- Parâmetro de precisão menor que 0 ou maior que 1 deve ser normalizado no intervalo fechado [0.10, 1.0], aplicando o piso mínimo protetivo de 0.10f caso o jogador erre completamente o timing.
- Ângulo de lançamento fixo ou configurável deve permanecer dentro dos limites físicos válidos (ex: entre 15° e 60°).

---

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: O sistema DEVE implementar o serviço de domínio `ServicoFisicaVoo` (implementando `IServicoFisicaVoo`) para realizar o cálculo matemático do impulso inicial, e o caso de uso `LancarAeronaveCasoDeUso` na camada de Aplicação para orquestrar a validação dos parâmetros, cálculo do vetor de lançamento e transição da entidade `Voo` para `EmVoo`.
- **FR-002**: A força do lançamento DEVE ser calculada pela fórmula canônica: $\text{VelocidadeEscalarInicial} = \text{FORCA\_BASE} \times (1 + (\text{NivelCatapulta} - 1) \times 0.25) \times \text{PrecisaoEfetiva}$, onde $\text{FORCA\_BASE} = 25.0\text{ m/s}$ e $\text{PrecisaoEfetiva} = \max(0.10f, \min(1.0f, \text{Precisao}))$, assegurando piso mínimo de 10% e velocidade máxima de $25.0\text{ m/s}$ no nível 1 até $81.25\text{ m/s}$ no nível máximo 10.
- **FR-003**: O ângulo padrão de lançamento DEVE ser configurado para 35 graus em relação ao horizonte, projetando o vetor de velocidade inicial `VetorVoo` com avanço horizontal no eixo Z ($\text{VelocidadeInicial} \times \cos(35^\circ)$), altitude vertical no eixo Y ($\text{VelocidadeInicial} \times \sin(35^\circ)$) e desvio lateral nulo no eixo X ($0f$), em total conformidade com o sistema de coordenadas canônico 3D da Unity Engine.
- **FR-004**: Ao disparar com sucesso, a sessão de `Voo` DEVE transitar seu estado para `EmVoo` e registrar a hora/tempo de início.
- **FR-005**: O sistema DEVE garantir que nenhum consumo de combustível ocorra durante a fase de disparo da catapulta.
- **FR-006**: O sistema DEVE implementar o Objeto de Valor `MedidorForcaOscilante` em C# puro, calculando a precisão instantânea normalizada (0.0 a 1.0) em função de uma frequência configurável em Hertz (padrão: 1.0 Hz) e do tempo decorrido, permitindo teste unitário desacoplado da Unity.

### Key Entities

- **`ParametrosLancamento`**: Objeto de valor contendo a precisão (0.0 a 1.0) e o ângulo de saída em graus.
- **`ResultadoLancamento`**: Objeto de valor contendo o vetor de velocidade inicial `VetorVoo` e o status de sucesso.
- **`MedidorForcaOscilante`**: Objeto de valor responsável pela amostragem matemática determinística da barra de força em função do tempo.

---

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: 100% dos testes unitários de cálculo vetorial de lançamento executados com precisão matemática em menos de 100ms, e a suíte completa de testes da solução executada em menos de 200ms.
- **SC-002**: Resposta instantânea da transição de estado da catapulta para voo em menos de 16ms (1 frame a 60 FPS).
- **SC-003**: Aumento linear e consistente de velocidade inicial comprovado a cada nível de catapulta.

---

## Assumptions

- O motor de simulação física desacoplado receberá o vetor inicial e assumirá a continuidade da trajetória balística.
- O ângulo da catapulta permanece constante na primeira versão, permitindo foco exclusivo na precisão de disparo do jogador.
