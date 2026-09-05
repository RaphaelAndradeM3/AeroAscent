# Research: Feature 005 — Sistema de Coletáveis em Voo e Object Pooling

## 🔬 Visão Geral de Pesquisa
Esta pesquisa consolida as decisões arquiteturais e padrões de engenharia para o sistema de coletáveis aéreos (**Moedas Flutuantes** e **Anéis de Vento / Air Boost Rings**) e a infraestrutura de **Object Pooling** de alta performance na camada de Domínio e Aplicação em C# puro (.NET Standard 2.1 e .NET 8), atendendo estritamente aos mandatos da Constituição do projeto:
- **Artigo I**: Experiência ética, familiar e acolhedora sem monetização predatória.
- **Artigo II**: Física vetorial transparente e ausência de barreiras artificiais.
- **Artigo III.1**: Idioma 100% em Português Brasileiro (pt-BR) e convenções C# .NET.
- **Artigo III.2**: Clean Architecture pura (zero dependências de `UnityEngine`).
- **Artigo III.4**: Performance Mobile First com **Zero Alocação no Heap (`GC Alloc = 0 bytes`)** e Object Pooling para todos os elementos dinâmicos.

---

## 📐 Decisões Técnicas Principais

### D1: Infraestrutura de Object Pooling Puro em C# (`IPoolObjetos<T>` e `GerenciadorPoolObjetos<T>`)
- **Problema**: A Unity possui `UnityEngine.Pool.ObjectPool<T>`, porém a camada de Domínio e Aplicação do AeroAscent é C# puro (.NET Standard 2.1 / .NET 8) e não pode conter referências à engine Unity. Além disso, no loop contínuo de 60 FPS, chamadas a `new` geram coletas de lixo (*GC Stutter*) que degradam a experiência mobile (Android).
- **Decisão**: Criar a interface `IPoolObjetos<T>` e a classe genérica `GerenciadorPoolObjetos<T>` na camada de Domínio (`AeroAscent.Core.Dominio.Comum` ou `Infraestrutura`).
  - Implementação fundamentada em array pré-alocado e ponteiro de topo de pilha ou `Stack<T>` pré-dimensionada.
  - Métodos $O(1)$: `T Obter()`, `void Liberar(T item)` e `void Limpar()`.
  - Pré-alocação padrão: 50 moedas e 15 anéis de vento.
  - Expansão elástica de segurança: caso a demanda exceda a capacidade inicial por um pico atípico, instancia um novo objeto de forma segura sem lançar exceção fatal ou travar o jogo.
- **Alternativas Rejeitadas**:
  - *Usar `UnityEngine.Pool.ObjectPool<T>`*: Rejeitada por violar o Artigo III.2 (Clean Architecture pura no Core).
  - *Instanciação e Destruição Dinâmica (`new` / `Destroy`)*: Rejeitada categoricamente pelo Artigo III.4 e SC-001 (`GC Alloc = 0 bytes`).

---

### D2: Modelo de Dados dos Coletáveis no Plano $Y-Z$
- **Problema**: Padronizar as propriedades espaciais e estados de ciclo de vida dos coletáveis alinhados à física do jogo.
- **Decisão**:
  - `TipoColetavel`: Enum com valores `Moeda = 1` e `AnelVento = 2`.
  - `Coletavel`: Classe de entidade no Domínio com:
    - `Guid Id` único para rastreabilidade;
    - `TipoColetavel Tipo`;
    - `VetorVoo Posicao` (onde $X = 0$, $Y = \text{altitude em metros}$ e $Z = \text{avanço horizontal em metros}$);
    - `float RaioColetaMetros` ($1.5\text{m}$ para moedas e $3.5\text{m}$ para anéis de vento);
    - `bool Ativo` (indica se está em tela apto a colidir);
    - `bool Coletado` (indica se já foi capturado na sessão atual).
  - Método de teste de colisão em $O(1)$ baseado na distância euclidiana ao quadrado no plano $Y-Z$ para evitar cálculos caros de raiz quadrada:
    $$(Y_{\text{aero}} - Y_{\text{col}})^2 + (Z_{\text{aero}} - Z_{\text{col}})^2 \le (R_{\text{aero}} + R_{\text{col}})^2$$
