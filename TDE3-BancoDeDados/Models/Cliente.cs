using SQLite;
using SQLiteNetExtensions.Attributes;

namespace TDE3_BancoDeDados.Models
{
    [Table("Cliente")]
    public class Cliente
    {
        [PrimaryKey, AutoIncrement]
        public int CodigoCliente { get; set; }

        [MaxLength(50), NotNull]
        public string Nome { get; set; } = string.Empty;

        public long RG { get; set; }

        [MaxLength(14), NotNull]
        public string Telefone { get; set; } = string.Empty;

        // Cliente (1,1) ---- (1,n) Compra
        [OneToMany]
        public List<Compra> Compras { get; set; } = new();

        // Primeira letra do nome, exibida no avatar da lista
        [Ignore]
        public string Inicial => string.IsNullOrWhiteSpace(Nome) ? "?" : Nome.Trim()[..1].ToUpper();
    }
}
