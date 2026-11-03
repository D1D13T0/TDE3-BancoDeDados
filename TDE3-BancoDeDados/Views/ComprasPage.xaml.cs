using TDE3_BancoDeDados.Models;
using TDE3_BancoDeDados.Services;

namespace TDE3_BancoDeDados.Views
{
    public partial class ComprasPage : ContentPage
    {
        readonly CompraService _service;

        public ComprasPage(CompraService service)
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
            Lista.ItemsSource = await _service.ListarAsync();
        }

        async void OnNovaClicked(object? sender, EventArgs e) =>
            await Shell.Current.GoToAsync(nameof(CompraPage));

        async void OnAbrirClicked(object? sender, EventArgs e)
        {
            if ((sender as BindableObject)?.BindingContext is Compra compra)
                await Shell.Current.GoToAsync($"{nameof(CompraPage)}?id={compra.CodigoCompra}");
        }

        async void OnExcluirClicked(object? sender, EventArgs e)
        {
            if ((sender as BindableObject)?.BindingContext is not Compra compra)
                return;

            if (!await DisplayAlertAsync("Excluir",
                    $"Excluir a compra #{compra.CodigoCompra}? Os itens serão removidos e os produtos voltarão ao estoque.",
                    "Sim", "Não"))
                return;

            await this.ExecutarAsync(async () =>
            {
                await _service.ExcluirAsync(compra.CodigoCompra);
                await CarregarAsync();
            });
        }
    }
}
