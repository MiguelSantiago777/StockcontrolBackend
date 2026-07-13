using StockControl.Domain.Aggregates;
using StockControl.Domain.Enums;

namespace StockControl.Domain.Repositories;

public interface IEntregadorRepository : IRepository<Entregador>
{
    Task<IReadOnlyList<Entregador>> ObterPorStatusAsync(StatusEntregador status, CancellationToken cancellationToken = default);
}
