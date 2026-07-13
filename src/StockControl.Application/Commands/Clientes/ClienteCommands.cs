using System.Text.Json.Serialization;
using FluentValidation;
using MediatR;
using StockControl.Application.Commands.Fornecedores;
using StockControl.Application.Common;
using StockControl.Application.DTOs;
using StockControl.Application.Interfaces;
using StockControl.Domain.Aggregates;
using StockControl.Domain.Common;
using StockControl.Domain.Interfaces;
using StockControl.Domain.Repositories;
using StockControl.Domain.ValueObjects;

namespace StockControl.Application.Commands.Clientes;

// ─── Criar ───────────────────────────────────────────────────────────────────

public sealed class CriarClienteCommand : IRequest<Result<ClienteDto>>
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("cpf")]
    public string Cpf { get; set; } = string.Empty;

    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;

    [JsonPropertyName("phone")]
    public string Phone { get; set; } = string.Empty;

    [JsonPropertyName("address")]
    public EnderecoInput Address { get; set; } = new();
}

public sealed class CriarClienteCommandValidator : AbstractValidator<CriarClienteCommand>
{
    public CriarClienteCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("O nome é obrigatório.").MaximumLength(200);
        RuleFor(x => x.Cpf).NotEmpty().WithMessage("O CPF é obrigatório.");
        RuleFor(x => x.Email).NotEmpty().WithMessage("O e-mail é obrigatório.");
        RuleFor(x => x.Phone).NotEmpty().WithMessage("O telefone é obrigatório.");
        RuleFor(x => x.Address.Street).NotEmpty().WithMessage("O logradouro é obrigatório.");
        RuleFor(x => x.Address.City).NotEmpty().WithMessage("A cidade é obrigatória.");
        RuleFor(x => x.Address.State).NotEmpty().WithMessage("A UF é obrigatória.");
        RuleFor(x => x.Address.ZipCode).NotEmpty().WithMessage("O CEP é obrigatório.");
    }
}

public sealed class CriarClienteCommandHandler : IRequestHandler<CriarClienteCommand, Result<ClienteDto>>
{
    private readonly IClienteRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cache;

    public CriarClienteCommandHandler(IClienteRepository repository, IUnitOfWork unitOfWork, ICacheService cache)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _cache = cache;
    }

    public async Task<Result<ClienteDto>> Handle(CriarClienteCommand request, CancellationToken cancellationToken)
    {
        var cpfResult = Cpf.Create(request.Cpf);
        if (cpfResult.IsFailure)
        {
            return Result.Failure<ClienteDto>(cpfResult.Error);
        }

        if (await _repository.CpfExisteAsync(cpfResult.Value, cancellationToken: cancellationToken))
        {
            return Result.Failure<ClienteDto>(
                Error.Conflict("Cliente.CpfDuplicado", "Já existe um cliente com este CPF."));
        }

        var emailResult = Email.Create(request.Email);
        if (emailResult.IsFailure)
        {
            return Result.Failure<ClienteDto>(emailResult.Error);
        }

        var telefoneResult = Telefone.Create(request.Phone);
        if (telefoneResult.IsFailure)
        {
            return Result.Failure<ClienteDto>(telefoneResult.Error);
        }

        var enderecoResult = request.Address.ToEndereco();
        if (enderecoResult.IsFailure)
        {
            return Result.Failure<ClienteDto>(enderecoResult.Error);
        }

        var clienteResult = Cliente.Criar(
            request.Name,
            cpfResult.Value,
            emailResult.Value,
            telefoneResult.Value,
            enderecoResult.Value);

        if (clienteResult.IsFailure)
        {
            return Result.Failure<ClienteDto>(clienteResult.Error);
        }

        await _repository.AddAsync(clienteResult.Value, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _cache.RemoveAsync(CacheKeys.Clientes, cancellationToken);

        return clienteResult.Value.ToDto();
    }
}

// ─── Atualizar ───────────────────────────────────────────────────────────────

public sealed class AtualizarClienteCommand : IRequest<Result<ClienteDto>>
{
    [JsonIgnore] // preenchido pelo controller a partir da rota
    public Guid Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("cpf")]
    public string Cpf { get; set; } = string.Empty;

    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;

    [JsonPropertyName("phone")]
    public string Phone { get; set; } = string.Empty;

    [JsonPropertyName("address")]
    public EnderecoInput Address { get; set; } = new();
}

