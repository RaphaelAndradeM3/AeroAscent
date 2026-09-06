namespace AeroAscent.Core.Aplicacao.Testes.Servicos;

using AeroAscent.Core.Aplicacao.Testes.Fixtures;
using AeroAscent.Core.Dominio.ObjetosDeValor;
using Xunit;

/// <summary>
/// Testes unitários para o contrato de partículas e feedback visual <see cref="GerenciadorParticulasFalso"/>,
/// assegurando conformidade de acionamento contínuo e pontual, limites de intensidade e zero alocação no heap.
/// </summary>
public sealed class GerenciadorParticulasTestes
{
    private readonly GerenciadorParticulasFalso _gerenciador = new();

    [Fact]
    public void DefinirRastroCauda_QuandoAtivadoComIntensidadeValida_DeveAtualizarEstadoComSucesso()
    {
        // Act
        _gerenciador.DefinirRastroCauda(true, 0.75f);

        // Assert
        Assert.True(_gerenciador.RastroCaudaAtivo);
        Assert.Equal(0.75f, _gerenciador.IntensidadeRastroCauda);
    }

    [Theory]
    [InlineData(-0.5f, 0.0f)]
    [InlineData(1.5f, 1.0f)]
    [InlineData(0.5f, 0.5f)]
    public void DefinirRastroCauda_DeveClamparIntensidadeEntreZeroEUm(float intensidadeEntrada, float intensidadeEsperada)
    {
        // Act
        _gerenciador.DefinirRastroCauda(true, intensidadeEntrada);

        // Assert
        Assert.Equal(intensidadeEsperada, _gerenciador.IntensidadeRastroCauda);
    }

    [Fact]
    public void DefinirPropulsao_QuandoAtivadoComIntensidade_DeveAtualizarEstadoComSucesso()
    {
        // Act
        _gerenciador.DefinirPropulsao(true, 1.0f);

        // Assert
        Assert.True(_gerenciador.PropulsaoAtiva);
        Assert.Equal(1.0f, _gerenciador.IntensidadePropulsao);
    }

    [Fact]
    public void DefinirPropulsao_QuandoDesativado_DeveMarcarInativo()
    {
        // Arrange
        _gerenciador.DefinirPropulsao(true, 0.8f);

        // Act
        _gerenciador.DefinirPropulsao(false, 0.0f);

        // Assert
        Assert.False(_gerenciador.PropulsaoAtiva);
        Assert.Equal(0.0f, _gerenciador.IntensidadePropulsao);
    }

    [Fact]
    public void EmitirColetaMoeda_DeveIncrementarContagemERegistrarPosicaoExata()
    {
        // Arrange
        var posicao = new VetorVoo(120.5f, 45.0f, 0.0f);

        // Act
        _gerenciador.EmitirColetaMoeda(posicao);

        // Assert
        Assert.Equal(1, _gerenciador.ContagemColetaMoeda);
        Assert.Equal(posicao, _gerenciador.UltimaPosicaoColetaMoeda);
    }

    [Fact]
    public void EmitirColetaCombustivel_DeveIncrementarContagemERegistrarPosicaoExata()
    {
        // Arrange
        var posicao = new VetorVoo(250.0f, 80.0f, 0.0f);

        // Act
        _gerenciador.EmitirColetaCombustivel(posicao);

        // Assert
        Assert.Equal(1, _gerenciador.ContagemColetaCombustivel);
        Assert.Equal(posicao, _gerenciador.UltimaPosicaoColetaCombustivel);
    }

    [Fact]
    public void EmitirCelebracaoRecorde_DeveIncrementarContagemERegistrarPosicaoExata()
    {
        // Arrange
        var posicao = new VetorVoo(500.0f, 150.0f, 0.0f);

        // Act
        _gerenciador.EmitirCelebracaoRecorde(posicao);

        // Assert
        Assert.Equal(1, _gerenciador.ContagemCelebracaoRecorde);
        Assert.Equal(posicao, _gerenciador.UltimaPosicaoCelebracaoRecorde);
    }

    [Fact]
    public void EmitirImpacto_DeveIncrementarContagemERegistrarPosicaoExata()
    {
        // Arrange
        var posicao = new VetorVoo(600.0f, 0.0f, 0.0f);

        // Act
        _gerenciador.EmitirImpacto(posicao);

        // Assert
        Assert.Equal(1, _gerenciador.ContagemImpacto);
        Assert.Equal(posicao, _gerenciador.UltimaPosicaoImpacto);
    }

    [Fact]
    public void PararTodosOsEfeitos_DeveDesativarRastroCaudaEPropulsaoEIncrementarContador()
    {
        // Arrange
        _gerenciador.DefinirRastroCauda(true, 0.9f);
        _gerenciador.DefinirPropulsao(true, 1.0f);

        // Act
        _gerenciador.PararTodosOsEfeitos();

        // Assert
        Assert.False(_gerenciador.RastroCaudaAtivo);
        Assert.Equal(0f, _gerenciador.IntensidadeRastroCauda);
        Assert.False(_gerenciador.PropulsaoAtiva);
        Assert.Equal(0f, _gerenciador.IntensidadePropulsao);
        Assert.Equal(1, _gerenciador.ContagemPararTodosOsEfeitos);
    }

    [Fact]
    public void Limpar_DeveRestaurarValoresIniciaisDoGerenciador()
    {
        // Arrange
        _gerenciador.DefinirRastroCauda(true, 0.8f);
        _gerenciador.EmitirColetaMoeda(new VetorVoo(10f, 20f, 0f));
        _gerenciador.EmitirCelebracaoRecorde(new VetorVoo(30f, 40f, 0f));

        // Act
        _gerenciador.Limpar();

        // Assert
        Assert.False(_gerenciador.RastroCaudaAtivo);
        Assert.Equal(0, _gerenciador.ContagemColetaMoeda);
        Assert.Equal(0, _gerenciador.ContagemCelebracaoRecorde);
        Assert.Equal(VetorVoo.Zero, _gerenciador.UltimaPosicaoColetaMoeda);
    }

    [Fact]
    public void OperacoesDeParticulas_NaoDevemGerarAlocacaoNoHeap()
    {
        // Aquecimento (JIT)
        var posTeste = new VetorVoo(10f, 20f, 0f);
        _gerenciador.DefinirRastroCauda(true, 0.5f);
        _gerenciador.DefinirPropulsao(true, 0.5f);
        _gerenciador.EmitirColetaMoeda(posTeste);

        // Medição de alocação no heap
        long bytesAntes = GC.GetAllocatedBytesForCurrentThread();

        for (int i = 0; i < 1000; i++)
        {
            var posicao = new VetorVoo(i * 1.5f, i * 0.5f, 0f);
            _gerenciador.DefinirRastroCauda(true, 0.8f);
            _gerenciador.DefinirPropulsao(i % 2 == 0, 0.9f);
            _gerenciador.EmitirColetaMoeda(posicao);
            _gerenciador.EmitirImpacto(posicao);
        }

        long bytesDepois = GC.GetAllocatedBytesForCurrentThread();
        long diferencaAlocacao = bytesDepois - bytesAntes;

        // Valida que nenhuma alocação de memória no heap ocorreu no loop contínuo (GC Alloc = 0 bytes)
        Assert.Equal(0, diferencaAlocacao);
    }
}
