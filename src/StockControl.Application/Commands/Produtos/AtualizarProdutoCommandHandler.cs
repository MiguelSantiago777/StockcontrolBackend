using AutoMapper;
using MediatR;
using StockControl.Application.Common;
using StockControl.Application.DTOs;
using StockControl.Application.Interfaces;
using StockControl.Domain.Common;
using StockControl.Domain.Interfaces;
using StockControl.Domain.Repositories;
using StockControl.Domain.ValueObjects;

namespace StockControl.Application.Commands.Produtos;

public sealed class AtualizarProdutoCommandHandler : IRequestHandler<AtualizarProdutoCommand, Result<ProdutoDto>>
{
    private readonly IProdutoRepository _produtoRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cache;
    private readonly IMapper _mapper;

    public AtualizarProdutoCommandHandler(
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

    public async Task<Result<ProdutoDto>> Handle(AtualizarProdutoCommand request, CancellationToken cancellationToken)
    {
        var produto = await _produtoRepository.GetByIdAsync(request.Id, cancellationToken);
        if (produto is null)
        {
            return Result.Failure<ProdutoDto>(
                Error.NotFound("Produto.NaoEncontrado", "Produto não encontrado."));
        }

        var codigoResult = CodigoProduto.Create(request.Codigo);
        if (codigoResult.IsFailure)
        {
            return Result.Failure<ProdutoDto>(codigoResult.Error);
        }

        var existente = await _produtoRepository.ObterPorCodigoAsync(codigoResult.Value, cancellationToken);
        if (existente is not null && existente.Id != produto.Id)
        {
            return Result.Failure<ProdutoDto>(
                Error.Conflict("Produto.CodigoDuplicado", "Já existe um produto com este código."));
        }

        var atualizarResult = produto.AtualizarDados(
            request.Nome,
            request.Descricao,
            request.Codigo,
            request.CodigoBarras,
            request.Preco,
            request.Estoque,
            request.EstoqueMinimo,
            request.CategoriaId,
            request.FornecedorId);

        if (atualizarResult.IsFailure)
        {
            return Result.Failure<ProdutoDto>(atualizarResult.Error);
        }

        _produtoRepository.Update(produto);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _cache.RemoveAsync(CacheKeys.Produtos, cancellationToken);

        return _mapper.Map<ProdutoDto>(produto);
    }
}
