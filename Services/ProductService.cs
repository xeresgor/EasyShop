using EasyShop.Data.Interfaces;
using EasyShop.Models;
using EasyShop.Services.Interfaces;

namespace EasyShop.Services;

/// <summary>
/// Regras de negócio do catálogo: valida nome e preço e converte o texto de atributos
/// digitado pelo usuário ("cor=azul; tamanho=M") no dicionário que vira documento no LiteDB.
/// </summary>
public class ProductService : IProductService
{
    private readonly IProductRepository _repository;

    public ProductService(IProductRepository repository)
    {
        _repository = repository;
    }

    public Task<List<Product>> GetAllAsync() => _repository.GetAllAsync();

    public async Task AddAsync(string name, decimal price, string? attributesText)
    {
        name = (name ?? "").Trim();

        if (name.Length == 0)
            throw new BusinessRuleException("O nome do produto é obrigatório.");

        if (price <= 0)
            throw new BusinessRuleException("O preço deve ser maior que zero.");

        var product = new Product
        {
            Name = name,
            Price = decimal.Round(price, 2),
            Attributes = ParseAttributes(attributesText)
        };

        await _repository.InsertAsync(product);
    }

    /// <summary>Converte "cor=azul; tamanho=M" em { cor: azul, tamanho: M }.</summary>
    private static Dictionary<string, string> ParseAttributes(string? text)
    {
        var result = new Dictionary<string, string>();
        if (string.IsNullOrWhiteSpace(text))
            return result;

        foreach (var part in text.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var idx = part.IndexOf('=');
            if (idx <= 0 || idx == part.Length - 1)
                throw new BusinessRuleException($"Atributo inválido: \"{part}\". Use o formato cor=azul; tamanho=M.");

            var key = part[..idx].Trim().ToLowerInvariant();
            var value = part[(idx + 1)..].Trim();

            // Chaves de documento não podem começar com "$" nem conter "." (regra do BSON).
            if (key.StartsWith('$') || key.Contains('.'))
                throw new BusinessRuleException($"Nome de atributo inválido: \"{key}\". Não use \".\" nem \"$\".");

            if (result.ContainsKey(key))
                throw new BusinessRuleException($"O atributo \"{key}\" foi informado mais de uma vez.");

            result[key] = value;
        }

        return result;
    }
}
