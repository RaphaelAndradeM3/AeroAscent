# Feature Specification: Simulação de Física Aerodinâmica e Controle de Pitch

**Feature Branch**: `003-fisica-voo-aerodinamica`  
**Created**: 2026-09-04  
**Status**: Ready for Planning  
**Input**: User description: "003 - Simulação Física de Sustentação, Arrasto, Gravidade e Controle de Pitch (inclinação do nariz) para cálculo desacoplado de trajetória."

---

## Clarifications

### Session 2026-09-05
- Q: Qual convenção de eixos 3D a simulação aerodinâmica e o controle de pitch devem adotar para o plano de voo longitudinal? → A: Adotar Eixo Z como avanço horizontal para frente e Eixo Y como altitude (plano Y-Z), alinhando 100% ao vetor de lançamento da Feature 002 e ao padrão canônico 3D da Unity Engine (Vector3.forward = Z, Vector3.up = Y, Vector3.right = X com rotação de pitch em torno de X).
- Q: Como os coeficientes aerodinâmicos de sustentação (CL), arrasto (CD) e a zona de estol devem ser modelados matematicamente? → A: Modelo Arcade Balanceado: CL linear com ângulo de ataque (alfa) até 20° (CL_max ~ 1.5) com transição suave pós-estol; CD parabólico (CD0 + k*CL²). O estol induz mergulho suave sem punição excessiva, acolhedor para famílias e crianças (Artigos I e II).
- Q: Como a entrada do jogador (-1.0 a +1.0) deve influenciar a inclinação do pitch da aeronave ao longo do tempo? → A: Taxa Angular Suave com Limites: o input (-1.0 a +1.0) comanda a velocidade de rotação do pitch (taxa padrão de até 45°/s), limitando a inclinação entre -45° (mergulho) e +60° (subida), com autoestabilização suave alinhada à trajetória balística ao soltar os controles.
- Q: Como a simulação física deve responder quando a altitude da aeronave atinge o nível do solo (Y <= 0)? → A: Deslize com Atrito de Solo: ao tocar o solo (Y <= 0), Vy é zerada (altitude travada em 0) e a aeronave desliza desacelerando por atrito cinético com o solo (mu ~ 0.3) até a velocidade decair abaixo de 0.5 m/s, momento em que a sessão transita automaticamente para 'Pousado' (voo.Pousar()).
- Q: Como o estado físico da aeronave e o caso de uso de atualização devem ser estruturados na Clean Architecture para garantir zero alocação (GC Alloc = 0 bytes)? → A: EstadoFisicoAeronave como readonly record struct: struct imutável na stack (Posicao, Velocidade, InclinacaoPitch, ForcaResultante, NoSolo) garantindo GC Alloc = 0 bytes. ServicoFisicaVoo calcula a cinemática pura no Domínio e AtualizarFisicaVooCasoDeUso orquestra as métricas e estado do Voo na Aplicação.

---

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Controle de Inclinação do Nariz (Pitch) e Balanço de Forças (Priority: P1)

Como jogador durante o voo livre, desejo inclinar o nariz do avião para cima a fim de ganhar sustentação e altitude, ou para baixo para mergulhar e ganhar velocidade horizontal, permitindo que eu planeje e controle minha trajetória.

**Why this priority**: É a mecânica central do core loop de voo do AeroAscent, onde a habilidade de pilotagem do jogador se manifesta.

**Independent Test**: Testável alimentando o `ServicoFisicaVoo` com diferentes ângulos de pitch e velocidades atuais, e verificando a aplicação correta dos vetores de sustentação (*lift*) e arrasto (*drag*).

**Acceptance Scenarios**:
1. **Given** a aeronave voando a 20 m/s horizontalmente, **When** o jogador inclina o nariz 15 graus para cima, **Then** o serviço de física calcula uma força de sustentação vertical positiva e aumenta ligeiramente o coeficiente de arrasto frontal.
2. **Given** a aeronave em altitude elevada, **When** o jogador inclina o nariz para baixo (mergulho), **Then** a sustentação diminui, a gravidade acelera a aeronave e a velocidade horizontal aumenta progressivamente.

---

### User Story 2 - Influência da Melhoria de Aerodinâmica no Arrasto (Priority: P2)

Como jogador que investiu em melhorias de aerodinâmica na oficina, desejo que meu avião sofra menos arrasto do ar para que possa manter velocidade e planar por distâncias significativamente maiores.

**Why this priority**: Conecta a progressão do jogador diretamente com a física do jogo, tornando as melhorias imediatamente perceptíveis no voo.

