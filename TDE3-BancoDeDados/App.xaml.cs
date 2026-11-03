using System.Globalization;
using Microsoft.Extensions.DependencyInjection;

namespace TDE3_BancoDeDados
{
    public partial class App : Application
    {
        public App()
        {
            // Moeda (R$) e datas no formato brasileiro, independente do idioma do aparelho
            var ptBr = new CultureInfo("pt-BR");
            CultureInfo.DefaultThreadCurrentCulture = ptBr;
            CultureInfo.DefaultThreadCurrentUICulture = ptBr;
            CultureInfo.CurrentCulture = ptBr;
            CultureInfo.CurrentUICulture = ptBr;

            InitializeComponent();
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            return new Window(new AppShell());
        }
    }
}
