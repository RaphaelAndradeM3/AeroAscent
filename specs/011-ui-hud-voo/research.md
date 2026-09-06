# Pesquisa e Decisões Técnicas: Interface HUD de Voo e Controles de Toque Mobile (Feature 011)

## Contexto e Objetivos

A Feature 011 estabelece a interface em tempo real exibida durante a simulação de voo da aeronave:
1. **Telemetria e Medidores Superiores/Laterais**: Distância horizontal percorrida em metros com destaque de recorde pessoal, altímetro, velocímetro, indicador de moedas coletadas e medidor vertical de combustível.
2. **Controles Táteis Mobile Ergonômicos**: Botões de inclinação de pitch (subir/descer) no lado esquerdo da tela e botão proeminente de propulsão (Boost) no lado direito, utilizáveis com os polegares em modo paisagem (*landscape*).
3. **Mapeamento Híbrido Desktop**: Suporte transparente a comandos de teclado (Setas direcionais / W-S e Barra de Espaço) para validação no editor e versão Windows.

Para atender à **Constituição do Projeto (v1.2.0)**, em especial os requisitos de **Clean Architecture**, **Alocação Zero no Loop (`GC Alloc = 0 bytes`)** e **Latência < 16ms**:
- A orquestração do HUD é desacoplada da Unity via padrão **Model-View-Presenter (MVP)** com visão passiva.
- Toda a telemetria transita através de uma estrutura imutável na stack (`readonly record struct TelemetriaHUDDTO`).
- A lógica de negócio e os comandos de voo são 100% testáveis via xUnit no .NET 8.

---

## Decisões Arquiteturais e de Design

### 1. Padrão de Apresentação: Model-View-Presenter (MVP) com Visão Passiva
- **Decisão**: Adotar o padrão **Model-View-Presenter (MVP)** com visão passiva (*Passive View*), em total simetria com a arquitetura estabelecida na Feature 010.
  - **Apresentador (`ApresentadorHUDVoo`)**: Classe em C# puro (.NET Standard 2.1) localizada em `AeroAscent.Core.Aplicacao`. Não referencia `UnityEngine`. Orquestra a leitura da sessão de `Voo` e do `EstadoFisicoAeronave`, rastreia superação de recorde, atualiza a telemetria, gerencia o estado dos botões táteis (subida, descida, boost) e traduz comandos em `ParametrosControlePiloto`.
  - **Visão Passiva (`IVisaoHUDVoo`)**: Interface em `AeroAscent.Core.Aplicacao/Contratos` definindo comandos de atualização visual (`AtualizarTelemetria`, `DefinirInteratividadeBoost`, `NotificarNovoRecorde`, `DefinirVisibilidadeControles`) e eventos de entrada do jogador (`AoSolicitarSubida`, `AoInterromperSubida`, `AoSolicitarDescida`, `AoInterromperDescida`, `AoSolicitarBoost`, `AoInterromperBoost`, `AoSolicitarPausa`).
  - **Visão Concreta (`ControladorHUDVoo`)**: Componente `MonoBehaviour` no Unity Canvas que implementa `IVisaoHUDVoo`, vinculando textos TextMeshPro, barra de preenchimento (*Image fill/Slider*) e botões de toque com listeners de ponteiro (`IPointerDownHandler`/`IPointerUpHandler`).
- **Justificativa**: Permite testar 100% da lógica de telemetria, detecção de recorde, cálculo de estado do botão de boost e despacho de comandos de arfagem/propulsão em milissegundos via xUnit no .NET 8, sem necessidade do Unity Test Framework.
- **Alternativas Rejeitadas**:
  - *MonoBehaviour único lendo diretamente entidades*: Inviabilizaria testes unitários automatizados em CI/CD sem abrir a Unity.
  - *Data Binding Reativo (UniRx/ReactiveProperty)*: Dependência externa desnecessária que introduz alocações indesejadas no loop de telemetria.

---

### 2. Alocação Zero de Memória no Loop Contínuo (`GC Alloc = 0 bytes`)
- **Decisão**: 
  1. A transferência de dados do apresentador para a visão utiliza o tipo `readonly record struct TelemetriaHUDDTO`, passado com o modificador `in` (`in TelemetriaHUDDTO telemetria`), residindo exclusivamente na stack sem alocação no heap.
  2. Na visão passiva (`ControladorHUDVoo`), os valores numéricos são comparados contra variáveis em cache (`_distanciaInteiraAnterior`, `_altitudeInteiraAnterior`, `_velocidadeInteiraAnterior`, `_combustivelPercentualAnterior`). Os componentes visuais e strings só são reformatados quando o valor inteiro em metros/km/h ou percentual mudar, ou utilizam buffers de caracteres reutilizáveis (`char[]` / `SetText` com buffer estático do TextMeshPro).
