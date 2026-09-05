# Implementation Plan: Simulação de Física Aerodinâmica e Controle de Pitch

**Branch**: `003-fisica-voo-aerodinamica` | **Date**: 2026-09-05 | **Spec**: [spec.md](file:///h:/tmp/RSA/Loterias/JogosMaster/GitHub/AeroAscent/specs/003-fisica-voo-aerodinamica/spec.md)

**Input**: Feature specification from `/specs/003-fisica-voo-aerodinamica/spec.md`

---

## Summary

Implementação do motor cinemático e aerodinâmico desacoplado no plano longitudinal 3D canônico da Unity Engine (Eixo Z para avanço frontal, Eixo Y para altitude/sustentação e Eixo X transversal). O sistema calcula as forças de sustentação (com modelo linear arcade e estol suave acolhedor até $\alpha_{\text{estol}} = 20^\circ$), arrasto aerodinâmico (com escalonamento redutor baseado no nível de melhoria da oficina), gravidade padrão ($9.81\text{ m/s}^2$) e integração numérica de Euler Semi-Implícito de altíssima performance ($< 0.05\text{ms}$ e `GC Alloc = 0 bytes`). O jogador comanda a taxa angular de arfagem/pitch ($-1.0$ a $+1.0$ limitando o nariz entre $-45^\circ$ e $+60^\circ$), com autoestabilização direcional ao soltar os controles e dinâmica de solo com atrito cinético ($\mu = 0.3$) ao tocar a pista ($Y \le 0$), desacelerando até a parada completa e consolidando a pontuação do voo via `voo.Pousar()`.

---

## Technical Context

**Language/Version**: C# 12 / .NET Standard 2.1 e .NET 8.0 (multi-target nativo)  
**Primary Dependencies**: C# BCL puro (`System`, `System.MathF`). Zero dependências de frameworks externos ou de `UnityEngine` no Domínio e Aplicação.  
**Storage**: N/A para esta feature (dados de voo são calculados na stack e acumulados na entidade em memória `Voo`).  
**Testing**: xUnit com asserções detalhadas, medição de tempo com `Stopwatch` para benchmarks de latência (< 0.05ms) e validação de `GC.GetAllocatedBytesForCurrentThread() == 0` para garantia de zero alocação no heap.  
**Target Platform**: Multiplataforma nativa de alta performance (Windows Standalone e Android via Unity com compilação IL2CPP).  
**Project Type**: Biblioteca de Classes de Domínio e Aplicação (.NET Standard 2.1 / .NET 8) para Game Engine.  
**Performance Goals**: Execução de 1 passo de simulação física em tempo $< 0.05\text{ms}$ (SC-001) para garantia absoluta de 60 FPS contínuos (16.6ms por frame).  
**Constraints**: `GC Alloc = 0 bytes` no loop de física (SC-002), tipos por valor `readonly record struct` na stack, 100% em Português Brasileiro (pt-BR) com documentação XML rigorosa.  
**Scale/Scope**: 2 novos Value Objects (`EstadoFisicoAeronave`, `ParametrosControlePiloto`), enriquecimento do serviço de domínio `ServicoFisicaVoo`, novo caso de uso de aplicação `AtualizarFisicaVooCasoDeUso` e cobertura exaustiva de testes unitários.

---

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

- [x] **Artigo I — Experiência Familiar, Ética e Zero Anúncios**: A simulação adota um modelo de estol acolhedor com decaimento suave de sustentação, sem quedas catastróficas ou punições abruptas. Zero monetização predatória ou anúncios.
- [x] **Artigo II — Gameplay Justo e Física como Pilar**: Trajetória autêntica regida por sustentação, arrasto, gravidade e comandos de pilotagem. Sem trilhos invisíveis. A melhoria de aerodinâmica da oficina impacta a física de forma tangível e proporcional.
- [x] **Artigo III.1 — Nomenclatura e Idioma (pt-BR Obrigatório)**: 100% das classes, structs, interfaces, métodos, parâmetros, testes e documentações XML em Português Brasileiro.
- [x] **Artigo III.2 — Clean Architecture e Domínio Desacoplado**: O núcleo físico reside em `AeroAscent.Core.Dominio` sem referências a `UnityEngine` ou `MonoBehaviour`. O fluxo de orquestração reside em `AeroAscent.Core.Aplicacao`.
- [x] **Artigo III.3 — Princípios SOLID e DDD**: Entidades (`Voo`) com encapsulamento e proteção de invariantes; Objetos de Valor (`EstadoFisicoAeronave`, `ParametrosControlePiloto`, `VetorVoo`) modelados como `readonly record struct` imutáveis; Injeção de Dependências exclusiva via interfaces.
- [x] **Artigo III.4 — Performance Mobile First e Gestão de Memória**: Alocação zero de memória no heap (`GC Alloc = 0 bytes`) durante o passo contínuo de simulação física.
- [x] **Artigo V — Checklist de Governança**: Todos os critérios constitucionais validados e respeitados no design técnico.

---

## Project Structure

### Documentation (this feature)

```text
specs/003-fisica-voo-aerodinamica/
├── plan.md              # Este documento de planejamento (/speckit-plan)
├── research.md          # Pesquisa técnica e decisões de modelagem física
├── data-model.md        # Modelo de dados, structs na stack e ciclo de estados
├── quickstart.md        # Guia rápido com cenários de teste e validação
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
│   │   └── IServicoFisicaVoo.cs                     # Contrato enriquecido com SimularPasso
│   ├── ObjetosDeValor/
│   │   ├── EstadoFisicoAeronave.cs                  # [NEW] Struct imutável com cinemática 3D
│   │   ├── ParametrosControlePiloto.cs              # [NEW] Struct imutável com inputs do jogador
│   │   └── VetorVoo.cs                              # Struct de vetor 3D na stack
│   └── Servicos/
│       └── ServicoFisicaVoo.cs                      # Implementação do modelo aerodinâmico e solo
└── AeroAscent.Core.Aplicacao/
    ├── Contratos/
    │   └── IAtualizarFisicaVooCasoDeUso.cs          # [NEW] Interface do caso de uso de física
    └── CasosDeUso/
        └── AtualizarFisicaVooCasoDeUso.cs           # [NEW] Orquestração de voo, métricas e pouso

tests/
├── AeroAscent.Core.Dominio.Testes/
│   ├── ObjetosDeValor/
│   │   ├── EstadoFisicoAeronaveTestes.cs            # [NEW] Testes unitários do EstadoFisicoAeronave
│   │   └── ParametrosControlePilotoTestes.cs        # [NEW] Testes unitários de ParametrosControlePiloto
│   └── Servicos/
│       └── ServicoFisicaVooTestes.cs                # Testes de sustentação, arrasto, estol e solo
└── AeroAscent.Core.Aplicacao.Testes/
    └── CasosDeUso/
        └── AtualizarFisicaVooCasoDeUsoTestes.cs     # [NEW] Testes de orquestração de voo e pouso
```

**Structure Decision**: Arquitetura em 2 projetos de biblioteca de classes puros (.NET Standard 2.1 / .NET 8) já existentes na solução: `AeroAscent.Core.Dominio` e `AeroAscent.Core.Aplicacao`, com espelhamento equivalente nos projetos de teste `AeroAscent.Core.Dominio.Testes` e `AeroAscent.Core.Aplicacao.Testes`.

---

## Complexity Tracking

> Nenhuma violação constitucional detectada. O design respeita rigorosamente a Clean Architecture, alocação zero de memória e convenções em pt-BR.
