using EasyShop.Models;

namespace EasyShop.Services.Interfaces;

public interface ICustomerService
{
    Task<List<Customer>> GetAllAsync();
    Task AddAsync(string name, string email);
}