- **Alternativas Rejeitadas**:
  - *Checagem baseada em colliders físicos pesados da Unity no Core*: Rejeitada por dependência externa.
  - *Coletáveis como structs imutáveis puras sem ID*: Rejeitada para permitir ciclo de ativação e devolução ao pool por referência de instância controlada.

---

### D3: Dinâmica de Impulso do Anel de Vento (*Air Boost Ring*)
- **Problema**: Como aplicar o impulso do anel sem causar solavancos ou quebras de trajetória no gameplay familiar.
- **Decisão**:
  - Ao colidir com o anel de vento, aplicar um acréscimo escalar fixo de **$+10.0\text{ m/s}$** projetado na direção do vetor unitário de velocidade da aeronave ($\vec{V} / |\vec{V}|$).
  - Caso a velocidade seja muito baixa ($|\vec{V}| < 0.5\text{ m/s}$), projetar os $+10.0\text{ m/s}$ na direção do bico (ângulo de pitch $\theta$).
  - Nenhum combustível é debitado do reservatório de voo.
  - Retornar o novo `EstadoFisicoAeronave` atualizado diretamente na stack.
- **Alternativas Rejeitadas**:
  - *Multiplicação percentual ($\times 1.35$)*: Rejeitada por penalizar aeronaves lentas e gerar velocidades excessivas em aeronaves já rápidas.
  - *Impulso apenas horizontal ($Z$)*: Rejeitada por anular comandos verticais intencionais do jogador.

---

### D4: Geração Procedural em Janela Espacial e Reciclagem Automática
- **Problema**: Como posicionar os coletáveis de forma equilibrada sem carregar o mundo inteiro na memória.
- **Decisão**:
  - Serviço `ServicoGeracaoProceduralColetaveis`:
    - Janela ativa de geração: entre **$+30\text{ m}$ e $+150\text{ m}$** à frente da posição $Z$ atual da aeronave.
    - Faixa de altitude permitida: entre **$5\text{ m}$ e $120\text{ m}$** acima do solo ($Y=0$).
    - Reciclagem automática de coletáveis: qualquer item cujo $Z < Z_{\text{aeronave}} - 20\text{ m}$ é desativado e devolvido imediatamente ao pool (SC-003).
    - Gerador pseudo-randômico com semente determinística (`Seed`) na sessão de voo para testes reproduzíveis.
- **Alternativas Rejeitadas**:
  - *Listas estáticas hardcoded*: Rejeitada por não permitir voos longos e progressão infinita.
  - *Spawn contínuo sem reciclagem*: Rejeitada por causar vazamento de memória e estouro do pool.

---

### D5: Orquestração Limpa de Aplicação (`ProcessarColetaveisVooCasoDeUso`)
- **Problema**: Onde orquestrar a detecção de colisão, entrega de moedas à entidade `Voo`, aplicação de impulsos e reciclagem no pool.
- **Decisão**:
  - Criar o caso de uso `ProcessarColetaveisVooCasoDeUso` implementando `IProcessarColetaveisVooCasoDeUso`.
  - Executa a cada frame/passo de simulação:
    1. Executa o teste de proximidade contra os coletáveis ativos na janela.
    2. Em caso de colisão com moeda: incrementa `voo.AtualizarMetricas(..., moedasNovas: 1)` e recicla a moeda.
    3. Em caso de colisão com anel de vento: aplica o impulso vetorial no `EstadoFisicoAeronave` e recicla o anel.
    4. Recicla coletáveis que ficaram para trás ($Z < Z_{\text{aeronave}} - 20\text{ m}$).
    5. Spawna novos coletáveis à frente para manter a densidade planejada.
- **Alternativas Rejeitadas**:
  - *Acoplar a lógica de coletáveis diretamente dentro de `ServicoFisicaVoo`*: Rejeitada por violar o Princípio da Responsabilidade Única (SRP). O serviço de física cuida de forças aerodinâmicas; o caso de uso de coletáveis orquestra elementos de cenário e pontuação.
