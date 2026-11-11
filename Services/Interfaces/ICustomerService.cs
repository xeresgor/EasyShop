using EasyShop.Models;

namespace EasyShop.Services.Interfaces;

// CONTRATO DO GRUPO. AddAsync lança BusinessRuleException com mensagem amigável se os dados forem inválidos.
public interface ICustomerService
{
    Task<List<Customer>> GetAllAsync();
    Task AddAsync(string name, string email);
}
