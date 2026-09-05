# 📜 Constituição do Projeto: AeroAscent

> **Versão:** 1.0.0  
> **Status:** Ativo e Obrigatório  
> **Idioma Oficial de Desenvolvimento:** Português Brasileiro (pt-BR)  
> **Pilares:** Ética Familiar, Excelência em Engenharia C#/.NET, Clean Architecture, Performance Mobile First

---

## 🏛️ Preâmbulo

Esta Constituição estabelece os princípios fundamentais, a filosofia de produto e os padrões inegociáveis de engenharia para o desenvolvimento do **AeroAscent** (anteriormente referenciado como AeroHorizon).

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

## 💻 Artigo III — Engenharia de Software C# .NET e Unity

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
  - Contém as entidades de negócio (`Aeronave`, `Voo`, `Oficina`), Objetos de Valor (`Combustivel`, `Velocidade`, `Altitude`, `Moeda`), regras de cálculo e interfaces.
  - **Zero Dependências da Engine:** O domínio é C# puro (.NET Standard / .NET Core), sem acoplamento a `UnityEngine`, `MonoBehaviour` ou bibliotecas proprietárias.
- **Camada de Aplicação (`Core/Aplicacao`):**
  - Casos de uso e orquestração dos fluxos de jogo (ex: `LancarAeronaveCasoDeUso`, `ComprarMelhoriaCasoDeUso`).
- **Camada de Infraestrutura (`Infraestrutura`):**
  - Implementações de persistência local em JSON, adaptadores de áudio e logging estruturado.
- **Camada de Apresentação / Unity (`Apresentacao/Unity`):**
  - Scripts `MonoBehaviour`, Controladores de Visualização, HUD, Partículas e integração com a física de `Rigidbody` da Unity via adaptadores limpos.

### 3. Princípios SOLID e Domain-Driven Design (DDD)
- **Entidades:** Classes com identidade única (`Guid Id`), métodos encapsulados de alteração de estado e invariantes protegidas.
- **Objetos de Valor (Value Objects):** Imutáveis, modelados como `record` em C# (ex: `PosicaoVoo`, `ConsumoCombustivel`).
- **Injeção de Dependências (DI):** Depender exclusivamente de interfaces (`IServico...`, `IRepositorio...`), nunca de classes concretas.
- **Documentação XML:** Toda classe pública, interface e método público deve conter documentação XML completa (`<summary>`, `<param>`, `<returns>`, `<exception>`).

### 4. Performance Mobile First e Gestão de Memória
- **Alocação Zero no Loop de Execução:** Proibido instanciar objetos (`new`) ou invocar métodos que gerem lixo de memória (*Garbage Collection - GC*) dentro de `Update()`, `FixedUpdate()` ou `LateUpdate()`.
- **Object Pooling:** Obrigatório para todos os elementos dinâmicos e repetitivos (moedas no ar, nuvens, partículas, marcadores de distância).
- **Target Frame Rate:** O jogo deve rodar de forma estável a 60 FPS em dispositivos móveis medianos.

---

## 🎨 Artigo IV — Identidade Visual e Licenciamento Aberto

1. **Assets Abertos e Éticos (CC0):**  
   Prioridade absoluta para o ecossistema de assets 2D, 3D e de interface em Domínio Público (CC0), como os criados pela plataforma **Kenney.nl**.
2. **Estilo Visual Low Poly e Cores Vivas:**  
   Direção de arte fundamentada em modelos *Low Poly* e sombreamento plano (*flat shading*), assegurando leveza de renderização, visual acolhedor, atemporal e consumo de bateria reduzido.
3. **Áudio e Trilha Sonora:**  
   Efeitos sonoros suaves e trilhas sonoras relaxantes que incentivem a calma e o prazer da exploração aérea.

---

## ✅ Artigo V — Checklist de Governança e Revisão de Código

Antes de qualquer funcionalidade ou código ser mesclado ao projeto, o desenvolvedor ou agente deve validar o seguinte checklist:

- [ ] A funcionalidade preserva a política de **Zero Anúncios** e ausência de mecanismos predatórios?
- [ ] O código respeita a **Clean Architecture** (Domínio puro e isolado de `UnityEngine`)?
- [ ] Todos os identificadores, comentários e documentações XML estão em **Português Brasileiro (pt-BR)**?
- [ ] Entidades usam `class` com `Guid` e Objetos de Valor usam `record` imutável?
- [ ] Todas as dependências estão injetadas via **Interfaces**?
- [ ] O código dentro de loops de atualização física (`Update`/`FixedUpdate`) possui **alocação de memória zero** (sem `new` ou delegates temporários)?
- [ ] Itens repetitivos utilizam o padrão **Object Pool**?
- [ ] Todos os métodos assíncronos utilizam o sufixo `Async`?

---

*Aprovado para a posteridade e para a alegria da família e dos jogadores de AeroAscent.*
