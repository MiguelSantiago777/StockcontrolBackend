using MediatR;
using StockControl.Application.DTOs;
using StockControl.Domain.Common;

namespace StockControl.Application.Commands.Produtos;

public sealed class UploadImagemProdutoCommand : IRequest<Result<ProdutoDto>>
{
    public Guid ProdutoId { get; set; }
    public Stream Conteudo { get; set; } = Stream.Null;
    public string NomeArquivo { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
}
