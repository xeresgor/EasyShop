using EasyShop.Data;
using EasyShop.Models;
using EasyShop.Services;
using LiteDB;

namespace EasyShop.Tests;

/// <summary>
/// Executável de console para demonstração rápida e validação imediata
/// do parser de atributos dinâmicos e do repositório NoSQL LiteDB.
/// </summary>
public static class Program
{
    public static async Task Main(string[] args)
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("====================================================================");
        Console.WriteLine("          EASYSHOP - TESTES DO LADO NOSQL (PRODUTOS)               ");
        Console.WriteLine("====================================================================\n");
        Console.ResetColor();

        int passed = 0;
        int failed = 0;

        void RunTest(string testName, Action testAction)
        {
            try
            {
                testAction();
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write("[PASSOU] ");
                Console.ResetColor();
                Console.WriteLine(testName);
                passed++;
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write("[FALHOU] ");
                Console.ResetColor();
                Console.WriteLine($"{testName} -> {ex.Message}");
                failed++;
            }
        }

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("--- 1. TESTES DO PARSER DE ATRIBUTOS (ProductService.ParseAttributes) ---");
        Console.ResetColor();

        RunTest("Parse de atributos válidos ('cor=azul; tamanho=M; marca=Nike')", () =>
        {
            var dict = ProductService.ParseAttributes("cor=azul; tamanho=M; marca=Nike");
            if (dict.Count != 3) throw new Exception($"Esperava 3 itens, obteve {dict.Count}");
            if (dict["cor"] != "azul") throw new Exception("Valor de 'cor' incorreto");
            if (dict["tamanho"] != "M") throw new Exception("Valor de 'tamanho' incorreto");
            if (dict["marca"] != "Nike") throw new Exception("Valor de 'marca' incorreto");
        });

        RunTest("Tratamento de espaços em branco antes/depois de chaves e valores", () =>
        {
            var dict = ProductService.ParseAttributes("  cor  =  azul escuro  ;   tamanho  =   GG   ");
            if (dict["cor"] != "azul escuro") throw new Exception($"'cor' não foi trimado corretamente: '{dict["cor"]}'");
            if (dict["tamanho"] != "GG") throw new Exception($"'tamanho' não foi trimado corretamente: '{dict["tamanho"]}'");
        });

        RunTest("Entrada nula ou vazia retorna dicionário vazio", () =>
        {
            var dict1 = ProductService.ParseAttributes(null);
            var dict2 = ProductService.ParseAttributes("");
            var dict3 = ProductService.ParseAttributes("   ");
            if (dict1.Count != 0 || dict2.Count != 0 || dict3.Count != 0)
                throw new Exception("Dicionário não veio vazio para entrada em branco");
        });

        RunTest("Ponto e vírgula extra no fim é ignorado ('cor=azul; tamanho=M;')", () =>
        {
            var dict = ProductService.ParseAttributes("cor=azul; tamanho=M;");
            if (dict.Count != 2) throw new Exception($"Esperava 2 itens, obteve {dict.Count}");
        });

        RunTest("Lançar exceção para par inválido sem sinal de '='", () =>
        {
            bool threw = false;
            try
            {
                ProductService.ParseAttributes("cor=azul; tamanho");
            }
            catch (ArgumentException ex) when (ex.Message.Contains("formato inválido"))
            {
                threw = true;
            }
            if (!threw) throw new Exception("Deveria lançar ArgumentException para par sem '='");
        });

        RunTest("Lançar exceção para chave vazia ('=azul')", () =>
        {
            bool threw = false;
            try
            {
                ProductService.ParseAttributes("=azul; tamanho=M");
            }
            catch (ArgumentException ex) when (ex.Message.Contains("formato inválido"))
            {
                threw = true;
            }
            if (!threw) throw new Exception("Deveria lançar ArgumentException para chave vazia");
        });

        RunTest("Lançar exceção para valor vazio ('cor=')", () =>
        {
            bool threw = false;
            try
            {
                ProductService.ParseAttributes("cor=; tamanho=M");
            }
            catch (ArgumentException ex) when (ex.Message.Contains("formato inválido"))
            {
                threw = true;
            }
            if (!threw) throw new Exception("Deveria lançar ArgumentException para valor vazio");
        });

