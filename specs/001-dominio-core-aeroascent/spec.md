# Feature Specification: Domínio Core, Entidades e Objetos de Valor do AeroAscent

**Feature Branch**: `001-dominio-core-aeroascent`  
**Created**: 2026-09-04  
**Status**: Ready for Planning  
**Input**: User description: "001 - Camada de Domínio C# Puro (.NET Standard), Entidades (Aeronave, Voo, Oficina), Value Objects (Combustivel, Moeda, VetorVoo) e Interfaces Base."

---

## Clarifications

### Session 2026-09-04
- Q: Como a entidade 'Oficina' e o conceito de 'Melhoria' devem ser estruturados no Domínio Core? → A: Modelar 'Oficina' como Entidade (`class`) gerenciando o catálogo e regras de 'Melhoria' (Objeto de Valor `record`), calculando custos de evolução e aplicando melhorias na 'Aeronave'.
- Q: Qual deve ser o limite máximo de nível das melhorias e a reação do domínio ao ultrapassar? → A: Limite máximo fixo de nível 10 para todas as melhorias (`Motor`, `Aerodinamica`, `TanqueCombustivel`, `Catapulta`), lançando a exceção customizada `MelhoriaNivelMaximoException` caso haja tentativa de evolução além desse teto.
- Q: Como a interface IRepositorioProgresso deve estruturar os contratos de persistência? → A: Modelar o agregado `ProgressoJogador` (unificando `Aeronave`, saldo de `Moeda` e recordes de distância e altitude), persistido atomicamente via `SalvarProgressoAsync` e `CarregarProgressoAsync` com suporte a `CancellationToken`.
- Q: Como o encerramento da sessao de voo e a consolidacao dos dados devem ser modelados na entidade Voo? → A: Expandir `StatusVoo` para incluir `Cancelado` (além de `EmPreparacao`, `EmVoo`, `Pousado`) e gerar o Objeto de Valor imutável `ResultadoVoo` (`record`) ao transitar para `Pousado`, calculando o bônus de moedas pela fórmula do PRD: $\lfloor \text{Distância} \times 0.1 \rfloor + \lfloor \text{Altitude} \times 0.05 \rfloor + \text{Moedas Coletadas}$.
- Q: Como o Objeto de Valor VetorVoo deve ser estruturado (dimensoes e precisao)? → A: Tridimensional 3D com componentes `float` (`X`, `Y`, `Z`) e operações imutáveis puras (soma, subtração, multiplicação escalar, magnitude e normalização), assegurando interoperabilidade de alto desempenho com o ecossistema .NET MAUI (Windows e Android) sem custo de conversão de tipos ou alocação excessiva de memória.

---

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Modelagem e Inicialização da Aeronave (Priority: P1)

Como jogador iniciando o jogo, o sistema deve instanciar uma aeronave configurada com atributos e níveis padrão de motor, aerodinâmica, tanque de combustível e catapulta para que todos os voos partam de um estado de domínio válido e consistente.

**Why this priority**: A entidade `Aeronave` é a raiz de agregação do domínio do jogo. Sem ela, nenhuma mecânica de voo, upgrade ou persistência pode operar.

**Independent Test**: Pode ser testado de forma 100% isolada via testes unitários C# verificando a criação de uma `Aeronave` com identificador único (`Guid`), níveis iniciais iguais a 1 e propriedades consistentes.

**Acceptance Scenarios**:
1. **Given** que uma nova sessão de jogo é criada sem dados anteriores, **When** a aeronave padrão for instanciada, **Then** ela deve possuir `Id` válido, nível de motor 1, aerodinâmica nível 1, tanque nível 1 e catapulta nível 1.
2. **Given** uma aeronave existente, **When** for solicitada a alteração de um nível para um valor menor que 1, **Then** o domínio deve lançar uma exceção de validação (`ArgumentOutOfRangeException` ou `DominioInvalidoException`) e impedir a alteração de estado.

