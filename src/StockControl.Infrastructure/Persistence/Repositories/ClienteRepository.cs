using StockControl.Domain.Aggregates;
using StockControl.Domain.Repositories;
using StockControl.Infrastructure.Persistence.Context;

namespace StockControl.Infrastructure.Persistence.Repositories;

public sealed class ClienteRepository : Repository<Cliente>, IClienteRepository
{
    public ClienteRepository(AppDbContext context)
        : base(context)
    {
    }
}
