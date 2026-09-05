# GEMINI.md — Padrões e Convenções de Projeto C# .NET (AeroAscent)

> **ATENÇÃO AGENTE DE IA:** Este projeto segue estritamente as diretrizes do **`csharp-dotnet-guidelines`** e a **`constitution.md`** do projeto. Respeite estas regras em TODAS as gerações, refatorações e análises de código.

---

## 🌐 Idioma Obrigatório
- **100% em Português Brasileiro (pt-BR)** para todas as classes, interfaces, métodos, variáveis, enums, comentários, mensagens de exceção, documentações XML (`///`) e logs.
- Identificadores técnicos externos universais (ex: `JSON`, `GUID`, `Vector3`) mantêm o nome técnico com contexto em pt-BR.

---

## 📛 Convenções de Nomenclatura C# (.NET)
| Elemento | Padrão | Exemplo |
|---|---|---|
| **Classes / Records / Enums** | `PascalCase` | `Aeronave`, `Combustivel`, `StatusVoo` |
| **Interfaces** | `I` + `PascalCase` | `IRepositorioProgresso`, `IServicoFisicaVoo` |
| **Métodos Públicos** | `PascalCase` | `CalcularSustentacao()`, `LancarAeronave()` |
| **Métodos Assíncronos** | Sufixo `Async` | `SalvarProgressoAsync()`, `CarregarProgressoAsync()` |
| **Propriedades Públicas** | `PascalCase` | `VelocidadeAtual`, `NivelMotor` |
| **Campos Privados** | `_` + `camelCase` | `_repositorioProgresso`, `_servicoFisica` |
| **Parâmetros e Variáveis Locais** | `camelCase` | `anguloLancamento`, `distanciaPercorrida` |
| **Constantes** | `UPPER_SNAKE_CASE` | `GRAVIDADE_PADRAO_METROS_POR_SEGUNDO` |

---

## 🏛️ Clean Architecture e Separação de Camadas
1. **Domínio (`Core/Dominio`):** C# Puro (.NET Standard 2.1 / .NET 8). Zero dependências de frameworks de interface (MAUI), engines gráficas ou bibliotecas externas.
2. **Aplicação (`Core/Aplicacao`):** Casos de uso e orquestração dos fluxos de jogo. Depende exclusivamente do Domínio.
3. **Infraestrutura (`Infraestrutura`):** Implementações de repositórios (JSON via `System.Text.Json` e `FileSystem.AppDataDirectory`), adaptadores de áudio e logging.
4. **Apresentação (`Apresentacao/MAUI`):** Aplicação **.NET MAUI** multiplataforma (**Windows e Android**), contendo telas XAML, renderização gráfica via `GraphicsView` / `IDrawable` (ou Canvas 2D), ViewModels (MVVM) e HUD reativo.

---

## 🗺️ Domain-Driven Design (DDD) & SOLID
- **Entidades:** Modeladas como `class` (NUNCA `record`), com `Guid Id` único e encapsulamento de invariantes.
- **Objetos de Valor (Value Objects):** Modelados como `record` imutável em C# (ex: `Combustivel`, `Moeda`, `VetorVoo`).
- **Inversão de Dependências (DIP):** Depender exclusivamente de interfaces (`IServico...`, `IRepositorio...`), nunca de implementações concretas.
- **Documentação XML:** Toda classe pública, interface, método e propriedade pública deve conter `/// <summary>`, `<param>`, `<returns>`.

---

## ⚡ Performance Mobile First e Multiplataforma (Windows e Android)
- **Alocação Zero no Loop de Execução e Renderização (`GC Alloc = 0 bytes`):** Proibido instanciar objetos (`new`) dentro do loop de desenho (`IDrawable.Draw`), atualização física contínua ou no ciclo de despacho de frames.
- **Object Pooling:** Obrigatório para coletáveis (moedas, anéis de vento), partículas e elementos dinâmicos da simulação.
- **Taxa de Quadros Alvo:** 60 FPS estáveis tanto em dispositivos móveis Android quanto em desktop Windows.
