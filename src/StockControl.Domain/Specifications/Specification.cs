using System.Linq.Expressions;
using StockControl.Domain.Common;

namespace StockControl.Domain.Specifications;

public abstract class Specification<T> : ISpecification<T> where T : AggregateRoot
{
    public Expression<Func<T, bool>>? Criteria { get; private set; }
    public List<Expression<Func<T, object>>> Includes { get; } = [];
    public Expression<Func<T, object>>? OrderBy { get; private set; }
    public Expression<Func<T, object>>? OrderByDescending { get; private set; }
    public int? Skip { get; private set; }
    public int? Take { get; private set; }

    protected void SetCriteria(Expression<Func<T, bool>> criteria) => Criteria = criteria;
    protected void AddInclude(Expression<Func<T, object>> include) => Includes.Add(include);
    protected void SetOrderBy(Expression<Func<T, object>> orderBy) => OrderBy = orderBy;
    protected void SetOrderByDescending(Expression<Func<T, object>> orderBy) => OrderByDescending = orderBy;

    protected void ApplyPaging(int page, int pageSize)
    {
        Skip = (page - 1) * pageSize;
        Take = pageSize;
    }
}
