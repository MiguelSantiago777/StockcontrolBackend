using Microsoft.EntityFrameworkCore;
using StockControl.Domain.Aggregates;
using StockControl.Domain.Repositories;
using StockControl.Domain.ValueObjects;
using StockControl.Infrastructure.Persistence.Context;

namespace StockControl.Infrastructure.Persistence.Repositories;

public sealed class FornecedorRepository : Repository<Fornecedor>, IFornecedorRepository
{
    public FornecedorRepository(AppDbContext context)
        : base(context)
    {
    }

    public async Task<bool> CnpjExisteAsync(
        Cnpj cnpj,
        Guid? ignorarId = null,
        CancellationToken cancellationToken = default)
    {
        return await DbSet.AnyAsync(
            fornecedor => fornecedor.Cnpj == cnpj && fornecedor.Id != ignorarId,
            cancellationToken);
    }
}
