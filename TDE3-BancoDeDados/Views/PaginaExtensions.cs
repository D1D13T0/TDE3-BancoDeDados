using TDE3_BancoDeDados.Services;

namespace TDE3_BancoDeDados.Views
{
    static class PaginaExtensions
    {
        /// <summary>
        /// Executa uma ação e mostra um alerta caso a validação ou o banco de dados recuse a operação.
        /// </summary>
        public static async Task ExecutarAsync(this Page pagina, Func<Task> acao)
        {
            try
            {
                await acao();
            }
            catch (ValidacaoException ex)
            {
                await pagina.DisplayAlertAsync("Atenção", ex.Message, "OK");
            }
            catch (Exception ex)
            {
                await pagina.DisplayAlertAsync("Erro", "Erro ao acessar o banco de dados: " + ex.Message, "OK");
            }
        }
    }
}
