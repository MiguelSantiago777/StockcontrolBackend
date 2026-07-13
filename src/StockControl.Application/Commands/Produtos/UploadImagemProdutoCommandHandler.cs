using AutoMapper;
using MediatR;
using StockControl.Application.Common;
using StockControl.Application.DTOs;
using StockControl.Application.Interfaces;
using StockControl.Domain.Common;
using StockControl.Domain.Interfaces;
using StockControl.Domain.Repositories;

namespace StockControl.Application.Commands.Produtos;

public sealed class UploadImagemProdutoCommandHandler : IRequestHandler<UploadImagemProdutoCommand, Result<ProdutoDto>>
{
    private static readonly HashSet<string> TiposPermitidos =
        new(StringComparer.OrdinalIgnoreCase) { "image/jpeg", "image/png", "image/webp" };

    private static readonly HashSet<string> ExtensoesPermitidas =
        new(StringComparer.OrdinalIgnoreCase) { ".jpg", ".jpeg", ".png", ".webp" };

    private const long TamanhoMaximoBytes = 5 * 1024 * 1024; // 5 MB

    private readonly IProdutoRepository _produtoRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IFileStorageService _fileStorage;
    private readonly ICacheService _cache;
    private readonly IMapper _mapper;

    public UploadImagemProdutoCommandHandler(
        IProdutoRepository produtoRepository,
        IUnitOfWork unitOfWork,
        IFileStorageService fileStorage,
        ICacheService cache,
        IMapper mapper)
    {
        _produtoRepository = produtoRepository;
        _unitOfWork = unitOfWork;
        _fileStorage = fileStorage;
        _cache = cache;
        _mapper = mapper;
    }

    public async Task<Result<ProdutoDto>> Handle(UploadImagemProdutoCommand request, CancellationToken cancellationToken)
    {
        var produto = await _produtoRepository.GetByIdAsync(request.ProdutoId, cancellationToken);
        if (produto is null)
        {
            return Result.Failure<ProdutoDto>(
                Error.NotFound("Produto.NaoEncontrado", "Produto não encontrado."));
        }

        var extensao = Path.GetExtension(request.NomeArquivo);
        if (!TiposPermitidos.Contains(request.ContentType) || !ExtensoesPermitidas.Contains(extensao))
        {
            return Result.Failure<ProdutoDto>(Error.Validation(
                "Produto.ImagemInvalida", "Formato de imagem inválido. Use JPG, PNG ou WEBP."));
        }

        if (request.Conteudo.Length > TamanhoMaximoBytes)
        {
            return Result.Failure<ProdutoDto>(Error.Validation(
                "Produto.ImagemMuitoGrande", "A imagem deve ter no máximo 5 MB."));
        }

        var nomeArquivo = $"{Guid.NewGuid()}{extensao}";
        var url = await _fileStorage.SalvarAsync(
            $"produtos/{produto.Id}", nomeArquivo, request.Conteudo, request.ContentType, cancellationToken);

        produto.DefinirImagem(url);
        _produtoRepository.Update(produto);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _cache.RemoveAsync(CacheKeys.Produtos, cancellationToken);

        return _mapper.Map<ProdutoDto>(produto);
    }
}
