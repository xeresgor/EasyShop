# EasyShop — Persistência Poliglota em .NET MAUI

## 👥 Integrantes

- Hygor Ferreira Barbosa
- Kauan Teodoro Lucio
- Elenara Caroline Xavier

## 📖 Descrição do projeto

O **EasyShop** é um protótipo de mini e-commerce em **.NET MAUI (.NET 10)** para Android. O app tem duas abas:

- **Produtos** — cadastro e listagem do catálogo, com atributos dinâmicos (ex.: `cor=azul; tamanho=M`).
- **Clientes** — cadastro e listagem de clientes (nome completo e e-mail).

O app antigo da EasyShop guardava tudo em um único banco relacional, o que obrigava a alterar o schema a cada nova categoria de produto. Este protótipo resolve isso separando os dados em dois bancos locais, cada um adequado ao tipo de dado.

## 🧠 Persistência Poliglota

Persistência poliglota é usar **mais de um paradigma de banco de dados na mesma aplicação**, escolhendo o melhor para cada tipo de dado.

| Dado | Natureza | Banco | Tecnologia | Por quê |
|------|----------|-------|------------|---------|
| **Clientes** | Estruturado, com regras estritas | **SQL (relacional)** | SQLite (`sqlite-net-pcl`) | Tabela de schema fixo, chave primária autoincremento, e-mail `UNIQUE` e campos `NOT NULL` garantidos pelo próprio banco |
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
├── Models/              Customer (tabela SQLite), Product (documento LiteDB)
├── Data/                CustomerRepository (SQLite), ProductRepository (LiteDB)
├── Services/            CustomerService, ProductService (regras de negócio)
│   └── Interfaces/      ICustomerService, IProductService (contrato entre as camadas)
├── ViewModels/          ProductsViewModel, CustomersViewModel, ProductItem
├── Views/               ProductsPage, CustomersPage
├── Platforms/Android/
├── AppShell.xaml        TabBar com as abas Produtos e Clientes
└── MauiProgram.cs       Injeção de dependência
```

## ▶️ Como rodar

### Opção rápida — instalar o APK pronto (sem compilar)

1. Baixe o **`EasyShop-v1.0.apk`** na página de
   [Releases](https://github.com/xeresgor/EasyShop/releases/latest) do repositório.
2. Abra um emulador Android (Android 7.0 ou superior) e **arraste o arquivo `.apk` para dentro da janela
   do emulador** — ele instala sozinho. Em um celular Android, abra o arquivo e permita a instalação de
   fontes desconhecidas.
3. Abra o app **EasyShop** na lista de aplicativos.

### Opção 1 — Visual Studio 2026 (recomendado)

1. Instale o [Visual Studio 2026 Community](https://visualstudio.microsoft.com/) com a carga de trabalho
   **"Desenvolvimento de interface do usuário de aplicativo multiplataforma do .NET"** (.NET MAUI).
   Ela já instala o .NET SDK, o Android SDK e o emulador.
2. Clone o repositório (ou baixe o ZIP pelo botão **Code → Download ZIP** e descompacte):
   ```bash
   git clone https://github.com/xeresgor/EasyShop.git
   ```
3. Abra o arquivo **`EasyShop.sln`** com duplo clique.
4. Na primeira vez, crie um emulador em **Ferramentas → Android → Gerenciador de Dispositivos Android**
   (qualquer Pixel com Android 14 ou superior).
5. Escolha o emulador na barra de cima e pressione **F5**.

> A primeira compilação demora alguns minutos (o Visual Studio baixa os pacotes NuGet). As seguintes são rápidas.

### Opção 2 — linha de comando

Pré-requisitos: .NET SDK 10 ou superior, workload do MAUI (`dotnet workload install maui-android`) e um
emulador Android já aberto.

```bash
git clone https://github.com/xeresgor/EasyShop.git
cd EasyShop
dotnet build -t:Run -f net10.0-android
```

### Problemas comuns

| Sintoma | Solução |
|---|---|
| O emulador não inicia ou fica lento | Ligar a virtualização na BIOS e o recurso do Windows **"Plataforma do Hipervisor do Windows"** (pede reiniciar o PC) |
| Erro `XA0129` ao implantar | Rodar de novo com `-p:EmbedAssembliesIntoApk=true` ou desinstalar o app do emulador |
| Visual Studio 2022 não abre o projeto | O projeto é .NET 10: use o Visual Studio 2026 |

### Onde ficam os dados no aparelho

Os dois bancos são arquivos locais em `FileSystem.AppDataDirectory`:

| Arquivo | Banco | Conteúdo |
|---|---|---|
| `easyshop.db3` | SQLite | Tabela `Customers` |
| `products.db` | LiteDB | Coleção `products` (documentos BSON) |

Para provar a persistência: cadastre clientes e produtos, feche o app por completo e abra de novo — as listas continuam lá.

## 🖼️ Prints (emulador Android)

| Catálogo no NoSQL | Clientes no SQL |
|:---:|:---:|
| <img src="docs/img/produtos.png" width="260" alt="Aba Produtos com três produtos de atributos diferentes"> | <img src="docs/img/clientes.png" width="260" alt="Aba Clientes barrando e-mail duplicado"> |
| Camiseta, furadeira e notebook com atributos diferentes na mesma coleção | E-mail duplicado barrado pela regra de negócio e pelo índice `UNIQUE` |

| Validação de negócio | Persistência |
|:---:|:---:|
| <img src="docs/img/erro.png" width="260" alt="Erro de preço igual a zero"> | <img src="docs/img/persistencia.png" width="260" alt="Produtos continuam após fechar e reabrir o app"> |
| Preço precisa ser maior que zero | Dados continuam lá depois de fechar o app por completo e abrir de novo |

## 🎥 Vídeo de demonstração

https://youtu.be/fFVeMDFTB14
