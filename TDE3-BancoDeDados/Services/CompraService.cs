using SQLite;
using SQLiteNetExtensions.Extensions;
using SQLiteNetExtensionsAsync.Extensions;
using TDE3_BancoDeDados.Data;
using TDE3_BancoDeDados.Models;

namespace TDE3_BancoDeDados.Services
{
    /// <summary>
    /// CRUD das tabelas Compra e ItemCompra. Cada alteração de item atualiza o estoque do produto
    /// e o valor total da compra dentro de uma mesma transação.
    /// </summary>
    public class CompraService
    {
        readonly Database _db;

        public CompraService(Database db)
        {
            _db = db;
        }

        // ================= Compra =================

        public async Task<List<Compra>> ListarAsync()
        {
            var conexao = await _db.ObterConexaoAsync();
            // Traz cada compra com o Cliente (N:1) e os Itens (1:N) preenchidos
            var compras = await conexao.GetAllWithChildrenAsync<Compra>();
            return compras
                .OrderByDescending(c => c.DataCompra)
                .ThenByDescending(c => c.CodigoCompra)
                .ToList();
        }

        public async Task<Compra?> ObterAsync(int codigo)
        {
            var conexao = await _db.ObterConexaoAsync();
            return await conexao.FindWithChildrenAsync<Compra>(codigo);
        }

        public async Task<Compra> SalvarAsync(int codigo, Cliente? cliente, DateTime data)
        {
            // ---- Validações ----
            if (cliente == null)
                throw new ValidacaoException("Selecione o cliente da compra.");
            if (data.Date > DateTime.Today)
                throw new ValidacaoException("A data da compra não pode ser futura.");
            if (data.Date < new DateTime(2000, 1, 1))
                throw new ValidacaoException("Informe uma data de compra válida.");

            var conexao = await _db.ObterConexaoAsync();

            if (await conexao.FindAsync<Cliente>(cliente.CodigoCliente) == null)
                throw new ValidacaoException("Cliente não encontrado.");

            // ---- Gravação ----
            Compra compra;
            if (codigo == 0)
            {
                compra = new Compra
                {
                    DataCompra = data.Date,
                    CodigoCliente = cliente.CodigoCliente,
                    ValorTotal = 0
                };
                await conexao.InsertAsync(compra);
            }
            else
            {
                compra = await conexao.FindAsync<Compra>(codigo)
                    ?? throw new ValidacaoException("Compra não encontrada.");
                compra.DataCompra = data.Date;
                compra.CodigoCliente = cliente.CodigoCliente;
                await conexao.UpdateAsync(compra);
            }

            return compra;
        }

        public async Task ExcluirAsync(int codigo)
        {
            var conexao = await _db.ObterConexaoAsync();

            await conexao.RunInTransactionAsync(c =>
            {
                var compra = c.FindWithChildren<Compra>(codigo)
                    ?? throw new ValidacaoException("Compra não encontrada.");

                // Devolve ao estoque os produtos da compra
                foreach (var item in compra.Itens)
                    AlterarEstoque(c, item.CodigoProduto, item.Quantidade);

                // Exclusão em cascata: remove a compra e seus itens
                c.Delete(compra, recursive: true);
            });
        }

        // ================= ItemCompra =================

        public async Task<List<ItemCompra>> ListarItensAsync(int codigoCompra)
        {
            var conexao = await _db.ObterConexaoAsync();
            // Traz cada item com o Produto (N:1) preenchido
            var itens = await conexao.GetAllWithChildrenAsync<ItemCompra>(i => i.CodigoCompra == codigoCompra);
            return itens.OrderBy(i => i.CodigoItem).ToList();
        }

