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

namespace StockControl.Application.Commands.Entregadores;

// ─── Criar ───────────────────────────────────────────────────────────────────

public sealed class CriarEntregadorCommand : IRequest<Result<EntregadorDto>>
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("cpf")]
    public string Cpf { get; set; } = string.Empty;

    [JsonPropertyName("phone")]
    public string Phone { get; set; } = string.Empty;

    [JsonPropertyName("userId")]
    public Guid UserId { get; set; }
}

public sealed class CriarEntregadorCommandValidator : AbstractValidator<CriarEntregadorCommand>
{
    public CriarEntregadorCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("O nome é obrigatório.").MaximumLength(200);
        RuleFor(x => x.Cpf).NotEmpty().WithMessage("O CPF é obrigatório.");
        RuleFor(x => x.Phone).NotEmpty().WithMessage("O telefone é obrigatório.");
        RuleFor(x => x.UserId).NotEmpty().WithMessage("O usuário vinculado é obrigatório.");
    }
}

public sealed class CriarEntregadorCommandHandler : IRequestHandler<CriarEntregadorCommand, Result<EntregadorDto>>
{
    private readonly IEntregadorRepository _repository;
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cache;

    public CriarEntregadorCommandHandler(
        IEntregadorRepository repository,
        IUsuarioRepository usuarioRepository,
        IUnitOfWork unitOfWork,
        ICacheService cache)
    {
        _repository = repository;
        _usuarioRepository = usuarioRepository;
        _unitOfWork = unitOfWork;
        _cache = cache;
    }

    public async Task<Result<EntregadorDto>> Handle(CriarEntregadorCommand request, CancellationToken cancellationToken)
    {
        var cpfResult = Cpf.Create(request.Cpf);
        if (cpfResult.IsFailure)
        {
            return Result.Failure<EntregadorDto>(cpfResult.Error);
        }

        if (await _repository.CpfExisteAsync(cpfResult.Value, cancellationToken: cancellationToken))
        {
            return Result.Failure<EntregadorDto>(
                Error.Conflict("Entregador.CpfDuplicado", "Já existe um entregador com este CPF."));
        }

        var usuario = await _usuarioRepository.GetByIdAsync(request.UserId, cancellationToken);
        if (usuario is null || usuario.Perfil != PerfilUsuario.Entregador)
        {
            return Result.Failure<EntregadorDto>(
                Error.Validation("Entregador.UsuarioInvalido", "Selecione um usuário com perfil Entregador."));
        }

        if (await _repository.UsuarioJaVinculadoAsync(usuario.Id, cancellationToken: cancellationToken))
        {
            return Result.Failure<EntregadorDto>(
                Error.Conflict("Entregador.UsuarioJaVinculado", "Este usuário já está vinculado a outro entregador."));
        }

        var telefoneResult = Telefone.Create(request.Phone);
        if (telefoneResult.IsFailure)
        {
            return Result.Failure<EntregadorDto>(telefoneResult.Error);
        }

        var entregadorResult = Entregador.Criar(request.Name, cpfResult.Value, telefoneResult.Value, usuario.Id);
        if (entregadorResult.IsFailure)
        {
            return Result.Failure<EntregadorDto>(entregadorResult.Error);
        }

        await _repository.AddAsync(entregadorResult.Value, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _cache.RemoveAsync(CacheKeys.Entregadores, cancellationToken);

        return entregadorResult.Value.ToDto();
    }
}

// ─── Atualizar ───────────────────────────────────────────────────────────────

public sealed class AtualizarEntregadorCommand : IRequest<Result<EntregadorDto>>
{
    [JsonIgnore] // preenchido pelo controller a partir da rota
    public Guid Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("phone")]
    public string Phone { get; set; } = string.Empty;
}

public sealed class AtualizarEntregadorCommandValidator : AbstractValidator<AtualizarEntregadorCommand>
{
    public AtualizarEntregadorCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().WithMessage("O nome é obrigatório.").MaximumLength(200);
        RuleFor(x => x.Phone).NotEmpty().WithMessage("O telefone é obrigatório.");
    }
}

