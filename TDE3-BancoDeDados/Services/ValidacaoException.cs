namespace TDE3_BancoDeDados.Services
{
    /// <summary>
    /// Erro de validação de dados, lançado antes de qualquer gravação no banco.
    /// </summary>
    public class ValidacaoException : Exception
    {
        public ValidacaoException(string mensagem) : base(mensagem) { }
    }
}
