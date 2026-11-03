using TDE3_BancoDeDados.Models;
using TDE3_BancoDeDados.Services;

namespace TDE3_BancoDeDados.Views
{
    public partial class ClientesPage : ContentPage
    {
        readonly ClienteService _service;
        int _codigoEditando;

        public ClientesPage(ClienteService service)
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
            var clientes = await _service.ListarAsync(TxtBusca.Text);
            BindableLayout.SetItemsSource(Lista, clientes);
            LblContagem.Text = $"Clientes cadastrados ({clientes.Count})";
        }

        void LimparFormulario()
        {
            _codigoEditando = 0;
            LblModo.Text = "Novo cliente";
            LblDica.Text = "Preencha os dados e toque em Salvar";
            BtnSalvar.Text = "Cadastrar cliente";
            TxtNome.Text = TxtRg.Text = TxtTelefone.Text = string.Empty;
        }

        async void OnSalvarClicked(object? sender, EventArgs e)
        {
            await this.ExecutarAsync(async () =>
            {
                await _service.SalvarAsync(_codigoEditando, TxtNome.Text, TxtRg.Text, TxtTelefone.Text);
                await DisplayAlertAsync("Sucesso", "Cliente salvo com sucesso.", "OK");
                LimparFormulario();
                await CarregarAsync();
            });
        }

        void OnLimparClicked(object? sender, EventArgs e) => LimparFormulario();

        async void OnBuscaChanged(object? sender, TextChangedEventArgs e) =>
            await this.ExecutarAsync(CarregarAsync);

        void OnEditarClicked(object? sender, EventArgs e)
        {
            if ((sender as BindableObject)?.BindingContext is not Cliente cliente)
                return;

            _codigoEditando = cliente.CodigoCliente;
            LblModo.Text = $"Editando cliente #{cliente.CodigoCliente}";
            LblDica.Text = "Altere os dados e toque em Salvar alterações";
            BtnSalvar.Text = "Salvar alterações";
            TxtNome.Text = cliente.Nome;
            TxtRg.Text = cliente.RG.ToString();
            TxtTelefone.Text = cliente.Telefone;
        }

        async void OnExcluirClicked(object? sender, EventArgs e)
        {
            if ((sender as BindableObject)?.BindingContext is not Cliente cliente)
                return;

            if (!await DisplayAlertAsync("Excluir", $"Excluir o cliente {cliente.Nome}?", "Sim", "Não"))
                return;

            await this.ExecutarAsync(async () =>
            {
                await _service.ExcluirAsync(cliente.CodigoCliente);
                if (_codigoEditando == cliente.CodigoCliente)
                    LimparFormulario();
                await CarregarAsync();
            });
        }
    }
}
