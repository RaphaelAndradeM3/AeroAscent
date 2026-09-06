# Implementation Plan: Áudio, Sistema de Partículas Kenney e Polimento Geral (Feature 013)

**Branch**: `013-audio-particulas-polish` | **Date**: 2026-09-06 | **Spec**: [spec.md](file:///h:/tmp/RSA/Loterias/JogosMaster/GitHub/AeroAscent/specs/013-audio-particulas-polish/spec.md)

**Input**: Feature specification from `specs/013-audio-particulas-polish/spec.md`

## Summary

Implementação do subsistema audiovisual e feedback sensorial de AeroAscent. Inclui a abstração desacoplada de áudio `IServicoAudio` na camada de aplicação, o catálogo tipado `EventoAudio` e o objeto de valor imutável `ConfiguracaoAudio` no domínio (integrado ao `ProgressoJogador`), suporte a loops contínuos de vento e propulsão na stack (`GC Alloc = 0 bytes`), modulação harmônica de pitch para coletas rápidas de moedas, gerenciamento de sistemas de partículas Shuriken com *Object Pooling* e polimento geral de performance mobile (60 FPS estáveis).

---

## Technical Context

**Language/Version**: C# (.NET Standard 2.1 para bibliotecas centrais de Domínio e Aplicação, .NET 8 para testes em xUnit, compatível com Unity 2022.3+ LTS e compilação IL2CPP)  
**Primary Dependencies**: Unity Audio Engine (via adaptador nativo), Unity Particle System (Shuriken); xUnit e FluentAssertions para suíte de testes automatizados. Zero dependências externas no núcleo de regras de negócio.  
**Storage**: Persistência de `ConfiguracaoAudio` integrada no arquivo JSON de `ProgressoJogador` via `IRepositorioProgresso`.  
**Testing**: Testes unitários puros com xUnit no .NET 8 com fixture `ServicoAudioFalso` (tempo de execução < 100ms).  
**Target Platform**: Multiplataforma nativa para Windows Standalone (DirectX/Vulkan) e Android Mobile (Vulkan/OpenGL ES 3.0) via Unity Engine.  
**Project Type**: Jogo arcade 2D/3D multiplataforma com Clean Architecture.  
**Performance Goals**: 60 FPS constantes, alocação zero no loop de execução (`GC Alloc = 0 bytes`) para áudio e partículas, polifonia segura de até 4 vozes de moedas simultâneas.  
**Constraints**: 100% em Português Brasileiro (pt-BR), documentação XML (`///`) integral, assets de áudio sob licença CC0 / Domínio Público (Kenney.nl).  
**Scale/Scope**: 1 serviço de áudio desacoplado, 1 enumeração com 10 eventos sonoros, 1 value object na stack, 4 emissores de partículas (trail, boost, moeda, confetes) e fixture de testes.

---

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

- [x] **Artigo I — Experiência Familiar, Ética e Zero Anúncios**: Efeitos sonoros acolhedores, sem estridência e sem ruídos punitivos. Experiência prazerosa pensada para Ruth, Sofia e Alice.
- [x] **Artigo II — Gameplay Justo, Física e Feedback Dinâmico**: Som de vento e partículas respondem dinamicamente às forças físicas e à aerodinâmica da aeronave.
- [x] **Artigo III.1 — Nomenclatura e Idioma (100% pt-BR)**: Todas as classes, interfaces, métodos, enums, variáveis e documentação XML em português brasileiro.
- [x] **Artigo III.2 — Clean Architecture e Domínio Desacoplado**: `IServicoAudio` em `Core.Aplicacao`, `ConfiguracaoAudio` e `EventoAudio` em `Core.Dominio`, sem acoplamento a `UnityEngine`.
- [x] **Artigo III.4 — Performance Mobile First (`GC Alloc = 0 bytes`)**: Loops de áudio trafegam dados primitivos na stack; partículas operam sob *Object Pooling* e controle por emissão contínua.
- [x] **Artigo IV — Identidade Visual e Licenciamento Aberto (CC0)**: Áudio e partículas fundamentados no catálogo aberto e ético de Kenney.nl.
- [x] **Artigo V — Checklist de Governança**: Todos os critérios aprovados sem violações.

---

## Project Structure

### Documentation (this feature)

```text
specs/013-audio-particulas-polish/
├── plan.md              # Este documento de planejamento técnico
├── research.md          # Decisões arquiteturais e resolução técnica
├── data-model.md        # Especificação de dados, enums e topologia sonora
├── quickstart.md        # Guia executável de validação de testes
├── contracts/           # Interfaces e contratos
│   └── IServicoAudio.cs # Contrato do serviço de áudio da aplicação
└── tasks.md             # Tarefas ordenadas (gerado via /speckit-tasks)
```

### Source Code (repository root)

```text
src/
├── AeroAscent.Core.Dominio/
│   ├── Enums/
│   │   └── EventoAudio.cs               # [NEW] Catálogo tipado dos eventos sonoros
│   ├── ObjetosDeValor/
│   │   └── ConfiguracaoAudio.cs         # [NEW] Struct imutável com volumes e flags
│   └── Entidades/
│       └── ProgressoJogador.cs          # [MODIFY] Integrar ConfiguracaoAudio e método de atualização
└── AeroAscent.Core.Aplicacao/
    └── Contratos/
        └── IServicoAudio.cs             # [NEW] Interface desacoplada do serviço de áudio

tests/
├── AeroAscent.Core.Dominio.Testes/
│   └── ObjetosDeValor/
│       └── ConfiguracaoAudioTestes.cs   # [NEW] Testes de invariantes e imutabilidade de áudio
└── AeroAscent.Core.Aplicacao.Testes/
    ├── Fixtures/
    │   └── ServicoAudioFalso.cs         # [NEW] Spy/Mock para validação isolada de áudio
    └── Servicos/
        └── ServicoAudioTestes.cs        # [NEW] Testes de despacho e modulação contínua
```

**Structure Decision**: A camada de Domínio mantém os tipos imutáveis (`ConfiguracaoAudio`, `EventoAudio`). A camada de Aplicação define o contrato `IServicoAudio`. A camada de Apresentação/Unity implementará o controlador de áudio e gerenciador de partículas com pooling na Unity Engine.

---

## Complexity Tracking

> **Nenhuma violação constitucional detectada.**

| Violação | Por que é necessária | Alternativa mais simples rejeitada porque |
|---|---|---|
| *Nenhuma* | *N/A* | *N/A* |
