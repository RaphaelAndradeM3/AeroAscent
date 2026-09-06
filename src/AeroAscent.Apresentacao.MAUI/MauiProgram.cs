namespace AeroAscent.Apresentacao.MAUI;

using System.IO;
using AeroAscent.Apresentacao.MAUI.Servicos;
using AeroAscent.Apresentacao.MAUI.Views;
using AeroAscent.Core.Aplicacao.CasosDeUso;
using AeroAscent.Core.Aplicacao.Contratos;
using AeroAscent.Core.Dominio.Contratos;
using AeroAscent.Core.Dominio.Servicos;
using AeroAscent.Infraestrutura.Configuracao;
using AeroAscent.Infraestrutura.Persistencia;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls.Hosting;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.Storage;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

#if DEBUG
        builder.Logging.AddDebug();
#endif

        // 1. Infraestrutura & Persistência Local JSON Offline First
        var caminhoDados = FileSystem.AppDataDirectory;
        var configPersistencia = new ConfiguracaoPersistenciaLocal(caminhoDados);
        builder.Services.AddSingleton(configPersistencia);
        builder.Services.AddSingleton<IRepositorioProgresso, RepositorioProgressoLocalJson>();

        // 2. Serviços de Domínio e Apresentação
        builder.Services.AddSingleton<IServicoFisicaVoo, ServicoFisicaVoo>();
        builder.Services.AddSingleton<IServicoAudio, ServicoAudioMAUI>();
        builder.Services.AddSingleton<GerenciadorParticulasMAUI>();
        builder.Services.AddSingleton<PublicadorEventosVooMAUI>();

        // 3. Casos de Uso da Aplicação
        builder.Services.AddSingleton<IConsultarOficinaCasoDeUso, ConsultarOficinaCasoDeUso>();
        builder.Services.AddSingleton<IComprarMelhoriaCasoDeUso, ComprarMelhoriaCasoDeUso>();
        builder.Services.AddSingleton<ILancarAeronaveCasoDeUso, LancarAeronaveCasoDeUso>();
        builder.Services.AddSingleton<IAtualizarFisicaVooCasoDeUso, AtualizarFisicaVooCasoDeUso>();
        builder.Services.AddSingleton<IProcessarPousoFimVooCasoDeUso>(sp =>
            new ProcessarPousoFimVooCasoDeUso(sp.GetRequiredService<PublicadorEventosVooMAUI>()));
        builder.Services.AddSingleton<IFinalizarVooCasoDeUso, FinalizarVooCasoDeUso>();

        // 4. Gerenciador Central da Sessão de Jogo
        builder.Services.AddSingleton<GerenciadorSessaoJogo>();

        // 5. Páginas do Jogo
        builder.Services.AddTransient<PaginaOficina>();

        return builder.Build();
    }
}
