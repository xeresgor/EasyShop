# EasyShop — Persistência Poliglota em .NET MAUI

## 👥 Integrantes

| Nome completo | Responsabilidade |
|---------------|------------------|
| Hygor _[sobrenomes]_ | Banco SQL (SQLite) — repositório e serviço de Clientes |
| Kauan _[sobrenomes]_ | Banco NoSQL (LiteDB) — repositório e serviço de Produtos |
| Elenara _[sobrenomes]_ | Camada de Apresentação (Views/ViewModels) e README |

## 📖 Descrição do projeto

O **EasyShop** é um protótipo de mini e-commerce em **.NET MAUI (.NET 8)** para Android. O app tem duas abas:

- **Produtos** — cadastro e listagem do catálogo, com atributos dinâmicos (ex.: `cor=azul; tamanho=M`).
- **Clientes** — cadastro e listagem de clientes (nome completo e e-mail).

O app antigo da EasyShop guardava tudo em um único banco relacional, o que obrigava a alterar o schema a cada nova categoria de produto. Este protótipo resolve isso separando os dados em dois bancos locais, cada um adequado ao tipo de dado.

## 🧠 Persistência Poliglota

Persistência poliglota é usar **mais de um paradigma de banco de dados na mesma aplicação**, escolhendo o melhor para cada tipo de dado.

| Dado | Natureza | Banco | Tecnologia | Por quê |
|------|----------|-------|------------|---------|
| **Clientes** | Estruturado, com regras estritas | **SQL (relacional)** | SQLite (`sqlite-net-pcl`) | Tabela de schema fixo, chave primária, e-mail único e campos obrigatórios garantidos pelo banco |
| **Produtos** | Semi-estruturado: cada produto tem atributos diferentes | **NoSQL (documentos)** | LiteDB | Cada produto é um documento com um dicionário livre de atributos; novas categorias não exigem alterar o schema |

## 🏗️ Arquitetura em camadas

```
┌─────────────────────────────────────────────────────────┐
│  APRESENTAÇÃO      Views (XAML) + ViewModels            │
│                    (só conhecem as interfaces de serviço)│
├─────────────────────────────────────────────────────────┤
│  NEGÓCIO           ICustomerService / IProductService   │
│                    (validações e regras)                │
├─────────────────────────────────────────────────────────┤
│  ACESSO A DADOS    Repositories                         │
│                    ├─ Clientes → SQLite  (SQL)          │
│                    └─ Produtos → LiteDB  (NoSQL)        │
└─────────────────────────────────────────────────────────┘
          dependências sempre apontam para baixo
```

**Regra do grupo:** as Views e ViewModels **nunca** referenciam banco de dados (nenhum `using SQLite;` ou `using LiteDB;`). Elas recebem apenas `ICustomerService` / `IProductService` por injeção de dependência.

### Estrutura de pastas

```
EasyShop/
├── Models/              Customer, Product, ICustomerService, IProductService (contrato do grupo)
├── Data/                Repositories (SQLite e LiteDB)            ← Hygor e Kauan
├── Services/            Serviços reais                            ← Hygor e Kauan
│   └── Mocks/           MockCustomerService, MockProductService (em memória)
├── ViewModels/          ProductsViewModel, CustomersViewModel, ProductItem
├── Views/               ProductsPage, CustomersPage
├── Platforms/Android/
├── AppShell.xaml        TabBar com as abas Produtos e Clientes
└── MauiProgram.cs       Injeção de dependência
```

## ▶️ Como rodar

**Pré-requisitos**
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- Workload do MAUI para Android: `dotnet workload install maui-android`
- Android SDK e um emulador Android (Visual Studio 2022 ou Android Studio)

**Passos**
```bash
git clone <URL_DO_REPOSITORIO>
cd EasyShop
dotnet restore
dotnet build -t:Run -f net8.0-android
```
Com o emulador já iniciado. No Visual Studio: selecione o emulador e pressione **F5**.

### Mock × serviço real

Em `MauiProgram.cs` há duas versões das linhas de registro dos serviços. Para usar os bancos reais, comente as linhas dos `Mock...` e descomente as dos serviços reais.

## 🖼️ Prints

| Aba Produtos | Aba Clientes |
|:---:|:---:|
| ![Produtos](docs/img/produtos.png) | ![Clientes](docs/img/clientes.png) |

| Validação de erro | Lista atualizada após salvar |
|:---:|:---:|
| ![Erro](docs/img/erro.png) | ![Lista](docs/img/lista.png) |

## 🎥 Vídeo de demonstração

[Link do vídeo (YouTube ou Google Drive)](COLE_O_LINK_AQUI)
