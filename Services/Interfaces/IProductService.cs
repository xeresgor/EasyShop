using EasyShop.Models;

namespace EasyShop.Services.Interfaces;

// CONTRATO DO GRUPO. attributesText no formato "cor=azul; tamanho=M" (opcional).
// AddAsync lança BusinessRuleException com mensagem amigável se os dados forem inválidos.
public interface IProductService
{
    Task<List<Product>> GetAllAsync();
    Task AddAsync(string name, decimal price, string? attributesText);
}
