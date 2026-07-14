using StockControl.Domain.Aggregates;
using StockControl.Domain.ValueObjects;

namespace StockControl.Domain.Repositories;

public interface IVeiculoRepository : IRepository<Veiculo>
{
    Task<bool> PlacaExisteAsync(Placa placa, Guid? ignorarId = null, CancellationToken cancellationToken = default);
}
