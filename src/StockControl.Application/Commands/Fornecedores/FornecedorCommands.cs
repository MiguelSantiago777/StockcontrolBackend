using System.Text.Json.Serialization;
using FluentValidation;
using MediatR;
using StockControl.Application.Common;
using StockControl.Application.DTOs;
using StockControl.Application.Interfaces;
using StockControl.Domain.Aggregates;
using StockControl.Domain.Common;
using StockControl.Domain.Interfaces;
using StockControl.Domain.Repositories;
using StockControl.Domain.ValueObjects;

namespace StockControl.Application.Commands.Fornecedores;

// ─── Endereço (input compartilhado entre Criar/Atualizar) ────────────────────

public sealed class EnderecoInput
{
    [JsonPropertyName("street")]
    public string? Street { get; set; }

    [JsonPropertyName("number")]
    public string? Number { get; set; }

    [JsonPropertyName("complement")]
    public string? Complement { get; set; }

    [JsonPropertyName("neighborhood")]
    public string? Neighborhood { get; set; }

    [JsonPropertyName("city")]
    public string? City { get; set; }

    [JsonPropertyName("state")]
    public string? State { get; set; }

    [JsonPropertyName("zipCode")]
    public string? ZipCode { get; set; }
}

internal static class EnderecoInputExtensions
{
    public static Result<Endereco> ToEndereco(this EnderecoInput input) =>
        Endereco.Create(input.Street, input.Number, input.Complement, input.Neighborhood, input.City, input.State, input.ZipCode);
}

// ─── Criar ───────────────────────────────────────────────────────────────────

public sealed class CriarFornecedorCommand : IRequest<Result<FornecedorDto>>
{
    [JsonPropertyName("companyName")]
    public string CompanyName { get; set; } = string.Empty;

    [JsonPropertyName("tradeName")]
    public string? TradeName { get; set; }

    [JsonPropertyName("cnpj")]
    public string Cnpj { get; set; } = string.Empty;

    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;

    [JsonPropertyName("phone")]
    public string Phone { get; set; } = string.Empty;

    [JsonPropertyName("address")]
    public EnderecoInput Address { get; set; } = new();
}

public sealed class CriarFornecedorCommandValidator : AbstractValidator<CriarFornecedorCommand>
{
    public CriarFornecedorCommandValidator()
    {
        RuleFor(x => x.CompanyName).NotEmpty().WithMessage("A razão social é obrigatória.").MaximumLength(200);
        RuleFor(x => x.Cnpj).NotEmpty().WithMessage("O CNPJ é obrigatório.");
        RuleFor(x => x.Email).NotEmpty().WithMessage("O e-mail é obrigatório.");
        RuleFor(x => x.Phone).NotEmpty().WithMessage("O telefone é obrigatório.");
        RuleFor(x => x.Address.Street).NotEmpty().WithMessage("O logradouro é obrigatório.");
        RuleFor(x => x.Address.City).NotEmpty().WithMessage("A cidade é obrigatória.");
        RuleFor(x => x.Address.State).NotEmpty().WithMessage("A UF é obrigatória.");
        RuleFor(x => x.Address.ZipCode).NotEmpty().WithMessage("O CEP é obrigatório.");
    }
}

public sealed class CriarFornecedorCommandHandler : IRequestHandler<CriarFornecedorCommand, Result<FornecedorDto>>
{
    private readonly IFornecedorRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cache;

    public CriarFornecedorCommandHandler(IFornecedorRepository repository, IUnitOfWork unitOfWork, ICacheService cache)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _cache = cache;
    }

    public async Task<Result<FornecedorDto>> Handle(CriarFornecedorCommand request, CancellationToken cancellationToken)
    {
        var cnpjResult = Cnpj.Create(request.Cnpj);
        if (cnpjResult.IsFailure)
        {
            return Result.Failure<FornecedorDto>(cnpjResult.Error);
        }

        if (await _repository.CnpjExisteAsync(cnpjResult.Value, cancellationToken: cancellationToken))
        {
            return Result.Failure<FornecedorDto>(
                Error.Conflict("Fornecedor.CnpjDuplicado", "Já existe um fornecedor com este CNPJ."));
        }

        var emailResult = Email.Create(request.Email);
        if (emailResult.IsFailure)
        {
            return Result.Failure<FornecedorDto>(emailResult.Error);
        }

        var telefoneResult = Telefone.Create(request.Phone);
        if (telefoneResult.IsFailure)
        {
            return Result.Failure<FornecedorDto>(telefoneResult.Error);
        }

        var enderecoResult = request.Address.ToEndereco();
        if (enderecoResult.IsFailure)
        {
            return Result.Failure<FornecedorDto>(enderecoResult.Error);
        }

        var fornecedorResult = Fornecedor.Criar(
            request.CompanyName,
            request.TradeName,
            cnpjResult.Value,
            emailResult.Value,
            telefoneResult.Value,
            enderecoResult.Value);

        if (fornecedorResult.IsFailure)
        {
            return Result.Failure<FornecedorDto>(fornecedorResult.Error);
        }

        await _repository.AddAsync(fornecedorResult.Value, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _cache.RemoveAsync(CacheKeys.Fornecedores, cancellationToken);

        return fornecedorResult.Value.ToDto();
    }
}

// ─── Atualizar ───────────────────────────────────────────────────────────────

public sealed class AtualizarFornecedorCommand : IRequest<Result<FornecedorDto>>
{
    [JsonIgnore] // preenchido pelo controller a partir da rota
    public Guid Id { get; set; }

    [JsonPropertyName("companyName")]
    public string CompanyName { get; set; } = string.Empty;

    [JsonPropertyName("tradeName")]
    public string? TradeName { get; set; }

