using System.Text.Json.Serialization;
using FluentValidation;
using MediatR;
using StockControl.Application.Common;
using StockControl.Application.DTOs;
using StockControl.Application.Interfaces;
using StockControl.Domain.Common;
using StockControl.Domain.Enums;
using StockControl.Domain.Interfaces;
using StockControl.Domain.Repositories;
using StockControl.Domain.Services;

namespace StockControl.Application.Commands.Movimentacoes;

// ─── Criar ───────────────────────────────────────────────────────────────────

public sealed class CriarMovimentacaoCommand : IRequest<Result<MovimentacaoDto>>
{
    [JsonPropertyName("productId")]
    public Guid ProductId { get; set; }

    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("quantity")]
    public int Quantity { get; set; }

    [JsonPropertyName("note")]
    public string? Note { get; set; }
}

public sealed class CriarMovimentacaoCommandValidator : AbstractValidator<CriarMovimentacaoCommand>
{
    public CriarMovimentacaoCommandValidator()
    {
        RuleFor(x => x.ProductId).NotEmpty().WithMessage("O produto é obrigatório.");
        RuleFor(x => x.Type)
            .Must(tipo => Enum.TryParse<TipoMovimentacao>(tipo, out _))
            .WithMessage("Tipo de movimentação inválido.");
        RuleFor(x => x.Quantity).GreaterThan(0).WithMessage("A quantidade deve ser maior que zero.");
        RuleFor(x => x.Note).MaximumLength(500);
    }
}

public sealed class CriarMovimentacaoCommandHandler : IRequestHandler<CriarMovimentacaoCommand, Result<MovimentacaoDto>>
{
    private readonly IProdutoRepository _produtoRepository;
    private readonly IMovimentacaoRepository _movimentacaoRepository;
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly EstoqueDomainService _estoqueDomainService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cache;
    private readonly ICurrentUserService _currentUser;

    public CriarMovimentacaoCommandHandler(
        IProdutoRepository produtoRepository,
        IMovimentacaoRepository movimentacaoRepository,
        IUsuarioRepository usuarioRepository,
        EstoqueDomainService estoqueDomainService,
        IUnitOfWork unitOfWork,
        ICacheService cache,
        ICurrentUserService currentUser)
    {
        _produtoRepository = produtoRepository;
        _movimentacaoRepository = movimentacaoRepository;
        _usuarioRepository = usuarioRepository;
        _estoqueDomainService = estoqueDomainService;
        _unitOfWork = unitOfWork;
        _cache = cache;
        _currentUser = currentUser;
    }

    public async Task<Result<MovimentacaoDto>> Handle(CriarMovimentacaoCommand request, CancellationToken cancellationToken)
    {
        if (_currentUser.UserId is not { } usuarioId)
        {
            return Result.Failure<MovimentacaoDto>(Error.Unauthorized("Auth.NaoAutenticado", "Usuário não autenticado."));
        }

        var produto = await _produtoRepository.GetByIdAsync(request.ProductId, cancellationToken);
        if (produto is null)
        {
            return Result.Failure<MovimentacaoDto>(Error.NotFound("Produto.NaoEncontrado", "Produto não encontrado."));
        }

        var tipo = Enum.Parse<TipoMovimentacao>(request.Type);

        var movimentacaoResult = _estoqueDomainService.Movimentar(produto, tipo, request.Quantity, usuarioId, request.Note);
        if (movimentacaoResult.IsFailure)
        {
            return Result.Failure<MovimentacaoDto>(movimentacaoResult.Error);
        }

        _produtoRepository.Update(produto);
        await _movimentacaoRepository.AddAsync(movimentacaoResult.Value, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _cache.RemoveAsync(CacheKeys.Produtos, cancellationToken);

        var usuario = await _usuarioRepository.GetByIdAsync(usuarioId, cancellationToken);

        return new MovimentacaoDto
        {
            Id = movimentacaoResult.Value.Id,
            ProductId = produto.Id,
            ProductName = produto.Nome,
            Type = movimentacaoResult.Value.Tipo.ToString(),
            Quantity = movimentacaoResult.Value.Quantidade.Value,
            UserName = usuario?.Nome ?? string.Empty,
            Note = movimentacaoResult.Value.Observacao,
            CreatedAt = movimentacaoResult.Value.CreatedAt
        };
    }
}
