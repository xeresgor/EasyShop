using EasyShop.Models;

namespace EasyShop.Services.Mocks;

/// <summary>
/// Implementação em memória de IProductService, apenas para desenvolver a UI
/// enquanto o serviço real (LiteDB) não fica pronto.
/// </summary>
public sealed class MockProductService : IProductService
{
    private readonly List<Product> _items = new()
    {
        new Product
        {
            Id = 1, Name = "Camiseta Básica", Price = 59.90m,
            Attributes = new() { ["cor"] = "azul", ["tamanho"] = "M" }
        },
        new Product
        {
            Id = 2, Name = "Notebook Pro 14", Price = 5499.00m,
            Attributes = new() { ["processador"] = "Ryzen 7", ["ram"] = "16GB", ["garantia"] = "24 meses" }
        },
    };
    private int _nextId = 3;

    public async Task<List<Product>> GetAllAsync()
    {
        await Task.Delay(150); // simula latência de banco
        return _items.Select(p => new Product
        {
            Id = p.Id,
            Name = p.Name,
            Price = p.Price,
            Attributes = new Dictionary<string, string>(p.Attributes)
        }).ToList();
    }

    public async Task AddAsync(string name, decimal price, string? attributesText)
    {
        await Task.Delay(150);

        name = (name ?? "").Trim();
        if (name.Length == 0)
            throw new ArgumentException("Informe o nome do produto.");

        if (price <= 0)
            throw new ArgumentException("O preço deve ser maior que zero.");

        _items.Add(new Product
        {
            Id = _nextId++,
            Name = name,
            Price = price,
            Attributes = ParseAttributes(attributesText)
        });
    }

    /// <summary>Converte "cor=azul; tamanho=M" em dicionário.</summary>
    private static Dictionary<string, string> ParseAttributes(string? text)
    {
        var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        if (string.IsNullOrWhiteSpace(text)) return result;

        foreach (var part in text.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var idx = part.IndexOf('=');
            if (idx <= 0 || idx == part.Length - 1)
                throw new ArgumentException($"Atributo inválido: \"{part}\". Use o formato cor=azul; tamanho=M.");

            result[part[..idx].Trim()] = part[(idx + 1)..].Trim();
        }
        return result;
    }
}
