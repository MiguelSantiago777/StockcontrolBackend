using StockControl.Domain.Aggregates;
using StockControl.Domain.Repositories;
using StockControl.Infrastructure.Persistence.Context;

namespace StockControl.Infrastructure.Persistence.Repositories;

public sealed class FornecedorRepository : Repository<Fornecedor>, IFornecedorRepository
{
    public FornecedorRepository(AppDbContext context)
        : base(context)
    {
    }
}
