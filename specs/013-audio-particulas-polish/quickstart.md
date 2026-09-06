# Guia de Inicialização Rápida e Validação: Áudio, Partículas e Polimento (Feature 013)

## Visão Geral

Este documento descreve os cenários de validação e testes para o subsistema audiovisual de AeroAscent, cobrindo a abstração de áudio desacoplada (`IServicoAudio`), o objeto de valor imutável `ConfiguracaoAudio`, a gestão de loops contínuos de vento e propulsão, a modulação procedural de pitch e polifonia na coleta de moedas e o gerenciamento de partículas com *Object Pooling*.

---

## Cenários de Validação Automatizada (xUnit)

### Cenário 1: Objeto de Valor `ConfiguracaoAudio` e Invariantes
- **Objetivo**: Garantir que `ConfiguracaoAudio` valida limites de volume (0.0 a 1.0) e provê métodos imutáveis seguros de transição.
- **Entrada**: Instância criada com `new ConfiguracaoAudio(0.8f, 0.7f, true, true)`.
- **Resultado Esperado**:
  - `config.ComVolumeEfeitos(1.5f)` lança `DominioInvalidoException`.
  - `config.ComVolumeEfeitos(-0.1f)` lança `DominioInvalidoException`.
  - `config.ComVolumeEfeitos(0.5f)` retorna nova instância com `VolumeEfeitos == 0.5f` e mantém as demais propriedades inalteradas.
  - `config.AlternarEfeitos()` inverte a flag `EfeitosAtivos`.

### Cenário 2: Integração com o Agregado `ProgressoJogador`
- **Objetivo**: Comprovar que o estado do jogador retém e persiste as preferências de áudio sem quebrar retrocompatibilidade.
- **Entrada**: Criação de `ProgressoJogador.CriarNovo()`.
- **Resultado Esperado**:
  - `progresso.ConfiguracaoAudio` é inicializado com `ConfiguracaoAudio.Padrao`.
  - `progresso.AtualizarConfiguracaoAudio(novaConfig)` atualiza o estado com sucesso.

### Cenário 3: Disparo de Eventos Sonoros via `ServicoAudioFalso`
- **Objetivo**: Comprovar que a camada de aplicação e apresentadores conseguem disparar qualquer `EventoAudio` através de `IServicoAudio` sem acoplamento à Unity.
- **Entrada**: Invocação de `servicoAudio.TocarEvento(EventoAudio.ColetaMoeda, 1f)`.
- **Resultado Esperado**:
  - `servicoFalso.UltimoEventoTocado == EventoAudio.ColetaMoeda`.
  - `servicoFalso.ContadorDisparos == 1`.

### Cenário 4: Modulação Contínua de Loops de Vento e Propulsão (`GC Alloc = 0 bytes`)
- **Objetivo**: Garantir que chamadas repetitivas de ajuste de intensidade de vento e propulsão funcionem na stack sem alocações de heap.
- **Entrada**: Atualização contínua com `AtualizarLoopVento(0.65f)` e `DefinirLoopPropulsao(true, 1.0f)`.
- **Resultado Esperado**:
  - `servicoFalso.UltimaIntensidadeVento == 0.65f`.
  - `servicoFalso.LoopPropulsaoAtivo == true`.

### Cenário 5: Modulação de Pitch na Coleta Rápida de Moedas
- **Objetivo**: Validar a lógica de cálculo de modulação de pitch (+0.05 por moeda se intervalo < 0,3s) e teto de polifonia.
- **Entrada**: Simulação de 5 coletas consecutivas com intervalo de 100ms.
- **Resultado Esperado**:
  - Pitchs crescentes: 1.00f $\to$ 1.05f $\to$ 1.10f $\to$ 1.15f $\to$ 1.20f.
  - Reset para 1.00f após espera de 400ms.

### Cenário 6: Tolerância a Falhas e Modo Silencioso
- **Objetivo**: Assegurar que se o hardware de áudio estiver desabilitado ou no mudo, o jogo executa normalmente sem exceções.
- **Entrada**: Desativação dos canais ou ausência de saída de som.
- **Resultado Esperado**:
  - O loop de simulação e interface continuam a 60 FPS estáveis.

---

## Comandos de Execução dos Testes

```powershell
# Executar todos os testes automatizados da solução
dotnet test AeroAscent.slnx

# Executar especificamente os testes de áudio e configurações
dotnet test tests/AeroAscent.Core.Dominio.Testes/AeroAscent.Core.Dominio.Testes.csproj --filter "FullyQualifiedName~ConfiguracaoAudioTestes"
dotnet test tests/AeroAscent.Core.Aplicacao.Testes/AeroAscent.Core.Aplicacao.Testes.csproj --filter "FullyQualifiedName~ServicoAudioTestes"
```
