# Guia Rápido de Validação (Quickstart): Simulação de Física Aerodinâmica e Controle de Pitch

> **Feature**: `003-fisica-voo-aerodinamica`  
> **Status**: Concluído (Fase 1)  
> **Idioma**: Português Brasileiro (pt-BR)  

Este guia detalha os cenários de teste executáveis para validar a implementação da física aerodinâmica, sustentação, arrasto, controle de arfagem/pitch, estol suave e atrito no solo.

---

## 1. Pré-Requisitos e Ambiente de Execução

- **SDK**: .NET 8.0+ instalado.
- **Repositório**: Diretório raiz do projeto AeroAscent clonado.
- **Branch Ativa**: `003-fisica-voo-aerodinamica`.

### Comando de Restauração e Compilação
```bash
dotnet build AeroAscent.sln
```

### Comando Geral de Execução de Testes
```bash
dotnet test --logger "console;verbosity=detailed"
```

---

## 2. Cenários de Validação Físico-Cinemática

### Cenário 1: Ganho de Sustentação ao Inclinar Nariz para Cima (Pitch Up)
- **Objetivo**: Provar que o controle de pitch positivo gera sustentação vertical proporcional ao ângulo de ataque $\alpha$.
- **Entrada**:
  - Posição inicial: `(0, 50, 0)` metros.
  - Velocidade inicial: `(0, 0, 25)` m/s horizontal.
  - Comando do piloto: `ParametrosControlePiloto.Criar(1.0f)` (subindo nariz).
- **Ação**: Executar simulação por 1 segundo ($dt = 0.02\text{s}$, 50 passos).
- **Resultado Esperado**:
  - `InclinacaoPitchGraus` aumenta suavemente até $\approx 45^\circ$.
  - Força resultante de sustentação possui componente vertical positiva significativa ($F_y > 0$).
  - Velocidade vertical $V_y$ transita para valores positivos (aeronave sobe).
  - Velocidade horizontal $V_z$ diminui ligeiramente devido ao arrasto induzido.

---

### Cenário 2: Ganho de Velocidade em Mergulho (Pitch Down)
- **Objetivo**: Provar a conversão de energia potencial gravitacional em velocidade cinética em mergulho.
- **Entrada**:
  - Posição inicial: `(0, 100, 0)` metros.
  - Velocidade inicial: `(0, 0, 15)` m/s horizontal.
  - Comando do piloto: `ParametrosControlePiloto.Criar(-1.0f)` (mergulhando nariz).
- **Ação**: Executar simulação por 1.5 segundos ($dt = 0.02\text{s}$, 75 passos).
- **Resultado Esperado**:
  - `InclinacaoPitchGraus` diminui até o piso de $-45^\circ$.
  - Velocidade escalar total ($|\vec{V}|$) aumenta progressivamente impulsionada pela gravidade ($g = 9.81\text{ m/s}^2$).

---

### Cenário 3: Redução de Arrasto por Melhoria de Aerodinâmica
- **Objetivo**: Provar que aeronave nível 5 na oficina viaja mais longe que nível 1 com o mesmo impulso.
- **Entrada**:
  - Duas simulações partindo de impulso da catapulta a $25\text{ m/s}$ a $35^\circ$ com comandos neutros.
  - Teste A: `Aeronave` com nível de aerodinâmica 1.
  - Teste B: `Aeronave` com nível de aerodinâmica 5.
- **Ação**: Simular ambas as aeronaves até o primeiro toque no solo ($Y \le 0$).
- **Resultado Esperado**:
  - O coeficiente de arrasto efetivo do Teste B é $44\%$ menor ($1 / 1.8$).
  - A distância horizontal percorrida ($Z$) do Teste B é expressivamente superior à do Teste A ($Z_B > Z_A$).

---

### Cenário 4: Comportamento de Estol Acolhedor (Física Não Punitiva)
- **Objetivo**: Validar conformidade com os Artigos I e II da Constituição (estol suave para crianças/família).
- **Entrada**:
  - Aeronave em velocidade baixa ($v = 4.0\text{ m/s}$) e inclinação excessiva ($+50^\circ$).
  - Ângulo de ataque $\alpha > 20^\circ$ (acima do estol).
- **Ação**: Simular voo por 1 segundo com comandos neutros.
- **Resultado Esperado**:
  - $C_L$ sofre atenuação suave (não despenca para 0 nem tem descontinuidade abrupta).
  - A aeronave inicia descida suave sem travamentos ou giros incontroláveis.
  - A autoestabilização atua reduzindo o pitch em direção ao vetor da trajetória.

---

### Cenário 5: Deslizamento no Solo com Atrito e Finalização com Pouso
- **Objetivo**: Provar a dinâmica de contato com solo, desaceleração por atrito ($\mu = 0.3$) e encerramento do voo com premiação.
- **Entrada**:
  - Aeronave tocando o solo em $Z = 200\text{ m}$ com $V_y = -3\text{ m/s}$ e $V_z = 10\text{ m/s}$.
- **Ação**: Executar `AtualizarFisicaVooCasoDeUso.Executar` ciclicamente até repouso.
- **Resultado Esperado**:
  - No instante de toque ($Y \le 0$): $Y$ trava em $0$, $V_y$ zera e `NoSolo` torna-se `true`.
  - Aceleração de atrito ($a \approx 2.943\text{ m/s}^2$) reduz $V_z$ gradualmente a cada passo de tempo.
  - A aeronave desliza por alguns metros no solo antes de atingir $V_z < 0.5\text{ m/s}$.
  - Ao parar: $V_z$ é zerada, `voo.Pousar()` é chamado automaticamente, o status do voo passa para `StatusVoo.Pousado` e o `ResultadoVoo` é consolidado com bônus e premiações.

---

### Cenário 6: Benchmark de Performance e Zero Alocação (GC Alloc = 0 bytes)
- **Objetivo**: Garantir conformidade estrita com o Critério SC-001 ($< 0.05\text{ms}$) e SC-002 (`GC Alloc = 0 bytes`).
- **Ação**: Executar loop de 10.000 passos de simulação física contínua medindo tempo total e alocações de memória via `GC.GetAllocatedBytesForCurrentThread()`.
- **Resultado Esperado**:
  - Tempo total para 10.000 passos $< 50\text{ms}$ (média por passo $< 0.005\text{ms}$, 10x mais rápido que o limite de $0.05\text{ms}$).
  - Bytes alocados no heap durante o loop contínuo de simulação $= 0\text{ bytes}$.
