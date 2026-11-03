using System.Globalization;

namespace TDE3_BancoDeDados.Services
{
    /// <summary>
    /// Converte o texto digitado em números, aceitando vírgula ou ponto como separador decimal.
    /// </summary>
    static class Conversao
    {
        static readonly CultureInfo PtBr = new("pt-BR");

        public static bool TentarDecimal(string? texto, out decimal valor)
        {
            texto = (texto ?? string.Empty).Replace("R$", "").Trim();
            if (texto.Contains('.') && !texto.Contains(','))
                texto = texto.Replace('.', ',');
            return decimal.TryParse(texto, NumberStyles.Number, PtBr, out valor);
        }

        public static bool TentarInteiro(string? texto, out int valor) =>
            int.TryParse((texto ?? string.Empty).Trim(), NumberStyles.Integer, PtBr, out valor);
    }
}
