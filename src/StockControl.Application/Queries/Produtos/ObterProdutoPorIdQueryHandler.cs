using AutoMapper;
using MediatR;
using StockControl.Application.DTOs;
using StockControl.Domain.Common;
using StockControl.Domain.Repositories;

namespace StockControl.Application.Queries.Produtos;

public sealed class ObterProdutoPorIdQueryHandler : IRequestHandler<ObterProdutoPorIdQuery, Result<ProdutoDto>>
{
    private readonly IProdutoRepository _produtoRepository;
    private readonly IMapper _mapper;

    public ObterProdutoPorIdQueryHandler(IProdutoRepository produtoRepository, IMapper mapper)
    {
        _produtoRepository = produtoRepository;
        _mapper = mapper;
    }

    public async Task<Result<ProdutoDto>> Handle(ObterProdutoPorIdQuery request, CancellationToken cancellationToken)
    {
        var produto = await _produtoRepository.GetByIdAsync(request.Id, cancellationToken);

        if (produto is null)
        {
            return Result.Failure<ProdutoDto>(
                Error.NotFound("Produto.NaoEncontrado", "Produto não encontrado."));
        }

        return _mapper.Map<ProdutoDto>(produto);
    }
}
