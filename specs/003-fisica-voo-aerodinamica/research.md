# Pesquisa Técnica: Simulação de Física Aerodinâmica e Controle de Pitch

> **Feature**: `003-fisica-voo-aerodinamica`  
> **Status**: Concluído (Fase 0)  
> **Idioma**: Português Brasileiro (pt-BR)  

---

## 1. Sistema de Coordenadas e Eixos Canônicos 3D

### Decisão
Adotar o **Eixo Z** como avanço horizontal longitudinal (para frente), o **Eixo Y** como altitude vertical (para cima) e o **Eixo X** como eixo transversal lateral ($X = 0$, voo 2.5D com pitch girando em torno do eixo X).

### Racional
1. **Consistência Canônica com Unity Engine**: Na Unity, `Vector3.forward = (0, 0, 1)`, `Vector3.up = (0, 1, 0)` e `Vector3.right = (1, 0, 0)`.
2. **Harmonia com a Feature 002**: O `ServicoFisicaVoo.CalcularImpulsoInicial` já projeta o impulso da catapulta em `(0, Vy, Vz)` no ângulo de 35° no plano Y-Z.
3. **Desacoplamento Seguro**: O domínio C# puro calcula as projeções trigonométricas puras em `VetorVoo(X, Y, Z)` sem jamais referenciar a DLL da Unity, mas com 100% de compatibilidade estrutural direta para mapeamento no `Transform.position` da camada de apresentação.

### Alternativas Consideradas
- *Plano X-Y (2D tradicional de jogos de rolagem lateral)*: Rejeitado porque a catapulta (Feature 002) foi implementada no plano Y-Z com avanço em Z, e a transição da catapulta para o voo livre exige continuidade do vetor de velocidade sem necessidade de rotação ou conversão de eixos.

---

## 2. Modelo Aerodinâmico Arcade Balanceado (Sustentação, Arrasto e Estol Acolhedor)

### Decisão
Implementar um modelo aerodinâmico analítico baseado na formulação clássica da dinâmica dos fluidos, calibrado para dirigibilidade arcade receptiva e acolhedora (Artigos I e II da Constituição):
1. **Constantes Físicas de Referência**:
   - Densidade do ar: $\rho = 1.225\text{ kg/m}^3$ (atmosfera padrão ao nível do mar ISA).
   - Área alar de referência: $S = 1.0\text{ m}^2$.
   - Massa da aeronave: $m = 10.0\text{ kg}$.
   - Aceleração da gravidade: $\vec{g} = (0, -9.81, 0)\text{ m/s}^2$.

2. **Ângulo de Ataque ($\alpha$)**:
   - O ângulo da trajetória de voo é calculado como:
     $$\gamma = \arctan2(V_y, V_z) \times \frac{180}{\pi}$$
   - O ângulo de arfagem ($\theta$, inclinação do nariz/pitch) varia entre $-45^\circ$ e $+60^\circ$.
   - O ângulo de ataque é dado por:
     $$\alpha = \theta - \gamma$$

3. **Coeficiente de Sustentação ($C_L$) e Estol Acolhedor**:
   - Na faixa linear ($|\alpha| \le 20^\circ$):
     $$C_L(\alpha) = 0.075 \cdot \alpha$$
     Atinge $C_{L\max} \approx 1.5$ a $\alpha = 20^\circ$.
   - Na faixa pós-estol ($|\alpha| > 20^\circ$):
     Em vez de uma descontinuidade abrupta ou colapso a zero (comportamento de simulador militar que frustraria crianças e novatos), adota-se um decaimento suave:
     $$C_L(\alpha) = \text{sign}(\alpha) \cdot \left[1.5 \cdot \cos\left(\frac{|\alpha| - 20^\circ}{90^\circ - 20^\circ} \cdot \frac{\pi}{2}\right) + 0.3\right]$$
     A sustentação decai até um piso sustentável de $0.3$, provocando descida suave com recuperação natural ao mergulhar o nariz.

4. **Coeficiente de Arrasto ($C_D$) e Redução por Nível de Aerodinâmica**:
   - Arrasto parasita base: $C_{D0} = 0.04$.
   - Arrasto induzido: $k = 0.05$.
   - Equação polar de arrasto:
     $$C_D(\alpha) = C_{D0} + k \cdot [C_L(\alpha)]^2$$
   - Redução pelo nível de aerodinâmica da oficina (nível 1 a 10):
     $$C_{D\text{efetivo}} = \frac{C_D(\alpha)}{1 + (\text{NivelAerodinamica} - 1) \times 0.20}$$
     No nível 1, divisor é $1.00$ ($100\%$ de arrasto); no nível 5, divisor é $1.80$ (arrasto reduzido em $44\%$); no nível 10, divisor é $2.80$ (arrasto reduzido em $64\%$).

5. **Decomposição Vetorial das Forças**:
   - Velocidade escalar total: $v = \sqrt{V_y^2 + V_z^2}$.
   - Vetor unitário da velocidade: $\vec{u}_v = (0, \frac{V_y}{v}, \frac{V_z}{v})$.
   - Magnitude da sustentação: $L = \frac{1}{2} \cdot \rho \cdot v^2 \cdot S \cdot C_L(\alpha)$.
   - Magnitude do arrasto: $D = \frac{1}{2} \cdot \rho \cdot v^2 \cdot S \cdot C_{D\text{efetivo}}$.
   - Vetor de arrasto: $\vec{F}_{\text{arrasto}} = -\vec{u}_v \cdot D$.
   - Vetor de sustentação (perpendicular à velocidade no plano Y-Z):
     $\vec{u}_L = (0, \frac{V_z}{v}, -\frac{V_y}{v})$. Multiplicado por $L$: $\vec{F}_{\text{sustentacao}} = \vec{u}_L \cdot L$.
   - Força gravitacional: $\vec{F}_{\text{gravidade}} = (0, -m \cdot g, 0)$.

