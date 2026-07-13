using StockControl.Domain.Aggregates;

namespace StockControl.Domain.Repositories;

public interface IPedidoRepository : IRepository<Pedido>
{
    Task<Pedido?> ObterComItensAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Pedido>> ObterPorClienteAsync(Guid clienteId, CancellationToken cancellationToken = default);
}