- **Justificativa**: Concatenação contínua de strings (`ToString() + " m"`) a 60 FPS aloca dezenas de objetos temporários no heap por segundo, gerando pausas bruscas de coleta de lixo (*GC spikes* / engasgos) críticas em dispositivos móveis Android.
- **Alternativas Rejeitadas**:
  - *Atualização cega a cada frame com interpolação de strings*: Gera alocações massivas no heap, violando frontalmente o critério `SC-001` e o Artigo III.4 da Constituição.

---

### 3. Mecanismo de Entrada Contínua e Mapeamento Dual (Toque Mobile + Teclado PC)
- **Decisão**:
  - O apresentador mantém flags internas de comando sustentado (`_estaSubindo`, `_estaDescendo`, `_estaComBoost`).
  - Métodos explícitos de transição: `IniciarSubida()`, `PararSubida()`, `IniciarDescida()`, `PararDescida()`, `IniciarBoost()`, `PararBoost()`.
  - Ao ser consultado pela simulação física via `ObterComandosControle()`, o apresentador sintetiza um `ParametrosControlePiloto` na stack:
    - Pitch: `+1.0f` se apenas subindo, `-1.0f` se apenas descendo, `0.0f` se neutro ou se ambos forem pressionados simultaneamente.
    - Boost: `true` se boost ativo e houver combustível restante.
  - Na visão Unity, botões táteis disparam esses métodos através de `IPointerDownHandler` (início) e `IPointerUpHandler` (parada). Simultaneamente, o loop de `Update()` escuta as teclas do teclado (`Setas Cima/Baixo`, `W`/`S`, `Espaço`) disparando as mesmas transições.
- **Justificativa**: Elimina atrasos de amostragem (latência < 16ms), resolve conflitos de multitoque e simplifica testes no editor sem exigir emulador mobile.
- **Alternativas Rejeitadas**:
  - *Polling analógico contínuo via float*: Adiciona ruído desnecessário para controles discretos arcade (botões subir/descer).

---

### 4. Feedback Visual de Combustível Esgotado e Superação de Recorde
- **Decisão**:
  - **Esgotamento de Combustível**: Quando `voo.Combustivel.EstaVazio == true`, o apresentador desativa o boost imediatamente e comanda `IVisaoHUDVoo.DefinirInteratividadeBoost(false)`. A visão aplica 50% de opacidade (alpha) no botão de Boost e desativa a recepção de toques.
  - **Superação de Recorde**: O apresentador compara `voo.DistanciaPercorrida > _recordeAtual`. No instante exato da superação, invoca `IVisaoHUDVoo.NotificarNovoRecorde()` uma única vez durante o voo. A visão aplica coloração dourada vibrante (`#FFD700`) e animação suave de pulso de escala (1.0 -> 1.25 -> 1.0) no texto do recorde.
- **Justificativa**: Fornece clareza imediata para o jogador familiar e infantil, recompensando a superação da marca e prevenindo frustração por pressionar um botão sem combustível.
- **Alternativas Rejeitadas**:
  - *Pop-up bloqueante de novo recorde*: Obstruiria a visão da trajetória do avião em um momento crítico de pilotagem.

---

### 5. Botão de Pausa e Encerramento da Sessão de Voo
- **Decisão**:
  - **Pausa**: Botão de Pausa discreto no canto superior direito. Ao ser clicado, dispara `ApresentadorHUDVoo.SolicitarPausa()`, que cancela qualquer comando sustentado ativo e emite o evento `event Action? AoSolicitarPausa`.
  - **Fim de Voo (`StatusVoo.Pousado` ou `StatusVoo.Colidido`)**: O apresentador detecta a mudança de estado e invoca `IVisaoHUDVoo.DefinirVisibilidadeControles(false)`. Os botões táteis desaparecem imediatamente da tela, prevenindo toques fantasmas, enquanto a telemetria final permanece visível até a exibição da tela de resultados da Feature 012.
- **Justificativa**: Garante transição suave e desacoplada entre a Feature 011 e a Feature 012 (`012-ui-resumo-fim-voo`).
