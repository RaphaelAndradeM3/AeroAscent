# Pesquisa e Decisões Técnicas: Interface do Menu Principal, Hangar 3D e Oficina (Feature 010)

## Contexto e Objetivos

A Feature 010 estabelece a interface visual primária de entrada do jogador no jogo: o Menu Principal e a Oficina mecânica com visualização da aeronave no Hangar 3D, exibição do saldo de moedas, 4 cartões de evolução de componentes (Motor, Aerodinâmica, Tanque de Combustível e Catapulta) e o botão chamativo de decolagem.

Para manter conformidade estrita com a **Constituição do Projeto (v1.2.0)** e as diretrizes do `csharp-dotnet-guidelines`:
- A lógica de negócio reside no Domínio e os fluxos na Aplicação (`IConsultarOficinaCasoDeUso`, `IComprarMelhoriaCasoDeUso`).
- A lógica de apresentação (formatação de moedas em pt-BR, cálculo de estados visuais, barras de progresso, bloqueio de spam click) deve ser puramente desacoplada da Unity Engine para viabilizar testes unitários em xUnit.
- A camada de apresentação na Unity (`MonoBehaviour`) deve ser uma visão passiva que apenas reflete comandos visuais emitidos pelo apresentador.

---

## Decisões Arquiteturais e de Design

### 1. Padrão de Apresentação: Model-View-Presenter (MVP) com Visão Passiva
- **Decisão**: Adotar o padrão **Model-View-Presenter (MVP)** com visão passiva (*Passive View*).
  - **Apresentador (`ApresentadorOficina`)**: Classe C# pura (.NET Standard 2.1), sem herança de `MonoBehaviour` e sem dependências de `UnityEngine`. Responsável por invocar os casos de uso, formatar os valores para apresentação em pt-BR, orquestrar os dados dos 4 cartões, controlar flags de reentrância e disparar atualizações para a interface `IVisaoOficina`.
  - **Visão (`IVisaoOficina`)**: Interface em C# puro definindo métodos de atualização visual (`AtualizarSaldo`, `AtualizarCartoes`, `DefinirInteracaoHabilitada`, `ExibirFeedbackCompra`, `ExibirMensagemErro`) e eventos de entrada do usuário (`event Action<TipoMelhoria> AoClicarComprar`, `event Action AoClicarDecolar`).
  - **Visão Concreta (`VisaoOficinaMonoBehaviour`)**: Componente `MonoBehaviour` no projeto Unity que implementa `IVisaoOficina`, vinculando elementos de UI (TextMeshPro / Text, Slider, Button, CanvasGroup).
- **Justificativa**: Permite testar 100% dos fluxos de tela, cálculos de formatação monetária, estados de habilitação e prevenção de duplo clique via xUnit no .NET 8 em frações de segundo, sem precisar abrir a Unity Engine.
- **Alternativas Rejeitadas**:
  - *MonoBehaviour único acoplado*: Tornaria impossível testar a lógica de interface no pipeline de testes do .NET 8 sem o Unity Test Framework.
  - *MVVM com Data Binding nativo*: O Unity UI não possui suporte nativo maduro a data binding bidirecional sem plugins pesados de terceiros.

---

### 2. Formatação Monetária e Internacionalização em pt-BR
- **Decisão**: Utilizar estritamente o formato de número com separador de milhar por ponto (`N0`) com a cultura `CultureInfo("pt-BR")` (ex: `💰 500`, `💰 1.250`, `💰 15.000`).
- **Justificativa**: Conforme Artigo III.1 da Constituição e esclarecido nas clarificações, números formatados com ponto são compreensíveis para o público familiar (especialmente crianças como Ruth, Sofia e Alice), evitando ambiguidades com sufixos em inglês ("k", "M").
- **Alternativas Rejeitadas**:
  - *Abreviações compactas ("1.2k")*: Rejeitado por misturar termos em língua estrangeira em um jogo concebido 100% em pt-BR.

---

### 3. Prevenção de Toques Concorrentes (Spam Click) e Debounce
- **Decisão**: Controle atômico no Presenter por flag booleana de processamento (`_estaProcessandoCompra`) combinada com comando imediato para a Visão desabilitar a interatividade (`IVisaoOficina.DefinirInteracaoHabilitada(false)`) durante a execução do método assíncrono `IComprarMelhoriaCasoDeUso.ExecutarAsync()`. Ao concluir, a Visão reabilita a interatividade com os novos estados calculados.
- **Justificativa**: Garante que se o jogador clicar 10 vezes em milissegundos, apenas uma requisição assíncrona será despachada, eliminando riscos de débitos múltiplos inadvertidos e prevenindo erros de I/O no salvamento em disco.
- **Alternativas Rejeitadas**:
  - *Debounce por temporizador (Timer)*: Não sincroniza com o término real do I/O assíncrono, podendo reabilitar o botão antes do salvamento ser concluído em dispositivos lentos.

---

### 4. Estado e Exibição de Melhoria no Nível Máximo
- **Decisão**: Quando `ItemOficinaDTO.EstaNoNivelMaximo == true`:
  - O nível exibe o texto `"Nível 10 (MAX)"`.
  - A barra de progresso visual atinge `1.0f` (100%) com destaque comemorativo.
  - O botão de compra permanece posicionado no layout do cartão porém desabilitado (`EstaHabilitado = false`), com texto `"MÁXIMO"` e sem valor numérico de custo.
- **Justificativa**: Mantém simetria de altura e largura perfeita entre os 4 cartões da oficina, evitando saltos de layout e informando visualmente de forma inequívoca que a aeronave atingiu o ápice mecânico naquele componente.
- **Alternativas Rejeitadas**:
  - *Ocultar o botão*: Causava desalinhamento visual na grade de 4 colunas.

---

### 5. Transição de Decolagem e Hangar 3D
- **Decisão**: O `ApresentadorOficina` expõe o evento C# puro `event Action? AoSolicitarDecolagem`.
  - Ao receber o clique do botão "DECOLAR" na visão, o Presenter notifica os ouvintes.
  - O orquestrador da Unity (`ControladorFluxoJogo` / `GerenciadorCenas`) assina o evento, fecha ou anima a saída do Canvas do menu e move a câmera suavemente da posição de visualização do Hangar para o ponto focal da catapulta de lançamento.
- **Justificativa**: Desacoplamento absoluto entre o ciclo de vida do menu e o subsistema de física de lançamento já construído na Feature 002.
