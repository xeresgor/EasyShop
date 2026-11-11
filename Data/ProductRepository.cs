using EasyShop.Data.Interfaces;
using EasyShop.Models;
using LiteDB;

namespace EasyShop.Data;

/// <summary>
/// Acesso a dados NoSQL (LiteDB). Cada produto é gravado como um documento BSON na
/// coleção "products" — o dicionário de atributos vira um subdocumento livre, sem schema.
/// Registrado como singleton: uma única instância de LiteDatabase durante todo o app
/// (abrir e fechar o arquivo a cada operação pode gerar conflito de acesso ao arquivo).
/// </summary>
public sealed class ProductRepository : IProductRepository, IDisposable
{
    private const string DbFileName = "products.db";
    private const string CollectionName = "products";

    private readonly LiteDatabase _db;
    private readonly ILiteCollection<Product> _products;

    public ProductRepository()
    {
        var path = Path.Combine(FileSystem.AppDataDirectory, DbFileName);

        // Modo Direct (padrão). O modo Shared usa Mutex nomeado, que não funciona bem no Android.
        _db = new LiteDatabase($"Filename={path};Connection=Direct");
        _products = _db.GetCollection<Product>(CollectionName);
        _products.EnsureIndex(p => p.Name);
    }

    public Task<List<Product>> GetAllAsync() =>
        Task.Run(() => _products.FindAll().OrderBy(p => p.Name).ToList());

    public Task InsertAsync(Product p) =>
        Task.Run(() => { _products.Insert(p); });

    public void Dispose() => _db.Dispose();
}
