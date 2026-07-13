using Microsoft.EntityFrameworkCore;
using StockControl.Domain.Aggregates;
using StockControl.Domain.Repositories;
using StockControl.Domain.ValueObjects;
using StockControl.Infrastructure.Persistence.Context;

namespace StockControl.Infrastructure.Persistence.Repositories;

public sealed class ClienteRepository : Repository<Cliente>, IClienteRepository
{
    public ClienteRepository(AppDbContext context)
        : base(context)
    {
    }

    public async Task<bool> CpfExisteAsync(
        Cpf cpf,
        Guid? ignorarId = null,
        CancellationToken cancellationToken = default)
    {
        return await DbSet.AnyAsync(
            cliente => cliente.Cpf == cpf && cliente.Id != ignorarId,
            cancellationToken);
    }
}
