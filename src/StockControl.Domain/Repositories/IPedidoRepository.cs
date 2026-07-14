using StockControl.Domain.Aggregates;
using StockControl.Domain.Enums;

namespace StockControl.Domain.Repositories;

public interface IPedidoRepository : IRepository<Pedido>
{
    Task<Pedido?> ObterComItensAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Pedido>> ObterPorClienteAsync(Guid clienteId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Pedido>> ObterTodosComItensAsync(CancellationToken cancellationToken = default);
    Task<bool> ExisteEmEntregaParaEntregadorAsync(
        Guid entregadorId, Guid pedidoIgnoradoId, CancellationToken cancellationToken = default);
}
