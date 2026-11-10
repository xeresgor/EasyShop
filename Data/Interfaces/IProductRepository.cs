using EasyShop.Models;

namespace EasyShop.Data.Interfaces;

public interface IProductRepository
{
    Task<List<Product>> GetAllAsync();
    Task InsertAsync(Product p);
}
