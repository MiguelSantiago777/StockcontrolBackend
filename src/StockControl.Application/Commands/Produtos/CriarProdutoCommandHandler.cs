using AutoMapper;
using MediatR;
using StockControl.Application.Common;
using StockControl.Application.DTOs;
using StockControl.Application.Interfaces;
using StockControl.Domain.Aggregates;
using StockControl.Domain.Common;
using StockControl.Domain.Interfaces;
using StockControl.Domain.Repositories;
using StockControl.Domain.ValueObjects;

namespace StockControl.Application.Commands.Produtos;

public sealed class CriarProdutoCommandHandler : IRequestHandler<CriarProdutoCommand, Result<ProdutoDto>>
{
    private readonly IProdutoRepository _produtoRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cache;
    private readonly IMapper _mapper;

    public CriarProdutoCommandHandler(
        IProdutoRepository produtoRepository,
        IUnitOfWork unitOfWork,
        ICacheService cache,
        IMapper mapper)
    {
        _produtoRepository = produtoRepository;
        _unitOfWork = unitOfWork;
        _cache = cache;
        _mapper = mapper;
    }

    public async Task<Result<ProdutoDto>> Handle(CriarProdutoCommand request, CancellationToken cancellationToken)
    {
        var codigoResult = CodigoProduto.Create(request.Codigo);
        if (codigoResult.IsFailure)
        {
            return Result.Failure<ProdutoDto>(codigoResult.Error);
        }

        if (await _produtoRepository.CodigoExisteAsync(codigoResult.Value, cancellationToken))
        {
            return Result.Failure<ProdutoDto>(
                Error.Conflict("Produto.CodigoDuplicado", "Já existe um produto com este código."));
        }

        var produtoResult = Produto.Criar(
            request.Nome,
            request.Descricao,
            request.Codigo,
            request.CodigoBarras,
            request.Preco,
            request.EstoqueInicial,
            request.EstoqueMinimo,
            request.CategoriaId,
            request.FornecedorId);

        if (produtoResult.IsFailure)
        {
            return Result.Failure<ProdutoDto>(produtoResult.Error);
        }

        await _produtoRepository.AddAsync(produtoResult.Value, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _cache.RemoveAsync(CacheKeys.Produtos, cancellationToken);

        return _mapper.Map<ProdutoDto>(produtoResult.Value);
    }
}
