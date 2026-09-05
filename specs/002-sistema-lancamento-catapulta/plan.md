# Implementation Plan: Sistema de Lançamento Inicial e Catapulta

**Branch**: `002-sistema-lancamento-catapulta` | **Date**: 2026-09-04 | **Spec**: [spec.md](file:///h:/tmp/RSA/Loterias/JogosMaster/GitHub/AeroAscent/specs/002-sistema-lancamento-catapulta/spec.md)

**Input**: Especificação da mecânica de lançamento inicial com catapulta, impulso vetorial 3D, precisão temporal e orquestração de voo.

---

## Summary

Esta feature implementa o mecanismo que dá início a cada rodada de jogo do **AeroAscent**. O objetivo é fornecer o cálculo do impulso vetorial tridimensional ($\vec{V}_0$) decomposto em $35^\circ$ no padrão de coordenadas da Unity Engine (avanço horizontal no eixo Z e altitude no eixo Y), aplicando a fórmula de escalonamento linear por nível de catapulta (+25% por nível com $\text{FORCA\_BASE} = 25.0\text{ m/s}$). Além disso, implementa a lógica analítica do medidor oscilante de força com piso mínimo protetivo de 10% (0.10f) e o caso de uso `LancarAeronaveCasoDeUso` na camada de Aplicação (`Core/Aplicacao`), orquestrando a transição segura da entidade `Voo` para `EmVoo`.

---

## Technical Context

**Language/Version**: C# 12 / .NET Standard 2.1 & .NET 8 (`netstandard2.1;net8.0`)  
**Primary Dependencies**: Zero dependências de terceiros no Domínio e Aplicação (apenas BCL). No projeto de testes: `xunit` (v2.x) e `Microsoft.NET.Test.Sdk`.  
**Storage**: N/A para esta fase (manipula entidades e objetos de valor em memória).  
**Testing**: Suíte xUnit executada via `dotnet test`.  
**Target Platform**: Multiplataforma (Windows e Android via Unity Engine / C# puro).  
**Project Type**: Bibliotecas de Classes C# (.NET Standard 2.1 / .NET 8) + Projetos de Testes xUnit.  
**Performance Goals**: Tempo de execução da suíte de testes < 200 ms (SC-001), transição de estado da catapulta < 16 ms (SC-002) e **GC Alloc = 0 bytes** nas operações de cálculo com `VetorVoo` e `ParametrosLancamento`.  
**Constraints**: Clean Architecture estrita (cálculo de física pura no Domínio via `ServicoFisicaVoo`, orquestração na camada de Aplicação via `LancarAeronaveCasoDeUso`), 100% dos nomes e documentação XML em Português Brasileiro (pt-BR).  
**Scale/Scope**: 1 Caso de Uso (`LancarAeronaveCasoDeUso`), 1 Serviço de Domínio (`ServicoFisicaVoo`), 3 Objetos de Valor (`ParametrosLancamento`, `ResultadoLancamento`, `MedidorForcaOscilante`), 1 Interface de Caso de Uso (`ILancarAeronaveCasoDeUso`).

---

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Artigo da Constituição | Requisito / Princípio | Status de Conformidade | Evidência / Decisão de Projeto |
|---|---|---|---|
| **Artigo I** | Ética Familiar e Zero Anúncios | **APROVADO** | Piso protetivo de 10% de impulso implementado para evitar frustração de crianças e famílias em erros de timing; ausência total de anúncios. |
| **Artigo II** | Gameplay Justo e Física | **APROVADO** | A velocidade e altura iniciais decorrem diretamente da precisão do timing do jogador na barra de força e dos upgrades legítimos da catapulta. |
| **Artigo III.1** | Nomenclatura e Idioma (pt-BR) | **APROVADO** | 100% dos nomes de classes, métodos, parâmetros, testes e documentação XML em Português Brasileiro. |
| **Artigo III.2** | Clean Architecture e Domínio Desacoplado | **APROVADO** | Separação entre Domínio (`ServicoFisicaVoo`) e Aplicação (`LancarAeronaveCasoDeUso`). Zero acoplamento com `UnityEngine` ou `MonoBehaviour`. |
| **Artigo III.3** | SOLID e DDD | **APROVADO** | Injeção de dependência via `IServicoFisicaVoo`; objetos de valor imutáveis (`readonly record struct` e `record`). |
| **Artigo III.4** | Performance e Alocação Zero | **APROVADO** | `ParametrosLancamento`, `MedidorForcaOscilante` e `VetorVoo` são `readonly record struct`, alocados na stack com 0 bytes de lixo GC. |
| **Artigo V** | Checklist de Governança | **APROVADO** | Todos os itens avaliados e conformes. |

---

## Project Structure

### Documentation (this feature)

```text
specs/002-sistema-lancamento-catapulta/
├── spec.md              # Especificação refinada com 5 clarificações incorporadas
├── plan.md              # Este plano de implementação técnica
├── research.md          # Fase 0: Decomposição 3D, função periódica triangular e arquitetura
├── data-model.md        # Fase 1: Diagrama de tipos, invariantes e fórmulas
├── quickstart.md        # Fase 1: Guia prático de comandos e cenários de validação
└── contracts/           # Fase 1: Interfaces de contratos de aplicação
    └── ILancarAeronaveCasoDeUso.cs
```

### Source Code (repository root)

```text
src/
├── AeroAscent.Core.Dominio/
│   ├── ObjetosDeValor/
│   │   ├── ParametrosLancamento.cs
│   │   ├── ResultadoLancamento.cs
│   │   └── MedidorForcaOscilante.cs
│   └── Servicos/
│       └── ServicoFisicaVoo.cs
│
└── AeroAscent.Core.Aplicacao/
    ├── AeroAscent.Core.Aplicacao.csproj
    ├── Contratos/
    │   └── ILancarAeronaveCasoDeUso.cs
    └── CasosDeUso/
        └── LancarAeronaveCasoDeUso.cs

tests/
├── AeroAscent.Core.Dominio.Testes/
│   ├── ObjetosDeValor/
│   │   ├── ParametrosLancamentoTestes.cs
│   │   └── MedidorForcaOscilanteTestes.cs
│   └── Servicos/
│       └── ServicoFisicaVooTestes.cs
│
└── AeroAscent.Core.Aplicacao.Testes/
    ├── AeroAscent.Core.Aplicacao.Testes.csproj
    └── CasosDeUso/
        └── LancarAeronaveCasoDeUsoTestes.cs
```

**Structure Decision**: Criação do projeto `AeroAscent.Core.Aplicacao` para hospedar casos de uso da Clean Architecture, com respectivo projeto de testes `AeroAscent.Core.Aplicacao.Testes`, mantendo a biblioteca de domínio focada exclusivamente nas regras, serviços de cálculo e invariantes fundamentais.

---

## Complexity Tracking

*(Nenhuma violação identificada. O desenho técnico está em total conformidade com todos os artigos da Constituição).*

| Violação | Justificativa | Alternativa Mais Simples Rejeitada Porque |
|---|---|---|
| *Nenhuma* | *N/A* | *N/A* |
