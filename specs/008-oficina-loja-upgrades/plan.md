# Plano de Implementação: Feature 008 — Loja e Oficina de Upgrades da Aeronave

**Branch**: `008-oficina-loja-upgrades` | **Data**: 2026-09-05 | **Spec**: [spec.md](./spec.md)  
**Artefatos Relacionados**: [research.md](./research.md) | [data-model.md](./data-model.md) | [quickstart.md](./quickstart.md) | [contracts/](./contracts/)

---

## 📋 Resumo da Funcionalidade
Implementar o subsistema de oficina e catálogo de melhorias mecânicas da aeronave na camada de Aplicação através de dois casos de uso dedicados:
1. `ComprarMelhoriaCasoDeUso` (`IComprarMelhoriaCasoDeUso`): comando transacional assíncrono responsável pela validação do saldo do jogador, aplicação da regra de evolução via entidade de domínio `Oficina`, débito do custo exponencial ($\lfloor \text{CustoBase} \times 1.5^{N-1} \rfloor$), incremento do nível do componente na `Aeronave` vinculada ao `ProgressoJogador`, persistência atômica via `IRepositorioProgresso.SalvarProgressoAsync` e retorno do extrato imutável `ResultadoCompraMelhoria` na stack (`readonly record struct`, `GC Alloc = 0 bytes`).
2. `ConsultarOficinaCasoDeUso` (`IConsultarOficinaCasoDeUso`): consulta de leitura responsável por obter o progresso do jogador, calcular os custos para a próxima evolução dos 4 componentes (`Motor`, `Aerodinamica`, `TanqueCombustivel`, `Catapulta`), identificar itens no teto máximo (`NIVEL_MAXIMO = 10`) e projetar a lista de `ItemOficinaDTO` pronta para renderização no menu.

---

## 💻 Contexto Técnico

- **Linguagem / Versão**: C# 12 (.NET 8.0 e .NET Standard 2.1 para compatibilidade com Unity IL2CPP).
- **Dependências Principais**:
  - `AeroAscent.Core.Dominio`: C# puro, sem dependências externas.
  - `AeroAscent.Core.Aplicacao`: Depende exclusivamente de `Core.Dominio`.
  - Testes: xUnit 2.8+, FluentAssertions (se aplicável), runner .NET 8.0.
- **Armazenamento**: `IRepositorioProgresso` (persistência atômica assíncrona).
- **Testes**: xUnit cobrindo compras válidas, rejeições por saldo insuficiente (`SaldoInsuficienteException`), bloqueio de compras no teto máximo (`MelhoriaNivelMaximoException`), cálculo de custos escalonados (SC-001) e latência de execução $< 5\text{ms}$ (SC-002).
- **Plataforma Alvo**: Unity Multiplataforma (Windows Desktop DirectX/Vulkan e Android Vulkan/OpenGL ES 3.2).
- **Metas de Performance**:
  - `SC-001`: 100% de testes unitários aprovados para compras válidas, rejeições e custos escalonados.
  - `SC-002`: Execução completa da operação de compra e salvamento em menos de 5 milissegundos.
  - `SC-003`: Impossibilidade matemática de saldo negativo ou duplicação de níveis.

---

## 📜 Constitution Check

*GATE: Validação pré e pós-design com base nos princípios da Constituição (v1.2.0).*

