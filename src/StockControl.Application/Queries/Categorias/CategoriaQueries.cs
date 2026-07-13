using MediatR;
using StockControl.Application.Commands.Categorias;
using StockControl.Application.Common;
using StockControl.Application.DTOs;
using StockControl.Application.Interfaces;
using StockControl.Domain.Common;
using StockControl.Domain.Entities;
using StockControl.Domain.Repositories;
using StockControl.Domain.Specifications;

namespace StockControl.Application.Queries.Categorias;

// ─── Listar (paginado) ──────────────────────────────────────────────────────

public sealed class ListarCategoriasQuery : IRequest<Result<PagedResultDto<CategoriaDto>>>
{
    public ListarCategoriasQuery(int page = 1, int pageSize = 20, string? busca = null)
    {
        Page = page < 1 ? 1 : page;
        PageSize = pageSize is < 1 or > 100 ? 20 : pageSize;
        Busca = busca;
    }

    public int Page { get; }
    public int PageSize { get; }
    public string? Busca { get; }
}

public sealed class ListarCategoriasQueryHandler : IRequestHandler<ListarCategoriasQuery, Result<PagedResultDto<CategoriaDto>>>
{
    private readonly IRepository<Categoria> _repository;
    private readonly ICacheService _cache;

    public ListarCategoriasQueryHandler(IRepository<Categoria> repository, ICacheService cache)
    {
        _repository = repository;
        _cache = cache;
    }

    public async Task<Result<PagedResultDto<CategoriaDto>>> Handle(
        ListarCategoriasQuery request,
        CancellationToken cancellationToken)
    {
        var usaCache = request.Page == 1 && request.Busca is null;

        if (usaCache)
        {
            var cached = await _cache.GetAsync<PagedResultDto<CategoriaDto>>(CacheKeys.Categorias, cancellationToken);
            if (cached is not null)
            {
                return Result.Success(cached);
            }
        }

        var spec = new CategoriasAtivasSpecification(request.Page, request.PageSize, request.Busca);
        var categorias = await _repository.ListAsync(spec, cancellationToken);
        var totalCount = await _repository.CountAsync(spec, cancellationToken);

        var resultado = new PagedResultDto<CategoriaDto>
        {
            Items = categorias.Select(c => c.ToDto()).ToList(),
            Page = request.Page,
            PageSize = request.PageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize)
        };

        if (usaCache)
        {
            await _cache.SetAsync(CacheKeys.Categorias, resultado, TimeSpan.FromMinutes(5), cancellationToken);
        }

        return Result.Success(resultado);
    }
}

// ─── Obter por Id ───────────────────────────────────────────────────────────

public sealed class ObterCategoriaPorIdQuery : IRequest<Result<CategoriaDto>>
{
    public ObterCategoriaPorIdQuery(Guid id) => Id = id;

    public Guid Id { get; }
}

public sealed class ObterCategoriaPorIdQueryHandler : IRequestHandler<ObterCategoriaPorIdQuery, Result<CategoriaDto>>
{
    private readonly IRepository<Categoria> _repository;

    public ObterCategoriaPorIdQueryHandler(IRepository<Categoria> repository)
    {
        _repository = repository;
    }

    public async Task<Result<CategoriaDto>> Handle(ObterCategoriaPorIdQuery request, CancellationToken cancellationToken)
    {
        var categoria = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (categoria is null)
        {
            return Result.Failure<CategoriaDto>(
                Error.NotFound("Categoria.NaoEncontrada", "Categoria não encontrada."));
        }

        return categoria.ToDto();
    }
}
