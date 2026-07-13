using Microsoft.EntityFrameworkCore;
using StockControl.Domain.Aggregates;
using StockControl.Domain.Repositories;
using StockControl.Domain.ValueObjects;
using StockControl.Infrastructure.Persistence.Context;

namespace StockControl.Infrastructure.Persistence.Repositories;

public sealed class ProdutoRepository : Repository<Produto>, IProdutoRepository
{
    public ProdutoRepository(AppDbContext context)
        : base(context)
    {
    }

    public async Task<Produto?> ObterPorCodigoAsync(
        CodigoProduto codigo,
        CancellationToken cancellationToken = default)
    {
        return await DbSet.FirstOrDefaultAsync(produto => produto.Codigo == codigo, cancellationToken);
    }

    public async Task<bool> CodigoExisteAsync(
        CodigoProduto codigo,
        CancellationToken cancellationToken = default)
    {
        return await DbSet.AnyAsync(produto => produto.Codigo == codigo, cancellationToken);
    }

    public async Task<IReadOnlyList<Produto>> ObterAbaixoDoEstoqueMinimoAsync(
        CancellationToken cancellationToken = default)
    {
        var produtos = await DbSet.AsNoTracking().ToListAsync(cancellationToken);
        return produtos.Where(produto => produto.EstoqueAbaixoDoMinimo()).ToList();
    }
}
