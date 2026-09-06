# Guia de Inicialização Rápida e Validação: Interface HUD de Voo e Controles Táteis (Feature 011)

## Visão Geral

Este documento descreve como validar a implementação do subsistema de HUD de voo e controles táteis (`ApresentadorHUDVoo`), cobrindo a exibição de telemetria em tempo real, detecção de superação de recorde pessoal, esgotamento e desativação do botão de propulsão (Boost), captura contínua de comandos de subida/descida e suporte a pausa, tanto em testes unitários xUnit no .NET 8 quanto na Unity Engine.

---

## Cenários de Validação Automatizada (xUnit)

### Cenário 1: Atualização de Telemetria e Zero Alocação de Memória
- **Objetivo**: Garantir que o `ApresentadorHUDVoo` projeta a telemetria na stack via `TelemetriaHUDDTO` sem gerar alocações no heap (`GC Alloc = 0 bytes`).
- **Entrada**: Aeronave voando a 125,4 m de distância, 45,2 m de altitude, velocidade horizontal de 28,5 m/s, 75% de combustível restante e 8 moedas coletadas.
- **Resultado Esperado**:
  - `visaoMock.UltimaTelemetria.DistanciaPercorridaMetros == 125.4f`
  - `visaoMock.UltimaTelemetria.AltitudeAtualMetros == 45.2f`
  - `visaoMock.UltimaTelemetria.VelocidadeAtualMetrosPorSegundo == 28.5f`
  - `visaoMock.UltimaTelemetria.PercentualCombustivel == 0.75f`
  - `visaoMock.UltimaTelemetria.MoedasColetadas == 8`
  - `visaoMock.UltimaTelemetria.BoostDisponivel == true`

### Cenário 2: Detecção de Quebra de Recorde e Disparo Único
- **Objetivo**: Garantir que quando a distância ultrapassar o recorde inicial, a visão receba `NotificarNovoRecorde()` exatamente uma única vez por voo.
- **Entrada**: Recorde inicial de 200,0 m; atualização de distância para 199,0 m, depois 201,0 m e posteriormente 250,0 m.
- **Resultado Esperado**:
  - Em 199,0 m: `visaoMock.NovoRecordeNotificado == false`
  - Em 201,0 m: `visaoMock.NovoRecordeNotificado == true` (chamado 1 vez)
  - Em 250,0 m: `visaoMock.ContadorNotificacoesRecorde == 1` (não repete a notificação comemorativa)

### Cenário 3: Esgotamento de Combustível e Desativação do Boost
- **Objetivo**: Garantir que ao esvaziar o combustível, o comando de Boost é cancelado e o botão é desabilitado na visão.
- **Entrada**: Jogador com boost mantido pressionado (`IniciarBoost()`), combustível atinge zero (`voo.Combustivel.EstaVazio == true`).
- **Resultado Esperado**:
  - `apresentador.ObterComandosControle().AcionarBoost == false`
  - `visaoMock.BoostHabilitado == false` (opacidade reduzida e desativado)

### Cenário 4: Despacho e Conflito de Comandos de Arfagem (Pitch)
- **Objetivo**: Validar que os métodos contínuos de controle traduzem corretamente as intenções do jogador em `ParametrosControlePiloto`.
- **Entrada**:
  1. Apenas `IniciarSubida()`: `IntensidadePitch == +1.0f`.
  2. Apenas `IniciarDescida()`: `IntensidadePitch == -1.0f`.
  3. Ambos acionados simultaneamente (multitoque oposto): `IntensidadePitch == 0.0f` (conflito anulado / neutro).
  4. Liberar botões: retorna a neutro (`0.0f`).

### Cenário 5: Botão de Pausa e Cancelamento de Comandos Sustentados
- **Objetivo**: Comprovar que ao acionar `SolicitarPausa()`, qualquer tecla ou toque mantido é cancelado e o evento `AoSolicitarPausa` é emitido.
- **Entrada**: Jogador segurando subida e boost; invocação de `apresentador.SolicitarPausa()`.
- **Resultado Esperado**:
  - `apresentador.EstaPausado == true`
  - `apresentador.ObterComandosControle().TemComandoAtivo == false`
  - `apresentador.ObterComandosControle().AcionarBoost == false`
  - Ouvinte de `AoSolicitarPausa` invocado.

### Cenário 6: Ocultação de Controles no Término do Voo
- **Objetivo**: Comprovar que ao transitar para `StatusVoo.Pousado` ou `Colidido`, os botões de toque são ocultados.
- **Entrada**: `voo.Pousar()`, seguido de `apresentador.Atualizar(voo, estadoFisico)`.
- **Resultado Esperado**:
  - `visaoMock.ControlesVisiveis == false`

---

## Comandos de Execução dos Testes

```powershell
# Executar todos os testes automatizados da solução
dotnet test AeroAscent.slnx

# Executar especificamente os testes do HUD de voo
dotnet test tests/AeroAscent.Core.Aplicacao.Testes/AeroAscent.Core.Aplicacao.Testes.csproj --filter "FullyQualifiedName~ApresentadorHUDVooTestes"
```