        RunTest("Lançar exceção para chaves duplicadas ('cor=azul; cor=vermelho')", () =>
        {
            bool threw = false;
            try
            {
                ProductService.ParseAttributes("cor=azul; cor=vermelho");
            }
            catch (ArgumentException ex) when (ex.Message.Contains("mais de uma vez"))
            {
                threw = true;
            }
            if (!threw) throw new Exception("Deveria lançar ArgumentException para chave duplicada");
        });

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n--- 2. TESTES DE REGRAS DE NEGÓCIO (ProductService) ---");
        Console.ResetColor();

        var tempDbPath = Path.Combine(Path.GetTempPath(), $"easyshop_test_{Guid.NewGuid():N}.db");
        using (var db = new LiteDatabase(tempDbPath))
        {
            var repo = new ProductRepository(db);
            var service = new ProductService(repo);

            RunTest("Validação de nome obrigatório", () =>
            {
                bool threw = false;
                try
                {
                    service.AddAsync("", 100m, "cor=azul").GetAwaiter().GetResult();
                }
                catch (ArgumentException ex) when (ex.Message.Contains("nome do produto é obrigatório"))
                {
                    threw = true;
                }
                if (!threw) throw new Exception("Deveria barrar nome vazio");
            });

            RunTest("Validação de preço maior que zero", () =>
            {
                bool threw = false;
                try
                {
                    service.AddAsync("Produto", 0m, "cor=azul").GetAwaiter().GetResult();
                }
                catch (ArgumentException ex) when (ex.Message.Contains("maior que zero"))
                {
                    threw = true;
                }
                if (!threw) throw new Exception("Deveria barrar preço zero");
            });

            RunTest("Cadastro bem-sucedido via Service e consulta via Repository", () =>
            {
                service.AddAsync("Teclado Mecânico RGB", 299.90m, "switch=Gateron Red; layout=ABNT2; conexao=USB-C")
                    .GetAwaiter().GetResult();

                var products = service.GetAllAsync().GetAwaiter().GetResult();
                var saved = products.FirstOrDefault(p => p.Name == "Teclado Mecânico RGB");
                if (saved == null) throw new Exception("Produto não encontrado após inserção");
                if (saved.Price != 299.90m) throw new Exception("Preço divergente");
                if (saved.Attributes["switch"] != "Gateron Red") throw new Exception("Atributo 'switch' incorreto");
                if (saved.Attributes["layout"] != "ABNT2") throw new Exception("Atributo 'layout' incorreto");
            });

            RunTest("Verificação do Seed inicial automático (3 produtos com esquemas distintos)", () =>
            {
                var all = repo.GetAllAsync().GetAwaiter().GetResult();
                if (all.Count < 3) throw new Exception($"Esperava ao menos 3 produtos do seed, obteve {all.Count}");

                var camisa = all.FirstOrDefault(p => p.Name.Contains("Camisa"));
                var note = all.FirstOrDefault(p => p.Name.Contains("Notebook"));
                var liquidificador = all.FirstOrDefault(p => p.Name.Contains("Liquidificador"));

                if (camisa == null || !camisa.Attributes.ContainsKey("tamanho"))
                    throw new Exception("Camisa do seed não possui atributo dinâmico 'tamanho'");
                if (note == null || !note.Attributes.ContainsKey("processador"))
                    throw new Exception("Notebook do seed não possui atributo dinâmico 'processador'");
                if (liquidificador == null || !liquidificador.Attributes.ContainsKey("voltagem"))
                    throw new Exception("Liquidificador do seed não possui atributo dinâmico 'voltagem'");
            });
        }

        // Limpeza do arquivo de teste
        try { if (File.Exists(tempDbPath)) File.Delete(tempDbPath); } catch { }

        Console.WriteLine("\n====================================================================");
        if (failed == 0)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"  TODOS OS {passed} TESTES PASSARAM COM SUCESSO! :)");
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"  TOTAL: {passed} PASSARAM, {failed} FALHARAM.");
        }
        Console.ResetColor();
        Console.WriteLine("====================================================================\n");
    }
}
