using StockControl.Domain.Aggregates;

namespace StockControl.Domain.Specifications;

public sealed class VeiculosAtivosSpecification : Specification<Veiculo>
{
    public VeiculosAtivosSpecification(int page, int pageSize, string? busca = null)
    {
        SetCriteria(v => v.IsActive &&
            (busca == null ||
             v.Placa.Value.Contains(busca.ToUpper()) ||
             (v.Modelo != null && v.Modelo.ToLower().Contains(busca.ToLower()))));
        SetOrderBy(v => v.Placa);
        ApplyPaging(page, pageSize);
    }
}
