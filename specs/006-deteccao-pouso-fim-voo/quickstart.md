# Guia Rápido de Validação (Quickstart): Feature 006 — Detecção de Pouso e Transição de Fim de Voo

## 🎯 Objetivo
Este guia descreve os cenários de teste automatizados e fluxos de validação ponta a ponta para verificar o contato com o solo, a desaceleração física contínua por atrito terrestre, a parada total no limiar canônico de $0.15\text{ m/s}$, a transição da sessão de voo para `StatusVoo.Pousado`, o disparo do evento de conclusão e a garantia inegociável de zero alocação no heap (`GC Alloc = 0 bytes`).

---

## 🛠️ Pré-requisitos
- .NET 8.0 SDK instalado.
- Repositório na branch `006-deteccao-pouso-fim-voo`.
- Execução de testes a partir da raiz do repositório:
  ```powershell
  dotnet test AeroAscent.slnx
  ```

---

## 🧪 Cenários de Teste Ponta a Ponta

### Cenário 1: Contato com Solo e Desaceleração Contínua por Atrito (US1 - P1)
1. **Configuração**:
   - Aeronave em trajetória descendente: altitude $Y = 0.5\text{ m}$, velocidade vertical $V_y = -3.0\text{ m/s}$, velocidade longitudinal $V_z = 20.0\text{ m/s}$.
2. **Execução**:
   - Executar passo de simulação física via `ServicoFisicaVoo.SimularPasso(...)`.
3. **Validação**:
   - A altitude é restrita ao piso: $Y = 0.0\text{ m}$ (não penetra o solo).
   - A velocidade vertical é totalmente anulada: $V_y = 0.0\text{ m/s}$.
   - O estado físico registra `NoSolo = true`.
   - A força de atrito horizontal é aplicada: $F_{\text{atrito}} = -\mu \cdot m \cdot g \approx -2.943 \cdot m\text{ N}$.
   - A velocidade horizontal $V_z$ diminui continuamente a cada $\Delta t$.

---

### Cenário 2: Parada Total no Limiar Canônico de $0.15\text{ m/s}$ (US1 - P1)
1. **Configuração**:
   - Aeronave deslizando no solo (`NoSolo = true`) com $V_z = 0.20\text{ m/s}$.
2. **Execução**:
   - Executar um passo de simulação com $\Delta t = 0.1\text{ s}$ onde a desaceleração levaria a velocidade para $< 0.15\text{ m/s}$.
3. **Validação**:
   - A velocidade longitudinal é congelada em $V_z = 0.0\text{ m/s}$.
   - O pitch é fixado em $0.0^\circ$ (horizontal).
   - O propulsor é mantido inativo.
   - Chamadas subsequentes à física mantêm a aeronave em repouso absoluto ($V = 0, Y = 0$).

---

### Cenário 3: Transição para `StatusVoo.Pousado` e Consolidação de Métricas (US2 - P2)
1. **Configuração**:
   - Sessão de voo ativa (`voo.Decolar()`) com 150 metros percorridos e 12 moedas coletadas.
   - Aeronave atinge parada total no solo ($NoSolo = true, V_z = 0$).
2. **Execução**:
   - Invocar o caso de uso `ProcessarPousoFimVooCasoDeUso.Executar(voo, estadoAtual)`.
3. **Validação**:
   - O status da sessão transita de `StatusVoo.EmVoo` para `StatusVoo.Pousado`.
   - `voo.Resultado` é preenchido com as métricas definitivas (distância final travada, altitude máxima e moedas).
   - O objeto `ResultadoFimVoo` retornado indica `AeronaveParou = true` e `Status = StatusVoo.Pousado`.

---

### Cenário 4: Notificação Desacoplada do Evento de Fim de Voo (SC-002)
1. **Configuração**:
   - Criar um mock ou spy implementando `IPublicadorEventosVoo`.
   - Injetar o publicador no construtor de `ProcessarPousoFimVooCasoDeUso`.
2. **Execução**:
   - Executar o pouso da aeronave em repouso.
3. **Validação**:
   - O método `PublicarVooConcluido` é invocado com sucesso contendo as métricas idênticas às consolidadas na entidade `Voo`.
   - O tempo de execução da chamada é inferior a 10ms (SC-002).

---

### Cenário 5: Benchmark de Zero Alocação de Memória no Heap (SC-003)
1. **Configuração**:
   - Warm-up do JIT para `SimularPasso` no solo e `ProcessarPousoFimVooCasoDeUso.Executar`.
   - Limpeza forçada do Garbage Collector (`GC.Collect()`).
2. **Execução**:
   - Medir bytes alocados via `GC.GetAllocatedBytesForCurrentThread()`.
   - Executar um loop de 10.000 iterações de deslizamento por atrito e teste de pouso.
3. **Validação**:
   - O total de bytes alocados no heap durante o loop deve ser estritamente igual a `0 bytes`.
