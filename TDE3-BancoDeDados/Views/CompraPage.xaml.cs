using TDE3_BancoDeDados.Models;
using TDE3_BancoDeDados.Services;

namespace TDE3_BancoDeDados.Views
{
    /// <summary>
    /// Tela de uma compra: dados da compra (cliente e data) e CRUD dos itens.
    /// Recebe o código da compra pela rota: CompraPage?id=5 (sem id = nova compra).
    /// </summary>
    public partial class CompraPage : ContentPage, IQueryAttributable
    {
        readonly CompraService _compraService;
        readonly ClienteService _clienteService;
        readonly ProdutoService _produtoService;

        int _codigoCompra;
        int _codigoItemEditando;
        List<Cliente> _clientes = new();
        List<Produto> _produtos = new();

        public CompraPage(CompraService compraService, ClienteService clienteService, ProdutoService produtoService)
        {
            InitializeComponent();
            _compraService = compraService;
            _clienteService = clienteService;
            _produtoService = produtoService;

            PckCliente.ItemDisplayBinding = new Binding(nameof(Cliente.Nome));
            PckProduto.ItemDisplayBinding = new Binding(nameof(Produto.Descricao));
            DtpData.Date = DateTime.Today;
            DtpData.MaximumDate = DateTime.Today;
        }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.TryGetValue("id", out var id) && int.TryParse(id?.ToString(), out var codigo))
                _codigoCompra = codigo;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await this.ExecutarAsync(CarregarAsync);
        }

        async Task CarregarAsync()
        {
            _clientes = await _clienteService.ListarAsync();
            PckCliente.ItemsSource = _clientes;

            if (_clientes.Count == 0)
                await DisplayAlertAsync("Atenção", "Cadastre um cliente antes de registrar uma compra.", "OK");

            if (_codigoCompra != 0)
            {
                var compra = await _compraService.ObterAsync(_codigoCompra)
                    ?? throw new ValidacaoException("Compra não encontrada.");
                PckCliente.SelectedIndex = _clientes.FindIndex(c => c.CodigoCliente == compra.CodigoCliente);
                DtpData.Date = compra.DataCompra;
            }

            await CarregarItensAsync();
        }

        async Task CarregarItensAsync()
        {
            _produtos = await _produtoService.ListarAsync();
            PckProduto.ItemsSource = _produtos;
            LimparItem();

            var temCompra = _codigoCompra != 0;
            PainelItem.IsEnabled = temCompra;
            PainelItem.Opacity = temCompra ? 1 : 0.5;
            LblCompra.Text = temCompra ? $"Compra #{_codigoCompra}" : "Nova compra";
            LblStatus.Text = temCompra
                ? "Adicione, edite ou remova os itens abaixo"
                : "Escolha o cliente e a data e salve a compra";
            BtnSalvarCompra.Text = temCompra ? "Salvar alterações da compra" : "Salvar compra";

            if (!temCompra)
            {
                BindableLayout.SetItemsSource(ListaItens, null);
                LblContagemItens.Text = "Itens da compra";
                LblTotal.Text = 0m.ToString("C");
                return;
            }

            var itens = await _compraService.ListarItensAsync(_codigoCompra);
            BindableLayout.SetItemsSource(ListaItens, itens);
            LblContagemItens.Text = $"Itens da compra ({itens.Count})";
            LblTotal.Text = itens.Sum(i => i.ValorParcial).ToString("C");
        }

        void LimparItem()
        {
            _codigoItemEditando = 0;
            LblItem.Text = _codigoCompra != 0 ? "Adicionar item" : "Adicionar item (salve a compra primeiro)";
            BtnSalvarItem.Text = "Adicionar item";
            PckProduto.IsEnabled = true;
            PckProduto.SelectedIndex = -1;
            TxtQuantidade.Text = string.Empty;
        }

        async void OnSalvarCompraClicked(object? sender, EventArgs e)
        {
            await this.ExecutarAsync(async () =>
            {
                var data = DtpData.Date is DateTime d ? d : DateTime.Today;
                var compra = await _compraService.SalvarAsync(_codigoCompra, PckCliente.SelectedItem as Cliente, data);
                var nova = _codigoCompra == 0;
                _codigoCompra = compra.CodigoCompra;
                await CarregarItensAsync();
                await DisplayAlertAsync("Sucesso",
                    nova ? "Compra criada. Agora adicione os itens." : "Compra atualizada.", "OK");
            });
        }

        async void OnSalvarItemClicked(object? sender, EventArgs e)
        {
            await this.ExecutarAsync(async () =>
            {
                await _compraService.SalvarItemAsync(_codigoItemEditando, _codigoCompra,
                    PckProduto.SelectedItem as Produto, TxtQuantidade.Text);
                await CarregarItensAsync();
            });
        }

        void OnCancelarItemClicked(object? sender, EventArgs e) => LimparItem();

        void OnEditarItemClicked(object? sender, EventArgs e)
        {
            if ((sender as BindableObject)?.BindingContext is not ItemCompra item)
                return;

            _codigoItemEditando = item.CodigoItem;
            LblItem.Text = $"Editando item: {item.Produto?.NomeProduto}";
            BtnSalvarItem.Text = "Salvar item";
            PckProduto.SelectedIndex = _produtos.FindIndex(p => p.CodigoProduto == item.CodigoProduto);
            PckProduto.IsEnabled = false;
            TxtQuantidade.Text = item.Quantidade.ToString();
        }

        async void OnExcluirItemClicked(object? sender, EventArgs e)
        {
            if ((sender as BindableObject)?.BindingContext is not ItemCompra item)
                return;

            if (!await DisplayAlertAsync("Excluir", $"Remover {item.Produto?.NomeProduto} da compra?", "Sim", "Não"))
                return;

            await this.ExecutarAsync(async () =>
            {
                await _compraService.ExcluirItemAsync(item.CodigoItem);
                await CarregarItensAsync();
            });
        }
    }
}
