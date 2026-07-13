using Microsoft.EntityFrameworkCore;
using StockControl.Domain.Aggregates;
using StockControl.Domain.Repositories;
using StockControl.Infrastructure.Persistence.Context;

namespace StockControl.Infrastructure.Persistence.Repositories;

public sealed class MovimentacaoRepository : Repository<Movimentacao>, IMovimentacaoRepository
{
    public MovimentacaoRepository(AppDbContext context)
        : base(context)
    {
    }

    public async Task<IReadOnlyList<Movimentacao>> ObterPorProdutoAsync(
        Guid produtoId,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .AsNoTracking()
            .Where(movimentacao => movimentacao.ProdutoId == produtoId)
            .OrderByDescending(movimentacao => movimentacao.CreatedAt)
            .ToListAsync(cancellationToken);
    }
}
