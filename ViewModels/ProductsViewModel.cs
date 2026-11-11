using System.Collections.ObjectModel;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EasyShop.Models;
using EasyShop.Services.Interfaces;

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

    /// <summary>
    /// Converte o texto digitado em decimal (conversão de entrada é papel da apresentação;
    /// a regra "preço maior que zero" fica no serviço). Aceita:
    /// "1299,90" e "1.299,90" (padrão brasileiro), "1.299" (ponto como milhar) e "1299.90".
    /// </summary>
    private static bool TryParsePrice(string? text, out decimal price)
    {
        var value = (text ?? string.Empty).Replace("R$", "").Replace(" ", "").Trim();

        if (value.Contains(','))
        {
            // Padrão brasileiro: ponto = milhar, vírgula = decimal.
            value = value.Replace(".", "").Replace(',', '.');
        }
        else
        {
            // Sem vírgula: "1.299" ou "1.299.000" são milhares; "1299.90" é decimal.
            var parts = value.Split('.');
            if (parts.Length > 1 && parts.Skip(1).All(p => p.Length == 3))
                value = value.Replace(".", "");
        }

        return decimal.TryParse(value, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out price);
    }
}
