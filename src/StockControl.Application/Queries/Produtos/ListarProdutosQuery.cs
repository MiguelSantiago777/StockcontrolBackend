using AutoMapper;
using MediatR;
using StockControl.Application.Common;
using StockControl.Application.DTOs;
using StockControl.Application.Interfaces;
using StockControl.Domain.Common;
using StockControl.Domain.Repositories;
using StockControl.Domain.Specifications;

namespace StockControl.Application.Queries.Produtos;

public sealed class ListarProdutosQuery : IRequest<Result<PagedResultDto<ProdutoDto>>>
{
    public ListarProdutosQuery(int page = 1, int pageSize = 20, string? busca = null)
    {
        Page = page < 1 ? 1 : page;
        PageSize = pageSize is < 1 or > 100 ? 20 : pageSize;
        Busca = busca;
    }

    public int Page { get; }
    public int PageSize { get; }
    public string? Busca { get; }
}

public sealed class ListarProdutosQueryHandler : IRequestHandler<ListarProdutosQuery, Result<PagedResultDto<ProdutoDto>>>
{
    private readonly IProdutoRepository _produtoRepository;
    private readonly ICacheService _cache;
    private readonly IMapper _mapper;

    public ListarProdutosQueryHandler(
        IProdutoRepository produtoRepository,
        ICacheService cache,
        IMapper mapper)
    {
        _produtoRepository = produtoRepository;
        _cache = cache;
        _mapper = mapper;
    }

    public async Task<Result<PagedResultDto<ProdutoDto>>> Handle(
        ListarProdutosQuery request,
        CancellationToken cancellationToken)
    {
        var cacheKey = $"{CacheKeys.Produtos}:{request.Page}:{request.PageSize}:{request.Busca}";
        var usaCache = request.Page == 1 && request.Busca is null;

        if (usaCache)
        {
            var cached = await _cache.GetAsync<PagedResultDto<ProdutoDto>>(cacheKey, cancellationToken);
            if (cached is not null)
            {
                return Result.Success(cached);
            }
        }

        var spec = new ProdutosAtivosSpecification(request.Page, request.PageSize, request.Busca);
        var produtos = await _produtoRepository.ListAsync(spec, cancellationToken);
        var totalCount = await _produtoRepository.CountAsync(spec, cancellationToken);
        var dtos = _mapper.Map<IReadOnlyList<ProdutoDto>>(produtos);

        var resultado = new PagedResultDto<ProdutoDto>
        {
            Items = dtos,
            Page = request.Page,
            PageSize = request.PageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize)
        };

        if (usaCache)
        {
            await _cache.SetAsync(cacheKey, resultado, TimeSpan.FromMinutes(5), cancellationToken);
        }

        return Result.Success(resultado);
    }
}
