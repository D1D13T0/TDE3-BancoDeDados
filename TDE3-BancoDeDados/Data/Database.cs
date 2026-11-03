using SQLite;

namespace TDE3_BancoDeDados.Data
{
    /// <summary>
    /// Abre a conexão SQLite e cria o esquema relacional (com chaves estrangeiras).
    /// As tabelas são criadas com SQL para que as FOREIGN KEYs e CHECKs existam de fato no banco.
    /// </summary>
    public class Database
    {
        const string NomeArquivo = "loja.db3";

        static readonly string[] Esquema =
        {
            @"CREATE TABLE IF NOT EXISTS Cliente (
                CodigoCliente INTEGER PRIMARY KEY AUTOINCREMENT,
                Nome          VARCHAR(50) NOT NULL,
                RG            INTEGER     NOT NULL UNIQUE,
                Telefone      VARCHAR(14) NOT NULL
            )",
            @"CREATE TABLE IF NOT EXISTS Produto (
                CodigoProduto INTEGER PRIMARY KEY AUTOINCREMENT,
                NomeProduto   VARCHAR(50) NOT NULL,
                Tipo          VARCHAR(50) NOT NULL,
                Preco         MONEY       NOT NULL CHECK (Preco > 0),
                QtdEstoque    INTEGER     NOT NULL CHECK (QtdEstoque >= 0)
            )",
            @"CREATE TABLE IF NOT EXISTS Compra (
                CodigoCompra  INTEGER PRIMARY KEY AUTOINCREMENT,
                DataCompra    DATE    NOT NULL,
                ValorTotal    MONEY   NOT NULL DEFAULT 0,
                CodigoCliente INTEGER NOT NULL,
                FOREIGN KEY (CodigoCliente) REFERENCES Cliente (CodigoCliente) ON DELETE RESTRICT
            )",
            @"CREATE TABLE IF NOT EXISTS ItemCompra (
                CodigoItem    INTEGER PRIMARY KEY AUTOINCREMENT,
                CodigoProduto INTEGER NOT NULL,
                CodigoCompra  INTEGER NOT NULL,
                Quantidade    INTEGER NOT NULL CHECK (Quantidade > 0),
                ValorParcial  MONEY   NOT NULL,
                UNIQUE (CodigoCompra, CodigoProduto),
                FOREIGN KEY (CodigoProduto) REFERENCES Produto (CodigoProduto) ON DELETE RESTRICT,
                FOREIGN KEY (CodigoCompra)  REFERENCES Compra (CodigoCompra)   ON DELETE CASCADE
            )"
        };

        readonly SemaphoreSlim _trava = new(1, 1);
        SQLiteAsyncConnection? _conexao;

        public static string Caminho => Path.Combine(FileSystem.AppDataDirectory, NomeArquivo);

        public async Task<SQLiteAsyncConnection> ObterConexaoAsync()
        {
            if (_conexao != null)
                return _conexao;

            await _trava.WaitAsync();
            try
            {
                if (_conexao == null)
                {
                    var conexao = new SQLiteAsyncConnection(Caminho,
                        SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create | SQLiteOpenFlags.FullMutex,
                        storeDateTimeAsTicks: false);

                    // O SQLite só valida chaves estrangeiras com essa opção ligada
                    await conexao.ExecuteAsync("PRAGMA foreign_keys = ON");

                    foreach (var sql in Esquema)
                        await conexao.ExecuteAsync(sql);

                    _conexao = conexao;
                }
            }
            finally
            {
                _trava.Release();
            }

            return _conexao;
        }
    }
}