**Independent Test**: Testável comparando a taxa de desaceleração de uma aeronave com aerodinâmica nível 1 versus nível 5 sob as mesmas condições iniciais.

**Acceptance Scenarios**:
1. **Given** duas aeronaves lançadas à mesma velocidade inicial, **When** a aeronave com maior nível de aerodinâmica plana livremente, **Then** seu coeficiente de arrasto é menor e ela percorre uma distância horizontal maior antes de perder sustentação.

---

### Edge Cases

- Velocidade horizontal baixa ou nula em voo: a sustentação decai e a aeronave entra em estol suave, ajustando a inclinação para mergulho gradual sem travamentos bruscos.
- Ângulo de ataque acima do estol (> 20°): o $C_L$ decai suavemente e o arrasto induzido aumenta, forçando a perda gradual de sustentação sem queda vertical instantânea.
- Ângulos extremos de inclinação (ex: > 80° para cima): o arrasto atinge o pico e a velocidade decai rapidamente.
- Altitude no solo ($Y \le 0$): Vy é zerada, travando a altitude em zero e aplicando atrito cinético de solo ($\mu \approx 0.3$) no avanço horizontal ($Z$) até parada completa ($< 0.5\text{ m/s}$), finalizando o voo com status `Pousado`.

---

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: O sistema DEVE fornecer o serviço de domínio `IServicoFisicaVoo` com método puro para cálculo das forças que atuam no avião a cada delta de tempo ($dt$).
- **FR-002**: A força de sustentação ($L$) DEVE seguir o Modelo Arcade Balanceado, sendo proporcional ao quadrado da velocidade e ao coeficiente de sustentação $C_L(\alpha)$: linear com o ângulo de ataque até $\alpha_{\text{estol}} = 20^\circ$ ($C_{L\max} \approx 1.5$) com atenuação suave pós-estol: $L = \frac{1}{2} \cdot \rho \cdot v^2 \cdot S \cdot C_L(\alpha)$.
- **FR-003**: A força de arrasto ($D$) DEVE ser calculada pelo modelo parabólico ($C_D = C_{D0} + k \cdot C_L^2$), com $C_{D0} \approx 0.04$, considerando a redução linear de arrasto baseada no nível de aerodinâmica: $D = \frac{1}{2} \cdot \rho \cdot v^2 \cdot S \cdot \frac{C_D(\alpha)}{1 + (\text{NivelAerodinamica} - 1) \times 0.20}$.
- **FR-004**: O sistema DEVE aplicar aceleração da gravidade padrão ($9.81\text{ m/s}^2$ apontando para baixo no eixo Y).
- **FR-005**: O sistema DEVE implementar o Objeto de Valor `EstadoFisicoAeronave` como `readonly record struct` imutável na stack (`GC Alloc = 0 bytes`), contendo posição 3D (`VetorVoo`), velocidade 3D (`VetorVoo`), inclinação de pitch em graus (`float`), força resultante (`VetorVoo`) e indicador de solo (`bool`).
- **FR-006**: O caso de uso `AtualizarFisicaVooCasoDeUso` na camada de Aplicação DEVE orquestrar a atualização periódica do `EstadoFisicoAeronave`, delegando o cálculo puro para `IServicoFisicaVoo`, atualizando as métricas acumuladas da sessão de `Voo` (`DistanciaPercorrida` e `AltitudeMaxima`), e transitando o voo para `StatusVoo.Pousado` ao parar no solo.

### Key Entities

- **`EstadoFisicoAeronave`**: Objeto de valor (`readonly record struct`) contendo `Posicao`, `Velocidade`, `InclinacaoPitchGraus`, `ForcaResultante` e `NoSolo`.
- **`ParametrosControlePiloto`**: Objeto de valor (`readonly record struct`) contendo a intensidade de inclinação de pitch (-1.0 a +1.0) e taxa de variação angular em graus por segundo.

---

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Execução do cálculo físico de um passo de simulação em menos de 0.05ms (permitindo folga massiva para 60 FPS / 16ms por frame).
- **SC-002**: Zero alocação de memória no heap (`GC Alloc = 0 bytes`) durante a chamada contínua de atualização física.
- **SC-003**: Conservação consistente de energia mecânica em simulações sem propulsão externa.

---

## Assumptions

- O ar possui densidade atmosférica padrão constante ao nível do mar nas fases iniciais do jogo.
- O modelo aerodinâmico opera no plano longitudinal Y-Z canônico da Unity Engine (Eixo Z para frente como avanço horizontal, Eixo Y para cima como altitude, e Eixo X lateral nulo = 0, com pitch rotacionando em torno do eixo X).
