# Modelo de Dados de Apresentação: Interface HUD de Voo e Controles Táteis (Feature 011)

## Visão Geral

Os modelos de dados de apresentação para o HUD de voo foram projetados para garantir **zero alocação no heap (`GC Alloc = 0 bytes`)** no loop contínuo de simulação a 60 FPS, desacoplamento absoluto da Unity Engine e testabilidade direta através de testes unitários xUnit no .NET 8.

---

## Estruturas de Dados

### 1. `TelemetriaHUDDTO` (Dados de Voo na Stack)

Estrutura imutável (`readonly record struct`) passada preferencialmente por referência (`in TelemetriaHUDDTO`) que consolida todas as informações necessárias para atualização dos marcadores visuais do HUD a cada passo de tempo.

| Campo | Tipo | Descrição | Exemplo |
|---|---|---|---|
| `DistanciaPercorridaMetros` | `float` | Distância horizontal percorrida em metros desde a catapulta. | `125.4f` |
| `RecordeDistanciaMetros` | `float` | Melhor marca histórica a ser superada na sessão. | `200.0f` |
| `AltitudeAtualMetros` | `float` | Altitude instantânea da aeronave em relação ao nível do solo. | `45.2f` |
| `VelocidadeAtualMetrosPorSegundo` | `float` | Módulo da velocidade vetorial da aeronave em metros por segundo. | `28.5f` |
| `PercentualCombustivel` | `float` | Fração normalizada de combustível no tanque (0.0f a 1.0f). | `0.75f` (75%) |
| `MoedasColetadas` | `int` | Total de moedas coletadas durante a sessão de voo atual. | `8` |
| `RecordeSuperado` | `bool` | Flag indicando se a distância atual ultrapassou o recorde da partida. | `false` |
| `BoostDisponivel` | `bool` | Indica se há combustível suficiente e aeronave no ar para acionar propulsor. | `true` |

#### Invariantes e Regras de Validação:
- `DistanciaPercorridaMetros >= 0f`
- `RecordeDistanciaMetros >= 0f`
- `AltitudeAtualMetros >= 0f`
- `VelocidadeAtualMetrosPorSegundo >= 0f`
- `PercentualCombustivel` mantido estritamente no intervalo `[0.0f, 1.0f]` via `Math.Clamp`
- `MoedasColetadas >= 0`

---

### 2. `EstadoControlesHUD` (Estado Interno de Entrada do Apresentador)

Mantido privadamente no `ApresentadorHUDVoo` para sintetizar a estrutura de domínio `ParametrosControlePiloto` sem gerar alocações no heap.

| Campo Interno | Tipo | Descrição |
|---|---|---|
| `_estaSubindo` | `bool` | `true` enquanto o botão/tecla de inclinação para cima estiver pressionado. |
| `_estaDescendo` | `bool` | `true` enquanto o botão/tecla de inclinação para baixo estiver pressionado. |
| `_estaComBoost` | `bool` | `true` enquanto o botão/tecla de Boost estiver pressionado. |
| `_estaPausado` | `bool` | `true` se o jogo estiver com a simulação pausada pelo botão de pausa do HUD. |
| `_recordeNotificado` | `bool` | Evita disparar o evento de celebração de novo recorde mais de uma vez por voo. |

---

## Diagrama de Relacionamento e Fluxo Arquitetural

```mermaid
classDiagram
    class ApresentadorHUDVoo {
        -IVisaoHUDVoo _visao
        -float _recordeAtual
        -bool _estaSubindo
        -bool _estaDescendo
        -bool _estaComBoost
        -bool _estaPausado
        -bool _recordeNotificado
        +event Action AoSolicitarPausa
        +void Inicializar(float recordeInicial)
        +void Atualizar(Voo voo, in EstadoFisicoAeronave estadoFisico)
        +ParametrosControlePiloto ObterComandosControle()
        +void IniciarSubida()
        +void PararSubida()
        +void IniciarDescida()
        +void PararDescida()
        +void IniciarBoost()
        +void PararBoost()
        +void SolicitarPausa()
        +bool EstaPausado
    }

    class IVisaoHUDVoo {
        <<interface>>
        +AtualizarTelemetria(in TelemetriaHUDDTO telemetria)
        +DefinirInteratividadeBoost(bool disponivel)
        +NotificarNovoRecorde()
        +DefinirVisibilidadeControles(bool visivel)
        +event Action AoSolicitarSubida
        +event Action AoInterromperSubida
        +event Action AoSolicitarDescida
        +event Action AoInterromperDescida
        +event Action AoSolicitarBoost
        +event Action AoInterromperBoost
        +event Action AoSolicitarPausa
    }

    class TelemetriaHUDDTO {
        <<readonly record struct>>
        +float DistanciaPercorridaMetros
        +float RecordeDistanciaMetros
        +float AltitudeAtualMetros
        +float VelocidadeAtualMetrosPorSegundo
        +float PercentualCombustivel
        +int MoedasColetadas
        +bool RecordeSuperado
        +bool BoostDisponivel
    }

    class ControladorHUDVoo {
        <<MonoBehaviour>>
        -TextMeshProUGUI _textoDistancia
        -TextMeshProUGUI _textoRecorde
        -TextMeshProUGUI _textoAltitude
        -TextMeshProUGUI _textoVelocidade
        -TextMeshProUGUI _textoMoedas
        -Image _barraCombustivel
        -CanvasGroup _grupoBotaoBoost
        -GameObject _painelControles
    }

    ApresentadorHUDVoo ..> IVisaoHUDVoo : Comanda Atualização Visual
    IVisaoHUDVoo ..> ApresentadorHUDVoo : Notifica Toques e Teclado
    ApresentadorHUDVoo ..> TelemetriaHUDDTO : Cria e despacha na stack
    ControladorHUDVoo ..|> IVisaoHUDVoo : Implementa no Unity Canvas
```

---

## Máquina de Estados de Apresentação do HUD

```mermaid
stateDiagram-v2
    [*] --> AguardandoDecolagem : Sessão de voo criada
    AguardandoDecolagem --> AtivoEmVoo : Catapulta dispara aeronave
    
    state AtivoEmVoo {
        [*] --> Normal
        Normal --> NovoRecordeAlcancado : Distancia > Recorde
        NovoRecordeAlcancado --> Normal : Pulso de escala e cor dourada
        
        Normal --> CombustivelEsgotado : Combustivel.EstaVazio
        CombustivelEsgotado --> Normal : (Se recarregado por power-up)
    }

    AtivoEmVoo --> Pausado : Clique em Pausa
    Pausado --> AtivoEmVoo : Retomar jogo
    
    AtivoEmVoo --> VooConcluido : Pouso ou Colisao
    state VooConcluido {
        [*] --> OcultarControles
        OcultarControles --> CongelarTelemetriaFinal
    }
    
    VooConcluido --> [*] : Transição para Feature 012 (Resumo Fim de Voo)
```
