# 🗺️ Roadmap de Especificações Técnicas (Spec-Kit) — AeroAscent

> **Total de Especificações:** 13 (Dentro do limite máximo de 20, calibrado no ideal de 13)  
> **Status:** Todas as 13 especificações devidamente criadas, estruturadas e com checklists de qualidade aprovados.  
> **Idioma Oficial:** Português Brasileiro (pt-BR)  
> **Padrão Arquitetural:** Clean Architecture, DDD, SOLID, 60 FPS Mobile First, Zero Anúncios

---

## 📋 Pipeline Sequencial de Desenvolvimento

| # | Diretório da Feature | Nome da Feature | Descrição e Escopo Central | Status |
|---|---|---|---|---|
| **001** | [`001-dominio-core-aeroascent`](001-dominio-core-aeroascent/spec.md) | `dominio-core-aeroascent` | Entidades (`Aeronave`, `Voo`, `Oficina`), Objetos de Valor (`Combustivel`, `Moeda`, `VetorVoo`) e Interfaces Base em C# puro (.NET Standard). | **Pronto para Planejamento** |
| **002** | [`002-sistema-lancamento-catapulta`](002-sistema-lancamento-catapulta/spec.md) | `sistema-lancamento-catapulta` | Mecânica de Lançamento Inicial com barra de força/precisão, cálculo de impulso vetorial e caso de uso `LancarAeronaveCasoDeUso`. | **Pronto para Planejamento** |
| **003** | [`003-fisica-voo-aerodinamica`](003-fisica-voo-aerodinamica/spec.md) | `fisica-voo-aerodinamica` | Simulação Física de Sustentação (*Lift*), Arrasto (*Drag*), Gravidade e Controle de *Pitch* desacoplado da engine. | **Pronto para Planejamento** |
| **004** | [`004-propulsao-boost-combustivel`](004-propulsao-boost-combustivel/spec.md) | `propulsao-boost-combustivel` | Sistema de Propulsão (*Boost*), queima contínua de combustível, aceleração extra e corte automático ao esgotar. | **Pronto para Planejamento** |
| **005** | [`005-coletaveis-ambiente-pooling`](005-coletaveis-ambiente-pooling/spec.md) | `coletaveis-ambiente-pooling` | Moedas flutuantes, anéis de vento (*air rings*) e arquitetura de *Object Pooling* com zero alocação de memória (0 bytes GC). | **Pronto para Planejamento** |
| **006** | [`006-deteccao-pouso-fim-voo`](006-deteccao-pouso-fim-voo/spec.md) | `deteccao-pouso-fim-voo` | Detecção de contato com o solo, desaceleração por atrito terrestre, congelamento de simulação e encerramento de voo. | **Pronto para Planejamento** |
| **007** | [`007-calculo-recompensas-pontuacao`](007-calculo-recompensas-pontuacao/spec.md) | `calculo-recompensas-pontuacao` | Caso de uso `FinalizarVooCasoDeUso`, aplicação da fórmula de conversão de distância/altitude em moedas e registro de recordes. | **Pronto para Planejamento** |
| **008** | [`008-oficina-loja-upgrades`](008-oficina-loja-upgrades/spec.md) | `oficina-loja-upgrades` | Caso de uso `ComprarMelhoriaCasoDeUso`, escalonamento exponencial de custos ($\text{Custo}(N) = \text{CustoBase} \times 1.5^{N-1}$) e validações. | **Pronto para Planejamento** |
| **009** | [`009-persistencia-local-json`](009-persistencia-local-json/spec.md) | `persistencia-local-json` | Persistência de Dados Local Offline First em JSON atômico e assíncrono (`SalvarProgressoAsync` / `CarregarProgressoAsync`). | **Pronto para Planejamento** |
| **010** | [`010-ui-menu-principal-oficina`](010-ui-menu-principal-oficina/spec.md) | `ui-menu-principal-oficina` | Interface do Menu Principal, Hangar 3D, 4 cartões de melhoria reativos e botão de decolagem ("DECOLAR"). | **Pronto para Planejamento** |
| **011** | [`011-ui-hud-voo`](011-ui-hud-voo/spec.md) | `ui-hud-voo` | HUD de Voo em tempo real (distância, recorde, altímetro, combustível) e controles táteis mobile ergonômicos de subida/descida e boost. | **Pronto para Planejamento** |
| **012** | [`012-ui-resumo-fim-voo`](012-ui-resumo-fim-voo/spec.md) | `ui-resumo-fim-voo` | Tela de Resumo de Voo, contagem animada de moedas ganhas, celebração de novo recorde e botões de redirecionamento. | **Pronto para Planejamento** |
| **013** | [`013-audio-particulas-polish`](013-audio-particulas-polish/spec.md) | `audio-particulas-polish` | Efeitos sonoros estéreo CC0 (Kenney), emissores de partículas (rastro, boost, confetes) e otimização geral para 60 FPS mobile. | **Pronto para Planejamento** |

---

## 🚀 Próximos Passos de Desenvolvimento

1. Selecionar a spec inicial ativa: `specs/001-dominio-core-aeroascent`.
2. Executar `/speckit-plan` para gerar a arquitetura detalhada e plano de implementação da spec selecionada.
3. Executar `/speckit-tasks` para gerar a quebra ordenada de tarefas com TDD.
4. Executar `/speckit-implement` para codificar e verificar os testes.
5. Avançar sequencialmente pelas specs `002` até `013`.
