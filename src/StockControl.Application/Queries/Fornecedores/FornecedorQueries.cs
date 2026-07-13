using MediatR;
using StockControl.Application.Commands.Fornecedores;
using StockControl.Application.Common;
using StockControl.Application.DTOs;
using StockControl.Application.Interfaces;
using StockControl.Domain.Aggregates;
using StockControl.Domain.Common;
using StockControl.Domain.Repositories;
using StockControl.Domain.Specifications;

namespace StockControl.Application.Queries.Fornecedores;

// ─── Listar (paginado) ──────────────────────────────────────────────────────

public sealed class ListarFornecedoresQuery : IRequest<Result<PagedResultDto<FornecedorDto>>>
{
    public ListarFornecedoresQuery(int page = 1, int pageSize = 20, string? busca = null)
    {
        Page = page < 1 ? 1 : page;
        PageSize = pageSize is < 1 or > 100 ? 20 : pageSize;
        Busca = busca;
    }

    public int Page { get; }
    public int PageSize { get; }
    public string? Busca { get; }
}

public sealed class ListarFornecedoresQueryHandler : IRequestHandler<ListarFornecedoresQuery, Result<PagedResultDto<FornecedorDto>>>
{
    private readonly IRepository<Fornecedor> _repository;
    private readonly ICacheService _cache;

    public ListarFornecedoresQueryHandler(IRepository<Fornecedor> repository, ICacheService cache)
    {
        _repository = repository;
        _cache = cache;
    }

    public async Task<Result<PagedResultDto<FornecedorDto>>> Handle(
        ListarFornecedoresQuery request,
        CancellationToken cancellationToken)
    {
        var usaCache = request.Page == 1 && request.Busca is null;

        if (usaCache)
        {
            var cached = await _cache.GetAsync<PagedResultDto<FornecedorDto>>(CacheKeys.Fornecedores, cancellationToken);
            if (cached is not null)
            {
                return Result.Success(cached);
            }
        }

        var spec = new FornecedoresAtivosSpecification(request.Page, request.PageSize, request.Busca);
        var fornecedores = await _repository.ListAsync(spec, cancellationToken);
        var totalCount = await _repository.CountAsync(spec, cancellationToken);

        var resultado = new PagedResultDto<FornecedorDto>
        {
            Items = fornecedores.Select(f => f.ToDto()).ToList(),
            Page = request.Page,
            PageSize = request.PageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize)
        };

        if (usaCache)
        {
            await _cache.SetAsync(CacheKeys.Fornecedores, resultado, TimeSpan.FromMinutes(5), cancellationToken);
        }

        return Result.Success(resultado);
    }
}

// ─── Obter por Id ───────────────────────────────────────────────────────────

public sealed class ObterFornecedorPorIdQuery : IRequest<Result<FornecedorDto>>
{
    public ObterFornecedorPorIdQuery(Guid id) => Id = id;

    public Guid Id { get; }
}

public sealed class ObterFornecedorPorIdQueryHandler : IRequestHandler<ObterFornecedorPorIdQuery, Result<FornecedorDto>>
{
    private readonly IRepository<Fornecedor> _repository;

    public ObterFornecedorPorIdQueryHandler(IRepository<Fornecedor> repository)
    {
        _repository = repository;
    }

    public async Task<Result<FornecedorDto>> Handle(ObterFornecedorPorIdQuery request, CancellationToken cancellationToken)
    {
        var fornecedor = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (fornecedor is null)
        {
            return Result.Failure<FornecedorDto>(
                Error.NotFound("Fornecedor.NaoEncontrado", "Fornecedor não encontrado."));
        }

        return fornecedor.ToDto();
    }
}
