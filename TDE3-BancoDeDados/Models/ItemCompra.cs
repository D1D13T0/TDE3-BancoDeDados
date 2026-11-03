using SQLite;
using SQLiteNetExtensions.Attributes;

namespace TDE3_BancoDeDados.Models
{
    [Table("ItemCompra")]
    public class ItemCompra
    {
        [PrimaryKey, AutoIncrement]
        public int CodigoItem { get; set; }

        [ForeignKey(typeof(Produto))]
        public int CodigoProduto { get; set; }

        [ForeignKey(typeof(Compra))]
        public int CodigoCompra { get; set; }

        public int Quantidade { get; set; }

        public decimal ValorParcial { get; set; }

        [ManyToOne]
        public Produto? Produto { get; set; }

        [ManyToOne]
        public Compra? Compra { get; set; }

        // Ex: "3 × R$ 12,50", exibido na lista de itens
        [Ignore]
        public string Detalhe => $"{Quantidade} × {(Quantidade > 0 ? ValorParcial / Quantidade : 0):C}";
    }
}
