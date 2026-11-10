using System.Text.RegularExpressions;
using EasyShop.Data.Interfaces;
using EasyShop.Models;
using EasyShop.Services.Interfaces;

namespace EasyShop.Services;

public class CustomerService : ICustomerService
{
    private static readonly Regex EmailRegex =
        new(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private readonly ICustomerRepository _repository;

    public CustomerService(ICustomerRepository repository)
    {
        _repository = repository;
    }

    public Task<List<Customer>> GetAllAsync() => _repository.GetAllAsync();

    public async Task AddAsync(string name, string email)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new BusinessRuleException("O nome é obrigatório.");

        if (string.IsNullOrWhiteSpace(email))
            throw new BusinessRuleException("O e-mail é obrigatório.");

        email = email.Trim().ToLower();

        if (!EmailRegex.IsMatch(email))
            throw new BusinessRuleException("O e-mail informado não tem um formato válido (exemplo: nome@dominio.com).");

        if (await _repository.EmailExistsAsync(email))
            throw new BusinessRuleException("Já existe um cliente cadastrado com este e-mail.");

        await _repository.InsertAsync(new Customer { Name = name.Trim(), Email = email });
    }
}
