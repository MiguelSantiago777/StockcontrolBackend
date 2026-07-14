using Microsoft.EntityFrameworkCore;
using StockControl.Domain.Aggregates;
using StockControl.Domain.Repositories;
using StockControl.Domain.ValueObjects;
using StockControl.Infrastructure.Persistence.Context;

namespace StockControl.Infrastructure.Persistence.Repositories;

public sealed class VeiculoRepository : Repository<Veiculo>, IVeiculoRepository
{
    public VeiculoRepository(AppDbContext context)
        : base(context)
    {
    }

    public async Task<bool> PlacaExisteAsync(Placa placa, Guid? ignorarId = null, CancellationToken cancellationToken = default)
    {
        return await DbSet.AnyAsync(veiculo => veiculo.Placa == placa && veiculo.Id != ignorarId, cancellationToken);
    }
}
