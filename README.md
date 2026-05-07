# Ambev Sales API

API REST para gestão de vendas, desenvolvida com Clean Architecture, CQRS e Domain-Driven Design (DDD).

---

## Sumário

- [Visão Geral](#visão-geral)
- [Arquitetura](#arquitetura)
- [Tech Stack](#tech-stack)
- [Estrutura do Projeto](#estrutura-do-projeto)
- [Regras de Negócio](#regras-de-negócio)
- [Endpoints](#endpoints)
- [Como Executar](#como-executar)
- [Testes](#testes)

---

## Visão Geral

Sistema de gerenciamento de vendas que permite criar, consultar, atualizar e cancelar vendas e seus itens. Cada venda possui um ou mais itens com aplicação automática de descontos baseada em quantidade.

---

## Arquitetura

O projeto segue **Clean Architecture** com separação em quatro camadas:

```
Domain  ←  Application  ←  Infrastructure
                ↑
             WebApi
```

- **Domain**: entidades, regras de negócio, interfaces de repositório e eventos de domínio.
- **Application**: casos de uso via CQRS (MediatR), DTOs, validações e mapeamentos.
- **Infrastructure**: implementação dos repositórios (EF Core + PostgreSQL) e migrações.
- **WebApi**: controllers REST, middleware de exceção e configuração do host.

### Padrões Utilizados

| Padrão | Implementação |
|---|---|
| CQRS | MediatR — Commands e Queries separados |
| Domain Events | Eventos publicados nas entidades, consumidos por handlers |
| Repository Pattern | `ISaleRepository` → `SaleRepository` |
| Mediator | Desacoplamento entre controller e handlers |

---

## Tech Stack

| Categoria | Tecnologia |
|---|---|
| Runtime | .NET 8 / C# |
| Banco de Dados | PostgreSQL 16 |
| ORM | Entity Framework Core 8 + Npgsql |
| CQRS / Mediator | MediatR 12 |
| Mapeamento | AutoMapper 15 |
| Validação | FluentValidation 11 |
| Documentação | Swagger (Swashbuckle) |
| Ordenação Dinâmica | System.Linq.Dynamic.Core |
| Testes | xUnit · NSubstitute · FluentAssertions · Bogus |

---

## Estrutura do Projeto

```
Ambev_Sales/
├── src/
│   ├── Ambev.DeveloperEvaluation.Domain/
│   │   ├── Common/              # BaseEntity com suporte a domain events
│   │   ├── Entities/            # Sale, SaleItem
│   │   ├── Enums/               # SaleStatus (Active, Cancelled)
│   │   ├── Events/              # SaleCreated, SaleModified, SaleCancelled, ItemCancelled
│   │   ├── Exceptions/          # DomainException
│   │   └── Repositories/        # ISaleRepository
│   │
│   ├── Ambev.DeveloperEvaluation.Application/
│   │   └── Sales/
│   │       ├── Commands/        # CreateSale, UpdateSale, CancelSale, CancelSaleItem
│   │       ├── Queries/         # GetSales (paginado), GetSaleById
│   │       ├── DTOs/            # Request / Response objects
│   │       ├── EventHandlers/   # Handlers dos domain events
│   │       └── Mapping/         # SaleProfile (AutoMapper)
│   │
│   ├── Ambev.DeveloperEvaluation.Infrastructure/
│   │   ├── Data/                # ApplicationDbContext + migrações EF Core
│   │   └── Repositories/        # SaleRepository
│   │
│   └── Ambev.DeveloperEvaluation.WebApi/
│       ├── Controllers/         # SalesController
│       ├── Middleware/          # ExceptionMiddleware
│       └── Program.cs
│
└── tests/
    └── Ambev.DeveloperEvaluation.UnitTests/
        └── Sales/
            ├── Domain/          # Testes de entidades e regras
            ├── Handlers/        # Testes de command/query handlers
            └── TestData/        # Builders com Bogus
```

---

## Regras de Negócio

### Desconto por Quantidade

| Quantidade de itens idênticos | Desconto |
|---|---|
| Menos de 4 | 0% |
| 4 a 9 | 10% |
| 10 a 20 | 20% |
| Mais de 20 | **não permitido** |

### Outras Regras

- Não é possível adicionar itens a uma venda cancelada.
- O `TotalAmount` do item é calculado automaticamente: `Quantidade × PreçoUnitário × (1 − Desconto)`.
- O `TotalAmount` da venda é a soma dos itens ativos.
- Cancelar um item não cancela a venda; cancelar a venda cancela todos os itens.

---

## Endpoints

Base URL: `/sales`

| Método | Rota | Descrição |
|---|---|---|
| `GET` | `/sales` | Lista paginada de vendas |
| `POST` | `/sales` | Cria uma nova venda |
| `GET` | `/sales/{id}` | Busca venda por ID |
| `PUT` | `/sales/{id}` | Atualiza uma venda |
| `DELETE` | `/sales/{id}` | Cancela uma venda |
| `DELETE` | `/sales/{id}/items/{itemId}` | Cancela um item da venda |

### Parâmetros de Listagem (`GET /sales`)

| Parâmetro | Tipo | Descrição |
|---|---|---|
| `_page` | int | Número da página (padrão: 1) |
| `_size` | int | Itens por página (padrão: 10) |
| `_order` | string | Campo e direção, ex: `saleDate desc` |
| `customerId` | guid | Filtra pelo cliente |
| `branchId` | guid | Filtra pela filial |
| `status` | string | `Active` ou `Cancelled` |

### Exemplo de Payload — Criar Venda

```json
{
  "saleNumber": "SALE-001",
  "saleDate": "2025-01-15T10:00:00",
  "customerId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "customerName": "Distribuidora XYZ",
  "branchId": "7fa85f64-5717-4562-b3fc-2c963f66afa6",
  "branchName": "Filial SP",
  "items": [
    {
      "productId": "1fa85f64-5717-4562-b3fc-2c963f66afa6",
      "productName": "Cerveja Brahma Lata 350ml",
      "quantity": 10,
      "unitPrice": 3.50
    }
  ]
}
```

A API retorna `SaleDto` com `discountPercentage` e `totalAmount` calculados automaticamente.

---

## Como Executar

### Pré-requisitos

- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- [Docker](https://www.docker.com/)

### 1. Subir o banco de dados

```bash
docker-compose up -d
```

Sobe um container PostgreSQL 16 na porta `5432`:

| Configuração | Valor |
|---|---|
| Database | `ambev_sales` |
| Usuário | `postgres` |
| Senha | `postgres` |

### 2. Executar a API

```bash
dotnet run --project src/Ambev.DeveloperEvaluation.WebApi
```

As migrações do EF Core são aplicadas automaticamente na inicialização.

### 3. Acessar a documentação Swagger

```
https://localhost:{porta}/swagger
```

---

## Testes

```bash
dotnet test tests/Ambev.DeveloperEvaluation.UnitTests/
```

### Cobertura

| Arquivo de Teste | O que cobre |
|---|---|
| `SaleTests.cs` | Regras de desconto, cancelamento e validação de quantidade |
| `SaleItemTests.cs` | Cálculo de totais e percentual de desconto |
| `CreateSaleCommandHandlerTests.cs` | Criação de venda com aplicação de desconto |
| `CancelSaleCommandHandlerTests.cs` | Cancelamento de venda |
| `CancelSaleItemCommandHandlerTests.cs` | Cancelamento de item individual |
| `GetSalesQueryHandlerTests.cs` | Listagem paginada e resposta vazia |
