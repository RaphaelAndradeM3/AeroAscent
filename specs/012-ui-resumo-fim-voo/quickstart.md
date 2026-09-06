# Guia de Inicialização Rápida e Validação: Interface de Resumo de Voo e Celebração de Recorde (Feature 012)

## Visão Geral

Este documento descreve como validar a implementação do subsistema da interface de resumo de voo (`ApresentadorResumoVoo`), cobrindo a formatação e projeção do extrato financeiro, animação de contagem numérica progressiva de moedas (1,5s), cancelamento antecipado com toque na tela (*skip to end*), ativação do banner de novo recorde pessoal e despacho desacoplado de eventos de navegação ("Oficina" e "Voar Novamente"), tanto em testes unitários xUnit no .NET 8 quanto na Unity Engine.

---

## Cenários de Validação Automatizada (xUnit)

### Cenário 1: Formatação Completa do Modelo Visual e Decomposição de Moedas
- **Objetivo**: Garantir que o `ApresentadorResumoVoo` projeta a struct `ModeloVisualResumoVoo` com valores numéricos exatos e strings em pt-BR (vírgulas decimais e separadores de milhar).
- **Entrada**: `ResumoFinalizacaoVoo` com:
  - Distância: 125,4 m (gera 12 moedas)
  - Altitude máxima: 45,2 m (gera 2 moedas)
  - Moedas coletadas no ar: 20
  - Total ganho: 34 moedas
  - Saldo acumulado anterior: 1.216 $\to$ Saldo atualizado: 1.250 moedas
  - `EhNovoRecordeDistancia == false`, `EhNovoRecordeAltitude == false`.
- **Resultado Esperado**:
  - `visaoMock.UltimoModelo.DistanciaFormatada == "125,4 m"`
  - `visaoMock.UltimoModelo.AltitudeFormatada == "45,2 m"`
  - `visaoMock.UltimoModelo.MoedasDistancia == 12`
  - `visaoMock.UltimoModelo.MoedasAltitude == 2`
  - `visaoMock.UltimoModelo.MoedasColetadas == 20`
  - `visaoMock.UltimoModelo.TotalMoedasGanhas == 34`
  - `visaoMock.UltimoModelo.TotalMoedasFormatado == "+34 moedas"`
  - `visaoMock.UltimoModelo.SaldoFinal == 1250`
  - `visaoMock.UltimoModelo.SaldoFinalFormatado == "💰 1.250"`
  - `visaoMock.UltimoModelo.EhNovoRecorde == false`
  - `visaoMock.BotoesNavegacaoHabilitados == false` (bloqueados enquanto animação inicia)

### Cenário 2: Ativação da Celebração de Novo Recorde Pessoal
- **Objetivo**: Garantir que se `EhNovoRecordeDistancia` ou `EhNovoRecordeAltitude` for verdadeiro, o modelo visual marca `EhNovoRecorde == true`.
- **Entrada**:
  1. Teste A: `EhNovoRecordeDistancia = true`, `EhNovoRecordeAltitude = false`.
  2. Teste B: `EhNovoRecordeDistancia = false`, `EhNovoRecordeAltitude = true`.
  3. Teste C: Ambos verdadeiros.
- **Resultado Esperado**:
  - Em todos os casos, `visaoMock.UltimoModelo.EhNovoRecorde == true`.
  - A visão passiva é orientada a ativar o selo "NOVO RECORDE!" e disparar o sistema de partículas comemorativo.

### Cenário 3: Fluxo da Animação de Contagem e Liberação de Botões
- **Objetivo**: Comprovar que a animação desabilita a navegação durante a execução e libera os botões após a conclusão.
- **Entrada**:
  - Início da exibição via `Exibir(resumo)`.
  - Verificação do estado `AnimacaoEmAndamento == true`.
  - Disparo de `visaoMock.SimularConclusaoAnimacaoMoedas()` (término dos 1,5 segundos).
- **Resultado Esperado**:
  - `apresentador.AnimacaoEmAndamento == false`
  - `visaoMock.BotoesNavegacaoHabilitados == true`
  - `visaoMock.ContadorConclusoesAnimacao == 1`

### Cenário 4: Toque na Tela para Pular Animação (*Skip to End*)
- **Objetivo**: Comprovar que o jogador pode tocar na tela a qualquer momento durante a contagem de moedas para exibir o total imediatamente e liberar a navegação.
- **Entrada**:
  - Início da exibição com `AnimacaoEmAndamento == true`.
  - Jogador toca na tela: `visaoMock.SimularCliquePularAnimacao()`.
- **Resultado Esperado**:
  - `visaoMock.ContadorConclusoesAnimacao == 1` (chamou `ConcluirAnimacaoMoedas()`)
  - `apresentador.AnimacaoEmAndamento == false`
  - `visaoMock.BotoesNavegacaoHabilitados == true`

### Cenário 5: Tentativa de Navegação Durante a Animação Força o Pulo
- **Objetivo**: Comprovar que clicar em "Oficina" ou "Voar Novamente" durante a animação não muda de tela prematuramente, mas força a conclusão imediata da animação.
- **Entrada**:
  - Animação em andamento (`AnimacaoEmAndamento == true`).
  - Jogador clica em "Oficina": `visaoMock.SimularCliqueOficina()`.
- **Resultado Esperado**:
  - O evento `AoSolicitarIrParaOficina` NÃO é disparado na primeira tentativa.
  - A animação é concluída instantaneamente (`visaoMock.ContadorConclusoesAnimacao == 1`).
  - Os botões tornam-se habilitados (`visaoMock.BotoesNavegacaoHabilitados == true`).
  - Um segundo clique em "Oficina" agora dispara com sucesso `AoSolicitarIrParaOficina`.

### Cenário 6: Navegação Pós-Voo ("Oficina" e "Voar Novamente")
- **Objetivo**: Garantir o disparo correto dos eventos desacoplados de roteamento de cena quando a animação já estiver concluída.
- **Entrada**:
  - Animação concluída.
  - Invocação de `visaoMock.SimularCliqueOficina()` e, em outro teste, `visaoMock.SimularCliqueVoarNovamente()`.
- **Resultado Esperado**:
  - Disparo do evento C# `AoSolicitarIrParaOficina` (ou `AoSolicitarVoarNovamente`).
  - `visaoMock.TelaOcultada == true`.

---

## Comandos de Execução dos Testes

```powershell
# Executar todos os testes automatizados da solução
dotnet test AeroAscent.slnx

# Executar especificamente os testes da tela de resumo de voo
dotnet test tests/AeroAscent.Core.Aplicacao.Testes/AeroAscent.Core.Aplicacao.Testes.csproj --filter "FullyQualifiedName~ApresentadorResumoVooTestes"
```
