using TDE3_BancoDeDados.Models;
using TDE3_BancoDeDados.Services;

namespace TDE3_BancoDeDados.Views
{
    public partial class ProdutosPage : ContentPage
    {
        readonly ProdutoService _service;
        int _codigoEditando;

        public ProdutosPage(ProdutoService service)
        {
            InitializeComponent();
            _service = service;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await this.ExecutarAsync(CarregarAsync);
        }

        async Task CarregarAsync()
        {
            Lista.ItemsSource = await _service.ListarAsync(TxtBusca.Text);
        }

        void LimparFormulario()
        {
            _codigoEditando = 0;
            LblModo.Text = "Novo produto";
            TxtNome.Text = TxtTipo.Text = TxtPreco.Text = TxtEstoque.Text = string.Empty;
        }

        async void OnSalvarClicked(object? sender, EventArgs e)
        {
            await this.ExecutarAsync(async () =>
            {
                await _service.SalvarAsync(_codigoEditando, TxtNome.Text, TxtTipo.Text, TxtPreco.Text, TxtEstoque.Text);
                await DisplayAlertAsync("Sucesso", "Produto salvo com sucesso.", "OK");
                LimparFormulario();
                await CarregarAsync();
            });
        }

        void OnLimparClicked(object? sender, EventArgs e) => LimparFormulario();

        async void OnBuscaChanged(object? sender, TextChangedEventArgs e) =>
            await this.ExecutarAsync(CarregarAsync);

        void OnEditarClicked(object? sender, EventArgs e)
        {
            if ((sender as BindableObject)?.BindingContext is not Produto produto)
                return;

            _codigoEditando = produto.CodigoProduto;
            LblModo.Text = $"Editando produto #{produto.CodigoProduto}";
            TxtNome.Text = produto.NomeProduto;
            TxtTipo.Text = produto.Tipo;
            TxtPreco.Text = produto.Preco.ToString("0.00");
            TxtEstoque.Text = produto.QtdEstoque.ToString();
        }

        async void OnExcluirClicked(object? sender, EventArgs e)
        {
            if ((sender as BindableObject)?.BindingContext is not Produto produto)
                return;

            if (!await DisplayAlertAsync("Excluir", $"Excluir o produto {produto.NomeProduto}?", "Sim", "Não"))
                return;

            await this.ExecutarAsync(async () =>
            {
                await _service.ExcluirAsync(produto.CodigoProduto);
                if (_codigoEditando == produto.CodigoProduto)
                    LimparFormulario();
                await CarregarAsync();
            });
        }
    }
}
