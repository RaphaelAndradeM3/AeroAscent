<!--
Sync Impact Report:
- Version change: 1.0.0 → 1.1.0
- Target Platforms & Presentation: Definido formalmente .NET MAUI (Windows e Android) em substituição a Unity
- Modified principles: Artigo III (Engenharia de Software C# .NET e .NET MAUI), Artigo V (Checklist de Governança com validação MAUI Windows/Android)
- Templates requiring updates: plan-template.md, spec-template.md, tasks-template.md
-->

# 📜 Constituição do Projeto: AeroAscent

> **Versão:** 1.1.0  
> **Status:** Ativo e Obrigatório  
> **Idioma Oficial de Desenvolvimento:** Português Brasileiro (pt-BR)  
> **Pilares:** Ética Familiar, Excelência em Engenharia C#/.NET, Clean Architecture, .NET MAUI Multiplataforma (Windows e Android), Performance Mobile First

---

## 🏛️ Preâmbulo

Esta Constituição estabelece os princípios fundamentais, a filosofia de produto e os padrões inegociáveis de engenharia para o desenvolvimento do **AeroAscent** (anteriormente referenciado como AeroHorizon), concebido como um jogo multiplataforma em **C# / .NET MAUI** para **Windows e Android**.

Nenhuma linha de código, asset, funcionalidade ou modelo de monetização será integrado ao repositório se violar qualquer um dos artigos aqui estabelecidos.

---

## 👨‍👩‍👧‍👦 Artigo I — Experiência Familiar, Ética e Zero Anúncios

1. **Zero Monetização Predatória:**  
   É estritamente proibida a inclusão de anúncios forçados, banners, vídeos com recompensa compulsória, compras pay-to-win, caixas de saque (*loot boxes*) ou qualquer mecanismo projetado para explorar psicologia comportamental.
2. **Imersão Limpa:**  
   O jogo deve inicializar rapidamente, sem telas falsas de carregamento, pop-ups intrusivos ou pedidos enganosos de permissão.
3. **Ambiente Seguro e Construtivo:**  
   O jogo foi concebido primordialmente como uma experiência acolhedora para todas as idades — especialmente inspirada na **Ruth, Sofia e Alice**. O design visual e sonoro deve inspirar curiosidade, leveza e diversão pura.

---

## 🎯 Artigo II — Gameplay Justo, Física e Progressão por Habilidade

1. **Recompensa por Desempenho Genuíno:**  
   Todo recurso, moeda ou melhoria conquistada no jogo decorre estritamente da habilidade do jogador: precisão no lançamento, gerenciamento aerodinâmico durante o voo e conservação de combustível.
2. **Física como Pilar de Diversão:**  
   O movimento da aeronave não opera sobre trilhos invisíveis. As forças aerodinâmicas (sustentação, arrasto, gravidade e empuxo) regem a trajetória. Dominar as leis físicas e a inclinação (*pitch*) é a mecânica central de aprendizado e maestria.
3. **Ausência de Barreiras Artificiais:**  
   A repetição (*loop*) deve existir organicamente para aprimoramento da habilidade motora e teste de novas configurações de aeronaves, nunca como uma barreira artificial ("grind punitivo") imposta para frustrar o jogador.

---

## 💻 Artigo III — Engenharia de Software C# .NET e .NET MAUI (Windows e Android)

Todo o desenvolvimento em C# deve obedecer rigorosamente às diretrizes do `csharp-dotnet-guidelines`:

### 1. Nomenclatura e Idioma (pt-BR Obrigatório)
- Todas as classes, interfaces, métodos, variáveis, enums, propriedades, documentações XML (`///`) e logs estruturados devem ser escritos em **Português Brasileiro (pt-BR)**.
- **Tabela de Nomenclatura:**
  - Classes / Records / Enums: `PascalCase` (ex: `Aeronave`, `MelhoriaMotor`, `StatusVoo`)
  - Interfaces: `I` + `PascalCase` (ex: `IAeronaveRepositorio`, `IServicoCalculoFisica`)
  - Métodos Públicos: `PascalCase` (ex: `CalcularSustentacao()`, `RegistrarVooAsync()`)
  - Métodos Assíncronos: Sufixo `Async` (ex: `SalvarProgressoAsync()`)
  - Propriedades Públicas: `PascalCase` (ex: `VelocidadeAtual`, `CapacidadeTanque`)
  - Campos Privados: `_` + `camelCase` (ex: `_servicoEconomia`, `_repositorioProgresso`)
  - Parâmetros e Variáveis Locais: `camelCase` (ex: `anguloLancamento`, `distanciaPercorrida`)
  - Constantes: `UPPER_SNAKE_CASE` (ex: `GRAVIDADE_PADRAO_METROS_POR_SEGUNDO`)

### 2. Clean Architecture e Domínio Desacoplado
- **Camada de Domínio (`Core/Dominio`):**
  - Contém as entidades de negócio (`Aeronave`, `Voo`, `Oficina`, `ProgressoJogador`), Objetos de Valor (`Combustivel`, `Moeda`, `VetorVoo`, `Melhoria`, `ResultadoVoo`), regras de cálculo e interfaces.
  - **Zero Dependências de Interface ou Frameworks de UI:** O domínio é C# puro (.NET Standard 2.1 / .NET 8), sem acoplamento a `.NET MAUI`, bibliotecas de UI ou frameworks externos.
- **Camada de Aplicação (`Core/Aplicacao`):**
  - Casos de uso e orquestração dos fluxos de jogo (ex: `LancarAeronaveCasoDeUso`, `ComprarMelhoriaCasoDeUso`, `FinalizarVooCasoDeUso`).
- **Camada de Infraestrutura (`Infraestrutura`):**
  - Implementações de persistência local em JSON via `System.Text.Json` (usando `FileSystem.AppDataDirectory` compatível com Windows e Android), adaptadores de áudio e logging estruturado.
- **Camada de Apresentação (.NET MAUI) (`Apresentacao/MAUI`):**
  - Interface construída com **.NET MAUI**, suportando nativamente **Windows e Android**.
  - Renderização visual através de `GraphicsView` / `IDrawable` (ou Canvas 2D / SkiaSharp), ViewModels reativos (MVVM), HUD interativo e suporte a múltiplos modos de entrada (toque em Android, mouse/teclado em Windows).

### 3. Princípios SOLID e Domain-Driven Design (DDD)
- **Entidades:** Classes com identidade única (`Guid Id`), métodos encapsulados de alteração de estado e invariantes protegidas.
- **Objetos de Valor (Value Objects):** Imutáveis, modelados como `record` em C# (ex: `Combustivel`, `Moeda`, `VetorVoo`, `Melhoria`, `ResultadoVoo`).
- **Injeção de Dependências (DI):** Depender exclusivamente de interfaces (`IServico...`, `IRepositorio...`), nunca de classes concretas.
- **Documentação XML:** Toda classe pública, interface e método público deve conter documentação XML completa (`<summary>`, `<param>`, `<returns>`, `<exception>`).

### 4. Performance Multiplataforma e Gestão de Memória
- **Alocação Zero no Loop de Execução e Renderização:** Proibido instanciar objetos (`new`) ou invocar métodos que gerem lixo de memória (*Garbage Collection - GC*) dentro do método de desenho contínuo (`IDrawable.Draw`), loop de física ou despacho de frames.
- **Object Pooling:** Obrigatório para todos os elementos dinâmicos e repetitivos (moedas no ar, nuvens, partículas, marcadores de distância).
- **Target Frame Rate:** O jogo deve rodar de forma estável a 60 FPS tanto em dispositivos Android (mobile) quanto em computadores com Windows (desktop).

---

## 🎨 Artigo IV — Identidade Visual e Licenciamento Aberto

1. **Assets Abertos e Éticos (CC0):**  
   Prioridade absoluta para o ecossistema de assets 2D, 3D e de interface em Domínio Público (CC0), como os criados pela plataforma **Kenney.nl**.
2. **Estilo Visual Limpo e Cores Vivas:**  
   Direção de arte fundamentada em vetores/sprites leves e formas geométricas coloridas, assegurando renderização veloz em telas de alta densidade no Android e no Windows.
3. **Áudio e Trilha Sonora:**  
   Efeitos sonoros suaves e trilhas sonoras relaxantes que incentivem a calma e o prazer da exploração aérea.

---

## ✅ Artigo V — Checklist de Governança e Revisão de Código

Antes de qualquer funcionalidade ou código ser mesclado ao projeto, o desenvolvedor ou agente deve validar o seguinte checklist:

- [ ] A funcionalidade preserva a política de **Zero Anúncios** e ausência de mecanismos predatórios?
- [ ] O código respeita a **Clean Architecture** (Domínio puro e isolado de `.NET MAUI` ou frameworks visuais)?
- [ ] A camada de apresentação é compatível e validada para **Windows e Android** via **.NET MAUI**?
- [ ] Todos os identificadores, comentários e documentações XML estão em **Português Brasileiro (pt-BR)**?
- [ ] Entidades usam `class` com `Guid` e Objetos de Valor usam `record` imutável?
- [ ] Todas as dependências estão injetadas via **Interfaces**?
- [ ] O código dentro do loop de renderização/física (`Draw`/`Tick`) possui **alocação de memória zero** (sem `new` ou delegates temporários)?
- [ ] Itens repetitivos utilizam o padrão **Object Pool**?
- [ ] Todos os métodos assíncronos utilizam o sufixo `Async`?

---

*Aprovado para a posteridade e para a alegria da família e dos jogadores de AeroAscent.*
