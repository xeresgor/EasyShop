using System.Text.RegularExpressions;
using EasyShop.Models;

namespace EasyShop.Services.Mocks;

/// <summary>
/// Implementação em memória de ICustomerService, apenas para desenvolver a UI
/// enquanto o serviço real (SQLite) não fica pronto. Imita o contrato: lança
/// exceção com mensagem quando os dados são inválidos.
/// </summary>
public sealed class MockCustomerService : ICustomerService
{
    private static readonly Regex EmailRegex = new(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled);

    private readonly List<Customer> _items = new()
    {
        new Customer { Id = 1, Name = "Maria Souza", Email = "maria@exemplo.com" },
        new Customer { Id = 2, Name = "João Pereira", Email = "joao@exemplo.com" },
    };
    private int _nextId = 3;

    public async Task<List<Customer>> GetAllAsync()
    {
        await Task.Delay(150); // simula latência de banco
        return _items.Select(c => new Customer { Id = c.Id, Name = c.Name, Email = c.Email }).ToList();
    }

    public async Task AddAsync(string name, string email)
    {
        await Task.Delay(150);

        name = (name ?? "").Trim();
        email = (email ?? "").Trim().ToLowerInvariant();

        if (name.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length < 2)
            throw new ArgumentException("Informe o nome completo (nome e sobrenome).");

        if (!EmailRegex.IsMatch(email))
            throw new ArgumentException("Informe um e-mail válido.");

        if (_items.Any(c => c.Email == email))
            throw new InvalidOperationException("Já existe um cliente com este e-mail.");

        _items.Add(new Customer { Id = _nextId++, Name = name, Email = email });
    }
}
