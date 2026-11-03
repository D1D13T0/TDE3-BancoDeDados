using SQLiteNetExtensionsAsync.Extensions;
using TDE3_BancoDeDados.Data;
using TDE3_BancoDeDados.Models;

namespace TDE3_BancoDeDados.Services
{
    public class ProdutoService
    {
        readonly Database _db;

        public ProdutoService(Database db)
        {
            _db = db;
        }

        public async Task<List<Produto>> ListarAsync(string? filtro = null)
        {
            var conexao = await _db.ObterConexaoAsync();
            var consulta = conexao.Table<Produto>();
            if (!string.IsNullOrWhiteSpace(filtro))
            {
                var termo = filtro.Trim();
                consulta = consulta.Where(p => p.NomeProduto.Contains(termo) || p.Tipo.Contains(termo));
            }
            return await consulta.OrderBy(p => p.NomeProduto).ToListAsync();
        }

        public async Task<Produto> SalvarAsync(int codigo, string? nome, string? tipo, string? preco, string? qtdEstoque)
        {
            // ---- Validações ----
            nome = (nome ?? string.Empty).Trim();
            if (nome.Length == 0)
                throw new ValidacaoException("Informe o nome do produto.");
            if (nome.Length > 50)
                throw new ValidacaoException("O nome do produto deve ter no máximo 50 caracteres.");

            tipo = (tipo ?? string.Empty).Trim();
            if (tipo.Length == 0)
                throw new ValidacaoException("Informe o tipo do produto.");
            if (tipo.Length > 50)
                throw new ValidacaoException("O tipo deve ter no máximo 50 caracteres.");

            if (!Conversao.TentarDecimal(preco, out var valorPreco))
                throw new ValidacaoException("Informe um preço válido (ex: 19,90).");
            if (valorPreco <= 0)
                throw new ValidacaoException("O preço deve ser maior que zero.");
            if (valorPreco > 1_000_000)
                throw new ValidacaoException("O preço informado é alto demais.");
            if (decimal.Round(valorPreco, 2) != valorPreco)
                throw new ValidacaoException("O preço deve ter no máximo 2 casas decimais.");

            if (!Conversao.TentarInteiro(qtdEstoque, out var quantidade))
                throw new ValidacaoException("Informe uma quantidade em estoque válida (número inteiro).");
            if (quantidade < 0)
                throw new ValidacaoException("A quantidade em estoque não pode ser negativa.");

            var conexao = await _db.ObterConexaoAsync();

            if (codigo != 0 && await conexao.FindAsync<Produto>(codigo) == null)
                throw new ValidacaoException("Produto não encontrado.");

            var repetido = await conexao.Table<Produto>()
                .Where(p => p.NomeProduto == nome && p.Tipo == tipo && p.CodigoProduto != codigo)
                .CountAsync();
            if (repetido > 0)
                throw new ValidacaoException("Já existe um produto com esse nome e tipo.");

            // ---- Gravação ----
            var produto = new Produto
            {
                CodigoProduto = codigo,
                NomeProduto = nome,
                Tipo = tipo,
                Preco = valorPreco,
                QtdEstoque = quantidade
            };

            if (codigo == 0)
                await conexao.InsertAsync(produto);
            else
                await conexao.UpdateAsync(produto);

            return produto;
        }

        public async Task ExcluirAsync(int codigo)
        {
            var conexao = await _db.ObterConexaoAsync();

            // Carrega o produto junto com os itens de compra que o referenciam (relacionamento 1:N)
            var produto = await conexao.FindWithChildrenAsync<Produto>(codigo);
            if (produto == null)
                throw new ValidacaoException("Produto não encontrado.");
            if (produto.Itens.Count > 0)
                throw new ValidacaoException($"Não é possível excluir: o produto aparece em {produto.Itens.Count} item(ns) de compra.");

            await conexao.DeleteAsync<Produto>(codigo);
        }
    }
}
