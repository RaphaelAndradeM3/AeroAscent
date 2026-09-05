# Guia Rápido de Validação (Quickstart): Sistema de Propulsão (Boost) e Queima de Combustível

> **Feature**: `004-propulsao-boost-combustivel`  
> **Status**: Pronto para Implementação  
> **Idioma**: Português Brasileiro (pt-BR)  

Este guia documenta os cenários de validação executáveis que comprovam o funcionamento ponta a ponta do sistema de propulsão ativa (*boost*), queima contínua de combustível, escalonamento por melhorias na oficina, conservação física em queima fracionária e alocação zero no heap.

---

## 1. Pré-Requisitos e Ambiente de Execução

- **SDK**: .NET 8.0+ instalado.
- **Repositório**: Diretório raiz do projeto AeroAscent clonado.
- **Branch Ativa**: `004-propulsao-boost-combustivel`.
- **Solução**: `AeroAscent.slnx` na raiz.

### Compilação da Solução
```bash
dotnet build AeroAscent.slnx
```

### Execução dos Testes Automatizados
```bash
dotnet test AeroAscent.slnx --logger "console;verbosity=detailed"
```

---

## 2. Cenários de Validação

### Cenário 1: Acionamento de Boost e Aceleração Vetorial no Ângulo do Nariz
- **Objetivo**: Comprovar que o boost acelera a aeronave na direção do nariz (pitch $\theta$) consumindo combustível na taxa de $5.0\text{ un/s}$.
- **Entrada**:
  - Aeronave em voo ativo (`StatusVoo.EmVoo`) a $Y = 50.0\text{ m}$ com velocidade $(0, 0, 20)\text{ m/s}$ e pitch nivelado $\theta = 0^\circ$.
  - Tanque nível 1 cheio ($20.0\text{ un}$).
  - Entrada do piloto: `new ParametrosControlePiloto(0f, 45f, acionarBoost: true)`.
- **Ação**: Executar simulação por 2.0 segundos ($dt = 0.02\text{s}$, 100 passos).
- **Resultado Esperado**:
  - `EstadoPropulsor.EstaAtivo == true` durante os 2.0 segundos.
  - Combustível restante reduz exatamente de $20.0\text{ un}$ para $10.0\text{ un}$ ($2.0\text{s} \times 5.0\text{ un/s} = 10.0\text{ un}$).
  - A velocidade longitudinal $V_z$ atinge acréscimo de aproximadamente $\approx 24.0\text{ m/s}$ ($T = 120\text{ N} / 10\text{ kg} = 12\text{ m/s}^2 \times 2\text{s}$ descontado o arrasto aerodinâmico).
  - Em novo teste com pitch $+30^\circ$, comprovar aceleração vertical positiva $V_y$ e avanço em $V_z$ conforme decomposição trigonométrica ($T_y = T \sin(30^\circ)$, $T_z = T \cos(30^\circ)$).

---

### Cenário 2: Esgotamento Automático com Precisão Temporal de Corte < 1ms (SC-001)
- **Objetivo**: Garantir que o combustível esgota sem resíduo negativo e que o corte do propulsor ocorre com conservação fracionária exata.
- **Entrada**:
  - Aeronave com $0.05\text{ un}$ de combustível restante no tanque.
  - Taxa de queima: $5.0\text{ un/s}$ (tempo de queima disponível = $0.05 / 5.0 = 0.010\text{ s} = 10\text{ ms}$).
  - Passo de simulação: $dt = 0.020\text{ s} = 20\text{ ms}$.
- **Ação**: Executar 1 passo de simulação via `AtualizarFisicaVooCasoDeUso.Executar`.
- **Resultado Esperado**:
  - Tempo efetivo de queima no passo calculado em exatamente $0.010\text{ s}$ ($10\text{ ms}$).
  - O empuxo aplicado é proporcional a $10\text{ ms}$ em vez de $20\text{ ms}$ ($\Delta V = T \times 0.010 / 10.0$).
  - Combustível restante após o passo = exatamente $0.000\text{ un}$ (`EstaVazio == true`).
  - No estado retornado e passos subsequentes: `EstadoPropulsor.EstaAtivo == false` e `EmpuxoNewtons == 0.0f`.
  - Margem temporal de erro menor que $1\text{ ms}$ em conformidade com SC-001.

---

### Cenário 3: Escalonamento por Upgrades de Motor e Tanque (User Story 2)
- **Objetivo**: Comprovar que upgrades aumentam a magnitude da força e a duração do impulso.
- **Entrada**:
  - Aeronave A: Motor Nível 1 ($120\text{ N}$), Tanque Nível 1 ($20\text{ un}$).
  - Aeronave B: Motor Nível 3 ($120 \times 1.6 = 192\text{ N}$), Tanque Nível 3 ($20 \times 1.5 = 30\text{ un}$).
- **Ação**:
  - Medir a aceleração inicial de boost em ambas.
  - Medir a duração de queima contínua até esgotamento em ambas.
- **Resultado Esperado**:
  - Aceleração inicial de B ($19.2\text{ m/s}^2$) é $60\%$ superior à de A ($12.0\text{ m/s}^2$).
  - Duração total de queima de B ($6.0\text{ s}$) é $50\%$ superior à de A ($4.0\text{ s}$).

---

### Cenário 4: Bloqueio Rígido em Catapulta e Solo / Pouso
- **Objetivo**: Provar que o propulsor não queima combustível na preparação nem após pouso/contato com o solo.
- **Entrada**:
  - Teste 4.1: Voo no status `EmPreparacao` com comando `AcionarBoost = true`.
  - Teste 4.2: Aeronave em solo (`NoSolo = true`) ou com status `Pousado` com comando `AcionarBoost = true`.
- **Ação**: Executar simulação de passo nesses estados.
- **Resultado Esperado**:
  - Em ambos os casos: `EstadoPropulsor.EstaAtivo == false`, `EmpuxoNewtons == 0.0f`.
  - A quantidade de combustível permanece 100% inalterada (zero vazamento ou queima inadvertida).

---

### Cenário 5: Verificação de Alocação Zero no Heap (`GC Alloc = 0 bytes` / SC-002)
- **Objetivo**: Validar conformidade inegociável com o Artigo III.4 da Constituição e SC-002.
- **Ação**:
  1. Executar warm-up de 1.000 passos para aquecimento do JIT da CLR.
  2. Capturar memória inicial com `GC.GetAllocatedBytesForCurrentThread()`.
  3. Executar loop de 10.000 passos de simulação contínua com boost ativo e queima de combustível.
  4. Capturar memória final.
- **Resultado Esperado**:
  - `bytesAlocados == 0`.
  - Todos os tipos manipulados no loop (`EstadoFisicoAeronave`, `EstadoPropulsor`, `ParametrosControlePiloto`, `VetorVoo`) residem na stack como `readonly record struct`.
