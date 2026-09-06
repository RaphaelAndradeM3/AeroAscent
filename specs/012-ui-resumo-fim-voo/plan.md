# Implementation Plan: Interface de Resumo de Voo e Celebração de Recorde (Feature 012)

**Branch**: `012-ui-resumo-fim-voo` | **Date**: 2026-09-06 | **Spec**: [spec.md](file:///h:/tmp/RSA/Loterias/JogosMaster/GitHub/AeroAscent/specs/012-ui-resumo-fim-voo/spec.md)

**Input**: Feature specification from `specs/012-ui-resumo-fim-voo/spec.md`

## Summary

Implementação da interface de término de voo no padrão Model-View-Presenter (MVP) com visão passiva. A tela exibe a distância final percorrida, altitude máxima, decomposição analítica das moedas ganhas por distância, altitude e coletáveis no ar com contagem progressiva animada de 1,5 segundos (e suporte a pulo instantâneo *skip to end* ao tocar na tela), celebração festiva com confetes e banner comemorativo caso novo recorde pessoal seja estabelecido (`EhNovoRecorde == true`), e botões de navegação desacoplados ("Ir para Oficina" e "Voar Novamente"). O progresso financeiro e recordes já se encontram liquidados e persistidos em disco antes da renderização através de `IFinalizarVooCasoDeUso`.

---

## Technical Context

**Language/Version**: C# (.NET Standard 2.1 para `AeroAscent.Core.Aplicacao` e .NET 8 para testes em `AeroAscent.Core.Aplicacao.Testes`, compatível com Unity 2022.3+ LTS / IL2CPP)  
**Primary Dependencies**: Nenhuma dependência externa no núcleo de aplicação; xUnit e FluentAssertions para suíte de testes automatizados.  
**Storage**: Persistência prévia em JSON via `IFinalizarVooCasoDeUso` / `IRepositorioProgresso` em disco local.  
**Testing**: xUnit no .NET 8 com zero dependências de `UnityEngine` e tempo de execução < 1 segundo.  
**Target Platform**: Multiplataforma Windows Standalone (64-bit) e Android Mobile via Unity Engine.  
**Project Type**: Jogo arcade 2D/3D multiplataforma com Clean Architecture.  
**Performance Goals**: 60 FPS estáveis, abertura do modal em < 100ms, zero alocação de memória no heap (`GC Alloc = 0 bytes`) para estruturas de dados e eventos contínuos.  
**Constraints**: 100% em Português Brasileiro (pt-BR), documentação XML (`///`) integral, desacoplamento rigoroso entre a visão e o apresentador.  
**Scale/Scope**: 1 tela modal de resumo de voo, 1 apresentador MVP, 1 contrato de visão passiva, 1 DTO imutável na stack, 1 fixture falsa de testes e cobertura de testes unitários exaustiva.

---

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

- [x] **Artigo I — Experiência Familiar, Ética e Zero Anúncios**: Sem anúncios forçados, pop-ups enganosos ou compras predatórias. Foco na diversão pura familiar e celebração saudável das conquistas.
- [x] **Artigo II — Gameplay Justo e Progressão por Habilidade**: Recompensas financeiras concedidas estritamente com base na distância, altitude e coletáveis obtidos pela habilidade do jogador.
- [x] **Artigo III.1 — Nomenclatura e Idioma (100% pt-BR)**: Todos os identificadores, classes, interfaces, métodos, variáveis e documentações XML em português brasileiro.
- [x] **Artigo III.2 — Clean Architecture e Domínio Desacoplado**: `ApresentadorResumoVoo`, `IVisaoResumoVoo` e `ModeloVisualResumoVoo` residem na camada de Aplicação (`Core.Aplicacao`), com zero referências a `UnityEngine` ou `MonoBehaviour`.
- [x] **Artigo III.3 — SOLID e DDD**: Responsabilidade única bem definida, contratos de interface injetáveis, DTOs imutáveis na stack (`readonly record struct`).
- [x] **Artigo III.4 — Performance Mobile First (`GC Alloc = 0 bytes`)**: O DTO de projeção visual e os extratos operam como `readonly record struct`, passados via `in` sem criar lixo na memória heap.
- [x] **Artigo V — Checklist de Governança**: Todos os itens do checklist atendidos sem ressalvas.

---

## Project Structure

### Documentation (this feature)

```text
specs/012-ui-resumo-fim-voo/
├── plan.md              # Este documento de planejamento técnico
├── research.md          # Decisões arquiteturais e resolução técnica
├── data-model.md        # Especificação de dados, DTOs e máquina de estados
├── quickstart.md        # Guia executável de validação de testes
├── contracts/           # Interfaces IApresentadorResumoVoo e IVisaoResumoVoo
│   ├── IApresentadorResumoVoo.cs
│   └── IVisaoResumoVoo.cs
└── tasks.md             # Tarefas ordenadas (gerado via /speckit-tasks)
```

### Source Code (repository root)

```text
src/
└── AeroAscent.Core.Aplicacao/
    ├── Contratos/
    │   ├── IApresentadorResumoVoo.cs      # [NEW] Contrato do apresentador MVP de resumo
    │   └── IVisaoResumoVoo.cs             # [NEW] Contrato de visão passiva do resumo
    ├── DTOs/
    │   └── ModeloVisualResumoVoo.cs       # [NEW] Struct imutável com dados e textos formatados
    └── Apresentadores/
        └── ApresentadorResumoVoo.cs       # [NEW] Lógica de apresentação, animação e navegação

tests/
└── AeroAscent.Core.Aplicacao.Testes/
    ├── Fixtures/
    │   └── VisaoResumoVooFalsa.cs         # [NEW] Spy/Mock para validação isolada do apresentador
    └── Apresentadores/
        └── ApresentadorResumoVooTestes.cs # [NEW] Testes unitários com xUnit cobrindo todos os cenários
```

**Structure Decision**: Camada de Aplicação pura em `AeroAscent.Core.Aplicacao` para garantir testabilidade imediata via xUnit no .NET 8. A implementação Unity do componente `ControladorUIResumoVoo` (`MonoBehaviour`) implementará a interface `IVisaoResumoVoo` na camada de Apresentação.

---

## Complexity Tracking

> **Nenhuma violação constitucional detectada.** A arquitetura adota estritamente o padrão MVP já consagrado no projeto (Features 010 e 011).

| Violação | Por que é necessária | Alternativa mais simples rejeitada porque |
|---|---|---|
| *Nenhuma* | *N/A* | *N/A* |
