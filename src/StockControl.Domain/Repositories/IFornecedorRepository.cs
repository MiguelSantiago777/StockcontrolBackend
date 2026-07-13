using StockControl.Domain.Aggregates;
using StockControl.Domain.ValueObjects;

namespace StockControl.Domain.Repositories;

public interface IFornecedorRepository : IRepository<Fornecedor>
{
    Task<bool> CnpjExisteAsync(Cnpj cnpj, Guid? ignorarId = null, CancellationToken cancellationToken = default);
}
