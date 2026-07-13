using MediatR;
using StockControl.Application.Commands.Clientes;
using StockControl.Application.Common;
using StockControl.Application.DTOs;
using StockControl.Application.Interfaces;
using StockControl.Domain.Aggregates;
using StockControl.Domain.Common;
using StockControl.Domain.Repositories;
using StockControl.Domain.Specifications;

namespace StockControl.Application.Queries.Clientes;

// ─── Listar (paginado) ──────────────────────────────────────────────────────

public sealed class ListarClientesQuery : IRequest<Result<PagedResultDto<ClienteDto>>>
{
    public ListarClientesQuery(int page = 1, int pageSize = 20, string? busca = null)
    {
        Page = page < 1 ? 1 : page;
        PageSize = pageSize is < 1 or > 100 ? 20 : pageSize;
        Busca = busca;
    }

    public int Page { get; }
    public int PageSize { get; }
    public string? Busca { get; }
}

public sealed class ListarClientesQueryHandler : IRequestHandler<ListarClientesQuery, Result<PagedResultDto<ClienteDto>>>
{
    private readonly IRepository<Cliente> _repository;
    private readonly ICacheService _cache;

    public ListarClientesQueryHandler(IRepository<Cliente> repository, ICacheService cache)
    {
        _repository = repository;
        _cache = cache;
    }

    public async Task<Result<PagedResultDto<ClienteDto>>> Handle(
        ListarClientesQuery request,
        CancellationToken cancellationToken)
    {
        var usaCache = request.Page == 1 && request.Busca is null;

        if (usaCache)
        {
            var cached = await _cache.GetAsync<PagedResultDto<ClienteDto>>(CacheKeys.Clientes, cancellationToken);
            if (cached is not null)
            {
                return Result.Success(cached);
            }
        }

        var spec = new ClientesAtivosSpecification(request.Page, request.PageSize, request.Busca);
        var clientes = await _repository.ListAsync(spec, cancellationToken);
        var totalCount = await _repository.CountAsync(spec, cancellationToken);

        var resultado = new PagedResultDto<ClienteDto>
        {
            Items = clientes.Select(c => c.ToDto()).ToList(),
            Page = request.Page,
            PageSize = request.PageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize)
        };

        if (usaCache)
        {
            await _cache.SetAsync(CacheKeys.Clientes, resultado, TimeSpan.FromMinutes(5), cancellationToken);
        }

        return Result.Success(resultado);
    }
}

// ─── Obter por Id ───────────────────────────────────────────────────────────

public sealed class ObterClientePorIdQuery : IRequest<Result<ClienteDto>>
{
    public ObterClientePorIdQuery(Guid id) => Id = id;

    public Guid Id { get; }
}

public sealed class ObterClientePorIdQueryHandler : IRequestHandler<ObterClientePorIdQuery, Result<ClienteDto>>
{
    private readonly IRepository<Cliente> _repository;

    public ObterClientePorIdQueryHandler(IRepository<Cliente> repository)
    {
        _repository = repository;
    }

    public async Task<Result<ClienteDto>> Handle(ObterClientePorIdQuery request, CancellationToken cancellationToken)
    {
        var cliente = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (cliente is null)
        {
            return Result.Failure<ClienteDto>(
                Error.NotFound("Cliente.NaoEncontrado", "Cliente não encontrado."));
        }

        return cliente.ToDto();
    }
}
