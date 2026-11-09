namespace EasyShop.Models;

/// <summary>
/// Modelo de Produto para o catálogo do EasyShop armazenado no LiteDB (NoSQL de documentos).
/// A coleção é flexível, permitindo armazenar atributos dinâmicos sem schema fixo
/// através do dicionário Attributes (ex: tamanho, cor, processador, voltagem).
/// </summary>
public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public decimal Price { get; set; }
    public Dictionary<string, string> Attributes { get; set; } = new();
}

/// <summary>
/// Contrato da camada de Acesso a Dados (Data/Repository) para Produtos.
/// </summary>
public interface IProductRepository
{
    Task<List<Product>> GetAllAsync();
    Task InsertAsync(Product p);
}

/// <summary>
/// Contrato da camada de Negócio (Services) para Produtos.
/// </summary>
public interface IProductService
{
    Task<List<Product>> GetAllAsync();
    Task AddAsync(string name, decimal price, string? attributesText);
}
