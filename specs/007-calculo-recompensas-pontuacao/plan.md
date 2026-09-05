# Plano de Implementação: Feature 007 — Cálculo de Recompensas, Conversão de Moedas e Recordes

**Branch**: `007-calculo-recompensas-pontuacao` | **Data**: 2026-09-05 | **Spec**: [spec.md](./spec.md)  
**Artefatos Relacionados**: [research.md](./research.md) | [data-model.md](./data-model.md) | [quickstart.md](./quickstart.md) | [contracts/](./contracts/)

---

## 📋 Resumo da Funcionalidade
Implementar o caso de uso de aplicação `FinalizarVooCasoDeUso` (`IFinalizarVooCasoDeUso`) responsável pelo fechamento formal de sessões de voo pousadas (`StatusVoo.Pousado`), cálculo matemático exato de conversão de métricas físicas em moedas ($\lfloor \text{Distancia} \times 0.1 \rfloor + \lfloor \text{AltitudeMaxima} \times 0.05 \rfloor + \text{MoedasColetadas}$), crédito direto na carteira do jogador via raiz de agregação `ProgressoJogador`, detecção e atualização de recordes históricos de distância e altitude, persistência atômica via `IRepositorioProgresso.SalvarProgressoAsync`, e garantia de execução idempotente protegida por `PremiacaoLiquidada` com retorno do extrato na stack `ResumoFinalizacaoVoo` (`readonly record struct`, `GC Alloc = 0 bytes`) em C# puro (.NET Standard 2.1 e .NET 8.0).

---

## 💻 Contexto Técnico

- **Linguagem / Versão**: C# 12 (.NET 8.0 e .NET Standard 2.1 para retrocompatibilidade com Unity IL2CPP).
- **Dependências Principais**:
  - `AeroAscent.Core.Dominio`: C# puro, sem dependências externas.
  - `AeroAscent.Core.Aplicacao`: Depende exclusivamente do Core.Dominio.
  - Testes: xUnit 2.8+, FluentAssertions (na camada de aplicação), runner .NET 8.0.
- **Armazenamento**: `IRepositorioProgresso` (abstração de persistência assíncrona).
- **Testes**: xUnit com asserções de exatidão matemática (SC-001), latência de execução $< 2\text{ms}$ (SC-002) e idempotência (SC-003).
- **Plataforma Alvo**: Unity Multiplataforma (Windows Desktop DirectX/Vulkan e Android Vulkan/OpenGL ES 3.2).
- **Metas de Performance**:
  - `SC-001`: 100% de exatidão matemática nos testes unitários para a fórmula de recompensas.
  - `SC-002`: Execução completa do caso de uso em menos de 2 milissegundos.
  - `SC-003`: Garantia de idempotência: chamar finalização repetidas vezes não duplica saldo.

---

## 📜 Constitution Check

*GATE: Validação pré e pós-design com base nos princípios da Constituição (v1.2.0).*

| Artigo Constitucional | Diretriz | Status | Justificativa / Validação |
|---|---|---|---|
| **Artigo I** | Ética Familiar e Sem Frustrações | ✅ Aprovado | Recompensas generosas baseadas em habilidade pura, sem mecânicas punitivas ou perda de dados na 1ª execução. |
| **Artigo II** | Gameplay Justo e Sem Barreiras | ✅ Aprovado | Conversão matemática direta e transparente conforme fórmula canônica do PRD ($\lfloor D \times 0.1 \rfloor + \lfloor H \times 0.05 \rfloor + M$). |
| **Artigo III.1** | Idioma 100% em pt-BR | ✅ Aprovado | Todos os identificadores, métodos, interfaces, propriedades e documentações XML (`///`) em Português Brasileiro. |
| **Artigo III.2** | Clean Architecture e Domínio Puro | ✅ Aprovado | Domínio e Aplicação 100% isolados de qualquer dependência de UnityEngine/MonoBehaviour. |
| **Artigo III.4** | Performance Mobile First (`0 bytes GC`) | ✅ Aprovado | `ResumoFinalizacaoVoo` modelado como `readonly record struct` na stack; tempo de execução $< 2\text{ms}$. |

---

## 📂 Estrutura do Projeto

### Documentação da Feature
```text
specs/007-calculo-recompensas-pontuacao/
├── spec.md              # Especificação de requisitos e clarificações da Feature 007
├── plan.md              # Este plano de implementação
├── research.md          # Decisões de arquitetura e orquestração financeira
├── data-model.md        # Modelos de dados, structs e diagramas de classes
├── quickstart.md        # Cenários executáveis de teste de integração
├── contracts/           # Contratos de interface C#
│   └── IFinalizarVooCasoDeUso.cs
└── tasks.md             # Tarefas de implementação (gerado pelo /speckit-tasks)
```

### Código-Fonte da Solução
```text
src/
├── AeroAscent.Core.Dominio/
│   ├── Entidades/
│   │   └── Voo.cs (adição de PremiacaoLiquidada e MarcarPremiacaoLiquidada)
│   └── ObjetosDeValor/
│       └── ResumoFinalizacaoVoo.cs (nova struct na stack)
│
├── AeroAscent.Core.Aplicacao/
│   ├── Contratos/
│   │   └── IFinalizarVooCasoDeUso.cs
│   └── CasosDeUso/
│       └── FinalizarVooCasoDeUso.cs
│
tests/
├── AeroAscent.Core.Dominio.Testes/
│   ├── Entidades/
│   │   └── VooTestes.cs (testes de liquidação de premiação e flag)
│   └── ObjetosDeValor/
│       └── ResumoFinalizacaoVooTestes.cs (testes de integridade da struct)
│
└── AeroAscent.Core.Aplicacao.Testes/
    └── CasosDeUso/
        └── FinalizarVooCasoDeUsoTestes.cs (testes de orquestração, recordes, idempotência e performance)
```

---

## 🗓️ Fases de Execução do Plano

### Fase 0: Pesquisa e Decisões de Engenharia (`research.md`)
- [x] Definir orquestração assíncrona do caso de uso e injeção de `IRepositorioProgresso` (D1).
- [x] Mapear decomposição exata das fontes de moedas de recompensa (D2).
- [x] Modelar a struct na stack `ResumoFinalizacaoVoo` (D3).
- [x] Definir regra de idempotência com a propriedade `PremiacaoLiquidada` em `Voo` (D4).
- [x] Estabelecer validação rigorosa de ciclo de vida do voo (D5).
- [x] Garantir resiliência na primeira execução sem save prévio (D6).

### Fase 1: Design e Contratos de Domínio/Aplicação
- [x] Criar `contracts/IFinalizarVooCasoDeUso.cs`.
- [x] Documentar modelo de dados em `data-model.md`.
- [x] Criar cenários de teste em `quickstart.md`.
- [x] Finalizar plano arquitetural em `plan.md`.

### Fase 2: Implementação e Tarefas (`tasks.md`)
- [ ] Gerar tarefas de implementação através do `/speckit-tasks 007-calculo-recompensas-pontuacao`.
