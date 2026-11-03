using TDE3_BancoDeDados.Views;

namespace TDE3_BancoDeDados
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
        }

        async void OnClientesClicked(object? sender, EventArgs e) =>
            await Shell.Current.GoToAsync(nameof(ClientesPage));

        async void OnProdutosClicked(object? sender, EventArgs e) =>
            await Shell.Current.GoToAsync(nameof(ProdutosPage));

        async void OnComprasClicked(object? sender, EventArgs e) =>
            await Shell.Current.GoToAsync(nameof(ComprasPage));
    }
}
