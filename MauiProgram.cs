using Microsoft.Extensions.Logging;
// TODO: adicionar os usings de EasyShop.Data, EasyShop.Services e EasyShop.ViewModels conforme as classes forem criadas

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
        // builder.Services.AddSingleton<ICustomerRepository, CustomerRepository>();   // Hygor (Etapa 2)
        // TODO (Kauan): builder.Services.AddSingleton<IProductRepository, ProductRepository>();

        // ===== Negócio (Services) =====
        // builder.Services.AddSingleton<ICustomerService, CustomerService>();         // Hygor (Etapa 2)
        // TODO (Kauan): builder.Services.AddSingleton<IProductService, ProductService>();

        // ===== Apresentação (ViewModels) =====
        // TODO (Elenara): builder.Services.AddSingleton<CustomersViewModel>();
        // TODO (Elenara): builder.Services.AddSingleton<ProductsViewModel>();

        // ===== Views =====
        // TODO (Elenara): registrar as páginas (AddSingleton<CustomersPage>() etc.)

#if DEBUG
        builder.Logging.AddDebug();
#endif
        return builder.Build();
    }
}
