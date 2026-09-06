# Pesquisa e Decisões Técnicas: Interface de Resumo de Voo e Celebração de Recorde (Feature 012)

## Contexto e Objetivos

A Feature 012 é responsável pela interface pós-voo exibida imediatamente após o término do voo (quando a aeronave pousa ou colide e para no solo):
1. **Exibição Consolidada e Transparente de Desempenho**: Apresentação da distância final percorrida e da altitude máxima alcançada em metros com formatação localizada (pt-BR).
2. **Decomposição e Animação de Recompensas**: Demonstração detalhada das moedas ganhas divididas por distância, altitude e coletáveis no ar, acompanhada por animação numérica progressiva (*counter tween*) com duração de 1,5 segundos e suporte a pulo instantâneo (*skip to end*) via toque em qualquer ponto da tela.
3. **Celebração Festiva de Recorde**: Exibição de banner comemorativo ("NOVO RECORDE!"), emissão de partículas de confetes coloridos e fanfarra festiva se `EhNovoRecorde == true`.
4. **Navegação Desacoplada e Decisão Pós-Voo**: Botões "Ir para Oficina" e "Voar Novamente", acionando eventos C# puros no apresentador para que o orquestrador de fluxo da Unity execute o roteamento de telas.
5. **Garantia de Persistência Imediata**: Liquidação e gravação em disco já efetuadas pelo `IFinalizarVooCasoDeUso` antes da abertura da tela de resumo, prevenindo qualquer perda de moedas ou quebra de recorde por encerramento abrupto do aplicativo.

---

## Decisões Arquiteturais e de Design

### 1. Padrão Arquitetural: Model-View-Presenter (MVP) com Visão Passiva
- **Decisão**: Adotar o padrão **MVP (Model-View-Presenter)** com visão passiva (*Passive View*), mantendo a arquitetura das Features 010 (Oficina) e 011 (HUD de Voo):
  - **Apresentador (`ApresentadorResumoVoo`)**: Classe em C# puro (.NET Standard 2.1) em `AeroAscent.Core.Aplicacao/Apresentadores`. Zero referências a `UnityEngine`. Implementa a interface `IApresentadorResumoVoo`, recebe a struct `ResumoFinalizacaoVoo`, formata o `ModeloVisualResumoVoo`, gerencia o estado da animação (em andamento vs. concluída), controla a interatividade dos botões de navegação e dispara os eventos `AoSolicitarIrParaOficina` e `AoSolicitarVoarNovamente`.
  - **Visão Passiva (`IVisaoResumoVoo`)**: Interface em `AeroAscent.Core.Aplicacao/Contratos` que define métodos para comando da interface gráfica (`ExibirResumo`, `ConcluirAnimacaoMoedas`, `HabilitarBotoesNavegacao`, `Ocultar`) e eventos de entrada do usuário (`AoClicarOficina`, `AoClicarVoarNovamente`, `AoClicarPularAnimacao`).
  - **Visão Concreta (`ControladorUIResumoVoo`)**: Componente `MonoBehaviour` na camada Unity (`Apresentacao/Unity`) que implementa `IVisaoResumoVoo`, associando textos TextMeshPro, animação de contagem de moedas com *tweening*, emissor de confetes Shuriken e eventos de clique do Unity Canvas.
- **Justificativa**: Garante que toda a lógica de exibição, formatação, controle de fluxo de animação e roteamento seja 100% testável via xUnit no .NET 8 em frações de segundo, sem depender do ciclo de vida da Unity ou de emuladores mobile.
- **Alternativas Rejeitadas**:
  - *Lógica de resumo embutida em MonoBehaviour*: Dificultaria a automação de testes unitários e violaria a Clean Architecture do projeto.
  - *MVVM com Reactive Extensions*: Desnecessário para uma tela com ciclo de vida simples e pontual, adicionando complexidade e alocações no heap.

---

### 2. Integração com Casos de Uso e Ordem de Persistência
- **Decisão**: A persistência do voo e a atualização do saldo do jogador são executadas estritamente **antes** de abrir o resumo, através do caso de uso já implementado `IFinalizarVooCasoDeUso.ExecutarAsync(voo)`.
  - Ao término do voo, o orquestrador invoca `ExecutarAsync()`, que persiste o arquivo JSON de progresso e retorna a struct `ResumoFinalizacaoVoo`.
  - O resultado `ResumoFinalizacaoVoo` é passado diretamente para o método `ApresentadorResumoVoo.Exibir(resumo)`.
