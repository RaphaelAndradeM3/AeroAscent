# Pesquisa e Decisões Técnicas: Áudio, Sistema de Partículas e Polimento Geral (Feature 013)

## Contexto e Objetivos

A Feature 013 é responsável por dar vida e acabamento audiovisual ao **AeroAscent**, implementando a camada de efeitos sonoros acolhedores (CC0 / Domínio Público da Kenney.nl), música suave, feedback de partículas visuais de alto desempenho e polimento de performance mobile:
1. **Ambiente Sonoro Acolhedor e Ético (Artigo I e IV da Constituição)**: Sons agradáveis e relaxantes para todas as idades (Ruth, Sofia e Alice), livres de ruídos estridentes ou mecânicas agressivas.
2. **Serviço de Áudio Desacoplado (`IServicoAudio`)**: Interface na camada de aplicação que permite testar toda a lógica de disparos e modulações sonoras via xUnit no .NET 8, sem dependência de hardware de áudio ou da engine Unity.
3. **Controle Dinâmico de Efeitos Contínuos na Stack (`GC Alloc = 0 bytes`)**: Efeitos contínuos de vento (proporcional à velocidade da aeronave) e turbina de propulsão (boost) manipulados via parâmetros na stack com atenuação suave (*fade in/out*).
4. **Polifonia e Modulação Melódica de Coleta de Moedas**: Limite de até 4 vozes simultâneas e acréscimo gradual de pitch (+0.05) para coletas rápidas consecutivas, criando sensação musical de arpeggio sem saturação ou corte brusco de áudio.
5. **Configuração Persistida de Áudio**: Objeto de valor imutável `ConfiguracaoAudio` integrado ao agregado `ProgressoJogador` e persistido em JSON local.
6. **Sistema de Partículas com Object Pooling**: Emissores Shuriken na Unity para rastro de cauda (*trail*), chamas de propulsão, brilho de moedas e confetes comemorativos sem alocações no heap em tempo de execução.

---

## Decisões Arquiteturais e de Design

### 1. Desacoplamento Arquitetural: `IServicoAudio` em `Core.Aplicacao`
- **Decisão**: Criar a interface `IServicoAudio` em `AeroAscent.Core.Aplicacao.Contratos` e o enum `EventoAudio` em `AeroAscent.Core.Dominio.Enums`.
  - O núcleo de regras de negócio e apresentadores injetam `IServicoAudio`.
  - A camada de apresentação Unity implementa `IServicoAudio` através de `ControladorAudioUnity` (`MonoBehaviour` persistente com `AudioSource` organizados por canais).
  - Em testes automatizados xUnit, utiliza-se a fixture `ServicoAudioFalso` (Spy/Mock).
- **Justificativa**: Respeita rigorosamente a Clean Architecture (Artigo III.2 da Constituição). Permite validar 100% das condições de áudio (disparos de efeitos, intensidades de vento, boost e configurações) em milissegundos sem abrir o editor da Unity.
- **Alternativas Rejeitadas**:
  - *Chamar `AudioSource.PlayClipAtPoint` diretamente nos scripts de gameplay*: Violação de Clean Architecture e impossibilita testes unitários no .NET 8.
  - *Bibliotecas externas de áudio (ex: FMOD, Wwise)*: Complexidade excessiva e peso desnecessário para um jogo arcade mobile minimalista.

---

### 2. Loops Contínuos de Vento e Propulsão com Zero Alocação (`GC Alloc = 0 bytes`)
- **Decisão**: 
  - `IServicoAudio` expõe os métodos dedicados `AtualizarLoopVento(float intensidadeNormalizada)` e `DefinirLoopPropulsao(bool ativo, float intensidade = 1f)`.
  - A intensidade trafega como tipo primitivo `float` na stack.
  - A implementação Unity mantém `AudioSource` dedicados com `loop = true` e executa interpolação linear suave (`Mathf.MoveTowards` / `Mathf.Lerp`) no volume e pitch durante o `Update()`, eliminando ruídos de transição (*audio clicks/pops*).
- **Justificativa**: Evita alocações contínuas a 60 FPS, mantendo estabilidade de memória nos limites rígidos de celulares Android (Artigo III.4 da Constituição).
- **Alternativas Rejeitadas**:
  - *Disparar eventos repetitivos de áudio a cada frame*: Causaria saturação de canais e sobrecarga de CPU.

---

### 3. Modulação Procedural de Pitch e Limite de Polifonia na Coleta de Coletáveis
- **Decisão**:
  - Na reprodução de `EventoAudio.ColetaMoeda`, o subsistema calcula a janela de tempo desde a última coleta.
  - Se inferior a 0,3s, incrementa o pitch em `+0.05` até o teto de `+0.30` (gerando um arpeggio musical ascendente). Se o intervalo for superior, reseta o pitch ao padrão `1.0f`.
  - Limita a 4 o número de vozes simultâneas do efeito de moeda, reutilizando o canal mais antigo via *Voice Stealing* suave se excedido.
- **Justificativa**: Transforma a coleta repetitiva de moedas em uma experiência sensorial gratificante e harmoniosa, alinhada à psicologia acolhedora e familiar (Artigo I da Constituição).
- **Alternativas Rejeitadas**:
  - *Canal único reiniciando o som*: Corta o áudio bruscamente, soando amador.
  - *Debounce silencioso (ignorar moedas subsequentes)*: Sensação de perda de feedback tátil e sonoro.

---

### 4. Modelagem e Persistência de `ConfiguracaoAudio` no Domínio
- **Decisão**:
  - Modelar `ConfiguracaoAudio` como `public readonly record struct ConfiguracaoAudio` na camada `Core.Dominio.ObjetosDeValor`.
  - Propriedades: `VolumeEfeitos` (float 0.0 a 1.0), `VolumeMusica` (float 0.0 a 1.0), `EfeitosAtivos` (bool), `MusicaAtiva` (bool).
  - Padrão inicial: `VolumeEfeitos = 0.8f`, `VolumeMusica = 0.7f`, ambos ativos.
  - Integrada diretamente ao agregado `ProgressoJogador`, sendo serializada e persistida no mesmo arquivo JSON local existente via `IRepositorioProgresso`.
- **Justificativa**: Garante atomicidade de gravação e mantém a integridade do estado global do jogador em um único arquivo protegido, com retrocompatibilidade assegurada.
- **Alternativas Rejeitadas**:
  - *Gravação separada em PlayerPrefs*: Dificulta testes unitários no .NET e não centraliza o backup de progresso.

---

### 5. Sistema de Partículas e Otimização Mobile First
- **Decisão**:
  - O `GerenciadorParticulas` mantém pools pré-alocados de instâncias de prefabs Shuriken para efeitos voláteis pontuais (brilho de moeda coletada e explosão de confetes).
  - Para rastro de cauda e chamas de turbina (*boost*), as partículas são componentes fixos na hierarquia do prefab da aeronave, alternando apenas o módulo `emission.enabled = true/false` e variando taxa de emissão proporcional à aceleração.
- **Justificativa**: Respeita o requisito estrito de 60 FPS com alocação zero no loop (Artigo III.4 da Constituição).
- **Alternativas Rejeitadas**:
  - *Instantiate e Destroy em tempo de execução*: Causa pausas frequentes de GC em dispositivos Android.
