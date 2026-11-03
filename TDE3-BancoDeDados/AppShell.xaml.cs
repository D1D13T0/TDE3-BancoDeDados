using TDE3_BancoDeDados.Views;

namespace TDE3_BancoDeDados
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            Routing.RegisterRoute(nameof(ClientesPage), typeof(ClientesPage));
            Routing.RegisterRoute(nameof(ProdutosPage), typeof(ProdutosPage));
            Routing.RegisterRoute(nameof(ComprasPage), typeof(ComprasPage));
            Routing.RegisterRoute(nameof(CompraPage), typeof(CompraPage));
        }
    }
}
