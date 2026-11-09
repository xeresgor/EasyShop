namespace EasyShop.Models;

// CONTRATO DO GRUPO. AddAsync lança exceção com mensagem amigável se os dados forem inválidos.
public interface ICustomerService
{
    Task<List<Customer>> GetAllAsync();
    Task AddAsync(string name, string email);
}
