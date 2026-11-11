using SQLite;

namespace EasyShop.Models;

// Tabela relacional "Customers" (SQLite): schema fixo, chave primária e restrições garantidas pelo banco.
[Table("Customers")]
public class Customer
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [NotNull, MaxLength(120)]
    public string Name { get; set; } = "";

    // Índice UNIQUE: o próprio banco recusa um segundo cliente com o mesmo e-mail.
    [NotNull, Unique, MaxLength(160)]
    public string Email { get; set; } = "";
}
