using StockControl.Domain.Common;
using StockControl.Domain.ValueObjects;

namespace StockControl.Domain.Aggregates;

public sealed class Cliente : AggregateRoot
{
    private Cliente() { } // EF Core

    private Cliente(string nome, Cpf cpf, Email email, Telefone telefone, Endereco endereco)
    {
        Nome = nome;
        Cpf = cpf;
        Email = email;
        Telefone = telefone;
        Endereco = endereco;
    }

    public string Nome { get; private set; } = null!;
    public Cpf Cpf { get; private set; } = null!;
    public Email Email { get; private set; } = null!;
    public Telefone Telefone { get; private set; } = null!;
    public Endereco Endereco { get; private set; } = null!;

    public static Result<Cliente> Criar(string? nome, Cpf cpf, Email email, Telefone telefone, Endereco endereco)
    {
        if (string.IsNullOrWhiteSpace(nome))
            return Result.Failure<Cliente>(Error.Validation("Cliente.Nome", "O nome é obrigatório."));

        return new Cliente(nome.Trim(), cpf, email, telefone, endereco);
    }

    public Result Atualizar(string? nome, Cpf cpf, Email email, Telefone telefone, Endereco endereco)
    {
        if (string.IsNullOrWhiteSpace(nome))
            return Result.Failure(Error.Validation("Cliente.Nome", "O nome é obrigatório."));

        Nome = nome.Trim();
        Cpf = cpf;
        Email = email;
        Telefone = telefone;
        Endereco = endereco;
        MarkAsUpdated();
        return Result.Success();
    }
}
