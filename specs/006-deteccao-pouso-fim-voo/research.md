# Pesquisa Técnica e Decisões de Arquitetura: Feature 006 — Detecção de Pouso e Transição de Fim de Voo

**Branch**: `006-deteccao-pouso-fim-voo` | **Data**: 2026-09-05  
**Spec**: [spec.md](./spec.md)

---

## 🔬 Pesquisa e Decisões de Engenharia

### D1: Orquestração do Pouso e Transição de Fim de Voo na Clean Architecture
- **Problema**: Como e onde orquestrar a detecção de parada no solo, o congelamento cinemático, a transição de estado da entidade `Voo` para `StatusVoo.Pousado` e a emissão do evento de conclusão.
- **Decisão**:
  - Criar o caso de uso dedicado `ProcessarPousoFimVooCasoDeUso` na camada `AeroAscent.Core.Aplicacao.CasosDeUso`, implementando o contrato `IProcessarPousoFimVooCasoDeUso`.
  - O motor de física (`ServicoFisicaVoo`) permanece estritamente responsável pelo cálculo de forças (atrito desacelerador $\mu \cdot g$, nivelamento de pitch e fixação de $Y=0, V_y=0$).
  - O caso de uso de aplicação inspeciona `estadoAtual.NoSolo` e $V_z \le 0.0f$. Quando a aeronave para completamente no solo e a sessão está em `StatusVoo.EmVoo`:
    1. Atualiza a métrica final de distância percorrida;
    2. Invoca `voo.Pousar()`, transitando para `StatusVoo.Pousado` e calculando o `ResultadoVoo`;
    3. Emite notificação de fim de voo via `IPublicadorEventosVoo`;
    4. Retorna a struct na stack `ResultadoFimVoo`.
- **Alternativas Rejeitadas**:
  - *Acoplar a finalização de voo dentro de `ServicoFisicaVoo`*: Rejeitada por violar o Princípio da Responsabilidade Única (SRP). O serviço de física cuida de cinemática e forças; casos de uso cuidam de regras de fluxo e ciclo de vida da sessão.
  - *Delegar a chamada a `voo.Pousar()` diretamente para scripts MonoBehaviour da Unity*: Rejeitada por acoplar regras de negócio ao framework de apresentação, violando a Clean Architecture.

---

### D2: Limiar Canônico de Parada Total no Solo ($V_z \le 0.15\text{ m/s}$)
- **Problema**: Qual limiar de velocidade longitudinal deve determinar a transição de deslizamento para congelamento total de movimento.
- **Decisão**:
  - Fixar canonicamente o limiar em **$0.15\text{ m/s}$**, atualizando a constante `VELOCIDADE_LIMIAR_PARADA_SOLO = 0.15f` em `ServicoFisicaVoo`.
  - Quando $V_z < 0.15\text{ m/s}$, a velocidade é imediatamente zerada ($V_z = 0$), o pitch é fixado em $0^\circ$ e a aceleração residual de atrito cessa.
  - Essa velocidade corresponde a $< 0.54\text{ km/h}$, gerando uma desaceleração visualmente contínua e suave até o repouso absoluto, sem paradas abruptas ou congelamentos prematuros.
- **Alternativas Rejeitadas**:
  - *Manter $0.50\text{ m/s}$*: Rejeitada por causar sensação de freio repentino ("travamento") na visão do jogador.
  - *Decaimento assintótico infinito ($V_z \to 0$)*: Rejeitada por impedir que a aeronave pare formalmente em tempo finito, atrasando indefinidamente o encerramento do voo.

---

### D3: Resposta de Impacto Vertical e Nivelamento Suave de Pitch
- **Problema**: Como tratar o impacto da fuselagem com o solo ($Y \le 0$), especialmente em ângulos descendentes acentuados ou mergulhos.
- **Decisão**:
  - Absorção plástica instantânea no eixo vertical: $Y$ é clampado estritamente em $0.0\text{ m}$ e $V_y$ é anulado ($V_y = 0.0\text{ m/s}$).
  - A força normal do solo equilibra o peso gravitacional ($N = m \cdot g$).
  - Nivelamento contínuo da arfagem (pitch): durante o deslizamento no solo, o ângulo de pitch $\theta$ é suavemente restaurado para $0.0^\circ$ (horizontal) à taxa de $15.0^\circ/\text{s}$ ($novoPitch = \max(0.0^\circ, pitch - 15.0 \cdot \Delta t)$).
  - Em mergulhos severos, o solo absorve o impacto vertical sem quiques caóticos ou penetração de terreno (SC-001).
- **Alternativas Rejeitadas**:
  - *Quique elástico com coeficiente de restituição*: Rejeitada para o MVP casual por introduzir instabilidade numérica e frustração na medição de distância final (conforme Artigo I da Constituição).

---

### D4: Bloqueio Estrito de Propulsão (Boost) e Comandos do Piloto no Solo
- **Problema**: O que acontece se o jogador mantiver o botão de boost pressionado ou tentar girar a arfagem após tocar o solo.
- **Decisão**:
  - No exato frame em que `NoSolo == true` ou `StatusVoo == Pousado`:
    1. O propulsor é imediatamente desativado (`EstadoPropulsor.CriarInativo`);
    2. A queima de combustível é bloqueada (`tempoEfetivoQueima = 0`), preservando integralmente o combustível restante;
    3. Qualquer comando de controle do piloto (`ParametrosControlePiloto`) é desconsiderado pela física e pelo caso de uso.
- **Alternativas Rejeitadas**:
  - *Permitir aceleração com propulsor no chão*: Rejeitada por descaracterizar a dinâmica de pouso e atrito de parada.

---

### D5: Despacho Desacoplado do Evento de Fim de Voo (`IPublicadorEventosVoo`)
- **Problema**: Como notificar os subsistemas de UI, áudio e persistência/economia sem acoplamento direto ou alocação dinâmica de delegates no loop.
- **Decisão**:
  - Definir o contrato `IPublicadorEventosVoo` na camada de Domínio (`AeroAscent.Core.Dominio.Contratos`).
  - O caso de uso `ProcessarPousoFimVooCasoDeUso` recebe `IPublicadorEventosVoo?` opcionalmente no construtor.
  - Ao concretizar o pouso, despacha `publicador.PublicarVooConcluido(resultado)` e retorna a struct `ResultadoFimVoo`.
  - A struct `ResultadoFimVoo` é um `readonly record struct` alocado na stack (`GC Alloc = 0 bytes`).
- **Alternativas Rejeitadas**:
  - *Eventos estáticos C# (`Action<...>`) globais*: Rejeitada por dificultar paralelismo de testes unitários e introduzir acoplamento global.
