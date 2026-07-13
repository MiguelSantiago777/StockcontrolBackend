using StockControl.Domain.Aggregates;

namespace StockControl.Domain.Specifications;

public sealed class UsuariosAtivosSpecification : Specification<Usuario>
{
    public UsuariosAtivosSpecification(int page, int pageSize, string? busca = null)
    {
        SetCriteria(u => u.IsActive &&
            (busca == null ||
             u.Nome.ToLower().Contains(busca.ToLower()) ||
             u.Email.Value.ToLower().Contains(busca.ToLower())));
        SetOrderBy(u => u.Nome);
        ApplyPaging(page, pageSize);
    }
}
