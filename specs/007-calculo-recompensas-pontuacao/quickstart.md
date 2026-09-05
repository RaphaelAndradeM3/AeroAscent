# Guia de Inicialização Rápida e Cenários de Teste: Feature 007

**Branch**: `007-calculo-recompensas-pontuacao` | **Data**: 2026-09-05  
**Spec**: [spec.md](./spec.md) | **Plano**: [plan.md](./plan.md)

---

## 🚀 Cenários Executáveis de Integração e Teste

### Cenário 1: Conversão Exata de Métricas e Crédito de Saldo (User Story 1 - SC-001)
- **Objetivo**: Garantir que as três fontes de moedas sejam calculadas com exatidão matemática e somadas à carteira do jogador.
- **Passo a Passo**:
  1. Instanciar `ProgressoJogador` com saldo inicial de 100 moedas.
  2. Criar uma sessão de `Voo`, decolar e simular percurso:
     - Distância horizontal: $250.0\text{ m}$;
     - Altitude máxima: $80.0\text{ m}$;
     - Moedas coletadas em voo: $5$;
  3. Executar o pouso (`voo.Pousar()`).
  4. Executar `FinalizarVooCasoDeUso.ExecutarAsync(voo)`.
- **Validações**:
  - `MoedasPorDistancia == 25` ($\lfloor 250 \times 0.1 \rfloor$).
  - `MoedasPorAltitude == 4` ($\lfloor 80 \times 0.05 \rfloor$).
  - `MoedasColetadas == 5`.
  - `MoedasTotalGanhas.Quantidade == 34`.
  - `SaldoTotalAtualizado.Quantidade == 134` ($100 + 34$).
  - O repositório persistiu o agregado com saldo 134.

---

### Cenário 2: Superação e Registro de Novo Recorde Pessoal (User Story 2)
- **Objetivo**: Identificar a superação de recorde e persistir a nova marca no repositório.
- **Passo a Passo**:
  1. Configurar `ProgressoJogador` com recorde anterior de 300.0m de distância e 60.0m de altitude.
  2. Executar voo pousando aos 350.0m de distância e 50.0m de altitude.
  3. Executar `FinalizarVooCasoDeUso.ExecutarAsync(voo)`.
- **Validações**:
  - `EhNovoRecordeDistancia == true`.
  - `EhNovoRecordeAltitude == false`.
  - `progresso.RecordeDistanciaMetros == 350.0f`.
  - `progresso.RecordeAltitudeMetros == 60.0f` (inalterado).

---

### Cenário 3: Voo Abaixo do Recorde Existente
- **Objetivo**: Assegurar que voos com distâncias inferiores não rebaixem nem alterem o recorde histórico.
- **Passo a Passo**:
  1. Configurar `ProgressoJogador` com recorde anterior de 300.0m.
  2. Executar voo pousando aos 280.0m.
  3. Executar `FinalizarVooCasoDeUso.ExecutarAsync(voo)`.
- **Validações**:
  - `EhNovoRecordeDistancia == false`.
  - `progresso.RecordeDistanciaMetros == 300.0f`.

---

### Cenário 4: Idempotência de Chamada Duplicada (SC-003)
- **Objetivo**: Comprovar que invocações repetidas de finalização para o mesmo voo não multiplicam o saldo da carteira nem a contagem de voos.
- **Passo a Passo**:
  1. Executar `FinalizarVooCasoDeUso.ExecutarAsync(voo)` (primeira chamada: saldo salta de 100 para 134, voos realizados de 0 para 1).
  2. Executar novamente `FinalizarVooCasoDeUso.ExecutarAsync(voo)` (segunda chamada acidental).
- **Validações**:
  - O segundo resumo retornado é idêntico ao primeiro em termos de premiação calculada.
  - O saldo do jogador permanece 134 (não saltou para 168).
  - O `TotalVoosRealizados` permanece 1 (não foi para 2).
  - O repositório não sofreu gravações duplicadas.

---

### Cenário 5: Resiliência na Primeira Execução (Perfil Inexistente)
- **Objetivo**: Validar o comportamento transparente em nova instalação sem arquivo de save.
- **Passo a Passo**:
  1. Repositório configurado para retornar `null` em `CarregarProgressoAsync`.
  2. Executar `FinalizarVooCasoDeUso.ExecutarAsync(voo)` com premiação de 40 moedas.
- **Validações**:
  - Perfil é instanciado automaticamente.
  - Saldo final resultante é 40 moedas.
  - Arquivo é salvo no repositório com o ID recém-criado.

---

### Cenário 6: Benchmark de Tempo de Execução (< 2 milissegundos - SC-002)
- **Objetivo**: Comprovar que o caso de uso completa sua execução em menos de 2ms.
- **Passo a Passo**:
  1. Aquecer JIT com uma execução prévia.
  2. Medir tempo de execução com `Stopwatch` de alta precisão.
- **Validações**:
  - `stopwatch.ElapsedMilliseconds < 2`.
