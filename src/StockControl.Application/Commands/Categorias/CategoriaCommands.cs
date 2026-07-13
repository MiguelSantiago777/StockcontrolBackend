using System.Text.Json.Serialization;
using FluentValidation;
using MediatR;
using StockControl.Application.Common;
using StockControl.Application.DTOs;
using StockControl.Application.Interfaces;
using StockControl.Domain.Common;
using StockControl.Domain.Entities;
using StockControl.Domain.Interfaces;
using StockControl.Domain.Repositories;

namespace StockControl.Application.Commands.Categorias;

// ─── Criar ───────────────────────────────────────────────────────────────────

public sealed class CriarCategoriaCommand : IRequest<Result<CategoriaDto>>
{
    [JsonPropertyName("name")]
    public string Nome { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string? Descricao { get; set; }
}

public sealed class CriarCategoriaCommandValidator : AbstractValidator<CriarCategoriaCommand>
{
    public CriarCategoriaCommandValidator()
    {
        RuleFor(x => x.Nome)
            .NotEmpty().WithMessage("O nome é obrigatório.")
            .MaximumLength(100);

        RuleFor(x => x.Descricao).MaximumLength(500);
    }
}

public sealed class CriarCategoriaCommandHandler : IRequestHandler<CriarCategoriaCommand, Result<CategoriaDto>>
{
    private readonly IRepository<Categoria> _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cache;

    public CriarCategoriaCommandHandler(IRepository<Categoria> repository, IUnitOfWork unitOfWork, ICacheService cache)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _cache = cache;
    }

    public async Task<Result<CategoriaDto>> Handle(CriarCategoriaCommand request, CancellationToken cancellationToken)
    {
        var categoriaResult = Categoria.Criar(request.Nome, request.Descricao);
        if (categoriaResult.IsFailure)
        {
            return Result.Failure<CategoriaDto>(categoriaResult.Error);
        }

        await _repository.AddAsync(categoriaResult.Value, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _cache.RemoveAsync(CacheKeys.Categorias, cancellationToken);

        return categoriaResult.Value.ToDto();
    }
}

// ─── Atualizar ───────────────────────────────────────────────────────────────

public sealed class AtualizarCategoriaCommand : IRequest<Result<CategoriaDto>>
{
    [JsonIgnore] // preenchido pelo controller a partir da rota
    public Guid Id { get; set; }

    [JsonPropertyName("name")]
    public string Nome { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string? Descricao { get; set; }
}

public sealed class AtualizarCategoriaCommandValidator : AbstractValidator<AtualizarCategoriaCommand>
{
    public AtualizarCategoriaCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();

        RuleFor(x => x.Nome)
            .NotEmpty().WithMessage("O nome é obrigatório.")
            .MaximumLength(100);

        RuleFor(x => x.Descricao).MaximumLength(500);
    }
}

public sealed class AtualizarCategoriaCommandHandler : IRequestHandler<AtualizarCategoriaCommand, Result<CategoriaDto>>
{
    private readonly IRepository<Categoria> _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cache;

    public AtualizarCategoriaCommandHandler(IRepository<Categoria> repository, IUnitOfWork unitOfWork, ICacheService cache)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _cache = cache;
    }

    public async Task<Result<CategoriaDto>> Handle(AtualizarCategoriaCommand request, CancellationToken cancellationToken)
    {
        var categoria = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (categoria is null)
        {
            return Result.Failure<CategoriaDto>(
                Error.NotFound("Categoria.NaoEncontrada", "Categoria não encontrada."));
        }

        var atualizarResult = categoria.Atualizar(request.Nome, request.Descricao);
        if (atualizarResult.IsFailure)
        {
            return Result.Failure<CategoriaDto>(atualizarResult.Error);
        }

        _repository.Update(categoria);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _cache.RemoveAsync(CacheKeys.Categorias, cancellationToken);

        return categoria.ToDto();
    }
}

// ─── Excluir ─────────────────────────────────────────────────────────────────

public sealed class ExcluirCategoriaCommand : IRequest<Result>
{
    public Guid Id { get; set; }

    public ExcluirCategoriaCommand(Guid id) => Id = id;
}

public sealed class ExcluirCategoriaCommandHandler : IRequestHandler<ExcluirCategoriaCommand, Result>
{
    private readonly IRepository<Categoria> _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cache;

    public ExcluirCategoriaCommandHandler(IRepository<Categoria> repository, IUnitOfWork unitOfWork, ICacheService cache)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _cache = cache;
    }

    public async Task<Result> Handle(ExcluirCategoriaCommand request, CancellationToken cancellationToken)
    {
        var categoria = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (categoria is null)
        {
            return Result.Failure(Error.NotFound("Categoria.NaoEncontrada", "Categoria não encontrada."));
        }

        _repository.Remove(categoria); // soft delete
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _cache.RemoveAsync(CacheKeys.Categorias, cancellationToken);

        return Result.Success();
    }
}

// ─── Mapeamento simples (sem AutoMapper: entidade trivial) ──────────────────

internal static class CategoriaMapping
{
    public static CategoriaDto ToDto(this Categoria categoria) => new()
    {
        Id = categoria.Id,
        Nome = categoria.Nome,
        Descricao = categoria.Descricao,
        IsActive = categoria.IsActive
    };
}
