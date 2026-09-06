namespace AeroAscent.Core.Aplicacao.Contratos;

using AeroAscent.Core.Dominio.Enums;
using AeroAscent.Core.Dominio.ObjetosDeValor;

/// <summary>
/// Contrato do serviço de áudio da aplicação responsável pelo gerenciamento de efeitos sonoros,
/// loops contínuos de vento e propulsão, música ambiente e preferências acústicas do jogador.
/// Implementado na camada de apresentação/infraestrutura pela Unity Engine.
/// </summary>
public interface IServicoAudio
{
    /// <summary>
    /// Reproduz um efeito sonoro pontual (SFX) com volume e modulação padrão.
    /// </summary>
    /// <param name="evento">Identificador do evento sonoro a ser tocado.</param>
    /// <param name="escalaVolume">Fator de escala de volume relativo (0.0f a 1.0f).</param>
    void TocarEvento(EventoAudio evento, float escalaVolume = 1f);

    /// <summary>
    /// Atualiza o loop contínuo de som de vento de acordo com a velocidade normalizada da aeronave.
    /// </summary>
    /// <param name="intensidadeNormalizada">Intensidade de vento de 0.0f (repouso) a 1.0f (velocidade terminal).</param>
    void AtualizarLoopVento(float intensidadeNormalizada);

    /// <summary>
    /// Define o estado e a potência do loop sonoro de propulsão (boost) da aeronave.
    /// </summary>
    /// <param name="ativo"><c>true</c> para acionar o som do motor; <c>false</c> para silenciar suavemente.</param>
    /// <param name="intensidade">Potência normalizada do motor (0.0f a 1.0f).</param>
    void DefinirLoopPropulsao(bool ativo, float intensidade = 1f);

    /// <summary>
    /// Inicia a reprodução suave da trilha musical temática do jogo em loop.
    /// </summary>
    void TocarMusicaTema();

    /// <summary>
    /// Interrompe a reprodução da música ambiente com fade out suave.
    /// </summary>
    void PararMusica();

    /// <summary>
    /// Aplica as preferências de volume e canais do jogador ao subsistema de áudio.
    /// </summary>
    /// <param name="configuracao">Estrutura imutável de preferências sonoras.</param>
    void AplicarConfiguracao(in ConfiguracaoAudio configuracao);

    /// <summary>
    /// Retorna as preferências atuais ativas no subsistema de áudio.
    /// </summary>
    ConfiguracaoAudio ObterConfiguracao();
}
