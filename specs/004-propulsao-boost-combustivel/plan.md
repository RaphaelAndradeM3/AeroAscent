# Implementation Plan: Sistema de Propulsão (Boost) e Queima de Combustível

**Branch**: `004-propulsao-boost-combustivel` | **Date**: 2026-09-05 | **Spec**: [spec.md](file:///h:/tmp/RSA/Loterias/JogosMaster/GitHub/AeroAscent/specs/004-propulsao-boost-combustivel/spec.md)

**Input**: Feature specification from `/specs/004-propulsao-boost-combustivel/spec.md`

---

## Summary

Implementação do sistema de propulsão ativa (*boost*) da aeronave e gerenciamento contínuo do reservatório de combustível em voo, projetado em C# puro (.NET Standard 2.1 / .NET 8) sob os princípios de Clean Architecture e DDD. A força de empuxo ($T = 120.0\text{ N} \times [1 + (\text{NivelMotor}-1) \times 0.30]$) é decomposta trigonometricamente no plano vertical de voo (Y-Z) conforme a atitude longitudinal do nariz ($T_y = T \sin(\theta), T_z = T \cos(\theta)$), impulsionando a aeronave com aceleração líquida positiva. O consumo ocorre na taxa contínua de $5.0\text{ un/s}$, com suporte a queima fracionária precisa no passo de esgotamento ($\Delta t_{\text{queima}} = \text{CombustivelRestante} / \text{TaxaConsumo}$), garantindo erro temporal $< 1\text{ms}$ (SC-001) e corte automático instantâneo sem resíduos negativos. Todo o fluxo opera na stack via `readonly record struct` (`EstadoPropulsor`, `ParametrosControlePiloto`), assegurando estritamente zero alocação no heap (`GC Alloc = 0 bytes` / SC-002 e Artigo III.4).

---

## Technical Context

**Language/Version**: C# 12 / .NET Standard 2.1 e .NET 8.0 (multi-target nativo)  
**Primary Dependencies**: BCL padrão do C# (`System`, `System.MathF`). Zero dependências de bibliotecas externas ou de `UnityEngine` no Domínio e Aplicação.  
**Storage**: N/A para esta feature (dados de combustível e telemetria são calculados na stack e acumulados na entidade em memória `Voo`).  
**Testing**: xUnit com asserções de física e conservação, validação temporal com margem $< 1\text{ms}$ e garantia de `GC.GetAllocatedBytesForCurrentThread() == 0` para zero alocação de memória no heap.  
**Target Platform**: Multiplataforma nativa de alta performance (Windows Standalone 64-bit e Android via Unity Engine com compilação IL2CPP).  
**Project Type**: Bibliotecas de Classes de Domínio e Aplicação (.NET Standard 2.1 / .NET 8) para Game Engine.  
**Performance Goals**: Tempo de execução de 1 passo de simulação com propulsão $< 0.05\text{ms}$ para garantia estável de 60 FPS contínuos (16.6ms por frame).  
**Constraints**: Alocação zero de memória no heap (`GC Alloc = 0 bytes` / SC-002), tipos por valor `readonly record struct` na stack, precisão de corte de combustível $< 1\text{ms}$ (SC-001), 100% em Português Brasileiro (pt-BR) com documentação XML rigorosa.  
**Scale/Scope**: Novo Value Object `EstadoPropulsor`, enriquecimento de `ParametrosControlePiloto`, `Combustivel` e `EstadoFisicoAeronave`, enriquecimento de `Voo` e `ServicoFisicaVoo`, atualização de `AtualizarFisicaVooCasoDeUso` e suíte completa de testes unitários e de integração.

---

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

- [x] **Artigo I — Experiência Familiar, Ética e Zero Anúncios**: O sistema de propulsão oferece aceleração tática e divertida para o jogador, sem barreiras predatórias, sem compras pay-to-win e sem publicidade intrusiva.
- [x] **Artigo II — Gameplay Justo e Física como Pilar**: A aceleração decorre estritamente da física vetorial e do gerenciamento consciente de combustível pelo jogador. O ganho de velocidade respeita a massa ($10\text{ kg}$) e a inclinação do nariz ($\theta$). Os upgrades de oficina geram benefícios proporcionais e autênticos.
- [x] **Artigo III.1 — Nomenclatura e Idioma (pt-BR Obrigatório)**: Todas as entidades, structs, interfaces, métodos, testes e documentações XML em Português Brasileiro estrito.
- [x] **Artigo III.2 — Clean Architecture e Domínio Desacoplado**: Domínio puro em `AeroAscent.Core.Dominio` (.NET Standard 2.1 / .NET 8) isolado de `UnityEngine` e `MonoBehaviour`. Casos de uso orquestrados em `AeroAscent.Core.Aplicacao`.
- [x] **Artigo III.3 — Princípios SOLID e DDD**: Entidade `Voo` com invariantes protegidas; Objetos de Valor (`EstadoPropulsor`, `Combustivel`, `ParametrosControlePiloto`, `EstadoFisicoAeronave`) modelados como tipos de valor imutáveis; Injeção de dependências exclusiva via interfaces (`IServicoFisicaVoo`, `IAtualizarFisicaVooCasoDeUso`).
- [x] **Artigo III.4 — Performance Mobile First e Gestão de Memória**: Alocação zero de memória no heap (`GC Alloc = 0 bytes`) durante todo o ciclo de propulsão e queima contínua de combustível.
- [x] **Artigo V — Checklist de Governança**: Todos os critérios constitucionais validados e aprovados no design técnico da feature.

---

## Project Structure

### Documentation (this feature)

```text
specs/004-propulsao-boost-combustivel/
├── plan.md              # Este plano de implementação (/speckit-plan)
├── research.md          # Pesquisa técnica, decisões físicas e calibração de constantes
├── data-model.md        # Modelo de dados, structs na stack e ciclo de estados
├── quickstart.md        # Guia rápido com cenários de validação e testes
├── contracts/           # Contratos de interface C#
│   ├── IAtualizarFisicaVooCasoDeUso.cs
│   └── IServicoFisicaVoo.cs
└── tasks.md             # Tarefas de implementação (geradas no /speckit-tasks)
```

### Source Code (repository root)

```text
src/
├── AeroAscent.Core.Dominio/
│   ├── Contratos/
│   │   └── IServicoFisicaVoo.cs                     # Contrato atualizado com sobrecarga de propulsão e empuxo
│   ├── Entidades/
│   │   └── Voo.cs                                   # ConsumirCombustivel fracionário e cálculo de capacidade escalonada
│   ├── ObjetosDeValor/
│   │   ├── Combustivel.cs                           # ConsumirFracionario com cálculo de fração residual dt
│   │   ├── EstadoFisicoAeronave.cs                  # Enriquecido com propriedade Propulsor (EstadoPropulsor)
│   │   ├── EstadoPropulsor.cs                       # [NEW] Struct de valor com telemetria instantânea do propulsor
│   │   └── ParametrosControlePiloto.cs              # Enriquecido com flag AcionarBoost
│   └── Servicos/
│       └── ServicoFisicaVoo.cs                      # Implementação da vetorização de empuxo e integração em SimularPasso
└── AeroAscent.Core.Aplicacao/
    ├── Contratos/
    │   └── IAtualizarFisicaVooCasoDeUso.cs          # Contrato de orquestração física e propulsão
    └── CasosDeUso/
        └── AtualizarFisicaVooCasoDeUso.cs           # Orquestração de queima, física e bloqueios (solo/catapulta)

tests/
├── AeroAscent.Core.Dominio.Testes/
│   ├── Entidades/
│   │   └── VooTestes.cs                             # Testes de consumo de combustível e capacidade
│   ├── ObjetosDeValor/
│   │   ├── CombustivelTestes.cs                     # Testes de consumo fracionário e esgotamento exato
│   │   ├── EstadoPropulsorTestes.cs                 # [NEW] Testes unitários do EstadoPropulsor
│   │   └── ParametrosControlePilotoTestes.cs        # Testes unitários com AcionarBoost
│   └── Servicos/
│       └── ServicoFisicaVooTestes.cs                # Testes de empuxo, vetorização em pitch e aceleração
└── AeroAscent.Core.Aplicacao.Testes/
    └── CasosDeUso/
        └── AtualizarFisicaVooCasoDeUsoTestes.cs     # Testes de orquestração de boost, esgotamento < 1ms e zero GC
```

**Structure Decision**: A implementação se integra diretamente às bibliotecas de classes existentes `AeroAscent.Core.Dominio` e `AeroAscent.Core.Aplicacao`, bem como aos seus projetos de teste correspondentes, sem introduzir projetos ou acoplamentos desnecessários.

---

## Complexity Tracking

> Nenhuma violação aos princípios da Constituição foi identificada. O design técnico preserva o desacoplamento de Clean Architecture, ausência total de referências à Unity no Core, modelagem DDD expressiva e conformidade estrita com `GC Alloc = 0 bytes`.
