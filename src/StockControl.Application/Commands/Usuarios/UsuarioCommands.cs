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

namespace StockControl.Application.Commands.Usuarios;

// ─── Criar ───────────────────────────────────────────────────────────────────

public sealed class CriarUsuarioCommand : IRequest<Result<UsuarioDto>>
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;

    [JsonPropertyName("password")]
    public string Password { get; set; } = string.Empty;

    [JsonPropertyName("role")]
    public string Role { get; set; } = string.Empty;
}

public sealed class CriarUsuarioCommandValidator : AbstractValidator<CriarUsuarioCommand>
{
    public CriarUsuarioCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("O nome é obrigatório.").MaximumLength(200);
        RuleFor(x => x.Email).NotEmpty().WithMessage("O e-mail é obrigatório.");
        RuleFor(x => x.Password).MinimumLength(8).WithMessage("A senha deve ter pelo menos 8 caracteres.");
        RuleFor(x => x.Role)
            .Must(role => Enum.TryParse<PerfilUsuario>(role, out _))
            .WithMessage("Perfil inválido.");
    }
}

public sealed class CriarUsuarioCommandHandler : IRequestHandler<CriarUsuarioCommand, Result<UsuarioDto>>
{
    private readonly IUsuarioRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ICacheService _cache;

    public CriarUsuarioCommandHandler(
        IUsuarioRepository repository,
        IUnitOfWork unitOfWork,
        IPasswordHasher passwordHasher,
        ICacheService cache)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
        _cache = cache;
    }

    public async Task<Result<UsuarioDto>> Handle(CriarUsuarioCommand request, CancellationToken cancellationToken)
    {
        var emailResult = Email.Create(request.Email);
        if (emailResult.IsFailure)
        {
            return Result.Failure<UsuarioDto>(emailResult.Error);
        }

        if (await _repository.EmailExisteAsync(emailResult.Value, cancellationToken: cancellationToken))
        {
            return Result.Failure<UsuarioDto>(
                Error.Conflict("Usuario.EmailDuplicado", "Já existe um usuário com este e-mail."));
        }

        var senhaResult = SenhaHash.Create(_passwordHasher.Hash(request.Password));
        if (senhaResult.IsFailure)
        {
            return Result.Failure<UsuarioDto>(senhaResult.Error);
        }

        var perfil = Enum.Parse<PerfilUsuario>(request.Role);

        var usuarioResult = Usuario.Criar(request.Name, emailResult.Value, senhaResult.Value, perfil);
        if (usuarioResult.IsFailure)
        {
            return Result.Failure<UsuarioDto>(usuarioResult.Error);
        }

        await _repository.AddAsync(usuarioResult.Value, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _cache.RemoveAsync(CacheKeys.Usuarios, cancellationToken);

        return usuarioResult.Value.ToDto();
    }
}

// ─── Atualizar ───────────────────────────────────────────────────────────────

public sealed class AtualizarUsuarioCommand : IRequest<Result<UsuarioDto>>
{
    [JsonIgnore] // preenchido pelo controller a partir da rota
    public Guid Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;

    [JsonPropertyName("role")]
    public string Role { get; set; } = string.Empty;
}

public sealed class AtualizarUsuarioCommandValidator : AbstractValidator<AtualizarUsuarioCommand>
{
    public AtualizarUsuarioCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().WithMessage("O nome é obrigatório.").MaximumLength(200);
        RuleFor(x => x.Email).NotEmpty().WithMessage("O e-mail é obrigatório.");
        RuleFor(x => x.Role)
            .Must(role => Enum.TryParse<PerfilUsuario>(role, out _))
            .WithMessage("Perfil inválido.");
    }
}

public sealed class AtualizarUsuarioCommandHandler : IRequestHandler<AtualizarUsuarioCommand, Result<UsuarioDto>>
{
    private readonly IUsuarioRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cache;

    public AtualizarUsuarioCommandHandler(IUsuarioRepository repository, IUnitOfWork unitOfWork, ICacheService cache)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _cache = cache;
    }

    public async Task<Result<UsuarioDto>> Handle(AtualizarUsuarioCommand request, CancellationToken cancellationToken)
    {
        var usuario = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (usuario is null)
        {
            return Result.Failure<UsuarioDto>(
                Error.NotFound("Usuario.NaoEncontrado", "Usuário não encontrado."));
        }

        var emailResult = Email.Create(request.Email);
        if (emailResult.IsFailure)
        {
            return Result.Failure<UsuarioDto>(emailResult.Error);
        }

        if (await _repository.EmailExisteAsync(emailResult.Value, usuario.Id, cancellationToken))
        {
            return Result.Failure<UsuarioDto>(
                Error.Conflict("Usuario.EmailDuplicado", "Já existe um usuário com este e-mail."));
        }

        var perfil = Enum.Parse<PerfilUsuario>(request.Role);

        var atualizarResult = usuario.Atualizar(request.Name, emailResult.Value, perfil);
        if (atualizarResult.IsFailure)
        {
            return Result.Failure<UsuarioDto>(atualizarResult.Error);
        }

        _repository.Update(usuario);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _cache.RemoveAsync(CacheKeys.Usuarios, cancellationToken);

        return usuario.ToDto();
    }
}

// ─── Excluir ─────────────────────────────────────────────────────────────────

public sealed class ExcluirUsuarioCommand : IRequest<Result>
{
    public Guid Id { get; }

    public ExcluirUsuarioCommand(Guid id) => Id = id;
}

public sealed class ExcluirUsuarioCommandHandler : IRequestHandler<ExcluirUsuarioCommand, Result>
{
    private readonly IUsuarioRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cache;

    public ExcluirUsuarioCommandHandler(IUsuarioRepository repository, IUnitOfWork unitOfWork, ICacheService cache)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _cache = cache;
    }

    public async Task<Result> Handle(ExcluirUsuarioCommand request, CancellationToken cancellationToken)
    {
        var usuario = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (usuario is null)
        {
            return Result.Failure(Error.NotFound("Usuario.NaoEncontrado", "Usuário não encontrado."));
        }

        _repository.Remove(usuario); // soft delete
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _cache.RemoveAsync(CacheKeys.Usuarios, cancellationToken);

        return Result.Success();
    }
}

// ─── Mapeamento simples (sem AutoMapper: entidade pequena) ───────────────────

internal static class UsuarioMapping
{
    public static UsuarioDto ToDto(this Usuario usuario) => new()
    {
        Id = usuario.Id,
        Name = usuario.Nome,
        Email = usuario.Email.Value,
        Role = usuario.Perfil.ToString(),
        IsActive = usuario.IsActive
    };
}
