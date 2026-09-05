# Guia de Validação Rápida: Domínio Core AeroAscent

**Feature**: `001-dominio-core-aeroascent`  
**Data**: 2026-09-04  
**Status**: Pronto para Validação  

---

## 🎯 Objetivo
Este guia descreve os cenários executáveis e comandos necessários para validar que a camada de Domínio Core do **AeroAscent** opera com 100% de conformidade com as regras de negócio, Clean Architecture, DDD e performance estabelecidas na [Constituição do Projeto](file:///h:/tmp/RSA/Loterias/JogosMaster/GitHub/AeroAscent/.specify/memory/constitution.md) e na [Especificação](file:///h:/tmp/RSA/Loterias/JogosMaster/GitHub/AeroAscent/specs/001-dominio-core-aeroascent/spec.md).

---

## 🛠️ Pré-requisitos
- **SDK .NET 8.0+** instalado no ambiente de desenvolvimento:
  ```powershell
  dotnet --version
  ```
  *(Deve retornar `8.0.xxx` ou superior)*
- Linha de comando / PowerShell no diretório raiz do repositório.

---

## 🧪 Comandos de Execução e Verificação

### 1. Compilação da Solução e Projetos
Para compilar os projetos de Domínio e Testes sem avisos de compilação:
```powershell
dotnet build AeroAscent.sln --configuration Release
```
**Resultado Esperado:** Compilação concluída com `0 Erros` e `0 Avisos`.

### 2. Execução da Suíte de Testes Automatizados (xUnit)
Para executar todos os testes unitários do domínio:
```powershell
dotnet test tests/AeroAscent.Core.Dominio.Testes/AeroAscent.Core.Dominio.Testes.csproj --configuration Release --verbosity normal
```
**Resultado Esperado:**
- 100% dos testes aprovados (`Passed: > 30, Failed: 0, Skipped: 0`).
- Tempo de execução total inferior a **500 milissegundos** (Critério de Sucesso SC-002).

### 3. Validação de Isolamento Arquitetural (Clean Architecture)
Verificar se o projeto `AeroAscent.Core.Dominio.csproj` possui zero referências a frameworks de terceiros ou UI:
```powershell
dotnet list src/AeroAscent.Core.Dominio/AeroAscent.Core.Dominio.csproj package
```
**Resultado Esperado:** Nenhuma dependência externa de pacotes NuGet de UI, frameworks ou engines.

---

## 📋 Cenários de Validação Funcional Ponta a Ponta

### Cenário 1: Inicialização e Invariantes da Aeronave
1. Criar uma nova `Aeronave` com `Guid.NewGuid()`.
2. Validar que `NivelMotor == 1`, `NivelAerodinamica == 1`, `NivelTanqueCombustivel == 1` e `NivelCatapulta == 1`.
3. Tentar atribuir nível `0` ou `-1` e validar disparo de `ArgumentOutOfRangeException`.
4. Tentar atribuir nível `11` e validar disparo de `MelhoriaNivelMaximoException`.

### Cenário 2: Ciclo de Vida da Sessão de Voo
1. Instanciar um `Voo` no status `EmPreparacao`.
2. Executar `IniciarVoo()` e validar transição para `EmVoo`.
3. Registrar métricas de trajetória (ex: 250m de distância, 80m de altitude) e coletar 15 moedas.
4. Finalizar o voo via `FinalizarVoo()` e validar:
   - Status alterado para `Pousado`.
   - Geração de `ResultadoVoo` com total de moedas calculado: $\lfloor 250 \times 0.1 \rfloor + \lfloor 80 \times 0.05 \rfloor + 15 = 25 + 4 + 15 = 44$ moedas.
   - Tentativa posterior de registrar métricas rejeitada com `DominioInvalidoException`.

### Cenário 3: Operações com Moedas e Combustível (Objetos de Valor)
1. Instanciar `Moeda` com saldo inicial de 50.
2. Subtrair 20 moedas $\to$ retorna nova instância com saldo 30.
3. Tentar subtrair 40 moedas $\to$ lança `SaldoInsuficienteException` e preserva integridade.
4. Consumir combustível por 2 segundos e verificar recálculo imutável da quantidade e percentual restante.

### Cenário 4: Evolução na Oficina com Saldo
1. Instanciar `Oficina` e `ProgressoJogador` com 100 moedas.
2. Solicitar evolução de motor (custo nível 1 $\to$ 2: $\lfloor 50 \times 1.5^0 \rfloor = 50$ moedas).
3. Confirmar que o motor da `Aeronave` evoluiu para nível 2 e o saldo final resultou em 50 moedas.

### Cenário 5: Alocação Zero e Vetores 3D (`VetorVoo`)
1. Operar operações vetoriais de soma, subtração e normalização com `VetorVoo`.
2. Confirmar que `VetorVoo` é `readonly record struct`, alocado na pilha (*stack*) sem gerar pressão no Garbage Collector.
