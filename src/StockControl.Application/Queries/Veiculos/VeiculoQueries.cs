using MediatR;
using StockControl.Application.Commands.Veiculos;
using StockControl.Application.Common;
using StockControl.Application.DTOs;
using StockControl.Application.Interfaces;
using StockControl.Domain.Aggregates;
using StockControl.Domain.Common;
using StockControl.Domain.Repositories;
using StockControl.Domain.Specifications;

namespace StockControl.Application.Queries.Veiculos;

// ─── Listar (paginado) ──────────────────────────────────────────────────────

public sealed class ListarVeiculosQuery : IRequest<Result<PagedResultDto<VeiculoDto>>>
{
    public ListarVeiculosQuery(int page = 1, int pageSize = 20, string? busca = null)
    {
        Page = page < 1 ? 1 : page;
        PageSize = pageSize is < 1 or > 100 ? 20 : pageSize;
        Busca = busca;
    }

    public int Page { get; }
    public int PageSize { get; }
    public string? Busca { get; }
}

public sealed class ListarVeiculosQueryHandler : IRequestHandler<ListarVeiculosQuery, Result<PagedResultDto<VeiculoDto>>>
{
    private readonly IRepository<Veiculo> _repository;
    private readonly ICacheService _cache;

    public ListarVeiculosQueryHandler(IRepository<Veiculo> repository, ICacheService cache)
    {
        _repository = repository;
        _cache = cache;
    }

    public async Task<Result<PagedResultDto<VeiculoDto>>> Handle(
        ListarVeiculosQuery request,
        CancellationToken cancellationToken)
    {
        var usaCache = request.Page == 1 && request.Busca is null;

        if (usaCache)
        {
            var cached = await _cache.GetAsync<PagedResultDto<VeiculoDto>>(CacheKeys.Veiculos, cancellationToken);
            if (cached is not null)
            {
                return Result.Success(cached);
            }
        }

        var spec = new VeiculosAtivosSpecification(request.Page, request.PageSize, request.Busca);
        var veiculos = await _repository.ListAsync(spec, cancellationToken);
        var totalCount = await _repository.CountAsync(spec, cancellationToken);

        var resultado = new PagedResultDto<VeiculoDto>
        {
            Items = veiculos.Select(v => v.ToDto()).ToList(),
            Page = request.Page,
            PageSize = request.PageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize)
        };

        if (usaCache)
        {
            await _cache.SetAsync(CacheKeys.Veiculos, resultado, TimeSpan.FromMinutes(5), cancellationToken);
        }

        return Result.Success(resultado);
    }
}

// ─── Obter por Id ───────────────────────────────────────────────────────────

public sealed class ObterVeiculoPorIdQuery : IRequest<Result<VeiculoDto>>
{
    public ObterVeiculoPorIdQuery(Guid id) => Id = id;

    public Guid Id { get; }
}

public sealed class ObterVeiculoPorIdQueryHandler : IRequestHandler<ObterVeiculoPorIdQuery, Result<VeiculoDto>>
{
    private readonly IRepository<Veiculo> _repository;

    public ObterVeiculoPorIdQueryHandler(IRepository<Veiculo> repository)
    {
        _repository = repository;
    }

    public async Task<Result<VeiculoDto>> Handle(ObterVeiculoPorIdQuery request, CancellationToken cancellationToken)
    {
        var veiculo = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (veiculo is null)
        {
            return Result.Failure<VeiculoDto>(Error.NotFound("Veiculo.NaoEncontrado", "Veículo não encontrado."));
        }

        return veiculo.ToDto();
    }
}
