using EasyShop.Models;
using EasyShop.Services.Mocks;
using EasyShop.ViewModels;
using EasyShop.Views;

namespace EasyShop;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder.UseMauiApp<App>();

        // ── Serviços ───────────────────────────────────────────────────────────
        // HOJE: mocks em memória (desenvolvimento da UI).
        builder.Services.AddSingleton<ICustomerService, MockCustomerService>();
        builder.Services.AddSingleton<IProductService, MockProductService>();

        // DEPOIS: quando Hygor e Kauan entregarem, comente as 2 linhas acima,
        // descomente as 2 abaixo (e registre os repositórios deles, se houver).
        // builder.Services.AddSingleton<ICustomerService, EasyShop.Services.CustomerService>(); // Hygor (SQLite)
        // builder.Services.AddSingleton<IProductService, EasyShop.Services.ProductService>();   // Kauan (LiteDB)

        // ── Apresentação ───────────────────────────────────────────────────────
        builder.Services.AddTransient<ProductsViewModel>();
        builder.Services.AddTransient<CustomersViewModel>();
        builder.Services.AddTransient<ProductsPage>();
        builder.Services.AddTransient<CustomersPage>();

        return builder.Build();
    }
}
