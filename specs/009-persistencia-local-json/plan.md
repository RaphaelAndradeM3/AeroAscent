# Plano de Implementação: Feature 009 — Persistência de Dados Local Offline First (JSON)

**Branch**: `009-persistencia-local-json` | **Data**: 2026-09-05 | **Spec**: [spec.md](./spec.md)  
**Artefatos Relacionados**: [research.md](./research.md) | [data-model.md](./data-model.md) | [quickstart.md](./quickstart.md) | [contracts/](./contracts/)

---

## 📋 Resumo da Funcionalidade
Implementar a camada de Infraestrutura do projeto através do projeto `AeroAscent.Infraestrutura`, fornecendo o repositório concreto `RepositorioProgressoLocalJson` que implementa a interface `IRepositorioProgresso` definida no Domínio.
A solução implementa salvamento atômico seguro (arquivo `.tmp` com rotação de `.bak` e promoção via `File.Move`), desserialização resiliente contra corrupção, sincronização de concorrência assíncrona com `SemaphoreSlim(1, 1)` e isolamento de DTO com versionamento explícito de schema (`VersaoSchema = 1`).

---

## 💻 Contexto Técnico

- **Linguagem / Versão**: C# 12 (.NET 8.0 e .NET Standard 2.1 para compatibilidade com Unity IL2CPP).
- **Dependências Principais**:
  - `AeroAscent.Core.Dominio`: Contém `IRepositorioProgresso`, `ProgressoJogador`, `Aeronave`, `Moeda`.
  - `System.Text.Json`: Serialização nativa de alta velocidade.
  - Testes: xUnit 2.8+, runner .NET 8.0.
- **Armazenamento**: Arquivo local JSON em UTF-8 com gravação atômica (`progresso.tmp` $\to$ `progresso.json` e cópia redundante `progresso.bak`).
- **Plataforma Alvo**: Unity Multiplataforma (Windows Desktop DirectX/Vulkan e Android Vulkan/OpenGL ES 3.2 via `Application.persistentDataPath`).
- **Metas de Performance**:
  - `SC-001`: Tempo de salvamento assíncrono em disco inferior a 15 milissegundos ($< 15\text{ms}$).
  - `SC-002`: Zero perda ou corrupção de dados comprovada em testes de concorrência com semáforo assíncrono.
  - `SC-003`: 100% de cobertura de testes unitários e de integração cobrindo roundtrip, fallback, corrupção e concorrência.

---

## 📜 Constitution Check

*GATE: Validação pré e pós-design com base nos princípios da Constituição (v1.2.0).*

| Artigo Constitucional | Diretriz | Status | Justificativa / Validação |
|---|---|---|---|
| **Artigo I** | Experiência Familiar, Ética e Zero Anúncios | ✅ Aprovado | 100% Offline First; zero telemetria, zero chamadas de rede ou analytics invasivos. |
| **Artigo II** | Gameplay Justo e Progressão por Habilidade | ✅ Aprovado | Integridade dos dados garantida por gravação atômica e backup, prevenindo perda de moedas ou níveis ganhos. |
| **Artigo III.1** | Idioma 100% em pt-BR | ✅ Aprovado | Todos os identificadores, métodos, propriedades, exceções e documentação XML (`///`) estritamente em Português Brasileiro. |
| **Artigo III.2** | Clean Architecture e Separação de Camadas | ✅ Aprovado | O repositório pertence exclusivamente à camada `Infraestrutura`, implementando a interface `IRepositorioProgresso` do Domínio sem acoplamento reverso. |
| **Artigo III.4** | Performance Mobile First | ✅ Aprovado | Semáforo assíncrono não bloqueante, serialização otimizada com `System.Text.Json` e tempo de resposta $< 15\text{ms}$. |
| **Artigo V** | Governança e Métodos Assíncronos | ✅ Aprovado | Métodos assíncronos utilizam sufixo `Async` e `CancellationToken`; injeção estrita de dependências via interfaces. |

---

## 📂 Estrutura do Projeto

### Documentação da Feature
```text
specs/009-persistencia-local-json/
├── spec.md              # Especificação de requisitos e clarificações da Feature 009
├── plan.md              # Este plano de implementação
├── research.md          # Decisões de arquitetura e tecnologia
├── data-model.md        # Modelos de dados e fluxo de I/O atômico
├── quickstart.md        # Cenários executáveis de teste de integração
├── contracts/           # Contratos de DTO e Configuração C#
│   ├── ConfiguracaoPersistenciaLocal.cs
│   └── ProgressoJogadorDTO.cs
└── tasks.md             # Tarefas de implementação (gerado pelo /speckit-tasks)
```

### Código-Fonte da Solução
```text
src/
├── AeroAscent.Core.Dominio/       # Domínio puro (já existente)
├── AeroAscent.Core.Aplicacao/     # Aplicação pura (já existente)
└── AeroAscent.Infraestrutura/     # [NOVO PROJETO C#]
    ├── Configuracao/
    │   └── ConfiguracaoPersistenciaLocal.cs
    ├── DTOs/
    │   └── ProgressoJogadorDTO.cs
    └── Persistencia/
        └── RepositorioProgressoLocalJson.cs

tests/
├── AeroAscent.Core.Dominio.Testes/       # Testes de domínio (já existente)
├── AeroAscent.Core.Aplicacao.Testes/     # Testes de aplicação (já existente)
└── AeroAscent.Infraestrutura.Testes/     # [NOVO PROJETO DE TESTES xUnit]
    ├── DTOs/
    │   └── ProgressoJogadorDTOTestes.cs
    └── Persistencia/
        ├── RepositorioProgressoLocalJsonTestes.cs
        └── RepositorioProgressoConcorrenciaTestes.cs
```

---

## 🗓️ Fases de Execução do Plano

### Fase 0: Pesquisa e Decisões de Engenharia (`research.md`)
- [x] Selecionar `System.Text.Json` para serialização de alta performance sem dependências externas (D1).
- [x] Definir protocolo de gravação atômica via arquivo temporário `.tmp`, backup `.bak` e `File.Move` (D2).
- [x] Estabelecer recuperação automática de corrupção com fallback e isolamento `.corrompido` (D3).
- [x] Implementar controle de concorrência com `SemaphoreSlim(1, 1)` assíncrono (D4).
- [x] Criar estrutura de DTO plano com `VersaoSchema = 1` e data/hora UTC (D5).
- [x] Projetar `ConfiguracaoPersistenciaLocal` desacoplando caminhos físicos da engine (D6).

### Fase 1: Design e Contratos de Infraestrutura
- [x] Criar `contracts/ProgressoJogadorDTO.cs`.
- [x] Criar `contracts/ConfiguracaoPersistenciaLocal.cs`.
- [x] Modelar ciclo de vida e diagramas de I/O em `data-model.md`.
- [x] Definir cenários de teste executáveis em `quickstart.md`.
- [x] Consolidar arquitetura e estrutura da solução em `plan.md`.

### Fase 2: Implementação e Tarefas (`tasks.md`)
- [ ] Gerar tarefas de implementação através do `/speckit-tasks 009-persistencia-local-json`.
