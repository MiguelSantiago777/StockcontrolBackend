using StockControl.Domain.Aggregates;

namespace StockControl.Domain.Specifications;

/// <summary>Exemplo de Specification: produtos ativos paginados por nome.</summary>
public sealed class ProdutosAtivosSpecification : Specification<Produto>
{
    public ProdutosAtivosSpecification(int page, int pageSize, string? busca = null)
    {
        SetCriteria(p => p.IsActive &&
            (busca == null || p.Nome.ToLower().Contains(busca.ToLower())));
        SetOrderBy(p => p.Nome);
        ApplyPaging(page, pageSize);
    }
}
