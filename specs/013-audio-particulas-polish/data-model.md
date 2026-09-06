# Modelo de Dados: Áudio, Partículas e Configurações (Feature 013)

## Visão Geral

Este documento detalha as estruturas de dados, enumerações, objetos de valor e extensões ao agregado do domínio necessários para suportar o subsistema de áudio, sistema de partículas e preferências audiovisuais do jogo, operando com alocação zero no heap (`GC Alloc = 0 bytes`).

---

## 1. Enumeração de Eventos Sonoros (`EventoAudio`)

Localizada em `AeroAscent.Core.Dominio.Enums`:

```csharp
namespace AeroAscent.Core.Dominio.Enums;

/// <summary>
/// Catálogo tipado dos eventos sonoros discretos e gatilhos da experiência audiovisual de AeroAscent.
/// </summary>
public enum EventoAudio
{
    /// <summary>Disparo da aeronave na rampa de lançamento inicial.</summary>
    LancamentoCatapulta = 1,

    /// <summary>Efeito sonoro de vento aerodinâmico durante o voo livre.</summary>
    VooVento = 2,

    /// <summary>Acionamento do propulsor de aceleração (boost) da aeronave.</summary>
    PropulsorBoost = 3,

    /// <summary>Coleta de moeda dourada no ar.</summary>
    ColetaMoeda = 4,

    /// <summary>Transposição bem-sucedida de um anel acelerador de vento.</summary>
    PassagemAnelVento = 5,

    /// <summary>Toque suave da aeronave no solo durante o pouso seguro.</summary>
    PousoSuave = 6,

    /// <summary>Celebração comemorativa de superação de novo recorde pessoal.</summary>
    NovoRecorde = 7,

    /// <summary>Feedback tátil/sonoro de interação com botões de interface.</summary>
    CliqueBotao = 8,

    /// <summary>Confirmação de aquisição de melhoria ou item na oficina mecânica.</summary>
    CompraOficina = 9,

    /// <summary>Impacto ou perda abrupta de sustentação ao colidir no solo.</summary>
    ColisaoSolo = 10
}
```

---

## 2. Objeto de Valor: Preferências de Áudio (`ConfiguracaoAudio`)

Localizado em `AeroAscent.Core.Dominio.ObjetosDeValor`:

```csharp
namespace AeroAscent.Core.Dominio.ObjetosDeValor;

/// <summary>
/// Objeto de valor imutável alocado na stack (<c>readonly record struct</c>, <c>GC Alloc = 0 bytes</c>)
/// que encapsula as preferências de volume e canais sonoros (SFX e Música) do jogador.
/// </summary>
public readonly record struct ConfiguracaoAudio
{
    /// <summary>Volume dos efeitos sonoros (SFX) normalizado entre 0.0f (silencioso) e 1.0f (máximo).</summary>
    public float VolumeEfeitos { get; }

    /// <summary>Volume da trilha musical normalizado entre 0.0f (silencioso) e 1.0f (máximo).</summary>
    public float VolumeMusica { get; }

    /// <summary>Sinaliza se os efeitos sonoros estão habilitados.</summary>
    public bool EfeitosAtivos { get; }

    /// <summary>Sinaliza se a música de fundo está habilitada.</summary>
    public bool MusicaAtiva { get; }

    /// <summary>Configuração padrão recomendada: SFX 80%, Música 70%, ambos ativos.</summary>
    public static readonly ConfiguracaoAudio Padrao = new(0.8f, 0.7f, true, true);
}
```

### Regras de Validação e Invariantes
1. `VolumeEfeitos` e `VolumeMusica` devem ser estritamente contidos no intervalo `[0.0f, 1.0f]`. Caso contrário, lança `DominioInvalidoException`.
2. Métodos de transição imutável:
   - `ComVolumeEfeitos(float novoVolume)`: retorna nova instância com volume de efeitos ajustado.
   - `ComVolumeMusica(float novoVolume)`: retorna nova instância com volume de música ajustado.
   - `AlternarEfeitos()`: inverte o estado da flag `EfeitosAtivos`.
   - `AlternarMusica()`: inverte o estado da flag `MusicaAtiva`.

---

## 3. Extensão do Agregado `ProgressoJogador`

O agregado raiz `ProgressoJogador` é estendido com a propriedade:

```csharp
/// <summary>
/// Preferências audiovisuais configuradas pelo jogador, persistidas no mesmo documento de estado.
/// </summary>
public ConfiguracaoAudio ConfiguracaoAudio { get; private set; }
```

### Métodos de Negócio
- `AtualizarConfiguracaoAudio(ConfiguracaoAudio novaConfiguracao)`: Atualiza o estado interno do agregado e garante que alterações sejam persistidas via `IRepositorioProgresso`.
- **Retrocompatibilidade**: O construtor e a fábrica `CriarNovo()` inicializam `ConfiguracaoAudio` com `ConfiguracaoAudio.Padrao` caso registros legados não possuam a seção correspondente no JSON.

---

## 4. Topologia de Canais de Áudio e Polifonia

```mermaid
graph TD
    A[IServicoAudio] --> B[Canal Música Ambiente]
    A --> C[Canal Loop Vento]
    A --> D[Canal Loop Boost]
    A --> E[Pool de Efeitos SFX]

    B --> B1[AudioSource Música: Loop, Fade In/Out]
    C --> C1[AudioSource Vento: Volume = f(Velocidade)]
    D --> D1[AudioSource Boost: Ativo/Inativo, Fade Suave]
    E --> E1[Voz 1: Coleta Moeda + Pitch Modulado]
    E --> E2[Voz 2: Coleta Moeda]
    E --> E3[Voz 3: Interface / Pouso]
    E --> E4[Voz 4: Fanfarra Recorde / Catapulta]
```
