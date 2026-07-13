using StockControl.Domain.Entities;

namespace StockControl.Domain.Specifications;

public sealed class CategoriasAtivasSpecification : Specification<Categoria>
{
    public CategoriasAtivasSpecification(int page, int pageSize, string? busca = null)
    {
        SetCriteria(c => c.IsActive &&
            (busca == null || c.Nome.ToLower().Contains(busca.ToLower())));
        SetOrderBy(c => c.Nome);
        ApplyPaging(page, pageSize);
    }
}
