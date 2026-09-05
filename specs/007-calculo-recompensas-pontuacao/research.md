# Pesquisa e Decisões Técnicas: Feature 007 — Cálculo de Recompensas, Conversão de Moedas e Recordes

**Branch**: `007-calculo-recompensas-pontuacao` | **Data**: 2026-09-05  
**Spec**: [spec.md](./spec.md)

---

## 🔬 Contexto e Decisões de Engenharia

### 1. Orquestração do Caso de Uso e Injeção de Dependências (D1)
- **Problema**: O fechamento de voo precisa articular o cálculo de premiação financeira, a atualização da raiz de agregação `ProgressoJogador`, a verificação de recordes e a persistência atômica.
- **Decisão**: Criar o caso de uso `FinalizarVooCasoDeUso` na camada de Aplicação (`AeroAscent.Core.Aplicacao.CasosDeUso`), implementando a interface de contrato `IFinalizarVooCasoDeUso`. Ele recebe `IRepositorioProgresso` via Inversão de Dependências (DIP) e expõe o método assíncrono:
  ```csharp
  Task<ResumoFinalizacaoVoo> ExecutarAsync(Voo voo, CancellationToken cancelamento = default);
  ```
- **Justificativa**: Preserva a Clean Architecture isolando o Domínio puro de operações de I/O assíncronas e evita que camadas externas precisem coordenar manualmente crédito de moedas e salvamento de arquivo.

---

### 2. Decomposição das Recompensas em Moedas (D2)
- **Problema**: O requisito FR-002 e a User Story 1 exigem que as moedas ganhas sejam discriminadas na tela de resultados da UI por fonte geradora (distância horizontal, altitude vertical e coletáveis apanhados no ar).
- **Decisão**: Utilizar a fórmula canônica do PRD:
  $$\text{MoedasPorDistancia} = \lfloor \text{DistanciaMetros} \times 0.1 \rfloor$$
  $$\text{MoedasPorAltitude} = \lfloor \text{AltitudeMaximaMetros} \times 0.05 \rfloor$$
  $$\text{MoedasTotalGanhas} = \text{MoedasPorDistancia} + \text{MoedasPorAltitude} + \text{MoedasColetadas}$$
  O objeto de valor `ResultadoVoo` já calcula e encapsula essa matemática de forma validada; o caso de uso decompõe esses valores individualmente para preenchimento de `ResumoFinalizacaoVoo`.
- **Justificativa**: Evita duplicação de regras matemáticas no código e garante exatidão de 100% (SC-001) com arredondamento estritamente para baixo (*floor*).

---

### 3. Struct na Stack `ResumoFinalizacaoVoo` (D3)
- **Problema**: A UI precisa receber o consolidado completo de resultados em alta performance sem pressionar o Garbage Collector (`GC Alloc = 0 bytes`).
- **Decisão**: Modelar `ResumoFinalizacaoVoo` como `readonly record struct` na camada de domínio (`AeroAscent.Core.Dominio.ObjetosDeValor`):
  ```csharp
  public readonly record struct ResumoFinalizacaoVoo(
      float DistanciaMetros,
      float AltitudeMaximaMetros,
      long MoedasPorDistancia,
      long MoedasPorAltitude,
      int MoedasColetadas,
      Moeda MoedasTotalGanhas,
      Moeda SaldoTotalAtualizado,
      bool EhNovoRecordeDistancia,
      bool EhNovoRecordeAltitude);
  ```
- **Justificativa**: Respeita o Artigo III.4 da Constituição (Performance Mobile First), alocando exclusivamente na stack em chamadas síncronas/assíncronas sem geração de lixo no heap.

---

### 4. Idempotência e Prevenção contra Crédito Duplicado (D4)
- **Problema**: Em interfaces móveis touch, duplos cliques acidentais ou chamadas repetidas de finalização podem gerar créditos indevidos de moedas ou duplicar o total de voos.
- **Decisão**: Estender a entidade `Voo` com a propriedade booleana:
  ```csharp
  public bool PremiacaoLiquidada { get; private set; }
  public void MarcarPremiacaoLiquidada();
  ```
  Ao receber um voo com `PremiacaoLiquidada == true`, o caso de uso `FinalizarVooCasoDeUso`:
  1. Identifica que a sessão já foi faturada;
  2. Não invoca `progresso.ProcessarFimDeVoo(...)`;
  3. Não salva o repositório novamente de forma redundante;
  4. Retorna a struct `ResumoFinalizacaoVoo` com os dados consolidados do voo e o saldo corrente do jogador;
  5. `EhNovoRecordeDistancia` e `EhNovoRecordeAltitude` são retornados como `false` em chamadas repetidas.
- **Justificativa**: Garante o critério de sucesso SC-003 sem necessidade de locks distribuídos ou estruturas pesadas de cache em memória.

---

### 5. Validação de Ciclo de Vida do Voo (D5)
- **Problema**: Tentativa de finalizar um voo que ainda está em decolagem ou em voo livre pode corromper a integridade dos dados da simulação.
- **Decisão**: 
  - Se `voo.Status == StatusVoo.Pousado`: executa a premiação normalmente.
  - Se `voo.Status == StatusVoo.Cancelado`: processa finalização com 0 moedas ganhas, mantendo o saldo e recordes intactos.
  - Se `voo.Status == StatusVoo.EmPreparacao` ou `StatusVoo.EmVoo`: lança `DominioInvalidoException`.
- **Justificativa**: Rigor da Clean Architecture e integridade da máquina de estados.

---

### 6. Resiliência na Primeira Inicialização (D6)
- **Problema**: Em instalações limpas do jogo (Windows ou Android), o arquivo JSON local ainda não existe e `CarregarProgressoAsync` retorna `null`.
- **Decisão**: Se `await _repositorioProgresso.CarregarProgressoAsync(cancelamento)` retornar `null`, o caso de uso cria automaticamente `ProgressoJogador.CriarNovo()`, aplica a premiação e executa `SalvarProgressoAsync`.
- **Justificativa**: Elimina telas de erro ou crashes no primeiro voo de novos jogadores, alinhado à Ética Familiar e Sem Frustrações (Artigo I).
