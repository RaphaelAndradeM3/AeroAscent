# Plano de Implementação: Feature 006 — Detecção de Pouso e Transição de Fim de Voo

**Branch**: `006-deteccao-pouso-fim-voo` | **Data**: 2026-09-05 | **Spec**: [spec.md](./spec.md)  
**Artefatos Relacionados**: [research.md](./research.md) | [data-model.md](./data-model.md) | [quickstart.md](./quickstart.md) | [contracts/](./contracts/)

---

## 📋 Resumo da Funcionalidade
Implementar o subsistema de contato com o solo e desaceleração por atrito terrestre contínuo ($a_{\text{atrito}} = \mu \cdot g$), detecção de parada no limiar canônico de $0.15\text{ m/s}$ com congelamento cinemático absoluto e nivelamento suave da fuselagem para $0^\circ$, orquestração atômica de transição de status para `StatusVoo.Pousado`, consolidação de métricas finais e notificação de evento de voo concluído via `IPublicadorEventosVoo`, garantindo estritamente zero alocação de memória no heap (`GC Alloc = 0 bytes`) em C# puro (.NET Standard 2.1 e .NET 8.0) sem qualquer dependência de Unity nas camadas de Domínio e Aplicação.

---

## 💻 Contexto Técnico

- **Linguagem / Versão**: C# 12 (.NET 8.0 e .NET Standard 2.1 para retrocompatibilidade com Unity IL2CPP).
- **Dependências Principais**:
  - `AeroAscent.Core.Dominio`: C# puro, sem dependências externas.
  - `AeroAscent.Core.Aplicacao`: Depende exclusivamente do Core.Dominio.
  - Testes: xUnit 2.8+, FluentAssertions (na camada de aplicação), runner .NET 8.0.
- **Armazenamento**: N/A no Core (persistência executada em serviços de infraestrutura/apresentação).
- **Testes**: xUnit com asserções estritas de memória (`GC.GetAllocatedBytesForCurrentThread() == 0`) e precisão física.
- **Plataforma Alvo**: Unity Multiplataforma (Windows Desktop DirectX/Vulkan e Android Vulkan/OpenGL ES 3.2).
- **Metas de Performance**:
  - `SC-001`: Transição de parada sem anomalias em 100% dos testes.
  - `SC-002`: Disparo do evento de conclusão em $< 10\text{ms}$.
  - `SC-003`: `GC Alloc = 0 bytes` durante todo o deslizamento no solo e parada.

---

## 📜 Constitution Check

*GATE: Validação pré e pós-design com base nos princípios da Constituição (v1.2.0).*

| Artigo Constitucional | Diretriz | Status | Justificativa / Validação |
|---|---|---|---|
| **Artigo I** | Ética Familiar e Sem Frustrações | ✅ Aprovado | Pouso suave, atrito transparente, sem mortes punitivas ou bugs visuais de colisão. |
| **Artigo II** | Física Justa e Sem Trilhos Ocultos | ✅ Aprovado | Desaceleração física real $\mu \cdot g$, conservação de energia e limiar canônico de $0.15\text{ m/s}$. |
| **Artigo III.1** | Idioma 100% em pt-BR | ✅ Aprovado | Todos os identificadores, comentários e documentação XML (`///`) em Português Brasileiro. |
| **Artigo III.2** | Clean Architecture e Domínio Puro | ✅ Aprovado | Domínio e Aplicação isolados sem referências à Unity Engine. |
| **Artigo III.4** | Performance Mobile First (`0 bytes GC`) | ✅ Aprovado | Structs imutáveis na stack (`readonly record struct`) e loop contínuo sem alocação no heap. |

---

## 📂 Estrutura do Projeto

### Documentação da Feature
```text
specs/006-deteccao-pouso-fim-voo/
├── spec.md              # Especificação de requisitos e clarificações da Feature 006
├── plan.md              # Este plano de implementação
├── research.md          # Decisões de física de atrito e orquestração de encerramento
├── data-model.md        # Modelos de dados, structs na stack e diagramas
├── quickstart.md        # Cenários executáveis de teste automatizado ponta a ponta
├── contracts/           # Contratos de interface C#
│   ├── IPublicadorEventosVoo.cs
│   └── IProcessarPousoFimVooCasoDeUso.cs
└── tasks.md             # Tarefas de implementação (gerado pelo /speckit-tasks)
```

### Código-Fonte da Solução
```text
src/
├── AeroAscent.Core.Dominio/
│   ├── Contratos/
│   │   └── IPublicadorEventosVoo.cs
│   ├── ObjetosDeValor/
│   │   ├── ParametrosPouso.cs
│   │   └── ResultadoFimVoo.cs
│   └── Servicos/
│       └── ServicoFisicaVoo.cs (atualização do limiar para 0.15m/s e nivelamento de pitch)
│
├── AeroAscent.Core.Aplicacao/
│   ├── Contratos/
│   │   └── IProcessarPousoFimVooCasoDeUso.cs
│   └── CasosDeUso/
│       └── ProcessarPousoFimVooCasoDeUso.cs
│
tests/
├── AeroAscent.Core.Dominio.Testes/
│   ├── ObjetosDeValor/
│   │   ├── ParametrosPousoTestes.cs
│   │   └── ResultadoFimVooTestes.cs
│   └── Servicos/
│       └── ServicoFisicaVooTestes.cs (testes de solo, atrito, limiar 0.15m/s e nivelamento)
│
└── AeroAscent.Core.Aplicacao.Testes/
    └── CasosDeUso/
        └── ProcessarPousoFimVooCasoDeUsoTestes.cs (integração de fim de voo e eventos)
```

---

## 🗓️ Fases de Execução do Plano

### Fase 0: Pesquisa e Decisões de Engenharia (`research.md`)
- [x] Resolver limiar canônico de parada ($0.15\text{ m/s}$).
- [x] Definir modelo de resposta de impacto vertical e nivelamento de pitch ($15^\circ/\text{s}$).
- [x] Definir orquestração desacoplada de Clean Architecture (`ProcessarPousoFimVooCasoDeUso`).
- [x] Definir mecanismo de notificação de eventos (`IPublicadorEventosVoo`).

### Fase 1: Design e Contratos de Interface (`data-model.md`, `contracts/`, `quickstart.md`)
- [x] Modelar structs na stack `ParametrosPouso` e `ResultadoFimVoo`.
- [x] Criar contratos `IPublicadorEventosVoo` e `IProcessarPousoFimVooCasoDeUso`.
- [x] Elaborar guia rápido de validação (`quickstart.md`).

### Fase 2: Geração de Tarefas de Implementação (`tasks.md`)
- [ ] Executar `/speckit-tasks 006-deteccao-pouso-fim-voo` para detalhar as tarefas estruturadas por fases (Setup, Foundational, US1, US2 e Polish/Benchmarks).
