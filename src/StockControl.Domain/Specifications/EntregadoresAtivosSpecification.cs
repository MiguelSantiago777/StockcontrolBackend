using StockControl.Domain.Aggregates;

namespace StockControl.Domain.Specifications;

public sealed class EntregadoresAtivosSpecification : Specification<Entregador>
{
    public EntregadoresAtivosSpecification(int page, int pageSize, string? busca = null)
    {
        SetCriteria(e => e.IsActive &&
            (busca == null ||
             e.Nome.ToLower().Contains(busca.ToLower()) ||
             e.Cpf.Value.Contains(busca)));
        SetOrderBy(e => e.Nome);
        ApplyPaging(page, pageSize);
    }
}
