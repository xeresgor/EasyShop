using EasyShop.Models;

namespace EasyShop.Services.Interfaces;

public interface IProductService
{
    Task<List<Product>> GetAllAsync();
    Task AddAsync(string name, decimal price, string? attributesText);
}
