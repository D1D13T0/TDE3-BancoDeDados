using System.Text.RegularExpressions;
using SQLiteNetExtensionsAsync.Extensions;
using TDE3_BancoDeDados.Data;
using TDE3_BancoDeDados.Models;

namespace TDE3_BancoDeDados.Services
{
    public class ClienteService
    {
        readonly Database _db;

        public ClienteService(Database db)
        {
            _db = db;
        }

        public async Task<List<Cliente>> ListarAsync(string? filtro = null)
        {
            var conexao = await _db.ObterConexaoAsync();
            var consulta = conexao.Table<Cliente>();
            if (!string.IsNullOrWhiteSpace(filtro))
            {
                var termo = filtro.Trim();
                consulta = consulta.Where(c => c.Nome.Contains(termo));
            }
            return await consulta.OrderBy(c => c.Nome).ToListAsync();
        }

        public async Task<Cliente> SalvarAsync(int codigo, string? nome, string? rg, string? telefone)
        {
            // ---- Validações ----
            nome = (nome ?? string.Empty).Trim();
            if (nome.Length == 0)
                throw new ValidacaoException("Informe o nome do cliente.");
            if (nome.Length < 3)
                throw new ValidacaoException("O nome deve ter pelo menos 3 caracteres.");
            if (nome.Length > 50)
                throw new ValidacaoException("O nome deve ter no máximo 50 caracteres.");
            if (!Regex.IsMatch(nome, @"^[\p{L} .-]+$"))
                throw new ValidacaoException("O nome deve conter apenas letras.");

            rg = (rg ?? string.Empty).Trim();
            if (rg.Length == 0)
                throw new ValidacaoException("Informe o RG.");
            if (!Regex.IsMatch(rg, @"^\d{5,10}$"))
                throw new ValidacaoException("O RG deve conter apenas números (de 5 a 10 dígitos).");
            var numeroRg = long.Parse(rg);

            telefone = (telefone ?? string.Empty).Trim();
            if (telefone.Length == 0)
                throw new ValidacaoException("Informe o telefone.");
            if (telefone.Length > 14)
                throw new ValidacaoException("O telefone deve ter no máximo 14 caracteres.");
            if (!Regex.IsMatch(telefone, @"^[\d() -]+$"))
                throw new ValidacaoException("O telefone deve conter apenas números, espaços, parênteses e hífen.");
            var digitos = telefone.Count(char.IsDigit);
            if (digitos < 10 || digitos > 11)
                throw new ValidacaoException("O telefone deve ter DDD + número (10 ou 11 dígitos).");

            var conexao = await _db.ObterConexaoAsync();

            if (codigo != 0 && await conexao.FindAsync<Cliente>(codigo) == null)
                throw new ValidacaoException("Cliente não encontrado.");

            var rgRepetido = await conexao.Table<Cliente>()
                .Where(c => c.RG == numeroRg && c.CodigoCliente != codigo)
                .CountAsync();
            if (rgRepetido > 0)
                throw new ValidacaoException("Já existe um cliente cadastrado com esse RG.");

            // ---- Gravação ----
            var cliente = new Cliente
            {
                CodigoCliente = codigo,
                Nome = nome,
                RG = numeroRg,
                Telefone = telefone
            };

            if (codigo == 0)
                await conexao.InsertAsync(cliente);
            else
                await conexao.UpdateAsync(cliente);

            return cliente;
        }

        public async Task ExcluirAsync(int codigo)
        {
            var conexao = await _db.ObterConexaoAsync();

            // Carrega o cliente junto com as compras dele (relacionamento 1:N)
            var cliente = await conexao.FindWithChildrenAsync<Cliente>(codigo);
            if (cliente == null)
                throw new ValidacaoException("Cliente não encontrado.");
            if (cliente.Compras.Count > 0)
                throw new ValidacaoException($"Não é possível excluir: o cliente possui {cliente.Compras.Count} compra(s) registrada(s).");

            await conexao.DeleteAsync<Cliente>(codigo);
        }
    }
}
