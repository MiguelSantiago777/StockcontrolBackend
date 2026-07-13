using MediatR;
using StockControl.Application.Common;
using StockControl.Application.Interfaces;
using StockControl.Domain.Common;
using StockControl.Domain.Interfaces;
using StockControl.Domain.Repositories;

namespace StockControl.Application.Commands.Produtos;

public sealed class ExcluirProdutoCommandHandler : IRequestHandler<ExcluirProdutoCommand, Result>
{
    private readonly IProdutoRepository _produtoRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cache;

    public ExcluirProdutoCommandHandler(
        IProdutoRepository produtoRepository,
        IUnitOfWork unitOfWork,
        ICacheService cache)
    {
        _produtoRepository = produtoRepository;
        _unitOfWork = unitOfWork;
        _cache = cache;
    }

    public async Task<Result> Handle(ExcluirProdutoCommand request, CancellationToken cancellationToken)
    {
        var produto = await _produtoRepository.GetByIdAsync(request.Id, cancellationToken);
        if (produto is null)
        {
            return Result.Failure(Error.NotFound("Produto.NaoEncontrado", "Produto não encontrado."));
        }

        _produtoRepository.Remove(produto);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _cache.RemoveAsync(CacheKeys.Produtos, cancellationToken);

        return Result.Success();
    }
}
