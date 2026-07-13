using StockControl.Domain.Common;
using StockControl.Domain.ValueObjects;

namespace StockControl.Domain.Aggregates;

public sealed class Fornecedor : AggregateRoot
{
    private Fornecedor() { } // EF Core

    private Fornecedor(string razaoSocial, string? nomeFantasia, Cnpj cnpj,
        Email email, Telefone telefone, Endereco endereco)
    {
        RazaoSocial = razaoSocial;
        NomeFantasia = nomeFantasia;
        Cnpj = cnpj;
        Email = email;
        Telefone = telefone;
        Endereco = endereco;
    }

    public string RazaoSocial { get; private set; } = null!;
    public string? NomeFantasia { get; private set; }
    public Cnpj Cnpj { get; private set; } = null!;
    public Email Email { get; private set; } = null!;
    public Telefone Telefone { get; private set; } = null!;
    public Endereco Endereco { get; private set; } = null!;

    public static Result<Fornecedor> Criar(string? razaoSocial, string? nomeFantasia,
        Cnpj cnpj, Email email, Telefone telefone, Endereco endereco)
    {
        if (string.IsNullOrWhiteSpace(razaoSocial))
            return Result.Failure<Fornecedor>(Error.Validation("Fornecedor.RazaoSocial", "A razão social é obrigatória."));

        return new Fornecedor(razaoSocial.Trim(), nomeFantasia?.Trim(), cnpj, email, telefone, endereco);
    }

    public Result Atualizar(string? razaoSocial, string? nomeFantasia,
        Cnpj cnpj, Email email, Telefone telefone, Endereco endereco)
    {
        if (string.IsNullOrWhiteSpace(razaoSocial))
            return Result.Failure(Error.Validation("Fornecedor.RazaoSocial", "A razão social é obrigatória."));

        RazaoSocial = razaoSocial.Trim();
        NomeFantasia = nomeFantasia?.Trim();
        Cnpj = cnpj;
        Email = email;
        Telefone = telefone;
        Endereco = endereco;
        MarkAsUpdated();
        return Result.Success();
    }
}
