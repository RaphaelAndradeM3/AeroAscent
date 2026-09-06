# Implementation Plan: Interface HUD de Voo e Controles de Toque Mobile (Feature 011)

**Branch**: `011-ui-hud-voo` | **Date**: 2026-09-05 | **Spec**: [specs/011-ui-hud-voo/spec.md](file:///h:/tmp/RSA/Loterias/JogosMaster/GitHub/AeroAscent/specs/011-ui-hud-voo/spec.md)

**Input**: Feature specification from `/specs/011-ui-hud-voo/spec.md`

---

## Summary

Implementação da camada de apresentação da Interface HUD de Voo e Controles de Toque Mobile com exibição de telemetria em tempo real (distância percorrida com destaque dourado de novo recorde, altímetro, velocímetro, contador de moedas e barra vertical de combustível), controles táteis ergonômicos em modo paisagem (inclinação subir/descer na esquerda e boost na direita), mapeamento simultâneo para teclas de PC (Setas/W-S e Espaço), suporte a pausa e ocultação imediata de controles ao pousar/colidir. A arquitetura segue estritamente o padrão **Model-View-Presenter (MVP)** com visão passiva: o `ApresentadorHUDVoo` puro em C# (.NET Standard 2.1) orquestra os dados de voo sem dependência de `UnityEngine`, transmitindo `TelemetriaHUDDTO` na stack com zero alocação de heap (`GC Alloc = 0 bytes`).

---

## Technical Context

**Language/Version**: C# 12 / .NET Standard 2.1 (compatibilidade com Unity) e .NET 8 (executores e testes)  
**Primary Dependencies**: `AeroAscent.Core.Dominio`, `AeroAscent.Core.Aplicacao`  
**Storage**: N/A direto no HUD (alimentado pelo fluxo de `Voo` e `EstadoFisicoAeronave`)  
**Testing**: xUnit no .NET 8 (`tests/AeroAscent.Core.Aplicacao.Testes`)  
**Target Platform**: Unity Engine Multiplataforma (Windows Standalone 64-bit e Android APK/AAB)  
**Project Type**: Jogo Arcade Multiplataforma (Apresentação UI / Canvas + Apresentadores C# Puros)  
**Performance Goals**: `GC Alloc = 0 bytes` no loop contínuo de telemetria (SC-001), latência de resposta ao toque $< 16\text{ms}$ (SC-002), 60 FPS estáveis tanto em mobile Android quanto desktop Windows  
**Constraints**: 100% em Português Brasileiro (pt-BR), zero dependência de `UnityEngine` no Presenter, suporte a telas 16:9 a 21:9 em modo paisagem  
**Scale/Scope**: 1 tela HUD de voo sobreposta ao espaço de jogo com painéis de telemetria, barra de combustível, 3 botões táteis e botão de pausa  

---

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Artigo da Constituição (v1.2.0) | Status | Avaliação / Justificativa |
|---|---|---|
| **Artigo I — Ética e Zero Anúncios** | **APROVADO** | HUD limpo, focado exclusivamente no feedback de telemetria da partida sem distrações ou anúncios. |
| **Artigo II — Gameplay Justo e Habilidade** | **APROVADO** | Feedback transparente de combustível, altitude, velocidade e controles de pitch manuais para pilotagem baseada em física. |
| **Artigo III.1 — Nomenclatura e pt-BR** | **APROVADO** | 100% em Português Brasileiro (`ApresentadorHUDVoo`, `IVisaoHUDVoo`, `TelemetriaHUDDTO`, etc.). |
| **Artigo III.2 — Clean Architecture** | **APROVADO** | `ApresentadorHUDVoo` puro em C# (.NET Standard 2.1), sem acoplamento a `UnityEngine` ou `MonoBehaviour`. Visão passiva implementada via interface. |
| **Artigo III.4 — Performance Mobile First** | **APROVADO** | `TelemetriaHUDDTO` concebido como `readonly record struct` na stack (`GC Alloc = 0 bytes`). Visão passiva com cache de valores inteiros para evitar geração de lixo em strings. |
| **Artigo V — Governança e Testes** | **APROVADO** | Cobertura total de cenários e invariantes de comandos/telemetria via testes unitários em xUnit. |

---

## Project Structure

### Documentation (this feature)

```text
specs/011-ui-hud-voo/
├── spec.md              # Especificação refinada com 5 clarificações aceitas
├── plan.md              # Este plano de implementação
├── research.md          # Fase 0: Pesquisa e decisões arquiteturais
├── data-model.md        # Fase 1: Modelo de dados de apresentação
├── quickstart.md        # Fase 1: Guia de execução e cenários de teste
└── contracts/           # Fase 1: Contratos de interface C#
    ├── IApresentadorHUDVoo.cs
    └── IVisaoHUDVoo.cs
```

### Source Code (repository root)

```text
src/
└── AeroAscent.Core.Aplicacao/
    ├── Contratos/
    │   ├── IApresentadorHUDVoo.cs          # [NOVO] Contrato do Presenter do HUD
    │   └── IVisaoHUDVoo.cs                 # [NOVO] Contrato da Visão Passiva do HUD
    ├── DTOs/
    │   └── TelemetriaHUDDTO.cs             # [NOVO] DTO readonly record struct na stack
    └── Apresentadores/
        └── ApresentadorHUDVoo.cs           # [NOVO] Implementação MVP pura em C#

tests/
└── AeroAscent.Core.Aplicacao.Testes/
    ├── Fixtures/
    │   └── VisaoHUDVooFalsa.cs             # [NOVO] Mock/Spy para testes unitários
    └── Apresentadores/
        └── ApresentadorHUDVooTestes.cs     # [NOVO] Suíte de testes unitários xUnit
```

---

## Complexity Tracking

> **Nenhuma violação da constituição detectada.** O padrão MVP adotado respeita rigorosamente a separação entre C# puro e a Unity Engine.