    [JsonPropertyName("cnpj")]
    public string Cnpj { get; set; } = string.Empty;

    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;

    [JsonPropertyName("phone")]
    public string Phone { get; set; } = string.Empty;

    [JsonPropertyName("address")]
    public EnderecoInput Address { get; set; } = new();
}

public sealed class AtualizarFornecedorCommandValidator : AbstractValidator<AtualizarFornecedorCommand>
{
    public AtualizarFornecedorCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.CompanyName).NotEmpty().WithMessage("A razão social é obrigatória.").MaximumLength(200);
        RuleFor(x => x.Cnpj).NotEmpty().WithMessage("O CNPJ é obrigatório.");
        RuleFor(x => x.Email).NotEmpty().WithMessage("O e-mail é obrigatório.");
        RuleFor(x => x.Phone).NotEmpty().WithMessage("O telefone é obrigatório.");
        RuleFor(x => x.Address.Street).NotEmpty().WithMessage("O logradouro é obrigatório.");
        RuleFor(x => x.Address.City).NotEmpty().WithMessage("A cidade é obrigatória.");
        RuleFor(x => x.Address.State).NotEmpty().WithMessage("A UF é obrigatória.");
        RuleFor(x => x.Address.ZipCode).NotEmpty().WithMessage("O CEP é obrigatório.");
    }
}

public sealed class AtualizarFornecedorCommandHandler : IRequestHandler<AtualizarFornecedorCommand, Result<FornecedorDto>>
{
    private readonly IFornecedorRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cache;

    public AtualizarFornecedorCommandHandler(IFornecedorRepository repository, IUnitOfWork unitOfWork, ICacheService cache)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _cache = cache;
    }

    public async Task<Result<FornecedorDto>> Handle(AtualizarFornecedorCommand request, CancellationToken cancellationToken)
    {
        var fornecedor = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (fornecedor is null)
        {
            return Result.Failure<FornecedorDto>(
                Error.NotFound("Fornecedor.NaoEncontrado", "Fornecedor não encontrado."));
        }

        var cnpjResult = Cnpj.Create(request.Cnpj);
        if (cnpjResult.IsFailure)
        {
            return Result.Failure<FornecedorDto>(cnpjResult.Error);
        }

        if (await _repository.CnpjExisteAsync(cnpjResult.Value, fornecedor.Id, cancellationToken))
        {
            return Result.Failure<FornecedorDto>(
                Error.Conflict("Fornecedor.CnpjDuplicado", "Já existe um fornecedor com este CNPJ."));
        }

        var emailResult = Email.Create(request.Email);
        if (emailResult.IsFailure)
        {
            return Result.Failure<FornecedorDto>(emailResult.Error);
        }

        var telefoneResult = Telefone.Create(request.Phone);
        if (telefoneResult.IsFailure)
        {
            return Result.Failure<FornecedorDto>(telefoneResult.Error);
        }

        var enderecoResult = request.Address.ToEndereco();
        if (enderecoResult.IsFailure)
        {
            return Result.Failure<FornecedorDto>(enderecoResult.Error);
        }

        var atualizarResult = fornecedor.Atualizar(
            request.CompanyName,
            request.TradeName,
            cnpjResult.Value,
            emailResult.Value,
            telefoneResult.Value,
            enderecoResult.Value);

        if (atualizarResult.IsFailure)
        {
            return Result.Failure<FornecedorDto>(atualizarResult.Error);
        }

        _repository.Update(fornecedor);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _cache.RemoveAsync(CacheKeys.Fornecedores, cancellationToken);

        return fornecedor.ToDto();
    }
}

// ─── Excluir ─────────────────────────────────────────────────────────────────

public sealed class ExcluirFornecedorCommand : IRequest<Result>
{
    public Guid Id { get; }

    public ExcluirFornecedorCommand(Guid id) => Id = id;
}

public sealed class ExcluirFornecedorCommandHandler : IRequestHandler<ExcluirFornecedorCommand, Result>
{
    private readonly IFornecedorRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cache;

    public ExcluirFornecedorCommandHandler(IFornecedorRepository repository, IUnitOfWork unitOfWork, ICacheService cache)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _cache = cache;
    }

    public async Task<Result> Handle(ExcluirFornecedorCommand request, CancellationToken cancellationToken)
    {
        var fornecedor = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (fornecedor is null)
        {
            return Result.Failure(Error.NotFound("Fornecedor.NaoEncontrado", "Fornecedor não encontrado."));
        }

        _repository.Remove(fornecedor); // soft delete
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _cache.RemoveAsync(CacheKeys.Fornecedores, cancellationToken);

        return Result.Success();
    }
}

// ─── Mapeamento simples (sem AutoMapper: entidade pequena) ───────────────────

internal static class FornecedorMapping
{
    public static FornecedorDto ToDto(this Fornecedor fornecedor) => new()
    {
        Id = fornecedor.Id,
        RazaoSocial = fornecedor.RazaoSocial,
        NomeFantasia = fornecedor.NomeFantasia,
        Cnpj = fornecedor.Cnpj.Value,
        Email = fornecedor.Email.Value,
        Phone = fornecedor.Telefone.Ddd + fornecedor.Telefone.Numero,
        Address = new EnderecoDto
        {
            Street = fornecedor.Endereco.Logradouro,
            Number = fornecedor.Endereco.Numero,
            Complement = fornecedor.Endereco.Complemento,
            Neighborhood = fornecedor.Endereco.Bairro,
            City = fornecedor.Endereco.Cidade,
            State = fornecedor.Endereco.Uf,
            ZipCode = fornecedor.Endereco.Cep
        },
        IsActive = fornecedor.IsActive
    };
}
