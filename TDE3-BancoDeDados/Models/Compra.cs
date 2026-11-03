using SQLite;
using SQLiteNetExtensions.Attributes;

namespace TDE3_BancoDeDados.Models
{
    [Table("Compra")]
    public class Compra
    {
        [PrimaryKey, AutoIncrement]
        public int CodigoCompra { get; set; }

        public DateTime DataCompra { get; set; }

        public decimal ValorTotal { get; set; }

        [ForeignKey(typeof(Cliente))]
        public int CodigoCliente { get; set; }

        [ManyToOne]
        public Cliente? Cliente { get; set; }

        // Compra (1,1) ---- (1,n) ItemCompra; ao excluir a compra, os itens vão junto
        [OneToMany(CascadeOperations = CascadeOperation.CascadeRead | CascadeOperation.CascadeDelete)]
        public List<ItemCompra> Itens { get; set; } = new();
    }
}
