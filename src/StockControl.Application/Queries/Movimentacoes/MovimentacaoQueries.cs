using MediatR;
using StockControl.Application.Common;
using StockControl.Application.DTOs;
using StockControl.Domain.Aggregates;
using StockControl.Domain.Common;
using StockControl.Domain.Enums;
using StockControl.Domain.Repositories;
using StockControl.Domain.Specifications;

namespace StockControl.Application.Queries.Movimentacoes;

public sealed class ListarMovimentacoesQuery : IRequest<Result<PagedResultDto<MovimentacaoDto>>>
{
    public ListarMovimentacoesQuery(
        int page = 1,
        int pageSize = 20,
        string? type = null,
        Guid? productId = null,
        DateTime? from = null,
        DateTime? to = null)
    {
        Page = page < 1 ? 1 : page;
        PageSize = pageSize is < 1 or > 5000 ? 20 : pageSize;
        Tipo = Enum.TryParse<TipoMovimentacao>(type, out var tipo) ? tipo : null;
        ProductId = productId;
        From = from;
        To = to;
    }

    public int Page { get; }
    public int PageSize { get; }
    public TipoMovimentacao? Tipo { get; }
    public Guid? ProductId { get; }
    public DateTime? From { get; }
    public DateTime? To { get; }
}

public sealed class ListarMovimentacoesQueryHandler : IRequestHandler<ListarMovimentacoesQuery, Result<PagedResultDto<MovimentacaoDto>>>
{
    private readonly IRepository<Movimentacao> _movimentacaoRepository;
    private readonly IRepository<Produto> _produtoRepository;
    private readonly IRepository<Usuario> _usuarioRepository;

    public ListarMovimentacoesQueryHandler(
        IRepository<Movimentacao> movimentacaoRepository,
        IRepository<Produto> produtoRepository,
        IRepository<Usuario> usuarioRepository)
    {
        _movimentacaoRepository = movimentacaoRepository;
        _produtoRepository = produtoRepository;
        _usuarioRepository = usuarioRepository;
    }

    public async Task<Result<PagedResultDto<MovimentacaoDto>>> Handle(
        ListarMovimentacoesQuery request,
        CancellationToken cancellationToken)
    {
        var spec = new MovimentacoesFiltradasSpecification(
            request.Page, request.PageSize, request.Tipo, request.ProductId, request.From, request.To);

        var movimentacoes = await _movimentacaoRepository.ListAsync(spec, cancellationToken);
        var totalCount = await _movimentacaoRepository.CountAsync(spec, cancellationToken);

        var produtos = (await _produtoRepository.ListAsync(cancellationToken)).ToDictionary(p => p.Id, p => p.Nome);
        var usuarios = (await _usuarioRepository.ListAsync(cancellationToken)).ToDictionary(u => u.Id, u => u.Nome);

        var dtos = movimentacoes.Select(m => new MovimentacaoDto
        {
            Id = m.Id,
            ProductId = m.ProdutoId,
            ProductName = produtos.GetValueOrDefault(m.ProdutoId, "—"),
            Type = m.Tipo.ToString(),
            Quantity = m.Quantidade.Value,
            UserName = usuarios.GetValueOrDefault(m.UsuarioId, "—"),
            Note = m.Observacao,
            CreatedAt = m.CreatedAt
        }).ToList();

        var resultado = new PagedResultDto<MovimentacaoDto>
        {
            Items = dtos,
            Page = request.Page,
            PageSize = request.PageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize)
        };

        return Result.Success(resultado);
    }
}
