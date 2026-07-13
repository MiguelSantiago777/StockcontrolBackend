using StockControl.Domain.Aggregates;
using StockControl.Domain.ValueObjects;

namespace StockControl.Domain.Repositories;

public interface IClienteRepository : IRepository<Cliente>
{
    Task<bool> CpfExisteAsync(Cpf cpf, Guid? ignorarId = null, CancellationToken cancellationToken = default);
}
