using System.Text.RegularExpressions;
using EasyShop.Data;
using EasyShop.Data.Interfaces;
using EasyShop.Models;
using EasyShop.Services.Interfaces;

namespace EasyShop.Services;

/// <summary>
/// Regras de negócio do cadastro de clientes: nome completo, e-mail válido e e-mail único.
/// A unicidade é checada aqui (mensagem amigável) e também garantida pelo índice UNIQUE do SQLite.
/// </summary>
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
        name = Regex.Replace((name ?? "").Trim(), @"\s+", " ");
        email = (email ?? "").Trim().ToLowerInvariant();

        if (name.Length == 0)
            throw new BusinessRuleException("O nome completo é obrigatório.");

        if (name.Split(' ').Length < 2)
            throw new BusinessRuleException("Informe o nome completo (nome e sobrenome).");

        if (email.Length == 0)
            throw new BusinessRuleException("O e-mail é obrigatório.");

        if (!EmailRegex.IsMatch(email))
            throw new BusinessRuleException("O e-mail informado não tem um formato válido (exemplo: nome@dominio.com).");

        if (await _repository.EmailExistsAsync(email))
            throw new BusinessRuleException("Já existe um cliente cadastrado com este e-mail.");

        try
        {
            await _repository.InsertAsync(new Customer { Name = name, Email = email });
        }
        catch (DuplicateEmailException)
        {
            // Rede de segurança: o índice UNIQUE do banco barrou o duplicado.
            throw new BusinessRuleException("Já existe um cliente cadastrado com este e-mail.");
        }
    }
}
