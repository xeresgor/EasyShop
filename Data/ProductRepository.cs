using LiteDB;
using EasyShop.Models;

namespace EasyShop.Data;

/// <summary>
/// Repositório de Produtos implementado com LiteDB (NoSQL de Documentos).
/// Segue a arquitetura em camadas, encapsulando todo o acesso ao banco NoSQL.
/// As chamadas síncronas do LiteDB são executadas em Task.Run para garantir
/// a assincronia e não bloquear a Thread de UI do .NET MAUI.
/// </summary>
public class ProductRepository : IProductRepository, IDisposable
{
    private readonly LiteDatabase _database;
    private readonly ILiteCollection<Product> _collection;
    private readonly bool _ownsDatabase;

    public const string CollectionName = "products";

    /// <summary>
    /// Construtor principal para Injeção de Dependência (DI).
    /// Recebe a instância Singleton do LiteDatabase configurada no MauiProgram.cs.
    /// </summary>
    /// <param name="database">Instância única do LiteDatabase.</param>
    public ProductRepository(LiteDatabase database)
    {
        _database = database ?? throw new ArgumentNullException(nameof(database));
        _collection = _database.GetCollection<Product>(CollectionName);
        _ownsDatabase = false;
    }

    /// <summary>
    /// Construtor alternativo informando o caminho do banco. Útil para testes ou caminhos customizados.
    /// </summary>
    /// <param name="databasePath">Caminho completo do arquivo .db do LiteDB.</param>
    public ProductRepository(string databasePath)
    {
        if (string.IsNullOrWhiteSpace(databasePath))
            throw new ArgumentException("O caminho do banco de dados não pode ser vazio.", nameof(databasePath));

        _database = new LiteDatabase(databasePath);
        _collection = _database.GetCollection<Product>(CollectionName);
        _ownsDatabase = true;
    }

    /// <summary>
    /// Construtor padrão que busca o arquivo no diretório de dados da aplicação (.NET MAUI).
    /// </summary>
    public ProductRepository() : this(GetDefaultDatabasePath())
    {
    }

    private static string GetDefaultDatabasePath()
    {
#if ANDROID || IOS || MACCATALYST || (WINDOWS && !NET8_0)
        try
        {
            return Path.Combine(Microsoft.Maui.Storage.FileSystem.AppDataDirectory, "products.db");
        }
        catch
        {
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "products.db");
        }
#else
        // No .NET MAUI em runtime ou fallback para console/testes unitários
        try
        {
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            if (!string.IsNullOrEmpty(appData))
            {
                return Path.Combine(appData, "products.db");
            }
        }
        catch { }

        return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "products.db");
#endif
    }

    /// <summary>
    /// Obtém todos os produtos cadastrados de forma assíncrona.
    /// Caso a coleção esteja vazia, executa a carga inicial (seed) automaticamente.
    /// </summary>
    public async Task<List<Product>> GetAllAsync()
    {
        return await Task.Run(async () =>
        {
            // Auto-seed para a demonstração na primeira execução
            if (_collection.Count() == 0)
            {
                await SeedAsync();
            }

            return _collection.FindAll().ToList();
        });
    }

    /// <summary>
    /// Insere um novo produto no LiteDB de forma assíncrona.
    /// O LiteDB gera automaticamente o ID incremental para o produto.
    /// </summary>
    public Task InsertAsync(Product p)
    {
        if (p == null) throw new ArgumentNullException(nameof(p));

        return Task.Run(() =>
        {
            _collection.Insert(p);
        });
    }

    /// <summary>
    /// Carga inicial (seed) com 3 produtos de categorias bem distintas (Vestuário, Informática, Eletroportáteis).
    /// Demonstra a flexibilidade do NoSQL: armazenar atributos dinâmicos sem schema fixo.
    /// Executado apenas se a coleção estiver vazia.
    /// </summary>
    public Task SeedAsync()
    {
        return Task.Run(() =>
        {
            if (_collection.Count() > 0)
                return;

            var seedProducts = new List<Product>
            {
                // 1. Vestuário: camisa tem tamanho, cor, material e gênero
                new Product
                {
                    Name = "Camisa Polo Manga Curta",
                    Price = 89.90m,
                    Attributes = new Dictionary<string, string>
                    {
                        { "tamanho", "G" },
                        { "cor", "Azul Marinho" },
                        { "material", "100% Algodão" },
                        { "genero", "Masculino" },
                        { "categoria", "Vestuário" }
                    }
                },
                // 2. Informática: notebook tem processador, ram, armazenamento, tela, placa de vídeo
                new Product
                {
                    Name = "Notebook Gamer Dell G15",
                    Price = 5499.00m,
                    Attributes = new Dictionary<string, string>
                    {
                        { "processador", "Intel Core i7 13ª Ger" },
                        { "ram", "16GB DDR5" },
                        { "armazenamento", "SSD 512GB NVMe" },
                        { "placa_video", "NVIDIA GeForce RTX 4050 6GB" },
                        { "tela", "15.6 FHD 120Hz" },
                        { "categoria", "Informática" }
                    }
                },
                // 3. Eletroportáteis: liquidificador tem voltagem, potência, capacidade e velocidades
                new Product
                {
                    Name = "Liquidificador Turbo Inox",
                    Price = 189.90m,
                    Attributes = new Dictionary<string, string>
                    {
                        { "voltagem", "110V" },
                        { "potencia", "1200W" },
                        { "capacidade", "3.0 Litros" },
                        { "velocidades", "12 velocidades + Pulsar" },
                        { "categoria", "Eletroportáteis" }
                    }
                }
            };

            _collection.InsertBulk(seedProducts);
        });
    }

    public void Dispose()
    {
        if (_ownsDatabase)
        {
            _database.Dispose();
        }
    }
}
