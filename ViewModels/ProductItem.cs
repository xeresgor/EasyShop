using System.Globalization;
using EasyShop.Models;

namespace EasyShop.ViewModels;

/// <summary>
/// Item de exibição da lista de produtos. Mantém a formatação (preço em R$, atributos
/// em texto) na camada de apresentação, sem poluir o modelo compartilhado do grupo.
/// </summary>
public sealed class ProductItem
{
    private static readonly CultureInfo PtBr = CultureInfo.GetCultureInfo("pt-BR");

    public ProductItem(Product product)
    {
        Name = product.Name;
        PriceDisplay = product.Price.ToString("C", PtBr);
        HasAttributes = product.Attributes.Count > 0;
        AttributesDisplay = HasAttributes
            ? string.Join("  •  ", product.Attributes.Select(a => $"{a.Key}: {a.Value}"))
            : "Sem atributos adicionais";
    }

    public string Name { get; }
    public string PriceDisplay { get; }
    public string AttributesDisplay { get; }
    public bool HasAttributes { get; }
}
