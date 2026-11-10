using EasyShop.Data.Interfaces;
using EasyShop.Models;
using SQLite;

namespace EasyShop.Data;

public class CustomerRepository : ICustomerRepository
{
    private const string DbFileName = "easyshop.db3";

    private readonly SemaphoreSlim _initLock = new(1, 1);
    private SQLiteAsyncConnection? _db;

    // Cria a conexão e a tabela uma única vez, mesmo com chamadas simultâneas.
    private async Task<SQLiteAsyncConnection> GetConnectionAsync()
    {
        if (_db is not null)
            return _db;

        await _initLock.WaitAsync();
        try
        {
            if (_db is null)
            {
                var path = Path.Combine(FileSystem.AppDataDirectory, DbFileName);
                var connection = new SQLiteAsyncConnection(path);
                await connection.CreateTableAsync<Customer>();
                _db = connection; // só publica depois da tabela existir
            }
            return _db;
        }
        finally
        {
            _initLock.Release();
        }
    }

    public async Task<List<Customer>> GetAllAsync()
    {
        var db = await GetConnectionAsync();
        return await db.Table<Customer>().ToListAsync();
    }

    public async Task InsertAsync(Customer c)
    {
        var db = await GetConnectionAsync();
        await db.InsertAsync(c);
    }

    public async Task<bool> EmailExistsAsync(string email)
    {
        var db = await GetConnectionAsync();
        var normalized = email.Trim().ToLower();
        var count = await db.Table<Customer>()
                            .Where(c => c.Email.ToLower() == normalized)
                            .CountAsync();
        return count > 0;
    }
}
