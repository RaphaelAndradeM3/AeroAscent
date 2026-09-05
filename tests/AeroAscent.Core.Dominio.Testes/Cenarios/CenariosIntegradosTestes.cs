namespace AeroAscent.Core.Dominio.Testes.Cenarios;

using AeroAscent.Core.Dominio.Entidades;
using AeroAscent.Core.Dominio.Enums;
using AeroAscent.Core.Dominio.Excecoes;
using AeroAscent.Core.Dominio.ObjetosDeValor;
using Xunit;

/// <summary>
/// Testes integrados cobrindo explicitamente os 5 cenários do quickstart.md.
/// </summary>
public class CenariosIntegradosTestes
{
    [Fact]
    public void Cenario1_InicializacaoEInvariantesDaAeronave()
    {
        // 1. Criar uma nova Aeronave
        var aeronave = Aeronave.CriarPadrao();

        // 2. Validar níveis padrão iguais a 1
        Assert.Equal(1, aeronave.NivelMotor);
        Assert.Equal(1, aeronave.NivelAerodinamica);
        Assert.Equal(1, aeronave.NivelTanqueCombustivel);
        Assert.Equal(1, aeronave.NivelCatapulta);

        // 3 e 4. Tentar atribuir níveis ilegais e validar exceção
        Assert.Throws<DominioInvalidoException>(() => aeronave.AtualizarNivel(TipoMelhoria.Motor, 0));
        Assert.Throws<DominioInvalidoException>(() => aeronave.AtualizarNivel(TipoMelhoria.Motor, 11));
    }

    [Fact]
    public void Cenario2_CicloDeVidaDaSessaoDeVoo()
    {
        // 1. Instanciar Voo EmPreparacao
        var voo = Voo.Iniciar(Aeronave.CriarPadrao());
        Assert.Equal(StatusVoo.EmPreparacao, voo.Status);

        // 2. Decolar para EmVoo
        voo.Decolar();
        Assert.Equal(StatusVoo.EmVoo, voo.Status);

        // 3. Registrar trajetória: 250m de distância, 80m de altitude e 15 moedas
        voo.AtualizarMetricas(250f, 80f, 15);

        // 4. Pousar e validar
        var resultado = voo.Pousar();
        Assert.Equal(StatusVoo.Pousado, voo.Status);

        // Fórmula: floor(250 * 0.1) + floor(80 * 0.05) + 15 = 25 + 4 + 15 = 44 moedas
        Assert.Equal(44, resultado.MoedasRecompensaTotal.Quantidade);

        // Tentativa posterior de registrar métricas rejeitada
        Assert.Throws<DominioInvalidoException>(() => voo.AtualizarMetricas(300f, 90f, 0));
    }

    [Fact]
    public void Cenario3_OperacoesComMoedasECombustivel()
    {
        // 1 e 2. Instanciar Moeda com 50 e subtrair 20 => saldo 30
        var saldo = new Moeda(50);
        var novoSaldo = saldo - new Moeda(20);
        Assert.Equal(30, novoSaldo.Quantidade);

        // 3. Subtrair 40 moedas com saldo 30 => SaldoInsuficienteException
        Assert.Throws<SaldoInsuficienteException>(() => novoSaldo.Subtrair(new Moeda(40)));

        // 4. Consumir combustível por 2 segundos e verificar recálculo imutável
        var combustivel = Combustivel.CriarCheio(100f, 10f); // 10 unidades/s
        var consumido = combustivel.Consumir(2f);
        Assert.Equal(80f, consumido.QuantidadeAtual);
        Assert.Equal(0.8f, consumido.PercentualRestante);
    }

    [Fact]
    public void Cenario4_EvolucaoNaOficinaComSaldo()
    {
        // 1. Instanciar Oficina e ProgressoJogador com 100 moedas
        var oficina = Oficina.CriarPadrao();
        var progresso = ProgressoJogador.CriarNovo();
        progresso.CreditarMoedas(new Moeda(100));

        // 2. Evoluir motor na aeronave (custo nível 1 -> 2: 50 * 1.5^0 = 50 moedas)
        var novoSaldo = oficina.EvoluirComponente(progresso.Aeronave, progresso.SaldoMoedas, TipoMelhoria.Motor);
        progresso.DebitarMoedas(new Moeda(50));

        // 3. Confirmar motor no nível 2 e saldo final de 50 moedas
        Assert.Equal(2, progresso.Aeronave.NivelMotor);
        Assert.Equal(new Moeda(50), novoSaldo);
        Assert.Equal(new Moeda(50), progresso.SaldoMoedas);
    }

    [Fact]
    public void Cenario5_AlocacaoZeroEVetores3D()
    {
        // 1. Operar operações vetoriais puras com VetorVoo
        var v1 = new VetorVoo(3f, 4f, 0f);
        var v2 = new VetorVoo(1f, 2f, 3f);

        var soma = v1 + v2;
        var sub = v1 - v2;
        var unitario = v1.Normalizar();

        Assert.Equal(new VetorVoo(4f, 6f, 3f), soma);
        Assert.Equal(new VetorVoo(2f, 2f, -3f), sub);
        Assert.Equal(1.0f, unitario.Magnitude(), precision: 3);

        // 2. Confirmar que VetorVoo é value type (struct) para alocação na stack
        Assert.True(typeof(VetorVoo).IsValueType);
    }
}
