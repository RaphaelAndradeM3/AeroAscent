# Guia Rápido de Validação (Quickstart): Feature 005 — Coletáveis em Voo e Object Pooling

## 🎯 Objetivo
Este guia descreve os cenários de teste automatizados e fluxos de validação ponta a ponta para verificar o correto funcionamento da coleta de moedas, impulso por anéis de vento (*Air Boost Rings*), geração procedural e reciclagem via Object Pooling com zero alocação no heap (`GC Alloc = 0 bytes`).

---

## 🛠️ Pré-requisitos
- .NET 8.0 SDK instalado.
- Repositório na branch `005-coletaveis-ambiente-pooling`.
- Execução de comandos a partir da raiz do repositório:
  ```powershell
  dotnet test AeroAscent.slnx
  ```

---

## 🧪 Cenários de Teste Ponta a Ponta

### Cenário 1: Coleta de Moeda Flutuante (User Story 1 - P1)
1. **Configuração**:
   - Iniciar sessão de `Voo` ativa (`voo.Decolar()`) com `MoedasColetadas = 0`.
   - Obter uma moeda do pool e posicioná-la em $Z = 50.0\text{ m}$, $Y = 20.0\text{ m}$, $X = 0$.
   - Aeronave posicionada em $Z = 49.0\text{ m}$, $Y = 20.0\text{ m}$ com velocidade $(0, 0, 20)\text{ m/s}$.
2. **Execução**:
   - Executar um passo de simulação via `ProcessarColetaveisVooCasoDeUso`.
3. **Validação**:
   - A distância entre a aeronave e a moeda é de $1.0\text{ m} \le 1.5\text{ m}$ (raio de coleta).
   - `voo.MoedasColetadas` é incrementado para `1`.
   - A moeda é marcada como `Coletado = true`, desativada (`Ativo = false`) e devolvida ao pool.
   - Uma passagem subsequente pela mesma coordenada não gera nova coleta.

---

### Cenário 2: Atravessar Anel de Vento (*Air Boost Ring*) (User Story 2 - P2)
1. **Configuração**:
   - Aeronave em voo livre com velocidade escalar de $15.0\text{ m/s}$ na direção longitudinal: $\vec{V} = (0, 0, 15.0)\text{ m/s}$.
   - Anel de vento posicionado em $Z = 80.0\text{ m}$, $Y = 30.0\text{ m}$.
   - Aeronave cruza a posição do anel ($Z = 80.0\text{ m}$, $Y = 30.0\text{ m}$).
2. **Execução**:
   - Executar o caso de uso `ProcessarColetaveisVooCasoDeUso`.
3. **Validação**:
   - A velocidade resultante da aeronave passa imediatamente para $25.0\text{ m/s}$ ($15.0 + 10.0\text{ m/s}$).
   - O reservatório de combustível (`voo.Combustivel.QuantidadeAtual`) permanece estritamente inalterado.
   - O anel é devolvido ao pool de anéis.

---

### Cenário 3: Reciclagem Automática Fora da Janela Espacial (SC-003)
1. **Configuração**:
   - Pool inicializado com 50 moedas e 15 anéis.
   - Coletáveis ativos espalhados entre $Z = 0$ e $Z = 150\text{ m}$.
   - Aeronave avança rapidamente até $Z = 100.0\text{ m}$.
2. **Execução**:
   - Chamar `ServicoGeracaoProceduralColetaveis.AtualizarJanela` passando a nova posição da aeronave ($Z = 100.0\text{ m}$).
3. **Validação**:
   - Todos os coletáveis com coordenada $Z < 80.0\text{ m}$ ($100.0 - 20.0\text{ m}$) são desativados e liberados de volta ao pool.
   - O contador de coletáveis em uso diminui proporcionalmente, liberando estoque para novos spawns à frente.

---

### Cenário 4: Benchmark de Alocação Zero no Heap (SC-001)
1. **Configuração**:
   - Inicializar o `GerenciadorPoolObjetos<Coletavel>` com 50 moedas.
   - Warm-up do JIT e coleta forçada de GC.
2. **Execução**:
   - Medir bytes alocados via `GC.GetAllocatedBytesForCurrentThread()`.
   - Executar um loop de 10.000 iterações de `Obter()` e `Liberar()` de moedas.
3. **Validação**:
   - Os bytes alocados no heap durante o loop devem ser estritamente iguais a `0 bytes`.
