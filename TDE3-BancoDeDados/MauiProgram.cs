using Microsoft.Extensions.Logging;
using TDE3_BancoDeDados.Data;
using TDE3_BancoDeDados.Services;
using TDE3_BancoDeDados.Views;

namespace TDE3_BancoDeDados
{
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

            AparenciaCampos.Configurar();

            // Banco de dados e serviços (validação + acesso ao banco)
            builder.Services.AddSingleton<Database>();
            builder.Services.AddSingleton<ClienteService>();
            builder.Services.AddSingleton<ProdutoService>();
            builder.Services.AddSingleton<CompraService>();

            // Páginas
            builder.Services.AddTransient<MainPage>();
            builder.Services.AddTransient<ClientesPage>();
            builder.Services.AddTransient<ProdutosPage>();
            builder.Services.AddTransient<ComprasPage>();
            builder.Services.AddTransient<CompraPage>();

#if DEBUG
    		builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
