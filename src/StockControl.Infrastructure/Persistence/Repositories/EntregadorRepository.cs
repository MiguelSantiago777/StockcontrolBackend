using Microsoft.EntityFrameworkCore;
using StockControl.Domain.Aggregates;
using StockControl.Domain.Enums;
using StockControl.Domain.Repositories;
using StockControl.Domain.ValueObjects;
using StockControl.Infrastructure.Persistence.Context;

namespace StockControl.Infrastructure.Persistence.Repositories;

public sealed class EntregadorRepository : Repository<Entregador>, IEntregadorRepository
{
    public EntregadorRepository(AppDbContext context)
        : base(context)
    {
    }

    public async Task<IReadOnlyList<Entregador>> ObterPorStatusAsync(
        StatusEntregador status,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .AsNoTracking()
            .Where(entregador => entregador.Status == status)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> CpfExisteAsync(Cpf cpf, Guid? ignorarId = null, CancellationToken cancellationToken = default)
    {
        return await DbSet.AnyAsync(entregador => entregador.Cpf == cpf && entregador.Id != ignorarId, cancellationToken);
    }

    public async Task<bool> UsuarioJaVinculadoAsync(Guid usuarioId, Guid? ignorarId = null, CancellationToken cancellationToken = default)
    {
        return await DbSet.AnyAsync(entregador => entregador.UsuarioId == usuarioId && entregador.Id != ignorarId, cancellationToken);
    }
}
