using StockControl.Domain.Common;

namespace StockControl.Domain.Entities;

public sealed class Categoria : AggregateRoot
{
    private Categoria() { } // EF Core

    private Categoria(string nome, string? descricao)
    {
        Nome = nome;
        Descricao = descricao;
    }

    public string Nome { get; private set; } = null!;
    public string? Descricao { get; private set; }

    public static Result<Categoria> Criar(string? nome, string? descricao = null)
    {
        if (string.IsNullOrWhiteSpace(nome))
            return Result.Failure<Categoria>(Error.Validation("Categoria.Nome", "O nome é obrigatório."));

        return new Categoria(nome.Trim(), descricao?.Trim());
    }

    public Result Atualizar(string? nome, string? descricao)
    {
        if (string.IsNullOrWhiteSpace(nome))
            return Result.Failure(Error.Validation("Categoria.Nome", "O nome é obrigatório."));

        Nome = nome.Trim();
        Descricao = descricao?.Trim();
        MarkAsUpdated();
        return Result.Success();
    }
}
