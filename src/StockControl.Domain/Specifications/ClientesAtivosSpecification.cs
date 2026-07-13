using StockControl.Domain.Aggregates;

namespace StockControl.Domain.Specifications;

public sealed class ClientesAtivosSpecification : Specification<Cliente>
{
    public ClientesAtivosSpecification(int page, int pageSize, string? busca = null)
    {
        SetCriteria(c => c.IsActive &&
            (busca == null ||
             c.Nome.ToLower().Contains(busca.ToLower()) ||
             c.Cpf.Value.Contains(busca) ||
             c.Email.Value.ToLower().Contains(busca.ToLower())));
        SetOrderBy(c => c.Nome);
        ApplyPaging(page, pageSize);
    }
}
