using StockControl.Domain.Aggregates;
using StockControl.Domain.Enums;

namespace StockControl.Domain.Specifications;

public sealed class PedidosSpecification : Specification<Pedido>
{
    public PedidosSpecification(int page, int pageSize, StatusPedido? status = null)
    {
        SetCriteria(p => status == null || p.Status == status);
        AddInclude(p => p.Itens);
        SetOrderByDescending(p => p.CreatedAt);
        ApplyPaging(page, pageSize);
    }
}
