# Guia de Inicialização Rápida e Validação: Interface do Menu Principal e Oficina (Feature 010)

## Visão Geral

Este documento descreve como validar a implementação do subsistema de apresentação da Oficina e Menu Principal (`ApresentadorOficina`), conferindo a renderização dos 4 cartões de upgrade, a formatação de moedas em pt-BR, o bloqueio de toques rápidos (*spam click*) e a transição para a partida via testes automatizados xUnit no .NET 8 e inspeção visual na Unity Engine.

---

## Cenários de Validação Automatizada (xUnit)

### Cenário 1: Inicialização da Tela e Formatação em pt-BR
- **Objetivo**: Garantir que o `ApresentadorOficina` consulta o catálogo via `IConsultarOficinaCasoDeUso`, projeta exatamente os 4 cartões mecânicos e formata o saldo com separador de milhar por ponto.
- **Entrada**: Saldo de 1.250 moedas, Motor nível 1 (custo 50), Aerodinâmica nível 2 (custo 60), Tanque nível 3 (custo 67), Catapulta nível 1 (custo 60).
- **Resultado Esperado**:
  - `visaoMock.UltimoModelo.SaldoFormatado == "💰 1.250"`
  - 4 cartões populados com `PodeComprar == true`
  - Valores de progresso normalizados consistentes (`0.1f`, `0.2f`, `0.3f`, `0.1f`).

### Cenário 2: Exibição e Bloqueio de Componente no Nível Máximo
- **Objetivo**: Comprovar que componentes no nível 10 exibem o selo "MÁXIMO", barra de 100% e botão desabilitado.
- **Entrada**: Motor nível 10 com saldo de 5.000 moedas.
- **Resultado Esperado**:
  - `cartaoMotor.EstaNoNivelMaximo == true`
  - `cartaoMotor.TextoNivel == "Nível 10 (MAX)"`
  - `cartaoMotor.ProgressoNormalizado == 1.0f`
  - `cartaoMotor.TextoBotao == "MÁXIMO"`
  - `cartaoMotor.PodeComprar == false`

### Cenário 3: Prevenção de Concorrência e Spam Click
- **Objetivo**: Validar que toques múltiplos disparados simultaneamente antes da conclusão do caso de uso de compra são bloqueados pela flag de reentrância.
- **Entrada**: 5 cliques disparados simultaneamente para compra do mesmo item.
- **Resultado Esperado**:
  - Apenas 1 chamada a `IComprarMelhoriaCasoDeUso.ExecutarAsync` é despachada.
  - `IVisaoOficina.DefinirInteracaoHabilitada(false)` é invocado no início e `true` ao final.

### Cenário 4: Disparo do Evento de Decolagem
- **Objetivo**: Validar que ao acionar `SolicitarDecolagem()`, o evento `AoSolicitarDecolagem` é invocado para notificar a orquestração da Unity.
- **Entrada**: Invocação de `apresentador.SolicitarDecolagem()`.
- **Resultado Esperado**: Ouvinte do evento acionado exatamente 1 vez.

---

## Comandos de Execução dos Testes

```powershell
# Executar todos os testes automatizados da solução
dotnet test AeroAscent.slnx

# Executar especificamente a suíte de testes de aplicação e apresentação
dotnet test tests/AeroAscent.Core.Aplicacao.Testes/AeroAscent.Core.Aplicacao.Testes.csproj
```