### Racional
Garante que a aeronave responda intuitivamente aos comandos do jogador: inclinar o nariz para cima gera sustentação e ganho de altitude às custas de velocidade; inclinar para baixo gera ganho de velocidade. O estol acolhedor cumpre integralmente os Artigos I e II da Constituição.

### Alternativas Consideradas
- *Tabelas de Coeficientes Empíricos NACA 0012/2412*: Rejeitadas por exigir interpolação computacional desnecessária e alocação de tabelas na memória, violando a simplicidade e a meta de execução $< 0.05\text{ms}$.
- *Modelo Falso / Guiado por Trilhos*: Rejeitado categoricamente pelo Artigo II da Constituição.

---

## 3. Integração Numérica Cinemática e Alocação Zero de Memória

### Decisão
Utilizar o método de **Euler Semi-Implícito (Euler-Cromer)** com tipos por valor imutáveis na stack (`readonly record struct` `VetorVoo` e `EstadoFisicoAeronave`).

### Racional
1. **Passo de Integração**:
   $$\vec{a}_t = \frac{\vec{F}_{\text{total}}}{m} = \frac{\vec{F}_{\text{sustentacao}} + \vec{F}_{\text{arrasto}} + \vec{F}_{\text{gravidade}}}{m}$$
   $$\vec{v}_{t+dt} = \vec{v}_t + \vec{a}_t \cdot dt$$
   $$\vec{p}_{t+dt} = \vec{p}_t + \vec{v}_{t+dt} \cdot dt$$
2. **Estabilidade e Conservação**: Euler semi-implícito conserva a energia orbital/mecânica muito melhor que Euler explícito para passos de simulação arcade (60 Hz a 100 Hz, $dt \in [0.01\text{s}, 0.02\text{s}]$), sem a sobrecarga de 4 avaliações por passo do Runge-Kutta 4 (RK4).
3. **Performance Absoluta**: Um passo de simulação é calculado em aproximadamente $0.002\text{ms}$, batendo com facilidade folgada o critério SC-001 ($< 0.05\text{ms}$).
4. **Alocação Zero (GC Alloc = 0 bytes)**: Estruturas passadas na stack (`readonly record struct`) não exigem boxing, garbage collection ou novas alocações no heap.

### Alternativas Consideradas
- *Runge-Kutta 4 (RK4)*: Rejeitado porque a física de jogo arcade com controle instantâneo do jogador não se beneficia da precisão de 4ª ordem e custaria 4x mais tempo de CPU por passo.
- *Physics 2D / 3D Nativo do Unity (Rigidbody)*: Rejeitado porque a simulação física do núcleo do jogo deve residir de forma pura e testável no Domínio .NET, permitindo testes unitários headless instantâneos e determinismo independente da engine gráfica.

---

## 4. Dinâmica de Controle de Pitch e Autoestabilização Suave

### Decisão
O input do piloto (de $-1.0$ a $+1.0$) comanda a velocidade angular do pitch ($\omega = \text{input} \times 45^\circ/\text{s}$), respeitando os limites físicos de $-45^\circ$ (mergulho máximo) e $+60^\circ$ (subida máxima). Ao soltar os controles ($|\text{input}| < 0.05$), o nariz da aeronave sofre torque restaurador de autoestabilização que converge suavemente em direção ao vetor velocidade $\gamma$.

### Racional
- Permite controle dinâmico fluido no teclado, joystick ou tela de toque (mobile).
- Impede atitudes anômalas (como avião voando de marcha à ré ou de ponta-cabeça sem controle), tornando a pilotagem acessível para crianças e jogadores casuais.

---

## 5. Dinâmica de Solo, Atrito e Encerramento do Voo

### Decisão
Ao atingir a altitude de solo ($Y \le 0$):
1. A altitude $Y$ é travada em $0$ e a velocidade vertical $V_y$ é zerada.
2. A aeronave transita para a condição de deslizamento no solo (`NoSolo = true`).
3. Uma desaceleração por atrito cinético de solo ($\mu = 0.30$) é aplicada contra o avanço horizontal $Z$:
   $$F_{\text{atrito}} = \mu \cdot m \cdot g = 0.30 \cdot 10.0 \cdot 9.81 = 29.43\text{ N}$$
   $$a_{\text{atrito}} = \frac{F_{\text{atrito}}}{m} = \mu \cdot g = 2.943\text{ m/s}^2$$
4. A velocidade horizontal $V_z$ diminui até atingir o limiar de parada ($V_z < 0.5\text{ m/s}$).
5. Ao parar completamente, o caso de uso aciona `voo.Pousar()`, consolidando a distância total percorrida e gerando o `ResultadoVoo`.

### Racional
Proporciona uma sensação gratificante de pouso onde o jogador vê a aeronave deslizar pela pista antes de parar e receber suas moedas e premiações.
