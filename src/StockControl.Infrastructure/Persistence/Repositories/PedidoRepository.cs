using Microsoft.EntityFrameworkCore;
using StockControl.Domain.Aggregates;
using StockControl.Domain.Enums;
using StockControl.Domain.Repositories;
using StockControl.Infrastructure.Persistence.Context;

namespace StockControl.Infrastructure.Persistence.Repositories;

public sealed class PedidoRepository : Repository<Pedido>, IPedidoRepository
{
    public PedidoRepository(AppDbContext context)
        : base(context)
    {
    }

    public async Task<Pedido?> ObterComItensAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(pedido => pedido.Itens)
            .FirstOrDefaultAsync(pedido => pedido.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Pedido>> ObterPorClienteAsync(
        Guid clienteId,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .AsNoTracking()
            .Include(pedido => pedido.Itens)
            .Where(pedido => pedido.ClienteId == clienteId)
            .OrderByDescending(pedido => pedido.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Pedido>> ObterTodosComItensAsync(CancellationToken cancellationToken = default)
    {
        return await DbSet
            .AsNoTracking()
            .Include(pedido => pedido.Itens)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExisteEmEntregaParaEntregadorAsync(
        Guid entregadorId, Guid pedidoIgnoradoId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .AsNoTracking()
            .AnyAsync(pedido =>
                pedido.Id != pedidoIgnoradoId &&
                pedido.EntregadorId == entregadorId &&
                pedido.Status == StatusPedido.EmEntrega,
                cancellationToken);
    }
}