---

### User Story 2 - Sessão de Voo e Atualização de Estado (Priority: P2)

Como sistema de simulação, devo poder iniciar uma sessão de `Voo` vinculada a uma `Aeronave`, rastrear distância percorrida, altitude máxima e moedas coletadas, e transitar o status de voo de forma segura (`EmPreparacao`, `EmVoo`, `Pousado`).

**Why this priority**: Permite que os casos de uso de voo e finalização trabalhem sobre uma entidade rica que encapsula regras de transição de estado e métricas.

**Independent Test**: Testável via testes unitários criando uma sessão de `Voo`, atualizando distância/altitude em tempo real e validando a máquina de estados interna.

**Acceptance Scenarios**:
1. **Given** um voo no status `EmPreparacao`, **When** a decolagem for confirmada, **Then** o status transita para `EmVoo`.
2. **Given** um voo no status `EmVoo`, **When** novas coordenadas e métricas forem registradas, **Then** a distância acumulada e a altitude máxima são atualizadas mantendo o maior valor histórico de altitude daquela sessão.
3. **Given** um voo no status `EmVoo`, **When** a aeronave pousar e o método de finalização for invocado, **Then** o status transita para `Pousado`, um `ResultadoVoo` imutável com a pontuação e bônus calculados é gerado e tentativas posteriores de atualizar coordenadas são rejeitadas.
4. **Given** um voo ativo no status `EmPreparacao` ou `EmVoo`, **When** o jogador abortar o voo, **Then** o status transita para `Cancelado` e nenhuma recompensa ou atualização de recorde é computada.

---

### User Story 3 - Operações Monetárias e Combustível com Objetos de Valor Imutáveis (Priority: P3)

