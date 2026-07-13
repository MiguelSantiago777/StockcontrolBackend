using MediatR;
using StockControl.Application.DTOs;
using StockControl.Domain.Common;

namespace StockControl.Application.Queries.Produtos;

public sealed class ObterProdutoPorIdQuery : IRequest<Result<ProdutoDto>>
{
    public ObterProdutoPorIdQuery(Guid id)
    {
        Id = id;
    }

    public Guid Id { get; }
}
