# EasyShop - Mini E-Commerce com Persistência Poliglota (.NET MAUI)

Projeto prático desenvolvido para a faculdade demonstrando **Persistência Poliglota** e **Arquitetura em Camadas** no **.NET MAUI (.NET 8)**.

---

## 👥 Divisão de Papéis no Grupo

| Integrante | Papel | Camada / Módulo | Banco de Dados / Persistência |
| :--- | :--- | :--- | :--- |
| **Kauan** | NoSQL & Catálogo de Produtos | Negócio (`Services`) e Dados (`Data`) | **LiteDB** (NoSQL de Documentos) |
| **Hygor** | Relacional & Gestão de Clientes | Negócio (`Services`) e Dados (`Data`) | **SQLite** (Relacional) |
| **Elenara** | Interface com o Usuário (UI) | Apresentação (`Views`, `ViewModels`, XAML) | *Consome apenas Services (sem acesso direto ao banco)* |

---

## 🏛️ Arquitetura em Camadas (Layered Architecture)

A aplicação segue rigorosamente o princípio de separação de responsabilidades:
1. **Camada de Apresentação (`Views` / `ViewModels`):** Interface MAUI. Interage **exclusivamente** através das interfaces da camada de negócio (`IProductService`), garantindo desacoplamento total da persistência.
2. **Camada de Negócio (`Services`):** Implementada em `ProductService.cs`. Contém as regras de negócio (validação de nome obrigatório, preço > 0) e higienização/parsing de atributos dinâmicos.
3. **Camada de Acesso a Dados (`Data` / `Repositories`):** Implementada em `ProductRepository.cs`. Encapsula o **LiteDB**, executando operações síncronas envolvidas em `Task.Run` para garantir chamadas assíncronas sem travar a thread de UI.

---

## 🎯 Por que NoSQL (LiteDB) para Produtos? (Apresentação ao Professor)

Em um e-commerce real, os produtos possuem **atributos altamente heterogêneos**:
- **Camisa:** tamanho (`G`), cor (`Azul`), material (`100% Algodão`), gênero (`Masculino`);
- **Notebook:** processador (`i7`), memória RAM (`16GB`), SSD (`512GB`), placa de vídeo (`RTX 4050`);
- **Liquidificador:** voltagem (`110V`), potência (`1200W`), capacidade (`3L`), velocidades (`12`);

### ❌ Limitações do Modelo Relacional:
Se usássemos um banco relacional tradicional como SQLite para produtos:
- Precisaríamos criar dezenas de colunas com valores nulos para a maioria dos itens;
- Ou recorrer ao anti-pattern **EAV (Entity-Attribute-Value)**, exigindo tabelas adicionais e múltiplos `JOINs` complexos que degradam a performance;
- Cada nova categoria exigiria uma migração estrutural (`ALTER TABLE`) no banco.

### ✅ Vantagens do NoSQL de Documentos (LiteDB):
- **Schema-free (Esquema Flexível):** Cada produto é armazenado como um documento BSON independente.
- **Dicionário de Atributos Dinâmicos:** A propriedade `Dictionary<string, string> Attributes` é serializada de forma nativa como um subdocumento BSON, permitindo atributos livres para cada categoria sem desperdício de espaço e sem schema rígido.
- **Embedded & Leve:** O LiteDB roda embarcado no dispositivo mobile/desktop em um arquivo local (`products.db`), sem necessidade de servidor externo.

---

## 📦 Pacotes NuGet Necessários

No projeto principal MAUI:
```bash
dotnet add package LiteDB --version 5.0.17
```

---

## ⚙️ Configuração da Injeção de Dependência (`MauiProgram.cs`)

O colega **Hygor** deve incluir as seguintes linhas no `MauiProgram.cs` para registrar o ecossistema NoSQL:

```csharp
using LiteDB;
using EasyShop.Models;
using EasyShop.Data;
using EasyShop.Services;

// ... dentro do método CreateMauiApp():

// 1. Instância ÚNICA de LiteDatabase (Singleton) apontando para products.db
// O Singleton é crucial para evitar erros de bloqueio de arquivo no LiteDB
builder.Services.AddSingleton<LiteDatabase>(sp =>
    new LiteDatabase(Path.Combine(FileSystem.AppDataDirectory, "products.db")));

// 2. Registro da Camada de Acesso a Dados (Data/Repository)
builder.Services.AddSingleton<IProductRepository, ProductRepository>();

// 3. Registro da Camada de Negócio (Services)
builder.Services.AddSingleton<IProductService, ProductService>();
```

---

## 🌱 Carga Inicial Automática (Seed)

Ao inicializar ou consultar os produtos pela primeira vez (`GetAllAsync`), se a coleção `"products"` estiver vazia, o repositório insere automaticamente 3 produtos com esquemas totalmente distintos para a demonstração ao professor:
1. **Camisa Polo Manga Curta** (Vestuário - tamanho, cor, material, gênero)
2. **Notebook Gamer Dell G15** (Informática - processador, RAM, SSD, placa de vídeo, tela)
3. **Liquidificador Turbo Inox** (Eletroportáteis - voltagem, potência, capacidade, velocidades)

---

## 🧪 Testes Unitários e Console Test Runner

Os testes cobrem:
- Parser de atributos dinâmicos (`cor=azul; tamanho=M`);
- Remoção de espaços em branco antes/depois de chaves e valores;
- Tratamento de múltiplos pontos-e-vírgulas no final;
- Exceções claras em português para entradas inválidas (pares sem `=`, chaves vazias, valores vazios);
- Exceção para chaves duplicadas/repetidas;
- Validação de nome obrigatório e preço maior que zero;
- Inserção e consulta no repositório NoSQL LiteDB.

### Como Executar os Testes:
```bash
# Execução direta pelo Console Runner:
dotnet run --project tests/EasyShop.Tests

# Ou via xUnit test runner:
dotnet test tests/EasyShop.Tests
```

---

## 🌿 Comandos Git para a Branch de Features

```bash
# 1. Garantir que está na main atualizada
git checkout main
git pull origin main

# 2. Criar e alternar para a branch da feature
git checkout -b feature/nosql-products

# 3. Adicionar os arquivos alterados
git add .

# 4. Criar o commit semântico
git commit -m "feat(nosql): implementa ProductRepository com LiteDB, ProductService e carga inicial de produtos"

# 5. Enviar para o repositório remoto
git push -u origin feature/nosql-products
```
