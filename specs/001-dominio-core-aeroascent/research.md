# Pesquisa Técnica e Decisões de Arquitetura: Domínio Core AeroAscent

**Feature**: `001-dominio-core-aeroascent`  
**Data**: 2026-09-04  
**Status**: Concluído e Aprovado  

---

## 1. Versão da Linguagem e Target Framework

### Decisão
Utilizar **.NET Standard 2.1 / .NET 8 (`netstandard2.1;net8.0`)** com **C# 12** para a biblioteca de classes do Domínio Core (`AeroAscent.Core.Dominio.csproj`).

### Racional
- .NET Standard 2.1 é o padrão universal consumido nativamente pela **Unity Engine** tanto em compilação Mono quanto **IL2CPP (Windows Standalone e Android APK/AAB)**.
- .NET 8 permite execução com máxima performance na suíte de testes xUnit fora da Unity.
- C# 12 introduz recursos essenciais para Domain-Driven Design de alta performance, como `record`, `readonly record struct`, *primary constructors* e inicializadores imutáveis que garantem código expressivo e sem *boilerplate*.
- Garante total independência de bibliotecas externas, permitindo compilação como assembly C# puro sem acoplamento a UI ou engines de terceiros.

### Alternativas Consideradas
- **Dependência direta de `UnityEngine`**: Rejeitado terminantemente. A Clean Architecture e a Constituição exigem que o Domínio seja C# puro, garantindo testes ultrarrápidos (< 500 ms) fora do editor da Unity.

---

## 2. Estrutura de Projetos e Clean Architecture

### Decisão
Organizar o código em conformidade com o padrão Clean Architecture em diretórios de projeto padronizados:
- `src/AeroAscent.Core.Dominio/`: Projeto C# puro com entidades, objetos de valor, exceções de domínio e interfaces base.
- `tests/AeroAscent.Core.Dominio.Testes/`: Suíte completa de testes unitários automatizados.

### Racional
- Garante isolamento físico das dependências: o projeto de domínio não possui referências a outros projetos nem a bibliotecas gráficas.
- Facilita a reutilização do domínio pela camada de apresentação da Unity (Windows e Android) e camada de aplicação.

### Alternativas Consideradas
- **Monolito de projeto único**: Rejeitado por violar o princípio de separação de camadas da Clean Architecture e a Constituição do projeto (Artigo III.2).

---

## 3. Gestão de Memória e Alocação Zero (Zero GC) para Objetos de Valor

### Decisão
- Modelar `VetorVoo` como `readonly record struct` imutável contendo componentes `float X`, `float Y`, `float Z`.
- Modelar `Combustivel`, `Moeda`, `Melhoria` e `ResultadoVoo` como `record` imutável.

### Racional
- Operações físicas vetoriais (posição, velocidade, impulso) ocorrem em alta frequência a cada quadro (60 FPS).
- Sendo um `struct` imutável por valor, `VetorVoo` reside na pilha (*stack*) e elimina completamente a alocação de memória na *heap*, satisfazendo a regra de **`GC Alloc = 0 bytes`** imposta pela Constituição (Artigo III.4 e RNF-02).
- Os demais objetos de valor (`Moeda`, `Combustivel`, `Melhoria`, `ResultadoVoo`) operam em transições de estado bem definidas, garantindo segurança matemática através da imutabilidade sem efeitos colaterais.

### Alternativas Consideradas
- **`VetorVoo` como `record class`**: Rejeitado pois instanciar vetores a cada cálculo físico na *heap* geraria pausas periódicas do *Garbage Collector*, comprometendo a estabilidade de 60 FPS no Android.

---

## 4. Framework de Testes Unitários e Metas de Desempenho

### Decisão
Utilizar **xUnit 2.x** com o test runner leve do .NET (`dotnet test`) e asserções nativas ou isoladas em C#.

### Racional
- xUnit é o padrão da comunidade .NET moderna para execução paralela de testes unitários isolados.
- Execução extremamente rápida (< 200 ms para centenas de testes de domínio puro), garantindo cumprimento da meta de validação (< 500 ms conforme SC-002).

### Alternativas Consideradas
- **MSTest / NUnit**: Embora válidos, xUnit oferece convenção mais moderna de isolamento por instância de teste e integração fluida com o CLI `dotnet test`.

---

## 5. Regras Econômicas e Fórmulas de Domínio

### Decisão
Encapsular as fórmulas matemáticas diretamente nas entidades e objetos de valor do domínio:
1. **Cálculo de Recompensa de Voo (`ResultadoVoo`):**
   $$\text{Moedas Ganhas} = \lfloor \text{Distância} \times 0.1 \rfloor + \lfloor \text{Altitude} \times 0.05 \rfloor + \text{Moedas Coletadas}$$
2. **Cálculo de Custo de Melhoria (`Oficina` / `Melhoria`):**
   $$\text{Custo}(N) = \lfloor \text{CustoBase} \times (1.5)^{N-1} \rfloor$$
   com validação estrita de $1 \le N \le 10$.
3. **Operações de Saldo (`Moeda`):**
   Subtração protegida impedindo valores negativos com lançamento de `SaldoInsuficienteException`.

### Racional
- Mantém o domínio rico (*Rich Domain Model*), evitando modelo anêmico.
- Regras de negócio essenciais ficam protegidas contra mutações indevidas e testadas independentemente de qualquer UI ou persistência.
