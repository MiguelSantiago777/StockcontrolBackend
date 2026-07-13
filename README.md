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

- **Domain**: zero dependências externas. Aggregate Roots (`Produto`, `Categoria`, `Pedido`, `Usuario`, `Cliente`, `Fornecedor`, `Entregador`, `Movimentacao`), Value Objects, Domain Events, Specifications e Result Pattern.
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

## Endpoints (`/api/v1`)

Todos os controllers seguem o mesmo formato: `GET` (paginado, `page`/`pageSize`/`search`), `GET /{id}`, `POST`, `PUT /{id}`, `DELETE /{id}` (soft delete), exceto onde indicado.

| Recurso | Rota | Policy | Observação |
|---|---|---|---|
| Auth | `/auth/*` | — | login, refresh, logout, me, change-password |
| Categorias | `/categories` | Leitura / GerenciaEstoque | |
| Produtos | `/products` | Leitura / GerenciaEstoque | + `POST /products/{id}/image` |
| Fornecedores | `/suppliers` | Leitura / GerenciaEstoque | CNPJ único, endereço obrigatório |
| Clientes | `/customers` | Leitura / GerenciaEstoque | CPF único, endereço obrigatório |
| Usuários | `/users` | Administrador | senha só no `POST`; `PUT` não altera senha |
| Movimentações | `/movements` | Leitura / GerenciaEstoque | sem `PUT`/`DELETE`; `GET /movements/export` (CSV) |
| Entregadores | `/drivers` | Leitura / GerenciaEstoque | `PATCH /{id}/status`, `PATCH /{id}/position`, `GET /drivers/eligible-users` |
| Pedidos | `/orders` | Leitura / GerenciaEstoque / Entregas | sem `PUT`; `PATCH /{id}/start-delivery`, `/finish-delivery`, `/cancel` |

Todos os índices únicos (CNPJ, CPF, e-mail, código de produto) são filtrados por `DeletedAt IS NULL`, então um valor de um registro excluído pode ser reutilizado livremente.

## Autenticação

1. `POST /api/auth/login` → retorna `accessToken` (15 min) + `refreshToken` (7 dias)
2. `POST /api/auth/refresh` → renova o par de tokens
3. SignalR aceita o token via query string `?access_token=`

Perfis: **Administrador**, **Estoquista**, **Entregador**, **Visualizador** (policies em `DependencyInjection.Policies`).

## SignalR

Hub em `/hubs/stock` com grupos:
- `dashboard` — atualizações do dashboard
- `entregadores` — pensado para posição em tempo real dos entregadores
- `movimentacoes` — movimentações de estoque

Os grupos existem e o cliente já entra neles, mas nenhum handler publica eventos ainda (ver "Próximos passos"). O rastreamento de entregadores hoje é só a última posição gravada via `PATCH /drivers/{id}/position`, não um push ao vivo.

## Convenções

- Toda entidade possui `Id`, `CreatedAt`, `UpdatedAt`, `DeletedAt`, `IsActive`, `Version` (concorrência otimista via `xmin`).
- Soft delete com filtro global de query.
- Nenhum atributo de mapeamento — apenas `IEntityTypeConfiguration`.
- Erros de negócio via Result Pattern; exceções apenas para casos excepcionais.

## Próximos passos sugeridos

- [ ] Publicar nos grupos do SignalR (`dashboard`, `entregadores`, `movimentacoes`) a partir dos handlers — os grupos e o hub já existem e o front já escuta os eventos, mas hoje nada é de fato publicado além do que já havia (a maioria dos domain events levantados, ex. `PedidoCriadoEvent`, `EntregaIniciadaEvent`, não tem handler; só `ProdutoSemEstoqueEvent` tem)
- [ ] Tela de Configurações no frontend (`/settings` ainda é placeholder)
- [ ] Cobertura de testes para os módulos novos (Categoria/Fornecedor/Cliente/Usuario/Movimentacao/Entregador/Pedido) — hoje só `Produto` e os Value Objects têm teste de domínio
- [ ] Testes de integração com Testcontainers para os novos endpoints
- [ ] Dashboard queries com cache Redis mais abrangente (hoje só listagens paginadas usam cache)
