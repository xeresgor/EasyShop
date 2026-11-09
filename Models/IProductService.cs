namespace EasyShop.Models;

// CONTRATO DO GRUPO. attributesText no formato "cor=azul; tamanho=M".
// AddAsync lança exceção com mensagem amigável se os dados forem inválidos.
public interface IProductService
{
    Task<List<Product>> GetAllAsync();
    Task AddAsync(string name, decimal price, string? attributesText);
}
