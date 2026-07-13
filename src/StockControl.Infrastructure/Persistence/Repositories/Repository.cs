using Microsoft.EntityFrameworkCore;
using StockControl.Domain.Common;
using StockControl.Domain.Repositories;
using StockControl.Domain.Specifications;
using StockControl.Infrastructure.Persistence.Context;

namespace StockControl.Infrastructure.Persistence.Repositories;

public class Repository<T> : IRepository<T> where T : AggregateRoot
{
    protected readonly AppDbContext Context;

    public Repository(AppDbContext context)
    {
        Context = context;
    }

    protected DbSet<T> DbSet => Context.Set<T>();

    public virtual async Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await DbSet.FirstOrDefaultAsync(entity => entity.Id == id, cancellationToken);
    }

    public virtual async Task<IReadOnlyList<T>> ListAsync(CancellationToken cancellationToken = default)
    {
        return await DbSet.AsNoTracking().ToListAsync(cancellationToken);
    }

    public virtual async Task<IReadOnlyList<T>> ListAsync(
        ISpecification<T> specification,
        CancellationToken cancellationToken = default)
    {
        return await ApplySpecification(specification).AsNoTracking().ToListAsync(cancellationToken);
    }

    public virtual async Task<T?> FirstOrDefaultAsync(
        ISpecification<T> specification,
        CancellationToken cancellationToken = default)
    {
        return await ApplySpecification(specification).FirstOrDefaultAsync(cancellationToken);
    }

    public virtual async Task<int> CountAsync(
        ISpecification<T> specification,
        CancellationToken cancellationToken = default)
    {
        return await ApplySpecification(specification, ignorePaging: true).CountAsync(cancellationToken);
    }

    public virtual async Task AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        await DbSet.AddAsync(entity, cancellationToken);
    }

    public virtual void Update(T entity)
    {
        DbSet.Update(entity);
    }

    public virtual void Remove(T entity)
    {
        entity.SoftDelete();
        DbSet.Update(entity);
    }

    protected IQueryable<T> ApplySpecification(ISpecification<T> spec, bool ignorePaging = false)
    {
        var query = DbSet.AsQueryable();

        if (spec.Criteria is not null)
        {
            query = query.Where(spec.Criteria);
        }

        foreach (var include in spec.Includes)
        {
            query = query.Include(include);
        }

        if (spec.OrderBy is not null)
        {
            query = query.OrderBy(spec.OrderBy);
        }

        if (spec.OrderByDescending is not null)
        {
            query = query.OrderByDescending(spec.OrderByDescending);
        }

        if (!ignorePaging)
        {
            if (spec.Skip.HasValue)
            {
                query = query.Skip(spec.Skip.Value);
            }

            if (spec.Take.HasValue)
            {
                query = query.Take(spec.Take.Value);
            }
        }

        return query;
    }
}
