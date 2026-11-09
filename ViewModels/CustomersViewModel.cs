using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EasyShop.Models;

namespace EasyShop.ViewModels;

public partial class CustomersViewModel : ObservableObject
{
    private readonly ICustomerService _service;

    public CustomersViewModel(ICustomerService service)
    {
        _service = service;
    }

    public ObservableCollection<Customer> Customers { get; } = new();

    // Campos do formulário
    [ObservableProperty] private string name = string.Empty;
    [ObservableProperty] private string email = string.Empty;

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
        try
        {
            await _service.AddAsync(Name, Email);

            Name = Email = string.Empty;
            await RefreshListAsync();
            ShowMessage("Cliente salvo com sucesso!", isError: false);
        }
        catch (Exception ex)
        {
            // Mensagem vinda do serviço (regra de negócio) é mostrada como está.
            ShowMessage(ex.Message, isError: true);
        }
    }

    private async Task RefreshListAsync()
    {
        var customers = await _service.GetAllAsync();
        Customers.Clear();
        foreach (var customer in customers)
            Customers.Add(customer);
    }

    private void ShowMessage(string text, bool isError)
    {
        Message = text;
        IsError = isError;
    }
}