        /// <summary>
        /// Inclui (codigoItem = 0) ou altera a quantidade de um item da compra.
        /// Se o produto já estiver na compra, a quantidade é somada ao item existente.
        /// </summary>
        public async Task SalvarItemAsync(int codigoItem, int codigoCompra, Produto? produto, string? quantidade)
        {
            // ---- Validações ----
            if (codigoCompra == 0)
                throw new ValidacaoException("Salve os dados da compra antes de adicionar itens.");
            if (produto == null)
                throw new ValidacaoException("Selecione um produto.");
            if (!Conversao.TentarInteiro(quantidade, out var qtd))
                throw new ValidacaoException("Informe uma quantidade válida (número inteiro).");
            if (qtd <= 0)
                throw new ValidacaoException("A quantidade deve ser maior que zero.");
            if (qtd > 10_000)
                throw new ValidacaoException("Quantidade alta demais para um único item.");

            var conexao = await _db.ObterConexaoAsync();

            await conexao.RunInTransactionAsync(c =>
            {
                if (c.Find<Compra>(codigoCompra) == null)
                    throw new ValidacaoException("Compra não encontrada.");

                var prod = c.Find<Produto>(produto.CodigoProduto)
                    ?? throw new ValidacaoException("Produto não encontrado.");

                var item = codigoItem != 0
                    ? c.Find<ItemCompra>(codigoItem) ?? throw new ValidacaoException("Item não encontrado.")
                    : c.Table<ItemCompra>().FirstOrDefault(i => i.CodigoCompra == codigoCompra && i.CodigoProduto == prod.CodigoProduto);

                var qtdAnterior = 0;
                var precoUnitario = prod.Preco;
                if (item != null)
                {
                    if (item.CodigoProduto != prod.CodigoProduto)
                        throw new ValidacaoException("Não é possível trocar o produto de um item. Exclua o item e adicione outro.");
                    qtdAnterior = item.Quantidade;
                    // Mantém o preço unitário da época em que o item foi lançado
                    precoUnitario = item.ValorParcial / item.Quantidade;
                }

                var novaQtd = codigoItem != 0 ? qtd : qtdAnterior + qtd;
                var diferenca = novaQtd - qtdAnterior;
                if (diferenca > prod.QtdEstoque)
                    throw new ValidacaoException($"Estoque insuficiente de {prod.NomeProduto}. Disponível: {prod.QtdEstoque}.");

                var valorParcial = decimal.Round(precoUnitario * novaQtd, 2);

                if (item == null)
                {
                    c.Insert(new ItemCompra
                    {
                        CodigoCompra = codigoCompra,
                        CodigoProduto = prod.CodigoProduto,
                        Quantidade = novaQtd,
                        ValorParcial = valorParcial
                    });
                }
                else
                {
                    item.Quantidade = novaQtd;
                    item.ValorParcial = valorParcial;
                    c.Update(item);
                }

                AlterarEstoque(c, prod.CodigoProduto, -diferenca);
                AtualizarValorTotal(c, codigoCompra);
            });
        }

        public async Task ExcluirItemAsync(int codigoItem)
        {
            var conexao = await _db.ObterConexaoAsync();

            await conexao.RunInTransactionAsync(c =>
            {
                var item = c.Find<ItemCompra>(codigoItem)
                    ?? throw new ValidacaoException("Item não encontrado.");

                AlterarEstoque(c, item.CodigoProduto, item.Quantidade);
                c.Delete<ItemCompra>(codigoItem);
                AtualizarValorTotal(c, item.CodigoCompra);
            });
        }

        // ================= Auxiliares =================

        static void AlterarEstoque(SQLiteConnection c, int codigoProduto, int quantidade) =>
            c.Execute("UPDATE Produto SET QtdEstoque = QtdEstoque + ? WHERE CodigoProduto = ?",
                quantidade, codigoProduto);

        static void AtualizarValorTotal(SQLiteConnection c, int codigoCompra) =>
            c.Execute(@"UPDATE Compra
                        SET ValorTotal = (SELECT IFNULL(SUM(ValorParcial), 0) FROM ItemCompra WHERE CodigoCompra = ?)
                        WHERE CodigoCompra = ?",
                codigoCompra, codigoCompra);
    }
}
