# Implementation Plan: Interface do Menu Principal, Hangar 3D e Oficina (Feature 010)

**Branch**: `010-ui-menu-principal-oficina` | **Date**: 2026-09-05 | **Spec**: [specs/010-ui-menu-principal-oficina/spec.md](file:///h:/tmp/RSA/Loterias/JogosMaster/GitHub/AeroAscent/specs/010-ui-menu-principal-oficina/spec.md)

**Input**: Feature specification from `/specs/010-ui-menu-principal-oficina/spec.md`

---

## Summary

Implementação da camada de apresentação da Oficina e Menu Principal com visualização da aeronave no Hangar 3D, saldo de moedas, 4 cartões de evolução mecânica (Motor, Aerodinâmica, Tanque e Catapulta) e botão de decolagem. A arquitetura segue estritamente o padrão **Model-View-Presenter (MVP)** com visão passiva: o `ApresentadorOficina` puro em C# (.NET Standard 2.1) orquestra os casos de uso de consulta e compra, formata dados no padrão pt-BR (`💰 1.250`), bloqueia concorrência de cliques (*spam click*) e dispara o evento desacoplado `AoSolicitarDecolagem` para a transição suave de câmera da Unity Engine.

---

## Technical Context

**Language/Version**: C# 12 / .NET Standard 2.1 (compatibilidade com Unity) e .NET 8 (executores e testes)  
**Primary Dependencies**: `AeroAscent.Core.Dominio`, `AeroAscent.Core.Aplicacao`  
**Storage**: N/A direto na UI (orquestrado via `IConsultarOficinaCasoDeUso` e `IComprarMelhoriaCasoDeUso` respaldados pela infraestrutura JSON)  
**Testing**: xUnit no .NET 8 (`tests/AeroAscent.Core.Aplicacao.Testes`)  
**Target Platform**: Unity Engine Multiplataforma (Windows Standalone 64-bit e Android APK/AAB)  
**Project Type**: Jogo Arcade Multiplataforma (Apresentação UI / Canvas + Apresentadores C# Puros)  
**Performance Goals**: Tempo de renderização inicial $< 200\text{ms}$ (SC-001), 60 FPS estáveis sem engasgos (SC-002), `GC Alloc = 0 bytes` no loop de renderização  
**Constraints**: 100% em Português Brasileiro (pt-BR), zero dependência de `UnityEngine` no Presenter, bloqueio atômico de *spam click*  
**Scale/Scope**: 1 tela principal da oficina integrada ao Hangar 3D com 4 cartões de evolução mecânica  

---

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Artigo da Constituição (v1.2.0) | Status | Avaliação / Justificativa |
|---|---|---|
| **Artigo I — Ética e Zero Anúncios** | **APROVADO** | Interface limpa, sem anúncios forçados, pop-ups predatórios ou moedas pagas com dinheiro real. |
| **Artigo II — Gameplay Justo e Habilidade** | **APROVADO** | Todo o saldo de moedas exibido e gasto provém unicamente do desempenho nos voos anteriores. |
| **Artigo III.1 — Nomenclatura e pt-BR** | **APROVADO** | 100% em Português Brasileiro (`ApresentadorOficina`, `IVisaoOficina`, `ItemCartaoOficinaDTO`, `ModeloVisualOficina`). |
| **Artigo III.2 — Clean Architecture** | **APROVADO** | `ApresentadorOficina` puro em C# (.NET Standard 2.1), sem acoplamento a `UnityEngine` ou `MonoBehaviour`. Visão passiva implementada via interface. |
| **Artigo III.4 — Performance Mobile First** | **APROVADO** | DTOs de apresentação concebidos como `readonly record struct` na stack (`GC Alloc = 0 bytes`). Operações de bind visual instantâneas. |
| **Artigo V — Governança e Testes** | **APROVADO** | Métodos assíncronos com sufixo `Async` e `CancellationToken`. Cobertura total de testes unitários em xUnit. |

---

## Project Structure

### Documentation (this feature)

```text
specs/010-ui-menu-principal-oficina/
├── spec.md              # Especificação refinada com 5 clarificações aceitas
├── plan.md              # Este plano de implementação
├── research.md          # Fase 0: Pesquisa e decisões arquiteturais
├── data-model.md        # Fase 1: Modelo de dados de apresentação
├── quickstart.md        # Fase 1: Guia de execução e cenários de teste
└── contracts/           # Fase 1: Contratos de interface C#
    ├── IApresentadorOficina.cs
    ├── IVisaoOficina.cs
    ├── ItemCartaoOficinaDTO.cs
    └── ModeloVisualOficina.cs
```

### Source Code (repository root)

```text
src/
└── AeroAscent.Core.Aplicacao/
    ├── Contratos/
    │   ├── IApresentadorOficina.cs         # [NOVO] Contrato do Presenter
    │   └── IVisaoOficina.cs                # [NOVO] Contrato da Visão Passiva
    ├── DTOs/
    │   ├── ItemCartaoOficinaDTO.cs         # [NOVO] Dados pré-formatados do cartão
    │   └── ModeloVisualOficina.cs          # [NOVO] Estado visual atômico da tela
    └── Apresentadores/
        └── ApresentadorOficina.cs          # [NOVO] Implementação MVP pura em C#

tests/
└── AeroAscent.Core.Aplicacao.Testes/
    ├── Apresentadores/
    │   └── ApresentadorOficinaTestes.cs    # [NOVO] Testes unitários do Presenter
    └── DTOs/
        └── ItemCartaoOficinaDTOTestes.cs   # [NOVO] Testes de formatação e DTOs
```

---

## Complexity Tracking

> **Nenhuma violação da constituição detectada.** O padrão MVP adotado respeita rigorosamente a separação entre C# puro e a Unity Engine.
