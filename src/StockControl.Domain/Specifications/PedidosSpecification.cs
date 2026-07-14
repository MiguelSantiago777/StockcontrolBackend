using StockControl.Domain.Aggregates;
using StockControl.Domain.Enums;

namespace StockControl.Domain.Specifications;

public sealed class PedidosSpecification : Specification<Pedido>
{
    public PedidosSpecification(
        int page,
        int pageSize,
        StatusPedido? status = null,
        bool aplicarEscopoEntregador = false,
        Guid? entregadorEscopoId = null)
    {
        SetCriteria(p =>
            (status == null || p.Status == status) &&
            (!aplicarEscopoEntregador ||
                p.EntregadorId == entregadorEscopoId ||
                (p.EntregadorId == null && p.Status != StatusPedido.Finalizado && p.Status != StatusPedido.Cancelado)));
        AddInclude(p => p.Itens);
        SetOrderByDescending(p => p.CreatedAt);
        ApplyPaging(page, pageSize);
    }
}
