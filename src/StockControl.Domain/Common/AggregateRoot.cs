namespace StockControl.Domain.Common;

/// <summary>
/// Marca uma entidade como raiz de agregado (DDD).
/// Apenas Aggregate Roots podem ser acessados por repositórios.
/// </summary>
public abstract class AggregateRoot : BaseEntity
{
}
