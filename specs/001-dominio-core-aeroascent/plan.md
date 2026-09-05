# Implementation Plan: Domínio Core, Entidades e Objetos de Valor do AeroAscent

**Branch**: `001-dominio-core-aeroascent` | **Date**: 2026-09-04 | **Spec**: [spec.md](file:///h:/tmp/RSA/Loterias/JogosMaster/GitHub/AeroAscent/specs/001-dominio-core-aeroascent/spec.md)

**Input**: Especificação da feature de Domínio Core em C# puro (.NET 8), contendo Entidades (`ProgressoJogador`, `Aeronave`, `Voo`, `Oficina`), Objetos de Valor (`Combustivel`, `Moeda`, `VetorVoo`, `Melhoria`, `ResultadoVoo`), Enums, Exceções de Domínio e Contratos Base (`IRepositorioProgresso`, `IServicoFisicaVoo`, `IServicoEconomia`).

---

## Summary

Esta feature estabelece a espinha dorsal de engenharia e regras de negócio do **AeroAscent**. O objetivo é implementar a camada de Domínio (`Core/Dominio`) como uma biblioteca de classes C# pura (.NET Standard 2.1 / .NET 8), totalmente desacoplada da Unity Engine e de frameworks visuais. A modelagem adota Domain-Driven Design (DDD) estrito, imutabilidade garantida para cálculos e economia, alocação zero no loop de execução com `VetorVoo` (`readonly record struct`), e 100% de cobertura de testes unitários automatizados via xUnit.

---

## Technical Context

**Language/Version**: C# 12 / .NET Standard 2.1 & .NET 8 (`netstandard2.1;net8.0`)  
**Primary Dependencies**: Zero dependências de pacotes externos na biblioteca de Domínio (somente BCL do .NET Standard 2.1 / .NET 8). No projeto de testes: `xunit` (v2.x) e `Microsoft.NET.Test.Sdk`.  
**Storage**: N/A na camada de domínio puro. A persistência é desacoplada através do contrato `IRepositorioProgresso` (implementada posteriormente pela camada de Infraestrutura com JSON via `FileSystem.AppDataDirectory`).  
**Testing**: xUnit 2.x executado via CLI `dotnet test`.  
**Target Platform**: Multiplataforma (Windows e Android via Unity Engine / C# puro em .NET Standard 2.1 e .NET 8).  
**Project Type**: Biblioteca de Classes C# (.NET Class Library) + Projeto de Testes xUnit.  
**Performance Goals**: Tempo de execução da suíte de testes < 500 ms (SC-002), 60 FPS garantidos para a simulação física na Unity (Windows/Android), e **`GC Alloc = 0 bytes`** para operações cinemáticas com `VetorVoo`.  
**Constraints**: Clean Architecture estrita (zero acoplamento com `UnityEngine` ou `MonoBehaviour` no domínio), 100% dos identificadores, comentários e documentações XML em Português Brasileiro (pt-BR), entidades modeladas como `class` com `Guid Id`, objetos de valor como `record`/`readonly record struct`.  
**Scale/Scope**: 4 Entidades (`ProgressoJogador`, `Aeronave`, `Voo`, `Oficina`), 5 Objetos de Valor (`Moeda`, `Combustivel`, `VetorVoo`, `Melhoria`, `ResultadoVoo`), 2 Enums (`StatusVoo`, `TipoMelhoria`), 3 Exceções de Domínio, 3 Interfaces de Contrato.

---

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Artigo da Constituição | Requisito / Princípio | Status de Conformidade | Evidência / Decisão de Projeto |
|---|---|---|---|
| **Artigo I** | Ética Familiar e Zero Anúncios | **APROVADO** | O domínio não contém qualquer dependência, serviço ou gatilho para monetização predatória, telemetria infantil ou anúncios. |
| **Artigo II** | Gameplay Justo e Física | **APROVADO** | Progressão baseada unicamente em habilidade motora e aerodinâmica; fórmulas de cálculo de recompensa e custos encapsuladas no domínio. |
| **Artigo III.1** | Nomenclatura e Idioma (pt-BR) | **APROVADO** | 100% das classes, métodos, interfaces, parâmetros, enums, exceções e documentações XML escritas em Português Brasileiro. |
| **Artigo III.2** | Clean Architecture e Domínio Desacoplado | **APROVADO** | O projeto `AeroAscent.Core.Dominio` compila em C# puro com zero referências a `UnityEngine`, `MonoBehaviour` ou pacotes externos. |
| **Artigo III.3** | SOLID e DDD | **APROVADO** | Entidades usam `class` com `Guid Id`; Objetos de Valor usam `record` imutável; injeção de dependências realizada estritamente via interfaces (`IRepositorio...`, `IServico...`). |
| **Artigo III.4** | Performance e Alocação Zero | **APROVADO** | `VetorVoo` implementado como `readonly record struct` alocado na stack, impedindo pressão no Garbage Collector durante o loop de voo. |
| **Artigo V** | Checklist de Governança | **APROVADO** | Todos os 8 itens do checklist de governança foram avaliados e atendidos no desenho técnico da feature. |

---

## Project Structure

### Documentation (this feature)

```text
specs/001-dominio-core-aeroascent/
├── spec.md              # Especificação refinada e clarificada da feature
├── plan.md              # Este arquivo de plano de implementação técnica
├── research.md          # Fase 0: Decisões técnicas de linguagem, alocação e frameworks
├── data-model.md        # Fase 1: Diagrama e modelo de entidades, objetos de valor e invariantes
├── quickstart.md        # Fase 1: Guia prático de comandos de compilação e validação de testes
└── contracts/           # Fase 1: Interfaces C# de contratos de domínio
    ├── IRepositorioProgresso.cs
    ├── IServicoFisicaVoo.cs
    └── IServicoEconomia.cs
```

### Source Code (repository root)

```text
src/
└── AeroAscent.Core.Dominio/
    ├── AeroAscent.Core.Dominio.csproj
    ├── Contratos/
    │   ├── IRepositorioProgresso.cs
    │   ├── IServicoFisicaVoo.cs
    │   └── IServicoEconomia.cs
    ├── Entidades/
    │   ├── ProgressoJogador.cs
    │   ├── Aeronave.cs
    │   ├── Voo.cs
    │   └── Oficina.cs
    ├── ObjetosDeValor/
    │   ├── Moeda.cs
    │   ├── Combustivel.cs
    │   ├── VetorVoo.cs
    │   ├── Melhoria.cs
    │   └── ResultadoVoo.cs
    ├── Enums/
    │   ├── StatusVoo.cs
    │   └── TipoMelhoria.cs
    └── Excecoes/
        ├── SaldoInsuficienteException.cs
        ├── MelhoriaNivelMaximoException.cs
        └── DominioInvalidoException.cs

tests/
└── AeroAscent.Core.Dominio.Testes/
    ├── AeroAscent.Core.Dominio.Testes.csproj
    ├── Entidades/
    │   ├── ProgressoJogadorTestes.cs
    │   ├── AeronaveTestes.cs
    │   ├── VooTestes.cs
    │   └── OficinaTestes.cs
    └── ObjetosDeValor/
        ├── MoedaTestes.cs
        ├── CombustivelTestes.cs
        ├── VetorVooTestes.cs
        ├── MelhoriaTestes.cs
        └── ResultadoVooTestes.cs
```

**Structure Decision**: Solução C# com separação física entre a biblioteca de classes do domínio (`src/AeroAscent.Core.Dominio`) e a suíte de testes unitários isolados (`tests/AeroAscent.Core.Dominio.Testes`), garantindo que o domínio não tenha contaminação com dependências externas e possa ser referenciado diretamente pelo projeto Unity (Windows e Android) e camada de Aplicação.

---

## Complexity Tracking

> **Fill ONLY if Constitution Check has violations that must be justified**

*(Nenhuma violação identificada. O desenho técnico está em total conformidade com todos os artigos da Constituição).*

| Violação | Justificativa | Alternativa Mais Simples Rejeitada Porque |
|---|---|---|
| *Nenhuma* | *N/A* | *N/A* |
