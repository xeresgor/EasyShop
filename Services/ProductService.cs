using EasyShop.Models;

namespace EasyShop.Services;

/// <summary>
/// Camada de Negócio (Services) para Produtos.
/// Valida as regras de negócio, realiza a higienização e conversão
/// dos dados de entrada (como o parser de atributos dinâmicos)
/// e orquestra a persistência via IProductRepository.
/// </summary>
public class ProductService : IProductService
{
    private readonly IProductRepository _repository;

    public ProductService(IProductRepository repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    /// <summary>
    /// Retorna todos os produtos do catálogo.
    /// </summary>
    public async Task<List<Product>> GetAllAsync()
    {
        return await _repository.GetAllAsync();
    }

    /// <summary>
    /// Cadastra um novo produto aplicando regras de negócio:
    /// - Nome é obrigatório;
    /// - Preço deve ser estritamente maior que zero;
    /// - Atributos dinâmicos são parseados e validados a partir do texto no formato "chave=valor; chave2=valor2".
    /// Lança ArgumentException com mensagem amigável em português quando inválido.
    /// </summary>
    public async Task AddAsync(string name, decimal price, string? attributesText)
    {
        // Regra 1: Nome obrigatório
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("O nome do produto é obrigatório.", nameof(name));
        }

        // Regra 2: Preço maior que zero
        if (price <= 0)
        {
            throw new ArgumentException("O preço do produto deve ser maior que zero.", nameof(price));
        }

        // Regra 3: Conversão e validação dos atributos dinâmicos
        var attributes = ParseAttributes(attributesText);

        var product = new Product
        {
            Name = name.Trim(),
            Price = price,
            Attributes = attributes
        };

        await _repository.InsertAsync(product);
    }

    /// <summary>
    /// Converte a string de atributos informada pelo usuário (ex: "cor=azul; tamanho=M")
    /// em um Dictionary&lt;string, string&gt;.
    /// 
    /// Tratamentos aplicados:
    /// - Espaços em branco extras antes ou depois de chaves, valores e separadores são removidos (.Trim());
    /// - Ponto-e-vírgulas adicionais ou vazios são ignorados;
    /// - Pares malformados (sem '=', com chave vazia ou valor vazio) lançam exceção em português;
    /// - Chaves duplicadas/repetidas lançam exceção explicativa em português;
    /// - Se a string for nula ou vazia, retorna um dicionário vazio (atributos são opcionais).
    /// </summary>
    /// <param name="attributesText">Texto contendo pares chave=valor separados por ponto e vírgula.</param>
    /// <returns>Dicionário com os atributos higienizados.</returns>
    /// <exception cref="ArgumentException">Lançada caso ocorra formato inválido ou chave repetida.</exception>
    public static Dictionary<string, string> ParseAttributes(string? attributesText)
    {
        var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        if (string.IsNullOrWhiteSpace(attributesText))
        {
            return result;
        }

        // Divide pelos pontos e vírgulas
        var parts = attributesText.Split(';');

        foreach (var rawPart in parts)
        {
            var part = rawPart.Trim();

            // Ignora partes vazias (ex: usuário digitou "cor=azul; tamanho=M;" com ';' no final)
            if (string.IsNullOrEmpty(part))
            {
                continue;
            }

            var separatorIndex = part.IndexOf('=');

            // Valida par inválido: ausência de '=', ou '=' no início, ou '=' no fim
            if (separatorIndex <= 0 || separatorIndex == part.Length - 1)
            {
                throw new ArgumentException(
                    $"O atributo '{part}' está em formato inválido. Utilize o formato 'chave=valor' (ex: cor=azul).");
            }

            var key = part.Substring(0, separatorIndex).Trim();
            var value = part.Substring(separatorIndex + 1).Trim();

            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentException(
                    $"O atributo '{part}' possui chave vazia. Utilize o formato 'chave=valor'.");
            }

            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException(
                    $"O atributo '{part}' possui valor vazio para a chave '{key}'. Utilize o formato 'chave=valor'.");
            }

            // Valida chave repetida (case-insensitive para evitar "Cor" e "cor" conflitantes)
            if (result.ContainsKey(key))
            {
                throw new ArgumentException(
                    $"A chave de atributo '{key}' foi informada mais de uma vez. Não utilize chaves repetidas.");
            }

            result.Add(key, value);
        }

        return result;
    }
}