| Artigo Constitucional | Diretriz | Status | Justificativa / Validação |
|---|---|---|---|
| **Artigo I** | Ética Familiar e Sem Frustrações | ✅ Aprovado | Zero mecânicas predatórias ou compras com dinheiro real; progressão orgânica puramente com moedas conquistadas em voo. |
| **Artigo II** | Gameplay Justo e Progressão por Habilidade | ✅ Aprovado | Custos transparentes baseados na fórmula exponencial canônica do PRD; progressão clara e previsível. |
| **Artigo III.1** | Idioma 100% em pt-BR | ✅ Aprovado | Todos os identificadores, métodos, interfaces, propriedades e documentação XML (`///`) estritamente em Português Brasileiro. |
| **Artigo III.2** | Clean Architecture e Domínio Puro | ✅ Aprovado | Domínio e Aplicação 100% isolados de qualquer dependência de `UnityEngine` ou `MonoBehaviour`. |
| **Artigo III.4** | Performance Mobile First (`0 bytes GC`) | ✅ Aprovado | `ResultadoCompraMelhoria` e `ItemOficinaDTO` modelados como `readonly record struct` na stack; tempo de execução $< 5\text{ms}$. |
| **Artigo V** | Checklist de Governança | ✅ Aprovado | Métodos assíncronos utilizam o sufixo `Async`; dependências injetadas exclusivamente via interfaces. |

---

## 📂 Estrutura do Projeto

### Documentação da Feature
```text
specs/008-oficina-loja-upgrades/
├── spec.md              # Especificação de requisitos e clarificações da Feature 008
├── plan.md              # Este plano de implementação
├── research.md          # Decisões de arquitetura e calibração econômica
├── data-model.md        # Modelos de dados, structs e diagramas de classes
├── quickstart.md        # Cenários executáveis de teste de integração
├── contracts/           # Contratos de interface C#
│   ├── IComprarMelhoriaCasoDeUso.cs
│   ├── IConsultarOficinaCasoDeUso.cs
│   ├── ItemOficinaDTO.cs
│   └── ResultadoCompraMelhoria.cs
└── tasks.md             # Tarefas de implementação (gerado pelo /speckit-tasks)
```

### Código-Fonte da Solução
```text
src/
├── AeroAscent.Core.Dominio/
│   └── ObjetosDeValor/
│       └── ResultadoCompraMelhoria.cs (novo extrato na stack)
│
├── AeroAscent.Core.Aplicacao/
│   ├── Contratos/
│   │   ├── IComprarMelhoriaCasoDeUso.cs
│   │   └── IConsultarOficinaCasoDeUso.cs
│   ├── DTOs/
│   │   └── ItemOficinaDTO.cs
│   └── CasosDeUso/
│       ├── ComprarMelhoriaCasoDeUso.cs
│       └── ConsultarOficinaCasoDeUso.cs
│
tests/
└── AeroAscent.Core.Aplicacao.Testes/
    └── CasosDeUso/
        ├── ComprarMelhoriaCasoDeUsoTestes.cs
        └── ConsultarOficinaCasoDeUsoTestes.cs
```

---

## 🗓️ Fases de Execução do Plano

### Fase 0: Pesquisa e Decisões de Engenharia (`research.md`)
- [x] Calibrar custos base da oficina com a entidade existente `Oficina.cs` (D1).
- [x] Fixar teto máximo de nível em 10 (`NIVEL_MAXIMO = 10`) (D2).
- [x] Segregar casos de uso de compra e consulta (CQRS leve) (D3).
- [x] Modelar DTOs e extrato de compra na stack (`readonly record struct`) (D4).
- [x] Definir representação de itens no nível máximo no catálogo (D5).
- [x] Estabelecer resiliência automática na primeira execução (D6).

### Fase 1: Design e Contratos de Domínio/Aplicação
- [x] Criar `contracts/IComprarMelhoriaCasoDeUso.cs`.
- [x] Criar `contracts/IConsultarOficinaCasoDeUso.cs`.
- [x] Criar `contracts/ResultadoCompraMelhoria.cs`.
- [x] Criar `contracts/ItemOficinaDTO.cs`.
- [x] Documentar modelo de dados e tabela de custos em `data-model.md`.
- [x] Criar cenários de teste em `quickstart.md`.
- [x] Finalizar plano arquitetural em `plan.md`.

### Fase 2: Implementação e Tarefas (`tasks.md`)
- [x] Gerar tarefas de implementação através do `/speckit-tasks 008-oficina-loja-upgrades`.
