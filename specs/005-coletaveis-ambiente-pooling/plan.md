# Implementation Plan: Feature 005 — Sistema de Coletáveis em Voo e Object Pooling

**Branch**: `005-coletaveis-ambiente-pooling` | **Date**: 2026-09-05 | **Spec**: [specs/005-coletaveis-ambiente-pooling/spec.md](spec.md)

---

## Summary

Implementação do sistema dinâmico de coletáveis aéreos (**Moedas Flutuantes** e **Anéis de Vento / Air Boost Rings**) e da infraestrutura de **Object Pooling** de alta performance na camada de Domínio e Aplicação em C# puro (.NET Standard 2.1 e .NET 8), garantindo zero alocação de memória no heap (`GC Alloc = 0 bytes`) durante o loop contínuo de simulação física, spawn procedural em janela ativa à frente da aeronave e reciclagem automática de elementos que ficarem para trás.

---

## Technical Context

**Language/Version**: C# 12 / .NET 8.0 e .NET Standard 2.1  
**Primary Dependencies**: Zero dependências externas no Core (`AeroAscent.Core.Dominio` e `AeroAscent.Core.Aplicacao` desacoplados de `UnityEngine` e `MonoBehaviour`).  
**Storage**: N/A (dados de moedas da rodada consolidados na entidade `Voo`).  
**Testing**: xUnit com asserções de memória estrita via `GC.GetAllocatedBytesForCurrentThread()`.  
**Target Platform**: Multiplataforma nativa — Android (mobile) e Windows (desktop) via Unity IL2CPP.  
**Project Type**: Jogo arcade de aviação e progressão (Clean Architecture).  
**Performance Goals**: 60 FPS cravados em mobile e desktop; detecção de colisão em $< 0.1\text{ms}$; `GC Alloc = 0 bytes` no loop contínuo de voo e reciclagem.  
**Constraints**: 100% em Português Brasileiro (pt-BR); nenhum framework externo no Domínio; física justa baseada em vetores no plano $Y-Z$.  
**Scale/Scope**: Janela ativa de 50 moedas e 15 anéis de vento simultâneos na memória com reciclagem contínua a cada $20\text{m}$ ultrapassados.

---

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

- [x] **Artigo I — Experiência Familiar, Ética e Zero Anúncios**: Coletáveis premiam habilidade e curiosidade sem qualquer monetização predatória, pop-ups ou anúncios.
- [x] **Artigo II — Gameplay Justo e Física como Pilar**: Colisões calculadas de forma determinística por distância euclidiana simples; anéis de vento impulsionam vetorialmente sem trilhos invisíveis.
- [x] **Artigo III.1 — Nomenclatura e Idioma (pt-BR)**: Todos os identificadores (`Coletavel`, `GerenciadorPoolObjetos`, `TipoColetavel`), documentações XML (`///`) e testes em Português Brasileiro.
- [x] **Artigo III.2 — Clean Architecture**: Camadas `Core.Dominio` e `Core.Aplicacao` 100% puras em C#, sem referências a `UnityEngine`.
- [x] **Artigo III.4 — Performance Mobile First e Gestão de Memória**: `GC Alloc = 0 bytes` no loop de voo, pooling obrigatório para todos os coletáveis e meta de 60 FPS estáveis.

---

## Project Structure

### Documentation (this feature)

```text
specs/005-coletaveis-ambiente-pooling/
├── plan.md              # Este plano de implementação arquitetural
├── research.md          # Decisões técnicas e alternativas avaliadas
├── data-model.md        # Diagrama de classes e modelagem de entidades e tipos
├── quickstart.md        # Guia rápido de execução de testes e cenários
└── contracts/           # Contratos públicos de interface
    ├── IPoolObjetos.cs
    ├── IServicoGeracaoProceduralColetaveis.cs
    └── IProcessarColetaveisVooCasoDeUso.cs
```

### Source Code (repository root)

```text
src/
├── AeroAscent.Core.Dominio/
│   ├── Comum/
│   │   ├── IPoolObjetos.cs
│   │   └── GerenciadorPoolObjetos.cs
│   ├── Contratos/
│   │   └── IServicoGeracaoProceduralColetaveis.cs
│   ├── Entidades/
│   │   └── Coletavel.cs
│   ├── Enums/
│   │   └── TipoColetavel.cs
│   ├── ObjetosDeValor/
│   │   └── ResultadoProcessamentoColetaveis.cs
│   └── Servicos/
│       └── ServicoGeracaoProceduralColetaveis.cs
│
└── AeroAscent.Core.Aplicacao/
    ├── Contratos/
    │   └── IProcessarColetaveisVooCasoDeUso.cs
    └── CasosDeUso/
        └── ProcessarColetaveisVooCasoDeUso.cs

tests/
├── AeroAscent.Core.Dominio.Testes/
│   ├── Comum/
│   │   └── GerenciadorPoolObjetosTestes.cs
│   ├── Entidades/
│   │   └── ColetavelTestes.cs
│   └── Servicos/
│       └── ServicoGeracaoProceduralColetaveisTestes.cs
│
└── AeroAscent.Core.Aplicacao.Testes/
    └── CasosDeUso/
        └── ProcessarColetaveisVooCasoDeUsoTestes.cs
```

---

## Phases & Milestones

### Phase 0: Outline & Research
- Consolidação dos requisitos e decisões no `research.md`. (Concluído)

### Phase 1: Design & Contracts
- Modelagem no `data-model.md`, interfaces em `contracts/` e guia de validação no `quickstart.md`. (Concluído)

### Phase 2: Setup (Shared Infrastructure)
- Criação dos testes unitários de infraestrutura e fixtures de teste de pooling (`GerenciadorPoolObjetosTestes.cs`).

### Phase 3: User Story 1 (Coleta de Moedas Flutuantes - 🎯 MVP)
- Entidade `Coletavel`, enum `TipoColetavel`, colisão por raio de $1.5\text{m}$ e incremento no saldo de `Voo`.

### Phase 4: User Story 2 (Atravessar Anéis de Vento / Air Boost Rings)
- Raio de colisão de $3.5\text{m}$, injeção de impulso de $+10.0\text{ m/s}$ no vetor velocidade da aeronave sem consumo de combustível.

### Phase 5: User Story 3 (Object Pooling, Geração Procedural e Zero GC)
- Implementação de `ServicoGeracaoProceduralColetaveis`, orquestração no `ProcessarColetaveisVooCasoDeUso`, reciclagem em $Z < Z_{\text{aeronave}} - 20\text{ m}$ e teste de benchmark com `GC Alloc = 0 bytes`.

---

## Complexity Tracking

*Nenhuma violação constitucional detectada. O design respeita a Clean Architecture, zero alocação no heap e convenções C# .NET.*
