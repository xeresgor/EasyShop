namespace EasyShop.Models;

// Documento da coleção "products" (LiteDB): sem schema fixo.
// Cada produto carrega o próprio conjunto de atributos (cor, tamanho, RAM, voltagem...).
public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public decimal Price { get; set; }
    public Dictionary<string, string> Attributes { get; set; } = new();
}
