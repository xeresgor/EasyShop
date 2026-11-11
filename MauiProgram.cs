using Microsoft.Extensions.Logging;
using EasyShop.Data;
using EasyShop.Data.Interfaces;
using EasyShop.Services;
using EasyShop.Services.Interfaces;
using EasyShop.ViewModels;
using EasyShop.Views;

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

        // ===== Acesso a Dados (Repositories) =====
        // Singletons: uma conexão SQLite e uma instância LiteDatabase para o app inteiro.
        builder.Services.AddSingleton<ICustomerRepository, CustomerRepository>();   // SQL   (SQLite)
        builder.Services.AddSingleton<IProductRepository, ProductRepository>();     // NoSQL (LiteDB)

        // ===== Negócio (Services) =====
        builder.Services.AddSingleton<ICustomerService, CustomerService>();
        builder.Services.AddSingleton<IProductService, ProductService>();

        // ===== Apresentação (ViewModels e Views) =====
        // As ViewModels só conhecem as interfaces de serviço — nunca SQLite ou LiteDB.
        builder.Services.AddTransient<ProductsViewModel>();
        builder.Services.AddTransient<CustomersViewModel>();
        builder.Services.AddTransient<ProductsPage>();
        builder.Services.AddTransient<CustomersPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif
        return builder.Build();
    }
}
