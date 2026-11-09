using EasyShop.Models;
using EasyShop.Services;
using Xunit;

namespace EasyShop.Tests;

/// <summary>
/// Testes unitários para o parser de atributos dinâmicos e regras do ProductService.
/// </summary>
public class AttributeParserTests
{
    [Fact]
    public void ParseAttributes_EntradaValida_RetornaDicionarioCorreto()
    {
        // Arrange
        var input = "cor=azul; tamanho=M; marca=Nike";

        // Act
        var result = ProductService.ParseAttributes(input);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.Count);
        Assert.Equal("azul", result["cor"]);
        Assert.Equal("M", result["tamanho"]);
        Assert.Equal("Nike", result["marca"]);
    }

    [Fact]
    public void ParseAttributes_ComEspacosExtras_AplicaTrimEmChavesEValores()
    {
        // Arrange
        var input = "   cor  =  azul marinho   ;   tamanho  =   GG   ";

        // Act
        var result = ProductService.ParseAttributes(input);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Equal("azul marinho", result["cor"]);
        Assert.Equal("GG", result["tamanho"]);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void ParseAttributes_NuloOuVazio_RetornaDicionarioVazio(string? input)
    {
        // Act
        var result = ProductService.ParseAttributes(input);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public void ParseAttributes_PontosEVirgulaExtrasNoFim_IgnoraPartesVazias()
    {
        // Arrange
        var input = "cor=preto; voltagem=220V;;  ; ";

        // Act
        var result = ProductService.ParseAttributes(input);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Equal("preto", result["cor"]);
        Assert.Equal("220V", result["voltagem"]);
    }

    [Fact]
    public void ParseAttributes_ParInvalidoSemSinalDeIgual_LancaArgumentException()
    {
        // Arrange
        var input = "cor=azul; tamanho_sem_igual; marca=Dell";

        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() => ProductService.ParseAttributes(input));
        Assert.Contains("formato inválido", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ParseAttributes_ChaveVazia_LancaArgumentException()
    {
        // Arrange
        var input = "=azul; tamanho=M";

        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() => ProductService.ParseAttributes(input));
        Assert.Contains("formato inválido", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ParseAttributes_ValorVazio_LancaArgumentException()
    {
        // Arrange
        var input = "cor=; tamanho=M";

        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() => ProductService.ParseAttributes(input));
        Assert.Contains("formato inválido", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ParseAttributes_ChavesDuplicadas_LancaArgumentException()
    {
        // Arrange
        var input = "cor=azul; tamanho=M; cor=vermelho";

        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() => ProductService.ParseAttributes(input));
        Assert.Contains("mais de uma vez", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task AddAsync_NomeInvalido_LancaArgumentException(string? nomeInvalido)
    {
        // Arrange
        var fakeRepo = new FakeProductRepository();
        var service = new ProductService(fakeRepo);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ArgumentException>(() =>
            service.AddAsync(nomeInvalido!, 100m, "cor=azul"));

        Assert.Contains("nome do produto é obrigatório", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    [InlineData(-0.01)]
    public async Task AddAsync_PrecoMenorOuIgualAZero_LancaArgumentException(decimal precoInvalido)
    {
        // Arrange
        var fakeRepo = new FakeProductRepository();
        var service = new ProductService(fakeRepo);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ArgumentException>(() =>
            service.AddAsync("Produto Teste", precoInvalido, "cor=azul"));

        Assert.Contains("maior que zero", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task AddAsync_DadosValidos_InsereProdutoComAtributosNoRepositorio()
    {
        // Arrange
        var fakeRepo = new FakeProductRepository();
        var service = new ProductService(fakeRepo);

        // Act
        await service.AddAsync("Camiseta Geek", 59.90m, "tamanho=GG; cor=Preta");

        // Assert
        Assert.Single(fakeRepo.SavedProducts);
        var saved = fakeRepo.SavedProducts[0];
        Assert.Equal("Camiseta Geek", saved.Name);
        Assert.Equal(59.90m, saved.Price);
        Assert.Equal("GG", saved.Attributes["tamanho"]);
        Assert.Equal("Preta", saved.Attributes["cor"]);
    }

    // Repositório Fake para isolar o teste do ProductService sem depender de disco
    private class FakeProductRepository : IProductRepository
    {
        public List<Product> SavedProducts { get; } = new();

        public Task<List<Product>> GetAllAsync() => Task.FromResult(SavedProducts);

        public Task InsertAsync(Product p)
        {
            SavedProducts.Add(p);
            return Task.CompletedTask;
        }
    }
}
