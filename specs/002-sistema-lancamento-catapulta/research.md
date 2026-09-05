# Research: Sistema de Lançamento Inicial e Catapulta

**Feature**: `002-sistema-lancamento-catapulta`  
**Data**: 2026-09-04  
**Status**: Concluído  

---

## 1. Decomposição Balística e Convenção Espacial 3D

### Contexto
O lançamento inicial define a velocidade inicial ($\vec{V}_0$) da aeronave no instante $t = 0$. A especificação estabelece um ângulo de inclinação de saída fixo de $35^\circ$ em relação ao plano horizontal.

### Decisão Técnica
Decompor a velocidade escalar inicial $V_0$ em um vetor 3D `VetorVoo` alinhado ao sistema de coordenadas canônico da Unity Engine:
- **Eixo Z (Forward / Avanço Horizontal)**: $V_z = V_0 \times \cos(35^\circ) \approx V_0 \times 0.819152f$
- **Eixo Y (Up / Altitude Vertical)**: $V_y = V_0 \times \sin(35^\circ) \approx V_0 \times 0.573576f$
- **Eixo X (Right / Desvio Lateral)**: $V_x = 0.0f$

### Racional
1. **Compatibilidade Nativa Unity**: Na Unity, o vetor $(0, 0, 1)$ (`Vector3.forward`) representa a direção frontal e $(0, 1, 0)$ (`Vector3.up`) a direção vertical. Mapear o avanço horizontal para o eixo Z evita matrizes de rotação arbitrárias no momento da renderização 3D.
2. **Zero Alocação de Memória**: O cálculo utiliza constantes pré-calculadas de seno e cosseno em ponto flutuante de precisão simples (`float`), retornando a `readonly record struct VetorVoo` diretamente na stack.

### Alternativas Rejeitadas
- *Lançamento no plano 2D XY ($X$ para frente, $Y$ para cima)*: Rejeitado porque na Unity 3D isso exigiria rotacionar a aeronave em 90 graus no eixo Y, criando inconsistências com modelos tridimensionais (Low Poly Kenney) cujo nariz aponta para o eixo +Z.

---

## 2. Dinâmica Matemática do Medidor de Força Oscilante

### Contexto
O jogador deve visualizar uma barra de oscilação contínua (0% a 100%) no momento do lançamento. O acerto no ápice representa 100% de precisão. O algoritmo de amostragem deve ser determinístico e 100% testável em C# puro sem acoplamento com `MonoBehaviour` ou `Time.deltaTime`.

### Decisão Técnica
Modelar o objeto de valor `MedidorForcaOscilante` utilizando uma função triangular contínua periódica (ping-pong analítico):
$$\text{Ciclo}(t) = (2 \times t \times \text{FrequenciaHz}) \pmod 2$$
$$\text{PrecisaoInstantanea}(t) = 1.0f - | \text{Ciclo}(t) - 1.0f |$$

- **Frequência Padrão**: $1.0\text{ Hz}$ (o ponteiro vai de 0% a 100% em 0.5s e retorna a 0% em mais 0.5s, totalizando 1 ciclo por segundo).
- **Piso Mínimo Protetivo**: Ao disparar, aplica-se $\max(0.10f, \text{Precisao})$, assegurando que o jogador nunca receba impulso nulo.

### Racional
1. **Determinismo Absoluto**: Para qualquer timestamp $t$ fornecido, o valor retornado é exato, permitindo testes unitários automatizados cobrindo $t = 0.0s$ ($0\%$), $t = 0.5s$ ($100\%$), $t = 1.0s$ ($0\%$) etc.
2. **Consumo Simplificado pela Unity**: O script de UI da Unity precisa apenas invocar `medidor.CalcularFator(Time.time)` a cada frame para mover o cursor visual.

### Alternativas Rejeitadas
- *Função Senoidal Pura ($(\sin(\omega t) + 1)/2$)*: Rejeitada pois desacelera nas pontas e acelera no centro, tornando o ápice excessivamente fácil de acertar em comparação com uma barra de habilidade linear arcade.

---

## 3. Arquitetura de Camadas: Criação de `Core.Aplicacao`

### Contexto
A Clean Architecture e a Constituição (Artigo III.2) exigem separação estrita entre:
- **Domínio (`Core/Dominio`)**: Entidades, Objetos de Valor e Serviços de Domínio (regras e cálculos físicos puros).
- **Aplicação (`Core/Aplicacao`)**: Casos de uso e orquestração de fluxos de jogo.

### Decisão Técnica
1. Implementar o serviço de cálculo físico `ServicoFisicaVoo` no Domínio (`src/AeroAscent.Core.Dominio/Servicos/ServicoFisicaVoo.cs`), implementando o contrato `IServicoFisicaVoo`.
2. Criar o projeto de biblioteca de classes C# `src/AeroAscent.Core.Aplicacao/AeroAscent.Core.Aplicacao.csproj` (.NET Standard 2.1 / .NET 8), referenciando o Domínio.
3. Implementar o caso de uso `LancarAeronaveCasoDeUso` em `src/AeroAscent.Core.Aplicacao/CasosDeUso/LancarAeronaveCasoDeUso.cs`.

### Racional
- Respeita rigorosamente a inversão de dependência (DIP) e a separação de responsabilidades. O caso de uso orquestra a sessão de voo e o cálculo de impulso, sem que o domínio precise conhecer fluxos de aplicação.