Como regra de economia e física, as moedas e o combustível devem ser representados por objetos de valor imutáveis (`record` em C#), garantindo que saldos negativos e consumos inválidos sejam impossíveis por construção.

**Why this priority**: Garante integridade matemática absoluta no balanceamento econômico e na queima de combustível sem risco de estados mutáveis corrompidos.

**Independent Test**: Testável via testes unitários somando, subtraindo e comparando instâncias de `Moeda` e `Combustivel`.

**Acceptance Scenarios**:
1. **Given** um saldo de 50 moedas, **When** tentar subtrair 100 moedas, **Then** o sistema deve lançar `SaldoInsuficienteException` e não permitir saldo negativo.
2. **Given** um tanque com 20 unidades de combustível, **When** for consumido 5 unidades, **Then** uma nova instância imutável de `Combustivel` é retornada com 15 unidades restantes e percentual recalculado.
3. **Given** dois vetores de voo tridimensionais com componentes `float` (`VetorVoo(10f, 0f, 5f)` e `VetorVoo(5f, 20f, 0f)`), **When** for executada a soma vetorial ou cálculo de magnitude, **Then** o sistema deve retornar uma nova instância imutável com os cálculos exatos sem dependência de bibliotecas externas.

---

### User Story 4 - Gestão de Oficina e Evolução de Melhorias (Priority: P2)

Como jogador progredindo no jogo, desejo acessar a `Oficina` para consultar o custo escalonado das melhorias disponíveis e aplicar evoluções aos componentes da `Aeronave` utilizando minhas `Moedas`, para que o desempenho do meu avião aumente progressivamente.

**Why this priority**: A `Oficina` viabiliza o loop econômico central do jogo (ganhar moedas em voo -> evoluir na oficina -> voar mais longe).

**Independent Test**: Testável de forma 100% isolada via testes unitários C# verificando cálculo de custo exponencial e aplicação de melhoria com débito de moedas.

**Acceptance Scenarios**:
1. **Given** uma `Oficina`, uma `Aeronave` com nível de motor 1 e saldo de 100 moedas, **When** for solicitada a melhoria de motor com custo de 50 moedas, **Then** o nível de motor da aeronave é elevado para 2 e o saldo de moedas resultante passa a ser 50.
2. **Given** uma tentativa de evolução cujo custo seja superior ao saldo disponível de moedas, **When** a melhoria for solicitada, **Then** o domínio deve lançar `SaldoInsuficienteException` e o estado da aeronave e das moedas deve permanecer inalterado.
3. **Given** uma `Aeronave` com motor no nível máximo 10, **When** for solicitada nova melhoria de motor na `Oficina`, **Then** o sistema deve lançar `MelhoriaNivelMaximoException` e não permitir alteração de nível nem debitar moedas.

---

### User Story 5 - Agregação e Persistência do Progresso do Jogador (Priority: P2)

Como jogador, desejo que meu progresso global (aeronave configurada, saldo total de moedas e recordes históricos de voo) seja consolidado sob a raiz de agregação `ProgressoJogador` e manipulado pelo contrato `IRepositorioProgresso`, para que os dados sejam preservados com integridade atômica entre sessões.

**Why this priority**: Evita inconsistências de estado e corrupção parcial ao persistir entidades separadamente, oferecendo um contrato coeso para a camada de infraestrutura.

**Independent Test**: Testável via testes unitários C# simulando o salvamento e carregamento atômico de instâncias de `ProgressoJogador`.

**Acceptance Scenarios**:
1. **Given** uma instância inicial de `ProgressoJogador` com aeronave padrão, saldo zerado e recordes nulos, **When** o método `SalvarProgressoAsync` for executado, **Then** o agregado completo deve ser fornecido para persistência sem erros.
2. **Given** um progresso existente com recorde de distância de 150m, **When** um novo voo registrar 200m, **Then** o método de atualização do `ProgressoJogador` substitui o recorde anterior por 200m de forma imutável/segura.

---

### Edge Cases

- Tentativa de instanciar `Aeronave`, `Voo` ou `Oficina` com `Guid.Empty` deve falhar imediatamente.
- Registro de distância ou altitude negativa deve ser rejeitado pelas invariantes do domínio.
- Operações aritméticas com `Moeda` que possam causar overflow numérico devem ser protegidas.
- Tentativa de elevar qualquer nível de melhoria acima do limite máximo 10 deve lançar `MelhoriaNivelMaximoException`.
- Tentativa de definir qualquer nível de melhoria inferior a 1 deve lançar `ArgumentOutOfRangeException`.

---

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: O sistema DEVE definir a entidade `Aeronave` em C# puro (.NET Standard) com identificador `Guid Id`, níveis numéricos inteiros no intervalo fixo de 1 a 10 para `Motor`, `Aerodinamica`, `TanqueCombustivel` e `Catapulta`.
- **FR-002**: O sistema DEVE definir a entidade `Voo` contendo `Id`, referência à `Aeronave`, `StatusVoo` (`EmPreparacao`, `EmVoo`, `Pousado`, `Cancelado`), distância percorrida, altitude máxima, moedas coletadas na sessão e método de finalização que gera o Objeto de Valor `ResultadoVoo`.
- **FR-003**: O sistema DEVE implementar o Objeto de Valor `Moeda` como `record` imutável, com métodos de adição, subtração e validação contra valores negativos.
- **FR-004**: O sistema DEVE implementar o Objeto de Valor `Combustivel` como `record` imutável com capacidade máxima, quantidade atual, taxa de queima e cálculo de percentual restante.
- **FR-005**: O sistema DEVE implementar o Objeto de Valor `VetorVoo` como `record` imutável tridimensional composto por `float X`, `float Y` e `float Z`, oferecendo operações vetoriais imutáveis (soma, subtração, multiplicação por escalar, magnitude e normalização) com zero acoplamento a bibliotecas gráficas ou motores externos.
- **FR-006**: O sistema DEVE definir a interface `IRepositorioProgresso` contendo os métodos `SalvarProgressoAsync(ProgressoJogador progresso, CancellationToken cancelamento)` e `CarregarProgressoAsync(CancellationToken cancelamento)`, além da entidade agregada `ProgressoJogador` (contendo `Guid Id`, `Aeronave`, `Moeda` saldo total, e recordes de distância/altitude) e os contratos de serviços do domínio (`IServicoFisicaVoo`, `IServicoEconomia`).
- **FR-007**: Todo o código desta camada DEVE estar em Português Brasileiro (pt-BR) e possuir zero referências a frameworks de interface (como .NET MAUI) ou engines gráficas.
- **FR-008**: O sistema DEVE implementar a entidade `Oficina` em C# puro (.NET Standard) com identificador `Guid Id`, responsável por gerenciar as regras de evolução e catálogo de `Melhoria`, calculando custos exponenciais, validando o teto de nível 10 com `MelhoriaNivelMaximoException` e aplicando atualizações de nível na `Aeronave`.
- **FR-009**: O sistema DEVE implementar o Objeto de Valor `Melhoria` como `record` imutável, definindo o tipo (`TipoMelhoria`: `Motor`, `Aerodinamica`, `TanqueCombustivel`, `Catapulta`), nível atual (1 a 10), multiplicador de eficácia e fórmula de cálculo de custo.
- **FR-010**: O sistema DEVE implementar o Objeto de Valor `ResultadoVoo` como `record` imutável, gerado ao transitar o voo para `Pousado`, contendo distância percorrida, altitude máxima atingida, moedas coletadas em voo e moedas de recompensa total calculadas pela fórmula: $\lfloor \text{Distância} \times 0.1 \rfloor + \lfloor \text{Altitude} \times 0.05 \rfloor + \text{Moedas Coletadas}$.

### Key Entities

- **`ProgressoJogador`**: Raiz de agregação que consolida o estado global e persistível do jogador (identificador `Id`, `Aeronave`, carteira de `Moeda` e recordes de distância e altitude).
- **`Aeronave`**: Entidade central representativa do avião do jogador e de suas configurações de peças mecânicas.
- **`Voo`**: Entidade que encapsula a sessão ativa de lançamento, trajetória, coleta de recursos, transições de estado e geração do encerramento.
- **`ResultadoVoo`**: Objeto de valor imutável gerado na conclusão do voo, consolidando distância, altitude, moedas coletadas e recompensas calculadas.
- **`Oficina`**: Entidade responsável por gerenciar o catálogo de melhorias disponíveis, o cálculo de custos exponenciais e a aplicação de evoluções na `Aeronave`.
- **`Melhoria`**: Objeto de valor que representa uma especificação de upgrade mecânico aplicável à aeronave, com tipo, nível e multiplicador.
- **`Moeda`**: Objeto de valor que representa o capital financeiro do jogador.
- **`Combustivel`**: Objeto de valor que representa a energia propelente armazenada.
- **`VetorVoo`**: Objeto de valor tridimensional (`float X, Y, Z`) que expressa posições, acelerações e velocidades puras com métodos matemáticos imutáveis.

---

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Cobertura de testes unitários de 100% sobre todas as entidades, objetos de valor e regras de validação do domínio.
- **SC-002**: Tempo de execução de toda a suíte de testes do domínio inferior a 500 milissegundos.
- **SC-003**: Zero acoplamento ou dependência externa de bibliotecas gráficas ou motores de jogo no assembly de domínio.
- **SC-004**: Todas as operações de criação e cálculo executadas sem alocação residual de memória.

---

## Assumptions

- O domínio será compilado em biblioteca de classes C# (.NET Standard 2.1 / .NET 8), consumível nativamente pelo projeto .NET MAUI em Windows e Android.
- Todas as unidades de medida físicas adotadas no domínio seguem o Sistema Internacional (metros, metros por segundo, graus).
- A identificação única global (`Guid`) garante rastreabilidade e suporte a sincronização offline futura sem conflitos.
