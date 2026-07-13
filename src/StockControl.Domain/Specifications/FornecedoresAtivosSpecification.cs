using StockControl.Domain.Aggregates;

namespace StockControl.Domain.Specifications;

public sealed class FornecedoresAtivosSpecification : Specification<Fornecedor>
{
    public FornecedoresAtivosSpecification(int page, int pageSize, string? busca = null)
    {
        SetCriteria(f => f.IsActive &&
            (busca == null ||
             f.RazaoSocial.ToLower().Contains(busca.ToLower()) ||
             (f.NomeFantasia != null && f.NomeFantasia.ToLower().Contains(busca.ToLower())) ||
             f.Cnpj.Value.Contains(busca)));
        SetOrderBy(f => f.RazaoSocial);
        ApplyPaging(page, pageSize);
    }
}