public sealed class AtualizarEntregadorCommandHandler : IRequestHandler<AtualizarEntregadorCommand, Result<EntregadorDto>>
{
    private readonly IEntregadorRepository _repository;
    private readonly IRepository<Veiculo> _veiculoRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cache;

    public AtualizarEntregadorCommandHandler(
        IEntregadorRepository repository,
        IRepository<Veiculo> veiculoRepository,
        IUnitOfWork unitOfWork,
        ICacheService cache)
    {
        _repository = repository;
        _veiculoRepository = veiculoRepository;
        _unitOfWork = unitOfWork;
        _cache = cache;
    }

    public async Task<Result<EntregadorDto>> Handle(AtualizarEntregadorCommand request, CancellationToken cancellationToken)
    {
        var entregador = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (entregador is null)
        {
            return Result.Failure<EntregadorDto>(Error.NotFound("Entregador.NaoEncontrado", "Entregador não encontrado."));
        }

        var telefoneResult = Telefone.Create(request.Phone);
        if (telefoneResult.IsFailure)
        {
            return Result.Failure<EntregadorDto>(telefoneResult.Error);
        }

        var atualizarResult = entregador.Atualizar(request.Name, telefoneResult.Value);
        if (atualizarResult.IsFailure)
        {
            return Result.Failure<EntregadorDto>(atualizarResult.Error);
        }

        _repository.Update(entregador);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _cache.RemoveAsync(CacheKeys.Entregadores, cancellationToken);

        var veiculoAtual = entregador.VeiculoAtualId is { } veiculoId
            ? await _veiculoRepository.GetByIdAsync(veiculoId, cancellationToken)
            : null;
        return entregador.ToDto(veiculoAtual);
    }
}

// ─── Atualizar status ──────────────────────────────────────────────────────

public sealed class AtualizarStatusEntregadorCommand : IRequest<Result<EntregadorDto>>
{
    [JsonIgnore]
    public Guid Id { get; set; }

    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;
}

public sealed class AtualizarStatusEntregadorCommandValidator : AbstractValidator<AtualizarStatusEntregadorCommand>
{
    public AtualizarStatusEntregadorCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Status)
            .Must(status => Enum.TryParse<StatusEntregador>(status, out _))
            .WithMessage("Status inválido.");
    }
}

public sealed class AtualizarStatusEntregadorCommandHandler : IRequestHandler<AtualizarStatusEntregadorCommand, Result<EntregadorDto>>
{
    private readonly IEntregadorRepository _repository;
    private readonly IRepository<Veiculo> _veiculoRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cache;

    public AtualizarStatusEntregadorCommandHandler(
        IEntregadorRepository repository,
        IRepository<Veiculo> veiculoRepository,
        IUnitOfWork unitOfWork,
        ICacheService cache)
    {
        _repository = repository;
        _veiculoRepository = veiculoRepository;
        _unitOfWork = unitOfWork;
        _cache = cache;
    }

    public async Task<Result<EntregadorDto>> Handle(AtualizarStatusEntregadorCommand request, CancellationToken cancellationToken)
    {
        var entregador = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (entregador is null)
        {
            return Result.Failure<EntregadorDto>(Error.NotFound("Entregador.NaoEncontrado", "Entregador não encontrado."));
        }

        switch (Enum.Parse<StatusEntregador>(request.Status))
        {
            case StatusEntregador.Disponivel: entregador.FicarDisponivel(); break;
            case StatusEntregador.EmEntrega: entregador.IniciarEntrega(); break;
            case StatusEntregador.Indisponivel: entregador.FicarIndisponivel(); break;
        }

        _repository.Update(entregador);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _cache.RemoveAsync(CacheKeys.Entregadores, cancellationToken);

        var veiculoAtual = entregador.VeiculoAtualId is { } veiculoId
            ? await _veiculoRepository.GetByIdAsync(veiculoId, cancellationToken)
            : null;
        return entregador.ToDto(veiculoAtual);
    }
}

// ─── Atualizar posição (dado de exemplo, sem rastreamento ao vivo) ──────────

