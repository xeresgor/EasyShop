using EasyShop.Models;

namespace EasyShop.Data.Interfaces;

public interface ICustomerRepository
{
    Task<List<Customer>> GetAllAsync();
    Task InsertAsync(Customer c);
    Task<bool> EmailExistsAsync(string email);
}