public sealed class AtualizarClienteCommandValidator : AbstractValidator<AtualizarClienteCommand>
{
    public AtualizarClienteCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().WithMessage("O nome é obrigatório.").MaximumLength(200);
        RuleFor(x => x.Cpf).NotEmpty().WithMessage("O CPF é obrigatório.");
        RuleFor(x => x.Email).NotEmpty().WithMessage("O e-mail é obrigatório.");
        RuleFor(x => x.Phone).NotEmpty().WithMessage("O telefone é obrigatório.");
        RuleFor(x => x.Address.Street).NotEmpty().WithMessage("O logradouro é obrigatório.");
        RuleFor(x => x.Address.City).NotEmpty().WithMessage("A cidade é obrigatória.");
        RuleFor(x => x.Address.State).NotEmpty().WithMessage("A UF é obrigatória.");
        RuleFor(x => x.Address.ZipCode).NotEmpty().WithMessage("O CEP é obrigatório.");
    }
}

public sealed class AtualizarClienteCommandHandler : IRequestHandler<AtualizarClienteCommand, Result<ClienteDto>>
{
    private readonly IClienteRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cache;

    public AtualizarClienteCommandHandler(IClienteRepository repository, IUnitOfWork unitOfWork, ICacheService cache)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _cache = cache;
    }

    public async Task<Result<ClienteDto>> Handle(AtualizarClienteCommand request, CancellationToken cancellationToken)
    {
        var cliente = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (cliente is null)
        {
            return Result.Failure<ClienteDto>(
                Error.NotFound("Cliente.NaoEncontrado", "Cliente não encontrado."));
        }

        var cpfResult = Cpf.Create(request.Cpf);
        if (cpfResult.IsFailure)
        {
            return Result.Failure<ClienteDto>(cpfResult.Error);
        }

        if (await _repository.CpfExisteAsync(cpfResult.Value, cliente.Id, cancellationToken))
        {
            return Result.Failure<ClienteDto>(
                Error.Conflict("Cliente.CpfDuplicado", "Já existe um cliente com este CPF."));
        }

        var emailResult = Email.Create(request.Email);
        if (emailResult.IsFailure)
        {
            return Result.Failure<ClienteDto>(emailResult.Error);
        }

        var telefoneResult = Telefone.Create(request.Phone);
        if (telefoneResult.IsFailure)
        {
            return Result.Failure<ClienteDto>(telefoneResult.Error);
        }

        var enderecoResult = request.Address.ToEndereco();
        if (enderecoResult.IsFailure)
        {
            return Result.Failure<ClienteDto>(enderecoResult.Error);
        }

        var atualizarResult = cliente.Atualizar(
            request.Name,
            cpfResult.Value,
            emailResult.Value,
            telefoneResult.Value,
            enderecoResult.Value);

        if (atualizarResult.IsFailure)
        {
            return Result.Failure<ClienteDto>(atualizarResult.Error);
        }

        _repository.Update(cliente);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _cache.RemoveAsync(CacheKeys.Clientes, cancellationToken);

        return cliente.ToDto();
    }
}

// ─── Excluir ─────────────────────────────────────────────────────────────────

public sealed class ExcluirClienteCommand : IRequest<Result>
{
    public Guid Id { get; }

    public ExcluirClienteCommand(Guid id) => Id = id;
}

public sealed class ExcluirClienteCommandHandler : IRequestHandler<ExcluirClienteCommand, Result>
{
    private readonly IClienteRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cache;

    public ExcluirClienteCommandHandler(IClienteRepository repository, IUnitOfWork unitOfWork, ICacheService cache)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _cache = cache;
    }

    public async Task<Result> Handle(ExcluirClienteCommand request, CancellationToken cancellationToken)
    {
        var cliente = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (cliente is null)
        {
            return Result.Failure(Error.NotFound("Cliente.NaoEncontrado", "Cliente não encontrado."));
        }

        _repository.Remove(cliente); // soft delete
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _cache.RemoveAsync(CacheKeys.Clientes, cancellationToken);

        return Result.Success();
    }
}

// ─── Mapeamento simples (sem AutoMapper: entidade pequena) ───────────────────

internal static class ClienteMapping
{
    public static ClienteDto ToDto(this Cliente cliente) => new()
    {
        Id = cliente.Id,
        Nome = cliente.Nome,
        Cpf = cliente.Cpf.Value,
        Email = cliente.Email.Value,
        Phone = cliente.Telefone.Ddd + cliente.Telefone.Numero,
        Address = new EnderecoDto
        {
            Street = cliente.Endereco.Logradouro,
            Number = cliente.Endereco.Numero,
            Complement = cliente.Endereco.Complemento,
            Neighborhood = cliente.Endereco.Bairro,
            City = cliente.Endereco.Cidade,
            State = cliente.Endereco.Uf,
            ZipCode = cliente.Endereco.Cep
        },
        IsActive = cliente.IsActive
    };
}
