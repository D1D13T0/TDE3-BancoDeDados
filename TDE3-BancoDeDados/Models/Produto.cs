using SQLite;
using SQLiteNetExtensions.Attributes;

namespace TDE3_BancoDeDados.Models
{
    [Table("Produto")]
    public class Produto
    {
        [PrimaryKey, AutoIncrement]
        public int CodigoProduto { get; set; }

        [MaxLength(50), NotNull]
        public string NomeProduto { get; set; } = string.Empty;

        [MaxLength(50), NotNull]
        public string Tipo { get; set; } = string.Empty;

        public decimal Preco { get; set; }

        public int QtdEstoque { get; set; }

        // Produto (1,1) ---- (1,n) ItemCompra
        [OneToMany]
        public List<ItemCompra> Itens { get; set; } = new();

        // Texto usado no Picker da tela de compra
        [Ignore]
        public string Descricao => $"{NomeProduto} - {Preco:C} (estoque: {QtdEstoque})";
    }
}
