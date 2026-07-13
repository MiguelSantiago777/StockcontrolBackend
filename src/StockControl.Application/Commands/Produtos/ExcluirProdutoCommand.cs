using MediatR;
using StockControl.Domain.Common;

namespace StockControl.Application.Commands.Produtos;

public sealed class ExcluirProdutoCommand : IRequest<Result>
{
    public Guid Id { get; set; }

    public ExcluirProdutoCommand(Guid id)
    {
        Id = id;
    }
}
