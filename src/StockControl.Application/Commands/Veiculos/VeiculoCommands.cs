using System.Text.Json.Serialization;
using FluentValidation;
using MediatR;
using StockControl.Application.Common;
using StockControl.Application.DTOs;
using StockControl.Application.Interfaces;
using StockControl.Domain.Aggregates;
using StockControl.Domain.Common;
using StockControl.Domain.Enums;
using StockControl.Domain.Interfaces;
using StockControl.Domain.Repositories;
using StockControl.Domain.ValueObjects;

namespace StockControl.Application.Commands.Veiculos;

// ─── Criar ───────────────────────────────────────────────────────────────────

public sealed class CriarVeiculoCommand : IRequest<Result<VeiculoDto>>
{
    [JsonPropertyName("plate")]
    public string Plate { get; set; } = string.Empty;

    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("model")]
    public string? Model { get; set; }
}

public sealed class CriarVeiculoCommandValidator : AbstractValidator<CriarVeiculoCommand>
{
    public CriarVeiculoCommandValidator()
    {
        RuleFor(x => x.Plate).NotEmpty().WithMessage("A placa é obrigatória.");
        RuleFor(x => x.Type)
            .Must(tipo => Enum.TryParse<TipoVeiculo>(tipo, out _))
            .WithMessage("Tipo de veículo inválido.");
        RuleFor(x => x.Model).MaximumLength(100);
    }
}

public sealed class CriarVeiculoCommandHandler : IRequestHandler<CriarVeiculoCommand, Result<VeiculoDto>>
{
    private readonly IVeiculoRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cache;

    public CriarVeiculoCommandHandler(IVeiculoRepository repository, IUnitOfWork unitOfWork, ICacheService cache)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _cache = cache;
    }

    public async Task<Result<VeiculoDto>> Handle(CriarVeiculoCommand request, CancellationToken cancellationToken)
    {
        var placaResult = Placa.Create(request.Plate);
        if (placaResult.IsFailure)
        {
            return Result.Failure<VeiculoDto>(placaResult.Error);
        }

        if (await _repository.PlacaExisteAsync(placaResult.Value, cancellationToken: cancellationToken))
        {
            return Result.Failure<VeiculoDto>(
                Error.Conflict("Veiculo.PlacaDuplicada", "Já existe um veículo com esta placa."));
        }

        var tipo = Enum.Parse<TipoVeiculo>(request.Type);

        var veiculoResult = Veiculo.Criar(placaResult.Value, tipo, request.Model);
        if (veiculoResult.IsFailure)
        {
            return Result.Failure<VeiculoDto>(veiculoResult.Error);
        }

        await _repository.AddAsync(veiculoResult.Value, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _cache.RemoveAsync(CacheKeys.Veiculos, cancellationToken);

        return veiculoResult.Value.ToDto();
    }
}

// ─── Atualizar ───────────────────────────────────────────────────────────────

public sealed class AtualizarVeiculoCommand : IRequest<Result<VeiculoDto>>
{
    [JsonIgnore]
    public Guid Id { get; set; }

    [JsonPropertyName("plate")]
    public string Plate { get; set; } = string.Empty;

    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("model")]
    public string? Model { get; set; }
}

public sealed class AtualizarVeiculoCommandValidator : AbstractValidator<AtualizarVeiculoCommand>
{
    public AtualizarVeiculoCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Plate).NotEmpty().WithMessage("A placa é obrigatória.");
        RuleFor(x => x.Type)
            .Must(tipo => Enum.TryParse<TipoVeiculo>(tipo, out _))
            .WithMessage("Tipo de veículo inválido.");
        RuleFor(x => x.Model).MaximumLength(100);
    }
}

public sealed class AtualizarVeiculoCommandHandler : IRequestHandler<AtualizarVeiculoCommand, Result<VeiculoDto>>
{
    private readonly IVeiculoRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cache;

    public AtualizarVeiculoCommandHandler(IVeiculoRepository repository, IUnitOfWork unitOfWork, ICacheService cache)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _cache = cache;
    }

    public async Task<Result<VeiculoDto>> Handle(AtualizarVeiculoCommand request, CancellationToken cancellationToken)
    {
        var veiculo = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (veiculo is null)
        {
            return Result.Failure<VeiculoDto>(Error.NotFound("Veiculo.NaoEncontrado", "Veículo não encontrado."));
        }

        var placaResult = Placa.Create(request.Plate);
        if (placaResult.IsFailure)
        {
            return Result.Failure<VeiculoDto>(placaResult.Error);
        }

        if (await _repository.PlacaExisteAsync(placaResult.Value, veiculo.Id, cancellationToken))
        {
            return Result.Failure<VeiculoDto>(
                Error.Conflict("Veiculo.PlacaDuplicada", "Já existe um veículo com esta placa."));
        }

        var tipo = Enum.Parse<TipoVeiculo>(request.Type);

        var atualizarResult = veiculo.Atualizar(placaResult.Value, tipo, request.Model);
        if (atualizarResult.IsFailure)
        {
            return Result.Failure<VeiculoDto>(atualizarResult.Error);
        }

        _repository.Update(veiculo);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _cache.RemoveAsync(CacheKeys.Veiculos, cancellationToken);

        return veiculo.ToDto();
    }
}

// ─── Excluir ─────────────────────────────────────────────────────────────────

public sealed class ExcluirVeiculoCommand : IRequest<Result>
{
    public Guid Id { get; }

    public ExcluirVeiculoCommand(Guid id) => Id = id;
}

public sealed class ExcluirVeiculoCommandHandler : IRequestHandler<ExcluirVeiculoCommand, Result>
{
    private readonly IVeiculoRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cache;

    public ExcluirVeiculoCommandHandler(IVeiculoRepository repository, IUnitOfWork unitOfWork, ICacheService cache)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _cache = cache;
    }

    public async Task<Result> Handle(ExcluirVeiculoCommand request, CancellationToken cancellationToken)
    {
        var veiculo = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (veiculo is null)
        {
            return Result.Failure(Error.NotFound("Veiculo.NaoEncontrado", "Veículo não encontrado."));
        }

        _repository.Remove(veiculo); // soft delete
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _cache.RemoveAsync(CacheKeys.Veiculos, cancellationToken);

        return Result.Success();
    }
}

// ─── Mapeamento simples (sem AutoMapper: entidade pequena) ───────────────────

internal static class VeiculoMapping
{
    public static VeiculoDto ToDto(this Veiculo veiculo) => new()
    {
        Id = veiculo.Id,
        Plate = veiculo.Placa.Value,
        Type = veiculo.Tipo.ToString(),
        Model = veiculo.Modelo,
        IsActive = veiculo.IsActive
    };
}
