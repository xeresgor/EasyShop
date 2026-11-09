using System.Collections.ObjectModel;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EasyShop.Models;

namespace EasyShop.ViewModels;

public partial class ProductsViewModel : ObservableObject
{
    private readonly IProductService _service;

    public ProductsViewModel(IProductService service)
    {
        _service = service;
    }

    public ObservableCollection<ProductItem> Products { get; } = new();

    // Campos do formulário
    [ObservableProperty] private string name = string.Empty;
    [ObservableProperty] private string priceText = string.Empty;
    [ObservableProperty] private string attributesText = string.Empty;

    // Feedback ao usuário (sucesso ou erro)
    [ObservableProperty, NotifyPropertyChangedFor(nameof(HasMessage))]
    private string message = string.Empty;

    [ObservableProperty] private bool isError;

    public bool HasMessage => !string.IsNullOrEmpty(Message);

    [RelayCommand]
    private async Task LoadAsync()
    {
        try
        {
            await RefreshListAsync();
        }
        catch (Exception ex)
        {
            ShowMessage(ex.Message, isError: true);
        }
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (!TryParsePrice(PriceText, out var price))
        {
            ShowMessage("Informe um preço válido (ex.: 1299,90).", isError: true);
            return;
        }

        try
        {
            await _service.AddAsync(Name, price, string.IsNullOrWhiteSpace(AttributesText) ? null : AttributesText);

            Name = PriceText = AttributesText = string.Empty;
            await RefreshListAsync();
            ShowMessage("Produto salvo com sucesso!", isError: false);
        }
        catch (Exception ex)
        {
            // Mensagem vinda do serviço (regra de negócio) é mostrada como está.
            ShowMessage(ex.Message, isError: true);
        }
    }

    private async Task RefreshListAsync()
    {
        var products = await _service.GetAllAsync();
        Products.Clear();
        foreach (var product in products)
            Products.Add(new ProductItem(product));
    }

    private void ShowMessage(string text, bool isError)
    {
        Message = text;
        IsError = isError;
    }

    /// <summary>Converte o texto digitado em decimal. Aceita "1299,90", "1.299,90" e "1299.90".</summary>
    private static bool TryParsePrice(string? text, out decimal price)
    {
        var value = (text ?? string.Empty).Replace("R$", "").Trim();
        if (value.Contains(','))
            value = value.Replace(".", "").Replace(',', '.');

        return decimal.TryParse(value, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out price);
    }
}
