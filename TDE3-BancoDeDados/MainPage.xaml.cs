using TDE3_BancoDeDados.Services;
using TDE3_BancoDeDados.Views;

namespace TDE3_BancoDeDados
{
    public partial class MainPage : ContentPage
    {
        readonly ClienteService _clienteService;
        readonly ProdutoService _produtoService;
        readonly CompraService _compraService;

        public MainPage(ClienteService clienteService, ProdutoService produtoService, CompraService compraService)
        {
            InitializeComponent();
            _clienteService = clienteService;
            _produtoService = produtoService;
            _compraService = compraService;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            // Atualiza o resumo sempre que volta para o menu
            await this.ExecutarAsync(async () =>
            {
                var compras = await _compraService.ListarAsync();
                LblClientes.Text = (await _clienteService.ListarAsync()).Count.ToString();
                LblProdutos.Text = (await _produtoService.ListarAsync()).Count.ToString();
                LblCompras.Text = compras.Count.ToString();
                LblFaturado.Text = compras.Sum(c => c.ValorTotal).ToString("C");
            });
        }

        async void OnClientesClicked(object? sender, EventArgs e) =>
            await Shell.Current.GoToAsync(nameof(ClientesPage));

        async void OnProdutosClicked(object? sender, EventArgs e) =>
            await Shell.Current.GoToAsync(nameof(ProdutosPage));

        async void OnComprasClicked(object? sender, EventArgs e) =>
            await Shell.Current.GoToAsync(nameof(ComprasPage));
    }
}
