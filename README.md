# StockControl — Backend

Sistema de controle de estoque com entregas em tempo real, construído em **.NET 10** seguindo **DDD + Clean Architecture**.

## Stack

| Categoria | Tecnologia |
|---|---|
| Framework | ASP.NET Core Web API (.NET 10) |
| Persistência | Entity Framework Core + PostgreSQL |
| CQRS | MediatR |
| Validação | FluentValidation |
| Mapeamento | AutoMapper |
| Autenticação | JWT + Refresh Token |
| Tempo real | SignalR |
| Cache | Redis |
| Logs | Serilog |
| Docs | Swagger |
| Testes | xUnit + Moq + FluentAssertions |
| Infra | Docker + Docker Compose |

## Arquitetura

```
┌──────────────────────────────────────────────┐
│                StockControl.API              │  Controllers, Middlewares, DI
├──────────────────────────────────────────────┤
│           StockControl.Infrastructure        │  EF Core, Redis, JWT, SignalR
├──────────────────────────────────────────────┤
│            StockControl.Application          │  Commands, Queries, Handlers
├──────────────────────────────────────────────┤
│              StockControl.Domain             │  Entidades, VOs, Eventos, Regras
└──────────────────────────────────────────────┘
        Dependências apontam sempre para dentro
```

- **Domain**: zero dependências externas. Aggregate Roots (`Produto`, `Pedido`, `Usuario`, `Cliente`, `Fornecedor`, `Entregador`, `Movimentacao`), Value Objects, Domain Events, Specifications e Result Pattern.
- **Application**: CQRS com MediatR, pipeline behaviors (validação + logging), interfaces de serviços.
- **Infrastructure**: implementações de repositórios (Repository + Unit of Work), `IEntityTypeConfiguration` para todo mapeamento, Redis, JWT, BCrypt, SignalR.
- **API**: controllers finos que só delegam para o MediatR, tratamento global de exceções, Swagger com suporte a Bearer token.

## Como rodar

### Com Docker (recomendado)

```bash
docker compose up --build
```

- API: http://localhost:8080/swagger
- PostgreSQL: localhost:5432
- Redis: localhost:6379

### Rodando direto no Swagger

```bash
docker compose up postgres redis -d      # dependências
dotnet run --project src/StockControl.API
```

Abra http://localhost:8080 — a raiz redireciona para **/swagger**, que agora fica habilitado em todos os ambientes. Pelo Visual Studio/Rider, o `launchSettings.json` já abre o navegador direto no Swagger com `ASPNETCORE_ENVIRONMENT=Development`.

### Local

```bash
# Suba as dependências
docker compose up postgres redis -d

# Crie a migration inicial e aplique
dotnet ef migrations add Initial -p src/StockControl.Infrastructure -s src/StockControl.API
dotnet ef database update -p src/StockControl.Infrastructure -s src/StockControl.API

# Rode a API
dotnet run --project src/StockControl.API
```

### Testes

```bash
dotnet test --collect:"XPlat Code Coverage"
```

## Fluxo CQRS (exemplo: criar produto)

```
POST /api/produtos
  → ProdutosController
    → MediatR (LoggingBehavior → ValidationBehavior)
      → CriarProdutoCommandHandler
        → Produto.Criar() (Factory + regras + ProdutoCriadoEvent)
        → IProdutoRepository.AddAsync()
        → IUnitOfWork.SaveChangesAsync()  → despacha Domain Events
        → Invalidação de cache Redis
  ← Result<ProdutoDto> → 201 Created (ou ProblemDetails)
```

## Autenticação

1. `POST /api/auth/login` → retorna `accessToken` (15 min) + `refreshToken` (7 dias)
2. `POST /api/auth/refresh` → renova o par de tokens
3. SignalR aceita o token via query string `?access_token=`

Perfis: **Administrador**, **Estoquista**, **Entregador**, **Visualizador** (policies em `DependencyInjection.Policies`).

## SignalR

Hub em `/hubs/stock` com grupos:
- `dashboard` — atualizações do dashboard
- `entregadores` — posição em tempo real dos entregadores
- `movimentacoes` — movimentações de estoque

## Convenções

- Toda entidade possui `Id`, `CreatedAt`, `UpdatedAt`, `DeletedAt`, `IsActive`, `Version` (concorrência otimista via `xmin`).
- Soft delete com filtro global de query.
- Nenhum atributo de mapeamento — apenas `IEntityTypeConfiguration`.
- Erros de negócio via Result Pattern; exceções apenas para casos excepcionais.

## Próximos passos sugeridos

- [ ] Módulo de autenticação completo (Login/Refresh/Logout commands)
- [ ] Handlers de Domain Events (ex.: notificar SignalR quando `ProdutoSemEstoqueEvent`)
- [ ] Migrations iniciais
- [ ] Dashboard queries com cache Redis
- [ ] Testes de integração com Testcontainers
