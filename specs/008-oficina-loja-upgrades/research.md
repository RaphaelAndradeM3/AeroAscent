# Pesquisa Técnica e Decisões de Engenharia: Feature 008 — Loja e Oficina de Upgrades da Aeronave

**Branch**: `008-oficina-loja-upgrades` | **Data**: 2026-09-05  
**Spec**: [spec.md](./spec.md)

---

## 🔬 Resumo Executivo
A **Feature 008** estabelece o ecossistema de evolução mecânica e progressão de longo prazo do **AeroAscent**. O objetivo é permitir que o jogador acesse o menu da oficina, inspecione os 4 componentes da aeronave (`Motor`, `Aerodinamica`, `TanqueCombustivel` e `Catapulta`), consulte os custos calculados pela fórmula exponencial ($\lfloor \text{CustoBase} \times 1.5^{N-1} \rfloor$), compre melhorias utilizando seu saldo de moedas acumulado, e tenha o estado da aeronave e carteira persistidos atomicamente via `IRepositorioProgresso`.

---

## 🏛️ Decisões de Arquitetura e Engenharia

### Decisão 1: Aproveitamento e Calibração da Entidade de Domínio `Oficina.cs`
- **Contexto**: A entidade `Oficina.cs` já foi introduzida no domínio com a fórmula exponencial canônica e tabela de custos base calibrada:
  - Motor: 50 moedas (base)
  - Aerodinâmica: 40 moedas (base)
  - Tanque de Combustível: 30 moedas (base)
  - Catapulta: 60 moedas (base)
- **Decisão**: Adotar a entidade de domínio `Oficina` existente como autoridade de cálculo de evolução (`Oficina.CalcularCustoMelhoria` e `Oficina.EvoluirComponente`), sem duplicar lógica de cálculo na camada de Aplicação.
- **Justificativa**: Respeita o Domain-Driven Design (DDD), mantendo a lógica de negócio encapsulada no Domínio puro (.NET Standard 2.1 / .NET 8).
- **Alternativas Rejeitadas**:
  - *Calcular custos diretamente no caso de uso*: Rejeitado por violar a responsabilidade do Domínio e dispersar a lógica econômica.

---

### Decisão 2: Teto Fixo de 10 Níveis (`NIVEL_MAXIMO = 10`)
- **Contexto**: `Aeronave.cs` e `Melhoria.cs` já impõem `NIVEL_MAXIMO = 10`. Tentativas de ultrapassar esse limite disparam `MelhoriaNivelMaximoException`.
- **Decisão**: Oficializar o nível 10 como teto máximo de melhoria mecânica no jogo.
- **Justificativa**: Evita descalibração física da simulação aerodinâmica (que foi testada e calibrada com níveis 1 a 10 nas Features 003 a 006) e garante estabilidade numérica nos cálculos exponenciais de custo.
- **Alternativas Rejeitadas**:
  - *Níveis infinitos*: Rejeitado por levar a números astronômicos de moedas e forças físicas irreais que quebram o jogo.

---

### Decisão 3: Segregação de Casos de Uso (CQRS Leve na Camada de Aplicação)
- **Contexto**: A interface do jogo necessita tanto inspecionar o catálogo quanto executar a compra transacional.
- **Decisão**: Criar dois casos de uso focados:
  1. `ComprarMelhoriaCasoDeUso` (`IComprarMelhoriaCasoDeUso`): Comando transacional que carrega o progresso, valida invariantes, debita moedas, evolui a aeronave, salva atomicamente no repositório e retorna `ResultadoCompraMelhoria`.
  2. `ConsultarOficinaCasoDeUso` (`IConsultarOficinaCasoDeUso`): Consulta de leitura que obtém o progresso atual do jogador, calcula os custos para o próximo nível de cada componente e projeta uma lista imutável de `ItemOficinaDTO`.
- **Justificativa**: Princípio da Responsabilidade Única (SRP) e separação limpa de mutação vs leitura, facilitando testes unitários isolados.
- **Alternativas Rejeitadas**:
  - *Caso de uso único monólito*: Rejeitado por acoplar operações de renderização de interface com fluxo de gravação de repositório.

---

### Decisão 4: Modelagem de DTOs e Alocação Zero de Memória na Stack
- **Contexto**: O Artigo III.4 da Constituição exige performance mobile first com zero GC no loop de jogo e extratos imutáveis.
- **Decisão**:
  - `ResultadoCompraMelhoria`: Modelado como `readonly record struct` na stack (`GC Alloc = 0 bytes`).
  - `ItemOficinaDTO`: Modelado como `readonly record struct` imutável, contendo `TipoMelhoria`, `string NomeAmigavel`, `int NivelAtual`, `Moeda? CustoProximoNivel`, `bool PodeComprar` e `bool EstaNoNivelMaximo`.
- **Justificativa**: Garante alocação mínima de heap na transição entre menus e renderização da loja.

---

### Decisão 5: Representação Visual de Nível Máximo no Catálogo
- **Contexto**: Quando uma peça atinge o nível 10, ela não possui próximo nível nem próximo custo.
- **Decisão**: Projetar `ItemOficinaDTO` com `CustoProximoNivel = null`, `PodeComprar = false` e `EstaNoNivelMaximo = true`.
- **Justificativa**: Permite que a UI do Canvas Unity renderize o rótulo "MÁXIMO" e desabilite o botão de compra de forma declarativa e transparente.

---

### Decisão 6: Resiliência Automática na Primeira Execução
- **Contexto**: Em instalações limpas, o jogador pode navegar até a oficina antes de realizar qualquer voo. Nesse cenário, `CarregarProgressoAsync()` retorna `null`.
- **Decisão**: Se o repositório retornar `null`, o caso de uso instancia automaticamente `ProgressoJogador.CriarNovo()`. Na consulta, exibe o catálogo limpo no nível 1 com saldo zero; na compra, rejeita adequadamente por saldo insuficiente (0 moedas) sem quebrar o jogo.
- **Justificativa**: Total alinhamento ao Artigo I da Constituição (experiência sem frustrações e acolhedora).
