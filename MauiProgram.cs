using LiteDB;
using Microsoft.Extensions.Logging;
using EasyShop.Models;
using EasyShop.Data;
using EasyShop.Services;

namespace EasyShop;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

#if DEBUG
        builder.Logging.AddDebug();
#endif

        // =========================================================================
        // PERSISTÊNCIA POLIGLOTA - LADO NOSQL (PRODUTOS - LiteDB)
        // Responsável: Kauan
        // =========================================================================

        // 1. Instância ÚNICA (Singleton) do LiteDatabase apontando para products.db
        // É fundamental ser Singleton para evitar bloqueio de concorrência no arquivo .db
        builder.Services.AddSingleton<LiteDatabase>(sp =>
            new LiteDatabase(Path.Combine(FileSystem.AppDataDirectory, "products.db")));

        // 2. Registro da camada de Acesso a Dados (Data/Repository)
        builder.Services.AddSingleton<IProductRepository, ProductRepository>();

        // 3. Registro da camada de Negócio (Services)
        // A UI (Elenara) injetará IProductService nas Views/ViewModels, nunca acessando o banco diretamente
        builder.Services.AddSingleton<IProductService, ProductService>();

        // =========================================================================
        // PERSISTÊNCIA POLIGLOTA - LADO RELACIONAL (CLIENTES - SQLite)
        // Responsável: Hygor
        // =========================================================================
        // builder.Services.AddSingleton<ICustomerRepository, ...>();
        // builder.Services.AddSingleton<ICustomerService, ...>();

        return builder.Build();
    }
}
