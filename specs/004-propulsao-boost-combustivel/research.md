# Pesquisa Técnica e Decisões de Arquitetura: Feature 004 (Propulsão, Boost e Combustível)

**Branch**: `004-propulsao-boost-combustivel`  
**Data**: 2026-09-05  
**Documento de Especificação**: [spec.md](file:///h:/tmp/RSA/Loterias/JogosMaster/GitHub/AeroAscent/specs/004-propulsao-boost-combustivel/spec.md)

---

## 1. Contexto e Objetivos

A Feature 004 introduz o sistema de propulsão ativa (*boost*) e queima contínua de combustível durante o voo da aeronave em AeroAscent. O jogador pode acionar e sustentar o comando de impulso para acelerar a aeronave na direção de seu nariz (ângulo de arfagem $\theta$), consumindo o combustível do tanque até o corte automático instantâneo ao esgotar.

Esta pesquisa consolida as escolhas de física vetorial, conservação de energia em passos fracionários, modelagem de dados na stack (`GC Alloc = 0 bytes`) e orquestração limpa entre Domínio e Aplicação em C# .NET Standard 2.1 / .NET 8.

---

## 2. Decisões de Arquitetura e Engenharia

### Decisão 1: Vetorização Tridimensional do Empuxo ($T$) e Decomposição no Ângulo de Pitch ($\theta$)

- **Decisão**: A força de empuxo escalar $T$ é gerada pelo motor conforme $T = \text{EmpuxoBase} \times (1 + (\text{NivelMotor} - 1) \times 0.30)$, adotando $\text{EmpuxoBase} = 120.0\text{ N}$. Esta força atua estritamente ao longo do eixo longitudinal do nariz do avião, decomposta no plano vertical de voo (Y-Z):
  $$T_x = 0$$
  $$T_y = T \cdot \sin(\theta)$$
  $$T_z = T \cdot \cos(\theta)$$
  onde $\theta$ é o ângulo de arfagem (*pitch*) em radianos.
- **Racional**:
  - No voo 2D/3D da Unity no plano Y-Z (onde Z é avanço frontal e Y é altitude vertical), inclinar o bico para cima ($\theta > 0$) direciona parte do empuxo para o ganho de altitude vertical ($T_y > 0$), permitindo subidas verticais vigorosas.
  - Com $\text{EmpuxoBase} = 120.0\text{ N}$, a força de propulsão supera com margem justa a força gravitacional ($P = m \cdot g = 10.0\text{ kg} \times 9.81\text{ m/s}^2 = 98.1\text{ N}$), gerando aceleração ascendente líquida mesmo em subidas verticais ($90^\circ$).
  - No nível máximo de motor (Nível 10), o empuxo atinge $120.0 \times (1 + 9 \times 0.30) = 120.0 \times 3.70 = 444.0\text{ N}$, proporcionando aceleração massiva de até $44.4\text{ m/s}^2$ (~4.5G), compatível com o gênero arcade empolgante.
- **Alternativas Rejeitadas**:
  - *Empuxo puramente horizontal ($T_z = T, T_y = 0$)*: Rejeitado porque impediria o jogador de manobrar verticalmente usando o motor, empobrecendo a jogabilidade.
  - *Empuxo alinhado com o vetor de velocidade atual ($\vec{V} / |\vec{V}|$)*: Rejeitado porque a física real e arcade de jatos/foguetes empurra na atitude do nariz do avião, não na direção em que o avião está se deslocando no ar.

---

### Decisão 2: Esgotamento Fracionário Preciso e Conservação de Energia ($\Delta t_{\text{queima}}$)

- **Decisão**: Quando o propulsor está ativo e o combustível restante é menor que a quantidade exigida para queimar durante um passo completo de simulação ($\text{CombustivelRestante} < \text{TaxaConsumo} \times \Delta t$), o sistema calcula a fração exata de tempo disponível:
  $$\Delta t_{\text{queima}} = \frac{\text{CombustivelRestante}}{\text{TaxaConsumo}}$$
  O impulso vetorial de empuxo transmitido à aeronave nesse passo é:
  $$\vec{I}_T = \vec{T} \cdot \Delta t_{\text{queima}} \implies \Delta \vec{V}_{\text{empuxo}} = \frac{\vec{T} \cdot \Delta t_{\text{queima}}}{m}$$
  O combustível é então reduzido exatamente a $0.0\text{ un}$ e o estado do propulsor transita imediatamente para `EstaAtivo = false`.
- **Racional**:
  - Cumpre rigorosamente o Critério de Sucesso **SC-001** (erro temporal de corte $< 1\text{ms}$). Em simulações a 50 Hz ou 60 Hz ($\Delta t \approx 16\text{ms}$ a $20\text{ms}$), não aplicar a fração de queima superestimaria a velocidade ou causaria cortes abruptos com desperdício de energia.
  - Conserva rigorosamente a energia cinética e a massa do sistema.
- **Alternativas Rejeitadas**:
  - *Empuxo integral durante todo o passo $\Delta t$*: Rejeitado porque violaria SC-001 ao presentear o jogador com velocidade extra grátis quando restavam apenas centésimos de combustível.
  - *Corte prematuro sem queimar o resíduo*: Rejeitado porque desperdiçaria o combustível residual do jogador.

---

### Decisão 3: Modelagem Zero Alocação de Memória (`readonly record struct EstadoPropulsor`)

- **Decisão**: O estado instantâneo do propulsor é encapsulado no Value Object:
  ```csharp
  public readonly record struct EstadoPropulsor(
      bool EstaAtivo,
      float EmpuxoNewtons,
      float CombustivelRestante,
      float PercentualRestante,
      float TaxaConsumoPorSegundo);
  ```
  Adicionalmente, `EstadoFisicoAeronave` é enriquecido para carregar esse `EstadoPropulsor` na stack, e `ParametrosControlePiloto` é enriquecido com `bool AcionarBoost`.
- **Racional**:
  - Em conformidade com o **Artigo III.4** da Constituição e **SC-002**, classes alocadas no heap (`class` ou delegates) dentro do loop de física contínua gerariam coletas de *Garbage Collection* no Android e Windows, causando engasgos de taxa de quadros (*micro-stutters*).
  - Tipos `readonly record struct` no C# 12/.NET 8 são manipulados puramente em registradores de CPU e stack, resultando em `GC.GetAllocatedBytesForCurrentThread() == 0`.
- **Alternativas Rejeitadas**:
  - *Classe com referência nula ou herança*: Rejeitado devido ao custo de alocação no heap e indireção de ponteiro.

---

### Decisão 4: Bloqueio Rígido em Catapulta (`EmPreparacao`) e Solo (`NoSolo = true` / `Pousado`)

- **Decisão**: A queima de combustível e o acionamento do propulsor são estritamente bloqueados se:
  1. O voo estiver no status `EmPreparacao` (aeronave armada no carrinho da catapulta antes do lançamento).
  2. O voo estiver no status `Pousado` ou `Cancelado`.
  3. A aeronave estiver no solo (`NoSolo = true`).
- **Racional**:
  - Evita que o jogador gaste combustível inadvertidamente antes do disparo da catapulta.
  - Impede que a aeronave continue acelerando no solo indefinidamente após tocar a pista, garantindo desaceleração por atrito cinético e parada completa determinística.
- **Alternativas Rejeitadas**:
  - *Permitir empuxo no solo como taxiamento*: Rejeitado porque o jogo foca no voo e na dinâmica de pouso da catapulta, e permitir empuxo no solo geraria loops infinitos de atrito ou sobreposição com a colisão de pista.

---

### Decisão 5: Calibração Numérica de Tanque e Motor

- **Decisão**:
  - **Empuxo**: $\text{EmpuxoBase} = 120.0\text{ N}$.
    - Nível 1: $120.0\text{ N}$ ($12.0\text{ m/s}^2$ longitudinal).
    - Nível 5: $120.0 \times (1 + 4 \times 0.30) = 264.0\text{ N}$ ($26.4\text{ m/s}^2$).
    - Nível 10: $120.0 \times (1 + 9 \times 0.30) = 444.0\text{ N}$ ($44.4\text{ m/s}^2$).
  - **Capacidade**: $\text{CapacidadeBase} = 20.0\text{ un}$.
    - Nível 1: $20.0\text{ un}$ (duração de $4.0\text{s}$ a $5.0\text{ un/s}$).
    - Nível 5: $20.0 \times (1 + 4 \times 0.25) = 40.0\text{ un}$ (duração de $8.0\text{s}$).
    - Nível 10: $20.0 \times (1 + 9 \times 0.25) = 65.0\text{ un}$ (duração de $13.0\text{s}$).
  - **Taxa de Consumo**: $\text{TaxaConsumo} = 5.0\text{ un/s}$.
- **Racional**:
  - Harmoniza diretamente com as instâncias já criadas em `Voo.cs` e `Combustivel.cs`, assegurando que upgrades na `Oficina` promovam sensível evolução na duração e na potência de aceleração (User Story 2).

---

## 3. Conclusão

Todas as áreas técnicas e físicas foram pesquisadas, quantificadas e validadas. O design respeita 100% da Constituição do Projeto e fornece os alicerces para a fase de contratos e modelagem de dados.
