using StockControl.Domain.Aggregates;
using StockControl.Domain.Enums;

namespace StockControl.Domain.Specifications;

public sealed class MovimentacoesFiltradasSpecification : Specification<Movimentacao>
{
    public MovimentacoesFiltradasSpecification(
        int page,
        int pageSize,
        TipoMovimentacao? tipo = null,
        Guid? produtoId = null,
        DateTime? de = null,
        DateTime? ate = null)
    {
        SetCriteria(m =>
            (tipo == null || m.Tipo == tipo) &&
            (produtoId == null || m.ProdutoId == produtoId) &&
            (de == null || m.CreatedAt >= de) &&
            (ate == null || m.CreatedAt <= ate));
        SetOrderByDescending(m => m.CreatedAt);
        ApplyPaging(page, pageSize);
    }
}
