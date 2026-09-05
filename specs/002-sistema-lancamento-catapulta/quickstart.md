# Guia de Validação Rápida: Sistema de Lançamento e Catapulta

**Feature**: `002-sistema-lancamento-catapulta`  
**Data**: 2026-09-04  
**Status**: Pronto para Validação  

---

## 🎯 Objetivo
Validar que a mecânica de lançamento inicial da catapulta, cálculo de impulso vetorial 3D e orquestração do caso de uso operam com 100% de conformidade com os requisitos de física, Clean Architecture e critérios de sucesso estabelecidos.

---

## 🛠️ Pré-requisitos
- **SDK .NET 8.0+** instalado no ambiente de desenvolvimento.
- Solução `AeroAscent.slnx` compilando sem avisos.

---

## 🧪 Comandos de Execução e Verificação

### 1. Compilação Completa da Solução
```powershell
dotnet build AeroAscent.slnx --configuration Release
```
**Resultado Esperado:** Compilação concluída com `0 Erros` e `0 Avisos`.

### 2. Execução dos Testes Automatizados (xUnit)
```powershell
dotnet test --configuration Release --verbosity normal
```
**Resultado Esperado:**
- Todos os testes unitários e de integração aprovados com 0 falhas.
- Tempo total de execução inferior a **200 milissegundos** (Critério SC-001).

---

## 📋 Cenários de Validação Funcional

### Cenário 1: Lançamento com Força Máxima (100% de Precisão) no Nível 1
1. Instanciar `Aeronave` padrão (Catapulta nível 1).
2. Criar sessão de `Voo` no status `EmPreparacao`.
3. Executar `LancarAeronaveCasoDeUso` com precisão de 1.0 (100%) e ângulo de 35°.
4. **Verificações:**
   - Sucesso = `true`.
   - Status do voo alterado para `EmVoo`.
   - Módulo da velocidade escalar = $25.0\text{ m/s}$.
   - Componente Z (horizontal) $\approx 20.479\text{ m/s}$ ($25 \times \cos(35^\circ)$).
   - Componente Y (vertical) $\approx 14.339\text{ m/s}$ ($25 \times \sin(35^\circ)$).
   - Componente X (lateral) = $0.0\text{ m/s}$.

### Cenário 2: Escalonamento de Força com Catapulta Evoluída (Nível 3)
1. Instanciar `Aeronave` com Catapulta no nível 3 ($1 + (3 - 1) \times 0.25 = 1.5\times$ multiplicador).
2. Executar lançamento com 100% de precisão.
3. **Verificações:**
   - Velocidade escalar total = $25.0 \times 1.5 = 37.5\text{ m/s}$.
   - Componente Z $\approx 30.718\text{ m/s}$.
   - Componente Y $\approx 21.509\text{ m/s}$.

### Cenário 3: Proteção de Piso Mínimo em Falha de Timing (Precisão 0%)
1. Executar lançamento com precisão de 0.0 (0%).
2. **Verificações:**
   - A precisão efetiva aplicada é de no mínimo 0.10 (10%).
   - Velocidade escalar = $25.0 \times 1.0 \times 0.10 = 2.5\text{ m/s}$.
   - Voo decola normalmente (`EmVoo`) sem travamento ou lançamento nulo.

### Cenário 4: Bloqueio de Lançamento Duplo
1. Executar lançamento com sucesso no voo.
2. Tentar executar um segundo lançamento no mesmo voo ativo (`EmVoo`).
3. **Verificações:**
   - O caso de uso retorna falha (`Sucesso = false`) com mensagem descritiva, sem corromper o estado em voo.

### Cenário 5: Dinâmica do Medidor de Força Oscilante
1. Criar `MedidorForcaOscilante` com frequência padrão de 1.0 Hz.
2. Validar que em $t = 0.0s$ o fator é $0.0$, em $t = 0.5s$ atinge exatamente o ápice $1.0$, e em $t = 1.0s$ retorna a $0.0$.
