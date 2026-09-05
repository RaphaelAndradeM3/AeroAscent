# Guia de Validação Rápida: Feature 008 — Loja e Oficina de Upgrades da Aeronave

**Branch**: `008-oficina-loja-upgrades` | **Data**: 2026-09-05  
**Spec**: [spec.md](./spec.md) | **Plano**: [plan.md](./plan.md) | **Modelo de Dados**: [data-model.md](./data-model.md)

---

## 🎯 Objetivo
Validar de ponta a ponta o fluxo de inspeção de catálogo, cálculo de custos exponenciais, compra de melhorias mecânicas e persistência atômica da carteira e da aeronave do jogador.

---

## 🧪 Cenários de Validação

### Cenário 1: Consulta Inicial da Oficina (Jogador Novo com Saldo Zerado)
- **Pré-condição**: Perfil com aeronave padrão (nível 1 em Motor, Aerodinâmica, Tanque e Catapulta) e saldo = 0 moedas.
- **Ação**: Invocar `ConsultarOficinaCasoDeUso.ExecutarAsync()`.
- **Resultado Esperado**:
  - Retorna 4 itens:
    - Motor: Nível 1, Próximo Custo = 50 moedas, `PodeComprar = false`, `EstaNoNivelMaximo = false`.
    - Aerodinâmica: Nível 1, Próximo Custo = 40 moedas, `PodeComprar = false`, `EstaNoNivelMaximo = false`.
    - Tanque: Nível 1, Próximo Custo = 30 moedas, `PodeComprar = false`, `EstaNoNivelMaximo = false`.
    - Catapulta: Nível 1, Próximo Custo = 60 moedas, `PodeComprar = false`, `EstaNoNivelMaximo = false`.

---

### Cenário 2: Compra Bem-Sucedida de Melhoria com Saldo Suficiente (US1)
- **Pré-condição**: Jogador com 200 moedas e Motor no nível 1. Custo para nível 2 = 50 moedas.
- **Ação**: Invocar `ComprarMelhoriaCasoDeUso.ExecutarAsync(TipoMelhoria.Motor)`.
- **Resultado Esperado**:
  - Retorna `ResultadoCompraMelhoria`:
    - `Tipo = Motor`
    - `NivelAnterior = 1`
    - `NovoNivel = 2`
    - `CustoPago = 50`
    - `SaldoRestante = 150`
    - `AtingiuNivelMaximo = false`
    - `ProximoCusto = 75` (fórmula exponencial: $\lfloor 50 \times 1.5 \rfloor = 75$)
  - No repositório, `aeronave.NivelMotor == 2` e `saldoMoedas == 150`.

---

### Cenário 3: Rejeição de Compra por Saldo Insuficiente
- **Pré-condição**: Jogador com 20 moedas e Tanque no nível 1. Custo para nível 2 = 30 moedas.
- **Ação**: Invocar `ComprarMelhoriaCasoDeUso.ExecutarAsync(TipoMelhoria.TanqueCombustivel)`.
- **Resultado Esperado**:
  - Lança `SaldoInsuficienteException`.
  - O saldo permanece intacto em 20 moedas.
  - O nível do Tanque permanece inalterado em 1.
  - `IRepositorioProgresso.SalvarProgressoAsync` NÃO é invocado.

---

### Cenário 4: Bloqueio no Teto Máximo de Nível (Nível 10)
- **Pré-condição**: Jogador com saldo abundante (10.000 moedas) e Catapulta já no nível 10.
- **Ação**: Invocar `ComprarMelhoriaCasoDeUso.ExecutarAsync(TipoMelhoria.Catapulta)`.
- **Resultado Esperado**:
  - Lança `MelhoriaNivelMaximoException`.
  - Saldo permanece inalterado.
  - Ao consultar a oficina, a Catapulta exibe `NivelAtual = 10`, `CustoProximoNivel = null`, `PodeComprar = false` e `EstaNoNivelMaximo = true`.

---

### Cenário 5: Resiliência na Primeira Execução (Repositório retorna null)
- **Pré-condição**: `IRepositorioProgresso.CarregarProgressoAsync()` retorna `null`.
- **Ação**: Invocar `ConsultarOficinaCasoDeUso.ExecutarAsync()`.
- **Resultado Esperado**:
  - Instancia `ProgressoJogador.CriarNovo()` silenciosamente.
  - Retorna o catálogo dos 4 componentes no nível 1 sem exceções.

---

## ⚡ Comandos de Execução dos Testes

```powershell
# Executar suíte completa de testes da aplicação
dotnet test tests/AeroAscent.Core.Aplicacao.Testes/AeroAscent.Core.Aplicacao.Testes.csproj

# Executar suíte completa da solução
dotnet test AeroAscent.slnx
```
