# Pesquisa e Decisões de Arquitetura: Feature 009 — Persistência de Dados Local Offline First (JSON)

**Branch**: `009-persistencia-local-json` | **Data**: 2026-09-05 | **Spec**: [spec.md](./spec.md)

---

## 🔬 Decisões de Engenharia

### Decisão 1: Biblioteca de Serialização JSON (`System.Text.Json`)
- **Escolha**: Utilizar exclusivamente `System.Text.Json` sem bibliotecas de terceiros (como Newtonsoft.Json).
- **Racional**:
  - Padrão nativo do .NET Standard 2.1 e .NET 8 com suporte oficial e alta performance no Unity IL2CPP.
  - Zero dependências externas no projeto `AeroAscent.Infraestrutura`.
  - Serialização tipada de alta velocidade atendendo ao critério de tempo assíncrono $< 15\text{ms}$ (SC-001).
- **Alternativas Rejeitadas**:
  - `Newtonsoft.Json`: Gera alocações desnecessárias no heap e dependência de pacote externo.
  - `JsonUtility` da Unity: Não suporta tipos de domínio puros sem acoplamento a classes de engine (`UnityEngine`).

### Decisão 2: Protocolo de Gravação Atômica e Prevenção de Corrupção
- **Escolha**: Gravação em arquivo temporário com extensão `.tmp`, rotação de arquivo de backup (`.bak`) e promoção atômica via `File.Move(caminhoTmp, caminhoPrincipal, overwrite: true)`.
- **Racional**:
  - `File.Replace` exige que o arquivo de destino já exista no Windows e pode apresentar inconsistências de permissão em sistemas baseados em Linux/Android.
  - `File.Move` com `overwrite: true` realiza uma substituição atômica no nível do sistema de arquivos e funciona tanto na criação inicial quanto na atualização.
  - O arquivo temporário isola gravações parciais em caso de fechamento forçado ou queda de energia.
- **Alternativas Rejeitadas**:
  - Gravação direta via `File.WriteAllTextAsync`: Risco iminente de corromper o arquivo principal caso a aplicação seja interrompida no meio da gravação.

### Decisão 3: Tolerância a Falhas e Recuperação de Corrupção
- **Escolha**: Protocolo de fallback em dois estágios:
  1. Detectar corrupção no arquivo principal (`JsonException` ou formato inválido).
  2. Tentar imediatamente restaurar e carregar o arquivo de backup `.bak`.
  3. Caso o backup também inexista ou esteja danificado, isolar o arquivo com extensão `.corrompido_[timestamp]`, registrar aviso e retornar `null`.
- **Racional**:
  - O jogador nunca perde o progresso se o backup estiver íntegro.
  - A aplicação nunca trava (*zero crashes*) na inicialização por arquivo danificado.
  - O arquivo corrompido fica preservado para depuração sem impedir o fluxo normal do jogo.
- **Alternativas Rejeitadas**:
  - Lançar exceção não tratada: Quebra a experiência do usuário logo na tela inicial.
  - Apagar sumariamente o arquivo: Impede qualquer análise de causa-raiz.

### Decisão 4: Sincronização de Concorrência Assíncrona (`SemaphoreSlim`)
- **Escolha**: Utilizar uma instância interna de `SemaphoreSlim(1, 1)` por repositório para controlar operações assíncronas de leitura e escrita em disco.
- **Racional**:
  - Permite aguardar assincronamente (`await _semaforo.WaitAsync(ct)`) sem travar a thread de execução do jogo ou a UI/Engine.
  - Garante exclusão mútua estrita na manipulação física dos arquivos `.json`, `.tmp` e `.bak`.
  - Liberação obrigatória em bloco `finally`.
- **Alternativas Rejeitadas**:
  - `lock (_lockObj)`: Bloqueia a thread atual de forma síncrona, inaceitável em loops de renderização e operações assíncronas em C#.

### Decisão 5: Estrutura do DTO de Persistência e Versionamento (`VersaoSchema`)
- **Escolha**: Criar um DTO plano dedicado `ProgressoJogadorDTO` com campo explícito `VersaoSchema = 1` e `DataHoraSalvamentoUtc`.
- **Racional**:
  - Desacopla o layout de persistência física em JSON das entidades de domínio (`ProgressoJogador`, `Aeronave`, `Moeda`).
  - Permite validação de compatibilidade na desserialização.
  - Facilita migrações futuras conforme novas funcionalidades e moedas forem introduzidas.
- **Alternativas Rejeitadas**:
  - Serializar a entidade `ProgressoJogador` diretamente: Viola o encapsulamento de invariantes e expõe propriedades privadas/construtores protegidos a deserializadores.

### Decisão 6: Configuração Flexível do Diretório de Dados
- **Escolha**: Modelar `ConfiguracaoPersistenciaLocal` recebendo o caminho base (`DiretorioBase`), com nomes canônicos `progresso.json`, `progresso.bak` e `progresso.tmp`.
- **Racional**:
  - Permite à camada Unity fornecer `Application.persistentDataPath` para Windows Standalone e Android.
  - Permite aos testes unitários e de integração em xUnit utilizar diretórios temporários isolados no disco sem colisões.