- **Justificativa**: Evita perda de recompensas caso o jogador feche o aplicativo ou a bateria acabe durante a animação de contagem. O resumo atua como um extrato visual de uma transação financeira já consolidada no disco.
- **Alternativas Rejeitadas**:
  - *Persistir apenas ao clicar em "Oficina" ou "Voar Novamente"*: Risco gravíssimo de perda de progresso se o jogo for encerrado na tela de resumo.

---

### 3. Modelo Visual Imutável na Stack (`readonly record struct ModeloVisualResumoVoo`)
- **Decisão**: Modelar o DTO de projeção como `public readonly record struct ModeloVisualResumoVoo`.
  - Contém valores numéricos puros (`DistanciaMetros`, `AltitudeMaximaMetros`, `MoedasDistancia`, `MoedasAltitude`, `MoedasColetadas`, `TotalMoedasGanhas`, `SaldoFinal`) e strings pré-formatadas em pt-BR com separadores decimais e de milhar (ex: `"125,4 m"`, `"45,2 m"`, `"+34"`, `"💰 1.250"`).
  - Inclui a flag agregada `EhNovoRecorde` (`EhNovoRecordeDistancia || EhNovoRecordeAltitude`).
  - Passado para `IVisaoResumoVoo.ExibirResumo(in ModeloVisualResumoVoo modelo)` via modificador `in` para evitar cópias de memória no stack.
- **Justificativa**: Respeita o Artigo III e Artigo V da Constituição: alocação zero de objetos temporários no heap para estruturas de dados intermediárias.
- **Alternativas Rejeitadas**:
  - *Classe POCO com heap allocation*: Geraria lixo de memória descartável após cada voo.

---

### 4. Controle de Animação de Contagem e Pulo Instantâneo (*Skip to End*)
- **Decisão**:
  - O tempo total padrão da animação de contagem de moedas é de **1,5 segundos**.
  - Ao iniciar, `ApresentadorResumoVoo.Exibir(resumo)` marca o estado `AnimacaoEmAndamento = true` e comanda a visão para iniciar o resumo e desabilitar os botões de navegação (`_visao.HabilitarBotoesNavegacao(false)`).
  - Se a visão notificar término do tempo ou se o jogador tocar na tela (`AoClicarPularAnimacao`), o apresentador invoca `_visao.ConcluirAnimacaoMoedas()` (que salta o contador para o valor final imediatamente), atualiza `AnimacaoEmAndamento = false` e comanda `_visao.HabilitarBotoesNavegacao(true)`.
  - Tentativas de clique nos botões de navegação enquanto `AnimacaoEmAndamento == true` não efetuam a transição de cena, mas acionam o pulo da animação para garantir excelente responsividade.
- **Justificativa**: Atende diretamente aos requisitos funcionais `FR-003`, `FR-006` e aos critérios de aceitação da User Story 1, oferecendo satisfação visual sem frustrar jogadores veteranos que desejam velocidade.
- **Alternativas Rejeitadas**:
  - *Animação bloqueante sem opção de pulo*: Punitiva e frustrante em loops rápidos de jogo.

---

### 5. Celebração de Recordes e Efeitos Audiovisuais
- **Decisão**:
  - Se `modelo.EhNovoRecorde` for `true`, a visão passiva ativa o banner comemorativo "NOVO RECORDE!" (com animação de pulso/escala), dispara o sistema de partículas de confete e reproduz o som festivo de recorde.
  - Se for `false`, o banner e o efeito de confetes permanecem ocultos (`SetActive(false)`).
- **Justificativa**: Reforça a experiência familiar positiva (Artigo I e IV da Constituição) sem penalizar voos regulares.
- **Alternativas Rejeitadas**:
  - *Pop-up modal separado para novo recorde*: Poluição visual e cliques extras desnecessários.

---

### 6. Desacoplamento da Navegação Pós-Voo
- **Decisão**:
  - O apresentador expõe os eventos C# puros:
    - `public event Action? AoSolicitarIrParaOficina;`
    - `public event Action? AoSolicitarVoarNovamente;`
  - O coordenador de fluxo da Unity (ex: `GerenciadorFluxoJogo`) se inscreve nesses eventos. Ao serem disparados, o coordenador fecha a tela de resumo e carrega a oficina ou reposiciona a aeronave na rampa de lançamento.
- **Justificativa**: Mantém total separação de responsabilidades. O apresentador decide *quando* é permitido navegar; o gerenciador da Unity decide *como* a cena ou estado é alternado.
- **Alternativas Rejeitadas**:
  - *Chamar SceneManager diretamente no apresentador*: Quebraria a Clean Architecture ao depender da Unity Engine no núcleo de aplicação.