public sealed class AtualizarPosicaoEntregadorCommand : IRequest<Result<EntregadorDto>>
{
    [JsonIgnore]
    public Guid Id { get; set; }

    [JsonPropertyName("latitude")]
    public double Latitude { get; set; }

    [JsonPropertyName("longitude")]
    public double Longitude { get; set; }
}

public sealed class AtualizarPosicaoEntregadorCommandHandler : IRequestHandler<AtualizarPosicaoEntregadorCommand, Result<EntregadorDto>>
{
    private readonly IEntregadorRepository _repository;
    private readonly IRepository<Veiculo> _veiculoRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cache;

    public AtualizarPosicaoEntregadorCommandHandler(
        IEntregadorRepository repository,
        IRepository<Veiculo> veiculoRepository,
        IUnitOfWork unitOfWork,
        ICacheService cache)
    {
        _repository = repository;
        _veiculoRepository = veiculoRepository;
        _unitOfWork = unitOfWork;
        _cache = cache;
    }

    public async Task<Result<EntregadorDto>> Handle(AtualizarPosicaoEntregadorCommand request, CancellationToken cancellationToken)
    {
        var entregador = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (entregador is null)
        {
            return Result.Failure<EntregadorDto>(Error.NotFound("Entregador.NaoEncontrado", "Entregador não encontrado."));
        }

        var result = entregador.AtualizarPosicao(request.Latitude, request.Longitude);
        if (result.IsFailure)
        {
            return Result.Failure<EntregadorDto>(result.Error);
        }

        _repository.Update(entregador);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _cache.RemoveAsync(CacheKeys.Entregadores, cancellationToken);

        var veiculoAtual = entregador.VeiculoAtualId is { } veiculoId
            ? await _veiculoRepository.GetByIdAsync(veiculoId, cancellationToken)
            : null;
        return entregador.ToDto(veiculoAtual);
    }
}

// ─── Excluir ─────────────────────────────────────────────────────────────────

public sealed class ExcluirEntregadorCommand : IRequest<Result>
{
    public Guid Id { get; }

    public ExcluirEntregadorCommand(Guid id) => Id = id;
}

public sealed class ExcluirEntregadorCommandHandler : IRequestHandler<ExcluirEntregadorCommand, Result>
{
    private readonly IEntregadorRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cache;

    public ExcluirEntregadorCommandHandler(IEntregadorRepository repository, IUnitOfWork unitOfWork, ICacheService cache)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _cache = cache;
    }

    public async Task<Result> Handle(ExcluirEntregadorCommand request, CancellationToken cancellationToken)
    {
        var entregador = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (entregador is null)
        {
            return Result.Failure(Error.NotFound("Entregador.NaoEncontrado", "Entregador não encontrado."));
        }

        _repository.Remove(entregador); // soft delete
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _cache.RemoveAsync(CacheKeys.Entregadores, cancellationToken);

        return Result.Success();
    }
}

// ─── Mapeamento simples (sem AutoMapper: entidade pequena) ───────────────────

internal static class EntregadorMapping
{
    public static EntregadorDto ToDto(this Entregador entregador, Veiculo? veiculoAtual = null) => new()
    {
        Id = entregador.Id,
        Name = entregador.Nome,
        Cpf = entregador.Cpf.Value,
        Phone = entregador.Telefone.Ddd + entregador.Telefone.Numero,
        UserId = entregador.UsuarioId,
        Status = entregador.Status.ToString(),
        LastPosition = entregador.UltimaPosicao is null
            ? null
            : new CoordenadaDto { Latitude = entregador.UltimaPosicao.Latitude, Longitude = entregador.UltimaPosicao.Longitude },
        PositionUpdatedAt = entregador.PosicaoAtualizadaEm,
        VehicleId = veiculoAtual?.Id,
        VehiclePlate = veiculoAtual?.Placa.Value,
        VehicleType = veiculoAtual?.Tipo.ToString(),
        VehicleModel = veiculoAtual?.Modelo,
        IsActive = entregador.IsActive
    };
}
